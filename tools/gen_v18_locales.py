# -*- coding: utf-8 -*-
"""v1.8.0: 12 new events x 6 languages + 3 option-summary keys. Idempotent (only fills missing keys)."""
import io, json, collections, os

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
os.chdir(ROOT)

EVENTS = collections.OrderedDict()
def ev(eid, opts, title, desc, *texts):
    EVENTS[eid] = (opts, title, desc, list(texts))
def q(*args): return args

# ===== 世界大战 =====
ev('world_war_pact', 2,
 ('世界大战·宣战书', '世界大戰·宣戰書', 'World War: Declaration', 'Мировая война: объявление', '世界大戦・宣戦布告', 'Weltkrieg: Kriegserklärung'),
 ('一份密约在列国间泄露：{kingdom}的宿敌与强邻暗通款曲，战火已悬于所有君主头顶。', '一份密約在列國間洩露：{kingdom}的宿敵與強鄰暗通款曲，戰火已懸於所有君主頭頂。', 'A secret pact leaks among the kingdoms: {kingdom}\'s rival and strong neighbor are colluding; war hangs over every throne.', 'Тайный пакт утёк меж королевствами: соперник {kingdom} сговаривается с соседом; война нависла над всеми тронами.', '密約が列国の間で漏れた：{kingdom}の宿敵と強国の結託——戦火がすべての王座に迫る。', 'Ein geheimer Pakt leakt zwischen den Reichen: der Rivale von {kingdom} und ein starker Nachbar konspirieren; Krieg schwebt über jedem Thron.'),
 q('点燃火药桶', '點燃火藥桶', 'Set the spark', 'Искра', '火種を点火', 'Den Funken setzen'),
 q('公开密约，让战火点燃整个大陆。', '公開密約，讓戰火點燃整個大陸。', 'Publish the pact; let the war ignite the whole continent.', 'Опубликовать пакт; дать войне зажечь континент.', '密約を公表し大陸全部を戦火に。', 'Den Pakt veröffentlichen; der Krieg entfacht den ganzen Kontinent.'),
 q('战火燎原，{kingdom}的国名与烽烟一同刻进史书。', '戰火燎原，{kingdom}的國名與烽煙一同刻進史書。', 'The continent burns; {kingdom} is carved into history alongside the smoke.', 'Континент горит; {kingdom} вписан в историю вместе с дымом.', '大陸が燃え{kingdom}の国名が煙とともに史書に刻まれる。', 'Der Kontinent brennt; {kingdom} wird mit dem Rauch in die Geschichte gemeißelt.'),
 q('冷眼旁观', '冷眼旁觀', 'Hold back', 'Сдержаться', '傍観する', 'Zurückhalten'),
 q('压下密约，先巩固自身的军备与盟好。', '壓下密約，先鞏固自身的軍備與盟好。', 'Bury the pact; shore up arms and friendships first.', 'Похоронить пакт; сперва укрепить оружие и дружбу.', '密約を握りつぶし先に軍備と盟好を固める。', 'Den Pakt begraben; erst Waffen und Freundschaft festigen.'),
 q('{kingdom}的选择让列国侧目，战火暂时熄灭，暗流却更汹涌。', '{kingdom}的選擇讓列國側目，戰火暫時熄滅，暗流卻更洶湧。', 'The choice draws stares; the war smolders, the undercurrent surges.', 'Выбор привлекает взгляды; война тлеет, подводное течение бурлит.', '選択に列国が注目し戦火は一時鎮まり暗流が激しくなる。', 'Die Wahl zieht Blicke an; der Krieg glimmt, die Unterströmung wogt.')
)
ev('world_war_driven', 2,
 ('世界大战·资源争夺', '世界大戰·資源爭奪', 'World War: Resource Drive', 'Мировая война: борьба за ресурсы', '世界大戦・資源争奪', 'Weltkrieg: Ressourcenkampf'),
 ('各大陆的矿脉与粮仓被各方觊觎，{kingdom}的斥候回报：所有强国都在扩军备战。', '各大陸的礦脈與糧倉被各方覬覦，{kingdom}的斥候回報：所有強國都在擴軍備戰。', 'Ore veins and granaries are coveted everywhere; {kingdom}\'s scouts report every power is arming.', 'Рудники и житницы вожделеемы всеми; разведчики {kingdom} докладывают: каждая держава вооружается.', 'すべての大陸の鉱脈と穀倉が狙われ、{kingdom}の斥候は列強の軍拡を報ずる。', 'Erzadern und Kornspeicher sind begehrt; die Späher von {kingdom} berichten, jede Macht rüstet auf.'),
 q('先发制人', '先發制人', 'Strike first', 'Бить первыми', '先手を打つ', 'Zuerst zuschlagen'),
 q('在众人尚未结盟前打响第一枪。', '在眾人尚未結盟前打響第一槍。', 'Fire the first shot before the powers ally.', 'Сделать первый выстрел, пока державы не объединились.', '列強が同盟する前に第一撃を放つ。', 'Den ersten Schuss abfeuern, ehe sich die Mächte verbünden.'),
 q('大陆秩序崩解，{kingdom}被卷入了史无前例的混战。', '大陸秩序崩解，{kingdom}被捲入了史無前例的混戰。', 'Continental order collapses; {kingdom} is drawn into an unprecedented melee.', 'Континентальный порядок рушится; {kingdom} втянут в беспрецедентную свару.', '大陸の秩序が崩れ{kingdom}が前代未聞の乱戦に巻き込まれる。', 'Die Ordnung des Kontinents bricht; {kingdom} wird in ein beispielloses Gemenge gezogen.'),
 q('伺机而动', '伺機而動', 'Wait and see', 'Выждать', '機をうかがう', 'Abwarten'),
 q('按兵不动，让第一枪由别人打响。', '按兵不動，讓第一槍由別人打響。', 'Hold; let someone else fire the first shot.', 'Сдержаться; пусть кто-то другой выстрелит первым.', '動かず他の誰かに第一撃を打たせる。', 'Abwarten; ein anderer soll den ersten Schuss abfeuern.'),
 q('旁观者式的冷静没有换来和平，{kingdom}后来仍被拖入战局。', '旁觀者式的冷靜沒有換來和平，{kingdom}後來仍被拖入戰局。', 'Observer-calm buys no peace; {kingdom} later gets dragged into the war anyway.', 'Спокойствие зрителя не купило мира; {kingdom} позже был втянут в войну.', '傍観の冷静は平和を買えず{kingdom}は後々戦局に巻き込まれる。', 'Beobachter-Ruhe erkauft keinen Frieden; {kingdom} wird später doch in den Krieg gezogen.')
)

