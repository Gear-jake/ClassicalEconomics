# -*- coding: utf-8 -*-
"""v1.6.0 ja残りイベント35件+残り非イベント。冪等。"""
import io, json, collections

path = 'Locales/ja.json'
existing = json.load(io.open(path, encoding='utf-8'), object_pairs_hook=collections.OrderedDict)
en = json.load(io.open('Locales/en.json', encoding='utf-8'))

JA = collections.OrderedDict()
def ev(eid, n, title, desc, *opts):
    JA['ev_' + eid] = title; JA['ev_' + eid + '_desc'] = desc
    for i in range(n):
        a, b, c = opts[i]
        JA['ev_%s_opt%d' % (eid, i+1)] = a
        JA['ev_%s_opt%d_desc' % (eid, i+1)] = b
        JA['ev_%s_res%d' % (eid, i+1)] = c
def q(a, b, c): return (a, b, c)
def t(d): JA.update(d)

# --- 残り天災 ---
ev('flood', 2, '洪水', '豪雨が続き河川が堤防を越え低地の村が水没した。',
    q('堤防を修復', '人手を動員し昼夜で修復し下流を守る', '堤防が完成し洪水が引き{kingdom}は家を守った。'),
    q('放置', '災民が四散し不満が各都市に広がる', '洪水が引いた後廃墟と不満だけが残った。'))
ev('wildfire', 2, '山火事', '乾燥した風が山火事を村落と林に向けて押し進めている。',
    q('防火帯を切る', '木こりに金を払い防火帯を切り林を守る', '防火帯が炎を止め林は無事だった。'),
    q('燃えるままに', '一錢も使わず火が自ずと消えるのを待つ', '火は灰の野原を残し村人は泣いた。'))
ev('blizzard', 2, '吹雪', '吹雪が家畜を凍らせ糧道を塞いだ。',
    q('倉を賑糧', '各家に糧を貸し冬を越させる', '冬を越され春に糧が返済された。'),
    q('閉居', '倉を節約し凍死者を数える', '雪が止んだ後どの村でも白事があった。'))
ev('mine_collapse', 2, '鉱山崩落', '深夜に鉱山が崩落し鉱夫が閉じ込められた。家族が坑口に集まっている。',
    q('全力救出', '費用を惜しまず掘り出し遺族を慰める', '鉱夫の大半が救出され{kingdom}は義と称えられた。'),
    q('坑口を封じる', '閉じ込めた者を諦め鉱脈を守る', '坑口封鎖の日泣き声が三日絶えなかった。'))
ev('river_dry', 2, '河川の涸渇', '主要河川が断流し水車が止まり両岸の田がひび割れている。',
    q('水路開削', '人夫で上流から水路を引く', '水路が完成し田は救われた。'),
    q('乾燥作物', '耐乾性作物に変え収量半減だが費用不要', '収量半減、村人は耐えた。'))
ev('rockslide', 2, '山崩れ', '雨の後の山崩れが交易路の峠を塞いだ。',
    q('石匠を雇う', '金で石匠を雇い峠を早く開通させる', '峠が再開し商隊が称賛した。'),
    q('迂回', '清障に出さず商隊が十日迂回する', '迂回で十日と税収が減った。'))

# --- 残り宮廷 ---
ev('old_regent', 2, '老摂政の請願', '長年輔政した老摂政が引退を請い朝中は息を潜めている。',
    q('厚賜栄帰', '金帛儀仗を賜い体面を全うして送る', '老臣は涙で別れ列国が{king}の恩義を讃えた。'),
    q('権を回収', '機に乗じ大権を回収するが老臣が怨む', '権柄は王室に帰したが怨者が朝に増えた。'))
ev('bastard_claim', 2, '庶子の認知', 'ある青年が古い証で{king}の血筋を称し宗籍への加入を要求している。',
    q('検して認める', '身元を検分し認知する——名分と非難が一緒', '認知が成り市井が噂で持ちきりになった。'),
    q('詐欺として追放', '偽造として糾弾し追い出す', '追放され去り際に冤罪を叫んだ。'))
ev('spy_ring', 2, '密探網', '内廷が密探網の設置を提言：使節と重臣の監視。',
    q('資金を承認', '資金で耳を四方に置き{king}が真っ先に知る', '耳が四方に置かれ{king}が真っ先に知る。'),
    q('不徳として拒否', '王室は白昼行動すべきだと拒否', '提案は拒否され{king}は一線を越えなかった。'))

