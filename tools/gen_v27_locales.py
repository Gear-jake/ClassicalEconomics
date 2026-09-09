# -*- coding: utf-8 -*-
"""v2.0.7: 10 chain/quick events x 6 languages. Idempotent (only fills missing keys)."""
import io, json, collections, os

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
os.chdir(ROOT)

EVENTS = collections.OrderedDict()
def ev(eid, opts, title, desc, *texts):
    EVENTS[eid] = (opts, title, desc, list(texts))
def q(*args): return args

# ===== 王朝兴衰四幕（10 年链）=====
ev('dynasty_peak', 2,
 ('王朝鼎盛', '王朝鼎盛', 'The Dynasty\'s Prime', 'Расцвет династии', '王朝の最盛期', 'Die Blüte der Dynastie'),
 ('四海升平，列国来朝——{king}的王朝正处鼎盛，一举一动皆有深意。', '四海昇平，列國來朝——{king}的王朝正處鼎盛，一舉一動皆有深意。', 'Sea-calm seas and tributes — {king}\'s dynasty is in its prime; every move matters.', 'Моря спокойны, дары текут — династия {king} в расцвете; каждый шаг важен.', '四海昇平、列国が来朝——{king}の王朝は最盛期、一挙一動が重い。', 'Ruhige Meere und Tribut — die Dynastie von {king} ist in Blüte; jeder Schritt zählt.'),
 q('广纳贤才', '廣納賢才', 'Gather the worthy', 'Собрать достойных', '賢を集める', 'Die Würdigen sammeln'),
 q('延揽四方贤士，充实朝堂。', '延攬四方賢士，充實朝堂。', 'Recruit the worthy from all sides; enrich the court.', 'Собрать достойных со всех сторон; обогатить двор.', '四方の賢を集め朝堂を満たす。', 'Die Würdigen von überall anwerben; den Hof bereichern.'),
 q('贤才云集，{kingdom}的国运如日中天。', '賢才雲集，{kingdom}的國運如日中天。', 'The worthy gather; the fortune of {kingdom} peaks.', 'Достойные собрались; удача {kingdom} на пике.', '賢が集い{kingdom}の国運が最高潮へ。', 'Die Würdigen sammeln sich; das Glück von {kingdom} erreicht seinen Höhepunkt.'),
 q('充实国库', '充實國庫', 'Fill the chest', 'Наполнить казну', '国庫を満たす', 'Die Kasse füllen'),
 q('趁鼎盛之期广开税源。', '趁鼎盛之期廣開稅源。', 'Open wide the taxes in prime time.', 'Расширить налоги в пору расцвета.', '最盛期に税源を広げる。', 'Die Steuern in der Blütezeit ausweiten.'),
 q('国库渐盈，{kingdom}的底气更足了。', '國庫漸盈，{kingdom}的底氣更足了。', 'The chest fills; {kingdom} stands taller.', 'Казна полнится; {kingdom} стоит выше.', '国庫が満ち{kingdom}の底力が増す。', 'Die Kasse füllt sich; {kingdom} steht höher.')
)
ev('dynasty_crisis', 2,
 ('王朝危机', '王朝危機', 'Dynasty Crisis', 'Кризис династии', '王朝の危機', 'Dynastiekrise'),
 ('盛极而衰——灾异频仍、民怨四起，{king}的王朝迎来第一场大考。', '盛極而衰——災異頻仍、民怨四起，{king}的王朝迎來第一場大考。', 'From peak to decline — omens and complaints; {king}\'s dynasty faces its first trial.', 'От расцвета к спаду — знамения и ропот; династия {king} проходит первое испытание.', '盛極は衰え——異変が相次ぎ民怨が起こり{king}の王朝が最初の試練へ。', 'Vom Gipfel zum Fall — Vorzeichen und Klagen; die Dynastie von {king} steht vor der ersten Prüfung.'),
 q('铁腕镇压', '鐵腕鎮壓', 'Crush the dissent', 'Подавить ропот', '強権で鎮圧', 'Den Widerspruch zermalmen'),
 q('以雷霆手段弹压民变。', '以雷霆手段彈壓民變。', 'Suppress the uprisings with thunder.', 'Подавить восстания громом.', '雷の勢いで民変を弾圧する。', 'Die Aufstände mit Donner unterdrücken.'),
 q('怨声暂伏，{kingdom}的隐患却在暗处发酵。', '怨聲暫伏，{kingdom}的隱患卻在暗處發酵。', 'The grumbling hides; the rot ferments in the dark.', 'Ропот затихает; гниение бродит в темноте.', '怨声は潜むが{kingdom}の病巣が暗所で発酵する。', 'Das Murren versteckt sich; die Fäulnis gärt im Dunkeln.'),
 q('赈济抚民', '賑濟撫民', 'Relieve the people', 'Утешить народ', '民を撫す', 'Das Volk trösten'),
 q('开仓放粮，绥靖人心。', '開倉放糧，綏靖人心。', 'Open the granaries; pacify the hearts.', 'Открыть житницы; умиротворить сердца.', '倉を開き人心をなだめる。', 'Die Speicher öffnen; die Herzen besänftigen.'),
 q('民心稍安，但国库付出重创。', '民心稍安，但國庫付出重創。', 'Hearts ease a little; the chest takes a heavy hit.', 'Сердца успокаиваются; казна получает удар.', '民心は安らぐが国庫が重傷を負う。', 'Die Herzen beruhigen sich; die Kasse nimmt einen schweren Treffer.')
)
ev('dynasty_fall', 2,
 ('王朝倾覆', '王朝傾覆', 'The Dynasty Falls', 'Падение династии', '王朝の崩壊', 'Der Fall der Dynastie'),
 ('大厦将倾——饥馑与乱军同时扑向{kingdom}，百年基业摇摇欲坠。', '大廈將傾——饑饉與亂軍同時撲向{kingdom}，百年基業搖搖欲墜。', 'The house tilts — famine and mutiny strike {kingdom} at once; a century\'s work shakes.', 'Дом кренится — голод и мятеж бьют {kingdom} разом; век труда шатается.', '大廈が傾く——飢饉と乱軍が同時に{kingdom}を襲い百年の基業が揺らぐ。', 'Das Haus neigt sich — Hunger und Meuterei treffen {kingdom} zugleich; das Jahrhundertwerk wankt.'),
 q('死守基业', '死守基業', 'Hold the legacy', 'Держать наследие', '基業を守る', 'Das Erbe halten'),
 q('倾尽府库，与乱军周旋到底。', '傾盡府庫，與亂軍周旋到底。', 'Empty the treasuries; fight the mutiny to the end.', 'Опустошить казну; драться до конца.', '府庫を尽くし乱軍と戦い抜く。', 'Die Schatzkammern leeren; gegen die Meuterei bis zum Ende kämpfen.'),
 q('基业暂保，{kingdom}却被掏空了筋骨。', '基業暫保，{kingdom}卻被掏空了筋骨。', 'The legacy holds a while; {kingdom} is hollowed out.', 'Наследие держится; {kingdom} истощён.', '基業は保つが{kingdom}は骨まで削られる。', 'Das Erbe hält; {kingdom} wird ausgehöhlt.'),
 q('弃都自保', '棄都自保', 'Flee the capital', 'Оставить столицу', '都を捨てる', 'Die Hauptstadt aufgeben'),
 q('迁都避祸，保存宗庙于新土。', '遷都避禍，保存宗廟於新土。', 'Move the capital; keep the shrines on fresh land.', 'Перенести столицу; сохранить святыни на новых землях.', '遷都して宗廟を新たな地に守る。', 'Die Hauptstadt verlegen; die Schreine auf frischem Land wahren.'),
 q('宗庙得存，{kingdom}的旧都却成了一片焦土。', '宗廟得存，{kingdom}的舊都卻成了一片焦土。', 'The shrines survive; the old capital of {kingdom} turns to ash.', 'Святыни выжили; старая столица {kingdom} в пепле.', '宗廟は残るが{kingdom}の旧都は焦土となる。', 'Die Schreine überleben; die alte Hauptstadt von {kingdom} fällt in Asche.')
)
ev('dynasty_rise', 2,
 ('王朝再兴', '王朝再興', 'The Dynasty Rises Again', 'Возрождение династии', '王朝の再興', 'Die Dynastie erhebt sich'),
 ('灰烬中长出新芽——{kingdom}的耐心与坚韧开始得到回报。', '灰燼中長出新芽——{kingdom}的耐心與堅韌開始得到回報。', 'New shoots from the ashes — patience and grit of {kingdom} pay off.', 'Новые побеги из пепла — терпение и упорство {kingdom} вознаграждаются.', '灰の中から新芽が——{kingdom}の忍耐と強靭さが報われ始める。', 'Neue Triebe aus der Asche — Geduld und Zähigkeit von {kingdom} zahlen sich aus.'),
 q('轻徭薄赋', '輕徭薄賦', 'Lighten the levies', 'Облегчить повинности', '軽徭薄賦', 'Die Lasten mindern'),
 q('减税轻徭，让民力休养生息。', '減稅輕徭，讓民力休養生息。', 'Cut taxes and corvées; let the people breathe.', 'Снизить налоги и повинности; дать народу вздохнуть.', '税と徭を軽くし民力を養う。', 'Steuern und Fron senken; das Volk atmen lassen.'),
 q('民力复苏，{kingdom}重新走上壮大之路。', '民力復甦，{kingdom}重新走上壯大之路。', 'The people recover; {kingdom} ascends anew.', 'Народ восстанавливается; {kingdom} снова восходит.', '民が蘇り{kingdom}が再び強大への道へ。', 'Das Volk erholt sich; {kingdom} steigt erneut auf.'),
 q('无为而治', '無為而治', 'Let it be', 'Пусть идёт', '無為', 'Geschehen lassen'),
 q('不多干预，静待自愈。', '不多干預，靜待自愈。', 'Intervene little; wait for self-healing.', 'Не вмешиваться; ждать самоисцеления.', '干渉せず自らの癒えを待つ。', 'Wenig eingreifen; auf Selbstheilung warten.'),
 q('不急不躁，{kingdom}的根基悄然稳固。', '不急不躁，{kingdom}的根基悄然穩固。', 'No haste; the roots of {kingdom} quietly firm up.', 'Без спешки; корни {kingdom} тихо крепнут.', '焦らず{kingdom}の基盤が静かに固まる。', 'Keine Eile; die Wurzeln von {kingdom} festigen sich leise.')
)

