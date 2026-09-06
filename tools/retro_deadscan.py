# -*- coding: utf-8 -*-
"""v1.4.2 复盘：零引用符号扫描（public/internal 声明在产品代码中仅出现 1 次）。"""
import io, os, re, subprocess, collections

files = [f for f in subprocess.run(['git', 'ls-files', '*.cs'], capture_output=True, text=True).stdout.split()
         if not f.startswith('tools') and os.path.exists(f)]
corpus = {f: io.open(f, encoding='utf-8', errors='ignore').read() for f in files}
all_text = '\n'.join(corpus.values())

# 排除字符串派发（ConfigCallbacks 回调由 default_config.json 按名调用）
json_cfg = io.open('default_config.json', encoding='utf-8').read()
candidates = collections.defaultdict(list)
pat = re.compile(r'\b(public|internal)\s+(?:sealed\s+)?(?:static\s+)?(?:readonly\s+)?'
                 r'(?:class|struct|const\s+\w+|enum|[\w<>\[\]]+\??)\s+(\w+)\s*(?:[({=;]|=>)')
for f, t in corpus.items():
    for m in pat.finditer(t):
        name = m.group(2)
        if name in ('Neutral', 'Instance'):
            continue
        n = all_text.count(name)
        if n <= 1 and name not in json_cfg:
            candidates[f].append(name)

for f, names in sorted(candidates.items()):
    for n in names:
        print(f, '::', n)
print('---- scan done ----')
