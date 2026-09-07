# -*- coding: utf-8 -*-
"""v1.7.0: convert 14 nested conditions to flat fields, add variantGroup tags,
append 240 new events (78 -> 318). Idempotent (skips existing ids)."""
import io, json, collections

data_path = 'events.json'
data = json.load(io.open(data_path, encoding='utf-8'), object_pairs_hook=collections.OrderedDict)
by_id = {e['id']: e for e in data['events']}
existing = set(by_id.keys())

# ===================== 1) 嵌套 conditions -> 扁平（引擎 EventDef 为扁平字段）=====================
COND_TARGETS = {
    'treasuryRatioMax': 'treasuryRatioMax',
    'treasuryRatioMin': 'treasuryRatioMin',
    'giniMin': 'giniMin', 'giniMax': 'giniMax',
    'atWar': 'atWar', 'bankRiskMin': 'bankRiskMin',
    'minPop': 'minPop', 'maxPop': 'maxPop', 'phase': 'phase',
}
converted = 0
for e in data['events']:
    if 'conditions' in e and isinstance(e['conditions'], dict):
        for k in e['conditions']:
            if k == 'phases':
                # 兼容历史字段 phases（数字数组）-> phase 取首个（历史未用，取第一个）
                try:
                    e['phase'] = int(e['conditions'][k])
                except Exception:
                    pass
            elif k in COND_TARGETS:
                e[COND_TARGETS[k]] = e['conditions'][k]
        del e['conditions']
        converted += 1
print('conditions flattened:', converted)

# ===================== 2) 变体组编组（现有同语义事件互斥化）=====================
WEATHER_GROUP = 'extreme_weather'
for eid in ('drought', 'locust', 'flood', 'wildfire', 'blizzard'):
    if eid in by_id and 'variantGroup' not in by_id[eid] and 'chainNext' not in by_id[eid]:
        by_id[eid]['variantGroup'] = WEATHER_GROUP
# 现有事件按题材成组：组内各版本互斥，同局只出部分（语义相近，替换感自然）
SEED_GROUPS = {
    'treasury_crisis': ('treasury_gap', 'tax_corruption', 'rich_petition'),
    'court_scandal': ('concubine_rivalry', 'royal_astrologer', 'hunt_accident'),
    'military_supply': ('army_pay', 'mercenary_default', 'supply_convoy'),
    'civil_unrest': ('bread_riot', 'tenant_strike', 'water_shortage'),
    'diplomacy_pressure': ('neighbor_extort', 'pirate_bribe', 'hostage_request'),
}
for gname, members in SEED_GROUPS.items():
    for eid in members:
        if eid in by_id and 'variantGroup' not in by_id[eid] and 'chainNext' not in by_id[eid]:
            by_id[eid]['variantGroup'] = gname
print('weather group tagged:', WEATHER_GROUP, '| seed groups:', len(SEED_GROUPS))

# ===================== 3) 追加新事件 =====================
def add(id, family, minYear=2, timeout=2, fallback=1, cd=14,
        onlyPlayer=False, conds=None, group=None, chain=None, chainDelay=1,
        chainAfter=None, options=None):
    if id in existing:
        return 0
    if not options or len(options) < 2:
        raise ValueError('%s: options < 2' % id)
    if fallback < 0 or fallback >= len(options):
        fallback = len(options) - 1  # 钳制到最后一个选项（历史笔误：2 对 2 选项越界）
    e = collections.OrderedDict()
    e['id'] = id; e['family'] = family
    e['minYear'] = minYear; e['timeoutYears'] = timeout
    e['fallback'] = fallback; e['cooldownYears'] = cd
    if onlyPlayer: e['onlyPlayer'] = True
    if conds:
        for k, v in conds.items():
            e[k] = v
    if group: e['variantGroup'] = group
    if chain:
        e['chainNext'] = chain
        e['chainDelay'] = chainDelay
        if chainAfter is not None: e['chainAfterOption'] = chainAfter
    e['options'] = [{'key': k, **(x or {})} for k, x in options]
    data['events'].append(e)
    return 1

