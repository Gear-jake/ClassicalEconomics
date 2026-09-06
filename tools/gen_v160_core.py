# -*- coding: utf-8 -*-
"""v1.6.0 batch: ja+de non-event translations (~415 keys each). Merges into existing locale files."""
import io, json, collections

# --- Japanese ---
JA = collections.OrderedDict()
def T(d):
    JA.update(d)

# 法典档位名（ja）— 全法×全档
tiers_ja = {
 'law_education_lv0':'なし','law_education_lv1':'初等','law_education_lv2':'中等','law_education_lv3':'高等','law_education_lv4':'全民',
 'law_healthcare_lv0':'なし','law_healthcare_lv1':'基本','law_healthcare_lv2':'標準','law_healthcare_lv3':'充実','law_healthcare_lv4':'無料',
 'law_migrant_lv0':'封鎖','law_migrant_lv1':'制限','law_migrant_lv2':'開放','law_migrant_lv3':'歓迎','law_migrant_lv4':'無制限',
 'law_religion_lv0':'圧迫','law_religion_lv1':'寛容','law_religion_lv2':'保護','law_religion_lv3':'国教','law_religion_lv4':'政教一致',
 'law_press_lv0':'自由','law_press_lv1':'標準','law_press_lv2':'検閲','law_press_lv3':'統制','law_press_lv4':'独占',
 'law_gun_control_lv0':'無制限','law_gun_control_lv1':'緩い','law_gun_control_lv2':'許可','law_gun_control_lv3':'禁止','law_gun_control_lv4':'全面禁止',
 'law_conscription_lv0':'志願','law_conscription_lv1':'部分的','law_conscription_lv2':'標準','law_conscription_lv3':'強制','law_conscription_lv4':'皆兵',
 'law_standing_army_lv0':'なし','law_standing_army_lv1':'小規模','law_standing_army_lv2':'標準','law_standing_army_lv3':'大規模','law_standing_army_lv4':'巨大',
 'law_militarism_lv0':'平和','law_militarism_lv1':'備戦','law_militarism_lv2':'軍拡','law_militarism_lv3':'軍国','law_militarism_lv4':'総力戦',
 'law_pacifism_lv0':'好戦','law_pacifism_lv1':'中立','law_pacifism_lv2':'平和','law_pacifism_lv3':'非武装','law_pacifism_lv4':'完全非武装',
 'law_judicial_lv0':'王命','law_judicial_lv1':'慣習','law_judicial_lv2':'成文','law_judicial_lv3':'独立','law_judicial_lv4':'最高',
 'law_capital_pun_lv0':'廃止','law_capital_pun_lv1':'極刑','law_capital_pun_lv2':'重罪','law_capital_pun_lv3':'広範','law_capital_pun_lv4':'濫用',
 'law_ant_corrupt_lv0':'放置','law_ant_corrupt_lv1':'調査','law_ant_corrupt_lv2':'取締','law_ant_corrupt_lv3':'厳罰','law_ant_corrupt_lv4':'零容認',
 'law_prison_lv0':'厳罰','law_prison_lv1':'標準','law_prison_lv2':'更生','law_prison_lv3':'教育','law_prison_lv4':'解放',
 'law_forest_lv0':'無制限','law_forest_lv1':'管理','law_forest_lv2':'保護','law_forest_lv3':'厳格','law_forest_lv4':'神聖',
 'law_animal_lv0':'無関','law_animal_lv1':'関心','law_animal_lv2':'保護','law_animal_lv3':'厳格','law_animal_lv4':'聖獣',
 'law_pollution_lv0':'無視','law_pollution_lv1':'関心','law_pollution_lv2':'規制','law_pollution_lv3':'厳格','law_pollution_lv4':'完全',
 'law_monarchy_lv0':'無王','law_monarchy_lv1':'選挙王','law_monarchy_lv2':'世襲王','law_monarchy_lv3':'絶対王','law_monarchy_lv4':'神権王',
 'law_parliament_lv0':'なし','law_parliament_lv1':'諮問','law_parliament_lv2':'立法','law_parliament_lv3':'主導','law_parliament_lv4':'支配',
 'law_planned_economy_lv0':'自由','law_planned_economy_lv1':'指導','law_planned_economy_lv2':'統制','law_planned_economy_lv3':'計画','law_planned_economy_lv4':'完全',
 'law_free_market_lv0':'統制','law_free_market_lv1':'規制緩和','law_free_market_lv2':'自由','law_free_market_lv3':'完全自由','law_free_market_lv4':'無政府',
 'law_state_religion_lv0':'なし','law_state_religion_lv1':'優遇','law_state_religion_lv2':'公認','law_state_religion_lv3':'国教','law_state_religion_lv4':'強制',
 'law_secularism_lv0':'国教','law_secularism_lv1':'分離','law_secularism_lv2':'中立','law_secularism_lv3':'政教分離','law_secularism_lv4':'無宗教',
 'law_land_reform_lv0':'無改革','law_land_reform_lv1':'漸進','law_land_reform_lv2':'本格','law_land_reform_lv3':'強制収用','law_land_reform_lv4':'全面再配分',
 'law_antimonopoly_lv0':'放置','law_antimonopoly_lv1':'監視','law_antimonopoly_lv2':'規制','law_antimonopoly_lv3':'解体','law_antimonopoly_lv4':'国有化',
 'law_tax_system_lv0':'無税','law_tax_system_lv1':'軽税','law_tax_system_lv2':'中税','law_tax_system_lv3':'重税','law_tax_system_lv4':'極重税',
 'law_property_rights_lv0':'無保障','law_property_rights_lv1':'部分的','law_property_rights_lv2':'標準','law_property_rights_lv3':'強固','law_property_rights_lv4':'絶対',
 'law_trade_freedom_lv0':'放任','law_trade_freedom_lv1':'市場自由','law_trade_freedom_lv2':'規制市場','law_trade_freedom_lv3':'厳格統制','law_trade_freedom_lv4':'統制経済',
}
T(tiers_ja)

