$ErrorActionPreference = "Stop"

$PatToken = $env:PAT_TOKEN
$CopilotToken = $env:COPILOT_TOKEN
$Repository = $env:REPO
# Шаблон веток студентов. Меняем здесь, если в следующем потоке будет другая группа.
$BranchPattern = 'group[1-3]/[^/]+'
$workspaceRoot = [System.IO.Path]::GetFullPath($env:GITHUB_WORKSPACE)

if ([string]::IsNullOrWhiteSpace($PatToken)) {
  throw "PAT_TOKEN не задан. Он нужен для checkout веток, создания PR и комментариев."
}

$env:GH_TOKEN = $PatToken

function ConvertTo-KemerovoTimeText([string]$isoDate) {
    $commitTime = [System.DateTimeOffset]::Parse(
        $isoDate,
        [System.Globalization.CultureInfo]::InvariantCulture
    )

    $kemerovoTime = $commitTime.ToUniversalTime().ToOffset([System.TimeSpan]::FromHours(7))
    return $kemerovoTime.ToString("yyyy-MM-dd HH:mm:ss 'UTC'zzz", [System.Globalization.CultureInfo]::InvariantCulture)
}

function Get-GitHubBranchHistoryUrl([string]$branch) {
    $encodedSegments = @(
        $branch -split '/' |
          ForEach-Object { [System.Uri]::EscapeDataString($_) }
    )

    $encodedBranch = $encodedSegments -join '/'
    return "https://github.com/$Repository/commits/$encodedBranch"
}

