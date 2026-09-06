# -*- coding: utf-8 -*-
"""v1.6.0 残り全キー日語翻訳（イベント45+法典/設定/UI等363）。冪等。"""
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
def t(d):
    JA.update(d)

# --- 残り天災 ---
ev('flood', 2, '洪水', '豪雨が続き河川が氾濫、低地の村が水没した。',
    q('堤防を修復', '人手を動員し昼夜で堤防を修復する', '堤防が完成し洪水が引き、{kingdom}は家を守った。'),
    q('放置', '災民が四散し不満が各都市に広がる', '洪水が引いた後、廃墟と不満だけが残った。'))
ev('wildfire', 2, '山火事', '乾燥した風が山火事を村落と林に向けて押し進めている。',
    q('防火帯を切る', '木こりに金を払い防火帯を切り林を守る', '防火帯が炎を止め林は無事だった。'),
    q('燃えるままに', '一錢も使わず火が自ずと消えるのを待つ', '火は灰の野原を残し村人は泣いた。'))
ev('blizzard', 2, '吹雪', '吹雪が家畜を凍死させ糧道を塞いだ。',
    q('倉を賑糧', '各家に糧を貸し出し冬を越させる', '冬は越され春に糧が返済された。'),
    q('閉じこもる', '倉を節約し凍死者を数える代わりにする', '雪が止んだ後、どの村でも葬式があった。'))
ev('mine_collapse', 2, '鉱山崩落', '深夜に鉱山が崩落し鉱夫が閉じ込められた。家族が坑口に集まっている。',
    q('全力で救出', '費用を惜しまず掘り出し遺族を慰める', '鉱夫の大半が救出され{kingdom}は義と称えられた。'),
    q('坑口を封じる', '閉じ込められた者を諦め鉱脈を守る', '坑口が封じられた日、三日間泣き声が絶えなかった。'))
ev('river_dry', 2, '河川の涸渇', '主要河川が断流し水車が止まり両岸の田がひび割れている。',
    q('水路を開削', '人夫を動員し上流から水路を引く', '水路が完成し両岸の田は救われた。'),
    q('乾燥作物に転換', '耐乾性の作物に変え収量は半減するが費用は不要', '乾燥田の収量は半減したが村人は耐えた。'))
ev('rockslide', 2, '山崩れ', '雨の後の山崩れが交易路の峠を塞いだ。',
    q('石工を雇う', '石匠に金を払い早く峠を開通させる', '峠が再開し通過する商隊が称賛した。'),
    q('迂回させる', '清障に出さず商隊は十日迂回する', '迂回で十日と税収が減った。'))

# --- 残り宮廷 ---
ev('old_regent', 2, '老摂政の請願', '長年輔政した老摂政が引退を請い、朝中は息を潜めている。',
    q('厚く送る', '金帛と儀仗を賜い体面を全うして送る', '老臣は涙で別れ、列国は{king}の恩義を讃えた。'),
    q('権を回収', '機に乗じて大権を回収し老臣の怨望は深い', '権柄は王室に帰したが朝に怨者が増えた。'))
ev('bastard_claim', 2, '庶子の認知', 'ある青年が古い証を持って{king}の血筋を称し宗籍への加入を要求している。',
    q('検して認める', '身元を検分し認知する——名分と共に非難も来る', '認知が成り市井は噂で持ちきりになった。'),
    q('詐欺として追放', '偽造として糾弾し棍棒で追い出す', '青年は追放され去り際に冤罪を叫んだ。'))
ev('spy_ring', 2, '密探網', '内廷が密探網の設置を提言：使節と重臣の監視。',
    q('資金を承認', '資金で耳を四方に置き{king}が真っ先に知る', '耳が四方に置かれ{king}が真っ先に知る。'),
    q('不徳として拒否', '王室は白昼行動すべきだと拒否', '提案は拒否され{king}は一線を越えなかった。'))

# --- 残り軍事 ---
ev('war_defector', 3, '敵将の来降', '敵国の宿将が陣前に降り秘密を差し出そうとしている——用心は分からない。',
    q('縛して見せしめ', '陣前に斬り見せしめとすれば敵胆は寒くなる', '陣前の処刑に敵軍は震えた。'),
    q('才を重用', '礼を尽くし帐下に迎える——才は使えるが心は測れない', '降将を迎え{king}が自ら帐前に出た。'),
    q('礼送する', '降将を納めず礼を持って送り信義を全うする', '礼送され両軍が{king}の古風を称えた。'))