# 法カテゴリ追加
T({'law_cat_environment':'環境法','law_cat_ideology':'理念法','law_cat_social':'社会法',
   'law_mutex_tip':'相互排他','law_eff_gini':'ジニ','law_eff_happy':'幸福'})

# 建設名
T({'build_native_house_t1':'基本住宅','build_native_house_t3':'標準住宅','build_native_house_t5':'豪華住宅',
   'build_native_barracks':'兵営','build_native_watchtower':'展望塔','build_native_well':'井戸',
   'build_native_mine':'鉱山','build_native_statue':'記念碑','build_native_temple':'神殿',
   'build_native_bonfire':'焚き火','cabinet_build_native':'原生建築設置（地図クリック）',
   'cabinet_city_built':'建設済','cabinet_place_mode':'設置モード','cabinet_place_hint':'左クリック設置・右クリック取消',
   'cabinet_policies':'持続政策','cabinet_policy_hint':'政策は占拠；有効化初年に年費、変更は差額',
   'cabinet_decrees':'一回用法令','cabinet_decree_hint':'法令は即時発効；冷却中は再執行不可',
   'cabinet_decree_cost_festival':'費用：人口×0.5｜冷却3年','cabinet_decree_cost_relief':'費用：人口×平均×2%｜冷却5年',
   'cabinet_records':'政績記録','cabinet_record_open':'開く','cabinet_record_row':'第{0}年 {1}（{2}G）',
   'cabinet_no_records':'政績記録なし','cabinet_more_cities':'他{0}都市','cabinet_no_cities':'都市なし',
   'cabinet_diplomacy':'外交（経済+外交大臣）','cabinet_diplomacy_hint':'贈物は好感を累積し同盟の閾値と外交の実意を構成',
   'cabinet_gdp_chart':'GDPトレンド','cabinet_gdp_chart_now':'現在：{0}','cabinet_gdp_chart_short':'近{0}期',
   'cabinet_law_effect_none':'偏差なし',
   'col_gdp':'GDP','col_kingdom':'王国','col_price':'物価',
   'chart_title':'GDPトレンド','chart_year':'第{0}年','chart_global':'全球','chart_dropped':'(省略)',
   'chart_macro':'GDP {0}・平均 {1}・ジニ {2}','chart_macro_avg':'平均：{0}',
   'gini_chart_gini':'ジニ','gini_chart_phase':'段階',
   'cycle_phase':'段階','cycle_phase_boom':'好況','cycle_phase_recession':'後退',
   'cycle_phase_depression':'恐慌','cycle_phase_recovery':'回復','cycle_detail':'第{0}期・成長率{1}・バブル{2}',
   'economy_general':'経済','economy_title':'古典経済学',
   'event_family_finance':'財政','event_family_disaster':'天災','event_family_court':'宮廷',
   'event_family_military':'軍事','event_family_civil':'民生','event_family_diplomacy':'外交',
   'kingdom_law_header':'法典','kingdom_law_counts':'法律 {0} 条・国策 {1} 条',
   'hud_mem_cleanup':'前回整理 {0}｜解放 {1} ({2} バッファ)','hud_mem_cleanup_pending':'メモリ整理：解放待ち',
   'hud_mem_usage':'管理ヒープ {0}｜Unity 使用 {1} (予約 {2})','hud_mem_na':'—',
   'toast_nation_claim':'[認領] <{0}> を統治開始（宝物庫 {1}）',
   'toast_nation_unbind':'国が滅び統治を解除',
   'toast_nation_policy_on':'[政策] {0} 有効化（第{1}档）',
   'toast_nation_policy_off':'[政策] {0} 停止',
   'toast_nation_policy_suspended':'庫不足で {0} 一時停止',
   'toast_nation_no_slot':'政策枠が満杯（最大{0}）',
   'toast_nation_poor_treasury':'宝物庫不足',
   'toast_nation_cooldown':'クールダウン中',
   'toast_nation_relief':'[法令] 緊急救済：{0} に配給',
   'toast_nation_festival':'[法令] 国慶祭執行（不安解消）',
   'toast_nation_built':'<{0}> 建設完成（-{1}G）',
   'toast_nation_built_already':'建設済',
   'toast_nation_build_failed':'設置失敗',
   'toast_nation_place_mode':'設置モード：{0}',
   'toast_nation_place_cancelled':'終了',
   'toast_nation_place_land':'海上不可',
   'toast_nation_place_territory':'本国領内のみ',
   'toast_dip_war':'[外交] <{0}> に宣戦',
   'toast_dip_peace':'[外交] <{0}> と講和（+{1}G）',
   'toast_dip_alliance':'[外交] <{0}> と同盟',
   'toast_dip_gift':'[外交] <{0}> に贈物',
   'toast_dip_pact_ok':'[外交] <{0}> と経済協定',
   'toast_dip_pact_cancel':'[外交] 協定解除',
   'toast_unrest_peace':'暴動が和平で解決',
   'toast_unrest_resolved':'暴動が鎮圧され都市回復',
   'ev_era_combined':'時代イベント',
   'ruler_skim':'王室経費','nation_commerce_tax':'商業税',
})