function Add-OrUpdatePrComment([int]$prNumber, [string]$marker, [string]$body) {
    $commentBody = "$marker`n$body"
    $commentJsonPath = Join-Path $env:GITHUB_WORKSPACE "pr-comment-$prNumber.json"

    @{
        body = $commentBody
    } | ConvertTo-Json -Depth 5 | Out-File -FilePath $commentJsonPath -Encoding utf8

    $jq = ".[] | select(.body | contains(`"$marker`")) | .id"
    $existingCommentId = gh api "repos/$Repository/issues/$prNumber/comments?per_page=100" --paginate --jq $jq 2>$null |
        Select-Object -First 1

    if ($existingCommentId) {
        Write-Host "Обновляю существующий комментарий #$existingCommentId в PR #$prNumber"
        gh api --method PATCH "repos/$Repository/issues/comments/$existingCommentId" --input $commentJsonPath | Out-Null
    } else {
        Write-Host "Создаю новый комментарий в PR #$prNumber"
        gh api --method POST "repos/$Repository/issues/$prNumber/comments" --input $commentJsonPath | Out-Null
    }

    Remove-Item $commentJsonPath -Force -ErrorAction SilentlyContinue
}

function Get-LastBranchPushEvent([string]$branch) {
    $targetRef = "refs/heads/$branch"

    # GitHub Events API отдаёт события в обратном хронологическом порядке.
    # Ищем последний PushEvent именно в нужную ветку.
    for ($page = 1; $page -le 10; $page++) {
        $eventsJson = gh api "repos/$Repository/events?per_page=100&page=$page" 2>$null
        if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($eventsJson)) {
            break
        }

        $events = @($eventsJson | ConvertFrom-Json)
        if ($events.Count -eq 0) {
            break
        }

        foreach ($event in $events) {
            if ($event.type -eq "PushEvent" -and $event.payload.ref -eq $targetRef) {
                return [pscustomobject]@{
                    CreatedAt = [string]$event.created_at
                    Actor     = [string]$event.actor.login
                    Before    = [string]$event.payload.before
                    After     = [string]$event.payload.head
                    Size      = [int]$event.payload.size
                }
            }
        }
    }

    return $null
}

function Add-OrUpdateLastCommitComment([int]$prNumber, [string]$branch) {
    $fullHash = (git rev-parse "origin/$branch").Trim()
    $shortHash = (git rev-parse --short=12 "origin/$branch").Trim()
    $commitIso = (git log -1 --format=%cI "origin/$branch").Trim()
    $commitSubject = (git log -1 --format=%s "origin/$branch").Trim()
    $kemerovoCommitTimeText = ConvertTo-KemerovoTimeText $commitIso
    $commitUrl = "https://github.com/$Repository/commit/$fullHash"
    $historyUrl = Get-GitHubBranchHistoryUrl $branch

    $pushEvent = Get-LastBranchPushEvent $branch
    if ($pushEvent) {
        $kemerovoPushTimeText = ConvertTo-KemerovoTimeText $pushEvent.CreatedAt
        $pushInfoBlock = @"
- **Последний push ветки:** $kemerovoPushTimeText
- **Кто сделал push:** $($pushEvent.Actor)
- **HEAD после push:** ``$($pushEvent.After)``
- **Коммитов в push-событии:** $($pushEvent.Size)
"@
    } else {
        $pushInfoBlock = @"
- **Последний push ветки:** не удалось найти через GitHub Events API
- **Примечание по push:** GitHub Events API хранит не полноценную историю ветки, поэтому для старых или вытесненных событий это поле может быть пустым.
"@
    }

    $marker = "<!-- student-last-commit-info -->"
    $body = @"
## 🕒 Последний коммит и push ветки

Данные о последних изменениях.

- **Ветка:** ``$branch``
- **Последний коммит:** [``$shortHash``]($commitUrl)
- **Полный хеш:** ``$fullHash``
- **Дата последнего коммита:** $kemerovoCommitTimeText
- **Сообщение коммита:** $commitSubject
- **История коммитов ветки:** [открыть в GitHub]($historyUrl)

$pushInfoBlock
- **Часовой пояс:** Кемерово, UTC+07:00 / Asia/Novokuznetsk
"@

    Add-OrUpdatePrComment $prNumber $marker $body
}


function Add-LogSection([string]$path, [string]$title) {
  Add-Content -Path $path -Value ""
  Add-Content -Path $path -Value "============================================================"
  Add-Content -Path $path -Value $title
  Add-Content -Path $path -Value "============================================================"
}

function Test-IsIgnoredPath([string]$workspace, [string]$path) {
  $full = [System.IO.Path]::GetFullPath($path)
  $relative = [System.IO.Path]::GetRelativePath($workspace, $full)

  $segments = @(
    $relative -split '[\\/]' |
      Where-Object { $_ -and $_.Trim() -ne '' }
  )

  if ($segments.Count -eq 0) {
    return $true
  }

  $ignoredSegments = @(
    "main-branch",
    "main-branch-temp",
    ".git",
    "bin",
    "obj",
    "packages"
  )

  foreach ($segment in $segments) {
    if ($ignoredSegments -contains $segment) {
      return $true
    }
  }

  return $false
}

function Get-DotNetSdkVersionFromTarget([string]$targetFramework) {
  if ([string]::IsNullOrWhiteSpace($targetFramework)) {
    return $null
  }

  $tfm = $targetFramework.Trim().ToLowerInvariant()

  if ($tfm -match '^net(\d+)\.0') {
    $major = [int]$Matches[1]
    if ($major -ge 5) {
      return "$major.0.x"
    }
  }

  if ($tfm -match '^netcoreapp(\d+)\.(\d+)') {
    return "$($Matches[1]).$($Matches[2]).x"
  }

  if ($tfm -match '^netstandard') {
    return "8.0.x"
  }

  return $null
}

function Test-DotNetSdkInstalled([string]$versionSpec) {
  if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    return $false
  }

  $installed = @(dotnet --list-sdks 2>$null)
  if ($installed.Count -eq 0) {
    return $false
  }

  $spec = $versionSpec.Trim()

  if ($spec -match '^(\d+)\.(\d+)\.x$') {
    $prefix = "$($Matches[1]).$($Matches[2])."
    return [bool]($installed | Where-Object { $_ -like "$prefix*" })
  }

  return [bool]($installed | Where-Object { $_ -like "$spec *" -or $_ -like "$spec`[*" })
}

function Install-DotNetSdkIfNeeded([string]$versionSpec) {
  if ([string]::IsNullOrWhiteSpace($versionSpec)) {
    return
  }

  if (Test-DotNetSdkInstalled $versionSpec) {
    Write-Host ".NET SDK $versionSpec уже установлен."
    return
  }

  $installDir = Join-Path $env:USERPROFILE ".dotnet"
  New-Item -Path $installDir -ItemType Directory -Force | Out-Null

  $scriptPath = Join-Path $env:RUNNER_TEMP "dotnet-install.ps1"
  if (-not (Test-Path $scriptPath)) {
    Write-Host "Скачиваю dotnet-install.ps1"
    Invoke-WebRequest "https://dot.net/v1/dotnet-install.ps1" -OutFile $scriptPath
  }

  Write-Host "Устанавливаю .NET SDK $versionSpec"

  if ($versionSpec -match '^(\d+)\.(\d+)\.x$') {
    $channel = "$($Matches[1]).$($Matches[2])"
    & $scriptPath -Channel $channel -InstallDir $installDir -NoPath
  } else {
    & $scriptPath -Version $versionSpec -InstallDir $installDir -NoPath
  }

  if ($LASTEXITCODE -ne 0) {
    throw "Не удалось установить .NET SDK $versionSpec"
  }

  $env:DOTNET_ROOT = $installDir
  $env:PATH = "$installDir;$env:USERPROFILE\.dotnet\tools;$env:PATH"
}

function Ensure-DotNetForFormat() {
  if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    return
  }

  Write-Host "dotnet CLI не найден, устанавливаю .NET SDK 8.0.x для dotnet format."
  Install-DotNetSdkIfNeeded "8.0.x"
}

