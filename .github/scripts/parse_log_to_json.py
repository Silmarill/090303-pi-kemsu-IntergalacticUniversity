import re
import json
import argparse

def extract_url(text):
    match = re.search(r'https?://\S+', text)
    if match:
        url = match.group(0)
        if url.endswith(')'):
            url = url[:-1]
        return url
    return None

def parse_build_log_to_json(input_path, output_json):
    with open(input_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()

    # Паттерн для строк с координатами (например: file.cs(10,5): warning SA1234: message)
    pattern_with_coords = re.compile(
        r'^(.+?)\((\d+),(\d+)\):\s+(warning|error|info)\s+([A-Za-z0-9]+):\s+(.*?)(?:\s+\[.*\])?$'
    )
    # Альтернативный паттерн для строк без координат (например: source : warning CODE: message)
    pattern_alt = re.compile(
        r'^(\S+)\s+:\s+(warning|error|info)\s+(\S+):\s+(.*)$'
    )

    unique_warnings = {}

    for line in lines:
        line = line.strip()
        if not line:
            continue
    
        match = pattern_with_coords.match(line)
        if match:
            full_path = match.group(1)
            line_num = int(match.group(2))
            col_num = int(match.group(3))
            severity = match.group(4)
            code = match.group(5)
            message = match.group(6).strip()
            url = extract_url(message)
            key = (full_path, line_num, col_num, code)
            if url is None and code.startswith('IDE'):
                url = f"https://learn.microsoft.com/ru-ru/dotnet/fundamentals/code-analysis/style-rules/{code}"
            if key not in unique_warnings:
                unique_warnings[key] = {
                    "full_path": full_path,
                    "line": line_num,
                    "column": col_num,
                    "severity": severity,
                    "code": code,
                    "message": message,
                    "url": url,
                    "raw_line": line
                }
            continue
    
        match = pattern_alt.match(line)
        if match:
            source = match.group(1)
            severity = match.group(2)
            code = match.group(3)
            message = match.group(4).strip()
            url = extract_url(message)
            if url is None and code.startswith('IDE'):
                url = f"https://learn.microsoft.com/ru-ru/dotnet/fundamentals/code-analysis/style-rules/{code}"
            key = (source, None, None, code)
            if key not in unique_warnings:
                unique_warnings[key] = {
                    "full_path": source,
                    "line": None,
                    "column": None,
                    "severity": severity,
                    "code": code,
                    "message": message,
                    "url": url,
                    "raw_line": line
                }
            # continue не нужен, так как это последний паттерн

    warnings_list = list(unique_warnings.values())
    with open(output_json, 'w', encoding='utf-8') as f:
        json.dump(warnings_list, f, indent=2, ensure_ascii=False)

    print(f"JSON saved to {output_json} (unique warnings: {len(warnings_list)})")

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--log-file", required=True)
    parser.add_argument("--output-json", required=True)
    args = parser.parse_args()
    parse_build_log_to_json(args.log_file, args.output_json)