# ===== 宣战 =====
ev('declare_war_casus', 2,
 ('宣战·边境借口', '宣戰·邊境藉口', 'Casus Belli', 'Casus belli', '宣戦・国境の口実', 'Casus Belli'),
 ('边境哨所与邻邦士兵发生冲突，{king}的将军们群情激愤：尊贵的王，该宣战还是克制？', '邊境哨所與鄰邦士兵發生衝突，{king}的將軍們群情激憤：尊貴的王，該宣戰還是克制？', 'Clashes at the border posts; {king}\'s generals are inflamed: declare war or hold back?', 'Столкновения на заставах; генералы {king} кипят: объявлять войну или сдержаться?', '国境の哨所で敵兵と衝突——{king}の将軍たちが激昂し宣戦か自制かを問う。', 'Zwischenfälle an den Grenzposten; die Generäle von {king} kochen: Krieg erklären oder halten?'),
 q('立即宣战', '立即宣戰', 'Declare war', 'Объявить войну', '宣戦する', 'Krieg erklären'),
 q('抓住借口，向最强的邻国宣战。', '抓住藉口，向最強的鄰國宣戰。', 'Seize the pretext; declare war on the strongest neighbor.', 'Воспользоваться предлогом; объявить войну сильнейшему соседу.', '口実を掴み最強の隣国に宣戦する。', 'Den Vorwand nutzen; dem stärksten Nachbarn den Krieg erklären.'),
 q('战鼓擂响，{kingdom}的军队向邻国边境开进。', '戰鼓擂響，{kingdom}的軍隊向鄰國邊境開進。', 'War drums roll; {kingdom}\'s army marches to the neighbor\'s border.', 'Барабаны бьют; армия {kingdom} идёт к границе соседа.', '戦鼓が鳴り{kingdom}の軍が隣国国境へ進軍する。', 'Die Kriegstrommeln rollen; die Armee von {kingdom} zieht zur Grenze des Nachbarn.'),
 q('明智克制', '明智克制', 'Show restraint', 'Проявить сдержанность', '自制を示す', 'Zurückhaltung zeigen'),
 q('赔礼安抚，换取列国的谅解。', '賠禮安撫，換取列國的諒解。', 'Apologize and soothe; win the powers\' understanding.', 'Извиниться и успокоить; завоевать понимание держав.', '謝罪し宥めて列国の理解を得る。', 'Entschuldigen und besänftigen; das Verständnis der Mächte gewinnen.'),
 q('克制换来了好名声，边境却依然紧张。', '克制換來了好名聲，邊境卻依然緊張。', 'Restraint wins a good name; the border stays tense.', 'Сдержанность дарит доброе имя; граница остаётся напряжённой.', '自制が名声を買うが国境は依然緊張する。', 'Zurückhaltung bringt guten Ruf; die Grenze bleibt angespannt.')
)
ev('war_monger', 2,
 ('主战派当道', '主戰派當道', 'The Warhawks', 'Ястребы', '主戦派の台頭', 'Die Kriegsfalken'),
 ('{king}的廷臣里主战派愈发强势——同宿敌的旧怨被反复提起，怂恿立刻开战。', '{king}的廷臣裡主戰派愈發強勢——同宿敵的舊怨被反覆提起，慫恿立刻開戰。', 'The hawks grow louder in {king}\'s court — old grudges are rehashed, urging instant war.', 'Ястребы при дворе {king} всё громче — старые обиды пересказывают, подталкивая к войне.', '{king}の廷臣で主戦派が勢いを増し宿敵への旧怨を繰り返し即時の開戦を促す。', 'Die Falken am Hof von {king} werden lauter — alte Feindschaften werden aufgewärmt und drängen zum Krieg.'),
 q('允许开战', '允許開戰', 'Allow war', 'Разрешить войну', '開戦を許す', 'Krieg erlauben'),
 q('给主战派一个机会：向当前的交战国或最强敌国开战。', '給主戰派一個機會：向當前的交戰國或最強敵國開戰。', 'Give the hawks a chance: declare war on the current rival (or strongest enemy).', 'Дать ястребам шанс: объявить войну текущему сопернику (или сильнейшему врагу).', '主戦派に機会を与え現在の交戦国（なければ最強敵国）へ宣戦する。', 'Den Falken eine Chance: dem aktuellen Rivalen (oder stärksten Feind) den Krieg erklären.'),
 q('战火被点燃，{kingdom}卷入与强敌的战争。', '戰火被點燃，{kingdom}捲入與強敵的戰爭。', 'War ignites; {kingdom} is drawn into a war with a strong foe.', 'Война вспыхнула; {kingdom} втянут в войну с сильным врагом.', '戦火が灯り{kingdom}は強敵との戦争に巻き込まれる。', 'Der Krieg entbrennt; {kingdom} wird in einen Krieg mit einem starken Feind gezogen.'),
 q('压下主战派', '壓下主戰派', 'Suppress the hawks', 'Подавить ястребов', '主戦派を抑える', 'Die Falken bremsen'),
 q('斥退主战派将领，重申和平为先。', '斥退主戰派將領，重申和平為先。', 'Dismiss the hawk generals; reaffirm peace first.', 'Отстранить генералов-ястребов; мир прежде всего.', '主戦派の将軍を斥け平和第一を再確認する。', 'Die Falken-Generäle entlassen; Frieden zuerst.'),
 q('主战派被压制，列国松了一口气，{kingdom}的形象温和了许多。', '主戰派被壓制，列國鬆了一口氣，{kingdom}的形象溫和了許多。', 'The hawks are curbed; powers exhale, and {kingdom}\'s image softens.', 'Ястребы укрощены; державы выдыхают, образ {kingdom} смягчается.', '主戦派が抑えられ列国の息が付き{kingdom}の印象が穏やかになる。', 'Die Falken sind gezügelt; die Mächte atmen auf, das Bild von {kingdom} wird weicher.')
)
ev('forced_peace_breaker', 2,
 ('撕毁密约', '撕毀密約', 'Rending the Pact', 'Разрыв пакта', '密約の破棄', 'Den Pakt zerreißen'),
 ('邻国背弃和约的陈年旧事浮出水面，{king}的朝臣群情激愤——要么宣战雪耻，要么咽下这口气。', '鄰國背棄和約的陳年舊事浮出水面，{king}的朝臣群情激憤——要麼宣戰雪恥，要麼嚥下這口氣。', 'An old breach of treaty resurfaces; {king}\'s court is inflamed — declare war to avenge, or swallow it.', 'Старое нарушение договора всплывает; двор {king} негодует — объявить войну ради мести или проглотить.', '条約違反の旧事が再燃し{king}の廷臣が激昂——宣戦で雪辱するか、呑み込むか。', 'Ein alter Vertragsbruch taucht auf; der Hof von {king} ist entbrannt — Krieg zur Rache oder schlucken.'),
 q('宣战雪耻', '宣戰雪恥', 'War to avenge', 'Война ради мести', '宣戦で雪辱', 'Krieg zur Rache'),
 q('向最强邻国宣战，让和约的账本流血偿还。', '向最強鄰國宣戰，讓和約的帳本流血償還。', 'Declare war on the strongest neighbor; make the ledger bleed.', 'Объявить войну сильнейшему соседу; пусть счёт истекает кровью.', '最強の隣国へ宣戦し和約の帳簿を血で償わせる。', 'Dem stärksten Nachbarn den Krieg erklären; das Konto bluten lassen.'),
 q('宣战布告送到邻国，战争的齿轮重新咬合。', '宣戰布告送到鄰國，戰爭的齒輪重新咬合。', 'The declaration arrives; the gears of war mesh again.', 'Декларация доставлена; шестерни войны снова сцепляются.', '宣戦布告が届き戦争の歯車が再び噛み合う。', 'Die Kriegserklärung kommt; die Zahnräder des Krieges greifen wieder.'),
 q('咽下这口气', '嚥下這口氣', 'Swallow it', 'Проглотить', '呑み込む', 'Schlucken'),
 q('隐忍不发，让列国看到克制。', '隱忍不發，讓列國看到克制。', 'Endure in silence; let the powers see restraint.', 'Молчать; пусть державы увидят сдержанность.', '沈黙を守り列国に克制を示す。', 'Schweigend ertragen; die Mächte sollen Zurückhaltung sehen.'),
 q('咽下屈辱换来了信任与时间，但邻国的傲慢更盛。', '嚥下屈辱換來了信任與時間，但鄰國的傲慢更盛。', 'Swallowing the insult buys trust and time; the neighbor\'s arrogance grows.', 'Проглоченное оскорбление покупает доверие и время; надменность соседа растёт.', '屈辱を呑んで信頼と時間を買うが隣国の傲慢は増す。', 'Die Beleidigung zu schlucken kauft Vertrauen und Zeit; die Arroganz des Nachbarn wächst.')
)