function Get-ProjectMetadata([string]$workspace) {
  $projectFiles = Get-ChildItem -Path $workspace -Filter *.csproj -Recurse -File |
    Where-Object { -not (Test-IsIgnoredPath $workspace $_.FullName) } |
    Sort-Object FullName

  $sourceFiles = Get-ChildItem -Path $workspace -Filter *.cs -Recurse -File |
    Where-Object { -not (Test-IsIgnoredPath $workspace $_.FullName) } |
    Sort-Object FullName

  if ($projectFiles.Count -eq 0) {
    Write-Host "❌ В ветке не найдено ни одного .csproj. Найдено .cs файлов: $($sourceFiles.Count)."
    Write-Host "Для сборки и StyleCop-проверки нужен хотя бы один проектный файл .csproj."
    throw "No .csproj files found"
  }

  $metadata = @()

  foreach ($project in $projectFiles) {
    $projectPath = $project.FullName
    $projectDir = Split-Path $projectPath
    $content = Get-Content $projectPath -Raw

    $isSdkStyle = $false
    if ($content -match '<Project\s+Sdk=' -or $content -match '<Project[^>]+Sdk=') {
      $isSdkStyle = $true
    }

    $usesPackagesConfig = Test-Path (Join-Path $projectDir "packages.config")

    $targets = @()
    if ($content -match '<TargetFramework>\s*([^<]+)\s*</TargetFramework>') {
      $targets = @($Matches[1].Trim())
    } elseif ($content -match '<TargetFrameworks>\s*([^<]+)\s*</TargetFrameworks>') {
      $targets = @(
        $Matches[1] -split ';' |
          ForEach-Object { $_.Trim() } |
          Where-Object { $_ -ne '' }
      )
    } elseif ($content -match '<TargetFrameworkVersion>\s*([^<]+)\s*</TargetFrameworkVersion>') {
      $targets = @($Matches[1].Trim())
    }

    $target = ""
    if ($targets.Count -gt 0) {
      $target = $targets[0]
    }

    $isNetFramework = $false
    if ($target -match '^v?4\.' -or $target -match '^net4') {
      $isNetFramework = $true
    }

    $buildConfiguration = "Debug"
    $buildPlatform = "AnyCPU"

    $conditionPattern = "\$\(Configuration\)\|\$\(Platform\)'\s*==\s*'([^|']+)\|([^']+)'"
    $conditionMatches = [regex]::Matches($content, $conditionPattern)

    if ($conditionMatches.Count -gt 0) {
      $selected = $null

      foreach ($match in $conditionMatches) {
        $config = $match.Groups[1].Value.Trim()
        if ($config -eq "Debug") {
          $selected = $match
          break
        }
      }

      if ($null -eq $selected) {
        $selected = $conditionMatches[0]
      }

      $buildConfiguration = $selected.Groups[1].Value.Trim()
      $buildPlatform = $selected.Groups[2].Value.Trim()
    }

    $metadata += [PSCustomObject]@{
      Path = $projectPath
      Dir = $projectDir
      Name = $project.BaseName
      RelativePath = [System.IO.Path]::GetRelativePath($workspace, $projectPath)
      IsSdkStyle = $isSdkStyle
      UsesPackagesConfig = $usesPackagesConfig
      Target = $target
      Targets = $targets
      IsNetFramework = $isNetFramework
      BuildConfiguration = $buildConfiguration
      BuildPlatform = $buildPlatform
    }
  }

  return [PSCustomObject]@{
    Projects = @($metadata)
    ProjectCount = $projectFiles.Count
    SourceCount = $sourceFiles.Count
  }
}

function Install-SdksForProjects([array]$projects, [string]$workspace) {
  $sdkVersions = @()

  $globalJsonPath = Join-Path $workspace "global.json"
  if (Test-Path $globalJsonPath) {
    try {
      $globalJson = Get-Content $globalJsonPath -Raw | ConvertFrom-Json
      if ($null -ne $globalJson.sdk -and -not [string]::IsNullOrWhiteSpace($globalJson.sdk.version)) {
        $sdkVersions += [string]$globalJson.sdk.version
      }
    } catch {
      Write-Host "⚠️ Не удалось прочитать global.json: $($_.Exception.Message)"
    }
  }

  if ($sdkVersions.Count -eq 0) {
    foreach ($project in $projects) {
      if ($project.IsSdkStyle -eq $true) {
        foreach ($targetName in @($project.Targets)) {
          $sdkVersion = Get-DotNetSdkVersionFromTarget $targetName
          if ($null -ne $sdkVersion) {
            $sdkVersions += $sdkVersion
          }
        }
      }
    }
  }

  $sdkVersions = @($sdkVersions | Sort-Object -Unique)

  if ($sdkVersions.Count -eq 0) {
    Write-Host "По найденным проектам отдельная установка .NET SDK не требуется."
    return
  }

  Write-Host "Нужные .NET SDK по global.json / TargetFramework проектов:"
  foreach ($sdkVersion in $sdkVersions) {
    Write-Host "- $sdkVersion"
    Install-DotNetSdkIfNeeded $sdkVersion
  }
}

