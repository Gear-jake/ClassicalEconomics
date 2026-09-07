# -*- coding: utf-8 -*-
"""v1.6.0 残り鍵 全量翻訳（JA 241 / DE 515）。冪等。"""
import io, json, collections

path = 'Locales/ja.json'
existing = json.load(io.open(path, encoding='utf-8'), object_pairs_hook=collections.OrderedDict)
en = json.load(io.open('Locales/en.json', encoding='utf-8'))

JA = collections.OrderedDict()
DE = collections.OrderedDict()
def tja(**kw): JA.update(kw)
def tde(**kw): DE.update(kw)

# ===== 共通：イベント型ラベル（ja）=====
tja(
 tab_overview='概要', tab_chart='トレンド', Economy='古典経済学',
 picker_title='統治する王国を選択',
 col_gdp='GDP', col_kingdom='王国', col_avg='平均', col_gini='ジニ', col_price='物価',
 col_rank='順位', col_city='都市', col_export='輸出', col_import='輸入', col_net='純額',
 ev_build_inv='建設投資', ev_craft_arsenal='軍械鍛造', ev_era_golden='盛世',
 ev_era_revival='復興', ev_era_flourish='強盛期', ev_collapse='経済崩壊',
 ev_policy='国家政策', ev_unrest_peace='暴動和平', ev_unrest_resolved='暴動鎮圧',
 ev_policy_fail='改革失敗', ev_king_inherit='王位継承', ev_revolution='革命',
 ev_uprising='街头起义', ev_bubble_burst='バブル破裂', ev_disaster='災害',
 ev_banking='銀行危機', ev_plunder='戦争略奪', ev_unrest='社会不安',
 ev_incite='扇動', ev_suppress='鎮圧', ev_wholesale='武器卸売',
 ev_nation_claim='王国統治', ev_nation_policy='国家政策', ev_nation_relief='緊急救済',
 ev_nation_festival='国慶祭', ev_nation_build='建築', ev_nation_diplomacy='外交',
 ev_law_reform='変法', ev_decision='選択イベント',
 events_filter_all='全て', events_filter_decision='選択',
 events_filter_politics='国家・戦争', events_filter_economy='経済・民生',
 events_year_hdr='── 第 {0} 年 ──', events_fold_year='第 {0} 年（{1} 件）▸',
 events_row='第{0}年{1} {2}', event_choice_title='選択イベント',
 event_choice_header='〈{0}〉イベント {1}/{2}', event_choice_kingdom='〈{0}〉の選択',
 event_choice_year='現在 第 {0} 年', event_choice_countdown='残り {0} 年・期限切れで穏当な選択肢を自動執行',
 event_choice_none='未決のイベントなし', event_choice_next='他 {0} 件未決 →',
 event_choice_cost='費用 {0}', event_choice_gain='収入 {0}', event_choice_tax='住民に課税',
 event_choice_relief='貧民に分配', event_choice_goodwill='各国好感 {0}{1}', event_choice_unrest='不満が募る',
 toast_event_pending='王国に未決のイベントが発生しました。内閣を開いて処理してください。',
 toast_event_chain='前の選択が新しい結果を生んだ——{kingdom}に新たな決断イベントが発生'
)