# ============================ FINANCE +39 (17 -> 56) ============================
fin = [
 ('embezzle_ring', dict(giniMin=0.45), 2, [('opt1', {'unrest': False}), ('opt2', {'treasuryGdpRatio': -0.005})]),
 ('toll_rebellion', dict(giniMin=0.5), 1, [('opt1', {'unrest': True}), ('opt2', {'treasuryGdpRatio': -0.006})]),
 ('debt_auction', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.01}), ('opt2', {})]),
 ('specie_crisis', dict(phase=1), 2, [('opt1', {'treasuryGdpRatio': -0.008}), ('opt2', {'unrest': True})]),
 ('bank_discount', dict(treasuryRatioMin=0.3), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {})]),
 ('salt_law', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.012}), ('opt2', {'unrest': True})], 'trade_net'),
 ('liquor_law', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.01}), ('opt2', {'unrest': True})], 'trade_net'),
 ('iron_law', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.008}), ('opt2', {'goodwillAll': 1})], 'trade_net'),
 ('currency_ruin', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.012}), ('opt2', {'unrest': True})], None, 'mint_clampdown', 2, 0),
 ('mint_clampdown', dict(), 0, [('opt1', {'unrest': True}), ('opt2', {'treasuryGdpRatio': -0.004})]),
 ('customs_raise', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.009}), ('opt2', {'goodwillAll': -2})]),
 ('treasury_moth', dict(giniMin=0.4), 1, [('opt1', {'treasuryGdpRatio': -0.007}), ('opt2', {'unrest': True})]),
 ('war_debt', dict(atWar=1), 1, [('opt1', {'treasuryGdpRatio': -0.015}), ('opt2', {'unrest': True})]),
 ('subsidy_cut', dict(phase=1), 1, [('opt1', {'goodwillAll': -2}), ('opt2', {'unrest': True})]),
 ('trade_house_rise', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {})]),
 ('usury_ring', dict(giniMin=0.55), 1, [('opt1', {'unrest': True}), ('opt2', {'treasuryGdpRatio': 0.005})]),
 ('granary_speculation', dict(phase=3), 1, [('opt1', {'treasuryGdpRatio': 0.008}), ('opt2', {'unrest': True})]),
 ('ferry_tax', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.006}), ('opt2', {'goodwillAll': -1})]),
 ('market_margin', dict(phase=0), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {'treasuryGdpRatio': 0.005})]),
 ('tax_reform_court', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.01, 'unrest': True}), ('opt2', {})]),
 ('royal_mint_debase', dict(phase=1), 1, [('opt1', {'treasuryGdpRatio': 0.015}), ('opt2', {'unrest': True})]),
 ('savings_hunt', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.01}), ('opt2', {'goodwillAll': -1})]),
 ('wage_freeze', dict(giniMax=0.55), 1, [('opt1', {'unrest': True}), ('opt2', {})]),
 ('gdp_boom_report', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {'unrest': True})]),
 ('land_sale_urgent', dict(treasuryRatioMin=0.1), 1, [('opt1', {'treasuryGdpRatio': 0.012}), ('opt2', {})]),
 ('prize_fund', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.005}), ('opt2', {'goodwillAll': 1})]),
 ('customs_leak', dict(giniMax=0.5), 1, [('opt1', {'treasuryGdpRatio': -0.008}), ('opt2', {'unrest': True})]),
 ('debt_default', dict(), 1, [('opt1', {'goodwillAll': -3}), ('opt2', {'unrest': True})], None, 'credit_ice', 2, 0),
 ('credit_ice', dict(), 0, [('opt1', {'treasuryGdpRatio': -0.01}), ('opt2', {'unrest': True})]),
 ('merchant_gift', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.008}), ('opt2', {'goodwillAll': -1})]),
 ('tax_shield', dict(giniMax=0.5), 1, [('opt1', {'treasuryGdpRatio': -0.006}), ('opt2', {'unrest': True})]),
 ('relief_auction', dict(phase=1), 1, [('opt1', {'treasuryGdpRatio': 0.007}), ('opt2', {'goodwillAll': 1})]),
 ('coin_cut', dict(phase=1), 1, [('opt1', {'unrest': True}), ('opt2', {})]),
 ('enterprise_rent', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.007}), ('opt2', {'unrest': True})]),
 ('tax_relief_petition', dict(giniMax=0.5), 1, [('opt1', {'goodwillAll': 2, 'treasuryGdpRatio': -0.008}), ('opt2', {'unrest': True})]),
 ('state_pawnshop', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.006}), ('opt2', {})]),
 ('port_duties', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.009}), ('opt2', {'goodwillAll': -1})]),
 ('fraud_accounting', dict(), 1, [('opt1', {'unrest': True}), ('opt2', {'treasuryGdpRatio': -0.005})]),
 ('budget_lean', dict(phase=1), 1, [('opt1', {'treasuryGdpRatio': 0.008}), ('opt2', {'unrest': True})]),
]
for spec in fin:
    eid, conds, fb, opts = spec[0], spec[1], spec[2], spec[3]
    group = spec[4] if len(spec) > 4 else None
    chain = spec[5] if len(spec) > 5 else None
    cdelay = spec[6] if len(spec) > 6 else 1
    cafter = spec[7] if len(spec) > 7 else None
    add(eid, 'finance', minYear=1, timeout=2, fallback=fb, cd=(0 if eid in ('mint_clampdown', 'credit_ice') else 16),
        onlyPlayer=eid in ('bank_discount', 'currency_ruin', 'mint_clampdown', 'debt_default', 'credit_ice', 'state_pawnshop'),
        conds=conds, group=group, chain=chain, chainDelay=cdelay, chainAfter=cafter,
        options=opts)

