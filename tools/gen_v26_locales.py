# -*- coding: utf-8 -*-
"""v2.0.6: 6 war-cycle events x 6 languages. Idempotent (only fills missing keys)."""
import io, json, collections, os

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
os.chdir(ROOT)

EVENTS = collections.OrderedDict()
def ev(eid, opts, title, desc, *texts):
    EVENTS[eid] = (opts, title, desc, list(texts))
def q(*args): return args

ev('armament_race', 2,
 ('军备竞赛', '軍備競賽', 'Arms Race', 'Гонка вооружений', '軍備競争', 'Rüstungswettlauf'),
 ('列国磨刀霍霍——{king}的将军们请奏大举营造武备，边军与兵工坊俱要整饬。', '列國磨刀霍霍——{king}的將軍們請奏大舉營造武備，邊軍與兵工坊俱要整飭。', 'The powers whet their blades — {king}\'s generals ask to arm up: border legions and arsenals alike.', 'Державы точат клинки — генералы {king} просят вооружиться: и легионы, и арсеналы.', '列国が刃を研ぐ——{king}の将軍が大規模な軍備と工房の整備を請う。', 'Die Mächte wetzen ihre Klingen — die Generäle von {king} bitten um Aufrüstung: Legionen und Arsenale.'),
 q('大举营造', '大舉營造', 'Arm up', 'Вооружиться', '大規模整備', 'Aufrüsten'),
 q('拨出重金，军械坊连夜开工。', '撥出重金，軍械坊連夜開工。', 'Pour out the gold; the forges work all night.', 'Высыпать золото; кузницы работают всю ночь.', '重金を投じ工房が夜通し稼働する。', 'Gold ausgeben; die Schmieden arbeiten die ganze Nacht.'),
 q('武备一新，{kingdom}的边军兵锋更锐，国库也瘦了一圈。', '武備一新，{kingdom}的邊軍兵鋒更銳，國庫也瘦了一圈。', 'The arms are renewed; {kingdom}\'s legions are sharper, the treasury leaner.', 'Оружие обновлено; легионы {kingdom} острее, казна тоньше.', '武備が新しくなり{kingdom}の辺軍が鋭く、国庫は痩せる。', 'Die Rüstung ist erneuert; die Legionen von {kingdom} sind schärfer, die Kasse schlanker.'),
 q('暂缓营造', '暫緩營造', 'Hold off', 'Повременить', '見送る', 'Abwarten'),
 q('先与列国修好，暂缓军备预算。', '先與列國修好，暫緩軍備預算。', 'Make peace with the powers first; shelve the arms budget.', 'Сначала мир с державами; отложить военный бюджет.', 'まず列国と交を結び軍備予算を見送る。', 'Erst Frieden mit den Mächten; das Rüstungsbudget vertagen.'),
 q('修好换来了平静，{kingdom}的武备却显得落后了一截。', '修好換來了平靜，{kingdom}的武備卻顯得落後了一截。', 'Peace buys calm, yet {kingdom}\'s arms look a step behind.', 'Мир покупает спокойствие, но оружие {kingdom} отстало.', '平静は得るが{kingdom}の武備は一歩遅れる。', 'Frieden schenkt Ruhe, doch die Rüstung von {kingdom} hinkt hinterher.')
)
ev('levy_muster', 2,
 ('全面征募', '全面徵募', 'The Great Levy', 'Великий набор', '大徴募', 'Die große Aushebung'),
 ('战云密布，{king}下令在{kingdom}全境征募壮丁，民田为之半荒。', '戰雲密佈，{king}下令在{kingdom}全境徵募壯丁，民田為之半荒。', 'War clouds gather; {king} orders a levy across {kingdom}, the fields half-tended.', 'Тучи войны сгущаются; {king} объявляет набор по {kingdom}, поля наполовину заброшены.', '戦雲が立ち込め{king}が{kingdom}全土で徴募を命じ、田畑は半分荒れる。', 'Kriegswolken ziehen auf; {king} ordnet eine Aushebung an, die Felder halb gepflegt.'),
 q('全境征募', '全境徵募', 'Levy everywhere', 'Набор повсюду', '全土で徴募', 'Überall ausheben'),
 q('壮丁入伍，农作暂废，武备空前。', '壯丁入伍，農作暫廢，武備空前。', 'The levy joins; farming yields to unprecedented arms.', 'Набор вступает; жатва уступает оружию.', '壮丁が入隊し耕作が止まり武備は空前。', 'Die Aushebung tritt ein; die Ernte weicht beispielloser Rüstung.'),
 q('大军成列，{kingdom}的粮仓却渐渐见底了。', '大軍成列，{kingdom}的糧倉卻漸漸見底了。', 'The columns form; {kingdom}\'s granaries run low.', 'Колонны строятся; житницы {kingdom} пустеют.', '大軍が整列し{kingdom}の穀倉が底をつく。', 'Die Kolonnen formieren sich; die Kornspeicher von {kingdom} leeren sich.'),
 q('以资代役', '以資代役', 'Commutate the levy', 'Выкупить набор', '銭で代役', 'Die Aushebung ablösen'),
 q('准许纳金代役，保农功而充军资。', '准許納金代役，保農功而充軍資。', 'Permit pay-in-lieu; keep the fields and fill the war chest.', 'Разрешить откуп; сохранить поля и наполнить казну.', '金で代役を許し農を守り軍資を満たす。', 'Ablösung erlauben; die Felder wahren und die Kriegskasse füllen.'),
 q('代役金充裕军资，{kingdom}的民心也安稳了几分。', '代役金充裕軍資，{kingdom}的民心也安穩了幾分。', 'The commutation gold fills the chest; hearts stay calm.', 'Откупное золото пополняет казну; сердца спокойны.', '代役金が軍資を満たし民心も落ち着く。', 'Das Ablösungsgold füllt die Kasse; die Herzen bleiben ruhig.')
)
ev('great_war_unleashed', 2,
 ('大战爆发', '大戰爆發', 'The Great War', 'Великая война', '大戦勃発', 'Der große Krieg'),
 ('多年积怨一朝点燃——{king}面前的战报：全面战争迫在眉睫，或沉或浮在此一举。', '多年積怨一朝點燃——{king}面前的戰報：全面戰爭迫在眉睫，或沉或浮在此一舉。', 'Years of grudge ignite — the dispatch before {king}: full-scale war looms, sink or sail.', 'Годы обид вспыхнули — донесение перед {king}: полномасштабная война, пан или пропал.', '積年の恨みが燃え上がる——{king}の前の戦報：全面戦争が眼前、浮くか沈むか。', 'Jahre des Grolls entzünden sich — die Meldung vor {king}: der große Krieg steht bevor, sinken oder segeln.'),
 q('倾国一战', '傾國一戰', 'Throw the kingdom in', 'Вложить всё королевство', '全力で戦う', 'Das Reich werfen'),
 q('举全国之力向当前敌国开战，不留退路。', '舉全國之力向當前敵國開戰，不留退路。', 'Commit the whole realm against the current foe; no way back.', 'Бросить всё против текущего врага; пути назад нет.', '全力を挙げ現在の敵国へ、退路なし。', 'Das ganze Reich gegen den aktuellen Feind werfen; kein Weg zurück.'),
 q('鼓角齐鸣，{kingdom}与强敌展开旷日持久的大战。', '鼓角齊鳴，{kingdom}與強敵展開曠日持久的大戰。', 'Horns sound; {kingdom} and its foe grind into a long war.', 'Трубы поют; {kingdom} и враг начинают долгую войну.', '角笛が鳴り{kingdom}は強敵と長い戦いへ。', 'Hörner erklingen; {kingdom} und sein Feind mahlen in einen langen Krieg.'),
 q('维持均势', '維持均勢', 'Preserve the balance', 'Сохранить баланс', '均衡を保つ', 'Die Balance wahren'),
 q('忍而决断，先以岁币换得喘息。', '忍而決斷，先以歲幣換得喘息。', 'Endure; buy a breath with tribute.', 'Стерпеть; купить передышку данью.', '耐え、歳幣で息を買う。', 'Erdulden; einen Atemzug mit Tribut kaufen.'),
 q('岁币换来了喘息，{kingdom}的武备却空转了几年。', '歲幣換來了喘息，{kingdom}的武備卻空轉了幾年。', 'The tribute buys breath, yet {kingdom}\'s arms idle for years.', 'Дань покупает передышку, но оружие {kingdom} простаивает.', '歳幣が息を買うが武備は数年空転。', 'Der Tribut erkauft Atem, doch die Rüstung von {kingdom} leert sich jahrelang.')
)
ev('postwar_decay', 2,
 ('战后民生凋敝', '戰後民生凋敝', 'Postwar Decay', 'Послевоенный упадок', '戦後の荒廃', 'Nachkriegsverfall'),
 ('大战方歇，{kingdom}的田畴荒芜、市集萧条，老兵流落街头。', '大戰方歇，{kingdom}的田疇荒蕪、市集蕭條，老兵流落街頭。', 'The war ends; {kingdom}\'s fields lie fallow, markets silent, veterans adrift.', 'Война кончилась; поля {kingdom} пустуют, рынки молчат, ветераны без крова.', '戦が終わり{kingdom}の田は荒れ、市場は寂れ、老兵が彷徨う。', 'Der Krieg endet; die Felder von {kingdom} liegen brach, die Märkte schweigen, Veteranen treiben.'),
 q('开仓振济', '開倉振濟', 'Open the granaries', 'Открыть житницы', '倉を開く', 'Die Speicher öffnen'),
 q('发粟施药，安抚流民与老兵。', '發粟施藥，安撫流民與老兵。', 'Distribute grain and medicine; soothe refugees and veterans.', 'Раздать зерно и лекарства; утешить беженцев и ветеранов.', '粟と薬を施し難民と老兵を慰撫する。', 'Getreide und Medizin verteilen; Flüchtlinge und Veteranen beruhigen.'),
 q('赈济有成，{kingdom}的民心慢慢回暖。', '賑濟有成，{kingdom}的民心慢慢回暖。', 'Relief works; the hearts of {kingdom} warm again.', 'Помощь действует; сердца {kingdom} согреваются.', '賑済が功を奏し{kingdom}の民心が温まる。', 'Die Hilfe wirkt; die Herzen von {kingdom} wärmen sich wieder.'),
 q('放任凋敝', '放任凋敝', 'Let it decay', 'Пусть упадёт', '放置する', 'Verfallen lassen'),
 q('不施援手，民生自愈，只是过程漫长。', '不施援手，民生自愈，只是過程漫長。', 'No helping hand; the land heals itself, slowly.', 'Без помощи; земля исцелится сама, медленно.', '手を貸さず民は自ら癒える、ただ長い。', 'Keine helfende Hand; das Land heilt sich selbst, langsam.'),
 q('多年后{kingdom}才从废墟中缓过神来。', '多年後{kingdom}才從廢墟中緩過神來。', 'Years pass before {kingdom} comes to from the ruins.', 'Проходят годы, прежде чем {kingdom} очнётся от руин.', '何年も経って{kingdom}は廃墟から立ち直る。', 'Jahre vergehen, ehe {kingdom} aus den Trümmern erwacht.')
)
ev('reconstruction_begin', 2,
 ('百废待兴', '百廢待興', 'Reconstruction Begins', 'Начало восстановления', '復興の始まり', 'Der Wiederaufbau beginnt'),
 ('战后第一缕晨光照进{kingdom}——工部请奏重建庙堂坊肆，修路通渠，百废待兴。', '戰後第一縷晨光照進{kingdom}——工部請奏重建廟堂坊肆，修路通渠，百廢待興。', 'The first postwar light enters {kingdom} — builders petition to rebuild temples and stalls, roads and canals.', 'Первый послевоенный свет входит в {kingdom} — строители просят восстановить храмы и лавки, дороги и каналы.', '戦後最初の陽が{kingdom}に差す——工部が廟堂と市場、道と水路の復興を請う。', 'Das erste Nachkriegslicht fällt in {kingdom} — die Bauleute bitten um Tempel und Stände, Straßen und Kanäle.'),
 q('全面重建', '全面重建', 'Rebuild all', 'Восстановить всё', '全面復興', 'Alles wiederaufbauen'),
 q('大兴土木，升级各处建筑，恢复元气。', '大興土木，升級各處建築，恢復元氣。', 'Great works: upgrade buildings across the land and restore vigor.', 'Большие работы: модернизировать здания и вернуть силы.', '大規模に建築を上げ、元気を回復する。', 'Große Werke: Gebäude aufwerten und die Kraft wiederherstellen.'),
 q('栋宇重新，{kingdom}的市面一点点恢复生气。', '棟宇重新，{kingdom}的市面一點點恢復生氣。', 'Roofs rise; {kingdom}\'s streets regain a little life.', 'Крыши встают; улицы {kingdom} обретают жизнь.', '屋根が甦り{kingdom}の街が少しずつ息を吹き返す。', 'Die Dächer steigen; die Straßen von {kingdom} atmen wieder.'),
 q('以农为先', '以農為先', 'Farm first', 'Сначала пашня', '農を優先', 'Erst die Felder'),
 q('先垦田亩，待仓廪实再谈营造。', '先墾田畝，待倉廩實再談營造。', 'Till the fields first; when granaries are full, then build.', 'Сначала пашня; когда житницы полны, потом стройка.', 'まず田を耕し、倉が満ちてから营造を。', 'Erst die Felder pflügen; wenn die Speicher voll sind, dann bauen.'),
 q('仓廪渐实，{kingdom}的重建缓了几年却更稳当了。', '倉廩漸實，{kingdom}的重建緩了幾年卻更穩當了。', 'Granaries fill; reconstruction lags but stands firmer.', 'Житницы полны; стройка замедлилась, но крепче.', '倉が満ち、復興は遅れるがより堅実だ。', 'Die Speicher füllen sich; der Bau verzögert sich, steht aber fester.')
)
ev('renewal_spring', 2,
 ('复兴之春', '復興之春', 'The Spring of Renewal', 'Весна возрождения', '復興の春', 'Der Frühling der Erneuerung'),
 ('暌违多年，作物的新绿与市集的喧哗一起回到{kingdom}——复兴之春来了。', '暌違多年，作物的新綠與市集的喧嘩一起回到{kingdom}——復興之春來了。', 'After long years, green shoots and market noise return together to {kingdom} — the spring of renewal.', 'После долгих лет зелень и шум рынка вместе возвращаются в {kingdom} — весна обновления.', '長い年月を経て、緑と市場の喧騒が一緒に{kingdom}へ——復興の春。', 'Nach langen Jahren kehren grüne Triebe und Marktlärm gemeinsam nach {kingdom} zurück — der Frühling der Erneuerung.'),
 q('春风化雨', '春風化雨', 'Beneficent spring', 'Благодатная весна', '慈雨', 'Segensreicher Frühling'),
 q('减税轻徭，让财富重新流入民间。', '減稅輕徭，讓財富重新流入民間。', 'Cut taxes and corvées; let wealth flow back to the people.', 'Снизить налоги и повинности; пусть богатство вернётся к народу.', '減税と軽徭で富を民に還す。', 'Steuern und Frondienste senken; der Wohlstand soll zum Volk zurückfließen.'),
 q('民心沸腾，{kingdom}的财富开始新一轮增长。', '民心沸騰，{kingdom}的財富開始新一輪增長。', 'Hearts leap; the wealth of {kingdom} begins a new ascent.', 'Сердца ликуют; богатство {kingdom} начинает новый подъём.', '民心が沸き{kingdom}の富が新たな上昇へ。', 'Die Herzen hüpfen; der Reichtum von {kingdom} beginnt einen neuen Aufstieg.'),
 q('厉兵秣马', '厲兵秣馬', 'Sharpen anew', 'Точить снова', '再び刃を研ぐ', 'Erneut schleifen'),
 q('复兴之际不忘武备，为下一代大战做准备。', '復興之際不忘武備，為下一代大戰做準備。', 'Even in renewal, keep arms ready for the next war.', 'Даже в обновлении — держать оружие наготове к следующей войне.', '復興の折も武備を忘れず次なる戦に備える。', 'Auch in der Erneuerung die Waffen für den nächsten Krieg bereit halten.'),
 q('武备重振，{kingdom}再次站上备战的起跑线。', '武備重振，{kingdom}再次站上備戰的起跑線。', 'Arms revive; {kingdom} stands at the next starting line.', 'Оружие возрождено; {kingdom} стоит на следующем старте.', '武備が再び栄え{kingdom}は次の出発線へ。', 'Die Rüstung erwacht; {kingdom} steht an der nächsten Startlinie.')
)

LANGS = {'ch': 0, 'zh_tw': 1, 'en': 2, 'ru': 3, 'ja': 4, 'de': 5}
total_added = 0
for lang, idx in LANGS.items():
    p = 'Locales/%s.json' % lang
    d = json.load(io.open(p, encoding='utf-8'), object_pairs_hook=collections.OrderedDict)
    added = 0
    for eid, (opts, title, desc, texts) in EVENTS.items():
        kv = {'ev_%s' % eid: title[idx], 'ev_%s_desc' % eid: desc[idx]}
        for i in range(opts):
            kv['ev_%s_opt%d' % (eid, i + 1)] = texts[i * 3][idx]
            kv['ev_%s_opt%d_desc' % (eid, i + 1)] = texts[i * 3 + 1][idx]
            kv['ev_%s_res%d' % (eid, i + 1)] = texts[i * 3 + 2][idx]
        for k, v in kv.items():
            if k not in d:
                d[k] = v; added += 1
    json.dump(d, io.open(p, 'w', encoding='utf-8'), ensure_ascii=False, indent=2)
    io.open(p, 'a', encoding='utf-8').write('\n')
    total_added += added
    print(lang, 'added', added, 'total', len(d))
print('TOTAL added keys:', total_added)