function Copy-ConfigsToProjects([array]$projects, [string]$workspace) {
  $editorConfig = Join-Path $workspaceRoot "main-branch-temp\.editorconfig"
  if (Test-Path $editorConfig) {
    Copy-Item -Path $editorConfig -Destination (Join-Path $workspace ".editorconfig") -Force
  } else {
    Write-Host ".editorconfig не найден в main."
  }

  $settingsPath = Join-Path $workspaceRoot "main-branch-temp\Settings.StyleCop.json"
  if (!(Test-Path $settingsPath)) {
    Write-Host "Settings.StyleCop.json не найден в main."
    return
  }

  foreach ($project in $projects) {
    Copy-Item `
      -Path $settingsPath `
      -Destination (Join-Path $project.Dir "Settings.StyleCop.json") `
      -Force

    Write-Host "Settings.StyleCop.json скопирован в $($project.RelativePath)"
  }
}

function Ensure-ChildElement([System.Xml.XmlDocument]$xml, [System.Xml.XmlElement]$parent, [string]$name, [string]$value) {
  $node = $parent.SelectSingleNode("*[local-name()='$name']")
  if ($null -eq $node) {
    $node = $xml.CreateElement($name)
    [void]$parent.AppendChild($node)
  }
  $node.InnerText = $value
}

function Ensure-StyleCopPackageReference([string]$projectPath, [string]$version) {
  [xml]$xml = Get-Content $projectPath -Raw
  $root = $xml.DocumentElement

  $package = $xml.SelectSingleNode("/*[local-name()='Project']/*[local-name()='ItemGroup']/*[local-name()='PackageReference' and @Include='StyleCop.Analyzers']")
  if ($null -eq $package) {
    $itemGroup = $xml.SelectSingleNode("/*[local-name()='Project']/*[local-name()='ItemGroup'][*[local-name()='PackageReference']]")
    if ($null -eq $itemGroup) {
      $itemGroup = $xml.CreateElement("ItemGroup")
      [void]$root.AppendChild($itemGroup)
    }

    $package = $xml.CreateElement("PackageReference")
    $package.SetAttribute("Include", "StyleCop.Analyzers")
    $package.SetAttribute("Version", $version)
    [void]$itemGroup.AppendChild($package)
  } else {
    $versionNode = $package.SelectSingleNode("*[local-name()='Version']")
    if ($package.HasAttribute("Version")) {
      $package.SetAttribute("Version", $version)
    } elseif ($null -ne $versionNode) {
      $versionNode.InnerText = $version
    } else {
      $package.SetAttribute("Version", $version)
    }
  }

  Ensure-ChildElement $xml $package "PrivateAssets" "all"
  Ensure-ChildElement $xml $package "IncludeAssets" "runtime; build; native; contentfiles; analyzers; buildtransitive"
  $xml.Save($projectPath)
}

function Add-StyleCopToSdkProjects([array]$projects) {
  $sdkProjects = @($projects | Where-Object { $_.IsSdkStyle -eq $true })
  $styleCopVersion = "$env:STYLECOP_ANALYZERS_VERSION"

  if ($sdkProjects.Count -eq 0) {
    Write-Host "SDK-style проекты не найдены. Подключение StyleCop пропущено."
    return
  }

  foreach ($project in $sdkProjects) {
    Write-Host "Подключаю StyleCop.Analyzers к $($project.RelativePath)"
    Ensure-StyleCopPackageReference "$($project.Path)" $styleCopVersion
  }
}

function Restore-Projects([array]$projects, [string]$workspace) {
  $legacyProjects = @($projects | Where-Object { $_.UsesPackagesConfig -eq $true })
  foreach ($project in $legacyProjects) {
    $packagesConfig = Join-Path $project.Dir "packages.config"

    Write-Host "Восстанавливаю packages.config для $($project.RelativePath)"
    nuget restore "$packagesConfig" -PackagesDirectory (Join-Path $workspace "packages")

    if ($LASTEXITCODE -ne 0) {
      throw "NuGet restore завершился с ошибкой для $($project.RelativePath)"
    }
  }

  $restoreProjects = @($projects | Where-Object { $_.IsSdkStyle -eq $true -and $_.UsesPackagesConfig -ne $true })
  foreach ($project in $restoreProjects) {
    Write-Host "dotnet restore: $($project.RelativePath)"
    dotnet restore "$($project.Path)"

    if ($LASTEXITCODE -ne 0) {
      throw "dotnet restore завершился с ошибкой для $($project.RelativePath)"
    }
  }
}


function Invoke-LoggedNativeCommand([string]$logPath, [string]$exe, [string[]]$arguments) {
  Write-Host "> $exe $($arguments -join ' ')"

  # Важно: не ставим native command сразу в pipeline с Tee-Object.
  # Иначе можно потерять реальный exit code msbuild/dotnet или случайно вернуть
  # из функции строки лога вместо одного числа.
  $output = & $exe @arguments 2>&1
  $exitCode = [int]$LASTEXITCODE

  $script:LastNativeCommandOutputText = (($output | ForEach-Object { [string]$_ }) -join "`n")

  foreach ($line in $output) {
    $text = [string]$line
    Write-Host $text
    Add-Content -Path $logPath -Value $text
  }

  return [int]$exitCode
}

function Test-LastNativeBuildOutputShowsSuccess() {
  if ([string]::IsNullOrWhiteSpace($script:LastNativeCommandOutputText)) {
    return $false
  }

  # Защитный fallback: если инструмент вернул подозрительный код, но именно последний
  # запуск msbuild/dotnet явно написал Build succeeded и не написал ошибок,
  # не считаем это падением сборки.
  return ($script:LastNativeCommandOutputText -match 'Build succeeded\.' -and $script:LastNativeCommandOutputText -match '0\s+Error\(s\)')
}

function Build-Projects([array]$projects, [string]$workspace) {
  $buildLog = Join-Path $workspace "build.log"
  "" | Set-Content -Path $buildLog -Encoding utf8

  $overallExitCode = 0

  foreach ($project in $projects) {
    Add-LogSection $buildLog "PROJECT: $($project.RelativePath)"
    Add-Content -Path $buildLog -Value "SDK-style: $($project.IsSdkStyle), Target: $($project.Target), Configuration: $($project.BuildConfiguration), Platform: $($project.BuildPlatform)"

    Write-Host "Собираю $($project.RelativePath)"

    if ($project.IsNetFramework -eq $true -or $project.IsSdkStyle -ne $true) {
      $exitCode = Invoke-LoggedNativeCommand `
        -logPath $buildLog `
        -exe "msbuild" `
        -arguments @(
          "$($project.Path)",
          "/p:Configuration=$($project.BuildConfiguration)",
          "/p:Platform=$($project.BuildPlatform)"
        )
    } else {
      $exitCode = Invoke-LoggedNativeCommand `
        -logPath $buildLog `
        -exe "dotnet" `
        -arguments @(
          "build",
          "$($project.Path)",
          "--no-restore"
        )
    }

    if ($exitCode -ne 0 -and (Test-LastNativeBuildOutputShowsSuccess)) {
      Write-Host "⚠️ Команда вернула код $exitCode для $($project.RelativePath), но лог содержит Build succeeded / 0 Error(s). Считаю сборку успешной."
      Add-Content -Path $buildLog -Value "WARNING: command exit code was $exitCode, but build log contains Build succeeded / 0 Error(s). Treated as success."
      $exitCode = 0
    }

    if ($exitCode -ne 0) {
      $overallExitCode = $exitCode
      Write-Host "❌ Сборка завершилась с ошибкой для $($project.RelativePath). Код: $exitCode"
    } else {
      Write-Host "✅ Сборка успешна: $($project.RelativePath)"
    }
  }

  return $overallExitCode
}

function Run-DotNetFormat([array]$projects, [string]$workspace) {
  $formatLog = Join-Path $workspace "ide_warnings.txt"
  "" | Set-Content -Path $formatLog -Encoding utf8

  if ($projects.Count -eq 0) {
    Add-Content -Path $formatLog -Value "Проекты не найдены. dotnet format пропущен."
    return
  }

  Ensure-DotNetForFormat
  $env:PATH = "$env:USERPROFILE\.dotnet\tools;$env:PATH"

  dotnet format --help *> $null
  if ($LASTEXITCODE -ne 0) {
    Write-Host "Команда dotnet format недоступна. Пробую установить/обновить dotnet-format tool."
    dotnet tool update -g dotnet-format
    if ($LASTEXITCODE -ne 0) {
      dotnet tool install -g dotnet-format
    }
  }

  foreach ($project in $projects) {
    Add-LogSection $formatLog "DOTNET FORMAT: $($project.RelativePath)"
    Add-Content -Path $formatLog -Value "SDK-style: $($project.IsSdkStyle), Target: $($project.Target), Configuration: $($project.BuildConfiguration), Platform: $($project.BuildPlatform)"

    Write-Host "dotnet format: $($project.RelativePath)"

    $output = & dotnet format "$($project.Path)" style --verify-no-changes --severity info 2>&1
    $exitCode = $LASTEXITCODE

    $output | Tee-Object -FilePath $formatLog -Append

    if ($exitCode -ne 0) {
      Add-Content -Path $formatLog -Value "dotnet format exit code: $exitCode"
      Write-Host "dotnet format завершился с кодом $exitCode для $($project.RelativePath). Это не ломает workflow; вывод попадёт в отчёт."
    }
  }
}

function Combine-Logs([string]$workspace) {
  $buildLog = Join-Path $workspace "build.log"
  $formatLog = Join-Path $workspace "ide_warnings.txt"
  $combinedLog = Join-Path $workspace "combined.log"

  if (Test-Path $buildLog) {
    Get-Content $buildLog > $combinedLog
  } else {
    "Файл build.log не найден." > $combinedLog
  }

  Add-Content $combinedLog "`n=== Предупреждения dotnet format ===`n"

  if (Test-Path $formatLog) {
    Get-Content $formatLog >> $combinedLog
  } else {
    Add-Content $combinedLog "Файл с предупреждениями IDE не найден."
  }
}

function New-FallbackReportFromLogs([string]$workspace, [string]$reason) {
  $combinedLog = Join-Path $workspace "combined.log"
  $warningsJson = Join-Path $workspace "warnings.json"
  $reportPath = Join-Path $workspace "stylecop_report.txt"

  $warningLines = @()
  if (Test-Path $combinedLog) {
    $warningLines = @(
      Get-Content $combinedLog |
        Where-Object { $_ -match ':\s*warning\s+(SA|IDE|CA|CS)\d+' }
    )
  }

  $items = @()
  foreach ($line in $warningLines) {
    $items += [PSCustomObject]@{
      raw = $line
    }
  }

  $items | ConvertTo-Json -Depth 4 | Set-Content -Path $warningsJson -Encoding utf8

  $report = @()
  $report += "Автоматический парсер отчёта недоступен или завершился ошибкой."
  $report += "Причина: $reason"
  $report += ""
  $report += "Найдено строк с предупреждениями в логах: $($warningLines.Count)"
  $report += ""

  if ($warningLines.Count -gt 0) {
    $report += "Первые предупреждения:"
    $report += ($warningLines | Select-Object -First 80)
  } else {
    $report += "Предупреждения в логах не найдены."
  }

  $report -join "`n" | Set-Content -Path $reportPath -Encoding utf8
}

function Convert-LogsToReport([string]$workspace) {
  $parseScript = Join-Path $workspaceRoot "main-branch-temp/.github/scripts/parse_log_to_json.py"
  $reportScript = Join-Path $workspaceRoot "main-branch-temp/.github/scripts/generate_report_from_json.py"
  $combinedLog = Join-Path $workspace "combined.log"
  $warningsJson = Join-Path $workspace "warnings.json"
  $reportPath = Join-Path $workspace "stylecop_report.txt"

  if (-not (Test-Path $parseScript) -or -not (Test-Path $reportScript)) {
    New-FallbackReportFromLogs $workspace "parse_log_to_json.py или generate_report_from_json.py не найден в main"
    return
  }

  python "$parseScript" --log-file "$combinedLog" --output-json "$warningsJson"
  $parseExitCode = $LASTEXITCODE

  if ($parseExitCode -ne 0) {
    New-FallbackReportFromLogs $workspace "parse_log_to_json.py завершился с кодом $parseExitCode"
    return
  }

  python "$reportScript" --json-file "$warningsJson" --output-txt "$reportPath"
  $reportExitCode = $LASTEXITCODE

  if ($reportExitCode -ne 0) {
    New-FallbackReportFromLogs $workspace "generate_report_from_json.py завершился с кодом $reportExitCode"
    return
  }
}


function Get-WarningCount([string]$warningsPath) {
  if (-not (Test-Path $warningsPath)) {
    return -1
  }

  try {
    $rawJson = (Get-Content $warningsPath -Raw).Trim()
    if ($rawJson -eq '[]') {
      return 0
    }

    if ($rawJson.StartsWith('[')) {
      return @($rawJson | ConvertFrom-Json).Count
    }

    $warningsJson = $rawJson | ConvertFrom-Json
    if ($null -ne $warningsJson.warnings) {
      return @($warningsJson.warnings).Count
    }

    return ($warningsJson | Get-Member -MemberType NoteProperty).Count
  } catch {
    return -1
  }
}

function Copy-BranchArtifacts([string]$workspace, [string]$artifactDir) {
  New-Item -Path $artifactDir -ItemType Directory -Force | Out-Null

  foreach ($f in @("build.log", "ide_warnings.txt", "combined.log", "warnings.json", "stylecop_report.txt", "projects.json", "project-list.txt")) {
    $source = Join-Path $workspace $f
    if (Test-Path $source) {
      Copy-Item $source -Destination $artifactDir -Force
    }
  }
}

function Set-PrBuildFailed([string]$prNumber, [string]$title) {
  if ($title -notmatch '\[BUILD FAILED\]') {
    gh pr edit $prNumber --title "$title [BUILD FAILED]" --repo $Repository | Out-Host
  }
}

function Clear-PrBuildFailed([string]$prNumber) {
  try {
    $prJson = gh pr view $prNumber --repo $Repository --json title | ConvertFrom-Json
    $title = [string]$prJson.title
    if ($title -match '\[BUILD FAILED\]') {
      $newTitle = ($title -replace '\[BUILD FAILED\]', '').Trim()
      gh pr edit $prNumber --title "$newTitle" --repo $Repository | Out-Host
    }
  } catch {
    Write-Host "⚠️ Не удалось убрать [BUILD FAILED] из PR #${prNumber}: $($_.Exception.Message)"
  }
}

function Add-BuildFailureComment([string]$prNumber, [string]$workspace, [string]$message) {
  $buildLog = Join-Path $workspace "build.log"
  $lastLines = $message

  if (Test-Path $buildLog) {
    $lastLines = (Get-Content $buildLog -Tail 60) -join "`n"
  }

  $fence = '```'
  $comment = "## ❌ Сборка не удалась`n`nПоследние строки вывода сборки:`n`n$fence`n$lastLines`n$fence"
  $commentFile = Join-Path $workspaceRoot "build-failure-comment.txt"
  $comment | Out-File -FilePath $commentFile -Encoding utf8
  gh pr comment $prNumber --body-file $commentFile --repo $Repository | Out-Host
  Remove-Item $commentFile -ErrorAction SilentlyContinue
}


function Add-ProcessingErrorComment([string]$prNumber, [string]$message) {
  $marker = "<!-- weekly-student-check-processing-error -->"
  $fence = '```'
  $body = "## ⚠️ Ошибка workflow после сборки`n`nСама C#-сборка могла завершиться успешно. Ошибка произошла на этапе обработки отчётов / комментариев / дополнительных проверок.`n`n$fence`n$message`n$fence"
  Add-OrUpdatePrComment ([int]$prNumber) $marker $body
}

function Add-ReportComment([string]$prNumber, [string]$workspace) {
  $reportPath = Join-Path $workspace "stylecop_report.txt"
  $warningsPath = Join-Path $workspace "warnings.json"

  $reportContent = Get-Content $reportPath -Raw -ErrorAction SilentlyContinue
  if (-not $reportContent) {
    $reportContent = "Детальный отчёт недоступен."
  }

  if ($reportContent.Length -gt 50000) {
    $reportContent = $reportContent.Substring(0, 50000) + "`n... (обрезано - см. артефакты в Actions)"
  }

  $warningCount = Get-WarningCount $warningsPath

  $statusMessage = if ($warningCount -eq 0) {
    "✅ Все автоматические проверки кода прошли успешно."
  } elseif ($warningCount -gt 0 -and $warningCount -le 5) {
    "👍 В целом неплохо, хорошая работа! Пожалуйста, ознакомьтесь с небольшими предупреждениями ниже."
  } elseif ($warningCount -gt 5) {
    "⚠️ Требуется доработка. Ознакомьтесь с замечаниями ниже и исправьте код."
  } else {
    "❌ Не удалось получить количество предупреждений. Проверьте логи сборки вручную."
  }

  $warningLabel = if ($warningCount -eq -1) { "ошибка" } else { "$warningCount" }
  $fence = '```'
  $reportComment = "## Анализ качества кода`n`n**Найдено предупреждений: $warningLabel**`n`n$statusMessage`n`n📋 **Полный отчёт:**`n`n$fence`n$reportContent`n$fence"
  $reportFile = Join-Path $workspaceRoot "report.txt"
  $reportComment | Out-File -FilePath $reportFile -Encoding utf8
  gh pr comment $prNumber --body-file $reportFile --repo $Repository | Out-Host
  Remove-Item $reportFile -ErrorAction SilentlyContinue
}

function Get-OrCreatePullRequest([string]$branch) {
  $existingNumber = gh pr list --base main --head "$branch" --state open --repo $Repository --json number --jq '.[0].number' 2>$null

  if (-not [string]::IsNullOrWhiteSpace($existingNumber)) {
    Write-Host "✅ Использую существующий PR #$existingNumber для $branch"
    return [int]$existingNumber
  }

  $prTitle = "$branch - Первичная проверка"
  $prBody = @"
Проведена первичная проверка стиля.
Проведено первичное ИИ ревью.
Внимательно изучите и внесите изменения.

Если будут вопросы:
1) Находите замечания, с которым не согласны (или есть сомнение/непонимание почему так).
2) Тегаете преподавателя под замечанием через @ и пишете вопрос в комментарии.
3) Скидываете ссылку на комментарий в чатик (можно сопроводить ссылку текстом) - преподаватель возьмет в работу.