# ===== 结盟 =====
ev('alliance_overture', 2,
 ('盟好之邀', '盟好之邀', 'An Alliance Proposal', 'Предложение союза', '盟約の誘い', 'Ein Bündnisangebot'),
 ('邻国的使者带着结盟的橄榄枝来到{kingdom}——以友谊换安全，还是婉拒以保留自由？', '鄰國的使者帶著結盟的橄欖枝來到{kingdom}——以友誼換安全，還是婉拒以保留自由？', 'A neighbor\'s envoy brings an olive branch to {kingdom} — trade friendship for safety, or decline to keep freedom?', 'Посол соседа приносит оливковую ветвь в {kingdom} — обменять дружбу на безопасность или отказаться ради свободы?', '隣国の使者が{kingdom}に友好の種を持って来る——安全と引き換えに同盟するか、自由のため断るか。', 'Ein Bote des Nachbarn bringt {kingdom} den Ölzweig — Freundschaft gegen Sicherheit tauschen oder ablehnen?'),
 q('接受结盟', '接受結盟', 'Accept alliance', 'Принять союз', '同盟を受ける', 'Bündnis annehmen'),
 q('与关系最好的邻国缔结盟约。', '與關係最好的鄰國締結盟約。', 'Form an alliance with the most amicable neighbor.', 'Заключить союз с самым дружелюбным соседом.', '最も親しい隣国と盟約を結ぶ。', 'Mit dem freundlichsten Nachbarn ein Bündnis schließen.'),
 q('盟约缔结，{kingdom}的国境线多了一份分量。', '盟約締結，{kingdom}的國境線多了一份分量。', 'The pact is sealed; the borders of {kingdom} gain weight.', 'Пакт скреплён; границы {kingdom} обретают вес.', '盟約が結ばれ{kingdom}の国境に重みが加わる。', 'Der Pakt ist besiegelt; die Grenzen von {kingdom} gewinnen Gewicht.'),
 q('婉拒提议', '婉拒提議', 'Decline', 'Отказаться', '断る', 'Ablehnen'),
 q('礼送使者，保持独立的外交姿态。', '禮送使者，保持獨立的外交姿態。', 'See the envoy off; keep an independent posture.', 'Проводить посла; сохранить независимую позицию.', '使者を送り独立の外交姿勢を保つ。', 'Den Boten begleiten; eine unabhängige Haltung wahren.'),
 q('婉拒让{kingdom}暂时保持了独立，也少了一个潜在盟友。', '婉拒讓{kingdom}暫時保持了獨立，也少了一個潛在盟友。', 'Declining keeps {kingdom} independent for now — and one ally fewer.', 'Отказ оставляет {kingdom} независимым — и на одного союзника меньше.', '断ることで{kingdom}の独立は保たれるが潜在的な盟友も一つ失う。', 'Die Ablehnung hält {kingdom} unabhängig — und einen Verbündeten ärmer.')
)
ev('treaty_of_salt', 2,
 ('盐业盟约', '鹽業盟約', 'The Salt Treaty', 'Соляной договор', '塩の盟約', 'Der Salzvertrag'),
 ('盐路之争悬而未决，邻邦提议：共享盐利的盟约，或维持现状的冷淡。', '鹽路之爭懸而未決，鄰邦提議：共享鹽利的盟約，或維持現狀的冷淡。', 'The salt-route dispute lingers; a neighbor proposes a shared-profit treaty, or cold status quo.', 'Соляной спор тянется; сосед предлагает договор о совместной прибыли или холодный статус-кво.', '塩路の争いが続く中、隣邦が利益共有の盟約か現状の冷たさかを提案する。', 'Der Salzstreit dauert an; ein Nachbar schlägt einen Gewinnvertrag vor oder kalten Status quo.'),
 q('缔结盐盟', '締結鹽盟', 'Sign the salt pact', 'Подписать соляной пакт', '塩盟を結ぶ', 'Den Salzvertrag schließen'),
 q('与国力最强的邻邦结盟并共享盐利。', '與國力最強的鄰邦結盟並共享鹽利。', 'Ally with the strongest neighbor and share the salt profit.', 'Вступить в союз с сильнейшим соседом и делить соляную прибыль.', '最強の隣邦と同盟し塩の利益を分け合う。', 'Mit dem stärksten Nachbarn verbünden und den Salzgewinn teilen.'),
 q('盐盟生效，商路畅通，金库与盟友同增。', '鹽盟生效，商路暢通，金庫與盟友同增。', 'The salt pact holds; trade flows, treasury and allies alike grow.', 'Соляной пакт действует; торговля идёт, казна и союзники растут вместе.', '塩盟が効き商路が開け金庫と盟友が共に増える。', 'Der Salzvertrag hält; der Handel fließt, Schatzkammer und Verbündete wachsen gemeinsam.'),
 q('维持冷淡', '維持冷淡', 'Stay cold', 'Остаться холодным', '冷淡を保つ', 'Kalt bleiben'),
 q('不接受盐利的捆绑，保持距离。', '不接受鹽利的捆綁，保持距離。', 'Refuse the profit-tie; keep distance.', 'Отказаться от связей; держать дистанцию.', '塩の利益の縛りを断り距離を保つ。', 'Die Gewinnbindung ablehnen; Distanz wahren.'),
 q('盐路依旧时断时续，但{kingdom}的独立性无可指摘。', '鹽路依舊時斷時續，但{kingdom}的獨立性無可指摘。', 'The salt road stays intermittent; {kingdom}\'s independence is impeccable.', 'Соляной путь всё ещё прерывист; независимость {kingdom} безупречна.', '塩路は相変わらず途切れがちだが{kingdom}の独立性は瑕疵ない。', 'Die Salzstraße bleibt unterbrochen; die Unabhängigkeit von {kingdom} ist makellos.')
)
ev('enemy_of_enemy', 2,
 ('远交近攻', '遠交近攻', 'Friend of My Enemy', 'Враг моего врага', '遠交近攻', 'Der Feind meines Feindes'),
 ('密探带回情报：有一个王国同样与{kingdom}的宿敌为敌——要不要与之结盟，甚至联手夹击？', '密探帶回情報：有一個王國同樣與{kingdom}的宿敵為敵——要不要與之結盟，甚至聯手夾擊？', 'Agents report a kingdom that shares {kingdom}\'s rivalry — ally with it, even strike jointly?', 'Лазутчики сообщают: есть королевство, враждебное сопернику {kingdom} — союзиться или даже ударить совместно?', '密偵が報告：{kingdom}の宿敵に敵対する王国がある——結盟し、果ては挟撃すべきか。', 'Späher melden ein Reich, das den Rivalen von {kingdom} ablehnt — verbünden oder sogar gemeinsam zuschlagen?'),
 q('联手夹击', '聯手夾擊', 'Strike jointly', 'Ударить совместно', '挟撃する', 'Gemeinsam zuschlagen'),
 q('与关系最好的王国结盟，并同时向最强邻国宣战。', '與關係最好的王國結盟，並同時向最強鄰國宣戰。', 'Ally with the friendliest kingdom and declare war on the strongest neighbor.', 'Союзиться с самым дружелюбным и объявить войну сильнейшему соседу.', '最も親しい王国と結盟し同時に最強の隣国へ宣戦する。', 'Mit dem freundlichsten Reich verbünden und dem stärksten Nachbarn den Krieg erklären.'),
 q('联盟与战书同时发出，{kingdom}的棋局又深了一重。', '聯盟與戰書同時發出，{kingdom}的棋局又深了一重。', 'Alliance and declaration go out together; {kingdom}\'s game deepens.', 'Союз и декларация выходят вместе; игра {kingdom} углубляется.', '同盟と宣戦布告が同時に発せられ{kingdom}の棋局が深まる。', 'Bündnis und Kriegserklärung gehen zusammen hinaus; das Spiel von {kingdom} vertieft sich.'),
 q('只结盟不开战', '只結盟不開戰', 'Ally only', 'Только союз', '同盟のみ', 'Nur Bündnis'),
 q('与关系最好的王国结盟，暂不树敌。', '與關係最好的王國結盟，暫不樹敵。', 'Ally with the friendliest kingdom; keep the sword sheathed.', 'Союзиться с самым дружелюбным; меч пока в ножнах.', '最も親しい王国と同盟し当面は敵を増やさない。', 'Mit dem freundlichsten Reich verbünden; das Schwert in der Scheide lassen.'),
 q('盟友在手，{kingdom}的外交空间开阔了不少。', '盟友在手，{kingdom}的外交空間開闊了不少。', 'With an ally in hand, {kingdom}\'s diplomatic room opens up.', 'Союзник в руках; дипломатическое пространство {kingdom} расширилось.', '盟友を得て{kingdom}の外交の余地が開けた。', 'Mit einem Verbündeten in der Hand öffnet sich der diplomatische Raum von {kingdom}.')
)

