# -*- coding: utf-8 -*-
import io, json, collections

p = 'events.json'
data = json.load(io.open(p, encoding='utf-8'), object_pairs_hook=collections.OrderedDict)
existing = {e['id'] for e in data['events']}

def ev(**kw):
    if kw['id'] in existing:
        return  # 幂等：已存在的事件跳过，防重复追加
    e = collections.OrderedDict()
    e['id'] = kw['id']; e['family'] = kw['family']
    e['minYear'] = kw.get('minYear', 2)
    e['timeoutYears'] = kw.get('timeout', 2)
    e['fallback'] = kw['fallback']
    e['cooldownYears'] = kw.get('cd', 12)
    if kw.get('onlyPlayer'): e['onlyPlayer'] = True
    conds = kw.get('conds')
    if conds: e['conditions'] = conds
    if kw.get('chain'): e['chainNext'] = kw['chain']
    if kw.get('chainDelay'): e['chainDelay'] = kw['chainDelay']
    if kw.get('chainAfter') is not None: e['chainAfterOption'] = kw['chainAfter']
    opts = []
    for key, extra in kw['options']:
        o = collections.OrderedDict(); o['key'] = key
        if extra: o.update(extra)
        opts.append(o)
    e['options'] = opts
    data['events'].append(e)

SW_MERCH = {'style_merchant': 2.0}
SW_MIL = {'style_bellicose': 1.8}
SW_WEL = {'style_welfare': 1.8}
SW_LEG = {'style_legalist': 1.8}
SW_ISO = {'style_isolationist': 2.0}

# ===== 财政 finance +6 =====
ev(id='merchant_loan', family='finance', minYear=3, timeout=2, fallback=1, cd=18, onlyPlayer=True,
   chain='loan_due', chainDelay=2, chainAfter=0,
   options=[('opt1', {'treasuryGdpRatio': 0.06}), ('opt2', None)])
ev(id='loan_due', family='finance', minYear=0, timeout=2, fallback=1, cd=0, onlyPlayer=True,
   options=[('opt1', {'treasuryGdpRatio': -0.08}), ('opt2', {'goodwillAll': -4, 'unrest': True})])
ev(id='tax_farm', family='finance', minYear=3, timeout=3, fallback=1, cd=16,
   options=[('opt1', {'treasuryGdpRatio': 0.03, 'unrest': True, 'styleWeights': SW_MERCH}),
            ('opt2', {'goodwillAll': 2})])
ev(id='war_bonds', family='finance', minYear=3, timeout=2, fallback=1, cd=12,
   conds={'atWar': 1},
   options=[('opt1', {'residentsTaxRatio': 0.02}), ('opt2', {'unrest': True})])
ev(id='land_survey', family='finance', minYear=4, timeout=3, fallback=1, cd=20,
   options=[('opt1', {'treasuryGdpRatio': -0.006, 'residentsTaxRatio': 0.01, 'styleWeights': SW_LEG}),
            ('opt2', None)])
ev(id='guild_bank', family='finance', minYear=4, timeout=3, fallback=1, cd=18,
   options=[('opt1', {'treasuryGdpRatio': 0.02, 'goodwillAll': -2}),
            ('opt2', {'poorReliefRatio': 0.06, 'styleWeights': SW_WEL})])

# ===== 天灾 disaster +6 =====
ev(id='flood', family='disaster', minYear=2, timeout=1, fallback=1, cd=10,
   options=[('opt1', {'poorReliefRatio': 0.12}), ('opt2', {'unrest': True})])
ev(id='wildfire', family='disaster', minYear=2, timeout=1, fallback=1, cd=10,
   options=[('opt1', {'treasuryGdpRatio': -0.005}), ('opt2', {'unrest': True})])
ev(id='blizzard', family='disaster', minYear=2, timeout=1, fallback=1, cd=10,
   options=[('opt1', {'poorReliefRatio': 0.1}), ('opt2', {'unrest': True})])
ev(id='mine_collapse', family='disaster', minYear=3, timeout=2, fallback=1, cd=12,
   options=[('opt1', {'treasuryGdpRatio': -0.006, 'goodwillAll': 1}),
            ('opt2', {'unrest': True})])
ev(id='river_dry', family='disaster', minYear=3, timeout=2, fallback=1, cd=12,
   options=[('opt1', {'poorReliefRatio': 0.08}),
            ('opt2', {'residentsTaxRatio': 0.008, 'unrest': True})])