# ============================ COURT +40 (15 -> 55) ============================
court = [
 ('duke_sword', dict(), 1, [('opt1', {'unrest': True}), ('opt2', {'goodwillAll': 1})], 'court_undercurrent'),
 ('palace_lady', dict(), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {'unrest': True})], 'court_undercurrent'),
 ('eunuch_power', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.005, 'goodwillAll': 1}), ('opt2', {'unrest': True})], 'court_undercurrent'),
 ('dowager_veil', dict(), 1, [('opt1', {'unrest': True}), ('opt2', {'goodwillAll': 1})], None, 'regent_reform', 2, 0),
 ('regent_reform', dict(), 0, [('opt1', {'goodwillAll': 2}), ('opt2', {'unrest': True})]),
 ('crown_prince_visit', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {})]),
 ('royal_confessor', dict(), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {})]),
 ('palace_oath', dict(), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {'unrest': True})]),
 ('hunt_omen', dict(), 1, [('opt1', {'unrest': True}), ('opt2', {})]),
 ('heir_tutor', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {})]),
 ('concubine_audit', dict(), 1, [('opt1', {'unrest': True}), ('opt2', {})]),
 ('marriage_plot', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {'unrest': True})]),
 ('royal_bodyguard', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.004}), ('opt2', {})]),
 ('eunuch_land', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.008}), ('opt2', {'unrest': True})]),
 ('censor_impeach', dict(), 1, [('opt1', {'unrest': True}), ('opt2', {})]),
 ('court_banquet', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.006, 'goodwillAll': 1}), ('opt2', {})]),
 ('astrology_omen', dict(phase=1), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {})]),
 ('royal_seal', dict(), 1, [('opt1', {'unrest': True}), ('opt2', {'treasuryGdpRatio': -0.004})]),
 ('succession_will', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {'unrest': True})]),
 ('old_guard_pension', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.006}), ('opt2', {'unrest': True})]),
 ('prince_exile', dict(), 1, [('opt1', {'goodwillAll': -2}), ('opt2', {'unrest': True})]),
 ('royal_birth', dict(), 1, [('opt1', {'goodwillAll': 3}), ('opt2', {})]),
 ('heir_duel', dict(), 1, [('opt1', {'unrest': True}), ('opt2', {'goodwillAll': 1})]),
 ('court_faction', dict(), 1, [('opt1', {'unrest': True}), ('opt2', {'goodwillAll': 2})]),
 ('eunuch_reform', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.007}), ('opt2', {'unrest': True})]),
 ('queen_rescript', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {'unrest': True})]),
 ('loyalist_swear', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {})]),
 ('spy_in_palace', dict(), 1, [('opt1', {'goodwillAll': -1}), ('opt2', {'unrest': True})], None, 'spy_exposure', 1, 0),
 ('spy_exposure', dict(), 0, [('opt1', {'unrest': True}), ('opt2', {'goodwillAll': 1})]),
 ('crown_jewels', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.008}), ('opt2', {'unrest': True})]),
 ('hunting_pact', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {})]),
 ('fortune_teller', dict(), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {'unrest': True})]),
 ('princess_guard', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.004}), ('opt2', {})]),
 ('noble_trial', dict(), 1, [('opt1', {'unrest': True}), ('opt2', {'goodwillAll': 1})]),
 ('jester_reign', dict(), 1, [('opt1', {'unrest': True}), ('opt2', {})]),
 ('ancestral_rites', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.007, 'goodwillAll': 1}), ('opt2', {})]),
 ('royal_soothsayer', dict(phase=1), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {'unrest': True})]),
 ('heir_foreign', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {'unrest': True})]),
 ('king_brew', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.004, 'goodwillAll': 1}), ('opt2', {})]),
 ('heir_adoption', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {'unrest': True})]),
]
for spec in court:
    eid, conds, fb, opts = spec[0], spec[1], spec[2], spec[3]
    group = spec[4] if len(spec) > 4 else None
    chain = spec[5] if len(spec) > 5 else None
    cdelay = spec[6] if len(spec) > 6 else 1
    cafter = spec[7] if len(spec) > 7 else None
    add(eid, 'court', minYear=1, timeout=2, fallback=fb, cd=(0 if eid in ('regent_reform', 'spy_exposure') else 15),
        onlyPlayer=True, conds=conds, group=group, chain=chain, chainDelay=cdelay, chainAfter=cafter,
        options=opts)

