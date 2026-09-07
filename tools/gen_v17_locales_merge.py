# -*- coding: utf-8 -*-
"""v1.7.0: 合并六族文案数据文件（tools/v17_text_*.py）→ 写入 Locales 六语言 JSON。
幂等：只补缺失键。用法：python tools/gen_v17_locales_merge.py"""
import io, json, collections, importlib.util, os, sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
os.chdir(ROOT)
sys.path.insert(0, ROOT)

EVENTS = {}
for fam in ('finance', 'court', 'military', 'disaster', 'civil', 'diplomacy'):
    path = os.path.join(ROOT, 'tools', 'v17_text_%s.py' % fam)
    if not os.path.exists(path):
        print('MISSING data file:', path)
        continue
    spec = importlib.util.spec_from_file_location('v17_text_%s' % fam, path)
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    n = len(mod.EVENTS)
    EVENTS.update(mod.EVENTS)
    print('loaded', fam, n)

print('total events with text:', len(EVENTS))

LANGS = {'ch': 0, 'zh_tw': 1, 'en': 2, 'ru': 3, 'ja': 4, 'de': 5}
total_added = 0
for lang, idx in LANGS.items():
    p = os.path.join(ROOT, 'Locales', '%s.json' % lang)
    d = json.load(io.open(p, encoding='utf-8'), object_pairs_hook=collections.OrderedDict)
    added = 0
    for eid, (opts, title, desc, texts) in EVENTS.items():
        if opts != len(texts) // 3:
            raise ValueError('%s: opts=%d but %d text rows' % (eid, opts, len(texts)))
        kv = {
            'ev_%s' % eid: title[idx],
            'ev_%s_desc' % eid: desc[idx],
        }
        for i in range(opts):
            kv['ev_%s_opt%d' % (eid, i + 1)] = texts[i * 3][idx]
            kv['ev_%s_opt%d_desc' % (eid, i + 1)] = texts[i * 3 + 1][idx]
            kv['ev_%s_res%d' % (eid, i + 1)] = texts[i * 3 + 2][idx]
        for k, v in kv.items():
            if k not in d:
                d[k] = v; added += 1
    json.dump(d, io.open(p, 'w', encoding='utf-8'), ensure_ascii=False, indent=2)
    io.open(p, 'a', encoding='utf-8').write('\n')
    total_added += added
    print(lang, 'added', added, 'total', len(d))
print('TOTAL added keys:', total_added)