# ===== 迁都 =====
ev('capital_relocate', 2,
 ('迁都之议', '遷都之議', 'Move the Capital', 'Перенос столицы', '遷都の議', 'Die Hauptstadt verlegen'),
 ('旧都日渐破败，而另一座雄城商旅云集——{king}面临迁都的抉择：新都，还是守旧？', '舊都日漸破敗，而另一座雄城商旅雲集——{king}面臨遷都的抉擇：新都，還是守舊？', 'The old capital decays while another city thrives — {king} must choose: new capital or keep the old?', 'Старая столица дряхлеет, а другой город процветает — {king} выбирает: новая столица или старая?', '旧都が荒れ、別の雄城に商人が集う——{king}は遷都か維持かを問われる。', 'Die alte Hauptstadt verfällt, während eine andere Stadt gedeiht — {king} muss wählen: neue Hauptstadt oder alte behalten?'),
 q('迁往新都', '遷往新都', 'Relocate', 'Перенести', '遷都する', 'Verlegen'),
 q('迁都到第二大城市，旧都降格为普通城邑。', '遷都到第二大城市，舊都降格為普通城邑。', 'Move the capital to the second city; the old one becomes ordinary.', 'Перенести столицу во второй город; старая становится обычной.', '二番目の都市へ遷都し旧都を普通の城邑に降格する。', 'Die Hauptstadt in die zweite Stadt verlegen; die alte wird gewöhnlich.'),
 q('新都立起王旗，{kingdom}的旧都卸下了权重的冠冕。', '新都立起王旗，{kingdom}的舊都卸下了權重的冠冕。', 'The new capital raises the royal flag; the old one sheds its heavy crown.', 'Новая столица поднимает флаг; старая слагает тяжёлую корону.', '新都に王旗が立ち{kingdom}の旧都は重い冠を脱ぐ。', 'Die neue Hauptstadt hisst die Königsflagge; die alte legt ihre schwere Krone ab.'),
 q('维持旧都', '維持舊都', 'Keep the old', 'Оставить старую', '旧都を守る', 'Alte behalten'),
 q('不动迁都之议，把银子投进旧都修复。', '不動遷都之議，把銀子投進舊都修復。', 'Abandon the move; pour silver into the old capital\'s repair.', 'Оставить переезд; вложить серебро в ремонт старой столицы.', '遷都をやめ銀を旧都の修復に注ぐ。', 'Den Umzug verwerfen; Silber in die Reparatur der alten Hauptstadt stecken.'),
 q('旧都得以加固，但{kingdom}的商旅仍在抱怨位置不便。', '舊都得以加固，但{kingdom}的商旅仍在抱怨位置不便。', 'The old capital is fortified, but traders still grumble about the location.', 'Старая столица укреплена, но торговцы всё ещё ворчат о расположении.', '旧都は固まるが商人は立地の不便を嘆く。', 'Die alte Hauptstadt wird verstärkt, doch Händler beklagen weiter die Lage.')
)
ev('capital_undaunted', 2,
 ('都城告急', '都城告急', 'Capital Under Siege', 'Столица в опасности', '首都危急', 'Hauptstadt in Gefahr'),
 ('敌军兵锋直指{kingdom}的首都——弃都自保，还是固守摇篮？', '敵軍兵鋒直指{kingdom}的首都——棄都自保，還是固守搖籃？', 'Enemy blades point at {kingdom}\'s capital — flee the city, or hold the cradle?', 'Вражеские клинки нацелены на столицу {kingdom} — бежать из города или держать колыбель?', '敵軍が{kingdom}の首都を狙う——棄都して自衛するか、ゆりかごを守るか。', 'Feindliche Klingen zielen auf die Hauptstadt von {kingdom} — die Stadt aufgeben oder die Wiege halten?'),
 q('弃都自保', '棄都自保', 'Flee the capital', 'Бежать из столицы', '首都を捨てる', 'Aus der Hauptstadt fliehen'),
 q('撤离首都，迁都到第二大城市保存王气。', '撤離首都，遷都到第二大城市保存王氣。', 'Evacuate; relocate the capital to the second city to preserve the king\'s aura.', 'Эвакуировать; перенести столицу во второй город ради ауры короля.', '首都を離れ二番目の都市へ遷都し王気を保つ。', 'Evakuieren; die Hauptstadt in die zweite Stadt verlegen, um die Aura des Königs zu bewahren.'),
 q('王旗随迁，首都的伤痕留给了旧城墙。', '王旗隨遷，首都的傷痕留給了舊城牆。', 'The flag moves; the wounds remain on the old walls.', 'Флаг перенесён; раны остаются на старых стенах.', '王旗は移り傷は旧い城壁に残る。', 'Die Flagge zieht um; die Wunden bleiben an den alten Mauern.'),
 q('固守摇篮', '固守搖籃', 'Hold the cradle', 'Держать колыбель', 'ゆりかごを守る', 'Die Wiege halten'),
 q('下令死守首都，军民同心。', '下令死守首都，軍民同心。', 'Order a defense to the death; soldiers and people united.', 'Приказать обороняться до конца; воины и народ едины.', '首都を死守するよう命じ军民が心を合わせる。', 'Verteidigung auf Leben und Tod befehlen; Soldaten und Volk vereint.'),
 q('血战多日，都城保住了，但{kingdom}的国库与人口都付出了代价。', '血戰多日，都城保住了，但{kingdom}的國庫與人口都付出了代價。', 'Fierce days pass; the capital holds, but treasury and population pay the price.', 'Яростные дни проходят; столица держится, но казна и население платят цену.', '血戦の末に首都は保たれるが金庫と人口が代償を払う。', 'Harte Tage vergehen; die Hauptstadt hält, aber Schatzkammer und Bevölkerung zahlen den Preis.')
)