# ===== 国祚盛衰五幕（20 年链）=====
ev('realm_birth', 2,
 ('国祚初立', '國祚初立', 'A Realm Is Born', 'Рождение державы', '国祚の初め', 'Ein Reich wird geboren'),
 ('新国初立，百事待举——年少的{kingdom}在旧秩序的废墟上开始耕耘。', '新國初立，百事待舉——年少的{kingdom}在舊秩序的廢墟上開始耕耘。', 'A new realm stands — young {kingdom} tills the ruins of the old order.', 'Новая держава встаёт — юный {kingdom} пашет руины старого порядка.', '新国が立ち、若き{kingdom}が旧秩序の廃墟を耕す。', 'Ein neues Reich steht — das junge {kingdom} pflügt die Ruinen der alten Ordnung.'),
 q('兴修土木', '興修土木', 'Raise the works', 'Поднять стройки', '土木を興す', 'Die Werke heben'),
 q('建城修路，勾勒国家的筋骨。', '建城修路，勾勒國家的筋骨。', 'Build cities and roads; sketch the nation\'s frame.', 'Строить города и дороги; начертить каркас державы.', '城と道を築き国の骨格を描く。', 'Städte und Straßen bauen; den Rahmen der Nation zeichnen.'),
 q('城路初成，{kingdom}的脊梁挺了起来。', '城路初成，{kingdom}的脊梁挺了起來。', 'Cities and roads first take shape; the spine of {kingdom} rises.', 'Города и дороги обретают формы; хребет {kingdom} встаёт.', '城と道が形になり{kingdom}の背骨が立つ。', 'Städte und Straßen nehmen Gestalt an; das Rückgrat von {kingdom} erhebt sich.'),
 q('休养生息', '休養生息', 'Recover in peace', 'Отдыхать в мире', '休息を優先', 'In Ruhe erholen'),
 q('暂缓大兴，先让民力蓄积。', '暫緩大興，先讓民力蓄積。', 'Hold the great works; let the people store strength.', 'Отложить большие работы; пусть народ копит силы.', '大興を控え民力の蓄積を優先する。', 'Die großen Werke vertagen; das Volk soll Kräfte speichern.'),
 q('民力渐蓄，{kingdom}的根基更厚实了。', '民力漸蓄，{kingdom}的根基更厚實了。', 'Strength accumulates; the foundations of {kingdom} thicken.', 'Силы копятся; основания {kingdom} толстеют.', '民力が蓄わり{kingdom}の土台が厚くなる。', 'Die Kräfte sammeln sich; die Fundamente von {kingdom} verdicken sich.')
)
ev('realm_growth', 2,
 ('国势日隆', '國勢日隆', 'The Realm Ascends', 'Держава восходит', '国勢の隆盛', 'Das Reich steigt auf'),
 ('商旅辐辏，财赋日丰——{kingdom}进入了厚积薄发的年代。', '商旅輻輳，財賦日豐——{kingdom}進入了厚積薄發的年代。', 'Traders crowd in, wealth mounts — {kingdom} enters the age of accumulated momentum.', 'Торговцы теснятся, богатство растёт — {kingdom} входит в эпоху накопленного разгона.', '商旅が集い財が日々増す——{kingdom}は蓄積の時代へ。', 'Händler drängen sich, Reichtum wächst — {kingdom} tritt ins Zeitalter der gesammelten Kraft.'),
 q('理财聚财', '理財聚財', 'Manage the wealth', 'Управлять богатством', '財を理める', 'Den Reichtum verwalten'),
 q('广开财路，充盈国库。', '廣開財路，充盈國庫。', 'Open every drain; fill the chest.', 'Открыть все источники; наполнить казну.', '財路を広げ国庫を満たす。', 'Alle Quellen öffnen; die Kasse füllen.'),
 q('府库充实，{kingdom}的底气更足了。', '府庫充實，{kingdom}的底氣更足了。', 'The chest swells; {kingdom} stands tall.', 'Казна полнеет; {kingdom} стоит высоко.', '府庫が満ち{kingdom}の底力が増す。', 'Die Kasse schwillt; {kingdom} steht hoch.'),
 q('重农抑商', '重農抑商', 'Farm over trade', 'Пашня прежде торговли', '農を重んじ商を抑える', 'Ackerbau vor Handel'),
 q('稳定农本，防范市井之浮。', '穩定農本，防範市井之浮。', 'Steady the farm base; guard against market froth.', 'Укрепить основу пашни; остеречься рыночной пены.', '農を固め市の浮きを防ぐ。', 'Die Ackergrundlage festigen; der Marktschäumerei wehren.'),
 q('农本稳固，{kingdom}的发展少了些虚火。', '農本穩固，{kingdom}的發展少了些虛火。', 'The farm base holds; {kingdom}\'s growth loses its froth.', 'Пашня держится; рост {kingdom} теряет пену.', '農が固まり{kingdom}の発展が落ち着く。', 'Die Ackergrundlage hält; das Wachstum von {kingdom} verliert die Blase.')
)
ev('realm_storm', 2,
 ('风暴骤至', '風暴驟至', 'The Storm Breaks', 'Буря прорывается', '嵐が来る', 'Der Sturm bricht los'),
 ('积攒多年的张力一朝释放——{kingdom}与强邻的战争机器同时发动。', '積攢多年的張力一朝釋放——{kingdom}與強鄰的戰爭機器同時發動。', 'Years of tension release at once — the war machines of {kingdom} and its strong neighbor ignite.', 'Годы напряжения разряжаются разом — военные машины {kingdom} и сильного соседа вспыхивают.', '積年の緊張が一気に解け、{kingdom}と強隣の戦争機構が始動する。', 'Jahre der Spannung entladen sich — die Kriegsmaschinen von {kingdom} und seines starken Nachbarn zünden.'),
 q('主动出击', '主動出擊', 'Strike first', 'Ударить первым', '先手を打つ', 'Zuerst zuschlagen'),
 q('向当前敌国宣战，先发制人。', '向當前敵國宣戰，先發制人。', 'Declare war on the current foe; strike preemptively.', 'Объявить войну текущему врагу; ударить упреждающе.', '現在の敵国へ宣戦し先手を取る。', 'Dem aktuellen Feind den Krieg erklären; präventiv zuschlagen.'),
 q('战端一启，{kingdom}的积蓄狂泻如注。', '戰端一啟，{kingdom}的積蓄狂瀉如注。', 'War opens; {kingdom}\'s savings pour down the drain.', 'Война открыта; сбережения {kingdom} стекают.', '戦端が開かれ{kingdom}の蓄えが流れ出る。', 'Der Krieg beginnt; die Ersparnisse von {kingdom} fließen davon.'),
 q('列国斡旋', '列國斡旋', 'Broker peace', 'Хлопотать о мире', '列国に働きかける', 'Frieden vermitteln'),
 q('请列国调停，暂避兵锋。', '請列國調停，暫避兵鋒。', 'Ask the powers to mediate; dodge the blade.', 'Просить державы о посредничестве; уклониться от клинка.', '列国に調停を求め刃を避ける。', 'Die Mächte um Vermittlung bitten; der Klinge ausweichen.'),
 q('兵锋暂避，{kingdom}的气焰却挫了几分。', '兵鋒暫避，{kingdom}的氣焰卻挫了幾分。', 'The blade is dodged; {kingdom}\'s flame banks a little.', 'Клинок обойдён; пламя {kingdom} чуть гаснет.', '刃は避けたが{kingdom}の勢いが少し削がれる。', 'Der Klinge ausgewichen; die Flamme von {kingdom} glimmt schwächer.')
)
ev('realm_ruin', 2,
 ('国祚崩摧', '國祚崩摧', 'The Realm Ruins', 'Держава рушится', '国祚の崩壊', 'Das Reich verfällt'),
 ('连年战乱与饥馑把{kingdom}拖入深渊——商路断绝、田园荒芜、忠臣凋零。', '連年戰亂與饑饉把{kingdom}拖入深淵——商路斷絕、田園荒蕪、忠臣凋零。', 'Years of war and famine drag {kingdom} into the abyss — routes cut, fields bare, loyal ministers gone.', 'Годы войн и голода тянут {kingdom} в бездну — пути отрезаны, поля пусты, верные министры ушли.', '戦乱と飢饉が{kingdom}を深淵へ——商路断絶、田園荒廃、忠臣凋落。', 'Jahre von Krieg und Hunger ziehen {kingdom} in den Abgrund — Wege gekappt, Felder kahl, treue Minister fort.'),
 q('沉疴猛药', '沉痾猛藥', 'Harsh remedy', 'Суровое лекарство', '猛薬', 'Harte Kur'),
 q('当机立断，倾库止损。', '當機立斷，傾庫止損。', 'Decide now; empty the chest to stop the bleeding.', 'Решить сейчас; опустошить казну, чтобы остановить кровь.', '即断し府庫を尽くして損を止める。', 'Jetzt entscheiden; die Kasse leeren, um das Bluten zu stoppen.'),
 q('壮士断腕，{kingdom}的元气大伤却保全了根本。', '壯士斷腕，{kingdom}的元氣大傷卻保全了根本。', 'A hero\'s amputation — {kingdom} bleeds but keeps its root.', 'Ампутация героя — {kingdom} истекает, но корень цел.', '壮士の腕断ち、{kingdom}は傷つくが根を保つ。', 'Die Amputation des Helden — {kingdom} blutet, bewahrt aber die Wurzel.'),
 q('听天由命', '聽天由命', 'Let fate decide', 'Пусть решит судьба', '天命に任せる', 'Das Schicksal entscheiden lassen'),
 q('不加干预，静观其变。', '不加干預，靜觀其變。', 'Hold back; watch what comes.', 'Сдержаться; смотреть, что будет.', '干渉せず成り行きを見守る。', 'Zurückhalten; beobachten, was kommt.'),
 q('十年生聚，{kingdom}的伤口才慢慢结痂。', '十年生聚，{kingdom}的傷口才慢慢結痂。', 'Ten years of gathering; {kingdom}\'s wound finally scabs.', 'Десять лет собирания; рана {kingdom} наконец затягивается.', '十年の蓄えで{kingdom}の傷がようやく癒える。', 'Zehn Jahre des Sammelns; die Wunde von {kingdom} vernarbt endlich.')
)
ev('realm_rebirth', 2,
 ('国祚重光', '國祚重光', 'The Realm Reborn', 'Возрождение державы', '国祚の再興', 'Das Reich neu geboren'),
 ('废墟之上炊烟再起——{kingdom}用二十年走完一个轮回，重新站起。', '廢墟之上炊煙再起——{kingdom}用二十年走完一個輪迴，重新站起。', 'Smoke rises over the ruins — twenty years of the cycle, {kingdom} stands again.', 'Дым встаёт над руинами — двадцать лет цикла, {kingdom} снова стоит.', '廃墟の上に炊煙が——二十年の輪廻を経て{kingdom}が再び立つ。', 'Rauch steigt über den Ruinen — zwanzig Jahre des Kreislaufs, {kingdom} steht wieder.'),
 q('再兴百业', '再興百業', 'Rebuild all trades', 'Возродить все ремёсла', '百業を再興', 'Alle Gewerbe neu beleben'),
 q('重整城郭，让百业重新兴旺。', '重整城郭，讓百業重新興旺。', 'Restore the towns; let every trade thrive again.', 'Восстановить города; пусть каждое ремесло цветёт.', '城郭を整え百業を再び栄えさせる。', 'Die Städte wiederherstellen; jedes Gewerbe wieder gedeihen lassen.'),
 q('百业兴旺，{kingdom}重新挤进了列强之列。', '百業興旺，{kingdom}重新擠進了列強之列。', 'Trades flourish; {kingdom} rejoins the powers.', 'Ремёсла цветут; {kingdom} возвращается в державы.', '百業が栄え{kingdom}が再び列強に伍する。', 'Die Gewerbe blühen; {kingdom} kehrt zu den Mächten zurück.'),
 q('安抚人心', '安撫人心', 'Soothe the hearts', 'Утешить сердца', '人心をなだめる', 'Die Herzen besänftigen'),
 q('先安民心，再谈恢弘。', '先安民心，再談恢弘。', 'Calm the hearts first; grand designs later.', 'Сначала успокоить сердца; великое — потом.', 'まず民心を安らげ、後に大計を。', 'Erst die Herzen beruhigen, die großen Pläne später.'),
 q('人心归附，{kingdom}的大业重新有了起点。', '人心歸附，{kingdom}的大業重新有了起點。', 'Hearts return; {kingdom}\'s great work finds a new start.', 'Сердца возвращаются; великий труд {kingdom} обретает новое начало.', '人心が戻り{kingdom}の大業が新たな起点を得る。', 'Die Herzen kehren zurück; das große Werk von {kingdom} findet einen neuen Anfang.')
)