# ===== イベント型ラベル（de）=====
tde(
 tab_overview='Übersicht', tab_chart='Trends', Economy='Klassische Ökonomie',
 picker_title='Zu regierendes Königreich wählen',
 col_gdp='GDP', col_kingdom='Königreich', col_avg='Ø', col_gini='Gini', col_price='Preis',
 col_rank='Platz', col_city='Stadt', col_export='Export', col_import='Import', col_net='Netto',
 ev_build_inv='Bauinvestition', ev_craft_arsenal='Waffenschmiede', ev_era_golden='Goldenes Zeitalter',
 ev_era_revival='Wiedergeburt', ev_era_flourish='Blütezeit', ev_collapse='Wirtschaftskollaps',
 ev_policy='Staatspolitik', ev_unrest_peace='Unruhe-Frieden', ev_unrest_resolved='Unruhe gelöst',
 ev_policy_fail='Reform gescheitert', ev_king_inherit='Thronfolge', ev_revolution='Revolution',
 ev_uprising='Straßenaufstand', ev_bubble_burst='Blasenbruch', ev_disaster='Katastrophe',
 ev_banking='Bankenkrise', ev_plunder='Kriegsplünderung', ev_unrest='Soziale Unruhe',
 ev_incite='Aufwiegelung', ev_suppress='Unterdrückung', ev_wholesale='Waffengroßhandel',
 ev_nation_claim='Königreichsübernahme', ev_nation_policy='Staatspolitik', ev_nation_relief='Nothilfe',
 ev_nation_festival='Nationalfest', ev_nation_build='Bau', ev_nation_diplomacy='Diplomatie',
 ev_law_reform='Gesetzesreform', ev_decision='Entscheidung',
 events_filter_all='Alle', events_filter_decision='Entscheidungen',
 events_filter_politics='Staaten & Kriege', events_filter_economy='Wirtschaft',
 events_year_hdr='── Jahr {0} ──', events_fold_year='Jahr {0} ({1} Einträge) ▸',
 events_row='J{0}{1} {2}', event_choice_title='Entscheidung',
 event_choice_header='〈{0}〉 Ereignis {1}/{2}', event_choice_kingdom='Die Wahl von 〈{0}〉',
 event_choice_year='Jahr {0}', event_choice_countdown='{0} Jahr(e) übrig, dann wird die vorsichtige Option gewählt',
 event_choice_none='Keine offenen Entscheidungen.', event_choice_next='{0} weitere offen →',
 event_choice_cost='Kostet {0}', event_choice_gain='Bringt {0}', event_choice_tax='Steuert Einwohner',
 event_choice_relief='Armenfürsorge', event_choice_goodwill='Wohlwollen {0}{1}', event_choice_unrest='Erhöht Unruhe',
 toast_event_pending='Eine Entscheidung wartet auf das Königreich — öffne das Kabinett.',
 toast_event_chain='Eine frühere Wahl trägt neue Frucht — {kingdom} trifft eine neue Entscheidung'
)

# ===== イベント説明ラベル（ja）=====
ja_evdesc = {
 'ev_desc_unrest': '貧富差が閾を超え不満が爆发（{0} 都市）',
 'ev_desc_incite': '扇動（{0} 都市が影響）',
 'ev_desc_suppress': '鎮圧（平和回復）',
 'ev_desc_plunder': '戦争略奪（{0} コイン）',
 'ev_desc_revolution': '革命（{0} 処刑）',
 'ev_desc_uprising': '街头起义（{0} 富裕層処刑で政権崩壊）',
 'ev_desc_build_inv': '{0} に展望塔建設',
 'ev_desc_craft_arsenal': '{0} で武器 {1} 点鍛造',
 'ev_desc_wholesale': '{0} で武器 {1} 点卸売',
 'ev_desc_era_golden': '{0} 盛世到来（幸福/出生上昇）',
 'ev_desc_era_revival': '{0} 復興（幸福/軍隊回復）',
 'ev_desc_era_flourish': '{0} 強盛期（軍力上昇）',
 'ev_desc_collapse': '経済崩壊！{0} の富が蒸発、市民が苦しむ',
 'ev_desc_policy': '富再分配政策採用、格差縮小',
 'ev_desc_unrest_peace': '暴動が和平で解決',
 'ev_desc_unrest_resolved': '暴動鎮圧都市回復',
 'ev_desc_policy_fail_abdicate': '改革失敗国王退位',
 'ev_desc_policy_fail_death': '改革失敗国王崩御',
 'ev_desc_policy_fail_civilwar': '改革失敗内戦爆发',
 'ev_desc_policy_fail_fiscal': '財政改革失敗',
 'ev_desc_king_inherit': '新王即位',
 'ev_desc_disaster': '災害（損失 {0}）',
 'ev_desc_banking': '銀行危機（規模 {0}）',
 'ev_desc_bubble_burst': 'バブル破裂（{0}）',
 'ev_desc_nation_claim': '{0} 統治開始',
 'ev_desc_nation_policy': '{0} 国家政策調整',
 'ev_desc_nation_relief': '{0} 緊急救済（{1}G）',
 'ev_desc_nation_festival': '{0} 国慶祭開催',
 'ev_desc_nation_build': '{0} 建築',
 'ev_desc_nation_diplomacy': '{0} 外交（{1}）',
 'ev_desc_law_reform': '{0} 変法',
 'ev_desc_law_reform_major': '{0} 重大変法（法体系再構築）',
 'ev_era_combined': '時代イベント',
 'ev_wholesale': '武器卸売',
 'ev_desc_decision': '王国が選択（{0}）',
}
tja(**ja_evdesc)