# ============================ MILITARY +40 (14 -> 54) ============================
mil = [
 ('sword_guild', dict(), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {'unrest': True})], 'arms_rivalry'),
 ('armor_works', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.006}), ('opt2', {'unrest': True})], 'arms_rivalry'),
 ('archer_lodge', dict(), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {})], 'arms_rivalry'),
 ('wage_arrears', dict(atWar=1), 1, [('opt1', {'treasuryGdpRatio': -0.01}), ('opt2', {'unrest': True})], None, 'mutiny_quell', 2, 0),
 ('mutiny_quell', dict(), 0, [('opt1', {'unrest': True}), ('opt2', {'treasuryGdpRatio': -0.006})]),
 ('levy_call', dict(atWar=1), 1, [('opt1', {'unrest': True}), ('opt2', {'treasuryGdpRatio': -0.008})]),
 ('border_watch', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.006}), ('opt2', {})]),
 ('veteran_pension', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.007}), ('opt2', {'unrest': True})]),
 ('marching_training', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.005}), ('opt2', {})]),
 ('general_rivalry', dict(), 1, [('opt1', {'unrest': True}), ('opt2', {'goodwillAll': 1})]),
 ('siege_test', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.006}), ('opt2', {})]),
 ('scout_failure', dict(atWar=1), 1, [('opt1', {'unrest': True}), ('opt2', {})]),
 ('navy_draft', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.007}), ('opt2', {'unrest': True})]),
 ('camp_disease', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.008}), ('opt2', {})]),
 ('weapon_bribe', dict(), 1, [('opt1', {'unrest': True}), ('opt2', {})]),
 ('royal_guard_move', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.004}), ('opt2', {})]),
 ('desertion', dict(atWar=1), 1, [('opt1', {'unrest': True}), ('opt2', {'treasuryGdpRatio': -0.004})]),
 ('war_plunder_share', dict(atWar=1), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {'unrest': True})]),
 ('general_young', dict(), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {'unrest': True})]),
 ('fort_novelty', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.006}), ('opt2', {})]),
 ('outpost_security', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.005}), ('opt2', {})]),
 ('wounded_care', dict(atWar=1), 1, [('opt1', {'treasuryGdpRatio': -0.007}), ('opt2', {'unrest': True})]),
 ('hero_statue', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.006}), ('opt2', {'goodwillAll': 2})]),
 ('land_grant_soldier', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.008}), ('opt2', {'goodwillAll': 1})]),
 ('militia_rise', dict(), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {'unrest': True})]),
 ('spy_report', dict(atWar=1), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {})]),
 ('ambush_warning', dict(atWar=1), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {})]),
 ('army_reduction', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.01}), ('opt2', {'unrest': True})]),
 ('war_trophy', dict(atWar=1), 1, [('opt1', {'treasuryGdpRatio': 0.008}), ('opt2', {'goodwillAll': 1})]),
 ('general_madness', dict(), 1, [('opt1', {'unrest': True}), ('opt2', {})]),
 ('barracks_brawl', dict(), 1, [('opt1', {'unrest': True}), ('opt2', {})]),
 ('supply_shortfall', dict(atWar=1), 1, [('opt1', {'treasuryGdpRatio': -0.009}), ('opt2', {'unrest': True})]),
 ('battlefield_courage', dict(atWar=1), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {})]),
 ('victory_parade', dict(atWar=1), 1, [('opt1', {'treasuryGdpRatio': -0.007, 'goodwillAll': 2}), ('opt2', {})]),
 ('troop_resupply', dict(atWar=1), 1, [('opt1', {'treasuryGdpRatio': -0.008}), ('opt2', {'unrest': True})]),
 ('general_protector', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {'unrest': True})]),
 ('border_feast', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.005, 'goodwillAll': 1}), ('opt2', {})]),
 ('archery_club', dict(), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {})]),
 ('watchtower_signal', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.005}), ('opt2', {})]),
 ('army_fund_embezzle', dict(), 1, [('opt1', {'unrest': True}), ('opt2', {'treasuryGdpRatio': -0.005})]),
]
for spec in mil:
    eid, conds, fb, opts = spec[0], spec[1], spec[2], spec[3]
    group = spec[4] if len(spec) > 4 else None
    chain = spec[5] if len(spec) > 5 else None
    cdelay = spec[6] if len(spec) > 6 else 1
    cafter = spec[7] if len(spec) > 7 else None
    add(eid, 'military', minYear=1, timeout=2, fallback=fb, cd=(0 if eid == 'mutiny_quell' else 16),
        conds=conds, group=group, chain=chain, chainDelay=cdelay, chainAfter=cafter,
        options=opts)