# --- 残り軍事 ---
ev('war_defector', 3, '敵将の来降', '敵国の宿将が陣前に降り秘密を差し出そうとしている——心思は不明。',
    q('縛して見せしめ', '陣前で斬り敵胆を寒くする', '陣前の処刑に敵軍が震えた。'),
    q('才を重用', '礼で帐下に迎え才能を使う——心は未検', '{king}が帐前で降将を迎えた。'),
    q('礼送', '降将を納めず礼で送り信義を全うする', '礼送され両軍が{king}の古風を称えた。'))
ev('defector_ambition', 2, '降将の野心', '降将が兵を擁して号令に従わなくなり禍根が芽を出した。',
    q('先制兵権回収', '代価を惜しまず兵権を回収し後患を絶つ', '兵権が回収され禍根は除かれた。'),
    q('忍耐', '強攻は内乱を恐れ暫く耐える', '忍耐が安穏を得たが誰もが亀裂を見た。'))
ev('supply_convoy', 2, '糧道の襲撃', '前線の糧道が襲われ護送官が增援を懇願している。',
    q('護衛增派', '城中から人手を出し糧道を守る', '糧道が再開し前線の軍心が定まった。'),
    q('增派不可', '断り護送官に自力を祈る', '糧道が再襲され前線は三日の軍糧を失った。'))
ev('fort_rebuild', 2, '旧堡の再建', '国境の要塞が崩れかけ守将が修復を請願している。',
    q('专款再建', '重金で稜堡を再建し辺民は安眠する', '要塞は新しくなり辺民は安眠した。'),
    q('工事を猶予', '辺務を緩め金を別に使う', '要塞は崩れたまま辺民は夜に風音を聞いた。'))
ev('veteran_company', 2, '老兵の請願', '退役老兵が常時辺境を守る老兵連の結成を請願している。軍餉さえもらえれば。',
    q('老兵連を許可', '軍餉を支給し老兵に国境を守らせる', '老兵連が国境を守り盗匪は絶えた。'),
    q('丁重に辞退', '国庫が緊迫、老兵を田里に帰す', '老兵は田里に散り酒場に昔の勇話が増えた。'))
ev('privateer_licence', 2, '私掠許可', '海賊頭目が私掠許可と引き換えに献金を申し出ている。',
    q('許可を発給', '金を取り証を発給すると海に編成狼が増える', '私掠船が進出し庫金と悪名が帳入された。'),
    q('焚船驅盜', '断固拒否し模範のため船を焼く', '海賊船は松明となり海路が清浄になった。'))
ev('hero_funeral', 2, '英雄の葬儀', '辺境を守った老将が逝去し全国が哀悼し葬儀規格が未定だ。',
    q('国葬規格', '国葬で送り費用は王室全額', '万民が街路で送別し列国の使節も参列した。'),
    q('簡素下葬', '遺願通り簡素に葬る', '老将は無名の墓に眠ったが心には名がある。'))

# --- 残り民生 ---
ev('market_fire', 2, '市場の火災', '深夜に市場が出火半分が焼け野原になり商人は資本を失った。',
    q('王室助成', '王庫金で商人の再建を助ける', '新市場が開店し从前より賑やかになった。'),
    q('自助に任せる', '市集は自建し王室は一钱も出さない', '市集は粗末に再開し商人は税吏に嘆息した。'))
ev('water_shortage', 2, '水不足', '夏の少雨で井戸が涸れ城中で日日長い列ができている。',
    q('深井戸開削', '重金で工匠に深井戸と水路を依頼', '清泉が城に入り水不足が解消した。'),
    q('給水制限', '戸別に制限し工数省くが不満が四起', '水が配給され列は減ったが不満が増えた。'))
ev('tenant_strike', 2, '小作の抗租', '田租の連続値上げに小作人が結束して拒否し田荘が王庭に訴えた。',
    q('減租を勧める', '王室が地主に出面し減租を勧めて事を収める', '租約が改訂され小作人が歓声を上げた。'),
    q('強制催缴', '役員を派遣し強制的に取り立てる王法が先', '租銀は全額入荘したが田埂の眼差しは冷えた。'))
ev('festival_request', 2, '庆典の請願', '百姓が連名で請願：豊年が到来したので{king}に全城庆典を許可してほしい。',
    q('庆典を钦定', '王室が出資し庆典を開き民と楽しむ', '三日間の祝祭で全城が熱狂し皆{king}の徳を称えた。'),
    q('豊年でも見送る', '良い日は過ごし金を悪年景に残す', '請願は婉拒され街は{king}を吝嗇と噂した。'))