# ===== 建筑升级 =====
ev('building_renaissance', 2,
 ('营造匠作', '營造匠作', 'A Building Renaissance', 'Ренессанс строительства', '营造の隆盛', 'Eine Baurenaissance'),
 ('城中贡匠自请大举营造：修缮与升级各处建筑，让{kingdom}的市面焕然一新。', '城中貢匠自請大舉營造：修繕與升級各處建築，讓{kingdom}的市面煥然一新。', 'Master builders petition for a grand renovation: repair and upgrade buildings across {kingdom}.', 'Мастера-строители просят о большой реконструкции: ремонт и модернизация зданий в {kingdom}.', '街の棟梁が大規模な营造を請願——{kingdom}の町並みを一新する。', 'Meisterbauleute bitten um eine große Erneuerung: Gebäude in {kingdom} reparieren und aufwerten.'),
 q('批准营造', '批准營造', 'Approve the works', 'Одобрить работы', '营造を許す', 'Die Arbeiten bewilligen'),
 q('拨款升级城市建筑（优先民居与工坊）。', '撥款升級城市建築（優先民居與工坊）。', 'Fund building upgrades (houses and workshops first).', 'Профинансировать модернизацию (дома и мастерские в первую очередь).', '都市の建築を資金支援し優先して住宅と工房を整える。', 'Gebäudeaufwertungen finanzieren (zuerst Häuser und Werkstätten).'),
 q('匠人昼夜赶工，{kingdom}的街巷换了模样。', '匠人晝夜趕工，{kingdom}的街巷換了模樣。', 'Builders work day and night; {kingdom}\'s streets look new.', 'Строители трудятся день и ночь; улицы {kingdom} преобразились.', '棟梁が昼夜を徹し{kingdom}の街並みが生まれ変わる。', 'Die Bauleute arbeiten Tag und Nacht; die Straßen von {kingdom} sehen neu aus.'),
 q('作罢', '作罷', 'Decline', 'Отказаться', '見送る', 'Ablehnen'),
 q('婉拒营造，把钱留在金库。', '婉拒營造，把錢留在金庫。', 'Decline the works; keep the coin in the treasury.', 'Отказаться; оставить монеты в казне.', '营造を見送り金を金庫に残す。', 'Die Arbeiten ablehnen; das Geld in der Schatzkammer lassen.'),
 q('旧建筑仍在服役，但{kingdom}的市面开始显得陈旧。', '舊建築仍在服役，但{kingdom}的市面開始顯得陳舊。', 'Old buildings still serve, yet {kingdom}\'s market shines less.', 'Старые здания ещё служат, но рынок {kingdom} блестит меньше.', '旧建築は役立つが{kingdom}の街は古びて見え始める。', 'Alte Gebäude dienen noch, doch der Markt von {kingdom} glänzt weniger.')
)