# ============================ DISASTER +40 (11 -> 51) ============================
dis = [
 ('shooting_star', dict(), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {'unrest': True})], 'celestial_omen'),
 ('solar_eclipse', dict(), 1, [('opt1', {'unrest': True}), ('opt2', {})], 'celestial_omen'),
 ('blood_moon', dict(), 1, [('opt1', {'unrest': True}), ('opt2', {'goodwillAll': 1})], 'celestial_omen'),
 ('quake_aftershock', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.01}), ('opt2', {'unrest': True})], None, 'quake_rebuild', 2, 0),
 ('quake_rebuild', dict(), 0, [('opt1', {'treasuryGdpRatio': -0.012}), ('opt2', {'unrest': True})]),
 ('hailstorm', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.006}), ('opt2', {})]),
 ('frostbite_spring', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.007}), ('opt2', {'unrest': True})]),
 ('heat_wave', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.006}), ('opt2', {'unrest': True})]),
 ('typhoon_wind', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.009}), ('opt2', {'unrest': True})]),
 ('sandstorm', dict(), 1, [('opt1', {'goodwillAll': -1}), ('opt2', {})]),
 ('mudslide', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.007}), ('opt2', {'unrest': True})]),
 ('avalanche', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.006}), ('opt2', {})]),
 ('lightning_storm', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.004}), ('opt2', {'unrest': True})]),
 ('drought_wells', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.008}), ('opt2', {'unrest': True})]),
 ('river_channel', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.007}), ('opt2', {})]),
 ('locust_larvae', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.008}), ('opt2', {'unrest': True})]),
 ('crop_blight', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.007}), ('opt2', {})]),
 ('orchard_wilt', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.005}), ('opt2', {})]),
 ('cattle_pest', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.006}), ('opt2', {'unrest': True})]),
 ('well_poison', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.008}), ('opt2', {'unrest': True})]),
 ('roof_snow', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.005}), ('opt2', {})]),
 ('glacier_melt', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.007}), ('opt2', {'unrest': True})]),
 ('sea_riser', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.008}), ('opt2', {'unrest': True})]),
 ('dust_death', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.006}), ('opt2', {'unrest': True})]),
 ('earthquake_fissure', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.009}), ('opt2', {'unrest': True})]),
 ('ash_cloud', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.01}), ('opt2', {'unrest': True})]),
 ('forest_fire', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.008}), ('opt2', {'unrest': True})]),
 ('ice_storm', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.006}), ('opt2', {})]),
 ('thunder_harvest', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.006}), ('opt2', {'unrest': True})]),
 ('famine_seed', dict(), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {'unrest': True})], None, 'famine_full', 2, 0),
 ('famine_full', dict(), 0, [('opt1', {'treasuryGdpRatio': -0.015}), ('opt2', {'unrest': True})]),
 ('storm_ruin', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.008}), ('opt2', {'unrest': True})]),
 ('plague_carrier', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.01}), ('opt2', {'unrest': True})]),
 ('rat_wave', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.006}), ('opt2', {'unrest': True})]),
 ('harvest_rot', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.007}), ('opt2', {})]),
 ('crop_ice', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.006}), ('opt2', {'unrest': True})]),
 ('storm_tide', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.008}), ('opt2', {'unrest': True})]),
 ('rain_delayed', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.006}), ('opt2', {'unrest': True})]),
 ('grass_fire', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.006}), ('opt2', {})]),
 ('river_ice', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.005}), ('opt2', {})]),
]
for spec in dis:
    eid, conds, fb, opts = spec[0], spec[1], spec[2], spec[3]
    group = spec[4] if len(spec) > 4 else None
    chain = spec[5] if len(spec) > 5 else None
    cdelay = spec[6] if len(spec) > 6 else 1
    cafter = spec[7] if len(spec) > 7 else None
    add(eid, 'disaster', minYear=1, timeout=2, fallback=fb, cd=(0 if eid in ('quake_rebuild', 'famine_full') else 16),
        conds=conds, group=group, chain=chain, chainDelay=cdelay, chainAfter=cafter,
        options=opts)

