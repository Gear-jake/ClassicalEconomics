# -*- coding: utf-8 -*-
"""v1.7.0 disaster 文案（六语）"""
EVENTS = {}
def ev(eid, opts, title, desc, *texts):
    EVENTS[eid] = (opts, title, desc, list(texts))
def q(*args): return args

ev('shooting_star', 2,
 ('星坠于野', '星墜於野', 'A Star Falls', 'Звезда упала', '星が落ちる', 'Ein Stern fällt'),
 ('夜半有星自天而坠，火光烛地，{king}急召太史占之，群相惊疑。', '夜半有星自天而墜，火光燭地，{king}急召太史占之，群相驚疑。', 'At midnight a star falls from heaven, painting the ground with light; {king} summons the stargazers, and the court is uneasy.', 'В полночь с неба падает звезда, свет её ложится на землю; {king} зовёт звездочётов, двор тревожится.', '真夜中、星が天から落ちて大地に光を敷いた。{king}が太史を召して占わせると、廷はみな不安に包まれた。', 'Um Mitternacht fällt ein Stern vom Himmel und legt Licht über das Land; {king} ruft die Sterndeuter, und der Hof ist unruhig.'),
 q('探陨赠邻', '探隕贈鄰', 'Seek the starstone', 'Искать звезду', '隕石を探す', 'Den Sternstein suchen'),
 q('遣使往观陨石，分赠列国，以示谦和。', '遣使往觀隕石，分贈列國，以示謙和。', 'Send envoys to find the stone and share it with the realms.', 'Послать послов найти камень и поделиться с державами.', '使いを遣って石を探させ、諸国に分け与えて睦みを示す。', 'Boten senden, den Stein zu finden und mit den Reichen zu teilen.'),
 q('陨石分赠列国，诸王称善，{kingdom}之名传于四方。', '隕石分贈列國，諸王稱善，{kingdom}之名傳於四方。', 'The stone is shared; kings approve, and the name of {kingdom} spreads abroad.', 'Камень поделён; цари одобряют, имя {kingdom} гремит далеко.', '石は諸国に分けられ、王たちは善しとし、{kingdom}の名は四方に轟いた。', 'Der Stein wird geteilt; die Könige loben es, und der Name von {kingdom} verbreitet sich weit.'),
 q('视若无睹', '視若無睹', 'Dismiss the omen', 'Пренебречь знамением', '兆しを眼中に置かない', 'Das Zeichen verwerfen'),
 q('斥星占为妖言，禁民议之。', '斥星占為妖言，禁民議之。', 'Call the omen idle talk and silence the gossip.', 'Назвать знамение басней, запретить пересуды.', '星占いを妖言と斥し、民が口にするのを禁じる。', 'Das Zeichen Geschwätz nennen und das Murmeln verbieten.'),
 q('民疑{king}讳灾，谣言汹汹，{kingdom}举国不安。', '民疑{king}諱災，謠言洶洶，{kingdom}舉國不安。', 'The people suspect {king} hides misfortune; rumor surges, and {kingdom} grows restless.', 'Народ думает, что {king} скрывает беду; слухи нарастают, {kingdom} тревожится.', '民は{king}が凶を隠していると疑い、噂が高まる。{kingdom}は国を挙げて不安に沈んだ。', 'Das Volk glaubt, {king} verberge Unheil; die Gerüchte schwellen, {kingdom} wird rastlos.')
)
ev('solar_eclipse', 2,
 ('日蚀之惊', '日蝕之驚', 'The Eclipse', 'Затмение', '日蝕の驚き', 'Die Sonnenfinsternis'),
 ('白日昼晦，星见于午，{king}率众击鼓救日，民皆仰天号呼。', '白日晝晦，星見於午，{king}率眾擊鼓救日，民皆仰天號呼。', 'Day darkens and stars show at noon; {king} beats drums to rescue the sun while the people cry to heaven.', 'День темнеет, звёзды видны в полдень; {king} бьёт в барабаны, народ взывает к небу.', '昼が闇に変わり、正午に星が見える。{king}は太鼓を打って太陽を救おうとし、民は天を仰いで泣き叫ぶ。', 'Der Tag verfinstert sich, Sterne stehen am Mittag; {king} schlägt die Trommeln, um die Sonne zu retten, das Volk ruft zum Himmel.'),
 q('杀巫止谤', '殺巫止謗', 'Silence the seers', 'Замолчать звездочётов', '占いを封じる', 'Die Seher zum Schweigen bringen'),
 q('斩司天监以塞众口，平民心。', '斬司天監以塞眾口，平民心。', 'Behead the court astronomers to stop the talk, calm the people.', 'Казнить придворных звездочётов, чтобы утихомирить народ.', '司天監を斬って口を塞ぎ、民の心を落ち着かせる。', 'Die Hofastronomen köpfen, um das Gerede zu beenden, das Volk zu beruhigen.'),
 q('司天监含冤而死，民惧巫祸，{kingdom}阴云不散。', '司天監含冤而死，民懼巫禍，{kingdom}陰雲不散。', 'The astronomers die wronged; the people dread the omen, and gloom clings to {kingdom}.', 'Невинные казнены; народ боится знамения, мрак держится над {kingdom}.', '司天監は無実のまま死に、民は凶兆を恐れる。{kingdom}に暗雲が垂れ込めたまま。', 'Die Astronomen sterben unschuldig; das Volk fürchtet das Zeichen, und Düsternis hängt über {kingdom}.'),
 q('斋戒修省', '齋戒修省', 'Fast and reflect', 'Пост и покаяние', '斎戒して省みる', 'Fasten und einkehren'),
 q('与民同食素，省徭役、薄赋税。', '與民同食素，省徭役、薄賦稅。', 'Share plain food with the folk and ease their burdens.', 'Разделить с народом простую пищу, облегчить тяготы.', '民と共に粗食を味わい、徭役と税を軽くする。', 'Schlichte Kost mit dem Volk teilen, die Lasten erleichtern.'),
 q('日复明，天渐开；灾异晏然，民心稍定。', '日復明，天漸開；災異晏然，民心稍定。', 'The sun returns and the sky clears; the portent passes and hearts settle.', 'Солнце вернулось, небо прояснилось; знамение ушло, сердца спокойны.', '日は戻り空も晴れた。天変は静まり、民の心もやや落ち着いた。', 'Die Sonne kehrt zurück, der Himmel klart; das Zeichen vergeht, die Herzen beruhigen sich.')
)
ev('blood_moon', 2,
 ('血月当空', '血月當空', 'The Blood Moon', 'Багровая луна', '血の月', 'Der Blutmond'),
 ('满月如血，夜犬惊嚎，{king}临观天象，见军民皆伏地拜月。', '滿月如血，夜犬驚嚎，{king}臨觀天象，見軍民皆伏地拜月。', 'The full moon bleeds crimson and dogs howl; {king} watches the sky while soldiers and folk kneel to the moon.', 'Полная луна налилась кровью, воют псы; {king} смотрит в небо, весь народ падает ниц.', '満月が血のように赤く、夜の犬が遠吠えする。{king}が天象を見渡すと、兵も民も地に伏して月を拝んでいる。', 'Der Vollmond glüht rot, Hunde heulen; {king} betrachtet den Himmel, während Volk und Soldaten zum Mond knien.'),
 q('禁民夜聚', '禁民夜聚', 'Curfew the watchers', 'Запретить сборища', '夜間の集会を禁ずる', 'Den Zuschauern Ausgang sperren'),
 q('下令夜禁，驱散拜月之众。', '下令夜禁，驅散拜月之眾。', 'Decree a night ban and scatter the moon-worshippers.', 'Ввести ночной запрет, разогнать толпу.', '夜間外出を禁じ、月を拝む群衆を追い散らす。', 'Nachtsperre verhängen und die Mondverehrer zerstreuen.'),
 q('驱赶过急，踩踏伤民；{kingdom}怨声四起，人心浮动。', '驅趕過急，踩踏傷民；{kingdom}怨聲四起，人心浮動。', 'The rush causes trampling; grief and murmurs spread through {kingdom}, hearts waver.', 'Второпях задавили людей; {kingdom} ропщет, сердца мечутся.', '追い立ての混乱で踏みにじられ、{kingdom}に怨みが広がり人心が動揺した。', 'In der Eile werden Menschen getrampelt; {kingdom} murmelt, die Herzen wanken.'),
 q('遣使安邻', '遣使安鄰', 'Send envoys abroad', 'Послать послов', '使節を諸国へ', 'Gesandte entsenden'),
 q('明诏辟谣，并遣使邻国互慰。', '明詔闢謠，並遣使鄰國互慰。', 'Issue a clear decree and send envoys to comfort the neighbors.', 'Издать ясный указ, отправить послов к соседям.', '明らかな詔で噂を払い、使いを諸国へ送って互いに慰める。', 'Ein klares Dekret erlassen und Boten senden, die Nachbarn zu trösten.'),
 q('邻邦闻讯，各遣使答礼，{kingdom}声望日隆。', '鄰邦聞訊，各遣使答禮，{kingdom}聲望日隆。', 'The realms reply with gifts and courtesies; the fame of {kingdom} grows.', 'Державы отвечают дарами; слава {kingdom} растёт.', '諸国は礼を返し、{kingdom}の声望は日ごとに高まった。', 'Die Reiche antworten mit Gaben; der Ruhm von {kingdom} wächst.')
)
ev('quake_aftershock', 2,
 ('余震未息', '餘震未息', 'Aftershocks', 'Повторные толчки', '余震続く', 'Nachbeben'),
 ('前震初定，地复大动，城垣再裂，{king}闻报失色：地脉似未宁帖。', '前震初定，地復大動，城垣再裂，{king}聞報失色：地脈似未寧帖。', 'The first quake barely settled when the earth heaves and walls crack anew; {king} pales: the veins of the land are restless.', 'Едва затихло, земля снова ходит, стены трещат; {king} бледнеет: недра ещё не успокоились.', '先の地震がやっと収まったかと思うと、大地がまた動いて城壁が再び裂けた。{king}は青ざめる——地の脈理はまだ落ち着かない。', 'Kaum ist das erste Beben verklungen, bebt die Erde erneut und Mauern bersten; {king} erbleicht: Die Adern der Erde sind unruhig.'),
 q('赈灾修城', '賑災修城', 'Relieve and rebuild', 'Восстановление', '救済と修復', 'Lindern und wiederaufbauen'),
 q('发仓廪、拨银钱，修缮城垣。', '發倉廩、撥銀錢，修繕城垣。', 'Open the granaries and spend gold to repair the walls.', 'Открыть житницы, потратить золото на стены.', '倉を開き銀を出して、城壁を修繕する。', 'Die Speicher öffnen und Gold für die Mauern ausgeben.'),
 q('银钱如流水，城垣渐复；然地底闷响未绝，更患隐伏。', '銀錢如流水，城垣漸復；然地底悶響未絕，更患隱伏。', 'Gold flows like water and the walls rise again, yet the deep rumble never fades: a worse harm lies in wait.', 'Серебро уходит рекой, стены чинятся; но гул в глубине не стихает — худшее притаилось.', '銀は流れるように出て、城壁は少しずつ復る。しかし地の底の唸りは消えず、さらなる禍が潜んでいる。', 'Silber verrinnt wie Wasser, die Mauern wachsen, doch das Grollen in der Tiefe schweigt nicht: Schlimmeres lauert.'),
 q('任其震荡', '任其震盪', 'Let the earth shake', 'Оставить как есть', '揺れに任せる', 'Die Erde beben lassen'),
 q('灾民四散，宫城亦不加固。', '災民四散，宮城亦不加固。', 'Leave the refugees, do not shore the palace.', 'Бросить беженцев, не укреплять дворец.', '被災者は散り散りになり、宮も補強しない。', 'Flüchtlinge im Stich lassen, den Palast nicht stützen.'),
 q('震动破坏愈重，流民结队逃荒，{kingdom}涣散难收。', '震動破壞愈重，流民結隊逃荒，{kingdom}渙散難收。', 'The jolts tear harder, refugees stream away, and {kingdom} scatters beyond recall.', 'Толчки бьют больнее, беженцы уходят, {kingdom} расползается неудержимо.', '揺れの被害は深まり、難民が列をなして逃げる。{kingdom}は散り散りになって取り返せない。', 'Die Stöße reißen härter, Flüchtlinge strömen fort, und {kingdom} zerfällt unwiederbringlich.')
)
ev('quake_rebuild', 2,
 ('震后重建', '震後重建', 'After the Quake', 'После землетрясения', '震災後の復興', 'Nach dem Beben'),
 ('大震之后疮痍满目，{king}下诏重修宗庙民居，所需银粮甚巨。', '大震之後瘡痍滿目，{king}下詔重修宗廟民居，所需銀糧甚巨。', 'Ruin lies everywhere after the quake; {king} decrees a rebuilding of temples and homes, at a cost beyond measure.', 'После толчков всюду развалины; {king} велит отстроить храмы и дома — ценой немалой.', '大震の後、傷跡は目の前に満ちている。{king}は社と民家の再建を命じるが、要る銀と糧は甚だ大きい。', 'Nach dem Beben liegt Trümmerfeld um Trümmerfeld; {king} befiehlt den Aufbau von Tempeln und Häusern — zu unermesslichem Preis.'),
 q('倾囊重建', '傾囊重建', 'Rebuild at all cost', 'Отстроить любой ценой', '総力を注いで再建', 'Um jeden Preis aufbauen'),
 q('国库尽出，雇民以工代赈。', '國庫盡出，僱民以工代賑。', 'Empty the treasury and hire the people for work-relief.', 'Опустошить казну, нанять народ на работы.', '国庫を尽くし、民を雇って労働に賑わす。', 'Die Kasse leeren und das Volk als Arbeiter anheuern.'),
 q('城郭气象一新，然{kingdom}国库为之一空。', '城郭氣象一新，然{kingdom}國庫為之一空。', 'The city rises fresh and grand, but the coffers of {kingdom} stand bare.', 'Город встаёт заново, но казна {kingdom} пуста.', '城は見違えるように新しくなったが、{kingdom}の国庫は空になった。', 'Die Stadt ersteht prächtig neu, doch die Kassen von {kingdom} sind leer.'),
 q('因陋就简', '因陋就簡', 'Patch it up', 'Залатать как-нибудь', '簡素に済ませる', 'Flickwerk'),
 q('只粗略修葺，余财留作他用。', '只粗略修葺，餘財留作他用。', 'Mend roughly and save the rest for other needs.', 'Починить грубо, остальное приберечь.', '粗略に繕って、余りは他の用途に残す。', 'Nur grob flicken und den Rest für anderes aufheben.'),
 q('屋舍苟且，民怨{king}视之若儿戏，怨声渐起。', '屋舍苟且，民怨{king}視之若兒戲，怨聲漸起。', 'The homes stay shoddy; the people call {king}\'s doings a child\'s game, and anger rises.', 'Жилища убоги; народ считает дело {king} забавой, гнев растёт.', '家は粗末なまま。民は{king}のやり方を児戯と嘆き、怨みが次第に高まる。', 'Die Häuser bleiben schäbig; das Volk nennt {king}\'s Werk ein Kinderspiel, der Zorn steigt.')
)
ev('hailstorm', 2,
 ('雹伤禾稼', '雹傷禾稼', 'The Hailstrike', 'Град', '雹の被害', 'Der Hagelschlag'),
 ('雹大如拳，自午至暮，{king}视田中禾稼尽偃，知秋收无望。', '雹大如拳，自午至暮，{king}視田中禾稼盡偃，知秋收無望。', 'Hail the size of fists falls from noon to dusk; {king} sees the fields leveled and knows the harvest is lost.', 'Град с кулак падает с полудня до вечера; {king} видит поваленные поля — урожая не будет.', '拳ほどの雹が昼から夕暮れまで降る。{king}が田を見ると作物はことごとく倒れ、秋の収穫は望めない。', 'Faustgroßer Hagel fällt von Mittag bis Abend; {king} sieht die Felder platt und weiß: Die Ernte ist verloren.'),
 q('补种赈农', '補種賑農', 'Replant the fields', 'Засеять заново', 'まき直して農を救う', 'Die Felder neu bestellen'),
 q('发银购种，令农抢时补栽。', '發銀購種，令農搶時補栽。', 'Buy new seed and bid the farmers replant in haste.', 'Купить семена, велеть крестьянам спешить.', '銀で種を買い、農民に急いで植え直させる。', 'Neues Saatgut kaufen und die Bauern zur Eile anhalten.'),
 q('补种虽迟，灾情稍纾；{kingdom}岁入略损。', '補種雖遲，災情稍紓；{kingdom}歲入略損。', 'The late sowing eases the damage, though the revenue of {kingdom} dips.', 'Поздний посев смягчает беду, но доходы {kingdom} просели.', '遅蒔きながら被害は和らぎ、{kingdom}の歳入はやや損なわれた。', 'Die späte Aussaat lindert den Schaden; die Einkünfte von {kingdom} sinken leicht.'),
 q('坐观其变', '坐觀其變', 'Wait it out', 'Переждать', '成り行きを見る', 'Abwarten'),
 q('令民自拾遗穗，勿兴大役。', '令民自拾遺穗，勿興大役。', 'Let the folk glean and raise no great works.', 'Велеть народу подбирать колосья, больших дел не затевать.', '民に落ち穂を拾わせ、大がかりな工を起こさない。', 'Die Leute nachlesen lassen, keine großen Werke beginnen.'),
 q('灾民自渡，事遂寂然；{kingdom}仓廪无恙。', '災民自渡，事遂寂然；{kingdom}倉廩無恙。', 'The people manage alone, the matter fades, and the stores of {kingdom} stand intact.', 'Народ справился сам; дело забылось, амбары {kingdom} целы.', '民は自ら凌ぎ、騒ぎは自然に収まった。{kingdom}の倉は無事だ。', 'Das Volk hilft sich selbst, die Sache verebbt; die Speicher von {kingdom} bleiben heil.')
)
ev('frostbite_spring', 2,
 ('春寒杀苗', '春寒殺苗', 'Killing Frost', 'Весенний мороз', '春霜が苗を殺す', 'Der Frosttöter'),
 ('三月飞霜，青苗尽萎，{king}闻野老言：此寒为数十载所未见。', '三月飛霜，青苗盡萎，{king}聞野老言：此寒為數十載所未見。', 'Frost falls in the third month and the young shoots wither; a village elder tells {king}: this cold is unseen in decades.', 'В третьем месяце ударил иней, всходы гибнут; старик говорит {king}: такой стужи не видели десятки лет.', '三月に霜が降り、若苗がしおれる。郷の老人は{king}に言う——この寒さは数十年にないものだと。', 'Frost fällt im dritten Monat, die junge Saat verdorrt; ein Alter sagt {king}: Solche Kälte sah man seit Jahrzehnten nicht.'),
 q('开仓济种', '開倉濟種', 'Open the granaries', 'Открыть житницы', '倉を開いて種を', 'Die Speicher öffnen'),
 q('发仓中余粮换种，补播一季。', '發倉中餘糧換種，補播一季。', 'Trade reserve grain for seed and sow one season more.', 'Обменять запасное зерно на семена, засеять сезон.', '倉の余り糧を種に替えて、一季をまき直す。', 'Reservekorn gegen Saat tauschen und eine Saison neu säen.'),
 q('新苗再起，人心稍安；{kingdom}仓廪薄了一分。', '新苗再起，人心稍安；{kingdom}倉廩薄了一分。', 'New shoots rise and hearts settle; the stores of {kingdom} thin a little.', 'Всходы поднялись, сердца спокойны; запасы {kingdom} поредели.', '新芽が立ち上がり人心も落ち着く。{kingdom}の倉はわずかに薄くなった。', 'Neue Saat sprießt, die Herzen ruhn; die Vorräte von {kingdom} werden knapper.'),
 q('不以为意', '不以為意', 'Shrug it off', 'Отмахнуться', '意に介さない', 'Achselzucken'),
 q('曰农事自有天命，不烦库银。', '曰農事自有天命，不煩庫銀。', 'Say the farms are heaven\'s to tend; spare the silver.', 'Сказать, что поля в руках небес; серебро не трогать.', '農事は天の命に任せよと、庫の銀に煩わさない。', 'Sagen, die Felder gehörten dem Himmel; das Silber schonen.'),
 q('农人怨饷无门，结队至王城讨说法，{kingdom}骚然。', '農人怨餉無門，結隊至王城討說法，{kingdom}騷然。', 'Farmers find no aid, march on the court for answers, and {kingdom} stirs with unrest.', 'Крестьяне не находят помощи, идут ко двору; {kingdom} бурлит.', '農民は救いを求められず、列をなして王城に答えを求めに来る。{kingdom}は騒然とした。', 'Die Bauern finden keine Hilfe, ziehen zum Hof und fordern Antwort; {kingdom} gärt.')
)
ev('heat_wave', 2,
 ('赤日如焚', '赤日如焚', 'The Red Sun', 'Пекло', '赤い太陽', 'Die Glutsonne'),
 ('盛暑无雨，田土龟裂，{king}巡行四野，见廪卒中暑而仆者相望。', '盛暑無雨，田土龜裂，{king}巡行四野，見廩卒中暑而仆者相望。', 'A rainless blaze cracks the ground; on his rounds {king} sees workers struck down by the heat in droves.', 'Зной без дождя раскаляет землю; в объезде {king} видит, как падают работники.', '雨のない酷暑で田がひび割れる。巡行の{king}は、暑さに斃れる者が相次ぐのを見る。', 'Glut ohne Regen bricht den Boden; auf der Runde sieht {king} Arbeiter reihenweise umsinken.'),
 q('凿冰施药', '鑿冰施藥', 'Ice and physic', 'Лёд и лекарства', '氷と薬を施す', 'Eis und Arznei'),
 q('发库存冰药，于道旁赐粥。', '發庫存冰藥，於道旁賜粥。', 'Hand out stored ice, medicine, and gruel along the roads.', 'Раздать запасы льда, лекарства и кашу на дорогах.', '蔵の氷と薬を配り、道端で粥を施す。', 'Vorräte an Eis und Arznei ausgeben, Brei an den Straßen austeilen.'),
 q('中暑者渐少，民心稍附；{kingdom}库银为之一耗。', '中暑者漸少，民心稍附；{kingdom}庫銀為之一耗。', 'Fewer sicken and hearts lean back to the crown; the treasury of {kingdom} spends.', 'Больных меньше, сердца к короне; казна {kingdom} потратилась.', '熱中症は減り民心もやや戻る。{kingdom}の庫の銀が費やされた。', 'Weniger erkranken, die Herzen kehren zur Krone zurück; die Kasse von {kingdom} schwindet.'),
 q('闭宫避暑', '閉宮避暑', 'Retreat from the heat', 'Сбежать от жары', '宮を閉じて避暑', 'Der Hitze entfliehen'),
 q('王宫自往清凉地，民苦不问。', '王宮自往清涼地，民苦不問。', 'The court removes to cool hills, indifferent to the people\'s pain.', 'Двор уезжает в прохладные горы, а народу — хоть бы что.', '宮みずから涼しい地へ移り、民の苦しみを問わない。', 'Der Hof zieht in kühle Höhen, gleichgültig gegen die Not des Volkes.'),
 q('民生嗔怒，指{king}者日众，{kingdom}渐有哗噪之象。', '民生嗔怒，指{king}者日眾，{kingdom}漸有嘩噪之象。', 'Anger spreads and more point at {king}; {kingdom} grows loud with clamor.', 'Гнев растёт, всё больше указывают на {king}; {kingdom} шумит.', '民の怒りが募り、{king}を指さす者が日に増える。{kingdom}に騒然の気配が立ち始めた。', 'Der Zorn wächst, immer mehr zeigen auf {king}; {kingdom} wird laut.')
)
ev('typhoon_wind', 2,
 ('飓风拔屋', '颶風拔屋', 'The Typhoon', 'Тайфун', '台風', 'Der Taifun'),
 ('飓风自海而来，拔木掀瓦，{king}登楼四望，见市井半为平地。', '颶風自海而來，拔木掀瓦，{king}登樓四望，見市井半為平地。', 'The typhoon sweeps from the sea, uprooting trees and tearing roofs; from the tower {king} sees half the town flat.', 'Тайфун с моря вырывает деревья, срывает крыши; с башни {king} видит половину города ровной.', '海から台風が来て木を抜き、瓦をひっくり返す。楼上の{king}には、市井の半分が平地に荒れているのが見える。', 'Der Taifun fegt vom Meer, entwurzelt Bäume, reißt Dächer; vom Turm sieht {king} die halbe Stadt platt.'),
 q('抚恤重建', '撫卹重建', 'Comfort and rebuild', 'Утешить и отстроить', '慰めて再建', 'Trösten und aufbauen'),
 q('拨银抚恤灾户，清除街巷积潦。', '撥銀撫卹災戶，清除街巷積潦。', 'Pay relief to stricken homes and clear the flooded lanes.', 'Выплатить помощь, расчистить улицы от воды.', '銀を出して被災した家を慰撫し、水浸しの街路を浚う。', 'Hilfe zahlen und die überfluteten Gassen räumen.'),
 q('市井渐复，流离者得所；{kingdom}库银大耗。', '市井漸復，流離者得所；{kingdom}庫銀大耗。', 'The town recovers and the homeless find shelter, at a heavy drain on the silver of {kingdom}.', 'Город оживает, бездомные устроены; серебро {kingdom} на исходе.', '市は次第に回復し、離散者も住まいを得た。{kingdom}の庫の銀は大いに減った。', 'Die Stadt erholt sich, die Obdachlosen finden Wohnung; das Silber von {kingdom} ist schwer geplündert.'),
 q('仓皇避风', '倉皇避風', 'Flee the storm', 'Спасаться бегством', '慌てて風を避ける', 'Vor dem Sturm fliehen'),
 q('阖宫先避，抚恤之政不行。', '闔宮先避，撫卹之政不行。', 'Bar the palace first and leave all relief undone.', 'Сперва укрыть дворец, помощь отложить.', '宮ごと先に避難し、撫恤の政は行われない。', 'Erst den Palast bergen, keine Hilfe leisten.'),
 q('灾民望救无门，结寨为乱，{kingdom}寇警时闻。', '災民望救無門，結寨為亂，{kingdom}寇警時聞。', 'With no help in sight the victims turn to banditry; alarms of raiders echo through {kingdom}.', 'Без помощи народ уходит в разбойники; в {kingdom} слышны тревоги.', '救いのない被災者は砦を結んで乱を起こし、{kingdom}に寇警がしばしば聞こえる。', 'Ohne Hilfe werden die Opfer zu Räubern; die Wehrsignale von {kingdom} wollen nicht schweigen.')
)
ev('sandstorm', 2,
 ('沙尘蔽日', '沙塵蔽日', 'The Sandstorm', 'Песчаная буря', '砂嵐', 'Der Sandsturm'),
 ('黄沙蔽天，三日不散，{king}闭窗难见日色；风沙越境，邻国亦诉其苦。', '黃沙蔽天，三日不散，{king}閉窗難見日色；風沙越境，鄰國亦訴其苦。', 'Yellow dust hides the sky for three days; {king} cannot see the sun through shuttered glass, and the storm crosses the border — neighbors mourn too.', 'Жёлтая пыль три дня закрывает небо; {king} не видит солнца. Буря перешла границу — стонут и соседи.', '黄砂が三日も空を覆い、{king}は窓を閉ざしても日を覗けない。嵐は国境を越え、隣国も苦しみを訴える。', 'Gelber Staub verhüllt drei Tage den Himmel; {king} sieht die Sonne nur hinter Läden, und der Sturm zieht über die Grenze — die Nachbarn klagen.'),
 q('不恤邻邦', '不恤鄰邦', 'Ignore the neighbors', 'Пренебречь соседями', '隣国を顧みない', 'Die Nachbarn missachten'),
 q('视风沙为天灾，不为邻国示好。', '視風沙為天災，不為鄰國示好。', 'Call it heaven\'s doing and offer the neighbors no courtesy.', 'Счесть бурю небесной и не слать любезностей.', '砂嵐は天災と決めつけ、隣国への志を欠く。', 'Es dem Himmel anlasten und den Nachbarn keine Höflichkeit erweisen.'),
 q('邻国怨{kingdom}无礼，相继断市，商路为之萧条。', '鄰國怨{kingdom}無禮，相繼斷市，商路為之蕭條。', 'Resentful realms cut trade with {kingdom}, and the merchant roads fall silent.', 'Обиженные державы рвут торговлю с {kingdom}; торговые пути пустеют.', '隣国は{kingdom}の無礼を恨み、相次いで市を断つ。商路はさびれてしまった。', 'Die gekränkten Reiche reißen den Handel mit {kingdom} ab; die Handelsstraßen veröden.'),
 q('闭城垂帷', '閉城垂帷', 'Shut the city', 'Закрыть город', '城を閉ざす', 'Die Stadt schließen'),
 q('令民闭户塞牖，静等风停。', '令民閉戶塞牖，靜等風停。', 'Bid the folk seal doors and windows and wait out the wind.', 'Велеть народу запереть двери и ждать.', '民に戸を閉ざし窓を塞いで風の止むのを待たせる。', 'Das Volk Türen und Fenster dicht machen lassen, den Wind aussitzen.'),
 q('风沙渐息，田舍积尘，扫除后乃复如常。', '風沙漸息，田舍積塵，掃除後乃復如常。', 'The dust at last settles; after the sweeping, farms and homes are as before.', 'Пыль осела; после уборки всё как было.', '砂風はやみ、家々に積もった塵を掃き出せば、また平常に戻った。', 'Der Staub legt sich; nach dem Fegen sind Höfe und Häuser wie zuvor.')
)
ev('mudslide', 2,
 ('山泥崩泻', '山泥崩瀉', 'The Mudslide', 'Оползень', '土石流', 'Der Erdrutsch'),
 ('连雨之后，山泥崩泻而下，{king}闻村寨覆没，急点兵役往救。', '連雨之後，山泥崩瀉而下，{king}聞村寨覆沒，急點兵役往救。', 'After days of rain the mountainside slides; {king} hears that a hamlet is buried and musters men to rescue.', 'После дождей склон поплыл; {king} слышит, что деревня завалена, и собирает спасателей.', '雨続きの後、山の泥が崩れ落ちた。{king}は村が埋まったと聞き、急いで兵夫を集めて救いに向かわせる。', 'Nach Regentagen gleitet der Hang; {king} hört, dass ein Dorf verschüttet ist, und mustert Retter.'),
 q('疏泥赈户', '疏泥賑戶', 'Clear and relieve', 'Расчистить и помочь', '泥を浚し戸を賑す', 'Räumen und lindern'),
 q('拨银雇夫疏浚，抚恤死伤。', '撥銀僱夫疏濬，撫卹死傷。', 'Hire diggers with silver and comfort the wounded and the bereaved.', 'Нанять землекопов, утешить пострадавших.', '銀を払って人を雇い泥を浚い、死傷者を撫恤する。', 'Mit Silber Wühler anheuern, die Toten und Versehrten trösten.'),
 q('数日而路通，死者得葬，生者得食；{kingdom}库银随减。', '數日而路通，死者得葬，生者得食；{kingdom}庫銀隨減。', 'The road opens in days; the dead are buried, the living fed, and the silver of {kingdom} thins.', 'Через дни дорога открыта; мёртвые погребены, живые накормлены; казна {kingdom} худеет.', '数日で道が通じ、死者は葬られ生者は食を得た。{kingdom}の庫の銀も相応に減った。', 'In Tagen öffnet sich der Weg; die Toten ruhen, die Lebenden essen, und das Silber von {kingdom} schwindet.'),
 q('闻变不发', '聞變不發', 'Hold back', 'Удержаться', '変を知りながら動かず', 'Sich zurückhalten'),
 q('恐民借机索赈，按兵观望。', '恐民藉機索賑，按兵觀望。', 'Fear the victims beg for alms; keep the troops idle.', 'Боясь попрошаек, не двигать войска.', '民が救済を求めにかかるのを恐れ、兵を止めて静観する。', 'Fürchten, die Opfer forderten Almosen; die Truppen stillhalten.'),
 q('淤塞愈甚，饥民鼓噪围城，{kingdom}仓廪危矣。', '淤塞愈甚，饑民鼓譟圍城，{kingdom}倉廩危矣。', 'The mire hardens, hungry crowds besiege the city, and the stores of {kingdom} stand in danger.', 'Ил затвердел, голодные толпы осаждают город; запасы {kingdom} в опасности.', '泥濘はますます激しく、飢えた民が騒いで城を囲む。{kingdom}の倉が危うい。', 'Der Schlamm verhärtet, hungrige Scharen umlagern die Stadt; die Vorräte von {kingdom} sind in Gefahr.')
)
ev('avalanche', 2,
 ('雪崩封途', '雪崩封途', 'The Avalanche', 'Лавина', '雪崩', 'Die Lawine'),
 ('雪峰崩落，声如雷震，{king}闻过往商队尽没谷中，怅立良久。', '雪峰崩落，聲如雷震，{king}聞過往商隊盡沒谷中，悵立良久。', 'The snow peak cracks with a boom like thunder; {king} hears a caravan was buried in the pass and stands long in grief.', 'Снежная вершина сорвалась с громом; {king} слышит, что караван погребён, и долго стоит в горе.', '雪の峰が雷のような音を立てて崩れた。{king}は通りがかりの隊商が谷に消えたと聞き、長く悔いのうちに立つ。', 'Der Schneegipfel bricht mit Donnerkrachen; {king} hört, dass eine Karawane im Pass verschüttet liegt, und steht lange in Trauer.'),
 q('开路搜救', '開路搜救', 'Cut the way in', 'Пробить путь', '道を切って救う', 'Den Weg freilegen'),
 q('雇民破雪开道，寻索全谷。', '僱民破雪開道，尋索全谷。', 'Hire diggers to break the snow and sweep the whole pass.', 'Нанять людей пробивать снег, прочесать ущелье.', '人を雇って雪を破り道を開き、谷全体を探す。', 'Leute dingen, den Schnee zu brechen, den Pass zu durchsuchen.'),
 q('得救者十数人，道亦重修；{kingdom}金银耗费不少。', '得救者十數人，道亦重修；{kingdom}金銀耗費不少。', 'A dozen saved and the road rebuilt; {kingdom} spends no small treasure.', 'Десятки спасены, дорога починена; {kingdom} тратится нешуточно.', '十数人が救われ、道も修り直された。{kingdom}の金銀の費えは小さくなかった。', 'Ein Dutzend gerettet, der Weg wiederhergestellt; {kingdom} zahlt keinen kleinen Schatz.'),
 q('悬告封谷', '懸告封谷', 'Seal the pass', 'Закрыть ущелье', '札を立てて封じる', 'Den Pass sperren'),
 q('悬榜告行人止步，绕道而行。', '懸榜告行人止步，繞道而行。', 'Post notices to halt all travelers and take the detour.', 'Выставить предупреждение, пускать в обход.', '高札を立てて行き人を止め、回り道させる。', 'Anschläge aufstellen, Reisende anhalten und umleiten.'),
 q('谷道不通，商旅改道而行，{kingdom}无损。', '谷道不通，商旅改道而行，{kingdom}無損。', 'The pass stays shut; caravans detour, and {kingdom} loses nothing.', 'Прохода нет; караваны идут в обход; {kingdom} не пострадал.', '谷の道は通れず、隊商は道を変える。{kingdom}に損はない。', 'Der Pass bleibt gesperrt; Karawanen weichen aus, {kingdom} verliert nichts.')
)
ev('lightning_storm', 2,
 ('雷火焚仓', '雷火焚倉', 'Lightning Fire', 'Грозовой пожар', '雷火', 'Blitzbrand'),
 ('雷雨之夜，天火三落，{king}闻粮仓库房俱着，急趋宫中救火。', '雷雨之夜，天火三落，{king}聞糧倉庫房俱著，急趨宮中救火。', 'On a stormy night the sky-fire strikes thrice; {king} hears the granary and stores are alight and rushes to fight the blaze.', 'В грозовую ночь молния бьёт трижды; {king} слышит, что горят житницы и склады, и спешит тушить.', '雷雨の夜、天火が三度落ちた。{king}は糧倉と倉庫が燃えたと聞き、急いで消しに向かう。', 'In einer Gewitternacht schlägt das Himmelsfeuer dreimal ein; {king} hört, dass Kornspeicher und Lager brennen, und eilt zu löschen.'),
 q('救火恤损', '救火恤損', 'Fight and console', 'Тушить и утешать', '消火して損を恤む', 'Löschen und trösten'),
 q('亲督扑救，出银抚恤被火之户。', '親督撲救，出銀撫卹被火之戶。', 'Oversee the dousing and give silver to the burned-out.', 'Лично руководить тушением, платить погорельцам.', '自ら消火を督し、銀を出して火に罹った家を撫恤する。', 'Selbst löschen anführen, den Ausgebrannten Silber geben.'),
 q('火既扑灭，仓米仅损其半；{kingdom}银库微耗。', '火既撲滅，倉米僅損其半；{kingdom}銀庫微耗。', 'The fire is quenched and only half the grain is lost; the silver of {kingdom} dips slightly.', 'Огонь потушен, потеряна лишь половина зерна; казна {kingdom} чуть убыла.', '火は消え、倉の米は半分の損で済んだ。{kingdom}の銀庫はわずかに減った。', 'Das Feuer ist gelöscht, nur halbes Korn verloren; das Silber von {kingdom} sinkt leicht.'),
 q('归罪巫祝', '歸罪巫祝', 'Blame the shamans', 'Обвинить шаманов', '巫祝を咎める', 'Die Schamanen beschuldigen'),
 q('究火因于巫祝，执狱问罪。', '究火因於巫祝，執獄問罪。', 'Sift the fire\'s cause among shamans and hurl them into jail.', 'Искать причину среди шаманов, сажать в тюрьму.', '火の原因を巫祝に求め、捕らえて獄で問う。', 'Die Ursache unter Schamanen suchen und sie ins Gefängnis werfen.'),
 q('巫祝遭殃，民惧牵连；怨怖交加于{kingdom}。', '巫祝遭殃，民懼牽連；怨怖交加於{kingdom}。', 'Shamans suffer and the people fear the net; grievance and terror mingle in {kingdom}.', 'Шаманы страдают, народ боится кары; в {kingdom} страх с обидой.', '巫祝は災いを受け、民は連座を恐れる。{kingdom}に怨みと怖れが交ざる。', 'Die Schamanen leiden, das Volk zittert vor dem Netz; Klage und Schrecken mischen sich in {kingdom}.')
)
ev('drought_wells', 2,
 ('井涸泉竭', '井涸泉竭', 'The Wells Run Dry', 'Колодцы пересохли', '井泉涸れる', 'Die Brunnen versiegen'),
 ('赤地千里，井泉俱涸，{king}亲祀于野，民扶老携幼列道求水。', '赤地千里，井泉俱涸，{king}親祀於野，民扶老攜幼列道求水。', 'Scorched earth stretches far and every well is dry; {king} prays in the open while the people line the roads begging for water.', 'Выжженная земля, все колодцы сухи; {king} молится в поле, а народ просит воды.', '赤土が千里に広がり、井も泉も涸れた。{king}は野で祀を執り行い、民は老いも幼きも連れて道に並んで水を求める。', 'Dürre Erde so weit das Auge reicht, alle Brunnen versiegt; {king} betet auf freiem Feld, das Volk säumt die Wege und fleht um Wasser.'),
 q('凿井引渠', '鑿井引渠', 'Dig wells and ditches', 'Копать колодцы', '井を掘り水路を引く', 'Brunnen und Gräben'),
 q('雇工深凿新井，开渠引远水。', '僱工深鑿新井，開渠引遠水。', 'Hire workers to sink deep wells and lead distant water.', 'Нанять рабочих, копать колодцы, вести воду издалека.', '人を雇って新井を深く掘り、遠くの水へ水路を引く。', 'Arbeiter dingen, tief zu bohren, fernes Wasser zu leiten.'),
 q('新井出水，民心大定；{kingdom}财货为之一空。', '新井出水，民心大定；{kingdom}財貨為之一空。', 'New wells flow and hearts are steadied; the wealth of {kingdom} all but empties.', 'Новые колодцы дали воду, сердца спокойны; богатство {kingdom} истаяло.', '新井が水を出し、民心は大いに定まった。{kingdom}の財貨は空になった。', 'Die neuen Brunnen fließen, die Herzen festigen sich; der Reichtum von {kingdom} ist dahin.'),
 q('摊派水捐', '攤派水捐', 'Levy a water tax', 'Обложить водяным налогом', '水税を課す', 'Wassersteuer erheben'),
 q('向富户强征水捐，贫民出役。', '向富戶強徵水捐，貧民出役。', 'Squeeze a levy from the rich and put the poor to corvee.', 'Обобрать богатых налогом, гнать бедных на работы.', '富家に水の課金を強く徴し、貧民には普請を課す。', 'Die Reichen schröpfen, die Armen zur Fron treiben.'),
 q('富户怨、贫民苦，灾未除而{kingdom}先乱。', '富戶怨、貧民苦，災未除而{kingdom}先亂。', 'The rich resent, the poor suffer, and before the drought lifts {kingdom} seethes.', 'Богатые ропщут, бедные страдают; засуха не кончилась, а {kingdom} уже бурлит.', '富は怨み、貧は苦しむ。災いが去らぬうちに{kingdom}が先に乱れた。', 'Die Reichen grollen, die Armen darben; ehe die Dürre weicht, gärt {kingdom} schon.')
)
ev('river_channel', 2,
 ('大河改道', '大河改道', 'The River Turns', 'Река меняет русло', '大河の改道', 'Der Fluss wendet sich'),
 ('河决于东，改道而南，{king}召工官相河道，见良田悉为浊流。', '河決於東，改道而南，{king}召工官相河道，見良田悉為濁流。', 'The river bursts east and swings south; {king} calls the engineers and sees good fields drowned in the muddy rush.', 'Река прорвалась на востоке и ушла на юг; {king} зовёт мастеров — поля под мутной водой.', '川が東で決壊し、流れを南へ変えた。{king}が工官を呼んで河道を見ると、良田はみな濁流の下にある。', 'Der Fluss bricht im Osten durch und wendet sich nach Süden; {king} ruft die Baumeister und sieht gute Felder in der Flut ersoffen.'),
 q('筑堤束水', '築堤束水', 'Dike the waters', 'Обваловать реку', '堤を築き水を束ねる', 'Die Wasser eindämmen'),
 q('筑新堤引水归槽，改滩为田。', '築新堤引水歸槽，改灘為田。', 'Raise new dikes to steer the flow and till the flats.', 'Возвести дамбы, вернуть воду в русло, распахать отмели.', '新堤を築いて水を元の川床に戻し、洲を田に変える。', 'Neue Dämme bauen, das Wasser ins Bett lenken, die Bänke bestellen.'),
 q('水归故道，新田渐垦；{kingdom}之费不赀。', '水歸故道，新田漸墾；{kingdom}之費不貲。', 'The water returns to its bed and new fields open, at a cost beyond counting for {kingdom}.', 'Вода вернулась, подняты новые поля; {kingdom} заплатил без счёта.', '水は元の道に戻り、新田が開けていく。{kingdom}の費えは測り知れない。', 'Das Wasser kehrt ins Bett, neue Felder entstehen; {kingdom} zahlt unzählig.'),
 q('顺水而居', '順水而居', 'Follow the water', 'Плыть по течению', '水に従って棲む', 'Dem Wasser folgen'),
 q('废故田为泽，迁民于南岸。', '廢故田為澤，遷民於南岸。', 'Yield the old fields to the marsh and move the folk to the south bank.', 'Отдать старые поля болоту, переселить народ на южный берег.', '古い田を沼に譲り、民を南岸へ移す。', 'Die alten Fluren dem Sumpf überlassen, das Volk ans Südufer umsiedeln.'),
 q('民安其新，河亦晏然；{kingdom}国库分文未损。', '民安其新，河亦晏然；{kingdom}國庫分文未損。', 'The people settle and the river runs calm; not a coin leaves the treasury of {kingdom}.', 'Народ обжился, река спокойна; казна {kingdom} не тронута.', '民は新しい土地に安んじ、川も穏やかだ。{kingdom}の国庫は一文も損なわれていない。', 'Das Volk lebt sich ein, der Fluss bleibt ruhig; nicht ein Heller verliert die Kasse von {kingdom}.')
)
ev('locust_larvae', 2,
 ('蝗蝻遍野', '蝗蝻遍野', 'Hoppers in the Fields', 'Личинки наступают', '飛蝗の幼虫', 'Heuschreckenlarven'),
 ('田间蝗蝻无数，见人亦不退避，{king}闻野老言：其父辈已早成蝗灾。', '田間蝗蝻無數，見人亦不退避，{king}聞野老言：其父輩已早成蝗災。', 'Hoppers teem beyond counting and will not flee from men; an elder tells {king}: their fathers already brought locusts.', 'Тучи личинок не бегут и от людей; старик говорит {king}: их отцы уже приносили саранчу.', '田に無数の飛蝗の幼虫がいて、人が近づいても退かない。郷の老人は{king}に言う——その親もすでに蝗災をもたらしたと。', 'Larven wimmeln ohne Zahl und weichen vor Menschen nicht; ein Alter sagt {king}: Ihre Väter brachten schon die Heuschrecken.'),
 q('悬赏捕蝗', '懸賞捕蝗', 'Bounty the hoppers', 'Награда за личинок', '懸賞で駆除', 'Kopfgeld auf Larven'),
 q('出银收蝗，以斗易钱，民皆踊跃。', '出銀收蝗，以斗易錢，民皆踴躍。', 'Buy captured hoppers by the measure, paying silver; the folk flock in.', 'Скупать личинок мерой, платя серебром; народ спешит помочь.', '銀を出して幼虫を斗ごとに買い取る。民はこぞって持ち寄った。', 'Larven kaufen, bezahlt nach Maß mit Silber; das Volk strömt herbei.'),
 q('蝗蝻几绝，又起焚尸之役；{kingdom}耗银颇巨。', '蝗蝻幾絕，又起焚屍之役；{kingdom}耗銀頗巨。', 'The hoppers die out, though burning the corpses follows; {kingdom} spends heavily.', 'Личинки истреблены, но пришлось сжигать туши; {kingdom} потратился изрядно.', '幼虫はほぼ絶えたが、焼き払いの役が続く。{kingdom}の銀の費えは大きかった。', 'Die Larven erlöschen, doch das Verbrennen folgt; {kingdom} zahlt schwer.'),
 q('焚香祷天', '焚香禱天', 'Pray the swarm away', 'Молиться и курить', '香を焚いて祈る', 'Den Schwarm wegbeten'),
 q('设坛祭祀，令民焚香驱蝗。', '設壇祭祀，令民焚香驅蝗。', 'Raise altars and have the people burn incense against them.', 'Поставить алтари, велеть жечь благовония.', '祭壇を設け、民に香を焚かせて蝗を払わせる。', 'Altäre errichten, das Volk Weihrauch gegen die Larven brennen lassen.'),
 q('蝗势愈炽，粮价踊贵，{kingdom}民有菜色。', '蝗勢愈熾，糧價踴貴，{kingdom}民有菜色。', 'The swarm swells, grain prices leap, and hunger shows in the faces of {kingdom}.', 'Тучи растут, хлеб дорожает, лица {kingdom} бледнеют от голода.', '蝗勢はますます猛り、米価が跳ね上がる。{kingdom}の民に青ざめた顔が増えた。', 'Der Schwarm wächst, die Kornpreise springen, Hunger zeichnet die Gesichter von {kingdom}.')
)
ev('crop_blight', 2,
 ('禾疫枯萎', '禾疫枯萎', 'The Blight', 'Гибель хлебов', '穂の疫病', 'Die Fäule'),
 ('田中禾稼成片萎黄，根皆俱黑，{king}遣农官验视，皆言疫气所染。', '田中禾稼成片萎黃，根皆俱黑，{king}遣農官驗視，皆言疫氣所染。', 'Green fields yellow in patches, roots all gone black; the agronomists {king} sends report a blight at work.', 'Поля желтеют пятнами, корни почернели; агрономы {king} докладывают — гибель хлебов.', '田の作物が一帯に黄ばみしおれ、根はみな黒い。{king}が遣わした農官は、皆、疫気に染まったと言う。', 'Die Saat vergilbt in Flecken, Wurzeln ganz schwarz; die Feldmeister von {king} melden eine Fäule.'),
 q('拔除更种', '拔除更種', 'Pull and replant', 'Вырвать и пересеять', '抜き取って植え替え', 'Ausreißen und neu säen'),
 q('焚其病株，改种耐病之谷。', '焚其病株，改種耐病之穀。', 'Burn the sick stands and sow hardier grain.', 'Сжечь больные всходы, сеять стойкое зерно.', '病んだ株を焼き、病に強い穀に替える。', 'Die kranken Stände verbrennen, zäheres Korn säen.'),
 q('焚禾之烟蔽日，来麦得免；{kingdom}费钱不少。', '焚禾之煙蔽日，來麥得免；{kingdom}費錢不少。', 'Smoke dims the sun but the next wheat is spared; {kingdom} pays no small sum.', 'Дым гасит солнце, но будущая пшеница цела; {kingdom} платит немало.', '焼き煙が日を覆うが、来季の麦は免れた。{kingdom}の金の費えは少なくない。', 'Rauch trübt die Sonne, doch der nächste Weizen überlebt; {kingdom} zahlt nicht wenig.'),
 q('任其自灭', '任其自滅', 'Let it burn out', 'Дать выгореть', '自然に絶えるに任せる', 'Ausbrennen lassen'),
 q('不下赈政，唯减价粜粮，以平市价。', '不下賑政，唯減價糶糧，以平市價。', 'Issue no policy; only sell grain cheaply to steady prices.', 'Ничего не менять, лишь продавать зерно дешевле, сбивая цены.', '賑の政は下さず、ただ米を安く売って値を抑える。', 'Keine Politik, nur Korn verbilligt verkaufen, um die Preise zu halten.'),
 q('疫自消退，收成略减，{kingdom}民渐忘其事。', '疫自消退，收成略減，{kingdom}民漸忘其事。', 'The blight fades of itself, the harvest is a little light, and the folk of {kingdom} forget.', 'Болезнь ушла сама, урожай небогат, народ {kingdom} забыл.', '疫は自然に退き、収穫はやや減る。{kingdom}の民は次第に忘れた。', 'Die Fäule weicht von selbst, die Ernte wird mager, das Volk von {kingdom} vergisst.')
)
ev('orchard_wilt', 2,
 ('果林尽凋', '果林盡凋', 'The Orchards Wilt', 'Сады вянут', '果樹園が凋む', 'Die Obstgärten welken'),
 ('果林忽染萎症，果实未熟即落，{king}叹曰：此乃{kingdom}税赋所系。', '果林忽染萎症，果實未熟即落，{king}嘆曰：此乃{kingdom}稅賦所繫。', 'The orchards sicken at once and the fruit drops unripe; {king} sighs that they carry the revenue of {kingdom}.', 'Сады вянут разом, плоды падают незрелыми; {king} вздыхает: на них держится доход {kingdom}.', '果樹園が突然しおれ、熟さぬ実が落ちる。{king}は嘆く——これは{kingdom}の税賦の基だと。', 'Die Gärten erkranken auf einmal, unreife Frucht fällt; {king} seufzt, dass sie die Einkünfte von {kingdom} tragen.'),
 q('伐老植新', '伐老植新', 'Fell and replant', 'Срубить и посадить', '伐って新しく植える', 'Fällen und neu pflanzen'),
 q('伐染疫老树，购新苗补植。', '伐染疫老樹，購新苗補植。', 'Cut down the sick old trees and buy young stock.', 'Свести больные деревья, купить молодняк.', '疫に染まった老木を伐り、新苗を買って植え直す。', 'Die kranken alten Bäume niederlegen, junge Setzlinge kaufen.'),
 q('新苗连片，来年可望；{kingdom}所费亦随之。', '新苗連片，來年可望；{kingdom}所費亦隨之。', 'New saplings stand in rows with hope for next year, and the cost of {kingdom} goes with them.', 'Ряды новых саженцев, надежда на будущее; расходы {kingdom} растут следом.', '新苗が連なり、来年が望める。{kingdom}の費えもそれに伴った。', 'Junge Reihen stehen, Hoffnung fürs Jahr; die Kosten von {kingdom} folgen.'),
 q('刮皮涂灰', '刮皮塗灰', 'Scrape and dress with ash', 'Соскоблить и прижечь золой', '皮を削り灰を塗る', 'Schaben und Äschern'),
 q('教民刮除病皮，涂灰以疗。', '教民刮除病皮，塗灰以療。', 'Teach the folk to scrape the sick bark and dress it with ash.', 'Научить счищать кору, лечить золой.', '民に病皮を削り、灰を塗って癒やす術を教える。', 'Das Volk lehren, die kranke Rinde zu schaben und mit Asche zu behandeln.'),
 q('树活十之七八，农人自足，{kingdom}库未耗。', '樹活十之七八，農人自足，{kingdom}庫未耗。', 'Seven or eight trees in ten recover, the growers cope, and the treasury of {kingdom} is spared.', 'Семь-восемь из десяти ожили; садоводы справились, казна {kingdom} цела.', '十の七八の木は生き返り、農人はうちで足りた。{kingdom}の庫は減らなかった。', 'Sieben, acht Bäume von zehn erholen sich; die Gärtner helfen sich, die Kasse von {kingdom} bleibt.')
)
ev('cattle_pest', 2,
 ('牛疫横行', '牛疫橫行', 'The Cattle Plague', 'Мор скота', '牛の疫病', 'Die Rinderpest'),
 ('耕牛染疫，相继倒毙于野，{king}见春耕即在眼前，驰书访医。', '耕牛染疫，相繼倒斃於野，{king}見春耕即在眼前，馳書訪醫。', 'Draft oxen sicken and die in turn; with spring plowing at hand, {king} sends riders seeking healers.', 'Рабочие волы болеют и падают; близка пахота, {king} шлёт за лекарями.', '耕牛が疫に染まり次々と倒れる。{king}は春耕が迫るのを見て、医師を訪ねる早馬を出す。', 'Zugochsen erkranken und fallen der Reihe nach; die Frühjahrsfurche naht, {king} jagt nach Heilern.'),
 q('重金购牛', '重金購牛', 'Buy new oxen', 'Купить новых волов', '牛を買い足す', 'Neue Ochsen kaufen'),
 q('出银购牛补役，隔离病畜。', '出銀購牛補役，隔離病畜。', 'Spend silver on oxen and pen the sick beasts apart.', 'Потратить серебро на волов, изолировать больных.', '銀を出して牛を買い役に充て、病畜を隔離する。', 'Silber für Ochsen ausgeben, die kranken Tiere absondern.'),
 q('耕牛足用，春耕如常；{kingdom}价昂而银耗。', '耕牛足用，春耕如常；{kingdom}價昂而銀耗。', 'Enough oxen, plowing as usual; {kingdom} pays dear prices and spends silver.', 'Волов хватает, пахота идёт; {kingdom} платит дорого.', '耕牛が揃い春耕は例年通り。{kingdom}は値高く銀を費やした。', 'Genug Ochsen, die Furche wie immer; {kingdom} zahlt hohe Preise.'),
 q('焚牛祭天', '焚牛祭天', 'Burn the oxen', 'Палить скот в жертву', '牛を焚いて祀る', 'Ochsen verbrennen'),
 q('杀牛祭神，祷以求安，冀其平息。', '殺牛祭神，禱以求安，冀其平息。', 'Slay the cattle to placate the gods, praying the pest abates.', 'Резать скот, умилостивляя богов, моля, чтобы мор отступил.', '牛を殺して神に祭り、息災を祈る。', 'Das Vieh schlachten, die Götter zu besänftigen, zu beten, dass die Pest weicht.'),
 q('牛愈少、耕愈急，民怨{king}妄耗耕畜。', '牛愈少、耕愈急，民怨{king}妄耗耕畜。', 'Fewer oxen, ever hastier plowing; the people blame {king} for wasting the draft beasts.', 'Волов меньше, пахота поспешнее; народ винит {king} в растрате.', '牛はますます減り、耕作は急かされる。民は{king}が耕畜を無駄に費やしたと恨む。', 'Weniger Ochsen, immer hastigeres Pflügen; das Volk macht {king} Vorwürfe wegen des Raubbaues.')
)
ev('well_poison', 2,
 ('毒井伤民', '毒井傷民', 'The Poisoned Wells', 'Отравленные колодцы', '毒された井', 'Die giftigen Brunnen'),
 ('数井之水忽作异色，饮者上吐下泻，{king}封井戒严，然谣言已起。', '數井之水忽作異色，飲者上吐下瀉，{king}封井戒嚴，然謠言已起。', 'Several wells turn strange and drinkers sicken violently; {king} seals them and bars the lanes, but rumor already runs.', 'Вода нескольких колодцев изменилась, пьющих рвёт и слабит; {king} запечатывает их, но слухи уже разбежались.', 'いくつかの井の水が妙な色に変わり、飲んだ者が吐き下しを起こす。{king}は井を封じて戒厳するが、噂はもう広がっている。', 'Mehrere Brunnen verfärben sich, Trinker erbrechen sich; {king} versiegelt sie und sperrt die Gassen, doch die Gerüchte laufen schon.'),
 q('淘井投药', '淘井投藥', 'Cleanse the wells', 'Вычерпать и пролечить', '井を浚い薬を投じる', 'Die Brunnen säubern'),
 q('淘尽积水，投药施石灰，遣医巡诊。', '淘盡積水，投藥施石灰，遣醫巡診。', 'Bail out the water, dose it with lime, send physicians on rounds.', 'Осушить воду, пролечить известью, разослать врачей.', '溜まった水を浚い、石灰を施し、医を遣って巡回させる。', 'Das Wasser ausschöpfen, mit Kalk behandeln, Ärzte ausschicken.'),
 q('水复清冽，疫者得治；{kingdom}费银买药。', '水復清冽，疫者得治；{kingdom}費銀買藥。', 'The water runs clear again, the sick recover, and {kingdom} pays for the physic.', 'Вода снова чиста, больные поднялись; {kingdom} заплатил за лекарства.', '水は清く戻り、病者も癒えた。{kingdom}は薬に銀を使った。', 'Das Wasser läuft wieder klar, die Kranken genesen; {kingdom} bezahlt die Arznei.'),
 q('穷索投毒', '窮索投毒', 'Hunt the poisoner', 'Искать отравителя', '毒を盛った者を追う', 'Den Vergifter jagen'),
 q('疑人为下毒，榜掠往来者。', '疑人為下毒，榜掠往來者。', 'Suspect foul play and torture those who pass.', 'Подозревать злодейство, пытать прохожих.', '人工の毒を疑い、行き交う者を拷問する。', 'Ein Anschlag wird vermutet, Passanten werden gefoltert.'),
 q('无辜受刑，真凶未获；{kingdom}人人自危。', '無辜受刑，真兇未獲；{kingdom}人人自危。', 'The innocent suffer and the true hand escapes; everyone in {kingdom} looks over a shoulder.', 'Невинные страдают, виновник ушёл; в {kingdom} каждый ждёт беды.', '無辜の者が拷問され、真犯人は捕まらない。{kingdom}は誰もが自らを危ぶむ。', 'Unschuldige leiden, der wahre Täter entwischt; jedermann in {kingdom} blickt auf seine Schulter.')
)
ev('roof_snow', 2,
 ('积雪崩屋', '積雪崩屋', 'Snow-Buried Roofs', 'Снег давит крыши', '雪で屋根が潰れる', 'Schneelast'),
 ('大雪压榻民屋，仓廪亦颓，{king}闻报亲往视之，见雪深没腰。', '大雪壓榻民屋，倉廩亦頹，{king}聞報親往視之，見雪深沒腰。', 'Heavy snow crushes the homes and collapses the stores; {king} rides out and finds snow waist-deep.', 'Снег давит дома, склады осели; {king} едет смотреть — снег по пояс.', '大雪が民家を押しつぶし、倉も崩れた。{king}は報せを聞いて視に行くと、雪が腰まで深い。', 'Schwerer Schnee zerschlägt die Häuser, die Speicher bersten; {king} reitet hin und findet Schnee bis zur Hüfte.'),
 q('扫雪修屋', '掃雪修屋', 'Clear and mend', 'Чистить и чинить', '雪を除き屋を修める', 'Räumen und flicken'),
 q('雇民扫雪除患，修补民屋。', '僱民掃雪除患，修補民屋。', 'Hire the folk to shovel the snow and patch the roofs.', 'Нанять народ грести снег, латать крыши.', '民を雇って雪を除き、家を繕う。', 'Leute dingen, den Schnee zu schaufeln, die Dächer zu flicken.'),
 q('屋舍得全，灾民得所；{kingdom}银钱花费不可免。', '屋舍得全，災民得所；{kingdom}銀錢花費不可免。', 'Homes are saved and the stricken find lodgings, though {kingdom} cannot avoid the outlay.', 'Дома целы, пострадавшие устроены; {kingdom} не избежал трат.', '家は守られ、被災者も住まいを得た。{kingdom}の銀の出費は避けられない。', 'Die Häuser sind gerettet, die Betroffenen untergebracht; {kingdom} kommt um die Ausgabe nicht herum.'),
 q('责令自扫', '責令自掃', 'Order them to shovel', 'Заставить чистить самих', '自ら除けと命ずる', 'Selbst schaufeln lassen'),
 q('布告民户自扫积雪，逾限有罚。', '佈告民戶自掃積雪，逾限有罰。', 'Decree the folk clear their own snow, with fines for the late.', 'Указ: чистить самим, опоздавшим — штраф.', '各自で雪を除くよう布告し、遅れれば罰する。', 'Dekret: das Volk soll selbst räumen, Säumige werden bestraft.'),
 q('民虽怨而争相除雪，损伤亦不甚重。', '民雖怨而爭相除雪，損傷亦不甚重。', 'Grumbling, the folk dig furiously; the damage stays light.', 'Ворча, народ копает; урон невелик.', '民は怨みながらも競って雪を除き、損傷もそれほど重くはなかった。', 'Murrend gräbt das Volk wetteifernd; der Schaden bleibt leicht.')
)
ev('glacier_melt', 2,
 ('冰融河涨', '冰融河漲', 'The Glacier Melts', 'Таяние ледника', '氷河の融解', 'Der Gletscher schmilzt'),
 ('上游冰川崩融，雪水陡涨，{king}见河谷村落危于旦夕之间。', '上游冰川崩融，雪水陡漲，{king}見河谷村落危於旦夕之間。', 'The glacier above breaks and melts, and the snowmelt suddenly swells; {king} sees the valley hamlets in peril.', 'Ледник сверху рушится и тает, вода внезапно прибывает; {king} видит деревни в опасности.', '上流の氷河が崩れ溶け、雪解け水が急に増す。{king}には谷の村々が危うく見える。', 'Der Gletscher oben bricht und schmilzt, die Schneeschmelze schwillt; {king} sieht die Taldörfer in Gefahr.'),
 q('筑堤迁民', '築堤遷民', 'Dike and resettle', 'Дамба и переселение', '堤を築き民を移す', 'Deichen und umsiedeln'),
 q('筑护堤导水，迁低地之民。', '築護堤導水，遷低地之民。', 'Raise banks to guide the water and move the low-lying folk.', 'Насыпать валы, отвести воду, поднять народ выше.', '護岸を築いて水を導き、低地の民を移す。', 'Uferwälle schütten, das Wasser leiten, die Tiefwohner umsiedeln.'),
 q('村落得保，民皆安迁；{kingdom}公帑费去之多。', '村落得保，民皆安遷；{kingdom}公帑費去之多。', 'The hamlets hold and the people resettle in peace; {kingdom} spends much of the public purse.', 'Деревни целы, народ устроен; казна {kingdom} потратилась изрядно.', '村は守られ、民は皆無事に移った。{kingdom}の公費の費えは大きい。', 'Die Dörfer halten, das Volk zieht in Ruhe um; {kingdom} verbraucht viel aus der Kasse.'),
 q('闭宫自安', '閉宮自安', 'Dwell in comfort', 'Жить в покое', '宮に籠もって安んじる', 'In Ruhe weilen'),
 q('曰冰川之事在天，不及区处。', '曰冰川之事在天，不及區處。', 'Say the glacier\'s doing is heaven\'s, beyond your care.', 'Сказать: лёд — дело небес, не моё.', '氷河のことは天の定めだと、手当てをしない。', 'Sagen, der Gletscher sei des Himmels Werk, darum kümmere man sich nicht.'),
 q('洪流破村，民怨载道，{kingdom}丧其东境。', '洪流破村，民怨載道，{kingdom}喪其東境。', 'The flood breaks the villages, grievance fills the roads, and {kingdom} loses its eastern borderlands.', 'Поток рвёт деревни, гнев на дорогах; {kingdom} теряет восточные земли.', '洪水が村を砕き、道は怨みに満ちる。{kingdom}は東の領土を失った。', 'Die Flut bricht die Dörfer, Klagen füllen die Wege, {kingdom} verliert die Ostmark.')
)
ev('sea_riser', 2,
 ('海潮侵陆', '海潮侵陸', 'The Rising Sea', 'Море наступает', '海が陸を侵す', 'Das Meer steigt'),
 ('海面岁岁而升，沃野渐为卤滩，{king}巡堤四顾，见盐花覆禾。', '海面歲歲而升，沃野漸為鹵灘，{king}巡堤四顧，見鹽花覆禾。', 'The sea climbs year by year and the fat fields grow briny; patrolling the dike, {king} sees salt crusting the grain.', 'Море ползёт вверх, поля становятся солончаком; вдоль дамбы {king} видит соль на колосьях.', '海面は年々上がり、肥えた野が塩の荒れ地になりつつある。堤を巡る{king}には、穂に塩の花が咲いて見える。', 'Das Meer steigt Jahr um Jahr, die fetten Fluren werden salzig; auf dem Deich sieht {king} Salz über dem Korn.'),
 q('筑堤蓄淡', '築堤蓄淡', 'Raise the seawall', 'Поднять дамбу', '堤を築き淡水を守る', 'Den Seewall erhöhen'),
 q('增高海堤，开淡蓄淡排盐。', '增高海堤，開澹蓄淡排鹽。', 'Raise the rampart and skim the salt, storing sweet water.', 'Поднять вал, отводить соль, копить пресную воду.', '海堤を高くし、清水を蓄え塩を排く。', 'Den Wall erhöhen, das Salz auswaschen, Süßwasser sammeln.'),
 q('卤水稍退，田稼渐保；{kingdom}堤工费用浩繁。', '鹵水稍退，田稼漸保；{kingdom}堤工費用浩繁。', 'The brine recedes and crops are saved, at vast cost for the dike works of {kingdom}.', 'Вода отступила, поля целы; работы {kingdom} стоят огромно.', '塩水は少し退き、田が徐々に守られる。{kingdom}の堤工の費えは膨大だ。', 'Die Flut weicht, die Saat hält; die Deichwerke von {kingdom} kosten ungemein.'),
 q('弃地迁民', '棄地遷民', 'Abandon the shore', 'Оставить берег', '地を棄て民を移す', 'Das Ufer aufgeben'),
 q('命民迁离海隅，听任潮侵。', '命民遷離海隅，聽任潮侵。', 'Bid the folk quit the coast and let the tide take it.', 'Велеть народу уйти, отдать землю приливу.', '民に海辺を離れるよう命じ、潮の侵すままに任せる。', 'Das Volk das Ufer räumen lassen, den Gezeiten freien Lauf.'),
 q('乡民不愿离故土，聚众抗争，{kingdom}海边骚然。', '鄉民不願離故土，聚眾抗爭，{kingdom}海邊騷然。', 'Villagers will not leave their soil; they band together and resist, and the coast of {kingdom} seethes.', 'Селяне не хотят уходить, бунтуют; побережье {kingdom} кипит.', '郷民は故郷を離れまいと集まって抗い、{kingdom}の海辺は騒然となる。', 'Die Dörfler wollen die Scholle nicht lassen, rotten sich zusammen; die Küste von {kingdom} gärt.')
)
ev('dust_death', 2,
 ('黑尘伤生', '黑塵傷生', 'The Black Dust', 'Чёрная пыль', '黒い塵', 'Der schwarze Staub'),
 ('黑尘蔽空，连日不散，人畜咳血，{king}令弃尸市外，医人束手。', '黑塵蔽空，連日不散，人畜咳血，{king}令棄屍市外，醫人束手。', 'Black dust veils the sky and will not clear for days; men and beasts cough blood. {king} has the dead carried out, and the healers stand helpless.', 'Чёрная пыль застит небо и не рассеивается днями; люди и звери кашляют кровью. {king} велит выносить мёртвых, лекари бессильны.', '黒い塵が空を覆い、何日も晴れない。人も獣も血を咳く。{king}は死者を城外へ棄てさせたが、医師は手の打ちようがない。', 'Schwarzer Staub verhüllt den Himmel und will tagelang nicht weichen; Mensch und Tier husten Blut. {king} lässt die Toten hinaustragen, die Heiler stehen ratlos.'),
 q('发药赈疾', '發藥賑疾', 'Physic for all', 'Раздать лекарства', '薬を配って病を賑す', 'Arznei für alle'),
 q('施医发药，令民以巾掩口。', '施醫發藥，令民以巾掩口。', 'Treat the sick, hand out physic, bid the folk cover their mouths.', 'Лечить, раздавать снадобья, велеть закрывать лица.', '医を施し薬を配り、民に布で口を覆わせる。', 'Behandeln, Arznei ausgeben, das Volk die Münder verhüllen lassen.'),
 q('病势渐缓，咳血者少；{kingdom}库银支应无算。', '病勢漸緩，咳血者少；{kingdom}庫銀支應無算。', 'The sickness eases and fewer cough blood, though {kingdom} pays out the treasury beyond all count.', 'Болезнь отступает, меньше крови; {kingdom} платит без счёта.', '病勢は和らぎ、血を咳く者も減る。{kingdom}の庫の銀は際限なく出て行った。', 'Die Krankheit weicht, weniger husten Blut; {kingdom} zahlt über alle Maßen.'),
 q('驱病民出', '驅病民出', 'Drive the sick out', 'Изгнать больных', '病民を追い出す', 'Die Kranken vertreiben'),
 q('悉逐病者于城外，城门紧闭。', '悉逐病者於城外，城門緊閉。', 'Expel the ailing beyond the walls and bolt the gates.', 'Выбросить больных за стены, запереть ворота.', '病者を城外に追い出し、門を固く閉ざす。', 'Die Kranken vor die Mauern stoßen und die Tore verriegeln.'),
 q('病民露死郊外，余者惊走，{kingdom}人心离散。', '病民露死郊外，餘者驚走，{kingdom}人心離散。', 'The sick perish in the fields and the rest flee in panic; the heart of {kingdom} scatters.', 'Больные гибнут в полях, остальные бегут; сердце {kingdom} распадается.', '病民は郊外に露死し、残る者は驚いて逃げる。{kingdom}の人心は散り散りになった。', 'Die Kranken verenden draußen, die übrigen fliehen bestürzt; das Herz von {kingdom} zerstreut sich.')
)
ev('earthquake_fissure', 2,
 ('地裂如渊', '地裂如淵', 'The Earth Splits', 'Разлом', '大地が裂く', 'Die Erde klafft'),
 ('地大震，裂堑百里，屋舍沦陷，{king}登楼望之，城中哭声震天。', '地大震，裂塹百里，屋舍淪陷，{king}登樓望之，城中哭聲震天。', 'A great quake splits the ground a hundred miles; houses sink in. From the tower {king} hears the city\'s wailing.', 'Сильный толчок разверзает землю на сто вёрст; дома уходят вниз. С башни {king} слышит плач города.', '大地が激しく震え、百里の裂け目が走り、家が沈んでいく。楼上から{king}が見守る中、城中に泣き声が響く。', 'Ein großes Beben klafft die Erde hundert Meilen; Häuser sinken. Vom Turm hört {king} das Weinen der Stadt.'),
 q('挑土筑路', '挑土築路', 'Fill the chasm', 'Засыпать пропасть', '土を運び道を築く', 'Die Kluft füllen'),
 q('出银雇民填壑架桥，收抚流民。', '出銀僱民填壑架橋，收撫流民。', 'Hire crews to fill the rift, raise bridges, take in refugees.', 'Нанять людей засыпать разлом, строить мосты, принять беженцев.', '銀を出して民を雇い、裂け目を埋め橋を架け、流民を収め慰撫する。', 'Leute dingen, den Spalt zu füllen, Brücken zu bauen, Flüchtlinge aufzunehmen.'),
 q('裂堑渐平，流民复业；{kingdom}国帑空耗。', '裂塹漸平，流民復業；{kingdom}國帑空耗。', 'The chasm slowly closes and refugees return to work, while the treasury of {kingdom} runs hollow.', 'Разлом смыкается, народ возвращается к делу; казна {kingdom} пустеет.', '裂け目は次第に平らぎ、流民も仕事に戻る。{kingdom}の国帑は空費された。', 'Die Kluft schließt sich, die Flüchtlinge kehren zur Arbeit; die Kasse von {kingdom} läuft aus.'),
 q('讳败不报', '諱敗不報', 'Hide the ruin', 'Скрыть беду', '敗を隠し報じない', 'Das Unheil verheimlichen'),
 q('匿灾情而不报，止流民入城。', '匿災情而不報，止流民入城。', 'Conceal the disaster and keep refugees out of the city.', 'Утаить беду, не пускать беженцев в город.', '災情を隠し、流民を城に入れない。', 'Die Not vertuschen, Flüchtlinge nicht in die Stadt lassen.'),
 q('灾情不能上达，饥民聚啸于野，{kingdom}几成草莽。', '災情不能上達，饑民聚嘯於野，{kingdom}幾成草莽。', 'The truth never reaches the court; starving bands howl in the wilds, and {kingdom} turns wild.', 'Правда не доходит до двора; голодные воют в полях, {kingdom} зарастает дичью.', '災情は上に届かず、飢えた民が野に群れ騒ぐ。{kingdom}は草莽の地になりかけた。', 'Die Wahrheit erreicht den Hof nie; hungrige Horden heulen im Ödland, {kingdom} verwildert.')
)
ev('ash_cloud', 2,
 ('灰云蔽日', '灰雲蔽日', 'The Ash Cloud', 'Пепловое облако', '灰の雲', 'Die Aschewolke'),
 ('远山喷火，灰云蔽日三日，{king}闻田禾蒙灰，民多病咽咳。', '遠山噴火，灰雲蔽日三日，{king}聞田禾蒙灰，民多病咽咳。', 'A distant mountain erupts and ash hides the sun three days; {king} hears the crops are blanketed and many throats are sick.', 'Дальняя гора извергается, пепел три дня прячет солнце; {king} слышит, что посевы укрыты пеплом, болят горла.', '遠くの山が噴火し、灰の雲が三日も日を隠す。{king}は田の作物が灰を被り、民の喉を病む者が多いと聞く。', 'Ein ferner Berg bricht aus, Asche verbirgt drei Tage die Sonne; {king} hört, dass die Saat begraben liegt und viele Kehlen kranken.'),
 q('洗禾赈民', '洗禾賑民', 'Wash and relieve', 'Смыть пепел', '穂を洗い民を賑す', 'Waschen und lindern'),
 q('雇民洗禾除灰，发药医咽。', '僱民洗禾除灰，發藥醫咽。', 'Hire hands to rinse the grain and give physic for the throats.', 'Нанять людей мыть зерно, давать снадобья от горла.', '人を雇って穂の灰を洗い、薬を配って喉を治す。', 'Hände dingen, das Korn zu waschen, Arznei für die Kehlen geben.'),
 q('禾上灰除，病者渐愈；{kingdom}耗费如山。', '禾上灰除，病者漸癒；{kingdom}耗費如山。', 'The ash lifts from the grain and the sick recover, as the costs of {kingdom} pile like the cloud.', 'Пепел сошёл, больные поправились; расходы {kingdom} выросли, как туча.', '穂の灰は除かれ、病者も次第に癒える。{kingdom}の費えは山のようになった。', 'Die Asche weicht vom Korn, die Kranken genesen; die Kosten von {kingdom} türmen sich wie die Wolke.'),
 q('拒之宫门', '拒之宮門', 'Bar the palace gate', 'Запереть дворец', '宮門を閉ざす', 'Das Palasttor sperren'),
 q('闭宫门禁灰入，且责民扰。', '閉宮門禁灰入，且責民擾。', 'Shut the palace against the ash and rebuke the people\'s clamor.', 'Закрыть двор от пепла, винить народ за шум.', '宮門を閉ざして灰を入れず、民の騒ぎを責める。', 'Den Hof gegen Asche sperren und das Volk für sein Lärmen rügen.'),
 q('灰入民舍而民病之，怨{king}坐视不救。', '灰入民舍而民病之，怨{king}坐視不救。', 'Ash reaches the homes and illness follows; the people accuse {king} of sitting still and offering no help.', 'Пепел идёт по домам, за ним хворь; народ винит {king} в бездействии и в отказе от помощи.', '灰は民家に入り病が広がる。民は{king}が座視して手を貸さないと恨む。', 'Asche dringt in die Häuser, Krankheit folgt; das Volk beschuldigt {king} des Zusehens, ohne Hilfe zu bieten.')
)
ev('forest_fire', 2,
 ('山火燎原', '山火燎原', 'The Forest Fire', 'Лесной пожар', '山火事', 'Der Waldbrand'),
 ('山火自北林而起，赤焰腾空，{king}见禽兽突入城中，知火势已炽。', '山火自北林而起，赤焰騰空，{king}見禽獸突入城中，知火勢已熾。', 'Fire rises from the northern woods, red flame leaping sky-high; {king} sees beasts burst into the city and knows the blaze is strong.', 'Огонь поднялся над северным лесом, пламя до небес; {king} видит, как звери бегут в город, и понимает: огонь силён.', '北の森から山火事が起こり、赤い炎が空に舞い上がる。獣が城内に飛び込んで来るのを見て、{king}は火勢の熾烈さを知る。', 'Feuer steigt aus dem Nordwald, rote Flammen springen himmelhoch; {king} sieht Tiere in die Stadt stürmen und weiß, der Brand ist stark.'),
 q('辟火断路', '闢火斷路', 'Cut a firebreak', 'Прорубить просеку', '防火線を切る', 'Brandschneise schlagen'),
 q('伐林断路，隔火为界，出银雇夫。', '伐林斷路，隔火為界，出銀僱夫。', 'Fell a corridor to stop the fire, hiring workers with silver.', 'Валить лес на пути огня, нанимая рабочих за серебро.', '木を伐って道を断ち、火を隔てる界を造り、銀で人を雇う。', 'Einen Korridor schlagen, den Brand zu stoppen; Arbeiter mit Silber dingen.'),
 q('火至断线而止，林失小半；{kingdom}银费不赀。', '火至斷線而止，林失小半；{kingdom}銀費不貲。', 'The fire dies at the break, a third of the wood lost; {kingdom} pays beyond counting.', 'Огонь умер на кромке, треть леса погибла; {kingdom} заплатил без счёта.', '火は断ち切った線で止まり、森は小半を失った。{kingdom}の銀の費えは測り知れない。', 'Das Feuer stirbt an der Schneise, ein Drittel des Holzes dahin; {kingdom} zahlt unzählbar.'),
 q('独保宫室', '獨保宮室', 'Save the palace first', 'Сперва дворец', 'まず宮を守る', 'Erst den Palast retten'),
 q('驱民救火，然独先护宫室。', '驅民救火，然獨先護宮室。', 'Drive the people to fight while the court shields its halls.', 'Гнать народ тушить, а двор укрывает свои палаты.', '民を駆って消火させ、まず宮室を守る。', 'Das Volk zum Löschen treiben, während der Hof seine Säle schützt.'),
 q('民居焚毁，民怨{king}视苍生如草芥。', '民居焚毀，民怨{king}視蒼生如草芥。', 'The homes burn and the people cry that {king} counts them like chaff.', 'Дома погорели; народ кричит, что {king} видит в нём траву.', '民家は焼け、民は{king}が蒼生を草芥と見ていると怨む。', 'Die Häuser brennen; das Volk schreit, {king} zähle es wie Stroh.')
)
ev('ice_storm', 2,
 ('冻雨封路', '凍雨封路', 'The Ice Storm', 'Ледяной дождь', '雨氷', 'Der Eisturm'),
 ('冻雨终夜，草木尽覆薄冰，{king}见树枝尽折，行旅断绝于道。', '凍雨終夜，草木盡覆薄冰，{king}見樹枝盡折，行旅斷絕於道。', 'Freezing rain falls all night and glazes every branch; {king} sees limbs snapped and all travel stopped.', 'Ледяной дождь всю ночь стеклит всё; {king} видит обломанные ветви, дороги стоят.', '夜通しの雨氷が草木を薄氷で包む。{king}が見ると枝は折れ、旅は途絶えている。', 'Gefrierregen fällt die ganze Nacht, glasiert Ast und Kraut; {king} sieht gebrochene Zweige, die Wege stehen.'),
 q('破冰通路', '破冰通路', 'Break the ice', 'Растопить лёд', '氷を砕き道を開く', 'Das Eis brechen'),
 q('遣役破冰洒盐，开市通商。', '遣役破冰灑鹽，開市通商。', 'Send crews to chop the ice, spread salt, reopen the trade.', 'Послать людей колоть, солить, открывать торговлю.', '人を遣って氷を砕き塩を撒き、市を開いて商を通す。', 'Trupps schicken, das Eis zu hacken, zu salzen, den Handel zu öffnen.'),
 q('路通而市复，盐银损耗不轻；{kingdom}商旅通行。', '路通而市復，鹽銀損耗不輕；{kingdom}商旅通行。', 'The roads open and the market revives, at no light cost in salt and silver; caravans pass through {kingdom} again.', 'Дороги открыты, рынок ожил; соль и серебро ушли; по {kingdom} вновь идут обозы.', '道は通じ市も復り、塩と銀の損耗は軽くなかった。{kingdom}をまた商旅が行き交う。', 'Wege öffnen sich, der Markt lebt; Salz und Silber kosten nicht wenig, Karawanen ziehen wieder durch {kingdom}.'),
 q('闭市待晴', '閉市待晴', 'Close till the thaw', 'Закрыть до оттепели', '市を閉じて晴れを待つ', 'Schließen bis zum Tau'),
 q('暂闭市廛，令民静候天晴。', '暫閉市廛，令民靜候天晴。', 'Shut the market a while and wait for the skies to clear.', 'Затворить рынок, ждать ясного неба.', 'しばらく市を閉め、民に天気を待たせる。', 'Den Markt schließen und den Himmel abwarten.'),
 q('数日后冻解，市门重启；{kingdom}唯失几日虚日。', '數日後凍解，市門重啟；{kingdom}唯失幾日虛日。', 'In days the ice thaws and the gates reopen; {kingdom} has only lost the empty hours.', 'Через дни лёд сошёл, ворота открыты; потеряны лишь праздные дни.', '数日で凍りが解け、市門が再び開く。{kingdom}は数日の虚日を失っただけだ。', 'Nach Tagen taut das Eis, die Tore öffnen sich; {kingdom} verlor nur leere Stunden.')
)
ev('thunder_harvest', 2,
 ('雷雨伤禾', '雷雨傷禾', 'Thunder over the Harvest', 'Гром над урожаем', '雷雨が穂を打つ', 'Donner über der Ernte'),
 ('麦熟之季，雷雨携雹骤至，{king}见新谷尽偃，雀噪田间啄食。', '麥熟之季，雷雨攜雹驟至，{king}見新穀盡偃，雀噪田間啄食。', 'At wheat-ripeness thunder, rain and hail strike sudden; {king} sees the new grain leveled and sparrows feasting in the fields.', 'К созреванию хлебов пришли гром, дождь и град; {king} видит поваленные колосья, слетелись воробьи.', '麦が熟す季節、雷雨が雹を伴って急にやって来る。{king}の目には新穀が倒れ、雀が田で啄ばむ様が映る。', 'Zur Weizenreife kommen Donner, Regen und Hagel plötzlich; {king} sieht die junge Saat flach und Sperlinge schmausen im Feld.'),
 q('抢收减损', '搶收減損', 'Harvest in haste', 'Спасти урожай', '急いで刈り取る', 'In Eile ernten'),
 q('雇人抢收伏麦，开仓借种。', '僱人搶收伏麥，開倉借種。', 'Hire reapers to save the flattened wheat and lend seed from the stores.', 'Нанять жнецов спасать пшеницу, выдать семена из запасов.', '人を雇って倒れた麦を急いで刈り、倉を開いて種を貸す。', 'Schnitter dingen, das flache Korn zu retten, Saat aus den Speichern leihen.'),
 q('十成存其五，民得种粮；{kingdom}银钱随耗。', '十成存其五，民得種糧；{kingdom}銀錢隨耗。', 'Half the crop survives and seed is secured, while the silver of {kingdom} follows.', 'Спасена половина, семена есть; серебро {kingdom} ушло следом.', '十のうち五は残り、民は種糧を得た。{kingdom}の銀も相応に費やされた。', 'Die halbe Ernte bleibt, die Saat ist gesichert; das Silber von {kingdom} folgt.'),
 q('归罪雷司', '歸罪雷司', 'Blame the thunder priests', 'Обвинить жрецов грома', '雷司を咎める', 'Die Donnerpriester tadeln'),
 q('责雷祀不虔，罚祀官夺俸。', '責雷祀不虔，罰祀官奪俸。', 'Scold the rites as impious, strip the priests of their pay.', 'Обвинить обряды в небрежности, лишить жрецов жалованья.', '雷の祀りが不誠実だと、祀官の俸を奪う。', 'Die Riten als unfromm schelten, den Priestern den Sold nehmen.'),
 q('祀官怨、乡民哗然，皆言{king}狂悖，{kingdom}气象更乱。', '祀官怨、鄉民嘩然，皆言{king}狂悖，{kingdom}氣象更亂。', 'The priests resent and the villages buzz, all calling {king} a madman; the storms of {kingdom} grow wilder.', 'Жрецы обижены, селяне шумят, зовут {king} безумцем; стихия злее.', '祀官は怨み、郷民は騒がしく、皆{king}の狂悖を口にする。{kingdom}の気象はいっそう乱れた。', 'Die Priester grollen, die Dörfer summen, alle rufen {king} einen Narren; das Wetter von {kingdom} wütet.')
)
ev('famine_seed', 2,
 ('饥馑之兆', '饑饉之兆', 'The First Hunger', 'Первые признаки глада', '飢饉の兆', 'Der erste Hunger'),
 ('岁收歉薄，仓廪日空，{king}闻道有流民拾穗而食，心甚忧之。', '歲收歉薄，倉廩日空，{king}聞道有流民拾穗而食，心甚憂之。', 'The harvest is thin and the granaries empty by the day; {king} hears of wanderers gleaning husks along the road and grieves.', 'Урожай скуден, амбары пустеют; {king} слышит о людях, собирающих колосья у дорог, и скорбит.', '収穫は乏しく、倉は日ごとに空になる。{king}は道で落ち穂を拾って食べる流民がいると聞き、心を痛める。', 'Die Ernte ist mager, die Speicher leeren sich; {king} hört von Wanderern, die am Weg Ähren lesen, und trauert.'),
 q('告急于邻', '告急於鄰', 'Appeal to neighbors', 'Воззвать к соседям', '隣国に告げる', 'Nachbarn anrufen'),
 q('开仓平籴，并遣使告难于四邻。', '開倉平糴，並遣使告難於四鄰。', 'Open the stores, sell at fair price, send envoys to the realms.', 'Открыть склады, продавать честно, слать послов.', '倉を開いて米を売り、使いを遣って四隣に苦難を告げる。', 'Speicher öffnen, fair verkaufen, Boten zu den Reichen senden.'),
 q('邻国输粟相助，民稍得食；然饥势未去，秋来恐有大荒。', '鄰國輸粟相助，民稍得食；然饑勢未去，秋來恐有大荒。', 'Neighbors send corn and the folk eat a little, yet the hunger has not left, and a great dearth may come with autumn.', 'Державы везут зерно, народ ест; но голод не ушёл — осенью может грянуть великий глад.', '隣国が粟を送って助け、民も少しは食える。しかし飢勢は去らず、秋には大凶作が来るかもしれない。', 'Die Nachbarn schicken Korn, das Volk isst ein wenig; doch der Hunger ruht nicht, und im Herbst kann die große Not kommen.'),
 q('闭籴卫京', '閉糴衛京', 'Hoard for the capital', 'Запереть хлеб в столицу', '米を都に留める', 'Für die Hauptstadt horten'),
 q('禁粮出境，听其涨价，先保都城。', '禁糧出境，聽其漲價，先保都城。', 'Ban grain from leaving, let prices climb, feed the court city first.', 'Запретить вывоз, дать ценам расти, сперва кормить столицу.', '穀の持ち出しを禁じ、値上がりを黙認してまず都を守る。', 'Ausfuhr verbieten, Preise steigen lassen, zuerst die Stadt am Hofe füttern.'),
 q('米价飞涨，饥民蜂起；饥馑之祸恐自此而萌。', '米價飛漲，饑民蜂起；饑饉之禍恐自此而萌。', 'Prices fly up and famished crowds rise; the great famine may well sprout from this.', 'Цены взлетают, толпы поднимаются; великий голод, похоже, начинается здесь.', '米価が跳ね上がり、飢えた民が蜂起する。飢饉の禍はここから芽生えそうだ。', 'Die Preise schießen, hungrige Scharen erheben sich; die große Not mag hier sprießen.')
)
ev('famine_full', 2,
 ('饿殍遍野', '餓殍遍野', 'The Great Famine', 'Великий голод', '大飢饉', 'Die große Hungersnot'),
 ('饥馑大作，饿殍遍塞于道，{king}减膳撤乐，宫中亦闻野哭。', '饑饉大作，餓殍遍塞於道，{king}減膳撤樂，宮中亦聞野哭。', 'Famine rages and the dead line the roads; {king} cuts the fare and silences the music, yet the weeping still reaches the court.', 'Глад бушует, павшие лежат у дорог; {king} урезает стол, но плач всё равно доходит до дворца.', '飢饉が吹き荒れ、道に餓死者が絶えない。{king}は膳を減らし楽を撤するが、宮中にも野の泣き声が聞こえる。', 'Die Hungersnot wütet, die Toten säumen die Straßen; {king} kürzt Tafel und Musik, doch das Weinen dringt bis in den Hof.'),
 q('倾仓赈灾', '傾倉賑災', 'Empty the granaries', 'Опорожнить житницы', '倉を空にして賑う', 'Die Speicher leeren'),
 q('尽发仓储，遣官四出设粥厂。', '盡發倉儲，遣官四出設粥廠。', 'Send out every store and post officials to open porridge kitchens.', 'Вывезти все запасы, разослать чиновников открывать столовые.', '倉の蓄えを全て出し、官を四方に遣って粥場を設ける。', 'Alle Vorräte ausgeben, Beamte aussenden, Breiküchen zu eröffnen.'),
 q('饿殍稍减而{kingdom}库为之一空，来岁之储亦竭。', '餓殍稍減而{kingdom}庫為之一空，來歲之儲亦竭。', 'Fewer die, yet the treasury of {kingdom} stands empty and next year\'s reserve is spent.', 'Смертей меньше, но казна {kingdom} пуста, и запас на будущий год израсходован.', '餓死者はやや減るが、{kingdom}の庫は空になり、来年の蓄えも尽きた。', 'Weniger sterben, doch die Kasse von {kingdom} steht leer, der Vorrat fürs Jahr dahin.'),
 q('驱民就食', '驅民就食', 'Send them to seek food', 'Отправить искать хлеб', '民を食に赴かせる', 'Sie Nahrung suchen lassen'),
 q('令饥民各自奔逃，去寻有谷之地。', '令饑民各自奔逃，去尋有穀之地。', 'Bid the hungry scatter and seek lands where corn remains.', 'Велеть голодным разбегаться искать хлебные земли.', '飢えた民にそれぞれ逃れて穀のある地を探させよと命じる。', 'Den Hungrigen befehlen, sich zu zerstreuen und Kornland zu suchen.'),
 q('流民塞道，劫掠时闻，{kingdom}盗贼蜂起。', '流民塞道，劫掠時聞，{kingdom}盜賊蜂起。', 'Refugees choke the roads, plunder rings out, and bandits swarm across {kingdom}.', 'Дороги забиты беженцами, гремит грабёж; по {kingdom} плодятся разбойники.', '流民が道を埋め、略奪が続く。{kingdom}に盗賊が蜂起した。', 'Flüchtlinge verstopfen die Wege, Raub hallt; Räuber schwärmen durch {kingdom}.')
)
ev('storm_ruin', 2,
 ('暴风毁屋', '暴風毀屋', 'Storm Wreckage', 'Буря крушит', '嵐が家を砕く', 'Sturmschaden'),
 ('大风挟雨，庐舍楼宇皆摧，{king}见仓廪覆瓦尽飞，知粮米受损。', '大風挾雨，廬舍樓宇皆摧，{king}見倉廩覆瓦盡飛，知糧米受損。', 'Storm and rain flatten hut and tower; {king} sees the granary roofs flying and knows the grain is damaged.', 'Буря с дождём сносит хижины и башни; {king} видит летящие крыши житниц — зерно пострадало.', '風雨が小屋も楼も打ち砕く。{king}は倉の瓦が舞い飛ぶのを見て、糧米が傷んだと知る。', 'Sturm und Regen legen Hütten und Türme flach; {king} sieht Speicherdächer fliegen und weiß: Das Korn leidet.'),
 q('抢粮抚灾', '搶糧撫災', 'Save the grain, aid the folk', 'Спасти зерно', '糧を守り災を撫す', 'Korn retten, das Volk trösten'),
 q('雇人抢晒粮米，拨银修缮民屋。', '僱人搶曬糧米，撥銀修繕民屋。', 'Hire hands to dry the grain and spend silver on the homes.', 'Нанять людей сушить зерно, потратить серебро на дома.', '人を雇って糧米を急いで干し、銀を出して民家を繕う。', 'Hände dingen, das Korn zu trocknen, Silber für die Häuser.'),
 q('粮损十存其七，民居渐修；{kingdom}所费孔殷。', '糧損十存其七，民居漸修；{kingdom}所費孔殷。', 'Seven parts of grain survive and the homes mend, at a very heavy cost for {kingdom}.', 'Спасено семь частей зерна, дома чинятся; расходы {kingdom} велики.', '糧の十の七は残り、民家も直っていく。{kingdom}の費えは甚だ大きい。', 'Sieben Teile Korn überleben, die Häuser heilen; {kingdom} zahlt gar schwer.'),
 q('责民自修', '責民自修', 'Let them mend it', 'Чините сами', '民に自ら修めよと責める', 'Selbst flicken lassen'),
 q('出令民自葺屋舍，官不过问。', '出令民自葺屋舍，官不過問。', 'Decree the folk repair their own roofs; the court keeps out.', 'Велеть народу чиниться самим; двор не вмешивается.', '民に自分の家を葺き直させ、官は関知しない。', 'Dekretieren, das Volk solle sich selbst flicken; der Hof hält sich fern.'),
 q('贫民露宿怨沸，有聚众掠仓之变，{kingdom}始乱。', '貧民露宿怨沸，有聚眾掠倉之變，{kingdom}始亂。', 'The poor sleep under open skies and seethe; crowds raid the granaries, and {kingdom} begins to fall apart.', 'Нищие спят под небом, злятся; толпы громят амбары — {kingdom} начинает разваливаться.', '貧しい民は露宿して怨みが沸く。群衆が倉を掠める騒動が起き、{kingdom}が乱れ始めた。', 'Die Armen schlafen im Freien und gären; Horden plündern die Speicher, {kingdom} beginnt zu zerfallen.')
)
ev('plague_carrier', 2,
 ('疫至边境', '疫至邊境', 'Plague at the Border', 'Мор на границе', '疫が国境へ', 'Die Pest an der Grenze'),
 ('边关报有商旅染疫入境，{king}闻之失色：此类之疫，向来十人九死。', '邊關報有商旅染疫入境，{king}聞之失色：此類之疫，向來十人九死。', 'The border post reports a trader carrying plague across; {king} goes pale — such fevers kill nine in ten.', 'Застава доносит о купце с мором; {king} бледнеет: такие хвори убивают девять из десяти.', '国境の関から、疫に染まった商人の入国が報じられる。{king}は顔色を失う——この類の疫は十人のうち九人が死ぬ。', 'Der Grenzposten meldet einen Händler mit der Pest; {king} erbleicht — solche Fieber töten neun von zehn.'),
 q('封关隔离', '封關隔離', 'Close and isolate', 'Закрыть и изолировать', '関を封じて隔離', 'Schließen und absondern'),
 q('闭边关，设疫所，遣医送药。', '閉邊關，設疫所，遣醫送藥。', 'Seal the pass, raise a lazar-house, send physicians and medicine.', 'Запереть заставы, устроить лазарет, слать врачей и лекарства.', '関を閉じ、疫所を設け、医と薬を遣る。', 'Den Pass sperren, ein Siechenhaus errichten, Ärzte und Arznei senden.'),
 q('疫气未入，人心稍定；{kingdom}关口之费与药价皆昂。', '疫氣未入，人心稍定；{kingdom}關口之費與藥價皆昂。', 'The plague stays out and hearts steady; the pass-garrisons and drugs cost {kingdom} dear.', 'Зараза не вошла, сердца спокойны; заставы и снадобья дорого обошлись {kingdom}.', '疫気は入らず人心もやや定まる。{kingdom}の関の費えも薬の値も高い。', 'Die Seuche bleibt draußen, die Herzen ruhn; Pässe und Drogen kosten {kingdom} teuer.'),
 q('讳疫通关', '諱疫通關', 'Hide and wave him through', 'Скрыть и пропустить', '疫を隠して通す', 'Vertuschen und durchwinken'),
 q('恐扰商道，匿疫不报，任其过关。', '恐擾商道，匿疫不報，任其過關。', 'Fear the trade halt, hide the plague, let him through.', 'Боясь разрыва торговли, скрыть мор, пропустить.', '商道の混乱を恐れ、疫を隠して報ぜず、前を通らせる。', 'Den Handelsstillstand fürchten, die Pest verschweigen, durchlassen.'),
 q('疫入城中，阖城戒惧，{kingdom}怨声载道。', '疫入城中，闔城戒懼，{kingdom}怨聲載道。', 'The plague enters the city, dread grips the streets, and {kingdom} fills with fury.', 'Мор вошёл в город, страх на улицах; {kingdom} кипит от гнева.', '疫は城に入り、全市が震え上がる。{kingdom}に怨みが満ちる。', 'Die Pest erreicht die Stadt, Furcht packt die Gassen, {kingdom} füllt sich mit Wut.')
)
ev('rat_wave', 2,
 ('鼠患成灾', '鼠患成災', 'The Rat Wave', 'Крысиное нашествие', '鼠の害', 'Die Rattenwelle'),
 ('硕鼠成群出没，夜啮仓廪，{king}见米袋皆为鼠窟，顿足长叹。', '碩鼠成群出沒，夜嚙倉廩，{king}見米袋皆為鼠窟，頓足長嘆。', 'Great rats swarm by night and gnaw the bins; {king} sees sacks turned to nests, stamps his foot and sighs.', 'Крупные крысы плодятся по ночам, грызут амбары; {king} видит мешки-гнёзда, топает и вздыхает.', '大いなる鼠が群れを成して出没し、夜ごと倉を齧る。{king}は米の袋が皆鼠の巣になっているのを見て、足を踏み鳴らして嘆く。', 'Große Ratten schwärmen und nagen nachts die Tröge; {king} sieht Säcke voller Nester, stampft und seufzt.'),
 q('悬赏捕鼠', '懸賞捕鼠', 'Bounty the rats', 'Награда за крыс', '懸賞で捕らえる', 'Kopfgeld auf Ratten'),
 q('出银购鼠，令民穿墙堵穴。', '出銀購鼠，令民穿牆堵穴。', 'Buy the rats for silver and have the folk stop the holes.', 'Скупать крыс за серебро, велеть заделывать норы.', '銀を出して鼠を買い、民に塀を穿ち穴を塞がせる。', 'Ratten mit Silber kaufen, das Volk die Löcher stopfen lassen.'),
 q('鼠患稍弭，仓米获安；{kingdom}赏银亦出。', '鼠患稍弭，倉米獲安；{kingdom}賞銀亦出。', 'The vermin ease and the stored corn is safe; {kingdom} pays out the bounty.', 'Твари отступили, зерно в безопасности; {kingdom} выплатил награды.', '鼠の害はやや鎮まり、倉の米は守られた。{kingdom}の賞銀も出て行った。', 'Das Ungeziefer weicht, das Korn ist sicher; {kingdom} zahlt das Kopfgeld.'),
 q('捕民问罪', '捕民問罪', 'Arrest the folk', 'Арестовать народ', '民を捕らえて罪に問う', 'Das Volk verhaften'),
 q('疑民故纵鼠类，下狱拷问。', '疑民故縱鼠類，下獄拷問。', 'Suspect the folk shelter the rats; jail and question them.', 'Заподозрить пособничество, сажать в тюрьму.', '民が鼠を放し飼いにしたと疑い、獄に繋いで問い詰める。', 'Verdächtigen, das Volk halte Ratten; einkerkern und verhören.'),
 q('狱中冤声四起，而鼠愈狂，{kingdom}人皆侧目。', '獄中冤聲四起，而鼠愈狂，{kingdom}人皆側目。', 'Wronged cries fill the prison while the rats grow wilder; all {kingdom} looks askance.', 'В тюрьме вопли невинных, а крысы дерзче; все в {kingdom} косятся.', '獄に無辜の叫びが絶える間なく、鼠はますます猛る。{kingdom}の人は皆、横目で見る。', 'Unschuldsrufe füllen die Kerker, die Ratten werden kecker; alle in {kingdom} blicken schief.')
)
ev('harvest_rot', 2,
 ('新谷尽腐', '新穀盡腐', 'The Grain Rots', 'Зерно сгнило', '新穀の腐敗', 'Das Korn verfault'),
 ('秋收入仓，旬日而谷皆霉烂，{king}亲视仓中，见黑霉如絮。', '秋收入倉，旬日而穀皆霉爛，{king}親視倉中，見黑霉如絮。', 'The harvest is stored, yet within days the grain is all mold; {king} inspects the store and sees black mold like wool.', 'Урожай уложен, но за дни зерно сгнило; {king} осматривает амбары — чёрная плесень, как шерсть.', '秋の収穫を倉に入れると、十日とせず穀物がみな黴で腐った。{king}が倉を見ると、黒い黴が綿のようだ。', 'Die Ernte ist gelagert, doch in Tagen fault das Korn ganz; {king} prüft den Speicher und sieht schwarzen Schimmel wie Wolle.'),
 q('翻仓晒谷', '翻倉曬穀', 'Air the grain', 'Проветрить зерно', '倉を干し穀を晒す', 'Das Korn lüften'),
 q('雇人翻晾仓谷，霉者焚之。', '僱人翻晾倉穀，霉者焚之。', 'Hire hands to turn and air it, burning all the moldy part.', 'Нанять людей ворошить и сушить, гниль сжечь.', '人を雇って倉の穀を翻し干し、黴た分は焚く。', 'Hände dingen, das Korn zu wenden, das Schimmelige zu verbrennen.'),
 q('得谷七成，仓廪气清；{kingdom}工费亦不赀。', '得穀七成，倉廪氣清；{kingdom}工費亦不貲。', 'Seven parts of grain saved and the stores smell fresh, though the labor costs {kingdom} dear.', 'Спасли семь частей, вонь ушла; работы обошлись {kingdom} недёшево.', '穀の七割を得て、倉の匂いも清らかになる。{kingdom}の工費もまた大きかった。', 'Sieben Teile Korn gerettet, die Speicher riechen sauber; die Arbeit kostet {kingdom} nicht wenig.'),
 q('别途另储', '別途另儲', 'Store it elsewhere', 'Хранить в ином месте', '別の場所に貯える', 'Anderswo lagern'),
 q('令免霉之地储粮，余皆陈弃。', '令免霉之地儲糧，餘皆陳棄。', 'Move the grain to drier vaults and let the rest rot.', 'Перевезти зерно в сухие погреба, остальное бросить.', '黴を免れる場所に糧を移し、残りは棄てる。', 'Das Korn in trocknere Gewölbe schaffen, den Rest verwerfen.'),
 q('霉者弃之，存者无损，{kingdom}事无波澜。', '霉者棄之，存者無損，{kingdom}事無波瀾。', 'The moldy is thrown, the sound is unharmed, and {kingdom} passes the matter without a ripple.', 'Гниль выброшена, целое цело; {kingdom} пережил без волнений.', '黴た分は捨て、残った分は傷まない。{kingdom}に波風は立たなかった。', 'Das Schimmelige weggeworfen, das Gute heil; {kingdom} geht die Sache ohne Welle vorbei.')
)
ev('crop_ice', 2,
 ('霜杀禾稼', '霜殺禾稼', 'Frost on the Fields', 'Иней на полях', '霜が穂を殺す', 'Frost auf den Fluren'),
 ('夜霜骤降，青苗尽皆发黑，{king}见残霜未消，心如刀割之痛。', '夜霜驟降，青苗盡皆發黑，{king}見殘霜未消，心如刀割之痛。', 'Night frost falls sudden and turns the young fields black; {king} sees the hoar still clinging, and his heart is cut with a knife.', 'Ночной иней погубил всходы; {king} видит изморозь, и сердце его режется ножом.', '夜の霜が急に降り、若苗が黒く枯れる。{king}は消えぬ霜を見て、心を刀で切られる思いだ。', 'Nachtfrost fällt plötzlich und schwärzt die jungen Fluren; {king} sieht den Reif hängen und spürt das Messer im Herzen.'),
 q('补种赈粥', '補種賑粥', 'Resow and feed', 'Сеять и кормить', 'まき直しと施粥', 'Neu säen und füttern'),
 q('发种令再播，设粥棚于道。', '發種令再播，設粥棚於道。', 'Lend seed for a new sowing and set up porridge tents.', 'Выдать семена на новый посев, открыть столовые.', '種を配ってまき直させ、粥棚を数ヶ所設ける。', 'Saat für eine neue Aussaat geben, Breizelte aufschlagen.'),
 q('苗再青，民不流离；{kingdom}银钱自然花销。', '苗再青，民不流離；{kingdom}銀錢自然花銷。', 'Green returns, none must wander, and the silver of {kingdom} naturally goes.', 'Зелень вернулась, народ на месте; серебро {kingdom} само собой ушло.', '苗は再び青み、民も離散しない。{kingdom}の銀は当然のように費やされた。', 'Das Grün kehrt, niemand muss wandern; das Silber von {kingdom} verrinnt wie von selbst.'),
 q('诿过于天', '諉過於天', 'Blame heaven', 'Свалить на небо', '天のせいにする', 'Dem Himmel die Schuld'),
 q('曰霜乃天意，责民不力自拯。', '曰霜乃天意，責民不力自拯。', 'Call the frost heaven\'s will and scold the folk for idleness.', 'Назвать иней небесной волей, ругать народ за лень.', '霜は天意だとし、民の怠惰を責める。', 'Den Frost Himmelswillen nennen und das Volk für Trägheit schelten.'),
 q('民腹诽怨望，转相诘责，{kingdom}田里不靖。', '民腹誹怨望，轉相詰責，{kingdom}田裡不靖。', 'The people mutter and shift blame on one another; the fields of {kingdom} grow unsettled.', 'Народ ворчит, винит друг друга; поля {kingdom} неспокойны.', '民は胸中で怨み、互いに責め合う。{kingdom}の田里が鎮まらない。', 'Das Volk murmelt und schiebt die Schuld hin und her; die Fluren von {kingdom} werden unruhig.')
)
ev('storm_tide', 2,
 ('风暴涌潮', '風暴湧潮', 'The Storm Tide', 'Штормовой прилив', '高潮', 'Die Sturmflut'),
 ('海风鼓浪，潮越堤顶，{king}见盐田尽咸没，舟子挂在屋角。', '海風鼓浪，潮越堤頂，{king}見鹽田盡鹹沒，舟子掛在屋角。', 'Sea-wind whips the waves and the tide climbs over the dike; {king} sees salt flats drowned and a boat hanging on a rooftop.', 'Ветер гонит волны, прилив перехлёстывает дамбу; {king} видит затопленные солевые поля, лодку на крыше.', '海風が波を立て、潮が堤の頂を越える。{king}には塩田が水に没し、舟が屋根の角に掛かっているのが見える。', 'Seewind peitscht die Wellen, die Flut klettert über den Deich; {king} sieht Salzgärten ersoffen, ein Boot hängt am Dach.'),
 q('加堤安置', '加堤安置', 'Raise and settle', 'Поднять и устроить', '堤を高め安置する', 'Erhöhen und unterbringen'),
 q('增高海堤，安置流离盐户。', '增高海堤，安置流離鹽戶。', 'Heighten the seawall and lodge the ousted salt-folk.', 'Поднять вал, устроить солевых рабочих.', '海堤を高くし、離散した塩戸を落ち着かせる。', 'Den Seewall erhöhen, die vertriebenen Salzleute beherbergen.'),
 q('盐田渐复，民得安居；{kingdom}堤钱随减。', '鹽田漸復，民得安居；{kingdom}堤錢隨減。', 'The salt flats recover and the people rest, though {kingdom} pays for the dike.', 'Поля оживают, народ устроен; {kingdom} платит за вал.', '塩田は復り、民も安居を得る。{kingdom}の堤の費用で銀が減った。', 'Die Gärten erholen sich, das Volk ruht; das Deichgeld von {kingdom} schwindet.'),
 q('驱民祭潮', '驅民祭潮', 'Sacrifice to the tide', 'Принести жертву приливу', '民を駆って潮に祭る', 'Der Flut opfern'),
 q('强驱盐户投牲祭潮，免畏潮神。', '強驅鹽戶投牲祭潮，免畏潮神。', 'Drive the salt-folk to throw beasts into the sea, to fear the tide-god.', 'Гнать людей бросать скот в море, умилостивляя бога прилива.', '塩戸に家畜を海へ投じて潮神を慰めるよう強く迫る。', 'Die Salzleute treiben, Vieh ins Meer zu werfen, den Flutgott zu besänftigen.'),
 q('盐户倾家而无益于潮，怨声涌若潮水。', '鹽戶傾家而無益於潮，怨聲湧若潮水。', 'The salt-folk are ruined and the tide unmoved; their curses swell like the flood.', 'Семьи разорены, приливу безразлично; ропот растёт, как прилив.', '塩戸は財を尽くしながら潮には何の益もなく、怨みが潮のように湧き上がる。', 'Die Salzleute sind ruiniert, die Flut unbewegt; die Klagen schwellen wie die See.')
)
ev('rain_delayed', 2,
 ('甘霖失期', '甘霖失期', 'The Rain Is Late', 'Дождь запаздывает', '雨が遅れる', 'Der Regen säumt'),
 ('雨季无雨，禾苗卷叶，{king}循例涉坛祈雨，叩天竟无应允。', '雨季無雨，禾苗卷葉，{king}循例涉壇祈雨，叩天竟無應允。', 'The rains come not in rain-season and the shoots curl; {king} prays for rain as custom bids — the sky answers nothing.', 'В сезон дождей дождя нет, листья вянут; {king} по обычаю молится — небо молчит.', '雨期に雨がなく、苗の葉が巻く。{king}は慣例に従って雨乞いをするが、天は応えない。', 'Die Regenzeit bringt keinen Regen, die Keime rollen sich; {king} betet, wie die Sitte es will — der Himmel schweigt.'),
 q('开渠引水', '開渠引水', 'Cut channels and water', 'Прорыть каналы', '水路を開き水を引く', 'Gräben ziehen und wässern'),
 q('发银开渠引水，车水救苗。', '發銀開渠引水，車水救苗。', 'Spend silver on ditches and water-wheels to save the shoots.', 'Потратить серебро на каналы и колёса, спасти всходы.', '銀を出して水路を開き、水車で苗を救う。', 'Silber für Gräben und Räder ausgeben, die Keime zu retten.'),
 q('禾苗得救，田水渐盈；{kingdom}库中银米俱减。', '禾苗得救，田水漸盈；{kingdom}庫中銀米俱減。', 'The seedlings are saved and the fields fill, while silver and rice alike dwindle in the stores of {kingdom}.', 'Всходы спасены, поля полны; в казне {kingdom} убыло и серебро, и зерно.', '苗は救われ、田に水が満ちる。{kingdom}の庫から銀も米も減った。', 'Die Keime gerettet, die Felder füllen sich; Silber und Reis in den Speichern von {kingdom} schwinden.'),
 q('焚巫祈雨', '焚巫祈雨', 'Burn the rainmaker', 'Сжечь вызывателя дождя', '巫を焚いて雨を祈る', 'Den Regenmacher verbrennen'),
 q('执巫师燔于郊，以乱天听。', '執巫師燔於郊，以亂天聽。', 'Seize the witch and burn her in the fields to move the sky.', 'Схватить ведьму, сжечь в поле, чтобы пронять небо.', '巫師を捕えて郊で焚き、天を動かそうとする。', 'Die Hexe greifen und im Feld verbrennen, den Himmel zu rühren.'),
 q('天不雨而民先哗，{king}之暴名遍传。', '天不雨而民先嘩，{king}之暴名遍傳。', 'The sky stays dry while the people raise their voices; the cruel name of {king} spreads abroad.', 'Небо молчит, народ шумит; жестокая слава {king} расходится.', '天は雨を降らせず、民が先に騒ぎ立てる。{king}の暴名が広く伝わる。', 'Der Himmel bleibt trocken, das Volk erhebt den Lärm; der grausame Name von {king} verbreitet sich.')
)
ev('grass_fire', 2,
 ('牧草连火', '牧草連火', 'The Steppe Fire', 'Степной пожар', '草原の火', 'Der Steppenbrand'),
 ('大旱不雨，牧草自燃而焚，{king}闻牛马惊走，望见浓烟蔽野。', '大旱不雨，牧草自燃而焚，{king}聞牛馬驚走，望見濃煙蔽野。', 'A great drought brings no rain and the pasture ignites itself; {king} hears the cattle bolt and sees smoke cover the plains.', 'В засуху степь загорается сама; {king} слышит топот стада и видит дым над равниной.', '大旱で雨がなく、牧草が自ら燃える。{king}は牛馬が驚いて駆けるのを聞き、草原を覆う濃煙を見る。', 'Große Dürre ohne Regen, das Weideland entzündet sich selbst; {king} hört das Vieh stürmen und sieht Rauch die Ebene decken.'),
 q('辟火护牧', '闢火護牧', 'Cut to guard the herds', 'Рубить и стеречь', '火を断ち牧を守る', 'Schneiden und Herden schützen'),
 q('雇民辟火道，驱畜而分群。', '僱民闢火道，驅畜而分群。', 'Hire crews to cut firebreaks and split the herds apart.', 'Нанять людей рубить просеки, разогнать стада.', '人を雇って火道を切り、獣を追って群を分ける。', 'Leute dingen für Brandschneisen, die Herden teilen.'),
 q('火止于界，畜群尽全；{kingdom}工费所耗如何。', '火止於界，畜群盡全；{kingdom}工費所耗如何。', 'The fire stops at the line and the herds are whole, at whatever the hired work cost {kingdom}.', 'Огонь замер на кромке, стада целы; работа стоила {kingdom} немало.', '火は界で止まり、群れは全て無事。{kingdom}の工費の費えもまた大きい。', 'Das Feuer stirbt an der Linie, die Herden heil; was die Arbeit {kingdom} kostet, ist nicht wenig.'),
 q('静候火熄', '靜候火熄', 'Wait for it to die', 'Переждать пожар', '火が熄むのを待つ', 'Auf das Ende warten'),
 q('缘火自烧自灭，禁人近之。', '緣火自燒自滅，禁人近之。', 'Let the fire burn its course and keep men away.', 'Дать огню выгореть, людей не подпускать.', '火は燃えて自ら滅ぶものと、人を近づけぬ。', 'Den Brand ausbrennen lassen und die Leute fernhalten.'),
 q('火延三日而自熄，草灰足以为肥。', '火延三日而自熄，草灰足以為肥。', 'The fire runs three days and dies; the ash is rich enough to feed the grass.', 'Пожар выгорел за три дня; пепла хватит на удобрение.', '火は三日で自ら熄え、草灰は肥料に足りるほど残った。', 'Der Brand erlischt nach drei Tagen; die Asche nährt das Gras.')
)
ev('river_ice', 2,
 ('河冰阻漕', '河冰阻漕', 'The River Freezes', 'Река скована льдом', '河の氷が漕を阻む', 'Der Fluss friert zu'),
 ('严冬大河尽封冻，漕船不行，{king}见粮价浮动，市贾观望。', '嚴冬大河盡封凍，漕船不行，{king}見糧價浮動，市賈觀望。', 'Hard winter locks the great river and the grain barges stand still; {king} sees prices waver and merchants watch.', 'Суровая зима сковала реку, баржи с зерном стоят; {king} видит, как цены качаются, купцы ждут.', '厳冬に大河が凍り、漕ぎ船が動かない。{king}には米価が揺れ動き、市の商人が眺めているのが見える。', 'Harter Winter friert den großen Fluss, die Kornkähne stehen; {king} sieht Preise wanken und Kaufleute warten.'),
 q('破冰通漕', '破冰通漕', 'Breach the ice', 'Расколоть лёд', '氷を破り漕を通す', 'Das Eis brechen'),
 q('雇民凿冰破冻，疏道通船。', '僱民鑿冰破凍，疏道通船。', 'Hire the folk to chop a channel for the boats.', 'Нанять людей прорубать канал для барж.', '民を雇って氷を鑿ち、道を浚って舟を通す。', 'Leute dingen, eine Rinne für die Kähne zu hauen.'),
 q('漕粮再通，粮价回落；{kingdom}凿冰之费孔急。', '漕糧再通，糧價回落；{kingdom}鑿冰之費孔急。', 'The river-grain flows again and prices fall, while {kingdom} is hard pressed for the ice-cutting costs.', 'Зерно пошло, цены упали; счета за лёд {kingdom} кусаются.', '漕糧は再び通り、米価も下がる。{kingdom}の氷を鑿つ費えは痛い。', 'Das Flusskorn fließt wieder, die Preise fallen; die Eishack-Rechnungen von {kingdom} beißen.'),
 q('缓漕待春', '緩漕待春', 'Wait for spring', 'Ждать весны', '漕を緩めて春を待つ', 'Auf den Frühling warten'),
 q('顺河之性，遣令缓运待冰解。', '順河之性，遣令緩運待冰解。', 'Yield to the river and order the barge-fleet to mark time.', 'Уступить реке, велеть флоту переждать.', '川の性に任せ、氷の解けるまでゆっくり運べと命じる。', 'Dem Fluss nachgeben und die Flotte anhalten lassen.'),
 q('春来冰解，漕运复行；{kingdom}粮价微涨而安。', '春來冰解，漕運復行；{kingdom}糧價微漲而安。', 'In spring the ice gives and the barges resume; prices in {kingdom} rise a little, yet all is calm.', 'Весной лёд сошёл, баржи пошли; цены чуть выше, но всё спокойно.', '春に氷が解け、漕運が再び行われる。{kingdom}の米価は僅かに上がるが皆安らかだ。', 'Im Frühjahr gibt das Eis nach, die Kähne fahren; die Preise von {kingdom} steigen leicht, doch alles bleibt ruhig.')
)