Пожелание: делать изменения за один пуш-коммит - так удобнее проверять, что вы не проигнорировали ревью.
"@

  $prResult = gh pr create --title "$prTitle" --body "$prBody" --base main --head $branch --repo $Repository 2>&1
  if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️ Не удалось создать PR стандартным способом: $prResult"
    $existingNumber = gh pr list --base main --head "$branch" --state open --repo $Repository --json number --jq '.[0].number' 2>$null
    if (-not [string]::IsNullOrWhiteSpace($existingNumber)) {
      Write-Host "✅ После ошибки найден существующий PR #$existingNumber"
      return [int]$existingNumber
    }

    throw "Failed to create PR for $branch. Error: $prResult"
  }

  $prNumber = $prResult -replace '.*/pull/(\d+).*', '$1'
  Write-Host "✅ PR #$prNumber создан"
  return [int]$prNumber
}

Write-Host "Starting student branches processing..."
Write-Host "Branch pattern: $BranchPattern"

if (Test-Path "main-branch-temp") {
  Remove-Item -Recurse -Force "main-branch-temp"
}

Write-Host "Cloning main branch for scripts and configs..."
git clone --depth 1 --branch main "https://x-access-token:$PatToken@github.com/$Repository" main-branch-temp

$failedBranchesFile = Join-Path $workspaceRoot "failed-branches.txt"
New-Item -Path $failedBranchesFile -ItemType File -Force | Out-Null
Add-Content $failedBranchesFile "Ветки без изменений или с ошибкой компиляции:`n"