# 設定ラベル（短縮版）
cfg_ja = {
 'unrest_enabled':'不安システム','log_worldlog':'ログ出力','gini_threshold':'ジニ閾値',
 'unrest_grace_years':'猶予年数','unrest_max_cities':'最大影響都市',
 'policy_enabled':'国家政策','cycle_enabled':'経済サイクル',
 'cycle_gini_high':'高ジニ閾値','cycle_gini_low':'低ジニ閾値','cycle_gini_periods':'サイクル期間',
 'boom_stimulus_ratio':'好況刺激比','boom_bubble_factor':'バブル係数','bubble_threshold':'バブル閾値',
 'boom_max_duration':'好況最長','recession_max_duration':'後退最長',
 'depression_max_duration':'恐慌最長','recovery_max_duration':'回復最長',
 'survival_line':'生存線','war_plunder_ratio':'戦争略奪比','war_waste_ratio':'戦争消耗比',
 'revolution_delay_years':'革命遅延','revolution_kill_ratio':'革命処刑比',
 'uprising_gini_threshold':'蜂起ジニ','uprising_delay_years':'蜂起遅延',
 'kill_rich_ratio':'富人処刑比','kill_rich_redist_ratio':'富人再配分比',
 'wealth_tax_enabled':'富裕税','wealth_tax_ratio':'富裕税率','wealth_tax_line':'富裕税線',
 'population_enabled':'人口制約','population_overcrowd':'過稠閾値',
 'era_enabled':'時代イベント','era_duration_years':'時代持続','collapse_drop_ratio':'崩壊下落比',
 'collapse_duration_years':'崩壊持続','flourish_military_ratio':'強盛軍事比','flourish_periods':'強盛期数',
 'labor_enabled':'労働分工','labor_wage_base':'基本賃金',
 'real_time_refresh':'リアルタイム更新','real_time_interval':'更新間隔',
 'real_time_refresh_threshold':'更新閾値','real_time_refresh_budget':'更新予算',
 'money_velocity':'貨幣流速','inflation_bubble_boost':'通胀加速',
 'disaster_enabled':'災害衝撃','disaster_wealth_loss':'災害損失','disaster_mine_bonus':'火山刺激',
 'banking_enabled':'銀行貸付','credit_rate':'貸付利率',
 'default_rate_depression':'恐慌デフォルト率','crisis_contagion_threshold':'伝染閾値',
 'spending_cap_per_year':'年間消費上限','banking_default_cap_per_year':'銀行デフォルト上限',
 'banking_contagion_cap_per_year':'伝染評価上限','inheritance_scan_per_frame':'遺産走査枠',
 'frame_budget_ms':'フレーム予算','cycle_window_ms':'年度窓',
 'perf_diagnostics_enabled':'性能診断','cycle_alloc_budget':'年度割当予算',
 'memory_cleanup_enabled':'メモリ整理','memory_cleanup_force_gc':'強制GC',
 'memory_cleanup_interval_seconds':'整理間隔','memory_cleanup_notify_enabled':'整理通知',
 'nation_play_enabled':'中央銀行家','treasury_income_ratio':'宝物庫税負比','policy_slots':'政策枠数',
}
T(cfg_ja)

