# -*- coding: utf-8 -*-
"""v1.6.0 最終残り（UIラベル/政策名/外交トースト/災害トースト等）。冪等。"""
import io, json, collections

en = json.load(io.open('Locales/en.json', encoding='utf-8'))

JA = collections.OrderedDict()
DE = collections.OrderedDict()
def tja(**kw): JA.update(kw)
def tde(**kw): DE.update(kw)

ja_final = {
 # rich list / 未闘状態
 'col_gdp': 'GDP', 'rich_empty': '（文明化エンティティなし）',
 'rich_noworld': '（ワールド未ロード — ワールドに入れば自動更新）',
 'rich_nok': '王国なし',
 # UI ラベル（旧・廃止済みだが en.json に残存）
 'ui_language': 'UI 言語', 'use_chinese_ui': '中文 UI を使用',
 'trait_group_classical_era': '古典時代',
 'memory_cleanup_toast': 'メモリを自動整理：解放 {0} MB、{1} バッファ',
 # 経済トースト
 'toast_unrest_start': '[不安] <{0}> 暴動が発生；{1} 都市が反乱',
 'toast_uprising': '[蜂起] <{0}> 街頭蜂起！{1} 富裕層処刑',
 'toast_bubble_burst': '[経済] バブル破裂！{0} の富が蒸発',
 'toast_era_start': '[時代] {0} が <{1}> に始まる',
 'toast_banking_crisis': '[経済] 銀行危機！恐慌デフォルトが拡散',
 'toast_disaster': '[経済] 災害発生！{0} 都市被災、{1} 損失',
 'toast_revolution': '[革命] <{0}> 政権崩壊！{1} 処刑、{2} 再分配',
 'unrest_state_uprising': '☠ {0} 街頭蜂起！政権崩壊',
 'unrest_state_none': '（不安なし）',
 'unrest_state_threshold': '自動発動：格差 ≥ {0}',
 # 国家政策名
 'nation_policy_taxcut': '減税', 'nation_policy_taxup': '増税',
 'nation_policy_poorrelief': '貧民救済', 'nation_policy_propaganda': 'プロパガンダ',
 'nation_policy_tradepact': '鋳貨', 'nation_policy_tariff': '国家専売',
 'nation_decree_relief': '緊急救済', 'nation_decree_festival': '国慶祭',
 'nation_build_market': '市場', 'nation_build_granary': '穀倉',
 # 外交トースト
 'toast_nation_destroyed_war': '[内閣] 戦争が {0} の建築を破壊！',
 'toast_nation_destroyed_disaster': '[内閣] 災害が {0} の建築を破壊',
 'toast_nation_build_unavailable': '[内閣] このゲームに対応建築なし',
 'toast_dip_declare_ok': '[外交] <{0}> に宣戦！',
 'toast_dip_peace_ok': '[外交] <{0}> と講和',
 'toast_dip_alliance_ok': '[外交] <{0}> と同盟締結',
 'toast_dip_gift_ok': '[外交] <{0}> に贈物送付；関係改善',
 'toast_dip_no_nation': '[外交] 先に王国を統治せよ',
 'toast_dip_unavailable': '[外交] 原版外交 API 利用不可',
 'toast_dip_not_war': '[外交] <{0}> とは交戦中でない',
 'toast_dip_peace_poor': '[外交] 宝物庫が講和条件を満たせない',
 'toast_dip_alliance_war': '[外交] 戦争中は同盟不可',
 'toast_dip_war_poor': '[外交] 宝物庫が宣戦費用を満たせない',
 'toast_dip_gift_reject': '[外交] 贈物が拒否された',
 'toast_dip_pact_full': '[外交] パクト枠満杯（最大2）',
 'toast_dip_pact_war': '[外交] 戦争相手とはパクト不可',
 'toast_dip_pact_same': '[外交] 同階のパクト済み',
}
tja(**ja_final)

