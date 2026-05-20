import json
import argparse
import re
from collections import defaultdict

def truncate_path(path, max_len=50):
    """Обрезает путь до последних max_len символов, добавляя '...' спереди."""
    if len(path) <= max_len:
        return path
    return "..." + path[-(max_len-3):]

def clean_message(message):
    """Удаляет квадратные скобки с путём к .csproj в конце сообщения."""
    # Удаляем пробелы и [что-угодно] в конце строки
    cleaned = re.sub(r'\s*\[[^\]]+\]\s*$', '', message)
    return cleaned.strip()

def generate_report(json_path, output_txt):
    with open(json_path, 'r', encoding='utf-8') as f:
        warnings = json.load(f)

    # Группировка по коду ошибки
    groups = defaultdict(list)
    for w in warnings:
        groups[w["code"]].append(w)

    lines = []
    for code in sorted(groups.keys()):
        items = groups[code]
        first = items[0]
        # Чистим сообщение от квадратных скобок с путём
        message = clean_message(first["message"])
        url = first.get("url", "")

        # Отделяем описание от URL, если URL уже есть в message (но обычно URL вынесен отдельно)
        if url and url in message:
            # Если URL встроен в message, убираем его (оставляем только текст)
            message = message.replace(f" ({url})", "").replace(f" {url}", "").strip()
        elif not url and 'https://' in message:
            # Если URL не выделен в отдельное поле, но есть в тексте, пробуем извлечь
            url_match = re.search(r'https?://\S+', message)
            if url_match:
                url = url_match.group(0)
                message = message.replace(f" ({url})", "").replace(f" {url}", "").strip()

        # Заголовок раздела
        lines.append(f"{code} {message}")
        if url:
            lines.append(f"URL: {url}")
        lines.append("")  # первая пустая строка после заголовка

        # Заголовок таблицы
        lines.append(f"{'#':<4} {'File':<50}   {'Line':<6} {'Column'}")
        lines.append("-" * 70)

        # Сортировка внутри группы
        sorted_items = sorted(items, key=lambda x: (x["full_path"], x["line"] or 0, x["column"] or 0))

        for idx, w in enumerate(sorted_items, start=1):
            # Только путь, без (строка,столбец) в конце
            display_path = w["full_path"]
            short_path = truncate_path(display_path, 50)
            line_str = str(w["line"]) if w["line"] is not None else ""
            col_str = str(w["column"]) if w["column"] is not None else ""
            lines.append(f"{idx:<4} {short_path:<50}   {line_str:<6} {col_str}")

        # Две пустые строки между разделами (последний раздел не требует дополнительной)
        lines.append("")
        lines.append("")

    # Убираем лишние пустые строки в конце файла (оставляем не более двух)
    while len(lines) > 0 and lines[-1] == "":
        lines.pop()

    with open(output_txt, 'w', encoding='utf-8') as f:
        f.write("\n".join(lines))

    print(f"Report saved to {output_txt}")

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--json-file", required=True)
    parser.add_argument("--output-txt", required=True)
    args = parser.parse_args()
    generate_report(args.json_file, args.output_txt)