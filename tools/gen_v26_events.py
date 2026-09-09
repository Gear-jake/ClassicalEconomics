# -*- coding: utf-8 -*-
"""v2.0.6: war-economy cycle events (armament -> war -> postwar decay -> recovery).
Idempotent (skips existing ids). Effects reuse existing keys; no new engine fields."""
import io, json, collections

data_path = 'events.json'
data = json.load(io.open(data_path, encoding='utf-8'), object_pairs_hook=collections.OrderedDict)
by_id = {e['id']: e for e in data['events']}
existing = set(by_id.keys())

def add(id, family, minYear=2, timeout=2, fallback=1, cd=14, rarity=1.0, options=None, **extra):
    if id in existing:
        return 0
    if not options or len(options) < 2:
        raise ValueError('%s: options < 2' % id)
    e = collections.OrderedDict()
    e['id'] = id; e['family'] = family
    e['minYear'] = minYear; e['timeoutYears'] = timeout
    e['fallback'] = fallback; e['cooldownYears'] = cd
    if rarity != 1.0: e['rarityWeight'] = rarity
    for k, v in extra.items():
        if v is not None: e[k] = v
    e['options'] = [{'key': k, **(x or {})} for k, x in options]
    data['events'].append(e)
    return 1

# ===== 军备竞赛（大增长后全面武装）=====
add('armament_race', 'military', minYear=4, timeout=2, fallback=1, cd=14,
    options=[('opt1', {'upgradeBuildings': 3, 'treasuryGdpRatio': -0.01}),
             ('opt2', {'goodwillAll': 1})])
# ===== 全面征募（战后预备役）=====
add('levy_muster', 'military', minYear=4, timeout=2, fallback=1, cd=14,
    options=[('opt1', {'treasuryGdpRatio': -0.012, 'unrest': True}),
             ('opt2', {'goodwillAll': 2})])
# ===== 大战爆发（军力透支）=====
add('great_war_unleashed', 'military', minYear=5, timeout=2, fallback=1, cd=20,
    options=[('opt1', {'declareWarTarget': 2, 'treasuryGdpRatio': -0.015}),
             ('opt2', {'goodwillAll': 2})])
# ===== 战后民生凋敝（GDP 大降）=====
add('postwar_decay', 'civil', minYear=6, timeout=2, fallback=1, cd=16,
    options=[('opt1', {'poorReliefRatio': 0.15, 'treasuryGdpRatio': -0.008}),
             ('opt2', {'citizenWealthRatio': -0.3, 'unrest': True})])
# ===== 百废待兴（缓慢恢复起点）=====
add('reconstruction_begin', 'civil', minYear=6, timeout=2, fallback=1, cd=14,
    options=[('opt1', {'upgradeBuildings': 4, 'treasuryGdpRatio': -0.01}),
             ('opt2', {'goodwillAll': 1})])
# ===== 复兴之春（恢复完成回到上行）=====
add('renewal_spring', 'civil', minYear=7, timeout=2, fallback=1, cd=16,
    options=[('opt1', {'citizenWealthRatio': 0.4, 'goodwillAll': 2}),
             ('opt2', {'treasuryGdpRatio': 0.006})])

with io.open(data_path, 'w', encoding='utf-8') as f:
    json.dump(data, f, ensure_ascii=False, indent=2)
    f.write('\n')
from collections import Counter
fam = Counter(e['family'] for e in data['events'])
print('events.json total:', len(data['events']), '| new:', len(data['events']) - 330)
print('families:', dict(fam))
