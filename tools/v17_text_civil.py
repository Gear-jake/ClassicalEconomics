# -*- coding: utf-8 -*-
"""v1.7.0 civil 文案（六语）"""
EVENTS = {}
def ev(eid, opts, title, desc, *texts):
    EVENTS[eid] = (opts, title, desc, list(texts))
def q(*args): return args

# ===== 行会闹事（guild_walk：磨坊/裁缝/铁匠）=====
ev('mill_guild', 2,
 ('磨坊行会', '磨坊行會', 'The Millers\' Guild', 'Гильдия мельников', '粉挽きギルド', 'Die Mühlengilde'),
 ('磨坊主们围住{king}的粮仓叫屈，因磨盘份额争执，扬言无利便歇业，市集面粉业已见紧。', '磨坊主們圍住{king}的糧倉叫屈，因磨盤份額爭執，揚言無利便歇業，市集麵粉業已見緊。', 'The millers crowd around {king}\'s granary, quarreling over shares of the stones, and threaten to shut the mill if profit fails.', 'Мельники теснятся у амбара {king}, споря о долях жерновов, и грозят закрыть мельницы, если не будет прибыли.', '粉挽きが{king}の穀倉に詰めかけ、臼の分け前を争い、利が立たねば休業すると脅す。', 'Die Müller bedrängen {king}\'s Getreidespeicher und streiten um die Mühlsteine; drohen mit Stillstand, wenn der Gewinn fehlt.'),
 q('许诺厚利', '許諾厚利', 'Promise fat margins', 'Пообещать выгоду', '厚利を約す', 'Fetten Gewinn versprechen'),
 q('行会得利自然停下磨盘，市集粮价随之回稳。', '行會得利自然停下磨盤，市集糧價隨之回穩。', 'Fat margins quiet the guild; flour prices settle.', 'Щедрая прибыль успокаивает гильдию; цены на муку оседают.', '厚利を約せばギルドは静まり、粉の値段も落ち着く。', 'Fetter Gewinn besänftigt die Zunft; die Mehlpreise beruhigen sich.'),
 q('加价抚之，磨坊复工，列国闻{kingdom}重商之名而称善。', '加價撫之，磨坊復工，列國聞{kingdom}重商之名而稱善。', 'Higher pay restarts the mills; foreign markets praise {kingdom}\'s fair dealing.', 'Плата повышена, мельницы вновь крутятся; державы хвалят {kingdom} за честную торговлю.', '値上げで粉挽きが再稼働し、列国は{kingdom}の商いの公平さを称えた。', 'Höherer Lohn setzt die Mühlen wieder in Gang; die Nachbarstaaten loben {kingdom}\'s ehrliche Handelsweise.'),
 q('铁腕压行', '鐵腕壓行', 'Crack down hard', 'Задавить силой', '強硬に抑える', 'Hart durchgreifen'),
 q('强行压服行会，磨盘不转，骚乱恐起街头。', '強行壓服行會，磨盤不轉，騷亂恐起街頭。', 'Force the guild to heel; stalled stones may bring street unrest.', 'Силою принудить гильдию; застывшие жернова грозят волнениями.', '強硬に押さえつければ、臼は止まり、街の騒乱を招く恐れがある。', 'Die Zunft zwangsweise fügsam machen; stille Steine können Unruhe auf die Straße bringen.'),
 q('磨坊主摔袖而走，粮价飞涨，街头怨声载道。', '磨坊主摔袖而走，糧價飛漲，街頭怨聲載道。', 'The millers walk off in fury; bread prices soar and the streets grumble.', 'Мельники в ярости уходят; хлеб дорожает, улицы ропщут.', '粉挽きは怒って去り、食料の値段が跳ね上がり、街に不満が渦巻く。', 'Die Müller ziehen wütend davon; die Brotpreise schießen hoch, die Straßen murren.')
)
ev('tailor_march', 2,
 ('裁缝行会', '裁縫行會', 'The Tailors\' Guild', 'Гильдия портных', '仕立てギルド', 'Die Schneiderzunft'),
 ('裁缝们头戴彩羽、列队巡行，为裁衣专营之利向{king}请愿，人潮堵住集市路口。', '裁縫們頭戴彩羽、列隊巡行，為裁衣專營之利向{king}請願，人潮堵住集市路口。', 'Feather-capped tailors parade the streets to beg {king} for the tailoring monopoly; their crowd blocks the market gate.', 'Портные в перьях маршируют, вымаливая у {king} портновскую монополию; толпа запрудила ворота рынка.', '羽根かぶりの仕立て屋が列をなし、仕立ての独占を{king}に求めて行進し、人波が市場の入口を塞ぐ。', 'Mit Federn beschmückte Schneider ziehen im Zug, um {king} um das Schneidermonopol zu bitten; ihre Menge verstopft das Markttor.'),
 q('拨库犒赏', '撥庫犒賞', 'Pay them off', 'Откупиться', '恩賞を出す', 'Auszahlen'),
 q('破费一笔库银，裁缝们满意而归，衣市照常开张。', '破費一筆庫銀，裁縫們滿意而歸，衣市照常開張。', 'Treasury silver buys the guild\'s joy; the cloth market opens as usual.', 'Казённые деньги покупают довольство; рынок тканей открывается как обычно.', '国庫から報酬を出せば仕立て屋は満足し、布の市はいつも通り開く。', 'Kassengeld erkauft die Zunft; der Tuchmarkt öffnet wie gewohnt.'),
 q('犒赏既下，锦缎满街，列国商人赞{kingdom}待匠厚道。', '犒賞既下，錦緞滿街，列國商人讚{kingdom}待匠厚道。', 'Pensions paid, silks fill the streets; merchants praise {kingdom}\'s kindness to craftsmen.', 'Награда выплачена, шёлка полны улицы; купцы славят {kingdom} за милость к мастерам.', '恩賞が下り錦の行商が街に溢れ、商人たちは{kingdom}の職人への情けを讃えた。', 'Die Belohnung ist gezahlt, Seide füllt die Straßen; Kaufleute rühmen {kingdom}\'s Güte zu den Handwerkern.'),
 q('驱散队伍', '驅散隊伍', 'Disperse the march', 'Разогнать шествие', '行進を追い散らす', 'Den Zug auflösen'),
 q('命卫兵驱散巡行队伍，衣裳之业恐起风波。', '命衛兵驅散巡行隊伍，衣裳之業恐起風波。', 'Guards scatter the march; the cloth trade may boil over.', 'Стража разгоняет шествие; ткацкое дело может вскипеть.', '衛兵が行進を追い散らせば、衣の業が荒れる恐れがある。', 'Garden zerstreuen den Zug; das Schneidereigewerbe könnte überkochen.'),
 q('人群被驱，裁缝闭店，街头隐见骚乱火头。', '人群被驅，裁縫閉店，街頭隱見騷亂火頭。', 'The crowd is routed, shops shut; embers of unrest glimmer in the lanes.', 'Толпу разогнали, лавки закрыты; в переулках тлеют угли беспорядков.', '人波は追い散らされ店は閉まり、路地に騒乱の火種がくすぶる。', 'Die Menge ist zerstreut, die Läden schließen; in den Gassen glimmen Unruhe-Funken.')
)
ev('smith_guild', 2,
 ('铁匠行会', '鐵匠行會', 'The Smiths\' Guild', 'Гильдия кузнецов', '鍛冶ギルド', 'Die Schmiedezunft'),
 ('铁匠们熄炉罢工，围坐待价，催{king}上调兵器采买之价，城中锻铁声一夜绝迹。', '鐵匠們熄爐罷工，圍坐待價，催{king}上調兵器採買之價，城中鍛鐵聲一夜絕跡。', 'The smiths bank their forges and wait, pressing {king} to raise weapons prices; the anvil song dies in one night.', 'Кузнецы затушили горны и ждут, торопя {king} поднять цены на оружие; звон наковален смолк за ночь.', '鍛冶は炉を消して腰を下ろし、武器の買い上げ値上げを{king}に迫り、金槌の音は一夜で消えた。', 'Die Schmiede löschen ihre Essen und warten; sie drängen {king}, die Waffenpreise anzuheben — der Ambossklang verstummt über Nacht.'),
 q('加订提价', '加訂提價', 'Raise the order price', 'Поднять цену заказа', '取り値を上げる', 'Den Bestellpreis erhöhen'),
 q('加价订购军器，铁匠复工，炉火重燃。', '加價訂購軍器，鐵匠復工，爐火重燃。', 'Priced up, the forges roar again.', 'Цена заказа поднята, горны вновь пылают.', '値上げで受注を続ければ、炉は再び燃え上がる。', 'Höhere Preise lassen die Essen wieder aufflammen.'),
 q('炉火复明，列国见{kingdom}重工匠而感其诚。', '爐火復明，列國見{kingdom}重工匠而感其誠。', 'Forges rekindle; the nations feel {kingdom}\'s honest regard for craftsmen.', 'Горны снова горят; державы видят честную заботу {kingdom} о ремесленниках.', '炉が再び燃え、列国は{kingdom}の職人への誠意を受け取った。', 'Die Essen brennen wieder; die Staaten spüren {kingdom}\'s ehrliche Achtung vor den Handwerkern.'),
 q('罢免行首', '罷免行首', 'Depose the guild head', 'Снять старосту', '頭を罷免する', 'Den Zunftmeister absetzen'),
 q('另立行规罢免行首，群匠愤愤，炉火仍熄。', '另立行規罷免行首，群匠憤憤，爐火仍熄。', 'New statutes, new master — the smiths grumble, forges stay dark.', 'Новый устав, новый староста — кузнецы ворчат, горны темны.', '新しい掟と頭を立てれば、職人たちは憤り、炉は暗いままだ。', 'Neue Ordnung, neuer Meister — die Schmiede murren, die Essen bleiben kalt.'),
 q('行首被逐，铁匠闭炉，市面隐隐然有骚动。', '行首被逐，鐵匠閉爐，市面隱隱然有騷動。', 'The master is expelled, the forges shut; a mutter of unrest stirs the town.', 'Староста изгнан, горны закрыты; город шевелится от недовольства.', '頭は追放され炉は閉ざされ、街のどこかで騒乱の気配が動く。', 'Der Meister ist verjagt, die Essen geschlossen; ein Murren der Unruhe regt sich in der Stadt.')
)
# ===== 行会垄断 → 行会罢工（跨年连锁，结果留伏笔）=====
ev('guild_monopoly', 2,
 ('行会垄断', '行會壟斷', 'The Guild Monopoly', 'Монополия гильдий', '行会の独占', 'Das Zunftmonopol'),
 ('三行会联手揽断{kingdom}市价，货物只许按行会牌价买卖，坐地起价、市井哗然。', '三行會聯手攬斷{kingdom}市價，貨物只許按行會牌價買賣，坐地起價、市井譁然。', 'Three guilds corner the markets of {kingdom}: goods may only change hands at guild prices, and the townsfolk cry out.', 'Три гильдии захватили рынки {kingdom}: товары идут лишь по гильдейским ценам, а горожане возмущаются.', '三つのギルドが{kingdom}の市価を握り、品物はギルドの定価でしか売れず、町人は騒いでいる。', 'Drei Zünfte beherrschen die Preise von {kingdom}: Ware darf nur zu Zunftpreisen wechseln, das Volk empört sich.'),
 q('抽取行税', '抽取行稅', 'Levy the guild tax', 'Ввести гильдейский налог', '行税を課す', 'Die Zunftsteuer erheben'),
 q('按行会牌价抽税，金库立丰，坊间冷暖自知。', '按行會牌價抽稅，金庫立豐，坊間冷暖自知。', 'Tax the fixed prices; the treasury swells, the townsfolk feel it.', 'Обложить гильдейские цены налогом; казна полнеет, горожане чувствуют.', 'ギルドの定価に税を掛ければ国庫が肥え、町人の負担は自ずと増す。', 'Die Zunftpreise besteuern; die Kasse schwillt, die Bürger spüren es.'),
 q('税银入鞘，垄断之局益固——然物议渐炽，恐有反弹。', '稅銀入鞘，壟斷之局益固——然物議漸熾，恐有反彈。', 'Tax silver pours in and the monopoly hardens — yet talk grows loud; a backlash is brewing.', 'Налоговое серебро течёт, монополия крепнет — но ропот растёт; расплата зреет.', '税銀が入り独占は固まる——だが物議は高まり、反発の萌芽がある。', 'Steuersilber fließt, das Monopol festigt sich — doch der Unmut wächst; ein Rückschlag gärt.'),
 q('打压行会', '打壓行會', 'Break the cartel', 'Сломать картель', '組合を潰す', 'Das Kartell brechen'),
 q('王室明令拆散牌价联盟，行会作色，市面骚动。', '王室明令拆散牌價聯盟，行會作色，市面騷動。', 'The crown outlaws the price pact; the guilds bristle, the market stirs.', 'Корона запрещает ценовой союз; гильдии кипят, рынок волнуется.', '王が価格協定を禁じれば、ギルドは憤り、市場が騒然とする。', 'Die Krone verbietet das Preisbündnis; die Zünfte sträuben sich, der Markt gärt.'),
 q('牌价禁令一出，市井哄抬乱象，骚乱四起。', '牌價禁令一出，市井哄抬亂象，騷亂四起。', 'The ban is out; prices jump wildly, and unrest flares through the lanes.', 'Запрет объявлен; цены скачут, по улицам вспыхивают столкновения.', '禁令が出れば値段が乱高下し、町のあちこちで騒乱が燃え上がる。', 'Das Verbot ist da; die Preise springen, und Unruhe lodert durch die Gassen.')
)
ev('guild_strike', 2,
 ('行会罢工', '行會罷工', 'The Guild Strike', 'Забастовка гильдий', '行会のストライキ', 'Der Zunftstreik'),
 ('行会挂出罢市牌，店铺闭门、集市冷清——{king}窗前，弹压与和解两案并陈。', '行會掛出罷市牌，店鋪閉門、集市冷清——{king}窗前，彈壓與和解兩案並陳。', 'The guild shuts the market with locked shops and empty stalls — {king} weighs cracking down or making peace.', 'Гильдия закрывает рынок: лавки заперты, прилавки пусты — {king} взвешивает карательный курс или мир.', 'ギルドが罷市の札を掲げ店は閉まり市場は閑散——{king}は弾圧か和解かを天秤にかける。', 'Die Zunft legt den Markt still: Läden verriegelt, Stände leer — {king} wägt zwischen Durchgreifen und Frieden.'),
 q('弹压罢市', '彈壓罷市', 'Crush the strike', 'Подавить забастовку', '罷市を弾圧', 'Den Streik niederschlagen'),
 q('命卫兵开市，与罢工者对峙于长街。', '命衛兵開市，與罷工者對峙於長街。', 'Guards reopen the market; strikers face them down the street.', 'Стража открывает рынок; забастовщики встают напротив.', '衛兵に市場を開かせれば、ストライキ側と長い街路でにらみ合いになる。', 'Garden öffnen den Markt; die Streikenden stellen sich entgegen.'),
 q('强开市集，货摊被砸于市，乱象随之而起。', '強開市集，貨攤被砸於市，亂象隨之而起。', 'The market opens by force; stalls are smashed and turmoil follows.', 'Рынок открыт силой; прилавки разбиты, следом приходит смута.', '無理に市場を開けば売り台が壊され、混乱が続いた。', 'Der Markt öffnet mit Gewalt; Stände werden zerschlagen, und Wirrwarr folgt.'),
 q('和谈让步', '和談讓步', 'Negotiate and concede', 'Договориться и уступить', '和解に譲歩', 'Verhandeln und nachgeben'),
 q('与行首谈判，许以小惠换复市。', '與行首談判，許以小惠換復市。', 'Talk with the masters; trade small favors for reopening.', 'Переговоры со старостами; малые уступки взамен открытия.', '頭たちと交渉し、ささやかな譲歩と引き換えに再開を求める。', 'Mit den Meistern reden; kleine Zugeständnisse für die Wiedereröffnung.'),
 q('复市之日，列国商人叹{kingdom}宽仁有度。', '復市之日，列國商人嘆{kingdom}寬仁有度。', 'On reopening day, foreign merchants praise {kingdom}\'s temperate mercy.', 'В день открытия иноземные купцы хвалят сдержанное милосердие {kingdom}.', '再開の日、列国の商人は{kingdom}の寛容な処置を嘆じた。', 'Am Tag der Wiedereröffnung rühmen auswärtige Kaufleute {kingdom}\'s maßvolle Güte.')
)
# ===== 公用与教化 =====
ev('street_lamp', 2,
 ('夜路灯火', '夜路燈火', 'The Street Lamps', 'Уличные фонари', '街灯の灯火', 'Die Straßenlampen'),
 ('暮色渐浓，街巷盗案频发，坊正请{king}拨款添设路灯，冬夜之费不赀。', '暮色漸濃，街巷盜案頻發，坊正請{king}撥款添設路燈，冬夜之費不貲。', 'Dusk grows heavy and burglaries spread; wardens beg {king} to fund more street lamps — no small winter cost.', 'Сумерки густеют, кражи множатся; старосты просят {king} дать казну на новые фонари — недешево зимой.', '夕闇が深まり盗難が多発、坊の長は{king}に街灯の増設を懇願する——冬の費用は軽くない。', 'Die Dämmerung wird schwer, Diebstähle nehmen zu; die Vorsteher bitten {king} um Geld für Lampen — kein geringer Winterpreis.'),
 q('拨款点灯', '撥款點燈', 'Fund the lamps', 'Дать на фонари', '灯り金を出す', 'Die Lampen finanzieren'),
 q('拨出库银，全城高灯低亮。', '撥出庫銀，全城高燈低亮。', 'Pay from the treasury; every street burns bright.', 'Платить из казны; улицы горят светом.', '国庫から出せば、街々は灯で明るく照らされる。', 'Aus der Kasse zahlen; alle Straßen leuchten hell.'),
 q('灯火彻夜，盗匪敛迹，夜市自此复得安宁。', '燈火徹夜，盜匪斂跡，夜市自此復得安寧。', 'Lamps burn till dawn; thieves vanish and the night lanes grow calm.', 'Фонари горят до утра; воры исчезают, ночные улицы спокойны.', '灯が夜通しともり、盗人も姿を消し、夜の路地に安らぎが戻った。', 'Die Lampen brennen bis zum Morgen; Diebe verschwinden, die Gassen werden ruhig.'),
 q('从长计议', '從長計議', 'Defer the plan', 'Отложить', '継続審議', 'Aufschieben'),
 q('路灯非急务，先搁置下月再议。', '路燈非急務，先擱置下月再議。', 'Lamps can wait; table it until next month.', 'Фонари подождут; отложить до следующего месяца.', '灯りは急を要さぬ、来月まで棚上げする。', 'Die Lampen warten; auf den nächsten Monat vertagen.'),
 q('灯未点，盗案仍频，士民窃议王室吝啬。', '燈未點，盜案仍頻，士民竊議王室吝嗇。', 'No lamps, more thefts; the people whisper of a stingy crown.', 'Фонарей нет, краж больше; народ шепчется о скупой короне.', '灯はともらず盗難は続き、民は王の吝嗇をひそかに噂する。', 'Keine Lampen, mehr Diebstähle; das Volk munkelt von einem knausrigen Hof.')
)
ev('schooling_plan', 2,
 ('兴办学塾', '興辦學塾', 'The Schooling Plan', 'План школ', '学塾の開設', 'Der Schulplan'),
 ('举子与富商联名上书，请{king}兴办学塾开蒙童子，然拨款之数惊人。', '舉子與富商聯名上書，請{king}興辦學塾開蒙童子，然撥款之數驚人。', 'Scholars and merchants petition {king} to found village schools, but the sum required is staggering.', 'Учёные и купцы просят {king} открыть школы для детей, но сумма ошеломляет.', '学者と豪商が連名で、{king}に童子向けの学塾開設を願い出る——だが掛かる金は莫大だ。', 'Gelehrte und Kaufleute bitten {king} um Dorfschulen, doch die benötigte Summe ist gewaltig.'),
 q('拨款兴学', '撥款興學', 'Fund the schools', 'Профинансировать школы', '学塾に支出', 'Die Schulen finanzieren'),
 q('国帑拨出，书声将起于闾巷。', '國帑撥出，書聲將起於閭巷。', 'The treasury pays; reading will ring through the alleys.', 'Казна платит; в переулках зазвучит чтение.', '国帑を出せば、巷に読書の声が響き始める。', 'Die Kasse zahlt; Lesen wird durch die Gassen klingen.'),
 q('学塾开学，童子习字，士林称颂{kingdom}文教之兴。', '學塾開學，童子習字，士林稱頌{kingdom}文教之興。', 'Schools open, children learn; the literati hail {kingdom}\'s new age of learning.', 'Школы открыты, дети учатся; книжники славят {kingdom} за расцвет учёности.', '学塾が開き童子が字を学び、士林は{kingdom}の文教の興隆を讃えた。', 'Die Schulen öffnen, Kinder lernen; die Gelehrten preisen {kingdom}\'s neues Zeitalter der Bildung.'),
 q('暂缓兴学', '暫緩興學', 'Delay the schools', 'Отложить школы', '学塾を延期', 'Die Schulen verschieben'),
 q('粮仓尚紧，学塾之议压后再议。', '糧倉尚緊，學塾之議壓後再議。', 'The granaries are thin; shelve the school plan.', 'Амбары пустоваты; отложить школьный план.', '穀倉は窮屈で、学塾の議は後回しにする。', 'Die Speicher sind knapp; den Schulplan zur Seite legen.'),
 q('学子群起失望，士人聚议，街巷怨声渐起。', '學子群起失望，士人聚議，街巷怨聲漸起。', 'Pupils despair, scholars gather; grumbling rises through the lanes.', 'Ученики унывают, учёные собираются; по улицам растёт ропот.', '子弟は失望し、士人らが集い、巷に不満の声が高まる。', 'Die Schüler verzweifeln, Gelehrte versammeln sich; durch die Gassen steigt das Murren.')
)
ev('surgery_house', 2,
 ('外科医馆', '外科醫館', 'The Surgery House', 'Дом хирургии', '外科医館', 'Das Chirurgenhaus'),
 ('游方郎中求见{king}，愿设一所外科医馆，开膛接骨，而医资库银所费不菲。', '遊方郎中求見{king}，願設一所外科醫館，開膛接骨，而醫資庫銀所費不菲。', 'A wandering barber-surgeon begs {king} to endow a surgery house — cut and bone-setting, at no small treasury cost.', 'Странствующий хирург просит {king} основать дом хирургии — вскрытие и костоправство, недешево для казны.', '渡りの外科医が{king}に外科医館の創設を願う——開腹や接骨の技、国庫の負担は小さくない。', 'Ein wandernder Wundarzt bittet {king} um ein Chirurgenhaus — Schnitt und Knochenrichten, kein geringer Kassenposten.'),
 q('设馆资助', '設館資助', 'Endow the house', 'Дать на дом хирургии', '医館に出資', 'Das Haus stiften'),
 q('拨银建馆，郎中立誓悬壶。', '撥銀建館，郎中立誓懸壺。', 'Silver for the house; the surgeon vows to serve.', 'Серебро на дом; хирург клянётся служить.', '銀を出して館を建てれば、外科医は懸壺を誓う。', 'Silber für das Haus; der Wundarzt schwört zu dienen.'),
 q('医馆开张，断骨得续，一时城中颂声不绝。', '醫館開張，斷骨得續，一時城中頌聲不絕。', 'The house opens; broken bones mend and praises ring through town.', 'Дом открыт; кости срастаются, по городу звучат хвалы.', '医館が開き折れた骨も繋がり、町に称賛の声が絶えない。', 'Das Haus öffnet; Knochen heilen, und Lob klingt durch die Stadt.'),
 q('婉言辞谢', '婉言辭謝', 'Kindly refuse', 'Вежливо отказать', '丁重に断る', 'Höflich ablehnen'),
 q('开膛之技惊世骇俗，恐伤风化，暂且不允。', '開膛之技驚世駭俗，恐傷風化，暫且不允。', 'Cutting flesh shocks custom; the crown declines for now.', 'Разрезы пугают нравы; корона пока отказывает.', '切り開く術は世を驚かせかねず、まずは認めない。', 'Das Aufschneiden schockiert die Sitte; die Krone lehnt vorerst ab.'),
 q('医馆未立，伤者仍求巫卜，坊间为之叹息。', '醫館未立，傷者仍求巫卜，坊間為之嘆息。', 'No house, no surgery; the wounded turn to charms, and the town sighs.', 'Дома нет, раненые идут к знахарям; город вздыхает.', '医館は建たず、怪我人は呪い祈りに頼り、町は嘆息する。', 'Kein Haus; die Verwundeten wenden sich an Amulette, und die Stadt seufzt.')
)
ev('bath_fee', 2,
 ('汤池新税', '湯池新稅', 'The Bath Fee', 'Налог на бани', '湯の新税', 'Das Badegeld'),
 ('汤池浴场车马喧嚣，税吏进言{king}：每浴收钱二文，岁入可观，可裕国用。', '湯池浴場車馬喧囂，稅吏進言{king}：每浴收錢二文，歲入可觀，可裕國用。', 'Baths swarm with traffic; a revenue clerk whispers to {king}: two coppers per bath would fatten the crown.', 'Бани кишат народом; сборщик шепчет {king}: по две монеты за помывку — и казна растолстеет.', '湯屋は車馬で賑わい、税吏が{king}に進言する——一浴につき二文、国の歳入になる。', 'Die Bäder quellen über; ein Zöllner flüstert {king}: zwei Kupfer pro Bad würden die Krone mästen.'),
 q('轻税抽厘', '輕稅抽釐', 'Light bath tax', 'Лёгкий налог', '軽い湯税', 'Leichte Badesteuer'),
 q('每浴抽一文，库银渐丰而民不怨。', '每浴抽一文，庫銀漸豐而民不怨。', 'One copper a bath: the till fills, no grumbling.', 'По монете за помывку: казна полнеет без ропота.', '一浴一銭なら、民の不満もなく国庫が増える。', 'Ein Kupfer pro Bad: die Kasse füllt sich, ohne Murren.'),
 q('抽厘既行，国库渐盈，浴客依旧熙攘往来。', '抽釐既行，國庫漸盈，浴客依舊熙攘往來。', 'The levy runs; the treasury swells, bathers still come and go.', 'Сбор идёт; казна пухнет, купальщики ходят как прежде.', '徴収が始まり国庫が太り、湯客は相変わらず出入りする。', 'Die Abgabe läuft; die Kasse schwillt, die Badegäste kommen weiter.'),
 q('重税苛征', '重稅苛征', 'Heavy bath tax', 'Тяжёлый налог', '重い湯税', 'Schwere Badesteuer'),
 q('加倍课税，浴客纷纷却步，市集哗然。', '加倍課稅，浴客紛紛卻步，市集譁然。', 'Double the levy; bathers stay away and the town objects.', 'Двойной сбор; купальщики уходят, город ропщет.', '税を倍にすれば浴客が足を遠のけ、町が騒然とする。', 'Die Abgabe verdoppeln; Badegäste bleiben aus, die Stadt protestiert.'),
 q('苛税加身，浴场冷清，市民聚于坊市滋事。', '苛稅加身，浴場冷清，市民聚於坊市滋事。', 'Under the harsh tax the baths empty and townsfolk gather to riot.', 'При тяжёлом налоге бани пустеют; горожане собираются буянить.', '過酷な税で湯屋は閑古鳥、市民が集まって騒ぎを起こす。', 'Unter der harten Steuer leeren sich die Bäder; Bürger rotten sich zum Aufruhr.')
)
ev('beggar_raid', 2,
 ('流民抢米', '流民搶米', 'The Beggars\' Raid', 'Налёт нищих', '流民の米奪い', 'Der Bettlerüberfall'),
 ('贫富悬殊已久，城中流民啸聚抢米，坊市哗然，市吏束手——{king}须定处置。', '貧富懸殊已久，城中流民嘯聚搶米，坊市譁然，市吏束手——{king}須定處置。', 'The wealth gap has widened long enough; vagrants swarm the town and seize grain, the market is aghast, and {king} must answer.', 'Пропасть между богатыми и бедными растёт давно; бродяги громят город и хватают зерно, рынок в ужасе — {king} должен ответить.', '富の差は長く広がり、浮浪者が町に集まり米を奪う——市場は騒然とし、市吏は手をこまねき、{king}の裁断が要る。', 'Die Kluft zwischen arm und reich ist lange breit; Vagabunden stürmen die Stadt und raffen Korn, der Markt entsetzt sich — {king} muss antworten.'),
 q('严惩首恶', '嚴懲首惡', 'Punish the ringleaders', 'Карать зачинщиков', '首謀者を厳罰', 'Die Rädelsführer strafen'),
 q('搜捕流民首领处斩，余众四散，怨气暗积。', '搜捕流民首領處斬，餘眾四散，怨氣暗積。', 'Hunt the heads, cut them down; the rest scatter, resentment gathers.', 'Поймать и казнить главарей; остальные разбегаются, злоба копится.', '首領を捕らえて斬れば、残りは散り、怨みが暗く積もる。', 'Die Köpfe fangen und richten; der Rest zerstreut sich, Groll staut sich.'),
 q('首恶虽诛，饥民愈愤，街市骚乱随之而来。', '首惡雖誅，饑民愈憤，街市騷亂隨之而來。', 'The ringleaders die, but hunger stokes fury and street turmoil follows.', 'Главари казнены, но голод раздувает ярость, и улицы бурлят.', '首謀者は斬られたが飢えた民はますます憤り、街に騒乱が続く。', 'Die Rädelsführer sterben, doch der Hunger heizt die Wut, und Unruhen folgen.'),
 q('开仓赈济', '開倉賑濟', 'Open the granaries', 'Открыть амбары', '倉を開き救う', 'Die Speicher öffnen'),
 q('开仓放米，赈济流民，库银为之一耗。', '開倉放米，賑濟流民，庫銀為之一耗。', 'Hand out grain from the stores; the treasury takes the loss.', 'Раздать зерно из амбаров; казна понесёт убыток.', '倉を開いて米を施せば、国庫がその分減る。', 'Korn aus den Speichern verteilen; die Kasse trägt den Verlust.'),
 q('米粮既施，饥民立散，市集自此重归宁静。', '米糧既施，饑民立散，市集自此重歸寧靜。', 'Grain is doled out, the hungry melt away, and the market finds peace.', 'Зерно роздано, голодные расходятся, рынок обретает покой.', '米が施され飢えた者たちは立ち去り、市場に平穏が戻った。', 'Das Korn ist verteilt, die Hungrigen schwinden, und der Markt findet Ruhe.')
)
ev('orphan_home', 2,
 ('孤贫养院', '孤貧養院', 'The Orphan Home', 'Приют для сирот', '孤児の養い院', 'Das Waisenhaus'),
 ('冬寒将至，孤儿与老丐露宿桥下，里正请{king}设养院收容孤贫，以全仁政。', '冬寒將至，孤兒與老丐露宿橋下，里正請{king}設養院收容孤貧，以全仁政。', 'Winter nears; orphans and old beggars sleep under the bridge, and the ward prefect begs {king} for a home in mercy.', 'Зима близко; сироты и старые нищие спят под мостом, и староста просит {king} о приюте из милосердия.', '冬が迫り、孤児と老いた乞食が橋の下で野宿する——里正が{king}に仁政を全うすべく養い院を願う。', 'Der Winter naht; Waisen und alte Bettler schlafen unter der Brücke, der Vorsteher bittet {king} aus Gnade um ein Heim.'),
 q('设院收养', '設院收養', 'Found the home', 'Открыть приют', '養い院を設ける', 'Das Heim gründen'),
 q('拨银设院，收养孤贫，棉衣粥饭俱全。', '撥銀設院，收養孤貧，棉衣粥飯俱全。', 'Silver grounds a home: clothes, porridge, and shelter for all.', 'Серебро открывает приют: одежда, каша и кров для всех.', '銀を出して院を設け、孤児らに綿入れと粥を揃える。', 'Silber gründet ein Heim: Kleidung, Brei und Obdach für alle.'),
 q('孤贫各得其所，桥下寒骨渐少，民心安定。', '孤貧各得其所，橋下寒骨漸少，民心安定。', 'The destitute find shelter; cold bones under the bridge thin out, and hearts settle.', 'Бедняки нашли кров; меньше коченеющих под мостом, сердца успокаиваются.', '貧しい者らは住まいを得、橋下の凍える姿が減り、人心が落ち着く。', 'Die Armen finden Obdach; die kalten Knochen unter der Brücke werden weniger, die Herzen beruhigen sich.'),
 q('责成乡里', '責成鄉里', 'Defer to the parish', 'Свалить на приход', '郷里に任せる', 'Aufs Dorf abschieben'),
 q('命乡里自行劝养，官府不费一文。', '命鄉里自行勸養，官府不費一文。', 'Bid the parishes to care; the crown spends not a coin.', 'Пусть приходы пекутся сами; корона не тратит монеты.', '郷に任せよと命じ、官は一文も出さない。', 'Den Gemeinden überlassen; die Krone zahlt keinen Heller.'),
 q('乡里推诿不休，孤儿仍在霜雪中瑟缩。', '鄉里推諉不休，孤兒仍在霜雪中瑟縮。', 'Parishes shuffle the duty; orphans still shiver in frost and snow.', 'Приходы отнекиваются; сироты всё ещё дрожат в мороз.', '郷は責任をなすり合い、孤児は霜雪の中でまだ震えている。', 'Die Gemeinden drücken sich; Waisen zittern weiter im Frost.')
)
ev('widow_fund', 2,
 ('寡妇恤金', '寡婦恤金', 'The Widow\'s Fund', 'Фонд вдов', '寡婦の恤金', 'Der Witwenfonds'),
 ('战殁者遗孀逾百，扶幼啼饥，众臣请{king}定抚恤之法，以安军心、慰亡魂。', '戰歿者遺孀逾百，扶幼啼饑，眾臣請{king}定撫恤之法，以安軍心、慰亡魂。', 'Over a hundred widows of the fallen, children at their skirts, ask {king} for relief and a settled law to soothe the army.', 'Более сотни вдов погибших с детьми у юбок просят {king} о помощи и законе, чтобы успокоить армию.', '戦死者の未亡人が百人余り、幼子を抱えて飢えを訴える——{king}に軍心を安んじる撫恤の法を求める声だ。', 'Über hundert Witwen der Gefallenen, Kinder am Rock, bitten {king} um Beistand und ein festes Gesetz zur Beruhigung des Heeres.'),
 q('薄恤聊慰', '薄恤聊慰', 'Small relief', 'Малая помощь', 'ささやかな下賜', 'Kleine Hilfe'),
 q('按户发银少许，聊解眼前饥寒。', '按戶發銀少許，聊解眼前饑寒。', 'A little silver per household, enough to blunt hunger and cold.', 'Немного серебра на двор, чтобы притупить голод и холод.', '戸ごとに少々の銀を渡し、当座の飢え寒さをしのぐ。', 'Wenig Silber je Haus, um Hunger und Kälte zu dämpfen.'),
 q('微薄之恤发下，寡妇得缓眉睫，众议平平。', '微薄之恤發下，寡婦得緩眉睫，眾議平平。', 'The thin relief reaches the widows; brows ease, talk stays muted.', 'Скудная помощь пришла; лица смягчились, разговоры тихи.', 'わずかな恤金が下り、寡婦らは一息つくが、世評はどちらともつかない。', 'Die dünne Hilfe trifft ein; die Mienen erleichtern sich, die Rede bleibt gedämpft.'),
 q('厚建义庄', '厚建義莊', 'Build the charity hall', 'Построить богадельню', '義荘を厚く建てる', 'Das Armenhaus errichten'),
 q('巨资别建义庄，赡养遗孤，惠泽及于海外。', '巨資別建義莊，贍養遺孤，惠澤及於海外。', 'Endow a grand charity hall for orphans — fame reaching beyond the seas.', 'Дать на богадельню для сирот — слава уйдёт за моря.', '巨費を投じて義荘を建て、遺児を養い、恵みを海の外にまで及ぼす。', 'Ein großes Armenhaus stiften — der Ruhm reicht über die Meere.'),
 q('义庄落成，列国使节皆叹{kingdom}慈恤之风。', '義莊落成，列國使節皆嘆{kingdom}慈恤之風。', 'The hall stands; envoys of every state praise {kingdom}\'s compassionate spirit.', 'Богадельня стоит; послы всех держав славят милосердие {kingdom}.', '義荘が落成し、列国の使節は{kingdom}の慈しみの風を称えた。', 'Das Haus steht; Gesandte aller Staaten rühmen {kingdom}\'s barmherzigen Geist.')
)
ev('madman_case', 2,
 ('疯汉闹市', '瘋漢鬧市', 'The Madman\'s Case', 'Дело безумца', '狂人の騒乱', 'Der Irrenfall'),
 ('一疯汉持斧立于市集，斩伤数人，围观者人人自危，急报{king}定夺。', '一瘋漢持斧立於市集，斬傷數人，圍觀者人人自危，急報{king}定奪。', 'A maniac with an axe by the market has cut down several folk; the crowd trembles, and word races to {king}.', 'Безумец с топором у рынка порубил людей; толпа трепещет, весть мчится к {king}.', '斧を持った狂人が市場に立ち、数人を斬り倒した——群衆は震え、急報が{king}の元へ届く。', 'Ein Rasender mit Axt beim Markt hat mehrere Leute niedergeschlagen; die Menge zittert, die Kunde eilt zu {king}.'),
 q('当街斩决', '當街斬決', 'Execute him publicly', 'Казнить всенародно', '公開斬首', 'Öffentlich hinrichten'),
 q('以正典刑，当众斩决，血溅市集。', '以正典刑，當眾斬決，血濺市集。', 'Justice on the spot: the axe falls before the crowd.', 'Справедливость на месте: топор падает при народе.', '公衆の面前で斬って見せれば、血が市場に飛び散る。', 'Gerechtigkeit auf der Stelle: das Beil fällt vor der Menge.'),
 q('当街行刑，血渍未干，市集之中竟起骚乱。', '當街行刑，血漬未乾，市集之中竟起騷亂。', 'The blood is barely dry before riots break out by the stalls.', 'Кровь не высохла, а у прилавков уже бунтуют.', '血の跡も乾かぬうちに、市場で騒乱が持ち上がった。', 'Kaum ist das Blut trocken, brechen am Markt Unruhen aus.'),
 q('收拘疯人', '收拘瘋人', 'Cage the madman', 'Заключить безумца', '狂人を拘束', 'Den Rasenden verwahren'),
 q('锁入窄巷围屋，延医调治，静待其愈。', '鎖入窄巷圍屋，延醫調治，靜待其癒。', 'Chain him in a cell, call a physician, and wait for sanity.', 'Запереть в келье, позвать лекаря и ждать рассудка.', '路地の囲い屋に鎖し、医師を呼び、正気を待つ。', 'In eine Zelle legen, einen Arzt rufen und auf Vernunft warten.'),
 q('疯人幽闭，刀斧之危遂解，市集复得安宁。', '瘋人幽閉，刀斧之危遂解，市集復得安寧。', 'The madman is caged, the axe-threat ends, and the market settles.', 'Безумец заперт, угроза топора миновала, рынок успокоился.', '狂人は閉じ込められ、斧の危険は去り、市場が落ち着く。', 'Der Rasende ist verwahrt, die Axtgefahr vorbei, der Markt beruhigt sich.')
)
ev('rumor_wave', 2,
 ('谣言四起', '謠言四起', 'The Rumor Wave', 'Волна слухов', '噂の波', 'Die Gerüchtwelle'),
 ('市井暗传{kingdom}王仓亏空、赋税将加，流言如风过巷，人心浮动，米价先跃。', '市井暗傳{kingdom}王倉虧空、賦稅將加，流言如風過巷，人心浮動，米價先躍。', 'Whispers spread that {kingdom}\'s royal stores are empty and taxes will rise; rumor gusts through the lanes and rice jumps first.', 'Шепчутся, что королевские амбары {kingdom} пусты и налоги вырастут; слух гуляет по переулкам, и рис скачет первым.', '{kingdom}の王倉が底を突き税が上がると囁かれ、風のように巷を抜け、米の値段が真っ先に跳ねる。', 'Man flüstert, {kingdom}\'s königliche Speicher seien leer und Steuern stiegen; das Gerücht fegt durch die Gassen, der Reis springt zuerst.'),
 q('听之任之', '聽之任之', 'Let it run', 'Пустить на самотёк', '放っておく', 'Laufen lassen'),
 q('流言愈炽，坊间争相添油加醋。', '流言愈熾，坊間爭相添油加醋。', 'The whisper swells; every lane adds its own spice.', 'Шёпот растёт; каждый переулок добавляет перцу.', '噂はますます燃え、巷ごとに尾ひれが付く。', 'Das Flüstern schwillt; jede Gasse fügt eigene Würze hinzu.'),
 q('谣言酿成恐慌，乡民聚众哄闹，骚乱渐起。', '謠言釀成恐慌，鄉民聚眾哄鬧，騷亂漸起。', 'Panic ferments; villagers crowd the streets and turmoil rises.', 'Паника бродит; селяне толпятся, смута поднимается.', '噂が恐慌を醸し、村人らが集まって騒ぎ、騒乱が起き始める。', 'Panik gärt; Dorfleute drängen sich, und Unruhe steigt.'),
 q('张榜辟谣', '張榜闢謠', 'Post the truth', 'Опубликовать правду', '榜で噂を打ち消す', 'Die Wahrheit anschlagen'),
 q('出榜晓谕，严斥谣言，明示仓禀实数。', '出榜曉諭，嚴斥謠言，明示倉稟實數。', 'Publish the facts, refute the rumor, show the true grain stocks.', 'Опубликовать факты, опровергнуть слух, показать запасы зерна.', '榜を出して流言を斥け、倉の実数を明示する。', 'Die Fakten anschlagen, das Gerücht widerlegen, die Kornbestände zeigen.'),
 q('榜文既下，流言渐息，市井人心稍得安定。', '榜文既下，流言漸息，市井人心稍得安定。', 'The edict lands; the whispers fade, and the town breathes easier.', 'Указ вышел; шёпот затихает, городу дышится легче.', '榜文が下り、噂は次第に消え、町の人心が少し落ち着く。', 'Das Edikt erscheint; das Flüstern ebbt ab, die Stadt atmet auf.')
)
ev('book_market', 2,
 ('雅士书市', '雅士書市', 'The Book Market', 'Книжная ярмарка', '書市の雅集', 'Die Büchermesse'),
 ('各地文士携孤本手稿云集，恳请{king}开书市以通文脉，传教于列国。', '各地文士攜孤本手稿雲集，懇請{king}開書市以通文脈，傳教於列國。', 'Scholars gather from every land with rare manuscripts, begging {king} to open a book market and spread letters abroad.', 'Книжники съезжаются с редкостными рукописями, прося {king} открыть книжную ярмарку и распространить письмена.', '各地の文人が孤本の写本を携えて集い、{king}に書市を開いて文脈を通し、列国に教えを伝えよと懇願する。', 'Gelehrte ziehen mit seltenen Handschriften herbei und bitten {king}, eine Büchermesse zu öffnen und Bildung weithin zu verbreiten.'),
 q('开市迎文', '開市迎文', 'Open the fair', 'Открыть ярмарку', '書市を開く', 'Die Messe öffnen'),
 q('辟地设市，免费聚书，文士云集。', '闢地設市，免費聚書，文士雲集。', 'Clear the ground, gather books free; scholars throng.', 'Расчистить место, собрать книги даром; книжники толпятся.', '広場を整え書物を集めれば、文人が雲のように集う。', 'Platz räumen, Bücher frei zusammentragen; Gelehrte strömen heran.'),
 q('书市大开，抄本远传，列国慕{kingdom}文风来投。', '書市大開，抄本遠傳，列國慕{kingdom}文風來投。', 'The fair opens; copies travel far, and foreign minds turn to {kingdom}.', 'Ярмарка открыта; списки идут далеко, чужие умы обращаются к {kingdom}.', '書市が開かれ写本は遠くへ伝わり、列国は{kingdom}の文風に憧れる。', 'Die Messe öffnet; Abschriften wandern weit, fremde Geister wenden sich {kingdom} zu.'),
 q('谢绝雅集', '謝絕雅集', 'Decline the fair', 'Отказать ярмарке', '雅集を断る', 'Die Messe ablehnen'),
 q('书市劳民伤财，旨意令其速散，另择良机。', '書市勞民傷財，旨意令其速散，另擇良機。', 'A fair costs more than it earns; bid them disperse and wait.', 'Ярмарка дороже, чем приносит; велеть разойтись и подождать.', '書市は費が嵩むとし、早く散じ、機を改めよと命じる。', 'Die Messe kostet mehr als sie einbringt; sie mögen auseinandergehen und warten.'),
 q('雅集散去，文士怅然，书市之议就此遂寝。', '雅集散去，文士悵然，書市之議就此遂寢。', 'The gathering disperses; scholars sigh, and the plan sleeps.', 'Собравшиеся разошлись; книжники вздыхают, план спит.', '雅集は散り、文人は憮然とし、書市の議は立ち消えた。', 'Die Versammlung zerstreut sich; die Gelehrten seufzen, der Plan schläft.')
)
ev('playhouse', 2,
 ('梨园新戏', '梨園新戲', 'The Playhouse', 'Театр', '芝居小屋', 'Das Schauspielhaus'),
 ('城中名伶排演新戏，万人空巷观之，请{king}拨款重修戏楼以壮声色。', '城中名伶排演新戲，萬人空巷觀之，請{king}撥款重修戲樓以壯聲色。', 'A famous troupe stages a new play and the whole town pours out to watch; the actors beg {king} for a grander house.', 'Знаменитая труппа ставит новую пьесу, и город высыпал смотреть; актёры просят {king} о новом театре.', '人気役者が新劇を演じれば町を挙げて観に来る——一座は{king}に上等な芝居小屋の再建を願う。', 'Eine berühmte Truppe spielt ein neues Stück, und die Stadt strömt hinaus; die Schauspieler bitten {king} um ein prächtigeres Haus.'),
 q('拨款重修', '撥款重修', 'Fund the rebuild', 'Дать на ремонт', '再建の金', 'Den Neubau finanzieren'),
 q('库银拨出，戏楼翻新，笙歌彻夜。', '庫銀撥出，戲樓翻新，笙歌徹夜。', 'Treasury funds the renovation; music rings till dawn.', 'Казна даёт на ремонт; музыка звенит до рассвета.', '国庫から金を出し楼を新調すれば、歌が夜通し響く。', 'Die Kasse finanziert den Umbau; Musik klingt bis zum Morgen.'),
 q('新楼开锣，观众如潮，市声因之愈见繁盛。', '新樓開鑼，觀眾如潮，市聲因之愈見繁盛。', 'The new stage opens to waves of spectators; the town hums louder.', 'Новая сцена открыта волнами зрителей; город гудит громче.', '新築の楼で開演し客が波のように押し寄せ、町の声がいっそう賑やかになる。', 'Die neue Bühne öffnet vor Zuschauerwellen; die Stadt summt lauter.'),
 q('不禁杂戏', '不禁雜戲', 'Let it run wild', 'Не сдерживать', '雑劇を野放し', 'Wild laufen lassen'),
 q('杂戏无禁，喧阗彻夜，坊间叫好声浪四涌。', '雜戲無禁，喧闐徹夜，坊間叫好聲浪四湧。', 'No curbs, no curfew; the cries of the crowd roll through the night.', 'Ни запретов, ни комендантского часа; крики толпы катятся всю ночь.', '雑劇に禁令を掛けず、夜通し喧騒が続き、拍手の波が四隅に涌く。', 'Keine Schranken, keine Sperrstunde; der Ruf der Menge wogt durch die Nacht.'),
 q('看客汹汹，竞相叫板，戏楼外竟起斗殴骚乱。', '看客洶洶，競相叫板，戲樓外竟起鬥毆騷亂。', 'The crowd boils over; rival fans brawl and riots break out at the doors.', 'Толпа кипит; соперники дерутся, у дверей вспыхивают беспорядки.', '観客が沸騰し、派閥の客同士が殴り合い、桟敷の外は騒乱と化した。', 'Die Menge kocht über; rivalisierende Fans prügeln sich, vor den Türen bricht Unruhe aus.')
)
# ===== 节庆与俚俗 =====
ev('festival_curfew', 2,
 ('节庆宵禁', '節慶宵禁', 'The Festival Curfew', 'Праздничный комендантский час', '祭の宵禁', 'Die Ausgangssperre zum Fest'),
 ('元夜将至，或云盗匪欲趁节而动，{king}须决：罢灯宵禁，还是任民狂欢。', '元夜將至，或云盜匪欲趁節而動，{king}須決：罷燈宵禁，還是任民狂歡。', 'The lantern night nears; word says thieves mean to move. {king} must choose: curfew, or let the people revel.', 'Ночь фонарей близка; шепчут, что воры готовятся. {king} решает: комендантский час или народное гулянье.', '灯りの夜が近づき、盗人どもが動くとの報——{king}は宵禁か、民の歓楽を許すかを決めねばならぬ。', 'Die Laternenacht naht; man sagt, Diebe wollten handeln. {king} muss wählen: Ausgangssperre oder Volksfest.'),
 q('严行宵禁', '嚴行宵禁', 'Enforce the curfew', 'Ввести комендантский час', '宵禁を敷く', 'Die Sperre verhängen'),
 q('灯火罢歇，巡丁加哨，百姓扫兴。', '燈火罷歇，巡丁加哨，百姓掃興。', 'Lamps out, extra watch; the people lose their joy.', 'Фонари гаснут, стражи больше; народ теряет радость.', '灯を消し夜番を増やせば、民はしょんぼりする。', 'Lampen aus, mehr Wachen; das Volk verliert seine Freude.'),
 q('宵禁之下，百姓怨言载道，街巷暗有骚动。', '宵禁之下，百姓怨言載道，街巷暗有騷動。', 'Under the curfew, complaints fill the lanes and unrest stirs.', 'При комендантском часе жалобы полнят переулки, бродит недовольство.', '宵禁の下、民の不満が道に溢れ、巷で騒動がひそかに揺れる。', 'Unter der Sperre füllen Klagen die Gassen, und Unruhe gärt.'),
 q('全城欢庆', '全城歡慶', 'Let them celebrate', 'Позволить гулять', '祝祭を許す', 'Feiern erlauben'),
 q('罢禁撤哨，任民彻夜歌舞观灯。', '罷禁撤哨，任民徹夜歌舞觀燈。', 'Lift both curfew and watch; let them sing till dawn.', 'Снять комендантский час и стражу; пусть поют до рассвета.', '禁を解き哨を引いて、民に夜通し歌い騒がせる。', 'Sperre und Wachen aufheben; sie dürfen bis zum Morgen singen.'),
 q('火树银花，列国使臣皆羡{kingdom}民风之盛。', '火樹銀花，列國使臣皆羨{kingdom}民風之盛。', 'Trees of fire, flowers of silver: envoys of all states envy {kingdom}\'s festal spirit.', 'Деревья огней, серебро цветов: послы всех держав завидуют веселью {kingdom}.', '火の木と銀の花——列国の使節は{kingdom}の祭りの豊かさを羨んだ。', 'Feuerbäume, Silberblumen: Gesandte aller Staaten beneiden {kingdom}\'s Festgeist.')
)
ev('boat_regatta', 2,
 ('端午竞渡', '端午競渡', 'The Dragon Regatta', 'Регата драконов', '龍舟競べ', 'Die Drachenbootregatta'),
 ('端午将近，水手磨桨待发，或请{king}出资办赛，或邀列国劲旅共竞河上。', '端午將近，水手磨槳待發，或請{king}出資辦賽，或邀列國勁旅共競河上。', 'Dragon Boat Day nears; crews ready their oars. {king} may fund a local race or invite foreign rivals to the river.', 'Праздник драконьих лодок близко; гребцы точат вёсла. {king} может дать на гонку или пригласить чужие флотилии.', '端午が近づき、漕ぎ手が櫓を研ぐ——{king}は金を出して催すか、列国の強豪を川に招くか。', 'Das Drachenbootfest naht; die Ruderer wetzen die Riemen. {king} kann ein Rennen finanzieren oder Rivalen auf den Strom laden.'),
 q('出资办赛', '出資辦賽', 'Fund the race', 'Дать на гонку', 'レースに出資', 'Das Rennen finanzieren'),
 q('库银助赛，龙舟齐发，鼓声震岸。', '庫銀助賽，龍舟齊發，鼓聲震岸。', 'Treasury backs the race; dragon boats launch, drums shake the shore.', 'Казна поддерживает гонку; ладьи стартуют, барабаны сотрясают берег.', '国庫の金で舟が一斉に漕ぎ出し、太鼓の音が岸を震わせる。', 'Die Kasse trägt das Rennen; die Drachenboote starten, Trommeln erschüttern das Ufer.'),
 q('竞渡落幕，岸上尽欢，水域风情一时无两。', '競渡落幕，岸上盡歡，水域風情一時無兩。', 'The regatta ends in cheers; the waterfront has never seen such joy.', 'Регата кончается криками; набережная не знала такой радости.', '競べは歓声のうちに終わり、水辺の風情は比類がない。', 'Die Regatta endet im Jubel; das Ufer sah nie solche Freude.'),
 q('广邀诸国', '廣邀諸國', 'Invite all nations', 'Пригласить все державы', '列国を招く', 'Alle Nationen einladen'),
 q('张书邀列国舟师共竞，胜者得厚赏。', '張書邀列國舟師共競，勝者得厚賞。', 'Letters summon foreign fleets; the victor takes a rich prize.', 'Грамоты зовут чужие флотилии; победитель получает богатый приз.', '列国の舟団を招き、勝者に厚い報賞を出す。', 'Briefe rufen fremde Flotten; der Sieger nimmt einen reichen Preis.'),
 q('使节挥桨竟舟，列国齐赞{kingdom}气度，邦谊愈笃。', '使節揮槳競舟，列國齊讚{kingdom}氣度，邦誼愈篤。', 'Envoys pull the oars; every state praises {kingdom}\'s bold grace, and friendship deepens.', 'Послы гребут; все державы хвалят щедрую стать {kingdom}, дружба крепнет.', '使節らが櫓を漕ぎ、列国は{kingdom}の気骨を一斉に讃え、邦誼が深まった。', 'Gesandte ziehen die Riemen; alle Staaten rühmen {kingdom}\'s kühne Anmut, die Freundschaft wächst.')
)
ev('flower_walk', 2,
 ('花朝踏青', '花朝踏青', 'The Flower Walk', 'Праздник цветов', '花見の散策', 'Der Blumenfestspaziergang'),
 ('花朝初至，满城踏青，有司请{king}设花会遣兴，或邀列国宾客观游同乐。', '花朝初至，滿城踏青，有司請{king}設花會遣興，或邀列國賓客觀遊同樂。', 'The Flower Festival opens; the city walks the green. Officers beg {king} for a flower feast — and foreign guests.', 'Праздник цветов настал; город идёт по зелени. Чиновники просят {king} о цветочном пире и заморских гостях.', '花朝が来て街を挙げて野外に遊ぶ——役人が{king}に花の会を設け、列国の客を招くよう願う。', 'Das Blumenfest bricht an; die Stadt wandert ins Grüne. Beamte bitten {king} um ein Blumenfest mit fremden Gästen.'),
 q('设会赏花', '設會賞花', 'Host the flower feast', 'Устроить цветочный пир', '花の会を催す', 'Das Blumenfest ausrichten'),
 q('置酒设花会，倾城踏青，红袖香车。', '置酒設花會，傾城踏青，紅袖香車。', 'Wine and flowers; the whole town strolls in bright dresses.', 'Вино и цветы; весь город гуляет в нарядных платьях.', '酒を置き花の会を開けば、町を挙げて遊び、紅の袖と香の車が溢れる。', 'Wein und Blumen; die ganze Stadt spaziert in hellen Gewändern.'),
 q('花会既散，春气愈浓，城中上下一时相悦。', '花會既散，春氣愈濃，城中上下一時相悅。', 'The feast ends; spring deepens, and the town is glad.', 'Пир окончен; весна густеет, город рад.', '花会が終わり春の気はますます濃く、町はしばし和んだ。', 'Das Fest endet; der Frühling wird tiefer, die Stadt ist froh.'),
 q('邀使同游', '邀使同遊', 'Invite envoys', 'Пригласить послов', '使節を招く', 'Gesandte einladen'),
 q('延请列国使节同游，共赏花朝胜景。', '延請列國使節同遊，共賞花朝勝景。', 'Ask foreign envoys to walk among the blossoms.', 'Позвать чужих послов гулять среди цветов.', '列国の使節を招き、花朝の名勝を共に眺める。', 'Fremde Gesandte einladen, durch die Blüten zu gehen.'),
 q('使节尽兴而归，列国对{kingdom}春情流连称羡。', '使節盡興而歸，列國對{kingdom}春情流連稱羨。', 'The envoys return delighted; nations linger over {kingdom}\'s spring.', 'Послы вернулись в восторге; державы залюбовались весной {kingdom}.', '使節は満足して帰り、列国は{kingdom}の春に心を残し憧れた。', 'Die Gesandten kehren entzückt heim; die Staaten schwärmen von {kingdom}\'s Frühling.')
)
ev('wedding_boom', 2,
 ('婚嫁成风', '婚嫁成風', 'The Wedding Boom', 'Свадебный бум', '婚礼ブーム', 'Der Hochzeitsboom'),
 ('{kingdom}境内婚嫁成风，聘礼宴席铺张成习，市集绸缎酒肆获利颇丰，有司请课税之。', '{kingdom}境內婚嫁成風，聘禮宴席鋪張成習，市集綢緞酒肆獲利頗豐，有司請課稅之。', 'Weddings sweep {kingdom}: betrothal gifts and feasts grow lavish, the silk-and-wine trades grow fat, and officers ask for a tax.', 'Свадьбы охватили {kingdom}: подарки и пиры пышнеют, торговля шёлком и вином жиреет, чиновники просят налога.', '{kingdom}では婚礼が風となり、結納の宴は豪華の一途、絹と酒の市がうるおい、役人が課税を請う。', 'Hochzeiten ergreifen {kingdom}: Brautgabe und Fest werden üppig, Seide und Wein schwellen an, Beamte bitten um eine Steuer.'),
 q('顺市抽厘', '順市抽釐', 'Tax the boom', 'Собрать с бума', '市場に課釐', 'Den Boom besteuern'),
 q('婚市设官征税，一纸厘金入公库。', '婚市設官徵稅，一紙釐金入公庫。', 'A clerk at the wedding market; a light levy fills the till.', 'Чиновник на свадебном рынке; лёгкий сбор полнит казну.', '婚礼の市に役人を置き、わずかな釐金を公庫へ。', 'Ein Beamter am Hochzeitsmarkt; eine leichte Abgabe füllt die Kasse.'),
 q('婚嫁愈盛，厘金积少成多，国库日见充裕。', '婚嫁愈盛，釐金積少成多，國庫日見充裕。', 'The boom swells, the levy grows, and the treasury fills.', 'Бум растёт, сбор растёт, казна полнеет.', '婚礼はますます盛んになり、釐金が積もって国庫がうるおう。', 'Der Boom wächst, die Abgabe wächst, die Kasse füllt sich.'),
 q('任礼自然', '任禮自然', 'Leave custom alone', 'Не мешать обычаю', '慣習に任せる', 'Den Brauch lassen'),
 q('不扰婚俗，任聘礼宴会自行其是。', '不擾婚俗，任聘禮宴會自行其是。', 'Let the custom run; no court hand in betrothal or feast.', 'Обычай идёт своим путём; двор не вмешивается.', '婚礼の風習に干渉せず、結納も宴もままに任せる。', 'Den Brauch laufen lassen; der Hof greift nicht ein.'),
 q('婚俗如旧，市面热闹如常，府库无所增减。', '婚俗如舊，市面熱鬧如常，府庫無所增減。', 'The custom endures, the market hums, the till neither grows nor shrinks.', 'Обычай живёт, рынок гудит, казна ни растёт, ни худеет.', '婚俗は昔のまま、市は賑やかで、庫は増えも減りもしない。', 'Der Brauch währt, der Markt summt, die Kasse wächst noch schrumpft.')
)
ev('funeral_custom', 2,
 ('丧礼奢靡', '喪禮奢靡', 'The Funeral Custom', 'Пышные похороны', '葬儀の風習', 'Der Leichenprunk'),
 ('富家办丧，纸马蔽路、素幔满城，贫者竞相效尤致倾家荡产——{king}闻报。', '富家辦喪，紙馬蔽路、素幔滿城，貧者競相效尤致傾家蕩產——{king}聞報。', 'Rich funerals paper the roads with paper horses and shroud the town; the poor ruin themselves in imitation — {king} hears.', 'Богатые похороны: бумажные кони на дорогах, саваны на городе; бедняки разоряются, подражая, — {king} слышит.', '金持ちの葬儀で紙馬が道を塞ぎ白幕が町を覆う——貧しい家も倣って傾家する、と{king}に報が届く。', 'Reiche Leichenzüge pflastern die Straßen mit Papierpferden und hüllen die Stadt ein; Arme ruinieren sich nach — {king} erfährt es.'),
 q('崇奢尽哀', '崇奢盡哀', 'Honor the pomp', 'Чтить пышность', '奢りを寿ぐ', 'Den Prunk ehren'),
 q('准其大事铺张，以示孝道，遂成风气。', '准其大事鋪張，以示孝道，遂成風氣。', 'Allow the grand display as filial piety; it becomes the fashion.', 'Разрешить пышность как сыновнюю почтительность; это становится модой.', '盛大に飾るのを孝行と許せば、たちまち風習となる。', 'Die große Schau als Kindespflicht erlauben; sie wird zur Mode.'),
 q('竞相厚葬，贫户纷纷典田，街巷怨声暗起。', '競相厚葬，貧戶紛紛典田，街巷怨聲暗起。', 'All outdo the last funeral; the poor pawn their fields and murmurs rise.', 'Все перебивают друг друга; бедняки закладывают поля, поднимается ропот.', '祭儀は競って豪華になり、貧家は田を質に入れ、巷に不満がひそかに湧く。', 'Alle überbieten einander; die Armen versetzen ihre Felder, und Murren steigt.'),
 q('姑置不问', '姑置不問', 'Leave it be', 'Оставить как есть', 'ひとまず不問', 'Lassen'),
 q('风俗之事，官府不宜深究，且由它去。', '風俗之事，官府不宜深究，且由它去。', 'Custom is not for the court to pry into; let it pass.', 'Обычай не для двора; пусть идёт.', '風習のことは官が口を挟むべきでない、ままにさせよ。', 'Bräuche sind nicht Sache des Hofes; lasst sie ziehen.'),
 q('奢风未改，贫者依旧典田卖屋，朝议喟然。', '奢風未改，貧者依舊典田賣屋，朝議喟然。', 'The pomp continues; the poor still pawn fields and houses, and the court sighs.', 'Пышность идёт; бедняки всё ещё закладывают поля и дома, двор вздыхает.', '風習は変わらず、貧家は相変わらず田や家を質入れ、朝議は嘆息する。', 'Der Prunk geht weiter; Arme verpfänden weiter Felder und Häuser, der Hof seufzt.')
)
ev('food_contest', 2,
 ('庖厨争味', '庖廚爭味', 'The Food Contest', 'Кулинарный спор', '料理の競べ', 'Der Kochwettstreit'),
 ('名厨云集斗味，香气满城，或请{king}出资设宴，或以五味待列国嘉宾。', '名廚雲集鬥味，香氣滿城，或請{king}出資設宴，或以五味待列國嘉賓。', 'Master cooks gather to duel in flavor; aroma fills the town. {king} may fund a feast — or host foreign guests.', 'Мастера поварни съезжаются дуэлью вкусов; аромат полнит город. {king} может дать на пир или принять чужих гостей.', '名厨が集い味を競い、香りが街に満ちる——{king}は宴の金を出すか、五味で列国の客を迎えるか。', 'Meisterköche duellieren sich im Geschmack; Duft füllt die Stadt. {king} kann ein Fest finanzieren oder fremde Gäste bewirten.'),
 q('设宴斗味', '設宴鬥味', 'Fund the feast', 'Дать на пир', '宴を設ける', 'Das Fest finanzieren'),
 q('出资设席，庖厨献技，老饕半城欣动。', '出資設席，庖廚獻技，老饕半城欣動。', 'Silver for the board; the cooks perform, half the town delights.', 'Серебро на стол; повара показывают себя, половина города в восторге.', '金を出して席を設ければ、料理人たちが技を見せ、食いしん坊の半町が沸く。', 'Silber für die Tafel; die Köche glänzen, halb die Stadt jubelt.'),
 q('佳肴斗罢，齿颊余味三日，城中讴歌不绝。', '佳餚鬥罷，齒頰餘味三日，城中謳歌不絕。', 'The dishes duel and linger three days; praise rings through town.', 'Блюда сразились и витают три дня; хвалы звенят по городу.', '料理は競い合い、余韻三日、町に讃歌が絶えない。', 'Die Speisen klingen drei Tage nach; Lob hallt durch die Stadt.'),
 q('广宴列国', '廣宴列國', 'Feast all nations', 'Пировать со всеми', '列国を宴する', 'Alle Nationen bewirten'),
 q('大张筵席，遍请列国使臣尝五味。', '大張筵席，遍請列國使臣嘗五味。', 'A great spread; envoys of every state taste the five flavors.', 'Великое застолье; послы всех держав вкушают пять вкусов.', '大いに筵を張り、列国の使節を招いて五味を味わわせる。', 'Ein großes Mahl; Gesandte aller Staaten kosten die fünf Geschmäcke.'),
 q('使臣尽醉而归，列国皆慕{kingdom}饮食之妙。', '使臣盡醉而歸，列國皆慕{kingdom}飲食之妙。', 'The envoys leave tipsy; every state covets {kingdom}\'s culinary art.', 'Послы уходят пьяные; каждая держава завидует кухне {kingdom}.', '使節は酔い帰り、列国は{kingdom}の食の妙を憧れた。', 'Die Gesandten gehen trunken; jeder Staat beneidet {kingdom}\'s Kochkunst.')
)
ev('night_market', 2,
 ('夜市初开', '夜市初開', 'The Night Market', 'Ночной рынок', '夜市の開設', 'Der Nachtmarkt'),
 ('市吏献策{king}：开夜市至三更，货殖可增、税利可观，然喧闹滋事亦堪忧。', '市吏獻策{king}：開夜市至三更，貨殖可增、稅利可觀，然喧鬧滋事亦堪憂。', 'An officer bids {king} keep the market open till the third watch: trade will swell and tax flow — yet brawls may follow.', 'Чиновник предлагает {king} торговать до третьей стражи: торговля пойдёт в гору, налог польётся — но и драки не за горами.', '市吏が{king}に献策する——夜の三更まで市を開けば商いは増え税の利も見込める、ただ喧騒の厄も憂えると。', 'Ein Beamter rät {king}, den Markt bis zur dritten Wache zu öffnen: Handel blüht, Steuer fließt — doch Raufereien auch.'),
 q('开市至夜', '開市至夜', 'Open till night', 'Торговать до ночи', '夜まで開く', 'Bis in die Nacht öffnen'),
 q('宵禁稍弛，灯火市集通明，百货争售。', '宵禁稍弛，燈火市集通明，百貨爭售。', 'Curfew relaxes; the lantern-lit market hums, every good for sale.', 'Запрет смягчён; рынок при фонарях гудит, всё на продажу.', '宵禁を少し緩め、灯りの市を明々と輝かせ、百貨を競って売る。', 'Die Sperre lockert; der laternhelle Markt summt, alle Waren feil.'),
 q('夜市兴旺，税银入库，{kingdom}商气愈盛。', '夜市興旺，稅銀入庫，{kingdom}商氣愈盛。', 'The night market thrives, tax silver pours in, and {kingdom}\'s trade roars.', 'Ночной рынок процветает, серебро льётся, торговля {kingdom} ревёт.', '夜市は繁昌し、税銀が庫に入り、{kingdom}の商いの気がますます盛んになる。', 'Der Nachtmarkt gedeiht, Steuersilber fließt, und {kingdom}\'s Handel dröhnt.'),
 q('放纵喧闹', '放縱喧鬧', 'Let it riot', 'Дать бушевать', '騒ぎ放題', 'Toben lassen'),
 q('不加约束，赌戏杂耍俱起，彻夜喧嚣。', '不加約束，賭戲雜耍俱起，徹夜喧囂。', 'No reins on the night: gambling, juggling, din till dawn.', 'Без узды: азарт, жонглёры, гам до рассвета.', '手綱をかけず、賭けも芸も夜通しの喧騒が続く。', 'Ohne Zügel: Glück, Gaukler, Lärm bis zum Morgen.'),
 q('酒后斗殴盈街，巡丁不暇，市面骚乱四起。', '酒後鬥毆盈街，巡丁不暇，市面騷亂四起。', 'Drunk brawls fill the lanes, watchmen swamped, disorder rules.', 'Пьяные потасовки на улицах, стражи не успевают, беспорядок правит.', '酔った喧嘩が街に溢れ、夜番が手薄で、市の騒乱が続く。', 'Blaue Nacht prügeln sich in den Gassen, die Wachen ertrinken, Unruhe regiert.')
)
ev('water_taxi', 2,
 ('水巷小舟', '水巷小舟', 'The Water Taxi', 'Водное такси', '水上の渡し舟', 'Das Wassertaxi'),
 ('巷河交错而舟渡寥寥，有人请{king}放舟船为渡，每人三文，官收其税。', '巷河交錯而舟渡寥寥，有人請{king}放舟船為渡，每人三文，官收其稅。', 'Canals thread the town but boats are few. Someone begs {king} to license ferries: three coppers a head, the crown takes its share.', 'Каналы ширятся, а лодок мало. Некий проситель зовёт {king} допустить перевозчиков: три монеты с головы, доля короне.', '水路は入り組むのに渡し舟は少ない——{king}に舟を出させ、一人三文、官が税を取るよう願う者がいる。', 'Kanäle durchziehen die Stadt, doch Boote sind rar. Einer bittet {king}, Fähren zuzulassen: drei Kupfer je Kopf, die Krone nimmt Anteil.'),
 q('放船抽税', '放船抽稅', 'License and tax', 'Допустить и облагать', '舟を許し税を取る', 'Lizenziert besteuern'),
 q('准许小船载客，每渡抽税三文。', '准許小船載客，每渡抽稅三文。', 'Permit small boats to ply; three coppers tax per crossing.', 'Разрешить лодкам ходить; три монеты налога с переправы.', '小舟に客を乗せてよいと許し、渡し一回につき三文の税を取る。', 'Kleine Boote zulassen; drei Kupfer Steuer je Überfahrt.'),
 q('小舟如织，渡资入税，河巷交通日渐便利。', '小舟如織，渡資入稅，河巷交通日漸便利。', 'Boats weave like looms; fees enter the tax, and the canals turn busy.', 'Лодки снуют, как челноки; сборы идут в казну, каналы оживают.', '小舟が機のように行き交い、渡し賃は税となり、水路の便が日に日に良くなる。', 'Boote weben wie Webstühle; Fährgeld fließt in die Steuer, die Kanäle beleben sich.'),
 q('官渡如旧', '官渡如舊', 'Keep the old ferries', 'Оставить старые паромы', '官渡のまま', 'Bei den alten Fähren bleiben'),
 q('仍用旧日官渡，不纳私舟之议。', '仍用舊日官渡，不納私舟之議。', 'Keep the crown ferries; no private boats allowed.', 'Остаются коронные паромы; частникам нельзя.', '昔ながらの官渡を使い、私舟の議は入れない。', 'Bei den Kronfähren bleiben; keine Privatboote.'),
 q('官渡依旧，舟子略生怨言，往来行程稍涩。', '官渡依舊，舟子略生怨言，往來行程稍澀。', 'The old ferries keep; the boatmen grumble and crossings flag.', 'Старые паромы; лодочники ворчат, переправы хромают.', '官渡は昔のまま、船頭は不満で、渡しはやや滞る。', 'Die alten Fähren bleiben; die Bootsleute murren, die Überfahrten schleppen.')
)
ev('chimney_cleaner', 2,
 ('烟囱扫灰', '煙囪掃灰', 'The Chimney Cleaner', 'Трубочист', '煙突掃除', 'Der Schornsteinfeger'),
 ('城中烟囱年久积灰，时有火患，有人请{king}立扫烟囱之役，按户收钱。', '城中煙囪年久積灰，時有火患，有人請{king}立掃煙囪之役，按戶收錢。', 'Soot thickens in the chimneys and fires burst out; one begs {king} for a sweep service, charged per house.', 'Сажа густеет в трубах, вспыхивают пожары; некто просит {king} о трубочистах — плата с дома.', '街の煙突は煤が積もり、折々火災が起きる——{king}に煤掃除の役を立て、戸ごとに銭を取るよう願う者がある。', 'Ruß setzt die Kamine zu, Brände brechen aus; jemand bittet {king} um eine Kehrpflicht, bezahlt je Haus.'),
 q('立役收费', '立役收費', 'Found and charge', 'Учредить и брать', '役を立て銭を取る', 'Einrichten und berechnen'),
 q('设扫灰之役，按户收费，官抽其成。', '設掃灰之役，按戶收費，官抽其成。', 'A sweep service, paid per house, the crown takes its share.', 'Служба трубочистов с платы с дома; корона берёт долю.', '煤掃除の役を置き、戸ごとに銭を取り、官がその分を徴収する。', 'Ein Kehrdienst, bezahlt je Haus, die Krone nimmt ihren Teil.'),
 q('烟囱既净，火患大减，役钱源源入公库。', '煙囪既淨，火患大減，役錢源源入公庫。', 'Chimneys clean, fires fade, and the fees fill the coffer.', 'Трубы чисты, пожары слабеют, сборы полнят казну.', '煙突はきれいになり火災がぐっと減り、役銭も公庫に入る。', 'Die Kamine sind sauber, das Feuer ebbt, die Gebühren füllen die Truhe.'),
 q('各家自理', '各家自理', 'Each house its own', 'Пусть сами', '各家まかせ', 'Jedes Haus selbst'),
 q('不设官役，任各家自行清扫。', '不設官役，任各家自行清掃。', 'No service; let each house sweep itself.', 'Без службы; пусть каждый дом чистит сам.', '官の役は立てず、各々が自ら掃くに任せる。', 'Kein Dienst; jedes Haus kehre selbst.'),
 q('屋户各自扫烟，火患时起时灭，无改于旧。', '屋戶各自掃煙，火患時起時滅，無改於舊。', 'Houses sweep as they please; fires come and go, nothing changes.', 'Дома чистят как попало; пожары то вспыхивают, то гаснут, всё по-старому.', '各家折々に掃くだけで、火災は時々起きては消える——昔と変わらない。', 'Die Häuser kehren nach Belieben; Brände kommen und gehen, nichts ändert sich.')
)
ev('brew_wave', 2,
 ('新酿潮起', '新釀潮起', 'The Brew Wave', 'Пивная волна', '醸造の波', 'Die Brauwelle'),
 ('新酿甘冽，城中酒肆人满为患，或请{king}课酒税以裕国，或惧酗酒生事。', '新釀甘冽，城中酒肆人滿為患，或請{king}課酒稅以裕國，或懼酗酒生事。', 'A sweet new brew; alehouses overflow. {king} may tax the drink to enrich the state — or fear the drunkards.', 'Новый сладкий напиток; пивные переполнены. {king} может обложить его налогом — или бояться пьяниц.', '新醸は甘く芳醇で、酒場は人であふれる——{king}は酒税を課して国を裕にするか、酔っ払いの乱を憂うるか。', 'Ein süßer neuer Sud; die Schänken quellen über. {king} kann den Trunk besteuern — oder die Trunkenbolde fürchten.'),
 q('课以酒税', '課以酒稅', 'Tax the brew', 'Обложить налогом', '酒税を課す', 'Den Sud besteuern'),
 q('酒肆登记课税，斗酒得钱入公库。', '酒肆登記課稅，斗酒得錢入公庫。', 'Register the taverns, tax the ale; each cask pays the crown.', 'Зарегистрировать таверны, обложить эль; каждая бочка платит короне.', '酒場を登録して酒に税を課し、一樽ごとに公庫へ銭を入れる。', 'Die Schänken registrieren, das Bier besteuern; jedes Fass zahlt der Krone.'),
 q('酒税既课，日进金铢，市井醺然而不失序。', '酒稅既課，日進金銖，市井醺然而不失序。', 'The levy runs; gold flows daily, and the town carouses in order.', 'Сбор идёт; золото течёт ежедневно, и город напивается в порядке.', '酒税がかかり日々金が入り、町は酔いながらも秩序を保つ。', 'Die Abgabe fließt; Gold kommt täglich, und die Stadt zechelt in Ordnung.'),
 q('不禁酗酒', '不禁酗酒', 'No ban on drink', 'Не запрещать пьянство', '酔いを禁じず', 'Trinken nicht verbieten'),
 q('罢去禁酒旧令，任肆中痛饮达旦。', '罷去禁酒舊令，任肆中痛飲達旦。', 'Lift the old wine bans; let them drink till dawn.', 'Снять прежние винные запреты; пусть пьют до рассвета.', '昔の禁酒令を解き、店で夜明けまで痛飲させる。', 'Die alten Weinverbote aufheben; sie mögen bis zum Morgen trinken.'),
 q('醉汉横卧街头，斗殴滋事，骚乱迭起不休。', '醉漢橫臥街頭，鬥毆滋事，騷亂迭起不休。', 'Drunkards sprawl in the lanes, fists fly, and riots repeat.', 'Пьяницы валяются в переулках, кулаки летят, бунты повторяются.', '酔っ払いが路頭に横たわり、喧嘩が絶えず、騒乱が繰り返される。', 'Trunkenbolde liegen in den Gassen, Fäuste fliegen, Unruhen wiederholen sich.')
)
# ===== 行商与货物 =====
ev('silk_weavers', 2,
 ('云锦之艺', '雲錦之藝', 'The Silk Weavers', 'Шёлкоткачи', '絹織りの匠', 'Die Seidenweber'),
 ('织户新成云锦一匹，五色灿如星汉，有司相询，请{king}献于列国以通好。', '織戶新成雲錦一匹，五色燦如星漢，有司相詢，請{king}獻於列國以通好。', 'The weavers finish a bolt of cloud-brocade, five colors blazing like the Milky Way, and ask {king} to gift it abroad.', 'Ткачи соткали шёлк-облако, пять красок горят, как Млечный Путь; просят {king} одарить им державы.', '織戸が雲錦を新たに織り上げ、五色は天の川のよう——役人が折をみて{king}に列国へ献じて国交を結べと請う。', 'Die Weber vollenden ein Stück Wolkendamast, fünf Farben glühen wie die Milchstraße, und bitten {king}, es nach außen zu schenken.'),
 q('献锦通好', '獻錦通好', 'Gift the brocade', 'Дарить парчу', '錦を献じる', 'Den Damast verschenken'),
 q('挑上等云锦，装匣分赠列国宫廷。', '挑上等雲錦，裝匣分贈列國宮廷。', 'Choose the finest bolt, pack it, and gift it to foreign courts.', 'Выбрать лучший отрез, уложить и подарить чужим дворам.', '上等の雲錦を選び、箱に納め、列国の宮廷に贈る。', 'Das feinste Stück wählen, verpacken und fremden Höfen schenken.'),
 q('锦使四出，列国皆叹{kingdom}工巧，邦谊愈笃。', '錦使四出，列國皆嘆{kingdom}工巧，邦誼愈篤。', 'Brocade envoys fan out; every state marvels at {kingdom}\'s craft, and friendship deepens.', 'Посланцы с парчой разошлись; державы дивятся мастерству {kingdom}, дружба крепнет.', '錦の使が四方へ出て、列国は{kingdom}の技を嘆賞し、邦誼がより深まる。', 'Damastboten ziehen aus; alle Staaten staunen über {kingdom}\'s Kunst, die Freundschaft wächst.'),
 q('藏之内府', '藏之內府', 'Keep it at court', 'Оставить при дворе', '内府に藏す', 'Am Hof behalten'),
 q('云锦天工，宜藏于内府自赏。', '雲錦天工，宜藏於內府自賞。', 'Heaven-woven stuff; keep it in the palace to admire.', 'Небесное творение; оставить во дворце на любование.', '天工の雲錦、内府に藏して自ら楽しむのがいい。', 'Himmelswerk; es im Palast bewundern.'),
 q('锦入深宫，织户获赏，然声名不出国门。', '錦入深宮，織戶獲賞，然聲名不出國門。', 'The brocade enters the palace; the weavers are paid, but the fame stays home.', 'Парча уходит во дворец; ткачи вознаграждены, но слава остаётся дома.', '錦は深宮に入り、織戸は褒美を得たが、名は国門を出ない。', 'Der Damast zieht in den Palast; die Weber sind bezahlt, doch der Ruhm bleibt daheim.')
)
ev('paper_dealers', 2,
 ('纸商议价', '紙商議價', 'The Paper Dealers', 'Торговцы бумагой', '紙商の会', 'Die Papierhändler'),
 ('纸价昂贵，文房商贾结成公会，愿缴新税一厘，换{king}许其会员定价。', '紙價昂貴，文房商賈結成公會，願繳新稅一釐，換{king}許其會員定價。', 'Paper runs dear; the stationers form a guild and offer {king} a new tax for the right to fix prices.', 'Бумага дорога; торговцы сбились в гильдию и предлагают {king} новый налог за право ставить цены.', '紙の値は高く、文房の商人が組合を結成——{king}に新税を納めて値決めを許してほしいと願う。', 'Papier ist teuer; die Papierhändler gründen eine Zunft und bieten {king} eine neue Steuer für das Preisrecht.'),
 q('许会抽税', '許會抽稅', 'Grant and tax', 'Разрешить и облагать', '組合を許し税を取る', 'Zulassen und besteuern'),
 q('允其设会定价，官抽纸税二厘。', '允其設會定價，官抽紙稅二釐。', 'Let the guild set prices; the crown takes two on the paper.', 'Позволить гильдии ставить цены; корона берёт два процента с бумаги.', '組合の値決めを許し、官は紙に二厘の税を取る。', 'Der Zunft die Preise lassen; die Krone nimmt zwei vom Papier.'),
 q('纸税入柜，纸价渐稳，公库为之日渐丰盈。', '紙稅入櫃，紙價漸穩，公庫為之日漸豐盈。', 'The paper tax flows in, prices steady, and the coffer fattens.', 'Бумажный налог течёт, цены ровны, казна жиреет.', '紙税が入り、値段は安定し、公庫が少しずつ肥える。', 'Die Papiersteuer fließt, die Preise stehen, die Truhe schwillt.'),
 q('不允设会', '不允設會', 'Refuse the guild', 'Отказать гильдии', '組合を許さない', 'Die Zunft ablehnen'),
 q('不准商贾结会，纸价听其自然。', '不准商賈結會，紙價聽其自然。', 'No guild for the stationers; let paper find its own price.', 'Никакой гильдии; пусть бумага найдёт свою цену.', '商人の結社は許さず、紙の値は自然に任せる。', 'Keine Zunft der Händler; das Papier finde seinen Preis.'),
 q('纸价涨落无常，书坊间有苦言，官不加闻。', '紙價漲落無常，書坊間有苦言，官不加聞。', 'Paper prices seesaw; the bookshops grumble, and the court turns a deaf ear.', 'Цены на бумагу скачут; книжные лавки ворчат, двор не слышит.', '紙の値は上下し、書肆に苦言あるも、官は聞かぬふりをする。', 'Die Papierpreise schwanken; die Buchläden murren, der Hof hört nicht hin.')
)
ev('potters_quarrel', 2,
 ('陶坊争窑', '陶坊爭窯', 'The Potters\' Quarrel', 'Спор гончаров', '陶工の争い', 'Der Töpferstreit'),
 ('两坊陶匠争一窑火，各聚门徒掷瓦相斗，市集为之哗然，急报{king}定夺。', '兩坊陶匠爭一窯火，各聚門徒擲瓦相鬥，市集為之譁然，急報{king}定奪。', 'Two kiln-houses fight over one furnace; apprentices hurl tiles — the market is aghast, and word races to {king}.', 'Два гончарных двора дерутся за одну печь; подмастерья мечут черепицу — рынок в ужасе, весть мчится к {king}.', '二つの陶坊が一つの窯を争い、弟子たちが瓦を投げ合う——市場は騒然とし、急報が{king}の元へ届く。', 'Zwei Töpferhöfe streiten um einen Ofen; Lehrlinge schleudern Kacheln — der Markt entsetzt sich, die Kunde eilt zu {king}.'),
 q('任其相争', '任其相爭', 'Let them fight', 'Пусть дерутся', '争うに任せる', 'Sie kämpfen lassen'),
 q('不置一词，任两坊自决胜负。', '不置一詞，任兩坊自決勝負。', 'Say nothing; let the two houses settle it themselves.', 'Промолчать; пусть дворы решают сами.', '一言も差し挟まず、二坊の決着に任せる。', 'Nichts sagen; die zwei Höfe selbst entscheiden lassen.'),
 q('瓦砾横飞，半条街巷遭殃，市面骚乱渐炽。', '瓦礫橫飛，半條街巷遭殃，市面騷亂漸熾。', 'Tiles fly; half the lane is wrecked and turmoil spreads.', 'Черепица летит; пол-переулка в щепках, смута ширится.', '瓦が乱れ飛び、半町が被害に遭い、騒乱が次第に激しくなる。', 'Kacheln fliegen; die halbe Gasse liegt in Trümmern, Unruhe greift um sich.'),
 q('判坊息争', '判坊息爭', 'Umpire the dispute', 'Рассудить спор', '判を下して収める', 'Den Streit schlichten'),
 q('依窑契断归，各责罚金，勒令息争。', '依窯契斷歸，各責罰金，勒令息爭。', 'Rule by the kiln deed, fine both houses, and bid them stop.', 'Судить по грамоте на печь, оштрафовать оба двора и велеть прекратить.', '窯の証文で帰属を決め、双方に罰金を課し、争いをやめさせる。', 'Nach dem Ofenbrief entscheiden, beide Höfe büßen lassen und Einhalt gebieten.'),
 q('窑判既定，两坊罢斗，陶市自此重归宁静。', '窯判既定，兩坊罷鬥，陶市自此重歸寧靜。', 'The ruling stands; the houses drop their tiles and the potteries fall quiet.', 'Решение принято; дворы бросают черепицу, гончарный рынок затих.', '窯の裁きが決まり、両坊は手を止め、陶の市に静けさが戻る。', 'Der Spruch steht; die Höfe legen die Kacheln nieder, der Töpfermarkt verstummt.')
)
ev('lamp_oil', 2,
 ('灯油告罄', '燈油告罄', 'The Lamp Oil', 'Масло для ламп', '燈油の窮乏', 'Das Lampenöl'),
 ('冬夜苦长，官灯油亦将尽，坊正请{king}拨库银购油，否则长街黑黢、盗影四伏。', '冬夜苦長，官燈油亦將盡，坊正請{king}撥庫銀購油，否則長街黑黢、盜影四伏。', 'Long winter nights; the lantern oil runs short. The warden begs {king} to buy oil, or the streets go black and thieves gather.', 'Долгие зимние ночи; лампадное масло на исходе. Староста просит {king} купить масло — иначе улицы погаснут и соберутся воры.', '冬の夜は長く、官の燈油も尽きかけ——坊正は{king}に油を買うよう、さもなくば長街が真っ暗になり盗人の影が潜むと願う。', 'Lange Winternächte; das Lampenöl geht zur Neige. Der Vorsteher bittet {king}, Öl zu kaufen, sonst wird die Straße schwarz und Diebe sammeln sich.'),
 q('拔款购油', '撥款購油', 'Buy the oil', 'Купить масло', '油を買う', 'Das Öl kaufen'),
 q('库银购油，官灯复明，长街如昼。', '庫銀購油，官燈復明，長街如晝。', 'Treasury buys oil; the lanterns relight, the long street glows.', 'Казна покупает масло; фонари снова горят, длинная улица сияет.', '国庫の銀で油を買えば官灯が再びともり、長い街が昼のように明るい。', 'Die Kasse kauft Öl; die Laternen brennen wieder, die lange Straße glüht.'),
 q('灯火彻夜，百姓称便，夜行之人日渐增多。', '燈火徹夜，百姓稱便，夜行之人日漸增多。', 'Lamps burn all night; the people rejoice and night walkers multiply.', 'Фонари горят всю ночь; народ радуется, ночных путников больше.', '灯りは夜通し輝き、民は便利を喜び、夜歩く人が増える。', 'Die Lampen brennen die ganze Nacht; das Volk frohlockt, Nachtschreiter mehren sich.'),
 q('罢灯省费', '罷燈省費', 'Let the lamps die', 'Погасить фонари', '灯を止めて節約', 'Die Lampen löschen'),
 q('停灯省油，街市入夜皆黑，盗影幢幢。', '停燈省油，街市入夜皆黑，盜影幢幢。', 'Cut the oil; at night the town is black and shadows slide.', 'Прекратить масло; ночью город чёрен, тени скользят.', '油を止めれば、夜の町は真っ暗で、影がうろつく。', 'Das Öl streichen; nachts ist die Stadt schwarz, Schatten gleiten.'),
 q('路灯熄灭，暗巷盗起，民怨转炽，聚而滋事。', '路燈熄滅，暗巷盜起，民怨轉熾，聚而滋事。', 'Lights out, thieves in the dark lanes; angry townsfolk gather to riot.', 'Огни погасли, воры в тёмных переулках; горожане собираются буянить.', '灯が消え、暗い路地に盗人が出、民の怨みが募って騒ぎを起こす。', 'Lichter aus, Diebe in den dunklen Gassen; wütende Bürger rotten sich zum Aufruhr.')
)
ev('salt_caravan', 2,
 ('盐路商队', '鹽路商隊', 'The Salt Caravan', 'Соляной караван', '塩の隊商', 'Die Salzkarawane'),
 ('盐道驼铃阵阵，商队欲假道{kingdom}，或抽厚税裕库，或薄征以示邻国之谊。', '鹽道駝鈴陣陣，商隊欲假道{kingdom}，或抽厚稅裕庫，或薄徵以示鄰國之誼。', 'Salt-road camel bells ring: a caravan asks passage through {kingdom}. Take a heavy toll — or light, as neighborly grace.', 'Соляная дорога звенит бубенцами: караван просит пути через {kingdom}. Взять высокую пошлину — или малую, по-соседски.', '塩の道にらくだの鈴が響く——隊商は{kingdom}の通過を求める。重い税を取るか、隣国の誼みに薄く征するか。', 'Salzstraßen-Handglocken klingen: eine Karawane bittet um Durchzug durch {kingdom}. Schwerer Zoll — oder leicht, als Nachbargunst.'),
 q('抽盐科税', '抽鹽科稅', 'Levy the salt toll', 'Взять соляную пошлину', '塩税を科す', 'Salz Zoll erheben'),
 q('过境盐船按引抽税，金库立见其益。', '過境鹽船按引抽稅，金庫立見其益。', 'Tax each salt lot passing; the treasury sees the gain at once.', 'Обложить каждую партию соли; казна сразу видит выгоду.', '通過する塩の引ごとに税を取れば、国庫はすぐにその益を見る。', 'Jede Salzcharge besteuern; die Kasse sieht sofort den Gewinn.'),
 q('盐税入柜，引钱如山，{kingdom}府库充盈。', '鹽稅入櫃，引錢如山，{kingdom}府庫充盈。', 'Salt silver piles up; {kingdom}\'s coffer runs full to the brim.', 'Соляное серебро растёт горой; казна {kingdom} полна до краёв.', '塩税が庫に入り、引銭は山と積もり、{kingdom}の庫は満ちあふれる。', 'Salzsilber türmt sich; {kingdom}\'s Truhe läuft über.'),
 q('薄税通盐', '薄稅通鹽', 'Light toll, open road', 'Лёгкая пошлина', '薄税で通す', 'Leichter Zoll, offene Straße'),
 q('薄征盐税，驰驿通商，以结列国欢心。', '薄徵鹽稅，馳驛通商，以結列國歡心。', 'A light toll, open the road, and win foreign hearts.', 'Малая пошлина, открытая дорога — и чужие сердца наши.', '塩税を薄くして駅路を開き、列国の心を結ぶ。', 'Leichter Zoll, offene Straße und fremde Herzen gewinnen.'),
 q('盐价平而邻道通，列国交口赞{kingdom}之谊。', '鹽價平而鄰道通，列國交口讚{kingdom}之誼。', 'Salt stays cheap and the road stays open; every state sings {kingdom}\'s friendship.', 'Соль дешева, дорога открыта; каждая держава поёт о дружбе {kingdom}.', '塩は安く道は開かれ、列国は口々に{kingdom}の誼みを讃えた。', 'Das Salz bleibt billig, die Straße offen; alle Staaten singen von {kingdom}\'s Freundschaft.')
)
ev('spice_spree', 2,
 ('香料之风', '香料之風', 'The Spice Spree', 'Пряный бум', '香辛料ブーム', 'Der Gewürzboom'),
 ('商船新捎异域胡椒丁香，富户争购席间生香，{king}闻税吏之言可富国库。', '商船新捎異域胡椒丁香，富戶爭購席間生香，{king}聞稅吏之言可富國庫。', 'Merchant ships land pepper and cloves; the rich race to buy, and the tax clerk whispers to {king} of profit.', 'Корабли привезли перец и гвоздику; богачи рвутся покупать, а сборщик шепчет {king} о прибыли.', '商船が異国の胡椒と丁子をもたらし、金持ちが競って買う——税吏が{king}に国庫のうるおいをささやく。', 'Handelsschiffe landen Pfeffer und Nelken; die Reichen stürmen den Kauf, der Zöllner flüstert {king} vom Gewinn.'),
 q('抽香设课', '抽香設課', 'Tax the spices', 'Обложить пряности', '香に課税', 'Die Gewürze besteuern'),
 q('胡椒丁香入市抽税，香市一举鼎沸。', '胡椒丁香入市抽稅，香市一舉鼎沸。', 'Tax pepper and clove at the gate; the spice market erupts.', 'Обложить перец и гвоздику у ворот; пряный рынок кипит.', '胡椒と丁子の入市に税をかけ、香りの市が一気に沸き立つ。', 'Pfeffer und Nelken am Tor besteuern; der Gewürzmarkt brodelt.'),
 q('香税既课，库中金珠渐积，宴飨之日益奢。', '香稅既課，庫中金珠漸積，宴饗之日益奢。', 'The dues run; the coffer fills with coin, and feasts grow grander.', 'Сборы идут; копилка полнится монетами, пиры пышнеют.', '香税がかかり、庫に金珠が積もり、宴はますます贅沢になる。', 'Die Abgabe läuft; die Truhe füllt sich mit Münzen, die Feste werden prächtiger.'),
 q('放市不问', '放市不問', 'Leave the market', 'Не мешать рынку', '市に任せる', 'Den Markt lassen'),
 q('香料细物，任商自由贩运，不设课。', '香料細物，任商自由販運，不設課。', 'Spices are trifles; let the merchants trade, no dues.', 'Пряности — мелочь; купцы торгуют, сборов нет.', '香辛料は細物、商人の自由な売買に任せて課さない。', 'Gewürze sind Kleinigkeiten; die Händler handeln, ohne Abgaben.'),
 q('香市自流，物价喧腾于市，官家分文未取。', '香市自流，物價喧騰於市，官家分文未取。', 'The spice lane runs free; prices carouse and the crown takes nothing.', 'Пряная лавка свободна; цены пляшут, корона не берёт ничего.', '香の市は流れに任せ、値段は沸き立つが、官は一文も取らない。', 'Die Gewürzgasse läuft frei; die Preise tanzen, die Krone nimmt nichts.')
)
ev('street_show', 2,
 ('百戏巡街', '百戲巡街', 'The Street Show', 'Уличное представление', '大道芸の巡演', 'Die Straßenschau'),
 ('卖艺者走索吞火，观者如堵，或请{king}出资设棚，亦或延列国使者观之。', '賣藝者走索吞火，觀者如堵，或請{king}出資設棚，亦或延列國使者觀之。', 'Street artists walk the rope and swallow fire; crowds wall in. Give them silver for a stage — or invite foreign eyes.', 'Лицедеи ходят по канату и глотают огонь; толпы стеной. Дать им серебра на подмостки — или позвать чужие взгляды.', '大道芸人が綱渡りに火呑み、見物人が壁のように囲む——{king}は壇の金を出すか、列国の使者を招くか。', 'Gaukler gehen das Seil und schlucken Feuer; die Menge steht wie eine Mauer. Silber für eine Bühne — oder fremde Augen einladen.'),
 q('出资设棚', '出資設棚', 'Fund a stage', 'Дать на подмостки', '舞台の金', 'Eine Bühne finanzieren'),
 q('库银搭棚，乐部排场，百戏连台。', '庫銀搭棚，樂部排場，百戲連台。', 'Treasury builds the stage; the band strikes up, show follows show.', 'Казна строит подмостки; оркестр вступает, номер за номером.', '国庫で棚を組み、楽団が揃い、百芸が続けざまに繰り出される。', 'Die Kasse baut die Bühne; die Band stimmt an, Schau folgt auf Schau.'),
 q('百戏连台，街巷欢腾，里巷称颂王恩浩荡。', '百戲連台，街巷歡騰，里巷稱頌王恩浩蕩。', 'Show follows show; the lanes rejoice and praise the royal grace.', 'Номер за номером; переулки радуются и славят царскую милость.', '芸が続き、巷は歓喜し、里は王の恩を称えた。', 'Schau folgt auf Schau; die Gassen jauchzen und preisen die königliche Gnade.'),
 q('延客共观', '延客共觀', 'Show the foreign guests', 'Показать чужим гостям', '外客を招いて観る', 'Fremden Gästen zeigen'),
 q('置酒结彩，延列国使节共观百戏。', '置酒結彩，延列國使節共觀百戲。', 'Wine and bunting; envoys of the states watch the show.', 'Вино и гирлянды; послы держав смотрят представление.', '酒を置き飾りを結び、列国の使節を招いて共に百芸を観る。', 'Wein und Girlanden; Gesandte der Staaten sehen die Schau.'),
 q('使节抚掌称奇，列国笑谈{kingdom}民乐之盛。', '使節撫掌稱奇，列國笑談{kingdom}民樂之盛。', 'Envoys applaud in wonder; the states gossip warmly of {kingdom}\'s joyful folk.', 'Послы хлопают в восторге; державы тепло судачат о весёлом народе {kingdom}.', '使節は拍手して奇を称え、列国は{kingdom}の民の楽しさを笑い話にした。', 'Gesandte klatschen staunend; die Staaten schwärmen von {kingdom}\'s fröhlichem Volk.')
)
ev('temple_market', 2,
 ('庙会开市', '廟會開市', 'The Temple Market', 'Храмовый рынок', '廟会の市', 'Der Tempelmarkt'),
 ('香火庙会旧址荒废，或请{king}兴市抽税，或供列国香客朝礼以结善缘。', '香火廟會舊址荒廢，或請{king}興市抽稅，或供列國香客朝禮以結善緣。', 'The old temple fair lies weedy. {king} may revive it for revenue — or welcome pilgrims from foreign states.', 'Старый храмовый торг зарос травой. {king} может возродить его ради дохода — или принять паломников из держав.', '香火の廟会の跡は荒れ果てた——{king}は市を興して税を取るか、列国の参詣人を迎えて善縁を結ぶか。', 'Der alte Tempelmarkt liegt verunkrautet. {king} kann ihn für Einkünfte erneuern — oder Pilger fremder Staaten empfangen.'),
 q('兴市抽税', '興市抽稅', 'Revive for revenue', 'Возродить ради дохода', '市を興し税を取る', 'Für Einkünfte erneuern'),
 q('重开庙市，香烛杂货皆税取一分。', '重開廟市，香燭雜貨皆稅取一分。', 'Reopen the fair; incense and trinkets all pay a tithe.', 'Открыть торг; благовония и безделушки платят десятину.', '廟会を再開し、香燭も雑貨も一分の税を取る。', 'Den Markt wieder öffnen; Weihrauch und Kram zahlen den Zehnt.'),
 q('庙市重开，香火如云，税钱日进于公库。', '廟市重開，香火如雲，稅錢日進於公庫。', 'The fair reopens under incense clouds; dues flow daily into the coffer.', 'Торг открыт под облаками благовоний; сборы ежедневно текут в казну.', '廟会が再開し、香煙は雲のよう、税銭が日々公庫に入る。', 'Der Markt öffnet unter Weihrauchwolken; die Abgaben fließen täglich in die Truhe.'),
 q('香集远客', '香集遠客', 'Welcome pilgrims', 'Принять паломников', '参詣人を迎える', 'Pilger empfangen'),
 q('广招列国香客朝会，斋醮三天三夜。', '廣招列國香客朝會，齋醮三天三夜。', 'Call pilgrims from the states: three days and nights of rites.', 'Звать паломников из держав: три дня и ночи обрядов.', '列国の参詣人を広く招き、斎醮の儀を三日三夜。', 'Pilger der Staaten rufen: drei Tage und Nächte der Riten.'),
 q('香客云集，列国皆谓{kingdom}庙民相得。', '香客雲集，列國皆謂{kingdom}廟民相得。', 'Pilgrims throng; every state says {kingdom}\'s temple and people fit well.', 'Паломники теснятся; каждая держава говорит, что храм и народ {kingdom} в ладу.', '参詣人が雲集し、列国は{kingdom}の廟と民の和を語った。', 'Pilger strömen; alle Staaten sagen, Tempel und Volk von {kingdom} passen zusammen.')
)
ev('communal_well', 2,
 ('公井修浚', '公井修浚', 'The Communal Well', 'Общественный колодец', '共同井の修繕', 'Der Gemeinschaftsbrunnen'),
 ('城西公井久已淤塞，夏旱取水如金，里老请{king}出钱浚井，以泽万民。', '城西公井久已淤塞，夏旱取水如金，里老請{king}出錢浚井，以澤萬民。', 'The west well is silted shut; in the summer drought, water is gold. Elders beg {king} to clear it for the people.', 'Западный колодец заилился; в летнюю засуху вода — золото. Старейшины просят {king} расчистить его народу.', '城西の公井が澱み、夏の干ばつで水は金の値——里老は{king}に銭を出して井を浚い万民に恵みをと願う。', 'Der Westbrunnen ist verschlammt; in der Sommerdürre ist Wasser Gold. Die Alten bitten {king}, ihn dem Volk zu räumen.'),
 q('出钱浚井', '出錢浚井', 'Pay to dredge', 'Дать на расчистку', '井を浚う', 'Die Räumung zahlen'),
 q('召集匠人清淤砌壁，深掘数丈。', '召集匠人清淤砌壁，深掘數丈。', 'Hire the crews: clear the silt, mend the walls, dig deeper.', 'Нанять артель: убрать ил, укрепить стены, копать глубже.', '職人を集め、澱みをさらって塀を直し、数丈深く掘る。', 'Die Trupps anheuern: Schlamm räumen, Wände flicken, tiefer graben.'),
 q('清泉涌出，四邻饮之，{kingdom}民感其恩。', '清泉湧出，四鄰飲之，{kingdom}民感其恩。', 'Clear water gushes; the streets drink and {kingdom}\'s people feel the grace.', 'Чистая вода бьёт; улицы пьют, и народ {kingdom} чувствует милость.', '清い泉が湧き出し、近隣が飲み、{kingdom}の民はその恩を感じる。', 'Klares Wasser quillt; die Straßen trinken, und {kingdom}\'s Volk spürt die Gnade.'),
 q('令民自浚', '令民自浚', 'Let the people dig', 'Пусть копают сами', '民に浚わせる', 'Das Volk graben lassen'),
 q('遣里长督民自浚，官库未动分毫。', '遣里長督民自浚，官庫未動分毫。', 'Bid the wardens make the people dredge; the till stays untouched.', 'Велеть старостам заставить народ расчищать; казна не тронута.', '里正に督させ民自らに浚わせ、官庫は一文も動かさない。', 'Die Vorsteher das Volk graben lassen; die Kasse bleibt unberührt.'),
 q('公井浚至半途而废，一遇天旱仍见争汲纷争。', '公井浚至半途而廢，一遇天旱仍見爭汲紛爭。', 'The dredge stalls midway; when drought comes, the wrangling at the well returns.', 'Расчистка застряла на полпути; в засуху у колодца снова грызня.', '浚いは半端で終わり、旱の時には井戸の争いがまた現れる。', 'Die Räumung stockt; kommt die Dürre, kehrt der Streit am Brunnen zurück.')
)
ev('bridge_toll', 2,
 ('桥关设税', '橋關設稅', 'The Bridge Toll', 'Мостовой сбор', '橋の通行税', 'Die Brückenmaut'),
 ('新桥既成，客商争渡如织，或请{king}设桥税以补府库，或畏其重激民怨。', '新橋既成，客商爭渡如織，或請{king}設橋稅以補府庫，或畏其重激民怨。', 'The new bridge stands and merchants throng it. {king} may set a toll to fill the coffers — or dread the wrath it stirs.', 'Новый мост стоит, купцы толпятся на нём. {king} может взять пошлину для казны — или страшиться гнева народа.', '新しい橋が架かり、客商が渡しを争う——{king}は橋税を設けて庫を補うか、民怨の高まりを恐れるか。', 'Die neue Brücke steht; Kaufleute drängen sich. {king} kann Maut für die Kasse erheben — oder den Volkszorn fürchten.'),
 q('设卡抽税', '設卡抽稅', 'Post a toll', 'Поставить пост', '関所を設け税を取る', 'Eine Maut posten'),
 q('桥头设卡，车马按载抽税，库银立增。', '橋頭設卡，車馬按載抽稅，庫銀立增。', 'A post at the bridge, a fee on every load; the treasury rises at once.', 'Пост у моста, плата с каждого воза; казна тотчас поднимается.', '橋のたもとに関所を置き、荷の分だけ税を取れば、庫銀がすぐに増える。', 'Ein Posten an der Brücke, eine Gebühr je Last; die Kasse steigt sogleich.'),
 q('桥税既设，客商往来如流，公库为之殷实。', '橋稅既設，客商往來如流，公庫為之殷實。', 'The toll stands; merchants flow as the river, and the coffer grows solid.', 'Пошлина стоит; купцы текут, как река, казна крепнет.', '橋税が設けられ、客商は川のように流れ、公庫が実りあるものになる。', 'Die Maut steht; Kaufleute fließen wie der Fluss, die Truhe wird fest.'),
 q('重征桥税', '重徵橋稅', 'Double the toll', 'Удвоить пошлину', '橋税を重く', 'Die Maut verdoppeln'),
 q('倍加其税，商旅裹足，怨声鼎沸。', '倍加其稅，商旅裹足，怨聲鼎沸。', 'Double the fee; merchants halt at the bank, and complaints boil.', 'Удвоить плату; купцы стопорятся у берега, жалобы кипят.', '税を倍にすれば商旅が足を止め、怨みの声が沸き立つ。', 'Die Gebühr verdoppeln; Kaufleute halten am Ufer, Klagen sieden.'),
 q('车马改道，桥夫空守，民愤聚而为骚乱。', '車馬改道，橋夫空守，民憤聚而為騷亂。', 'Carts reroute, the tollmen stand idle, and popular fury thickens into riot.', 'Возы идут в обход, сборщики стоят впустую, ярость сгущается в бунт.', '車馬は道を変え、橋守は空しく立ち、民の憤りが騒乱に凝る。', 'Fuhrwerke weichen aus, die Einnehmer stehen leer, Volkszorn gerinnt zu Unruhe.')
)
# ===== 邻里与治乱 =====
ev('neighbor_dispute', 2,
 ('邻家争界', '鄰家爭界', 'The Neighbors\' Dispute', 'Спор соседей', '隣人の地争い', 'Der Nachbarstreit'),
 ('两邻为三尺滴水之界动武，殃及半巷，里正束手无策，急候{king}定夺。', '兩鄰為三尺滴水之界動武，殃及半巷，里正束手無策，急候{king}定奪。', 'Two neighbors fight over a three-foot eaves line; half the lane suffers, the warden is helpless, and all wait on {king}.', 'Два соседа дерутся за трёхфутовую линию карниза; пол-переулка страдает, староста бессилен, всё ждёт {king}.', '隣人二人が三尺の軒先の境を争って殴り合い、半町が巻き添えに——里正は手をこまねき、{king}の裁断を待つ。', 'Zwei Nachbarn streiten um eine Drei-Fuß-Trauflinie; die halbe Gasse leidet, der Vorsteher ist ratlos, alles wartet auf {king}.'),
 q('各执其罪', '各執其罪', 'Punish both', 'Наказать обоих', '双方を罰する', 'Beide bestrafen'),
 q('不问界契，两户同罚，各杖二十。', '不問界契，兩戶同罰，各杖二十。', 'Deed or no, both houses are flogged — twenty strokes each.', 'Без грамоты оба двора получают по двадцать ударов.', '証文を問わず、両戸ともに各二十の杖を課す。', 'Gleich ob Urkunde: beide Höfe erhalten je zwanzig Streiche.'),
 q('两户冤声载道，邻人抱不平，巷中骚动。', '兩戶冤聲載道，鄰人抱不平，巷中騷動。', 'Both houses cry foul, neighbors protest, and the lane turns restless.', 'Оба двора кричат о несправедливости, соседи протестуют, переулок беспокоен.', '両戸は冤罪を叫び、隣人は不満を抱え、巷に騒動が起きる。', 'Beide Höfe schreien über Unrecht, Nachbarn protestieren, die Gasse wird unruhig.'),
 q('勘界息讼', '勘界息訟', 'Survey the line', 'Замерить черту', '境を測って収める', 'Die Grenze vermessen'),
 q('遣勘界官丈量立桩，断讼息争。', '遣勘界官丈量立樁，斷訟息爭。', 'Send surveyors, measure and set stones, end the quarrel.', 'Послать землемеров, отмерить и поставить камни, кончить спор.', '勘界の官を遣わし、測って杭を立て、訴訟を裁いて争いを収める。', 'Vermesser senden, messen und Steine setzen, den Streit beenden.'),
 q('界石既定，两家服气，巷中复闻炊烟，四邻称善。', '界石既定，兩家服氣，巷中復聞炊煙，四鄰稱善。', 'The boundary stones stand; both houses yield, cooking smoke rises again, and the neighbors approve.', 'Межевые камни стоят; оба двора смирились, дым очагов снова вьётся, соседи довольны.', '境石が定まり、両家は従い、巷に再び炊煙が立ちのぼり、近隣も善しと称える。', 'Die Grenzsteine stehen; beide Höfe fügen sich, Herdrauch steigt wieder, und die Nachbarn nicken.')
)
ev('house_rent', 2,
 ('屋租飞涨', '屋租飛漲', 'The House Rent', 'Квартирная рента', '家賃の高騰', 'Die Mietkrise'),
 ('城中屋租一倍再倍，佣工租户夜宿桥洞，怨声鼎沸——{king}震怒，亦知难禁。', '城中屋租一倍再倍，傭工租戶夜宿橋洞，怨聲鼎沸——{king}震怒，亦知難禁。', 'House rents double and double again; laborers sleep under the bridge and their fury boils — {king} fumes, knowing a ban is hard.', 'Плата за жильё растёт и растёт; батраки спят под мостом, ярость кипит — {king} гневается, зная, что запретить трудно.', '家賃は倍々に膨れ上がり、働き手は橋の下に泊まる——不満は沸騰し、{king}は怒るが、禁じようのないことも知る。', 'Die Mieten verdoppeln sich und wieder; Tagelöhner schlafen unter der Brücke, ihre Wut kocht — {king} tobt, doch ein Verbot ist schwer.'),
 q('任其市定', '任其市定', 'Let the market rule', 'Пусть рынок правит', '市場に任せる', 'Den Markt regieren lassen'),
 q('房租随市，官府不干涉，租户自困。', '房租隨市，官府不干涉，租戶自困。', 'Rents follow the market; the court stays out, tenants suffer.', 'Цены следуют рынку; двор не вмешивается, жильцы страдают.', '家賃は相場任せ、官は口を挟まず、借家人が困る。', 'Die Mieten folgen dem Markt; der Hof greift nicht ein, die Mieter leiden.'),
 q('租户忍无可忍，群起哄闹，桥洞塞满，市面骚然。', '租戶忍無可忍，群起哄鬧，橋洞塞滿，市面騷然。', 'Tenants can bear no more; they riot in crowds, the bridge fills, and the town roils.', 'Жильцы больше не терпят; бунтуют толпами, под мостом полно тел, город бурлит.', '借家人は堪えかねて集まって騒ぎ、橋の下は満ち、市のありさまが騒然となる。', 'Die Mieter ertragen es nicht mehr; sie rotten sich auf, die Brücke füllt sich, die Stadt gärt.'),
 q('平价限租', '平價限租', 'Cap the rents', 'Ограничить ренту', '家賃を抑える', 'Die Mieten deckeln'),
 q('颁例限租，平价录册，违者问罪。', '頒例限租，平價錄冊，違者問罪。', 'Issue a rule, record fair rents, and punish the greedy.', 'Издать указ, записать честные цены и карать жадных.', '令を出して家賃を限り、相場を帳簿に録し、背く者を問罪する。', 'Eine Regel erlassen, faire Mieten erfassen und Geiz bestrafen.'),
 q('限租令下，租价稍平，佣工得安，市井称快。', '限租令下，租價稍平，傭工得安，市井稱快。', 'The cap holds; rents ease, workers find rest, and the town approves.', 'Предел держится; плата спадает, работники отдыхают, город одобряет.', '家賃の限界が効き、少し平らになり、働き手が安らぎ、町も満足する。', 'Die Deckelung hält; die Mieten sinken, Arbeiter kommen zur Ruhe, die Stadt nickt.')
)
ev('harvest_dance', 2,
 ('丰年之舞', '豐年之舞', 'The Harvest Dance', 'Пляска урожая', '豊穣の舞', 'Der Erntetanz'),
 ('五谷丰登，仓廪皆满，乡人击缶而歌，请{king}开庆丰之宴，与列国同乐。', '五穀豐登，倉廩皆滿，鄉人擊缶而歌，請{king}開慶豐之宴，與列國同樂。', 'Five grains ripen and the storehouses overflow; villagers sing to the pot-drum, asking {king} to feast the harvest with the nations.', 'Пять злаков вызрели, амбары полны; селяне поют под барабаны, прося {king} устроить пир урожая для держав.', '五穀が実り、倉は満ちる——村人は缶を打ちて歌い、{king}に豊年の宴を開き列国と共に祝えと請う。', 'Fünf Körner reifen, die Speicher laufen über; die Dörfler singen zum Tontopf und bitten {king}, mit den Staaten das Erntefest zu feiern.'),
 q('开庆丰宴', '開慶豐宴', 'Host the harvest feast', 'Устроить пир урожая', '豊年の宴', 'Das Erntefest ausrichten'),
 q('设千人宴，击缶而歌，乡民痛饮。', '設千人宴，擊缶而歌，鄉民痛飲。', 'A thousand-seat feast, pot-drums rolling, villagers drinking deep.', 'Пир на тысячу мест, барабаны катят, селяне пьют глубоко.', '千人宴を敷き、缶を打ち歌い、村人が痛飲する。', 'Ein Tausend-Platz-Fest, Tontopf-Trommeln rollen, die Dörfler trinken tief.'),
 q('庆宴既开，万姓欢腾，年成之丰闻于四境。', '慶宴既開，萬姓歡騰，年成之豐聞於四境。', 'The feast opens, the people rejoice, and the fat year rings through the lands.', 'Пир открыт, народ ликует, тучный год гремит по землям.', '宴が開かれ、万姓が喜び、豊作の年が四方に轟く。', 'Das Fest öffnet, das Volk jubelt, das fette Jahr hallt durch die Lande.'),
 q('遍请列国', '遍請列國', 'Invite the nations', 'Пригласить державы', '列国を招く', 'Alle Nationen laden'),
 q('广发金函，邀列国使臣共贺丰年。', '廣發金函，邀列國使臣共賀豐年。', 'Golden letters fly out; envoys come to toast the harvest.', 'Золотые грамоты летят; послы приезжают поднять тост за урожай.', '金の書簡を広く遣わし、列国の使節を招いて豊年を共に祝う。', 'Goldene Briefe fliegen hinaus; Gesandte kommen, auf die Ernte anzustoßen.'),
 q('列国使臣联袂举觞，皆颂{kingdom}得天时之厚。', '列國使臣聯袂舉觴，皆頌{kingdom}得天時之厚。', 'Envoys raise their cups together, all praising {kingdom}\'s bounteous skies.', 'Послы поднимают чаши разом, славя щедрые небеса {kingdom}.', '使節は揃って杯を挙げ、{kingdom}の天時の厚みを皆が謳う。', 'Gesandte erheben gemeinsam die Becher und preisen {kingdom}\'s reiche Himmel.')
)
ev('witch_case', 2,
 ('巫蛊疑云', '巫蠱疑雲', 'The Witch Case', 'Дело о колдовстве', '妖術の疑獄', 'Der Hexenfall'),
 ('村巫被指散播诅咒，群情汹汹请{king}诛之，亦有士人疑其出自诬告。', '村巫被指散播詛咒，群情洶洶請{king}誅之，亦有士人疑其出自誣告。', 'A village witch is accused of spreading curses; the crowd cries for death, though some scholars whisper of a frame-up.', 'Деревенскую ведьму обвиняют в проклятиях; толпа требует смерти, хотя иные книжники шепчут о подставе.', '村の巫女が呪いを蒔いたと告発され、人々は{king}に誅殺を求める——一方で学者は濡れ衣を疑う。', 'Eine Dorfhexe wird beschuldigt, Flüche zu säen; die Menge verlangt den Tod, doch manche Gelehrte flüstern von einer Verschwörung.'),
 q('明正典刑', '明正典刑', 'Burn her', 'Сжечь', '火刑に処す', 'Verbrennen'),
 q('顺民愤诛巫，当众焚之，以肃妖风。', '順民憤誅巫，當眾焚之，以肅妖風。', 'Heed the crowd: burn the witch in public to purge the evil.', 'Внять толпе: сжечь ведьму публично, извести зло.', '民の憤りに従い、巫女を公然と焼き、妖の風を鎮める。', 'Der Menge folgen: die Hexe öffentlich verbrennen, das Böse tilgen.'),
 q('火起之时，民情骇然，翌日市中竟有乱象。', '火起之時，民情駭然，翌日市中竟有亂象。', 'As the fire rises, the crowd turns grim; by next day riots wander the market.', 'Пока огонь встаёт, толпа мрачнеет; наутро рынки бунтуют.', '火が上がるにつれ民心は慄き、翌日には市場で騒乱が起きた。', 'Während das Feuer steigt, wird die Menge finster; am nächsten Tag wandern Unruhen über den Markt.'),
 q('释疑放归', '釋疑放歸', 'Free her', 'Освободить её', '疑いを解き帰す', 'Sie freilassen'),
 q('判为诬告，杖诬告者，召医验疾，放归村野。', '判為誣告，杖誣告者，召醫驗疾，放歸村野。', 'Deem the charge false, cane the slanderers, call the physician, and let her go home.', 'Признать навет, выпороть доносчиков, позвать лекаря и отпустить домой.', '訴えは虚と判じ、訴えた者を杖ち、医師を召し、村へ放つ。', 'Die Anklage für falsch erklären, die Verleumder züchtigen, den Arzt rufen und sie heimlassen.'),
 q('村巫获释，邻邦闻之，称{kingdom}执法有恕。', '村巫獲釋，鄰邦聞之，稱{kingdom}執法有恕。', 'The witch walks free; neighbor states hear it and praise {kingdom}\'s lenient justice.', 'Ведьма свободна; соседние державы слышат и хвалят мягкий суд {kingdom}.', '巫女は赦され、隣国は聞いて、{kingdom}の法の寛容さを称えた。', 'Die Hexe geht frei; Nachbarstaaten hören es und rühmen {kingdom}\'s milde Justiz.')
)