# ============================ CIVIL +40 (11 -> 51) ============================
civ = [
 ('mill_guild', dict(), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {'unrest': True})], 'guild_walk'),
 ('tailor_march', dict(), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {'unrest': True})], 'guild_walk'),
 ('smith_guild', dict(), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {'unrest': True})], 'guild_walk'),
 ('guild_monopoly', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.008}), ('opt2', {'unrest': True})], None, 'guild_strike', 2, 0),
 ('guild_strike', dict(), 0, [('opt1', {'unrest': True}), ('opt2', {'goodwillAll': 1})]),
 ('street_lamp', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.005}), ('opt2', {})]),
 ('schooling_plan', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.008}), ('opt2', {'unrest': True})]),
 ('surgery_house', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.006}), ('opt2', {})]),
 ('bath_fee', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.005}), ('opt2', {'unrest': True})]),
 ('beggar_raid', dict(giniMin=0.55), 1, [('opt1', {'unrest': True}), ('opt2', {'treasuryGdpRatio': -0.004})]),
 ('orphan_home', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.006}), ('opt2', {})]),
 ('widow_fund', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.005}), ('opt2', {'goodwillAll': 1})]),
 ('madman_case', dict(), 1, [('opt1', {'unrest': True}), ('opt2', {})]),
 ('rumor_wave', dict(phase=1), 1, [('opt1', {'unrest': True}), ('opt2', {})]),
 ('book_market', dict(), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {})]),
 ('playhouse', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.004}), ('opt2', {'unrest': True})]),
 ('festival_curfew', dict(), 1, [('opt1', {'unrest': True}), ('opt2', {'goodwillAll': 1})]),
 ('boat_regatta', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.004}), ('opt2', {'goodwillAll': 2})]),
 ('flower_walk', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.004}), ('opt2', {'goodwillAll': 1})]),
 ('wedding_boom', dict(phase=0), 1, [('opt1', {'treasuryGdpRatio': 0.006}), ('opt2', {})]),
 ('funeral_custom', dict(), 1, [('opt1', {'unrest': True}), ('opt2', {})]),
 ('food_contest', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.004}), ('opt2', {'goodwillAll': 1})]),
 ('night_market', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.006}), ('opt2', {'unrest': True})]),
 ('water_taxi', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.005}), ('opt2', {})]),
 ('chimney_cleaner', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.004}), ('opt2', {})]),
 ('brew_wave', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.006}), ('opt2', {'unrest': True})]),
 ('silk_weavers', dict(), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {})]),
 ('paper_dealers', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.005}), ('opt2', {})]),
 ('potters_quarrel', dict(), 1, [('opt1', {'unrest': True}), ('opt2', {})]),
 ('lamp_oil', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.005}), ('opt2', {'unrest': True})]),
 ('salt_caravan', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.007}), ('opt2', {'goodwillAll': 1})]),
 ('spice_spree', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.006}), ('opt2', {})]),
 ('street_show', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.004}), ('opt2', {'goodwillAll': 1})]),
 ('temple_market', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.005}), ('opt2', {'goodwillAll': 1})]),
 ('communal_well', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.004}), ('opt2', {})]),
 ('bridge_toll', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.006}), ('opt2', {'unrest': True})]),
 ('neighbor_dispute', dict(), 1, [('opt1', {'unrest': True}), ('opt2', {})]),
 ('house_rent', dict(), 1, [('opt1', {'unrest': True}), ('opt2', {})]),
 ('harvest_dance', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.003}), ('opt2', {'goodwillAll': 2})]),
 ('witch_case', dict(), 1, [('opt1', {'unrest': True}), ('opt2', {'goodwillAll': 1})]),
]
for spec in civ:
    eid, conds, fb, opts = spec[0], spec[1], spec[2], spec[3]
    group = spec[4] if len(spec) > 4 else None
    chain = spec[5] if len(spec) > 5 else None
    cdelay = spec[6] if len(spec) > 6 else 1
    cafter = spec[7] if len(spec) > 7 else None
    add(eid, 'civil', minYear=1, timeout=2, fallback=fb, cd=(0 if eid == 'guild_strike' else 16),
        conds=conds, group=group, chain=chain, chainDelay=cdelay, chainAfter=cafter,
        options=opts)