# ===== 个体财富 =====
ev('merchant_tycoon', 2,
 ('巨贾沉浮', '巨賈沉浮', 'The Merchant Tycoon', 'Тайкун-купец', '豪商の浮沈', 'Der Kaufmann-Tycoon'),
 ('一位巨贾的发家史或豪赌的惨败震动了{kingdom}的市集——有人一夜暴富，也有人倾家荡产。', '一位巨賈的發家史或豪賭的慘敗震動了{kingdom}的市集——有人一夜暴富，也有人傾家蕩產。', 'A tycoon\'s rise or ruinous gamble shakes {kingdom}\'s market — some struck it rich, some lost it all.', 'Взлёт или разрушительная авантюра купца потрясает рынок {kingdom} — кто-то разбогател, кто-то всё потерял.', '豪商の成功談か破局が{kingdom}の市を揺るがす——一夜の富豪と傾家者。', 'Der Aufstieg oder ruinöse Wette eines Tycoons erschüttert den Markt von {kingdom} — manche wurden reich, andere verloren alles.'),
 q('豪赌起航', '豪賭起航', 'Gamble high', 'Играть по-крупному', '大勝負に出る', 'Hoch pokern'),
 q('让资本流向最敢赌的生意人——他们将被新财富点燃。', '讓資本流向最敢賭的生意人——他們將被新財富點燃。', 'Let capital flow to the boldest traders — they ignite with new wealth.', 'Пустить капитал к самым смелым торговцам — они вспыхнут новым богатством.', '最も大胆な商人へ資本を流し新たな富を灯す。', 'Kapital zu den kühnsten Händlern fließen lassen — sie entbrennen an neuem Reichtum.'),
 q('资本的狂热让皇商与巨贾身价倍增，也烙下了风险的印记。', '資本的狂熱讓皇商與巨賈身價倍增，也烙下了風險的印記。', 'Capital fever doubles the tycoons\' worth and brands the risk.', 'Лихорадка капитала удваивает состояние тайкунов и клеймит риск.', '資本の熱狂が豪商の身代を倍加させリスクの刻印を残す。', 'Das Kapitalfieber verdoppelt den Wert der Tycoons und brandmarkt das Risiko.'),
 q('清算风浪', '清算風浪', 'The crash', 'Крах', '清算の波', 'Der Krach'),
 q('挤出泡沫，让投机者承担代价。', '擠出泡沫，讓投機者承擔代價。', 'Squeeze the bubble; let speculators pay.', 'Выдавить пузырь; спекулянты платят.', '泡を潰し投機家に代償を負わせる。', 'Die Blase drücken; Spekulanten zahlen.'),
 q('一批商人破产离场，{kingdom}的市集从此多了几分谨慎。', '一批商人破產離場，{kingdom}的市集從此多了幾分謹慎。', 'A wave of merchants goes bankrupt; {kingdom}\'s market gains caution.', 'Волна купцов банкротится; рынок {kingdom} становится осторожнее.', '商人が倒産し{kingdom}の市に警戒が増える。', 'Eine Welle von Händlern geht bankrott; der Markt von {kingdom} wird vorsichtiger.')
)