de_final = {
 'col_gdp': 'GDP', 'col_gini': 'Gini',
 'gini_chart_gini': 'Gini', 'gini_chart_phase': 'Phase',
 'rich_empty': '（Keine zivilisierten Akteure）',
 'rich_noworld': '（Keine Welt geladen — Welt betreten für Auto-Update）',
 'rich_nok': 'kein Königreich',
 'ui_language': 'UI-Sprache', 'use_chinese_ui': 'Chinesische UI verwenden',
 'trait_group_classical_era': 'Klassisches Zeitalter',
 'memory_cleanup_toast': 'Speicher automatisch bereinigt: {0} MB freigegeben ({1} Puffer)',
 'toast_unrest_start': '[Unruhe] <{0}> Unruhe bricht aus; {1} Städte rebellieren',
 'toast_uprising': '[Aufstand] <{0}> Straßenaufstand! {1} Reiche hingerichtet',
 'toast_bubble_burst': '[Wirtschaft] Blase platzt! {0} Reichtum verdampft',
 'toast_era_start': '[Zeitalter] {0} beginnt in <{1}>',
 'toast_banking_crisis': '[Wirtschaft] Bankenkrise! Depression-Ausfälle verbreiten sich',
 'toast_disaster': '[Wirtschaft] Katastrophe! {0} Städte getroffen, {1} verloren',
 'toast_revolution': '[Revolution] <{0}> Regime fällt! {1} getötet, {2} umverteilt',
 'unrest_state_uprising': '☠ {0} Straßenaufstand! Regime gefallen',
 'unrest_state_none': '（Keine Unruhe）',
 'unrest_state_threshold': 'Auto-Auslöser: Kluft ≥ {0}',
 'nation_policy_taxcut': 'Steuersenkung', 'nation_policy_taxup': 'Steuererhöhung',
 'nation_policy_poorrelief': 'Armenfürsorge', 'nation_policy_propaganda': 'Propaganda',
 'nation_policy_tradepact': 'Münzprägung', 'nation_policy_tariff': 'Staatsmonopol',
 'nation_decree_relief': 'Nothilfe', 'nation_decree_festival': 'Nationalfest',
 'nation_build_market': 'Markt', 'nation_build_granary': 'Kornspeicher',
 'toast_nation_destroyed_war': '[Kabinett] Krieg hat ein Gebäude von {0} zerstört!',
 'toast_nation_destroyed_disaster': '[Kabinett] Katastrophe hat ein Gebäude von {0} zerstört',
 'toast_nation_build_unavailable': '[Kabinett] Kein passendes Gebäude-Asset in diesem Spiel',
 'toast_dip_declare_ok': '[Diplomatie] Krieg gegen <{0}> erklärt!',
 'toast_dip_peace_ok': '[Diplomatie] Frieden mit <{0}> geschlossen',
 'toast_dip_alliance_ok': '[Diplomatie] Bündnis mit <{0}> gebildet',
 'toast_dip_gift_ok': '[Diplomatie] Geschenk an <{0}> gesendet; Beziehung verbessert',
 'toast_dip_no_nation': '[Diplomatie] Zuerst ein Königreich übernehmen',
 'toast_dip_unavailable': '[Diplomatie] Vanille-Diplomatie-API nicht verfügbar',
 'toast_dip_not_war': '[Diplomatie] Nicht im Krieg mit <{0}>',
 'toast_dip_peace_poor': '[Diplomatie] Schatzkammer kann Friedensbedingungen nicht bezahlen',
 'toast_dip_alliance_war': '[Diplomatie] Kein Bündnis während des Krieges',
 'toast_dip_war_poor': '[Diplomatie] Schatzkammer kann Kriegskosten nicht bezahlen',
 'toast_dip_gift_reject': '[Diplomatie] Geschenk abgelehnt',
 'toast_dip_pact_full': '[Diplomatie] Pakt-Slots voll (max. 2)',
 'toast_dip_pact_war': '[Diplomatie] Kein Pakt mit Kriegsgegner',
 'toast_dip_pact_same': '[Diplomatie] Pakt dieser Stufe besteht bereits',
 'ev_revolution': 'Revolution', 'cycle_phase_boom': 'Boom',
 'cycle_phase_depression': 'Depression',
}
tde(**de_final)

for lang, d in [('ja', JA), ('de', DE)]:
    p = 'Locales/%s.json' % lang
    existing = json.load(io.open(p, encoding='utf-8'), object_pairs_hook=collections.OrderedDict)
    merged = 0
    for k, v in d.items():
        if existing.get(k) == en.get(k):
            existing[k] = v; merged += 1
    json.dump(existing, io.open(p, 'w', encoding='utf-8'), ensure_ascii=False, indent=2)
    io.open(p, 'a', encoding='utf-8').write('\n')
    t2 = sum(1 for k in en if existing.get(k) != en.get(k))
    miss = [k for k in en if existing.get(k) == en.get(k)]
    print(lang, 'merged:', merged, 'translated:', t2, '/', len(en), '(', round(t2/len(en)*100), '%)', 'remaining:', len(miss))
    for k in miss[:8]: print('   still:', k)