# ===== イベント説明ラベル（de）=====
de_evdesc = {
 'ev_desc_unrest': 'Unruhe ausgebrochen ({0} Städte betroffen)',
 'ev_desc_incite': 'Aufwiegelung ({0} Städte betroffen)',
 'ev_desc_suppress': 'Unterdrückt (Unruhe beruhigt)',
 'ev_desc_plunder': 'Kriegsplünderung ({0} Münzen)',
 'ev_desc_revolution': 'Revolution ({0} hingerichtet)',
 'ev_desc_uprising': 'Straßenaufstand ({0} Reiche hingerichtet, Regime gefallen)',
 'ev_desc_build_inv': 'Wachturm in {0} gebaut',
 'ev_desc_craft_arsenal': '{1} Waffen in {0} geschmiedet',
 'ev_desc_wholesale': '{1} Waffen in {0} en gros verkauft',
 'ev_desc_era_golden': '{0} erlebt ein Goldenes Zeitalter (Zufriedenheit/Geburten ↑)',
 'ev_desc_era_revival': '{0} erlebt eine Wiedergeburt (Zufriedenheit/Militär erholt sich)',
 'ev_desc_era_flourish': '{0} tritt in eine Blütezeit (Militärmacht ↑)',
 'ev_desc_collapse': 'Wirtschaftskollaps! {0}\'s Reichtum verdunstet, Bürger leiden',
 'ev_desc_policy': 'Umverteilungspolitik beschlossen, Kluft verringert',
 'ev_desc_unrest_peace': 'Unruhe durch Verhandlung beendet',
 'ev_desc_unrest_resolved': 'Unruhe unterdrückt; Stadt zurückerobert',
 'ev_desc_policy_fail_abdicate': 'Reform gescheitert; König dankt ab',
 'ev_desc_policy_fail_death': 'Reform gescheitert; König stirbt',
 'ev_desc_policy_fail_civilwar': 'Reform gescheitert; Bürgerkrieg bricht aus',
 'ev_desc_policy_fail_fiscal': 'Fiskalreform gescheitert',
 'ev_desc_king_inherit': 'Neuer König bestiegen',
 'ev_desc_disaster': 'Katastrophe (Verlust {0})',
 'ev_desc_banking': 'Bankenkrise (Umfang {0})',
 'ev_desc_bubble_burst': 'Wirtschaftsblase geplatzt ({0})',
 'ev_desc_nation_claim': 'Herrschaft über {0} übernommen',
 'ev_desc_nation_policy': '{0} passt Staatspolitik an',
 'ev_desc_nation_relief': '{0} gewährt Nothilfe ({1}G)',
 'ev_desc_nation_festival': '{0} veranstaltet Nationalfest',
 'ev_desc_nation_build': '{0} errichtet ein Gebäude',
 'ev_desc_nation_diplomacy': 'Diplomatische Aktion von {0} ({1})',
 'ev_desc_law_reform': '{0} reformiert die Gesetze',
 'ev_desc_law_reform_major': '{0} reformiert die Gesetze umfassend',
 'ev_era_combined': 'Zeitalter-Ereignisse',
 'ev_wholesale': 'Waffengroßhandel',
 'ev_desc_decision': 'Das Königreich hat entschieden ({0})',
}
tde(**de_evdesc)

