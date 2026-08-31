# -*- coding: utf-8 -*-
"""全局改名：Codex -> Law（消除与 AI 助手同名联想）。"""
import glob, os

MAP = [
    ("CodexEngine", "LawEngine"),
    ("CodexAi", "LawAi"),
    ("CodexSave", "LawSave"),
    ("CodexTrait", "LawTrait"),
    ("TypeCodexReform", "TypeLawReform"),
    ("codex_", "law_"),
    ("Codex", "Law"),
]

def replace_in(path):
    with open(path, 'r', encoding='utf-8') as f:
        s = f.read()
    orig = s
    for old, new in MAP:
        s = s.replace(old, new)
    if s != orig:
        with open(path, 'w', encoding='utf-8', newline='') as f:
            f.write(s)
        return True
    return False

count = 0
for ext, skip in [('*.cs', ('bin', 'obj', 'evidence')),
                  ('*.json', ('bin', 'obj', 'evidence')),
                  ('*.ps1', ()),
                  ('*.md', ('bin', 'obj')),
                  ('*.txt', ())]:
    for path in glob.glob('**/' + ext, recursive=True):
        parts = path.replace(os.sep, '/').split('/')
        if any(p in skip for p in parts):
            continue
        if replace_in(path):
            count += 1
            print('MOD', path)
print('files modified:', count)