ev('night_patrol', 2, '夜盗の横行', '夜の盗賊が横行し商家が連続で盗まれ商会が夜巡を請求している。',
    q('夜巡隊を組織', '金で夜巡隊を養い商家に安寧な夜を返す', '夜巡が配置され盗賊が絶え商家が安心した。'),
    q('商家自保', '各商家が自ら護院を雇い王室は出さない', '護院が各々で戦い盗賊は弱点を狙った。'))
ev('bathhouse_fad', 2, '風呂屋の流行', '城中で風呂屋が突然流行し行会が王室の擡頭を請求している。',
    q('新澡堂を倡建', '王室が拡建を倡導し衛生の恩恵が全城に及ぶ', '澡堂が立ち並び{kingdom}は清潔で聞こえた。'),
    q('流行は去る', '王は民と風呂屋を争わない流行は自ずと去る', '流行は去り数軒の澡堂だけが苦撐した。'))
ev('traveling_fair', 2, '巡遊市', '巡遊市が大挙して城に入り王室の開市と課税を請求している。',
    q('開市を許可', '開市を許可し王室が割合で課税', '半月の市で税金と歓声が一緒に庫に入った。'),
    q('入城を辞退', '市集の秩序を恐れ入城を丁重に辞退する', '市は隣国に行き{kingdom}の街が少し寂しくなった。'))

# --- 残り外交 ---
ev('royal_visit', 2, '王室の来訪', '隣国王室が来訪を表明した——排出負けはできず礼儀はなお更だ。',
    q('全力で接待', '国礼で全力接待し賓主が楽しむ', '賓主が楽しみ列国に美話として伝わった。'),
    q('定例で接待', '常例通り失礼なく規矩を超えない', '礼は整ったが驚きはなく貴賓の印象は平淡だった。'))
ev('hostage_request', 2, '質子の要請', '強盟が書簡を送り王室の子弟を質に送り両国の好を証明するよう求めている。',
    q('忍んで質子を送る', '骨肉の分離で強盟を安心させる宮中は泣き声ばかり', '質子が出発し盟約は堅くなったが宮中の痛みは自知のみ。'),
    q('丁重に拒否', '幼子が年弱を理由に丁重に拒否し盟友の顔が曇る', '拒絶の辞が伝わり盟友に間隙が生まれた。'))
ev('border_treaty', 2, '境界の盟約', '隣国が境界条約の締結を提案し界碑を定め永く辺釁を息にしたい。',
    q('条約に調印', '争議地を譲り百年の辺安と引換える', '界碑が立定し両国の辺民は械闘しなくなった。'),
    q('寸土も让らず', '祖産は寸土も让らず辺軍が警戒を倍加', '交渉が破裂し辺境の哨所が互いに警戒した。'))
ev('pirate_bribe', 2, '海賊の通行料', '海賊が分账清单を送ってきた：金を払えば{kingdom}の商船は安全と保証する。',
    q('買路金を払う', '破財免災で商船に安全旗を掲げさせる', '買路錢が出ると{kingdom}商船は阻害なく通行した。'),
    q('賞金で討伐', '海賊に賞金を懸けて討伐し以牙還牙', '海賊の数股が剿滅され残りは恨んだ。'))
ev('pilgrim_wave', 2, '巡礼の波', '聖地の顕霊の噂が四起し大群の巡礼者が{kingdom}の国境に押し寄せている。',
    q('棚を設けて接待', '沿道に粥棚駅舎を設け過境の巡礼者を手厚く遇する', '巡礼者が恩を感じ{kingdom}善名が遠くに広がった。'),
    q('閉関して追い出す', '流民の動乱を恐れ関を閉じ巡礼の队伍を追い出す', '巡礼者は界の外に阻まれ聖地の名声は他国に帰した。'))
ev('tribute_envoy', 2, '貢使团', '遠方の小国が使を遣わし称臣納貢、{kingdom}の庇護のみを求めている。',
    q('貢を受ける', '貢品を受け入れ庇護の名も引き受ける', '貢品が庫に入り庇護の責も{kingdom}の頭に記された。'),
    q('貢礼を謙辞', '小国の物を貪らず平等の礼で遇する', '貢礼が謙辞され小国はかえって{kingdom}を尊敬した。'))

