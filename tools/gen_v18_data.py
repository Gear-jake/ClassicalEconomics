# -*- coding: utf-8 -*-
"""v1.8.0: 12 new events with new effect types (declareWar/formAlliance/moveCapital/
upgradeBuildings/citizenWealthRatio/worldWar + rarityWeight). Idempotent (skips existing ids)."""
import io, json, collections

data_path = 'events.json'
data = json.load(io.open(data_path, encoding='utf-8'), object_pairs_hook=collections.OrderedDict)
by_id = {e['id']: e for e in data['events']}
existing = set(by_id.keys())

def add(id, family, minYear=2, timeout=2, fallback=1, cd=16, rarity=1.0,
        options=None, **extra):
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

# ===== 世界大战 ×2（rarityWeight=0.08，概率极小）=====
add('world_war_pact', 'diplomacy', minYear=8, timeout=3, fallback=1, cd=40, rarity=0.08,
    options=[('opt1', {'worldWar': True, 'goodwillAll': -3}),
             ('opt2', {'worldWar': True, 'goodwillAll': 3})])
add('world_war_driven', 'military', minYear=8, timeout=3, fallback=1, cd=40, rarity=0.08,
    options=[('opt1', {'worldWar': True, 'treasuryGdpRatio': -0.01}),
             ('opt2', {'worldWar': True, 'goodwillAll': -2})])

# ===== 宣战 ×3（declareWarTarget：-1=最强邻国 0=随机 1=最弱 2=当前交战）=====
add('declare_war_casus', 'military', minYear=3, timeout=2, fallback=1, cd=18,
    conditions=None if False else None,
    options=[('opt1', {'declareWarTarget': -1}),
             ('opt2', {'goodwillAll': 2, 'treasuryGdpRatio': -0.005})])
add('war_monger', 'court', minYear=4, timeout=2, fallback=1, cd=18, onlyPlayer=True,
    options=[('opt1', {'declareWarTarget': 2, 'goodwillAll': -1}),
             ('opt2', {'goodwillAll': 2})])
add('forced_peace_breaker', 'diplomacy', minYear=5, timeout=2, fallback=1, cd=20,
    chainNext='war_escalation', chainDelay=2, chainAfterOption=0,
    options=[('opt1', {'declareWarTarget': -1, 'treasuryGdpRatio': -0.008}),
             ('opt2', {'goodwillAll': 2})])

# ===== 结盟 ×3（formAllianceTarget：-1=关系最好 0=随机 1=国力最强）=====
add('alliance_overture', 'diplomacy', minYear=3, timeout=2, fallback=1, cd=16,
    options=[('opt1', {'formAllianceTarget': -1, 'goodwillAll': 1}),
             ('opt2', {'goodwillAll': 2})])
add('treaty_of_salt', 'diplomacy', minYear=4, timeout=2, fallback=1, cd=18,
    options=[('opt1', {'formAllianceTarget': 1, 'treasuryGdpRatio': 0.006}),
             ('opt2', {'goodwillAll': 1})])
add('enemy_of_enemy', 'court', minYear=5, timeout=2, fallback=1, cd=18, onlyPlayer=True,
    options=[('opt1', {'formAllianceTarget': -1, 'declareWarTarget': -1}),
             ('opt2', {'goodwillAll': 2})])

# ===== 迁都 ×2（moveCapital）=====
add('capital_relocate', 'court', minYear=3, timeout=2, fallback=1, cd=16, onlyPlayer=True,
    options=[('opt1', {'moveCapital': True, 'goodwillAll': 1}),
             ('opt2', {'treasuryGdpRatio': -0.006})])
add('capital_undaunted', 'military', minYear=4, timeout=2, fallback=1, cd=16,
    options=[('opt1', {'moveCapital': True, 'treasuryGdpRatio': -0.008}),
             ('opt2', {'unrest': True})])

# ===== 建筑升级 ×1（upgradeBuildings）=====
add('building_renaissance', 'civil', minYear=3, timeout=2, fallback=1, cd=16,
    options=[('opt1', {'upgradeBuildings': 4, 'treasuryGdpRatio': -0.006}),
             ('opt2', {'goodwillAll': 1})])

# ===== 个体财富 ×1（citizenWealthRatio）=====
add('merchant_tycoon', 'finance', minYear=3, timeout=2, fallback=1, cd=16,
    options=[('opt1', {'citizenWealthRatio': 0.5, 'treasuryGdpRatio': 0.004}),
             ('opt2', {'citizenWealthRatio': -0.4, 'goodwillAll': 1})])

with io.open(data_path, 'w', encoding='utf-8') as f:
    json.dump(data, f, ensure_ascii=False, indent=2)
    f.write('\n')
from collections import Counter
fam = Counter(e['family'] for e in data['events'])
print('events.json total:', len(data['events']))
print('families:', dict(fam))
print('new ids:', len(data['events']) - 318)
