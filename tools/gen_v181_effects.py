# -*- coding: utf-8 -*-
"""v1.8.1: 现有事件接入 v1.8 新效果键（语义契合微调，幂等——已有键不覆盖）。
只改 options 效果键与部分 desc 文案；事件条件/连锁/时机不动。"""
import io, json, collections

# (事件id, 选项下标, 新效果键, 修改后的选项3行文案元组(ch,zh_tw,en,ru,ja,de) 或 None=不动文案)
PLAN = [
    # ==== 结盟 formAllianceTarget ====
    ('marriage_alliance', 0, {'formAllianceTarget': -1}, None),
    ('princess_betrothal', 0, {'formAllianceTarget': -1}, None),
    ('heir_foreign', 0, {'formAllianceTarget': 1}, None),
    ('military_pact', 0, {'formAllianceTarget': 1}, None),
    ('hunting_pact', 0, {'formAllianceTarget': -1}, None),
    ('gift_entourage', 0, {'formAllianceTarget': -1}, None),
    # ==== 宣战 declareWarTarget ====
    ('war_escalation', 0, {'declareWarTarget': -1}, None),
    ('ally_betrayal', 0, {'declareWarTarget': 2}, None),
    # ==== 迁都 moveCapital ====
    ('volcano_eruption', 0, {'moveCapital': True}, None),
    ('earthquake_fissure', 0, {'moveCapital': True}, None),
    # ==== 升级建筑 upgradeBuildings ====
    ('fort_rebuild', 0, {'upgradeBuildings': 4}, None),
    ('watchtower_signal', 0, {'upgradeBuildings': 3}, None),
    ('street_lamp', 0, {'upgradeBuildings': 2}, None),
    ('communal_well', 0, {'upgradeBuildings': 2}, None),
    ('surgery_house', 0, {'upgradeBuildings': 2}, None),
    ('market_fire', 0, {'upgradeBuildings': 3}, None),
    # ==== 个体财富 citizenWealthRatio ====
    ('granary_speculation', 0, {'citizenWealthRatio': 0.5}, None),
    ('market_margin', 0, {'citizenWealthRatio': 0.4}, None),
    ('usury_ring', 1, {'citizenWealthRatio': 0.6}, None),
    ('merchant_gift', 0, {'citizenWealthRatio': 0.3}, None),
    ('war_plunder_share', 0, {'citizenWealthRatio': 0.4}, None),
    ('coin_cut', 0, {'citizenWealthRatio': -0.3}, None),
    ('wage_freeze', 0, {'citizenWealthRatio': -0.2}, None),
]

data_path = 'events.json'
d = json.load(io.open(data_path, encoding='utf-8'), object_pairs_hook=collections.OrderedDict)
by_id = {e['id']: e for e in d['events']}

updated = 0
for eid, optIdx, fx, texts in PLAN:
    e = by_id.get(eid)
    if e is None:
        print('MISSING event', eid); continue
    o = e['options'][optIdx]
    if 'key' not in o: getkey = 'opt%d' % (optIdx + 1); o['key'] = getkey
    # 幂等：不覆盖已有键
    for k, v in fx.items():
        if k not in o and o['key'] != 'key':
            o[k] = v
            updated += 1
        elif k not in o:
            o[k] = v
            updated += 1
    print(eid, 'opt', optIdx, ' ->', json.dumps(fx, ensure_ascii=False), 'options=', json.dumps(e['options'], ensure_ascii=False))

with io.open(data_path, 'w', encoding='utf-8') as f:
    json.dump(d, f, ensure_ascii=False, indent=2)
    f.write('\n')
print('fields updated:', updated)