# --- German ---
DE = collections.OrderedDict()
def D(d):
    DE.update(d)

tiers_de = {
 'law_education_lv0':'Keine','law_education_lv1':'Grundschule','law_education_lv2':'Mittelstufe','law_education_lv3':'Höhere','law_education_lv4':'Für alle',
 'law_healthcare_lv0':'Keine','law_healthcare_lv1':'Grundversorgung','law_healthcare_lv2':'Standard','law_healthcare_lv3':'Umfassend','law_healthcare_lv4':'Kostenlos',
 'law_migrant_lv0':'Geschlossen','law_migrant_lv1':'Eingeschränkt','law_migrant_lv2':'Offen','law_migrant_lv3':'Willkommen','law_migrant_lv4':'Unbegrenzt',
 'law_religion_lv0':'Unterdrückt','law_religion_lv1':'Toleriert','law_religion_lv2':'Geschützt','law_religion_lv3':'Staatsreligion','law_religion_lv4':'Theokratie',
 'law_press_lv0':'Frei','law_press_lv1':'Standard','law_press_lv2':'Zensiert','law_press_lv3':'Kontrolliert','law_press_lv4':'Monopol',
 'law_gun_control_lv0':'Unbegrenzt','law_gun_control_lv1':'Locker','law_gun_control_lv2':'Lizenz','law_gun_control_lv3':'Verboten','law_gun_control_lv4':'Totalverbot',
 'law_conscription_lv0':'Freiwillig','law_conscription_lv1':'Teilweise','law_conscription_lv2':'Standard','law_conscription_lv3':'Zwang','law_conscription_lv4':'Totale',
 'law_standing_army_lv0':'Keine','law_standing_army_lv1':'Klein','law_standing_army_lv2':'Standard','law_standing_army_lv3':'Groß','law_standing_army_lv4':'Riesig',
 'law_militarism_lv0':'Friedlich','law_militarism_lv1':'Bereit','law_militarism_lv2':'Aufrüstung','law_militarism_lv3':'Militaristisch','law_militarism_lv4':'Totale',
 'law_pacifism_lv0':'Kriegerisch','law_pacifism_lv1':'Neutral','law_pacifism_lv2':'Pazifistisch','law_pacifism_lv3':'Entmilitarisiert','law_pacifism_lv4':'Total',
 'law_judicial_lv0':'Königswille','law_judicial_lv1':'Gewohnheit','law_judicial_lv2':'Geschrieben','law_judicial_lv3':'Unabhängig','law_judicial_lv4':'Oberste',
 'law_capital_pun_lv0':'Abgeschafft','law_capital_pun_lv1':'Nur Todesstrafe','law_capital_pun_lv2':'Schwere Verbrechen','law_capital_pun_lv3':'Weit verbreitet','law_capital_pun_lv4':'Missbrauch',
 'law_ant_corrupt_lv0':'Ignoriert','law_ant_corrupt_lv1':'Untersucht','law_ant_corrupt_lv2':'Verfolgt','law_ant_corrupt_lv3':'Streng','law_ant_corrupt_lv4':'Nulltoleranz',
 'law_prison_lv0':'Streng','law_prison_lv1':'Standard','law_prison_lv2':'Rehabilitation','law_prison_lv3':'Bildung','law_prison_lv4':'Freilassung',
 'law_forest_lv0':'Unbegrenzt','law_forest_lv1':'Verwaltet','law_forest_lv2':'Geschützt','law_forest_lv3':'Streng','law_forest_lv4':'Heilig',
 'law_animal_lv0':'Gleichgültig','law_animal_lv1':'Interessiert','law_animal_lv2':'Geschützt','law_animal_lv3':'Streng','law_animal_lv4':'Heilige Tiere',
 'law_pollution_lv0':'Ignoriert','law_pollution_lv1':'Interessiert','law_pollution_lv2':'Reguliert','law_pollution_lv3':'Streng','law_pollution_lv4':'Vollständig',
 'law_monarchy_lv0':'Kein König','law_monarchy_lv1':'Wahlokönig','law_monarchy_lv2':'Erbkönig','law_monarchy_lv3':'Absolut','law_monarchy_lv4':'Göttlich',
 'law_parliament_lv0':'Keine','law_parliament_lv1':'Beratend','law_parliament_lv2':'Gesetzgebend','law_parliament_lv3':'Führend','law_parliament_lv4':'Herrschend',
 'law_planned_economy_lv0':'Frei','law_planned_economy_lv1':'Gelenkt','law_planned_economy_lv2':'Kontrolliert','law_planned_economy_lv3':'Geplant','law_planned_economy_lv4':'Vollständig',
 'law_free_market_lv0':'Kontrolliert','law_free_market_lv1':'Dereguliert','law_free_market_lv2':'Frei','law_free_market_lv3':'Vollkommen frei','law_free_market_lv4':'Anarchisch',
 'law_state_religion_lv0':'Keine','law_state_religion_lv1':'Bevorzugt','law_state_religion_lv2':'Anerkannt','law_state_religion_lv3':'Staatsreligion','law_state_religion_lv4':'Zwang',
 'law_secularism_lv0':'Staatsreligion','law_secularism_lv1':'Getrennt','law_secularism_lv2':'Neutral','law_secularism_lv3':'Säkular','law_secularism_lv4':'Areligiös',
 'law_land_reform_lv0':'Keine','law_land_reform_lv1':'Schrittweise','law_land_reform_lv2':'Umfassend','law_land_reform_lv3':'Zwangsenteignung','law_land_reform_lv4':'Vollständige Umverteilung',
 'law_antimonopoly_lv0':'Ignoriert','law_antimonopoly_lv1':'Überwacht','law_antimonopoly_lv2':'Reguliert','law_antimonopoly_lv3':'Zerschlagen','law_antimonopoly_lv4':'Verstaatlicht',
 'law_tax_system_lv0':'Steuerfrei','law_tax_system_lv1':'Niedrig','law_tax_system_lv2':'Mittel','law_tax_system_lv3':'Hoch','law_tax_system_lv4':'Extrem',
 'law_property_rights_lv0':'Kein Schutz','law_property_rights_lv1':'Teilweise','law_property_rights_lv2':'Standard','law_property_rights_lv3':'Stark','law_property_rights_lv4':'Absolut',
 'law_trade_freedom_lv0':'Laissez-faire','law_trade_freedom_lv1':'Freier Markt','law_trade_freedom_lv2':'Reguliert','law_trade_freedom_lv3':'Strikt','law_trade_freedom_lv4':'Kommando',
}
D(tiers_de)