# --- 商業/銀行 ---
ev('caravan_ambush', 2, '商隊の襲撃', '{king}の商隊が国境で襲われた。交易路は恐怖に包まれている。',
    q('出金護商', '護衛を雇い沿路护送し商路を立稳', '護衛が出動し商隊が復し市場が安心した。'),
    q('自求多福', '担保せず商路が二年断絶する', '商路が断たれ市面が沈滞し不満が{kingdom}に向かった。'))
ev('trade_route_cut', 2, '商路の遮断', '商路が半年遮断され商店が品薄で税金が鋭減している。{king}は疏通するか耐えるか。',
    q('重金疏通', '沿途の勢力に働きかけ商路を再開させる', '商路が再開し商業税が回復した。'),
    q('陸路で耐える', '山陸を迂回し時間と費用がかかるが頭は下げない', '陸路が辛うじて維持し商業は零を免れた。'))
ev('guild_petition2', 2, '行会の請願', '手工業行会が貸付銀行の設立と王室の出資配当を求めている。',
    q('出資する', '出資して配当を受け取る——行会と一つの船に乗る', '銀行が開業し王室が毎年配当を受け取った。'),
    q('丁重に辞退', '王室の名を錢荘の商いに出さない', '出資を辞退し貧民救済に転じた。'))
ev('bank_run', 2, '取付騒ぎ', 'デフォルトが相次ぎ預金者が銀行に押し寄せている。準備金が枯渇し恐怖が広がる。今すぐ決断を。',
    q('王室注資', '王室の金庫から準備金を補填し信頼を買い戻す', '注資がパニックを鎮め市場が息をついた。'),
    q('倒産に任せる', '銀行は救わず国庫を守る——民の怒りが雪球のように滾る', '銀行倒産潮が爆发し街は怒れる預金者で満ちた。'))
ev('bank_run_aftermath', 2, '騒ぎの余波', '騒ぎは収まったが市場の信頼は砕け散った。{king}はどう人心を整理するか。',
    q('開倉济困', '金を散じて擠兌で破壊された預金者を救済する', '救済を受けた民が信頼を取り戻し市面が徐々に回復した。'),
    q('市場の自癒に任せる', '介入せず時間がすべてを癒す', '市場の回復は遅く{kingdom}の信用の傷はまだ癒えていない。'))

# --- 法典档位（残り）---
for k, v in {
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
 'law_cat_environment':'環境法','law_cat_ideology':'理念法','law_cat_social':'社会法',
 'law_mutex_tip':'相互排他','law_eff_gini':'ジニ','law_eff_happy':'幸福',
}.items():
    t({k: v})