$artifactsRoot = Join-Path $workspaceRoot "artifacts"
New-Item -Path $artifactsRoot -ItemType Directory -Force | Out-Null

git fetch origin '+refs/heads/*:refs/remotes/origin/*'

$branches = git branch -r |
  Where-Object { $_ -match "origin/($BranchPattern)$" } |
  ForEach-Object { $matches[1] } |
  Sort-Object -Unique

Write-Host "Found branches: $($branches -join ', ')"

$total = $branches.Count
$index = 0

foreach ($branch in $branches) {
  $index++
  $branchSafe = $branch -replace '/', '_'
  $artifactDir = Join-Path $artifactsRoot $branchSafe
  $studentPath = Join-Path $workspaceRoot "student-code"
  $prNumber = $null
  $prTitle = "$branch - Первичная проверка"
  $pushedLocation = $false

  Write-Host "`n========================================"
  Write-Host "[$index/$total] Processing branch: $branch"
  Write-Host "========================================"

  try {
    git fetch origin $branch
    $diffStat = git diff --stat main...origin/$branch
    if (-not $diffStat) {
      Write-Host "⚠️ Branch $branch has no changes compared to main. Skipping."
      Add-Content $failedBranchesFile "- $branch (no changes)"
      continue
    }

    $prNumber = Get-OrCreatePullRequest $branch
    Add-OrUpdateLastCommitComment $prNumber $branch

    if (Test-Path $studentPath) {
      Remove-Item -Recurse -Force $studentPath
    }

    Write-Host "Cloning student code from branch $branch ..."
    git clone --depth 1 --branch $branch "https://x-access-token:$PatToken@github.com/$Repository" $studentPath

    Push-Location $studentPath
    $pushedLocation = $true
    $workspace = [System.IO.Path]::GetFullPath((Get-Location).Path)

    $detection = Get-ProjectMetadata $workspace
    $projects = @($detection.Projects)

    $projectsJsonPath = Join-Path $workspace "projects.json"
    $projectListPath = Join-Path $workspace "project-list.txt"
    $projects | ConvertTo-Json -Depth 5 | Set-Content -Path $projectsJsonPath -Encoding utf8
    $projects | ForEach-Object { $_.Path } | Set-Content -Path $projectListPath -Encoding utf8

    Write-Host "Найдено .csproj: $($detection.ProjectCount)"
    Write-Host "Найдено .cs файлов: $($detection.SourceCount)"
    Write-Host "Список проектов:"
    foreach ($project in $projects) {
      $targetsForLog = @($project.Targets) -join ';'
      Write-Host "- $($project.RelativePath) | SDK-style=$($project.IsSdkStyle) | packages.config=$($project.UsesPackagesConfig) | target=$targetsForLog | config=$($project.BuildConfiguration) | platform=$($project.BuildPlatform)"
    }

    Install-SdksForProjects $projects $workspace
    Copy-ConfigsToProjects $projects $workspace
    Add-StyleCopToSdkProjects $projects

    if (@($projects | Where-Object { $_.IsSdkStyle -ne $true }).Count -gt 0) {
      Write-Host "В ветке есть non-SDK проекты. Они не будут переписаны автоматически."
      Write-Host "Такие проекты будут собраны через NuGet packages.config + MSBuild."
    }

    Restore-Projects $projects $workspace
    $buildExitCode = Build-Projects $projects $workspace

    if ($buildExitCode -ne 0) {
      Write-Host "❌ Build failed for $branch"
      Set-PrBuildFailed "$prNumber" $prTitle
      Add-BuildFailureComment "$prNumber" $workspace "Build failed. Exit code: $buildExitCode"
      Copy-BranchArtifacts $workspace $artifactDir
      Add-Content $failedBranchesFile "- $branch (build failed)"
      continue
    }

    Write-Host "✅ Build successful for $branch"
    Clear-PrBuildFailed "$prNumber"

    Run-DotNetFormat $projects $workspace
    Combine-Logs $workspace
    Convert-LogsToReport $workspace
    Copy-BranchArtifacts $workspace $artifactDir
    Add-ReportComment "$prNumber" $workspace

    if (-not [string]::IsNullOrWhiteSpace($CopilotToken)) {
      Write-Host "Running Copilot review..."
      gh extension install k1LoW/gh-copilot-review --force
      $env:GH_TOKEN = $CopilotToken
      gh copilot-review $prNumber
      if ($LASTEXITCODE -ne 0) { Write-Host "⚠️ Copilot review failed" }
      $env:GH_TOKEN = $PatToken
    } else {
      Write-Host "COPILOT_GITHUB_TOKEN не задан. Copilot review пропущен."
    }

    Write-Host "✅ Finished processing branch $branch"
  } catch {
    Write-Host "❌ Ошибка при обработке ветки ${branch}: $($_.Exception.Message)"
    Add-Content $failedBranchesFile "- $branch (error: $($_.Exception.Message))"

    if ($null -ne $prNumber) {
      try {
        $currentWorkspace = if (Test-Path $studentPath) { $studentPath } else { $workspaceRoot }
        Add-ProcessingErrorComment "$prNumber" $_.Exception.Message
        if (Test-Path $studentPath) {
          Copy-BranchArtifacts $studentPath $artifactDir
        }
      } catch {
        Write-Host "⚠️ Не удалось оставить комментарий об ошибке: $($_.Exception.Message)"
      }
    }
  } finally {
    if ($pushedLocation) {
      Pop-Location
    }

    if (Test-Path $studentPath) {
      Remove-Item -Recurse -Force $studentPath
    }

    $env:GH_TOKEN = $PatToken
  }
}

Copy-Item $failedBranchesFile -Destination (Join-Path $artifactsRoot "failed-branches.txt") -Force
Write-Host "`nProcessing complete. Failed branches saved to $failedBranchesFile"