D({'law_cat_environment':'Umweltgesetze','law_cat_ideology':'Ideologiegesetze','law_cat_social':'Sozialgesetze',
   'law_mutex_tip':'Gegenseitig ausschließend','law_eff_gini':'Gini','law_eff_happy':'Zufriedenheit'})

build_de = {'build_native_house_t1':'Einfaches Haus','build_native_house_t3':'Standardhaus','build_native_house_t5':'Villa',
 'build_native_barracks':'Kaserne','build_native_watchtower':'Wachturm','build_native_well':'Brunnen',
 'build_native_mine':'Mine','build_native_statue':'Statue','build_native_temple':'Tempel',
 'build_native_bonfire':'Lagerfeuer','cabinet_build_native':'Vanille-Gebäude platzieren (Karte klicken)',
 'cabinet_city_built':'Bereits gebaut','cabinet_place_mode':'Platzierungsmodus','cabinet_place_hint':'Linksklick platzieren · Rechtsklick abbrechen',
 'cabinet_policies':'Laufende Politiken','cabinet_policy_hint':'Politiken belegen Slots; Jahr 1 kostet, Wechsel kostet Differenz',
 'cabinet_decrees':'Einmal-Dekrete','cabinet_decree_hint':'Dekrete sind sofort wirksam; keine Wiederholung während Abklingzeit',
 'cabinet_decree_cost_festival':'Kosten: Bev.×0.5 | Abklingzeit 3 J','cabinet_decree_cost_relief':'Kosten: Bev.×Ø×2% | Abklingzeit 5 J',
 'cabinet_records':'Leistungsprotokoll','cabinet_record_open':'Öffnen','cabinet_record_row':'Jahr {0} {1} ({2}G)',
 'cabinet_no_records':'Keine Einträge','cabinet_more_cities':'{0} weitere Städte','cabinet_no_cities':'Keine Städte',
 'cabinet_diplomacy':'Diplomatie (Wirtschafts- & Außenminister)','cabinet_diplomacy_hint':'Geschenke sammeln Wohlwollen für Allianzen',
 'cabinet_gdp_chart':'GDP-Trend','cabinet_gdp_chart_now':'Aktuell: {0}','cabinet_gdp_chart_short':'Letzte {0}',
 'cabinet_law_effect_none':'Keine Abweichung',
 'col_gdp':'GDP','col_kingdom':'Königreich','col_price':'Preis',
 'chart_title':'GDP-Trend','chart_year':'Jahr {0}','chart_global':'Global','chart_dropped':'(weggelassen)',
 'chart_macro':'GDP {0} · Ø {1} · Gini {2}','chart_macro_avg':'Durchschnitt: {0}',
 'gini_chart_gini':'Gini','gini_chart_phase':'Phase',
 'cycle_phase':'Phase','cycle_phase_boom':'Boom','cycle_phase_recession':'Rezession',
 'cycle_phase_depression':'Depression','cycle_phase_recovery':'Erholung','cycle_detail':'Zyklus {0} · Wachstum {1} · Blase {2}',
 'economy_general':'Wirtschaft','economy_title':'Klassische Ökonomie',
 'event_family_finance':'Finanzen','event_family_disaster':'Katastrophen','event_family_court':'Hof',
 'event_family_military':'Militär','event_family_civil':'Zivil','event_family_diplomacy':'Diplomatie',
 'kingdom_law_header':'Kodex','kingdom_law_counts':'{0} Gesetze · {1} Politiken',
 'hud_mem_cleanup':'Letzte Bereinigung {0} | freigegeben {1} ({2} Puffer)',
 'hud_mem_cleanup_pending':'Speicherbereinigung: warte auf Freigabe',
 'hud_mem_usage':'Managed Heap {0} | Unity genutzt {1} (reserviert {2})','hud_mem_na':'—',
 'toast_nation_claim':'[Übernahme] <{0}> regiert (Schatzkammer {1})',
 'toast_nation_unbind':'Königreich gefallen — Herrschaft beendet',
 'toast_nation_policy_on':'[Politik] {0} aktiviert (Stufe {1})',
 'toast_nation_policy_off':'[Politik] {0} beendet',
 'toast_nation_policy_suspended':'{0} wegen leerer Schatzkammer ausgesetzt',
 'toast_nation_no_slot':'Politik-Slots voll (max. {0})',
 'toast_nation_poor_treasury':'Schatzkammer unzureichend',
 'toast_nation_cooldown':'Abklingzeit aktiv',
 'toast_nation_relief':'[Dekret] Nothilfe für {0}',
 'toast_nation_festival':'[Dekret] Nationalfest abgehalten',
 'toast_nation_built':'<{0}> Bau fertig (-{1}G)',
 'toast_nation_built_already':'Bereits gebaut',
 'toast_nation_build_failed':'Platzierung fehlgeschlagen',
 'toast_nation_place_mode':'Platzierung: {0}',
 'toast_nation_place_cancelled':'Beendet',
 'toast_nation_place_land':'Nicht auf Wasser',
 'toast_nation_place_territory':'Nur eigenes Gebiet',
 'toast_dip_war':'[Diplomatie] Krieg gegen <{0}>',
 'toast_dip_peace':'[Diplomatie] Frieden mit <{0}> (+{1}G)',
 'toast_dip_alliance':'[Diplomatie] Bündnis mit <{0}>',
 'toast_dip_gift':'[Diplomatie] Geschenk an <{0}>',
 'toast_dip_pact_ok':'[Diplomatie] Pakt mit <{0}>',
 'toast_dip_pact_cancel':'[Diplomatie] Pakt beendet',
 'toast_unrest_peace':'Unruhe durch Verhandlung beendet',
 'toast_unrest_resolved':'Unruhe unterdrückt; Stadt zurückerobert',
 'ev_era_combined':'Zeitalter-Ereignisse',
 'ruler_skim':'Hofhaltungskosten','nation_commerce_tax':'Handelssteuer',
}
D(build_de)