ev(id='rockslide', family='disaster', minYear=3, timeout=2, fallback=1, cd=12,
   options=[('opt1', {'treasuryGdpRatio': -0.004}),
            ('opt2', {'goodwillAll': -2})])

# ===== 宫廷 court +8（全部 onlyPlayer）=====
ev(id='heir_plot', family='court', minYear=5, timeout=3, fallback=0, cd=0, onlyPlayer=True,
   chain='heir_plot_crushed', chainDelay=1,
   options=[('opt1', {'unrest': True}), ('opt2', {'treasuryGdpRatio': -0.012}),
            ('opt3', {'goodwillAll': -2})])
ev(id='heir_plot_crushed', family='court', minYear=0, timeout=2, fallback=0, cd=0, onlyPlayer=True,
   options=[('opt1', {'residentsTaxRatio': 0.01}), ('opt2', {'goodwillAll': 2})])
ev(id='concubine_rivalry', family='court', minYear=4, timeout=3, fallback=1, cd=16, onlyPlayer=True,
   options=[('opt1', {'treasuryGdpRatio': -0.006}),
            ('opt2', {'unrest': True})])
ev(id='royal_astrologer', family='court', minYear=3, timeout=3, fallback=1, cd=16, onlyPlayer=True,
   options=[('opt1', {'treasuryGdpRatio': -0.004, 'goodwillAll': 2}),
            ('opt2', {'unrest': True, 'styleWeights': {'style_tech': 0.4}})])
ev(id='hunt_accident', family='court', minYear=4, timeout=2, fallback=1, cd=18, onlyPlayer=True,
   options=[('opt1', {'poorReliefRatio': 0.05}),
            ('opt2', {'treasuryGdpRatio': -0.002, 'goodwillAll': 1}),
            ('opt3', {'unrest': True})])
ev(id='old_regent', family='court', minYear=5, timeout=3, fallback=1, cd=20, onlyPlayer=True,
   options=[('opt1', {'treasuryGdpRatio': -0.01, 'goodwillAll': 3}),
            ('opt2', {'unrest': True, 'styleWeights': SW_LEG})])
ev(id='bastard_claim', family='court', minYear=6, timeout=3, fallback=1, cd=20, onlyPlayer=True,
   options=[('opt1', {'treasuryGdpRatio': -0.008}),
            ('opt2', {'unrest': True})])
ev(id='spy_ring', family='court', minYear=4, timeout=3, fallback=1, cd=18, onlyPlayer=True,
   options=[('opt1', {'treasuryGdpRatio': -0.005, 'styleWeights': SW_ISO}),
            ('opt2', {'goodwillAll': -2})])

# ===== 军事 military +7 =====
ev(id='war_defector', family='military', minYear=4, timeout=2, fallback=2, cd=16,
   conds={'atWar': 1}, chain='defector_ambition', chainDelay=2, chainAfter=1,
   options=[('opt1', {'unrest': True, 'goodwillAll': 1, 'styleWeights': SW_LEG}),
            ('opt2', {'treasuryGdpRatio': -0.006, 'styleWeights': SW_MERCH}),
            ('opt3', {'goodwillAll': -3})])
ev(id='defector_ambition', family='military', minYear=0, timeout=2, fallback=1, cd=0,
   options=[('opt1', {'treasuryGdpRatio': -0.01, 'styleWeights': SW_MIL}),
            ('opt2', {'unrest': True})])
ev(id='supply_convoy', family='military', minYear=3, timeout=1, fallback=1, cd=10,
   conds={'atWar': 1},
   options=[('opt1', {'poorReliefRatio': 0.04}), ('opt2', {'unrest': True})])
ev(id='fort_rebuild', family='military', minYear=3, timeout=3, fallback=1, cd=18,
   options=[('opt1', {'treasuryGdpRatio': -0.012, 'goodwillAll': 2, 'styleWeights': SW_LEG}),
            ('opt2', {'unrest': True})])
ev(id='veteran_company', family='military', minYear=4, timeout=3, fallback=1, cd=16,
   options=[('opt1', {'treasuryGdpRatio': -0.008, 'styleWeights': SW_MIL}),
            ('opt2', {'goodwillAll': 1})])
ev(id='privateer_licence', family='military', minYear=4, timeout=3, fallback=1, cd=16,
   options=[('opt1', {'treasuryGdpRatio': 0.01, 'goodwillAll': -3, 'styleWeights': SW_MERCH}),
            ('opt2', {'goodwillAll': 2})])