# --- 設定ラベル + UI ---
for k, v in {
 'unrest_enabled':'不安システム','log_worldlog':'ログ出力','gini_threshold':'ジニ閾値',
 'unrest_grace_years':'猶予年数','unrest_max_cities':'最大影響都市',
 'policy_enabled':'国家政策','cycle_enabled':'経済サイクル',
 'cycle_gini_high':'高ジニ','cycle_gini_low':'低ジニ','cycle_gini_periods':'サイクル期間',
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
 'event_chance_player':'プレイヤーイベント率','event_chance_ai':'AIイベント率',
 'event_cooldown_years':'イベント全局冷却',
 'bank_enabled':'銀行と商業','bank_money_supply_factor':'貨幣供給係数',
 'bank_max_loans_per_city':'都市貸加上限','bank_deposit_ratio_default':'預金比率',
 'ruler_personality_enabled':'統治者性格',
 'economy_general':'経済','economy_title':'古典経済学',
 'event_family_finance':'財政','event_family_disaster':'天災','event_family_court':'宮廷',
 'event_family_military':'軍事','event_family_civil':'民生','event_family_diplomacy':'外交',
 'kingdom_law_header':'法典','kingdom_law_counts':'法律 {0} 条・国策 {1} 条',
 'hud_mem_cleanup':'前回整理 {0}｜解放 {1} ({2} バッファ)','hud_mem_cleanup_pending':'メモリ整理：解放待ち',
 'hud_mem_usage':'管理ヒープ {0}｜Unity 使用 {1} (予約 {2})','hud_mem_na':'—',
 'col_gdp':'GDP','col_kingdom':'王国','col_price':'物価',
 'chart_title':'GDPトレンド','chart_year':'第{0}年','chart_global':'全球',
 'chart_macro':'GDP {0}・平均 {1}・ジニ {2}','chart_macro_avg':'平均：{0}',
 'gini_chart_gini':'ジニ','gini_chart_phase':'段階',
 'cycle_phase':'段階','cycle_phase_boom':'好況','cycle_phase_recession':'後退',
 'cycle_phase_depression':'恐慌','cycle_phase_recovery':'回復',
 'cycle_detail':'第{0}期・成長率{1}・バブル{2}',
 'build_native_house_t1':'基本住宅','build_native_house_t3':'標準住宅','build_native_house_t5':'豪華住宅',
 'build_native_barracks':'兵営','build_native_watchtower':'展望塔','build_native_well':'井戸',
 'build_native_mine':'鉱山','build_native_statue':'記念碑','build_native_temple':'神殿',
 'build_native_bonfire':'焚き火','cabinet_build_native':'原生建築設置（地図クリック）',
 'cabinet_city_built':'建設済','cabinet_policies':'持続政策','cabinet_decrees':'一回用法令',
 'cabinet_decree_hint':'法令は即時発効','cabinet_decree_cost_festival':'費用：人口×0.5｜冷却3年',
 'cabinet_decree_cost_relief':'費用：人口×平均×2%｜冷却5年','cabinet_records':'政績記録',
 'cabinet_record_open':'開く','cabinet_record_row':'第{0}年 {1}（{2}G）','cabinet_no_records':'政績記録なし',
 'cabinet_more_cities':'他{0}都市','cabinet_no_cities':'都市なし',
 'cabinet_diplomacy':'外交','cabinet_diplomacy_hint':'贈物は好感を累積する',
 'cabinet_gdp_chart':'GDPトレンド','cabinet_gdp_chart_now':'現在：{0}','cabinet_gdp_chart_short':'近{0}期',
 'cabinet_law_effect_none':'偏差なし',
 'cabinet_dip_back':'戻る','cabinet_dip_actions':'外交アクション',
 'cabinet_dip_war':'宣戦','cabinet_dip_peace':'講和','cabinet_dip_alliance':'同盟',
 'cabinet_dip_gift':'贈物（500G）','cabinet_dip_pact':'経済協定','cabinet_dip_pact_on':'経済協定·第{0}档',
 'cabinet_dip_list_row':'{0}（GDP {1}）関係 {2} 好感 {3}',
 'cabinet_dip_stat_relation':'関係 {0}｜好感 {1}','cabinet_dip_stat_style':'国民性：{0}',
 'cabinet_dip_stat_gdp':'GDP {0}','cabinet_dip_stat_pop':'人口 {0}・平均 {1}',
 'cabinet_dip_stat_laws':'法律 {0} 条・国策 {1} 条',
 'cabinet_dip_stat_war':'交戦中','cabinet_dip_stat_peace':'平和',
 'cabinet_policy_hint':'政策は占拠','cabinet_policy_cost':'年費 少 {0}｜中 {1}｜大 {2}',
 'cabinet_policy_active':'{0}·第{1}档','cabinet_enable':'有効化','cabinet_upgrade':'昇格','cabinet_disable':'停止',
 'cabinet_decree_cooling':'{0}（冷却中）','cabinet_execute':'執行',
 'cabinet_build_title':'建設（地図クリック設置）','cabinet_build_market':'市場','cabinet_build_granary':'穀倉',
 'cabinet_place_mode':'設置モード','cabinet_place_hint':'左クリック設置・右クリック取消',
 'cabinet_city_built':'建設済','cabinet_law_nation':'《{0}》法典','cabinet_law_style':'国民性：{0}',
 'cabinet_law_effect_title':'現在の集計効果','cabinet_law_policy_hdr':'国策',
 'cabinet_law_effect_none':'偏差なし','cabinet_no_nation':'国未選択',
 'cabinet_claim_hint':'選択で宝物庫資金入金','cabinet_claim_row':'{0}（GDP {1}）',
 'cabinet_treasury':'宝物庫：{0}','cabinet_flow':'前年収入 {0}｜支出 {1}',
 'cabinet_switch_cooldown':'国替えクールダウン：あと {0} 年',
 'cabinet_disabled':'中央銀行家プレイ無効','cabinet_no_nation':'国未選択：リストから選択',
 'cabinet_claim_hint':'選択で資金入金',
 'toast_nation_claim':'[認領] <{0}> 統治開始（宝物庫 {1}）',
 'toast_nation_unbind':'国が滅び統治解除',
 'toast_nation_policy_on':'[政策] {0} 有効化（第{1}档）',
 'toast_nation_policy_off':'[政策] {0} 停止',
 'toast_nation_policy_suspended':'庫不足で {0} 一時停止',
 'toast_nation_no_slot':'政策枠満杯（最大{0}）',
 'toast_nation_poor_treasury':'宝物庫不足',
 'toast_nation_cooldown':'クールダウン中',
 'toast_nation_relief':'[法令] 緊急救済：{0}',
 'toast_nation_festival':'[法令] 国慶祭執行',
 'toast_nation_built':'<{0}> 建設完成（-{1}G）',
 'toast_nation_built_already':'建設済',
 'toast_nation_build_failed':'設置失敗',
 'toast_nation_place_mode':'設置モード：{0}',
 'toast_nation_place_cancelled':'終了',
 'toast_nation_place_land':'海上不可',
 'toast_nation_place_territory':'本国領内のみ',
 'toast_dip_war':'[外交] <{0}> 宣戦',
 'toast_dip_peace':'[外交] <{0}> 講和（+{1}G）',
 'toast_dip_alliance':'[外交] <{0}> 同盟',
 'toast_dip_gift':'[外交] <{0}> 贈物',
 'toast_dip_pact_ok':'[外交] <{0}> 経済協定',
 'toast_dip_pact_cancel':'[外交] 協定解除',
 'toast_unrest_peace':'暴動和平解決',
 'toast_unrest_resolved':'暴動鎮圧都市回復',
 'ev_era_combined':'時代イベント',
 'ev_desc_unrest':'貧富差が閾を超え不満が爆发（{0}）',
 'ev_desc_incite':'扇動により暴動発生（{0}）',
 'ev_desc_suppress':'鎮圧执行',
 'ev_desc_plunder':'戦争略奪 +{0}G',
 'ev_desc_revolution':'革命勃発（{0}）',
 'ev_desc_uprising':'街头起义（{0}）',
 'ev_desc_build_inv':'{0} 建設投資',
 'ev_desc_craft_arsenal':'{0} 軍械 {1} 点鍛造',
 'ev_desc_wholesale':'{0} 武器 {1} 点卸売',
 'ev_desc_era_golden':'{0} 盛世到来',
 'ev_desc_era_revival':'{0} 復興を迎える',
 'ev_desc_era_flourish':'{0} 強盛期に入る',
 'ev_desc_collapse':'{0} 経済崩壊',
 'ev_desc_policy':'国家政策調整',
 'ev_desc_unrest_peace':'暴動和平解決',
 'ev_desc_unrest_resolved':'暴動鎮圧都市回復',
 'ev_desc_policy_fail_abdicate':'改革失敗国王退位',
 'ev_desc_policy_fail_death':'改革失敗国王崩御',
 'ev_desc_policy_fail_civilwar':'改革失敗内戦爆发',
 'ev_desc_policy_fail_fiscal':'財政改革失敗',
 'ev_desc_king_inherit':'新王即位',
 'ev_desc_disaster':'災害経済衝撃（損失 {0}）',
 'ev_desc_banking':'銀行貸付デフォルト（規模 {0}）',
 'ev_desc_bubble_burst':'経済バブル破裂（{0}）',
 'ev_desc_nation_claim':'{0} 統治開始',
 'ev_desc_nation_policy':'{0} 政策調整',
 'ev_desc_nation_relief':'{0} 緊急救済（{1}G）',
 'ev_desc_nation_festival':'{0} 国慶祭開催',
 'ev_desc_nation_build':'{0} 建設',
 'ev_desc_nation_diplomacy':'{0} 外交アクション（{1}）',
 'ev_desc_law_reform':'{0} 変法',
 'ev_desc_law_reform_major':'{0} 重大変法',
 'ev_desc_decision':'王国が選択を下した（選択肢 {0}）。',
 'ev_era_combined':'時代イベント',
 'ev_wholesale':'武器卸売',
 'ruler_skim':'王室経費','nation_commerce_tax':'商業税',
}.items():
    t({k: v})

# マージ
merged = 0
for k, v in JA.items():
    if existing.get(k) == en.get(k):
        existing[k] = v; merged += 1
json.dump(existing, io.open(path, 'w', encoding='utf-8'), ensure_ascii=False, indent=2)
io.open(path, 'a', encoding='utf-8').write('\n')

t2 = sum(1 for k in en if existing.get(k) != en.get(k))
print('JA translated:', t2, '/', len(en), '(', round(t2/len(en)*100), '%)')
miss = [k for k in en if existing.get(k) == en.get(k)]
print('remaining:', len(miss))
for k in miss[:10]: print(' ', k)