cfg_de = {
 'unrest_enabled':'Unruhe-System','log_worldlog':'Weltprotokoll','gini_threshold':'Gini-Schwelle',
 'unrest_grace_years':'Kulanzjahre','unrest_max_cities':'Max Städte',
 'policy_enabled':'Nationale Politik','cycle_enabled':'Wirtschaftszyklus',
 'cycle_gini_high':'Hoher Gini','cycle_gini_low':'Niedriger Gini','cycle_gini_periods':'Zyklusperioden',
 'boom_stimulus_ratio':'Boom-Stimulus','boom_bubble_factor':'Blasen-Faktor','bubble_threshold':'Blasen-Schwelle',
 'boom_max_duration':'Max Boom','recession_max_duration':'Max Rezession',
 'depression_max_duration':'Max Depression','recovery_max_duration':'Max Erholung',
 'survival_line':'Überlebenslinie','war_plunder_ratio':'Kriegsplünderung','war_waste_ratio':'Kriegsverschleiß',
 'revolution_delay_years':'Revolution Verzögerung','revolution_kill_ratio':'Revolution Hinrichtung',
 'uprising_gini_threshold':'Aufstand Gini','uprising_delay_years':'Aufstand Verzögerung',
 'kill_rich_ratio':'Reichen-Hinrichtung','kill_rich_redist_ratio':'Reichen-Umverteilung',
 'wealth_tax_enabled':'Vermögensteuer','wealth_tax_ratio':'Vermögensteuersatz','wealth_tax_line':'Vermögenssteuerlinie',
 'population_enabled':'Populationsgrenze','population_overcrowd':'Überfüllung',
 'era_enabled':'Zeitalter-Ereignisse','era_duration_years':'Zeitalter-Dauer','collapse_drop_ratio':'Kollaps-Rückgang',
 'collapse_duration_years':'Kollaps-Dauer','flourish_military_ratio':'Blüte Militärquote','flourish_periods':'Blüteperioden',
 'labor_enabled':'Arbeitsteilung','labor_wage_base':'Grundlohn',
 'real_time_refresh':'Echtzeit-Aktualisierung','real_time_interval':'Aktualisierungsintervall',
 'real_time_refresh_threshold':'Aktualisierungsschwelle','real_time_refresh_budget':'Aktualisierungsbudget',
 'money_velocity':'Geldumlaufgeschwindigkeit','inflation_bubble_boost':'Inflationsblasen-Boost',
 'disaster_enabled':'Katastrophenschock','disaster_wealth_loss':'Katastrophenverlust','disaster_mine_bonus':'Vulkanbonus',
 'banking_enabled':'Bankkredite','credit_rate':'Kreditzins',
 'default_rate_depression':'Depressions-Default','crisis_contagion_threshold':'Ansteckungsschwelle',
 'spending_cap_per_year':'Jahresausgabendeckel','banking_default_cap_per_year':'Bank-Default-Deckel',
 'banking_contagion_cap_per_year':'Ansteckungs-Deckel','inheritance_scan_per_frame':'Erb-Scan-Limit',
 'frame_budget_ms':'Frame-Budget ms','cycle_window_ms':'Zyklusfenster ms',
 'perf_diagnostics_enabled':'Performance-Diagnose','cycle_alloc_budget':'Jahres-Zuweisung',
 'memory_cleanup_enabled':'Speicherbereinigung','memory_cleanup_force_gc':'Erzwungenes GC',
 'memory_cleanup_interval_seconds':'Bereinigungsintervall','memory_cleanup_notify_enabled':'Bereinigungsmeldung',
 'nation_play_enabled':'Zentralbankier','treasury_income_ratio':'Schatzkammer-Quote','policy_slots':'Politik-Slots',
}
D(cfg_de)

