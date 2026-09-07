# -*- coding: utf-8 -*-
"""v1.6.1: 15 new events + 3 chain extensions + 2 new chains. Idempotent (skips existing ids)."""
import io, json, collections

# ===================== events.json 追加 =====================
data_path = 'events.json'
data = json.load(io.open(data_path, encoding='utf-8'), object_pairs_hook=collections.OrderedDict)
by_id = {e['id']: e for e in data['events']}
existing = set(by_id.keys())

def add(**kw):
    if kw['id'] in existing:
        return
    e = collections.OrderedDict()
    e['id'] = kw['id']; e['family'] = kw['family']
    e['minYear'] = kw.get('minYear', 2); e['timeoutYears'] = kw.get('timeout', 2)
    e['fallback'] = kw['fallback']; e['cooldownYears'] = kw.get('cd', 14)
    if kw.get('onlyPlayer'): e['onlyPlayer'] = True
    if kw.get('conds'): e['conditions'] = kw['conds']
    if kw.get('chain'): e['chainNext'] = kw['chain']
    if kw.get('chainDelay'): e['chainDelay'] = kw['chainDelay']
    if kw.get('chainAfter') is not None: e['chainAfterOption'] = kw['chainAfter']
    e['options'] = [{'key': k, **(x or {})} for k, x in kw['options']]
    data['events'].append(e)

# --- 延长现有链：补 chainNext ---
if 'loan_due' in by_id: by_id['loan_due']['chainNext'] = 'loan_recovery'; by_id['loan_due']['chainDelay'] = 2; by_id['loan_due']['chainAfterOption'] = 0
if 'heir_plot_crushed' in by_id: by_id['heir_plot_crushed']['chainNext'] = 'heir_legacy'; by_id['heir_plot_crushed']['chainDelay'] = 2
if 'defector_ambition' in by_id: by_id['defector_ambition']['chainNext'] = 'defector_rebellion'; by_id['defector_ambition']['chainDelay'] = 2; by_id['defector_ambition']['chainAfterOption'] = 1

# ===== 链尾新事件 =====
add(id='loan_recovery', family='finance', minYear=0, timeout=2, fallback=0, cd=0, onlyPlayer=True,
    options=[('opt1', {'goodwillAll': 3, 'treasuryGdpRatio': -0.004}),
             ('opt2', {'unrest': True})])
add(id='heir_legacy', family='court', minYear=0, timeout=3, fallback=0, cd=0, onlyPlayer=True,
    options=[('opt1', {'treasuryGdpRatio': -0.01, 'unrest': True}),
             ('opt2', {'goodwillAll': 2})])
add(id='defector_rebellion', family='military', minYear=0, timeout=2, fallback=1, cd=0,
    options=[('opt1', {'unrest': True}),
             ('opt2', {'treasuryGdpRatio': -0.008, 'goodwillAll': 2})])

# ===== 新事件（12）=====
add(id='royal_heist', family='finance', minYear=3, timeout=2, fallback=1, cd=16, onlyPlayer=True,
    options=[('opt1', {'treasuryGdpRatio': -0.01}),
             ('opt2', {'treasuryGdpRatio': -0.004, 'unrest': True})])
add(id='mint_shortage', family='finance', minYear=4, timeout=2, fallback=1, cd=18,
    options=[('opt1', {'treasuryGdpRatio': -0.008, 'styleWeights': {'style_merchant': 2.0}}),
             ('opt2', {'unrest': True})])
add(id='volcano_eruption', family='disaster', minYear=3, timeout=1, fallback=1, cd=10,
    options=[('opt1', {'poorReliefRatio': 0.1}),
             ('opt2', {'unrest': True})])
add(id='plague_second', family='disaster', minYear=4, timeout=2, fallback=1, cd=12,
    options=[('opt1', {'treasuryGdpRatio': -0.01, 'styleWeights': {'style_welfare': 2.0}}),
             ('opt2', {'unrest': True})])
add(id='cult_rise', family='court', minYear=3, timeout=3, fallback=1, cd=16, onlyPlayer=True,
    chain='cult_aftermath', chainDelay=2,
    options=[('opt1', {'goodwillAll': 2, 'treasuryGdpRatio': -0.005}),
             ('opt2', {'unrest': True})])
add(id='cult_aftermath', family='court', minYear=0, timeout=2, fallback=1, cd=0, onlyPlayer=True,
    options=[('opt1', {'treasuryGdpRatio': -0.006, 'goodwillAll': 1}),
             ('opt2', {'unrest': True})])
add(id='princess_betrothal', family='court', minYear=4, timeout=3, fallback=1, cd=18, onlyPlayer=True,
    options=[('opt1', {'treasuryGdpRatio': -0.01, 'goodwillAll': 4}),
             ('opt2', {'goodwillAll': -2})])
add(id='border_skirmish', family='military', minYear=3, timeout=2, fallback=1, cd=12,
    chain='war_escalation', chainDelay=2, chainAfter=1,
    options=[('opt1', {'goodwillAll': 3}),
             ('opt2', {'unrest': True})])
add(id='war_escalation', family='military', minYear=0, timeout=2, fallback=0, cd=0,
    options=[('opt1', {'treasuryGdpRatio': -0.012, 'styleWeights': {'style_bellicose': 2.0}}),
             ('opt2', {'goodwillAll': 2, 'treasuryGdpRatio': -0.004})])
add(id='armory_explosion', family='military', minYear=3, timeout=2, fallback=1, cd=14,
    options=[('opt1', {'treasuryGdpRatio': -0.01}),
             ('opt2', {'unrest': True})])
add(id='city_fire', family='civil', minYear=2, timeout=1, fallback=1, cd=10,
    options=[('opt1', {'treasuryGdpRatio': -0.008}),
             ('opt2', {'unrest': True})])
add(id='refugee_wave', family='civil', minYear=3, timeout=2, fallback=1, cd=14,
    options=[('opt1', {'poorReliefRatio': 0.08, 'goodwillAll': 2, 'styleWeights': {'style_welfare': 2.0}}),
             ('opt2', {'unrest': True})])
add(id='ally_betrayal', family='diplomacy', minYear=4, timeout=2, fallback=1, cd=16,
    options=[('opt1', {'unrest': True, 'goodwillAll': -2}),
             ('opt2', {'goodwillAll': 3})])
add(id='secret_negotiation', family='diplomacy', minYear=4, timeout=3, fallback=1, cd=16,
    options=[('opt1', {'treasuryGdpRatio': -0.004, 'goodwillAll': 3}),
             ('opt2', {'goodwillAll': -1})])

json.dump(data, io.open(data_path, 'w', encoding='utf-8'), ensure_ascii=False, indent=2)
io.open(data_path, 'a', encoding='utf-8').write('\n')
print('events.json total:', len(data['events']), '(new:', len(data['events']) - 61, ')')