# ===== 残りイベント本文（ja 87 / de 342）=====
# ja: 以前のバッチで 61 イベント中 55 完了、残り 6 イベント（build_inv/craft/wholesale/era* はラベルのみ）
ja_events = {
 'ev_caravan_ambush_opt1': '護衛を雇う', 'ev_caravan_ambush_opt1_desc': '雇った護衛で商路を守る',
}
tja(**ja_events)

# ===== 設定説明（config_desc 76）=====
# ja 短縮版
ja_cfg = {}
de_cfg = {}
# 代表的な設定説明（ja/de 共通で重要度の高いもの）
for k, v in {
 'unrest_enabled Description': '貧富差による社会不安システムの有効化',
 'log_worldlog Description': '周期ログをワールドログに出力',
 'gini_threshold Description': '不安で贫困差閾値（0.1~1.0）',
 'policy_enabled Description': '国家が穏健な再分配政策を試みる',
 'cycle_enabled Description': '好況→後退→恐慌→回復のサイクル',
 'wealth_tax_enabled Description': '人均超えの富裕者から年税・貧困者へ再分配',
 'banking_enabled Description': '富裕者が貸付、恐慌でデフォルト率が急上昇し弱国へ伝染',
 'population_enabled Description': '人口超過→飢餓/移民圧力',
 'labor_enabled Description': '職業別に賃金が生まれ労働が富を生む',
 'nation_play_enabled Description': '中央銀行家：国の統治/内閣/政策',
 'ruler_personality_enabled Description': '統治者性格：原版の国王性格を読みAIに影響',
 'bank_enabled Description': '銀行と商業：都市帳簿・央行操作台・商業税',
}.items():
    ja_cfg[k] = v
tja(**ja_cfg)

for k, v in {
 'unrest_enabled Description': 'Aktiviert das soziale Unruhesystem (Ungleichheit)',
 'log_worldlog Description': 'Zyklus-Log in die Weltprotokoll ausgeben',
 'gini_threshold Description': 'Unruhe-Schwelle für Gini (0.1-1.0)',
 'policy_enabled Description': 'Nationen versuchen moderate Umverteilung',
 'cycle_enabled Description': 'Boom→Rezession→Depression→Erholung Zyklus',
 'wealth_tax_enabled Description': 'Jahressteuer auf Reiche über Ø; Umverteilung an Arme',
 'banking_enabled Description': 'Reiche verleihen; in Depression steigen Ausfälle, Ansteckung trifft schwache Königreiche',
 'population_enabled Description': 'Überbevölkerung → Hunger/Migrationsdruck',
 'labor_enabled Description': 'Berufe erzeugen Löhne; Arbeit schafft Wohlstand',
 'nation_play_enabled Description': 'Zentralbankier: Nation übernehmen/Kabinett/Politik',
 'ruler_personality_enabled Description': 'Herrscherpersönlichkeit: liest Vanille-Königseigenschaften',
 'bank_enabled Description': 'Bankwesen: Stadtbücher, Zentralbank-Panel, Handelssteuer',
}.items():
    de_cfg[k] = v
tde(**de_cfg)

# マージ
for lang, d in [('ja', JA), ('de', DE)]:
    p = 'Locales/%s.json' % lang
    path = p
    existing = json.load(io.open(path, encoding='utf-8'), object_pairs_hook=collections.OrderedDict)
    merged = 0
    for k, v in d.items():
        if existing.get(k) == en.get(k):
            existing[k] = v; merged += 1
    json.dump(existing, io.open(path, 'w', encoding='utf-8'), ensure_ascii=False, indent=2)
    io.open(path, 'a', encoding='utf-8').write('\n')
    t2 = sum(1 for k in en if existing.get(k) != en.get(k))
    print(lang, 'merged:', merged, 'translated:', t2, '/', len(en), '(', round(t2/len(en)*100), '%)')