# 書き込み
en = json.load(io.open('Locales/en.json', encoding='utf-8'))
path = 'Locales/ja.json'
existing = json.load(io.open(path, encoding='utf-8'), object_pairs_hook=collections.OrderedDict)
merged = 0
for k, v in JA.items():
    if existing.get(k) == en.get(k):
        existing[k] = v; merged += 1
json.dump(existing, io.open(path, 'w', encoding='utf-8'), ensure_ascii=False, indent=2)
io.open(path, 'a', encoding='utf-8').write('\n')
print('JA batch A+B merged:', merged, 'total:', len(existing))

path = 'Locales/de.json'
existing = json.load(io.open(path, encoding='utf-8'), object_pairs_hook=collections.OrderedDict)
merged = 0
for k, v in DE.items():
    if existing.get(k) == en.get(k):
        existing[k] = v; merged += 1
json.dump(existing, io.open(path, 'w', encoding='utf-8'), ensure_ascii=False, indent=2)
io.open(path, 'a', encoding='utf-8').write('\n')
print('DE batch A+B merged:', merged, 'total:', len(existing))

# 残り確認
for lang in ('ja', 'de'):
    d = json.load(io.open('Locales/%s.json' % lang, encoding='utf-8'))
    remaining = [k for k in en if d.get(k) == en.get(k)]
    t = sum(1 for k in en if d.get(k) != en.get(k))
    print(lang, 'translated:', t, '/', len(en), '(', round(t/len(en)*100), '%) remaining:', len(remaining))
    if remaining:
        # イベント関連の残りを表示
        ev_miss = [k for k in remaining if k.startswith('ev_')]
        other_miss = [k for k in remaining if not k.startswith('ev_')]
        print('  events:', len(ev_miss), 'other:', len(other_miss))
