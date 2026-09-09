# -*- coding: utf-8 -*-
"""v2.0.7: demo long-cycle chain events (user request: events span 1 year vs 10-20 years).
Adds two showcase chains with stretched chainDelay + a quick-event sample.
Idempotent (skips existing ids)."""
import io, json, collections

data_path = 'events.json'
data = json.load(io.open(data_path, encoding='utf-8'), object_pairs_hook=collections.OrderedDict)
existing = set(e['id'] for e in data['events'])

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

# ============ 10 年长链：王朝兴衰四幕（4 跳 × 2~3 年间隔）============
# dynasty_peak(年3) ->[2y] dynasty_crisis(年5+ ->[3y] dynasty_fall(年8+ ->[2y] dynasty_rise(年10+
add('dynasty_peak', 'court', minYear=3, timeout=2, fallback=1, cd=18,
    chainNext='dynasty_crisis', chainDelay=2, chainAfterOption=None,
    options=[('opt1', {'goodwillAll': 2}), ('opt2', {'treasuryGdpRatio': 0.008})])
add('dynasty_crisis', 'court', minYear=5, timeout=2, fallback=1, cd=0,
    chainNext='dynasty_fall', chainDelay=3, chainAfterOption=None,
    options=[('opt1', {'unrest': True}), ('opt2', {'treasuryGdpRatio': -0.01})])
add('dynasty_fall', 'disaster', minYear=8, timeout=2, fallback=1, cd=0,
    chainNext='dynasty_rise', chainDelay=2, chainAfterOption=None,
    options=[('opt1', {'citizenWealthRatio': -0.4}), ('opt2', {'unrest': True})])
add('dynasty_rise', 'civil', minYear=10, timeout=2, fallback=1, cd=0,
    options=[('opt1', {'citizenWealthRatio': 0.5, 'goodwillAll': 2}), ('opt2', {})])

# ============ 20 年长链：国祚盛衰五幕（5 跳 × 4 年间隔）============
add('realm_birth', 'civil', minYear=4, timeout=2, fallback=1, cd=20,
    chainNext='realm_growth', chainDelay=4, chainAfterOption=None,
    options=[('opt1', {'upgradeBuildings': 3, 'goodwillAll': 1}), ('opt2', {})])
add('realm_growth', 'finance', minYear=8, timeout=2, fallback=1, cd=0,
    chainNext='realm_storm', chainDelay=4, chainAfterOption=None,
    options=[('opt1', {'treasuryGdpRatio': 0.012}), ('opt2', {'unrest': True})])
add('realm_storm', 'military', minYear=12, timeout=2, fallback=1, cd=0,
    chainNext='realm_ruin', chainDelay=4, chainAfterOption=None,
    options=[('opt1', {'declareWarTarget': 2, 'treasuryGdpRatio': -0.015}), ('opt2', {'goodwillAll': 2})])
add('realm_ruin', 'disaster', minYear=16, timeout=2, fallback=1, cd=0,
    chainNext='realm_rebirth', chainDelay=4, chainAfterOption=None,
    options=[('opt1', {'unrest': True}), ('opt2', {'citizenWealthRatio': -0.3})])
add('realm_rebirth', 'civil', minYear=20, timeout=2, fallback=1, cd=0,
    options=[('opt1', {'upgradeBuildings': 4, 'citizenWealthRatio': 0.4}), ('opt2', {'goodwillAll': 2})])

# ============ 1 年速决样例：御前廷议（当年拍板，无 chain）============
add('court_verdict', 'court', minYear=2, timeout=1, fallback=1, cd=8,
    options=[('opt1', {'treasuryGdpRatio': -0.006}), ('opt2', {'goodwillAll': 1})])

with io.open(data_path, 'w', encoding='utf-8') as f:
    json.dump(data, f, ensure_ascii=False, indent=2)
    f.write('\n')
from collections import Counter
fam = Counter(e['family'] for e in data['events'])
print('events.json total:', len(data['events']), '| new:', len(data['events']) - 336)
print('families:', dict(fam))