# ===== 摘要键（event_choice_* 已有键外新增；六语元组：ch/zh_tw/en/ru/ja/de）=====
SUMMARY = {
    'event_choice_commerce': ('商路断绝 {0} 年', '商路斷絕 {0} 年', 'Commerce cut {0}y', 'Торговый запрет {0} г.', '商路断絶 {0} 年', 'Handel unterbrochen {0}J'),
    'event_choice_declare_war': ('宣战', '宣戰', 'Declares war', 'Объявляет войну', '宣戦', 'Kriegserklärung'),
    'event_choice_alliance': ('结盟', '結盟', 'Forms alliance', 'Заключает союз', '同盟', 'Bündnis'),
    'event_choice_move_capital': ('迁都', '遷都', 'Moves capital', 'Переносит столицу', '遷都', 'Hauptstadtverlegung'),
    'event_choice_upgrade': ('升级建筑 ×{0}', '升級建築 ×{0}', 'Upgrades ×{0}', 'Улучшает ×{0}', '建築強化 ×{0}', 'Verbessert ×{0}'),
    'event_choice_citizen_gain': ('民众暴富', '民眾暴富', 'Citizens enriched', 'Граждане богатеют', '民衆が潤う', 'Bürger bereichern'),
    'event_choice_citizen_loss': ('民众破产', '民眾破產', 'Citizens ruined', 'Граждане разорены', '民衆の破産', 'Bürger ruiniert'),
    'event_choice_worldwar': ('世界大战（毁灭级）', '世界大戰（毀滅級）', 'WORLD WAR (cataclysmic)', 'МИРОВАЯ ВОЙНА (катастрофа)', '世界大戦（破滅級）', 'WELTKRIEG (kataklysmisch)'),
}

# ===== 六语合并（幂等：只补缺键）=====
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
    for k, v in SUMMARY.items():
        if k not in d:
            d[k] = v[idx]; added += 1
    json.dump(d, io.open(p, 'w', encoding='utf-8'), ensure_ascii=False, indent=2)
    io.open(p, 'a', encoding='utf-8').write('\n')
    total_added += added
    print(lang, 'added', added, 'total', len(d))
print('TOTAL added keys:', total_added)