# ===== 速决样例（1 年）=====
ev('court_verdict', 2,
 ('御前廷议', '御前廷議', 'Court Verdict', 'Приговор двора', '御前廷議', 'Hofurteil'),
 ('一桩悬案急待{king}御前拍板——今日之内就要定夺。', '一樁懸案急待{king}御前拍板——今日之內就要定奪。', 'A pending case begs {king}\'s verdict — decide within the year.', 'Незакрытое дело просит вердикта {king} — решить в этом же году.', '懸案が{king}の御前裁断を待つ——年内に決める。', 'Ein offener Fall fleht um {king}\'s Urteil — noch im selben Jahr entscheiden.'),
 q('速断速决', '速斷速決', 'Verdict at once', 'Вердикт сейчас', '即断即決', 'Sofort urteilen'),
 q('当日拍板，绝不拖延。', '當日拍板，絕不拖延。', 'Rule on the spot; no delay.', 'Решить на месте; без задержек.', 'その場で裁断し遅延しない。', 'Auf der Stelle entscheiden; keine Verzögerung.'),
 q('果断定谳，{kingdom}的政令畅通无阻。', '果斷定讞，{kingdom}的政令暢通無阻。', 'A firm ruling; {kingdom}\'s orders flow unblocked.', 'Твёрдый вердикт; указы {kingdom} текут беспрепятственно.', '果断な裁断で{kingdom}の政令が滞りなく通る。', 'Ein festes Urteil; die Befehle von {kingdom} fließen ungehindert.'),
 q('三思而行', '三思而行', 'Think thrice', 'Подумать трижды', '熟慮', 'Dreifach denken'),
 q('押后再议，宁可多留几日。', '押後再議，寧可多留幾日。', 'Defer; better to hold a few more days.', 'Отложить; лучше подержать ещё дней несколько.', '再考のため数日延ばす。', 'Aufschieben; besser noch ein paar Tage halten.'),
 q('廷议搁置，{kingdom}的政务慢了几分。', '廷議擱置，{kingdom}的政務慢了幾分。', 'The case sits; {kingdom}\'s governance lags a bit.', 'Дело лежит; управление {kingdom} слегка отстаёт.', '廷議が棚上げされ{kingdom}の政務が遅れる。', 'Der Fall liegt; die Regierung von {kingdom} hinkt etwas.')
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