ev(id='hero_funeral', family='military', minYear=3, timeout=3, fallback=1, cd=18,
   options=[('opt1', {'treasuryGdpRatio': -0.006, 'goodwillAll': 3}),
            ('opt2', {'unrest': True})])

# ===== 民生 civil +7 =====
ev(id='market_fire', family='civil', minYear=3, timeout=2, fallback=1, cd=12,
   options=[('opt1', {'treasuryGdpRatio': -0.007}), ('opt2', {'unrest': True})])
ev(id='water_shortage', family='civil', minYear=3, timeout=2, fallback=1, cd=12,
   options=[('opt1', {'treasuryGdpRatio': -0.008, 'styleWeights': SW_LEG}),
            ('opt2', {'unrest': True})])
ev(id='tenant_strike', family='civil', minYear=4, timeout=2, fallback=1, cd=14,
   conditions={'giniMin': 0.45},
   options=[('opt1', {'poorReliefRatio': 0.06}), ('opt2', {'unrest': True})])
ev(id='festival_request', family='civil', minYear=3, timeout=3, fallback=1, cd=14,
   options=[('opt1', {'treasuryGdpRatio': -0.005, 'goodwillAll': 2, 'styleWeights': SW_WEL}),
            ('opt2', {'unrest': True})])
ev(id='night_patrol', family='civil', minYear=3, timeout=3, fallback=1, cd=14,
   options=[('opt1', {'treasuryGdpRatio': -0.004, 'styleWeights': SW_LEG}),
            ('opt2', {'unrest': True})])
ev(id='bathhouse_fad', family='civil', minYear=3, timeout=3, fallback=1, cd=16,
   options=[('opt1', {'treasuryGdpRatio': -0.003, 'goodwillAll': 1, 'styleWeights': SW_WEL}),
            ('opt2', None)])
ev(id='traveling_fair', family='civil', minYear=3, timeout=2, fallback=1, cd=16,
   options=[('opt1', {'residentsTaxRatio': 0.006, 'goodwillAll': 1, 'styleWeights': SW_MERCH}),
            ('opt2', None)])

# ===== 外交 diplomacy +6 =====
ev(id='royal_visit', family='diplomacy', minYear=4, timeout=3, fallback=1, cd=16,
   options=[('opt1', {'treasuryGdpRatio': -0.008, 'goodwillAll': 3}),
            ('opt2', {'goodwillAll': -2})])
ev(id='hostage_request', family='diplomacy', minYear=5, timeout=3, fallback=1, cd=20,
   options=[('opt1', {'goodwillAll': 5, 'unrest': True}),
            ('opt2', {'goodwillAll': -3})])
ev(id='border_treaty', family='diplomacy', minYear=4, timeout=3, fallback=1, cd=18,
   conds={'atWar': 0},
   options=[('opt1', {'treasuryGdpRatio': -0.004, 'goodwillAll': 3}),
            ('opt2', {'goodwillAll': -2})])
ev(id='pirate_bribe', family='diplomacy', minYear=4, timeout=2, fallback=1, cd=14,
   options=[('opt1', {'treasuryGdpRatio': -0.006}),
            ('opt2', {'unrest': True, 'goodwillAll': -2, 'styleWeights': SW_MIL})])
ev(id='pilgrim_wave', family='diplomacy', minYear=3, timeout=2, fallback=1, cd=14,
   options=[('opt1', {'poorReliefRatio': 0.05, 'goodwillAll': 2}),
            ('opt2', {'goodwillAll': -2})])
ev(id='tribute_envoy', family='diplomacy', minYear=4, timeout=3, fallback=1, cd=16,
   options=[('opt1', {'treasuryGdpRatio': 0.008, 'goodwillAll': -1, 'styleWeights': SW_MERCH}),
            ('opt2', {'goodwillAll': 2})])

# 校验
ids = [e['id'] for e in data['events']]
assert len(ids) == len(set(ids)), 'duplicate ids'
chains = [e['chainNext'] for e in data['events'] if 'chainNext' in e]
for c in chains:
    assert c in ids, 'chain target missing: ' + c
print('total events:', len(ids))
json.dump(data, io.open(p, 'w', encoding='utf-8'), ensure_ascii=False, indent=2)
io.open(p, 'a', encoding='utf-8').write('\n')
print('events.json written')