ev('defector_ambition', 2, '降将の野心', '降将が兵を擁して号令に従わなくなり、過去の禍根が芽を出した。',
    q('先制して兵権を奪う', '代価を惜しまず兵権を回収し後患を絶つ', '兵権が回収され禍根は除かれた。'),
    q('忍耐する', '強攻は内乱を恐れ暫く耐える', '忍耐が一時の安穏を得たが誰もが亀裂を見た。'))
ev('supply_convoy', 2, '糧道の襲撃', '前線の糧道が襲われ護送官が增援を懇願している。',
    q('護衛を增派', '城中から人手を出し糧道を守る', '糧道が再開し前線の軍心が定まった。'),
    q('增派できない', '断って護送官に自力を祈る', '糧道が再襲され前線は三日の軍糧を失った。'))
ev('fort_rebuild', 2, '旧堡の再建', '国境の古い要塞が崩れかけ守将が修復を請願している。',
    q('专款で再建', '重金で稜堡を再建し辺民は安らかに眠る', '要塞は新しくなり辺民は安眠した。',
    q('工事を猶予', '辺務を緩め金を別に使う', '要塞は相変わらず崩れ辺民は夜に風の音を聞いた。')
ev('veteran_company', 2, '老兵の請願', '退役老兵が常時辺境を守る老兵連の結成を請願している。軍餉さえもらえれば。',
    q('老兵連を許可', '軍餉を支給し老兵に国境を守らせる', '老兵連が国境を守り盗匪は絶えた。'),
    q('丁重に辞退', '国庫が緊迫しているので老兵を田里に帰す', '老兵は田里に散り酒場に昔の勇話が増えた。'))
ev('privateer_licence', 2, '私掠許可', '海賊頭目が私掠許可と引き換えに献金を申し出ている。',
    q('許可を発給', '金を取り証を発給すると{kingdom}の海に編成された狼が増えた', '私掠船が進出し庫金と悪名が一緒に帳入された。'),
    q('船を焼く', '断固拒否し模範とするため船を焼く', '海賊船は松明となり海路が清浄になった。'))
ev('hero_funeral', 2, '英雄の葬儀', '辺境を守り抜いた老将が逝去し全国が哀悼し葬儀の規格が未定だ。',
    q('国葬', '国葬で送り費用は王室全額', '万民が街路で見送り列国の使節も参列した。'),
    q('簡素に葬る', '遺言通り簡素に葬る', '老将は無名の墓に眠ったが心には名がある。'))

# --- 残り民生 ---
ev('market_fire', 2, '市場の火災', '深夜に市場が出火、半分が焼け野原になり商人は資本を失った。',
    q('王室が助成', '王庫金で商人の再建を助ける', '新市場が開店し从前より賑やかになった。'),
    q('自助に任せる', '市集は自建し王室は一钱も出さない', '市集は粗末に再開し商人は税吏に嘆息した。'))
ev('water_shortage', 2, '水不足', '夏の少雨で井戸が涸れ城中で毎日長い列ができている。',
    q('深井戸を掘る', '重金で工匠に深井戸と水路を依頼', '清泉が城に入り水不足は解消した。'),
    q('給水を制限', '戸別に制限し工数は省くが不満が四起', '水が配給され列は減ったが不満が増えた。'))
ev('tenant_strike', 2, '小作人の抗租', '田租の連続値上げに小作人が結束して拒否し田荘が王庭に訴えた。',
    q('減租を勧める', '王室が地主に出面し減租を勧めて事を収める', '租約が改訂され小作人が歓声を上げた。'),
    q('強制催缴', '役人を派遣し強制的に取り立てる王法が先', '租銀は全額入荘したが田畑の眼差しは冷えた。')
ev('festival_request', 2, '祝祭の請願', '百姓が連名で請願：豊年が来たので{king}に全城庆典を許可してほしい。',
    q('庆典を钦定', '王室が出資し庆典を開き民と楽しむ', '三日間の祝祭で全城が熱狂し皆{king}の徳を称えた。'),
    q('豊年でも見送る', '良い日は過ごし金は悪年景に残す', '請願は婉拒され街は{king}を吝嗇と噂した。')
ev('night_patrol', 2, '夜盗の横行', '夜になると盗賊が横行し商家が続けて盗まれ商会が夜巡を請求している。',
    q('夜巡隊を組織', '金で夜巡隊を養い商家に安寧な夜を返す', '夜巡が配置され盗賊は絶え商家は安心した。'),
    q('商家の自保に任せる', '各商家が自ら護院を雇い王室は出さない', '護院が各々で戦い盗賊は弱点を狙った。'))
ev('bathhouse_fad', 2, '風呂屋の流行', '城中で風呂屋が突然流行し行会が王室の擡頭を請求している。',
    q('新澡堂を倡建', '王室が拡建を倡導し衛生の恩恵が全城に及ぶ', '澡堂が立ち並び{kingdom}は清潔で聞こえた。'),
    q '流行は去る', '王は民と風呂屋を争わない流行は自ずと去る', '流行は予想通り去り数軒の澡堂だけが残った。')
ev('traveling_fair', 2, '巡遊市', '巡遊市が大挙して城に入り王室の開市と課税を請求している。',
    q('開市を許可', '開市を許可し王室が割合で課税', '半月の市で税金と歓声が一緒に庫に入った。'),
    q('入城を辞退', '市集の秩序を恐れ入城を丁重に辞退する', '市は隣国に行き{kingdom}の街は少し寂しくなった。'))

# --- 残り外交 ---
ev('royal_visit', 2, '王室の来訪', '隣国王室が来訪を表明した——排出負けはできず礼儀はなお更だ。',
    q('全力で接待', '国礼で全力接待し賓主ともに楽しむ', '賓主が楽しみ列国に美談として伝わった。'),
    q('定例で接待', '常例通り失礼なく規矩を超えない', '礼は整ったが驚きはなく来賓の印象は平淡だった。'))
ev('hostage_request', 2, '質子の要請', '強盟が書簡を送り王室の子弟を質に送り両国の好を証明するよう求めている。',
    q('忍んで質子を送る', '骨肉の分離で強盟を安心させる宮中は泣き声ばかり', '質子が出発し盟約は堅くなったが宮中の痛みは自知のみ。',
    q('丁重に拒否', '幼子が年弱を理由に丁重に拒否し盟友の顔が曇る', '拒絶の辞が伝わり盟友に間隙が生まれた。')
ev('border_treaty', 2, '境界の盟約', '隣国が境界条約の締結を提案し界碑を定め永く辺釁を息にしたい。',
    q('条約に調印', '争議地を譲り百年の辺安と引換える', '界碑が立定し両国の辺民は械闘しなくなった。',
    q('一歩も譲らず', '祖産は寸土も譲らず辺軍は警戒を倍加', '交渉が破裂し辺境の哨所が互いに警戒した。')
ev('pirate_bribe', 2, '海賊の通行料', '海賊が分账清单を送ってきた：金を払えば{kingdom}の商船は安全と保証する。',
    q('金を払う', '破財免災で商船に安全旗を掲げさせる', '買路錢が出ると{kingdom}商船は阻害なく通行した。',
    q('賞金で討伐', '海賊に賞金を懸けて討伐し以牙還牙', '海賊の数股が剿滅され残りは恨んだ。')
ev('pilgrim_wave', 2, '巡礼の波', '聖地の顕霊の噂が広まり大群の巡礼者が{kingdom}の国境に押し寄せている。',
    q('棚を設けて接待', '沿道に粥棚と駅舎を設け過境の巡礼者を手厚く遇する', '巡礼者は恩を感じ{kingdom}の善名が遠くに広がった。',
    q('閉関して追い出す', '流民の動乱を恐れ関を閉じ巡礼の队伍を追い出す', '巡礼者は界の外に阻まれ聖地の名声は他国に帰した。')
ev('tribute_envoy', 2, '貢使团', '遠方の小国が使を遣わし称臣納貢、{kingdom}の庇護のみを求めている。',
    q('貢を受ける', '貢品を受け入れ庇護の名も引き受ける', '貢品が庫に入り庇護の責も{kingdom}の頭に記された。'),
    q('貢礼を謙辞', '小国の物を貪らず平等の礼で遇する', '貢礼が謙辞され小国はかえって{kingdom}を三分割に尊敬した。')

# --- 法典档位名 ---
for k, v in {
 'law_education_lv0': 'なし', 'law_education_lv1': '初等', 'law_education_lv2': '中等', 'law_education_lv3': '高等', 'law_education_lv4': '全民',
 'law_healthcare_lv0': 'なし', 'law_healthcare_lv1': '基本', 'law_healthcare_lv2': '標準', 'law_healthcare_lv3': '充実', 'law_healthcare_lv4': '無料',
 'law_migrant_lv0': '閉鎖', 'law_migrant_lv1': '制限', 'law_migrant_lv2': '開放', 'law_migrant_lv3': '歓迎', 'law_migrant_lv4': '無制限',
 'law_religion_lv0': '圧迫', 'law_religion_lv1': '寛容', 'law_religion_lv2': '保護', 'law_religion_lv3': '国教', 'law_religion_lv4': '政教一致',
 'law_press_lv0': '自由', 'law_press_lv1': '標準', 'law_press_lv2': '検閲', 'law_press_lv3': '統制', 'law_press_lv4': '独占',
 'law_gun_control_lv0': '無制限', 'law_gun_control_lv1': '緩い', 'law_gun_control_lv2': '許可制', 'law_gun_control_lv3': '禁止', 'law_gun_control_lv4': '完全禁止',
 'law_conscription_lv0': '志願', 'law_conscription_lv1': '部分的', 'law_conscription_lv2': '標準', 'law_conscription_lv3': '強制', 'law_conscription_lv4': '全民皆兵',
 'law_standing_army_lv0': 'なし', 'law_standing_army_lv1': '小規模', 'law_standing_army_lv2': '標準', 'law_standing_army_lv3': '大規模', 'law_standing_army_lv4': '巨大',
 'law_militarism_lv0': '平和', 'law_militarism_lv1': '備戦', 'law_militarism_lv2': '軍拡', 'law_militarism_lv3': '軍国', 'law_militarism_lv4': '総力戦',
 'law_pacifism_lv0': '好戦', 'law_pacifism_lv1': '中立', 'law_pacifism_lv2': '平和主義', 'law_pacifism_lv3': '非武装', 'law_pacifism_lv4': '完全非武装',
 'law_judicial_lv0': '王命', 'law_judicial_lv1': '慣習', 'law_judicial_lv2': '成文', 'law_judicial_lv3': '独立', 'law_judicial_lv4': '最高',
 'law_capital_pun_lv0': '廃止', 'law_capital_pun_lv1': '極刑のみ', 'law_capital_pun_lv2': '重罪', 'law_capital_pun_lv3': '広範', 'law_capital_pun_lv4': '乱用',
 'law_ant_corrupt_lv0': '放置', 'law_ant_corrupt_lv1': '調査', 'law_ant_corrupt_lv2': '取締', 'law_ant_corrupt_lv3': '厳罰', 'law_ant_corrupt_lv4': ' ZERO tolerance',
 'law_prison_lv0': '厳罰', 'law_prison_lv1': '標準', 'law_prison_lv2': '更生', 'law_prison_lv3': '教育', 'law_prison_lv4': '解放',
 'law_forest_lv0': '無制限', 'law_forest_lv1': '管理', 'law_forest_lv2': '保護', 'law_forest_lv3': '厳格', 'law_forest_lv4': '神聖',
 'law_animal_lv0': '無関', 'law_animal_lv1': '関心', 'law_animal_lv2': '保護', 'law_animal_lv3': '厳格', 'law_animal_lv4': '聖獣',
 'law_pollution_lv0': '無視', 'law_pollution_lv1': '関心', 'law_pollution_lv2': '規制', 'law_pollution_lv3': '厳格', 'law_pollution_lv4': '完全',
 'law_monarchy_lv0': '無王', 'law_monarchy_lv1': '選挙王', 'law_monarchy_lv2': '世襲王', 'law_monarchy_lv3': '絶対王', 'law_monarchy_lv4': '神権王',
 'law_parliament_lv0': 'なし', 'law_parliament_lv1': '諮問', 'law_parliament_lv2': '立法', 'law_parliament_lv3': '主導', 'law_parliament_lv4': '支配',
 'law_planned_economy_lv0': '自由', 'law_planned_economy_lv1': '指導', 'law_planned_economy_lv2': '統制', 'law_planned_economy_lv3': '計画', 'law_planned_economy_lv4': '完全',
 'law_free_market_lv0': '統制', 'law_free_market_lv1': '規制緩和', 'law_free_market_lv2': '自由', 'law_free_market_lv3': '完全自由', 'law_free_market_lv4': '無政府',
 'law_state_religion_lv0': 'なし', 'law_state_religion_lv1': '優遇', 'law_state_religion_lv2': '公認', 'law_state_religion_lv3': '国教', 'law_state_religion_lv4': '強制',
 'law_secularism_lv0': '国教', 'law_secularism_lv1': '分离', 'law_secularism_lv2': '中立', 'law_secularism_lv3': '政教分離', 'law_secularism_lv4': '無宗教',
}.items():
    t({k: v})

# --- 設定ラベル ---
for k, v in {
 'unrest_enabled': '不安システム', 'log_worldlog': 'ワールドログ出力',
 'gini_threshold': 'ジニ閾値', 'unrest_grace_years': '不安猶予年数',
 'unrest_max_cities': '最大影響都市', 'policy_enabled': '国家政策',
 'cycle_enabled': '経済サイクル', 'cycle_gini_high': 'サイクル高ジニ',
 'cycle_gini_low': 'サイクル低ジニ', 'cycle_gini_periods': 'サイクル期間',
 'boom_stimulus_ratio': '好況刺激比', 'boom_bubble_factor': 'バブル係数',
 'bubble_threshold': 'バブル閾値', 'boom_max_duration': '好況最長',
 'recession_max_duration': '後退最長', 'depression_max_duration': '恐慌最長',
 'recovery_max_duration': '回復最長', 'survival_line': '生存線',
 'war_plunder_ratio': '戦争略奪比', 'war_waste_ratio': '戦争消耗比',
 'revolution_delay_years': '革命遅延年', 'revolution_kill_ratio': '革命処刑比',
 'uprising_gini_threshold': '蜂起ジニ閾値', 'uprising_delay_years': '蜂起遅延年',
 'kill_rich_ratio': '富人処刑比', 'kill_rich_redist_ratio': '富人再分配比',
 'wealth_tax_enabled': '富裕税', 'wealth_tax_ratio': '富裕税率',
 'wealth_tax_line': '富裕税線', 'population_enabled': '人口制約',
 'population_overcrowd': '過稠閾値', 'era_enabled': '時代イベント',
 'era_duration_years': '時代持続年', 'collapse_drop_ratio': '崩壊下落比',
 'collapse_duration_years': '崩壊持続年', 'flourish_military_ratio': '強盛軍事比',
 'flourish_periods': '強盛期数', 'labor_enabled': '労働分工',
 'labor_wage_base': '基本賃金', 'real_time_refresh': 'リアルタイム更新',
 'real_time_interval': '更新間隔', 'real_time_refresh_threshold': '更新閾値',
 'real_time_refresh_budget': '更新予算', 'money_velocity': '貨幣流速',
 'inflation_bubble_boost': '通胀バブル加速', 'disaster_enabled': '災害衝撃',
 'disaster_wealth_loss': '災害財産損失', 'disaster_mine_bonus': '火山鉱産刺激',
 'banking_enabled': '銀行貸付', 'credit_rate': '貸付利率',
 'default_rate_depression': '恐慌デフォルト率', 'crisis_contagion_threshold': '危機伝染閾値',
 'spending_cap_per_year': '年間消費上限', 'banking_default_cap_per_year': '銀行デフォルト上限',
 'banking_contagion_cap_per_year': '伝染評価上限', 'inheritance_scan_per_frame': '遺産走査枠',
 'frame_budget_ms': 'フレーム予算ms', 'cycle_window_ms': '年度窓ms',
 'perf_diagnostics_enabled': '性能診断', 'cycle_alloc_budget': '年度割当予算',
 'memory_cleanup_enabled': 'メモリ自動整理', 'memory_cleanup_force_gc': '強制GC',
 'memory_cleanup_interval_seconds': '整理間隔秒', 'memory_cleanup_notify_enabled': '整理通知',
 'nation_play_enabled': '中央銀行家', 'treasury_income_ratio': '宝物庫税負比',
 'policy_slots': '政策枠数', 'event_chance_player': 'プレイヤーイベント率',
 'event_chance_ai': 'AIイベント率', 'event_cooldown_years': 'イベント全局冷却',
}.items():
    t({k: v})
for k, v in {
 'unrest_enabled Description': '貧富差による社会不安システムの有効化',
 'log_worldlog Description': '周期ログをワールドログに出力',
 'gini_threshold Description': '不安触发の基尼係数阈值（0.1~1.0）',
}.items():
    t({k: v})

# マージ
merged = 0
for k, v in JA.items():
    if existing.get(k) == en.get(k):
        existing[k] = v; merged += 1
json.dump(existing, io.open(path, 'w', encoding='utf-8'), ensure_ascii=False, indent=2)
io.open(path, 'a', encoding='utf-8').write('\n')
print('JA batch B merged:', merged, 'total:', len(existing))

# 残り確認
en_keys = set(en.keys())
remaining = [k for k in en_keys if existing.get(k) == en.get(k)]
print('remaining untranslated:', len(remaining))
if remaining:
    for k in remaining[:10]: print(' ', k)