# ============================ DIPLOMACY +41 (10 -> 51) ============================
dip = [
 ('border_stone', dict(), 1, [('opt1', {'goodwillAll': -2}), ('opt2', {'goodwillAll': 2})], 'border_rift'),
 ('fishing_spat', dict(), 1, [('opt1', {'goodwillAll': -1}), ('opt2', {'goodwillAll': 2})], 'border_rift'),
 ('hunt_line', dict(), 1, [('opt1', {'goodwillAll': -1}), ('opt2', {'goodwillAll': 2})], 'border_rift'),
 ('hostage_walk', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {'unrest': True})], None, 'hostage_return', 2, 0),
 ('hostage_return', dict(), 0, [('opt1', {'goodwillAll': 2}), ('opt2', {'unrest': True})]),
 ('embassy_swap', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {})]),
 ('state_visit', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.006, 'goodwillAll': 2}), ('opt2', {})]),
 ('peace_priest', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {'unrest': True})]),
 ('gift_entourage', dict(), 1, [('opt1', {'goodwillAll': 2, 'treasuryGdpRatio': -0.005}), ('opt2', {})]),
 ('salt_pact', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {'goodwillAll': -1})]),
 ('grain_bridge', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {})]),
 ('ship_licence', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.007}), ('opt2', {'goodwillAll': -1})]),
 ('ferry_treaty', dict(), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {})]),
 ('exile_welcome', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {'unrest': True})]),
 ('rival_envoy', dict(), 1, [('opt1', {'goodwillAll': -2}), ('opt2', {'goodwillAll': 2})]),
 ('neutral_pledge', dict(), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {})]),
 ('betrothal_overture', dict(), 1, [('opt1', {'goodwillAll': 3}), ('opt2', {})]),
 ('border_market', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.006, 'goodwillAll': 1}), ('opt2', {})]),
 ('trade_embargo', dict(), 1, [('opt1', {'goodwillAll': -2}), ('opt2', {'goodwillAll': 1})]),
 ('embassy_code', dict(), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {'unrest': True})]),
 ('spy_swap', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {})]),
 ('pirate_offer', dict(), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {'treasuryGdpRatio': 0.005})]),
 ('refugee_treaty', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {'unrest': True})]),
 ('river_fishery', dict(), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {})]),
 ('mountain_pass', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {'goodwillAll': -1})]),
 ('temple_diplomacy', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {})]),
 ('scholar_visit', dict(), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {})]),
 ('doctor_mission', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {})]),
 ('astronomical_mission', dict(), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {})]),
 ('military_pact', dict(), 1, [('opt1', {'goodwillAll': 3}), ('opt2', {'goodwillAll': -2})]),
 ('naval_truce', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {'goodwillAll': -1})]),
 ('garrison_border', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.005, 'goodwillAll': 1}), ('opt2', {})]),
 ('diplomat_bride', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {'unrest': True})]),
 ('insult_word', dict(), 1, [('opt1', {'goodwillAll': -2}), ('opt2', {'goodwillAll': 1})]),
 ('gesture_of_peace', dict(), 1, [('opt1', {'goodwillAll': 3}), ('opt2', {})]),
 ('tax_for_trade', dict(), 1, [('opt1', {'treasuryGdpRatio': 0.006, 'goodwillAll': 1}), ('opt2', {'goodwillAll': -1})]),
 ('royal_letter', dict(), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {})]),
 ('currency_pact', dict(), 1, [('opt1', {'goodwillAll': 2}), ('opt2', {})]),
 ('map_dispute', dict(), 1, [('opt1', {'goodwillAll': -1}), ('opt2', {'goodwillAll': 1})]),
 ('goodwill_feast', dict(), 1, [('opt1', {'treasuryGdpRatio': -0.005, 'goodwillAll': 2}), ('opt2', {})]),
 ('embassy_rotation', dict(), 1, [('opt1', {'goodwillAll': 1}), ('opt2', {})]),
]
for spec in dip:
    eid, conds, fb, opts = spec[0], spec[1], spec[2], spec[3]
    group = spec[4] if len(spec) > 4 else None
    chain = spec[5] if len(spec) > 5 else None
    cdelay = spec[6] if len(spec) > 6 else 1
    cafter = spec[7] if len(spec) > 7 else None
    add(eid, 'diplomacy', minYear=1, timeout=2, fallback=fb, cd=(0 if eid == 'hostage_return' else 16),
        conds=conds, group=group, chain=chain, chainDelay=cdelay, chainAfter=cafter,
        options=opts)

# ===================== 归一化保障（幂等重跑安全）=====================
for e in data['events']:
    n = len(e.get('options') or [])
    if n > 0 and (e.get('fallback', 0) < 0 or e.get('fallback', 0) >= n):
        e['fallback'] = n - 1

# ===================== 写回 =====================
with io.open(data_path, 'w', encoding='utf-8') as f:
    json.dump(data, f, ensure_ascii=False, indent=2)
    f.write('\n')
from collections import Counter
fam = Counter(e['family'] for e in data['events'])
print('events.json total:', len(data['events']))
print('families:', dict(fam))
print('chains:', sum(1 for e in data['events'] if e.get('chainNext')))
print('variant groups:', sorted(set(e['variantGroup'] for e in data['events'] if e.get('variantGroup'))))
