# -*- coding: utf-8 -*-
"""v1.7.0 diplomacy 文案（六语）"""
EVENTS = {}
def ev(eid, opts, title, desc, *texts):
    EVENTS[eid] = (opts, title, desc, list(texts))
def q(*args): return args

# ===== 边境纠纷组 border_rift =====
ev('border_stone', 2,
 ('界碑之争', '界碑之爭', 'The Border Stone', 'Спор о межевом камне', '国境の石碑', 'Der Grenzstein'),
 ('两国新立界碑于山口，为方圆几尺之地，边境烽烟又起，百姓惶惶。', '兩國新立界碑於山口，為方圓幾尺之地，邊境烽煙又起，百姓惶惶。', 'A new boundary stone rises in the pass; the neighbors squabble over a few feet of land and folk grow uneasy.', 'На перевале поставили новый межевой камень; соседи спорят из-за нескольких футов земли, народ в тревоге.', '峠に新しい国境の石碑が立つ——数尺の土地を巡り両国が争い、民は不安を抱く。', 'Ein neuer Grenzstein steht im Pass; die Nachbarn zanken um ein paar Fuß Land, und die Leute werden unruhig.'),
 q('据理不退', '據理不退', 'Stand firm', 'Стоять на своём', '一歩も譲らず', 'Beharren'),
 q('寸土必争，驻军持械立于碑前。', '寸土必爭，駐軍持械立於碑前。', 'Yield nothing; armed men guard the stone.', 'Ни вершка; у камня встают воины.', '寸土を争い兵が碑前に立てこもる。', 'Keinen Fingerbreit; Bewaffnete stehen am Stein.'),
 q('对峙月余，边患未除，{kingdom}之名受损于列国。', '對峙月餘，邊患未除，{kingdom}之名受損於列國。', 'The standoff drags on; {kingdom} loses standing among nations.', 'Противостояние тянется; {kingdom} теряет вес среди стран.', '対峙が続き{kingdom}の名声が各国で傷つく。', 'Das Tauziehen dauert; {kingdom} verliert Ansehen unter den Völkern.'),
 q('释争让地', '釋爭讓地', 'Make way', 'Уступить ради мира', '争いを譲る', 'Nachgeben'),
 q('愿让数尺之地，化干戈为玉帛。', '願讓數尺之地，化干戈為玉帛。', 'Concede a few feet and turn swords into silk.', 'Уступить пару футов, обратив мечи в шёлк.', '数尺を譲り干戈を玉帛に変える。', 'Ein paar Fuß nachgeben und Schwerter in Seide verwandeln.'),
 q('宽厚之名远播，列国对{kingdom}刮目相看。', '寬厚之名遠播，列國對{kingdom}刮目相看。', 'Magnanimity spreads; nations look on {kingdom} with new respect.', 'Великодушие славится; страны смотрят на {kingdom} с уважением.', '寛厚の名が広まり{kingdom}を各国が見直す。', 'Großmut verbreitet sich; die Nationen blicken mit neuem Respekt auf {kingdom}.')
)
ev('fishing_spat', 2,
 ('渔汛之争', '漁汛之爭', 'The Fishing Spat', 'Рыбная ссора', '漁場の諍い', 'Der Fischereistreit'),
 ('界河水美鱼肥，两国渔舟竞逐下网，堤岸聚满看客，冲突一触即发。', '界河水美魚肥，兩國漁舟競逐下網，堤岸聚滿看客，衝突一觸即發。', 'The border river teems with fish; both fleets race to cast nets while crowds watch — a clash is brewing.', 'Пограничная река полна рыбы; обе флотилии спешат закинуть сети, толпа смотрит — вот-вот вспыхнет ссора.', '国境の川は魚肥え、両国の漁船が網を競う——岸は見物人で溢れ、衝突は目前だ。', 'Der Grenzfluss wimmelt von Fischen; beide Flotten wetteifern mit Netzen, das Ufer schaut zu — ein Streit braut sich zusammen.'),
 q('封河护渔', '封河護漁', 'Close the river', 'Запереть реку', '川を封鎖', 'Den Fluss sperren'),
 q('驱逐邻舟，独占渔汛之利。', '驅逐鄰舟，獨占漁汛之利。', 'Drive the neighbor vessels away and keep the whole season.', 'Прогнать соседние лодки и забрать весь сезон.', '隣の舟を追い払い漁を独占する。', 'Die Nachbarboote vertreiben und die ganze Saison behalten.'),
 q('邻舟败归，怨声四起，界河从此多事。', '鄰舟敗歸，怨聲四起，界河從此多事。', 'The neighbor boats retreat in fury; the border river brews trouble.', 'Соседние лодки уходят в ярости; граница кипит.', '隣舟は悔しげに引き上げ国境の川が騒がしくなる。', 'Die Nachbarboote ziehen wütend ab; der Grenzfluss brodelt.'),
 q('约期共渔', '約期共漁', 'Share the season', 'Делить улов', '漁期を分かち合う', 'Die Saison teilen'),
 q('议定休渔之期，两舟错时下网。', '議定休漁之期，兩舟錯時下網。', 'Agree on closed seasons and stagger the casts.', 'Договориться о нересте и чередовать сети.', '禁漁期を定め網を交互に打つ。', 'Schonzeiten vereinbaren und Netze abwechselnd werfen.'),
 q('渔汛共享，界河两岸笑语相闻。', '漁汛共享，界河兩岸笑語相聞。', 'The catch is shared; laughter echoes on both banks.', 'Улов общий; смех слышен на обоих берегах.', '漁を分け合い国境の川に笑い声が響く。', 'Der Fang wird geteilt; Lachen hallt an beiden Ufern.')
)
ev('hunt_line', 2,
 ('猎线之争', '獵線之爭', 'The Hunting Line', 'Охотничья черта', '狩猟線の争い', 'Die Jagdlinie'),
 ('两国猎户竞逐于界山，兽踪未辨，人声先起，林中弓矢已张。', '兩國獵戶競逐於界山，獸蹤未辨，人聲先起，林中弓矢已張。', 'Hunters of both realms chase game across the border ridges; bows are drawn before any spoor is found.', 'Охотники двух держав гонят зверя через хребты; луки натянуты раньше, чем найден след.', '両国の猟師が国境の峰を競う——獣の足跡より先に人声が立ち、林に弓矢が張られる。', 'Jäger beider Reiche jagen über die Grenzgrate; Bögen sind gespannt, ehe eine Spur gefunden ist.'),
 q('越境缉兽', '越境緝獸', 'Cross & chase', 'Гнать через границу', '越えて追う', 'Über die Grenze jagen'),
 q('兵士随猎，直入邻境山林。', '兵士隨獵，直入鄰境山林。', 'Send hunting parties with guards into the neighbor woods.', 'Послать отряды с охраной в соседние леса.', '兵を伴い隣国の山へ踏み込む。', 'Jagdgruppen mit Wachen in den Nachbarwald schicken.'),
 q('山中猎角声乱，邻邦疑忌渐生。', '山中獵角聲亂，鄰邦疑忌漸生。', 'Hunting horns blare; distrust grows across the border.', 'Рога трубят; через границу растёт недоверие.', '狩りの角笛が乱れ国境の疑念が深まる。', 'Hörner schmettern; Misstrauen wächst über die Grenze.'),
 q('划界共猎', '劃界共獵', 'Draw the line', 'Разметить границу', '界限を決めて共に狩る', 'Die Grenze ziehen'),
 q('祭山划界，互不越线。', '祭山劃界，互不越線。', 'Mark the ridges and hunt only on your side.', 'Отметить хребты и охотиться на своей стороне.', '山を祭り境界を定め互いに出ない。', 'Die Grate abstecken und nur auf eigener Seite jagen.'),
 q('界线既明，两邦猎户不复相犯。', '界線既明，兩邦獵戶不復相犯。', 'With lines clear, hunters no longer cross each other.', 'С ясными линиями охотники больше не пересекаются.', '境界が定まり猟師同士が干渉しなくなる。', 'Mit klaren Linien kreuzen Jäger einander nicht mehr.')
)

# ===== 质子连锁 hostage_walk -> hostage_return =====
ev('hostage_walk', 2,
 ('质子行期', '質子行期', 'The Hostage Envoy', 'Срок заложника', '人質の旅立ち', 'Der Geisel-Aufbruch'),
 ('邻国质子客居{kingdom}多年，今使者来迎，{king}须择期送归。', '鄰國質子客居{kingdom}多年，今使者來迎，{king}須擇期送歸。', 'The neighbor heir has lodged in {kingdom} for years; envoys now come to escort him home.', 'Соседний заложник годами жил в {kingdom}; послы прибыли сопроводить его домой.', '隣国の人質は{kingdom}に長年滞在——今、迎えの使者が来て{king}が帰国期を決める。', 'Der Geisel des Nachbarreiches lebte Jahre in {kingdom}; Gesandte kommen, ihn heimzugeleiten.'),
 q('礼送启程', '禮送啟程', 'Escort in honor', 'Почетный эскорт', '礼を尽くして送る', 'Ehrenvoll geleiten'),
 q('仪仗随行，一路款待如宾。', '儀仗隨行，一路款待如賓。', 'Send him off with honors and comforts along the way.', 'Отправить с почестями и удобствами в пути.', '儀仗を添え道中も賓客として遇す。', 'Mit Ehren und Bequemlichkeiten auf die Reise schicken.'),
 q('车驾西行，邻国感其厚意；归途万里，犹待来年。', '車駕西行，鄰國感其厚意；歸途萬里，猶待來年。', 'The carriage rolls west; the neighbor appreciates the grace. The long road home still waits ahead.', 'Повозка едет на запад; сосед ценит милость. Долгий путь домой ещё ждёт впереди.', '車は西へ——隣国は厚意に感謝する。帰国の長い道はまだ先に続く。', 'Der Wagen rollt gen Westen; der Nachbar dankt für die Gnade. Der lange Heimweg wartet noch.'),
 q('留质观变', '留質觀變', 'Detain the heir', 'Удержать наследника', '人質を留め置く', 'Die Geisel festhalten'),
 q('借故暂缓归期，留作筹码。', '藉故暫緩歸期，留作籌碼。', 'Delay the departure; keep the heir as a lever.', 'Отсрочить отъезд; держать наследника рычагом.', '理由を設け帰国を延ばし駒として残す。', 'Die Abreise verzögern; den Erben als Hebel halten.'),
 q('廷议争执，民间物议沸腾，国中隐有不安。', '廷議爭執，民間物議沸騰，國中隱有不安。', 'The court quarrels; gossip boils in the streets; unease stirs at home.', 'Двор спорит; молва кипит на улицах; дома растёт тревога.', '廷議が紛糾し巷で物議が沸く——国内に不安が潜む。', 'Der Hof zankt; Klatsch kocht auf den Straßen; Unruhe gärt im Land.')
)
ev('hostage_return', 2,
 ('质子归国', '質子歸國', 'The Hostage Returns', 'Возвращение заложника', '人質の帰国', 'Die Geisel kehrt heim'),
 ('次年春，邻国再三来书，请{king}践前诺送质子归国。', '次年春，鄰國再三來書，請{king}踐前諾送質子歸國。', 'Next spring the neighbor presses again: {king} must honor the promise and send the heir home.', 'Следующей весной сосед настаивает: {king} должен сдержать слово и отправить наследника домой.', '翌春、隣国が再び書を送る——{king}は約束通り人質を帰すべきか。', 'Im nächsten Frühjahr drängt der Nachbar erneut: {king} muss das Wort halten und den Erben heimschicken.'),
 q('践诺放归', '踐諾放歸', 'Honor the word', 'Сдержать слово', '約束を果たす', 'Das Wort halten'),
 q('如约送返，并馈车马罗衣。', '如約送返，並饋車馬羅衣。', 'Send him home with horses, robes, and goodwill.', 'Отправить домой с лошадьми, одеяниями и доброй волей.', '約束通り馬と衣を添えて帰す。', 'Ihn mit Pferden, Gewändern und Wohlwollen heimschicken.'),
 q('质子归国，邻邦释疑，{kingdom}信义之名更盛。', '質子歸國，鄰邦釋疑，{kingdom}信義之名更盛。', 'The heir returns; trust heals and {kingdom} gains fame for good faith.', 'Наследник вернулся; доверие восстановлено, {kingdom} славится верностью слову.', '人質が帰り隣国の疑いが晴れ{kingdom}の信義の名が高まる。', 'Der Erbe kehrt heim; Vertrauen heilt und {kingdom} erntet Ruhm für Treue zum Wort.'),
 q('迁延索礼', '遷延索禮', 'Haggle & delay', 'Тянуть и торговаться', '延ばして礼を求める', 'Feilschen und zögern'),
 q('以旧约为词，索偿而后放。', '以舊約為詞，索償而後放。', 'Plead old accounts and ask repayment before release.', 'Сослаться на старые счёты и требовать платы.', '旧約を口実に償いを求めてから放つ。', 'Alte Rechnungen vorbringen und Lösegeld fordern.'),
 q('邻国怒火中烧，朝野亦怨{king}失信，怨声渐起。', '鄰國怒火中燒，朝野亦怨{king}失信，怨聲漸起。', 'The neighbor seethes; court and commons blame {king} for breaking faith.', 'Сосед кипит; двор и народ винят {king} в неверности слову.', '隣国は激怒し朝野も{king}の背信を怨む声が上がる。', 'Der Nachbar kocht; Hof und Volk machen {king} Treuebruch zum Vorwurf.')
)

# ===== 使节往来 =====
ev('embassy_swap', 2,
 ('互置使馆', '互置使館', 'Embassy Exchange', 'Обмен посольствами', '使館の互置', 'Botschaften tauschen'),
 ('邻国愿与{kingdom}互设常驻使馆，各遣使臣长驻。', '鄰國願與{kingdom}互設常駐使館，各遣使臣長駐。', 'The neighbor proposes permanent embassies with {kingdom}, envoys stationed in each capital.', 'Сосед предлагает {kingdom} постоянные посольства — посланники в обеих столицах.', '隣国が{kingdom}と常駐使館を交換し双方に使臣を置く提案をする。', 'Der Nachbar schlägt {kingdom} ständige Botschaften vor — Gesandte in beiden Hauptstädten.'),
 q('许之', '許之', 'Accept', 'Принять', '承諾する', 'Annehmen'),
 q('互遣使臣，常年驻节。', '互遣使臣，常年駐節。', 'Exchange envoys who reside year-round.', 'Обменяться посланниками с постоянным пребыванием.', '使臣を常駐で交換する。', 'Gesandte dauerhaft austauschen.'),
 q('两邦使节往还如织，嫌隙渐消。', '兩邦使節往還如織，嫌隙漸消。', 'Envoys shuttle between the courts; old grudges thin out.', 'Посланники снуют между дворами; старые обиды тают.', '使節が行き交い遺恨が薄れる。', 'Gesandte pendeln zwischen den Höfen; alte Feindschaft verblasst.'),
 q('缓行其议', '緩行其議', 'Hold it off', 'Отложить', '見送る', 'Aufschieben'),
 q('以疆务未宁为由，暂不置议。', '以疆務未寧為由，暫不置議。', 'Plead border troubles and put the matter aside.', 'Сослаться на пограничные дела и отложить.', '国境の紛争を理由に持ち越す。', 'Auf Grenzprobleme verweisen und aufschieben.'),
 q('使者怅然而返，两邦依然隔阂未通。', '使者悵然而返，兩邦依然隔閡未通。', 'The envoy leaves disappointed; the two courts stay estranged.', 'Посланник уходит разочарованным; дворы остаются отчуждёнными.', '使者は失望して帰り両国の隔たりは残る。', 'Der Gesandte zieht enttäuscht davon; die Höfe bleiben entfremdet.')
)
ev('state_visit', 2,
 ('国事之访', '國事之訪', 'The State Visit', 'Государственный визит', '国賓の訪問', 'Der Staatsbesuch'),
 ('邻国君主亲来{kingdom}，举国迎驾之仪所费甚巨，朝下议论如沸。', '鄰國君主親來{kingdom}，舉國迎駕之儀所費甚鉅，朝下議論如沸。', 'The neighbor sovereign comes in person to {kingdom}; a fitting welcome would strain the treasury, and the court buzzes.', 'Соседний государь прибывает в {kingdom}; достойный приём ударит по казне, и двор гудит.', '隣国の君主が{kingdom}を訪れる——正式な迎えには国費が嵩み宮中がざわめく。', 'Der Nachbar-Souverän kommt persönlich nach {kingdom}; ein würdiger Empfang belastet die Kasse, der Hof summt.'),
 q('盛礼迎驾', '盛禮迎駕', 'Splendid welcome', 'Пышный приём', '盛大に迎える', 'Prunkvoller Empfang'),
 q('沿途结彩，犒军享宴。', '沿途結綵，犒軍享宴。', 'Banners along the roads, feasts for the army.', 'Знамёна вдоль дорог, пиры для войска.', '道中に飾りを張り軍に宴を振る舞う。', 'Banner entlang der Straßen, Feste für das Heer.'),
 q('耗金巨万，然邻君尽欢，两邦约好更深。', '耗金鉅萬，然鄰君盡歡，兩邦約好更深。', 'Gold pours out, but the guest is delighted and the alliance deepens.', 'Золото утекает, но гость доволен и союз крепнет.', '国費は嵩んだが君主は満足し両国の親交が深まる。', 'Gold fließt, aber der Gast ist entzückt und das Bündnis vertieft sich.'),
 q('俭礼相迎', '儉禮相迎', 'Modest welcome', 'Скромный приём', '質素に迎える', 'Schlichter Empfang'),
 q('一切从简，不失礼数。', '一切從簡，不失禮數。', 'All plain, yet courteous.', 'Всё просто, но учтиво.', 'すべて質素にしながら礼は欠かさない。', 'Alles schlicht, doch höflich.'),
 q('邻君辞行，然语带疏淡。', '鄰君辭行，然語帶疏淡。', 'The sovereign departs; his words are distant.', 'Государь уезжает; его слова холодны.', '君主は去る——言葉はどこかよそよそしい。', 'Der Souverän reist ab; seine Worte klingen kühl.')
)
ev('peace_priest', 2,
 ('和平之僧', '和平之僧', 'The Peace Monk', 'Монах мира', '平和の僧', 'Der Friedensmönch'),
 ('游方僧人自邻邦来，愿入宫为{king}斡旋旧怨，百姓引颈以待。', '遊方僧人自鄰邦來，願入宮為{king}斡旋舊怨，百姓引頸以待。', 'A wandering monk arrives, offering to mediate old feuds for {king}; the people look on eagerly.', 'Странствующий монах прибыл, готовый уладить старые распри для {king}; народ ждёт с надеждой.', '遍歴の僧が現れ{king}の旧怨を仲裁しようと申し出る——民は首を長くして待つ。', 'Ein Wandermönch kommt und bietet an, alte Fehden für {king} zu schlichten; das Volk wartet hoffnungsvoll.'),
 q('请其斡旋', '請其斡旋', 'Ask him to mediate', 'Просить посредничества', '仲裁を頼む', 'Um Vermittlung bitten'),
 q('赐座宫中，听其说合。', '賜座宮中，聽其說合。', 'Seat him in the palace and hear his counsel.', 'Дать место во дворце и выслушать его совет.', '宮に席を授け和解の説を聞く。', 'Ihm einen Platz im Palast geben und seinem Rat lauschen.'),
 q('老僧往来两都，干戈之气化为檀香。', '老僧往來兩都，干戈之氣化為檀香。', 'The monk shuttles between the capitals; sword anger turns to incense.', 'Монах снуёт между столицами; гнев мечей обращается в ладан.', '僧が両都を往復し剣戟の気が香煙に変わる。', 'Der Mönch pendelt zwischen den Hauptstädten; Schwertzorn wird zu Weihrauch.'),
 q('疑而逐之', '疑而逐之', 'Banish him', 'Изгнать', '疑って追放', 'Ihn verbannen'),
 q('疑为细作，逐出城门。', '疑為細作，逐出城門。', 'Suspect espionage and drive him out.', 'Заподозрить шпионаж и выгнать.', '間者と疑い城門から追い出す。', 'Spionage vermuten und ihn hinauswerfen.'),
 q('僧去人怨，{kingdom}民议渐生问难。', '僧去人怨，{kingdom}民議漸生問難。', 'The monk is gone, but the people murmur against {kingdom}.', 'Монах ушёл, но народ ропщет на {kingdom}.', '僧が去り民が{kingdom}に不満の声を上げ始める。', 'Der Mönch ist fort, aber das Volk murrt gegen {kingdom}.')
)
ev('gift_entourage', 2,
 ('赠礼使团', '贈禮使團', 'The Gift Entourage', 'Дарственная свита', '贈礼の使節', 'Die Geschenk-Gesandtschaft'),
 ('邻国遣使来聘，献上珍玩，礼单上亦列所求，{king}当如何回礼。', '鄰國遣使來聘，獻上珍玩，禮單上亦列所求，{king}當如何回禮。', 'The neighbor sends a mission with rare gifts — and a list of requests. How shall {king} reply?', 'Сосед прислал миссию с редкими дарами — и списком просьб. Как {king} ответит?', '隣国が珍しい品を献じる使節を送る——求める物のリストも添えて。{king}は如何に返礼するか。', 'Der Nachbar entsendet eine Mission mit seltenen Gaben — und einer Wunschliste. Wie antwortet {king}?'),
 q('厚礼回聘', '厚禮回聘', 'Answer in kind', 'Ответить дарами', '厚礼で返す', 'Mit Gaben erwidern'),
 q('回礼重于来礼，馈赠列国。', '回禮重於來禮，饋贈列國。', 'Return gifts beyond their sending, spread among the nations.', 'Ответить дарами щедрее посланного, осыпать страны.', '贈り物より重い礼で各国に報いる。', 'Die Gaben übertreffen und unter den Nationen verteilen.'),
 q('金库为之减色，列国对{kingdom}好感却增。', '金庫為之減色，列國對{kingdom}好感卻增。', 'The vault thins, yet nations warm to {kingdom}.', 'Казна худеет, но страны теплеют к {kingdom}.', '国庫は細るが各国の{kingdom}への好感が増す。', 'Die Kasse schrumpft, doch die Nationen wärmen sich für {kingdom}.'),
 q('等价而酬', '等價而酬', 'Return in kind', 'Ответить равно', '同価で返す', 'Gleichwertig erwidern'),
 q('按礼单之价，如数回赠。', '按禮單之價，如數回贈。', 'Match each gift at its listed worth.', 'Ответить каждой ценностью по списку.', '礼状の価格通りに返礼する。', 'Jede Gabe zu ihrem gelisteten Wert erwidern.'),
 q('礼数周全，交情中等。', '禮數周全，交情中等。', 'Courtesy complete, friendship middling.', 'Учтивость полна, дружба средняя.', '礼は満たされ交情はそれなりに保たれる。', 'Höflichkeit voll, Freundschaft mittelgut.')
)

# ===== 盟约与税市 =====
ev('salt_pact', 2,
 ('食盐之约', '食鹽之約', 'The Salt Pact', 'Соляной пакт', '塩の盟約', 'Der Salz-Pakt'),
 ('邻国盐池歉收，请以牛羊易{kingdom}之盐，并立长年契约。', '鄰國鹽池歉收，請以牛羊易{kingdom}之鹽，並立長年契約。', 'Neighbor salt pans failed; cattle for the salt of {kingdom}, and a lasting pact.', 'Соляные пруды соседа оскудели; скот в обмен на соль {kingdom} и долгий пакт.', '隣国の塩池が不作——牛や羊を{kingdom}の塩と引き換えに永続の約定を望む。', 'Die Salzpfannen des Nachbarn sind ertraglos; Vieh gegen Salz von {kingdom} und ein dauerhafter Pakt.'),
 q('立约通盐', '立約通鹽', 'Sign the pact', 'Подписать пакт', '塩の約定を結ぶ', 'Den Pakt schließen'),
 q('订价立约，常年互市。', '訂價立約，常年互市。', 'Fix a price and trade for years to come.', 'Зафиксировать цену и торговать годами.', '価格を定め長年にわたり交易する。', 'Einen Preis festlegen und jahrelang Handel treiben.'),
 q('盐路畅通，邻国尊{kingdom}为信友。', '鹽路暢通，鄰國尊{kingdom}為信友。', 'The salt road flows; the neighbor holds {kingdom} as a true friend.', 'Соляной путь течёт; сосед чтит {kingdom} как друга.', '塩の道が通じ隣国が{kingdom}を信友と仰ぐ。', 'Der Salzweg fließt; der Nachbar hält {kingdom} für einen echten Freund.'),
 q('抬价限盐', '抬價限鹽', 'Jack up the price', 'Взвинтить цену', '値上げと制限', 'Den Preis treiben'),
 q('奇货可居，配额而售。', '奇貨可居，配額而售。', 'Hoard the salt and sell by quota.', 'Придержать соль и продавать по квотам.', '希少価値を狙い配給制で売る。', 'Salz horten und nur kontingentiert verkaufen.'),
 q('邻民淡食一年，怨声直指{kingdom}。', '鄰民淡食一年，怨聲直指{kingdom}。', 'A neighbor year of plain food — the blame falls on {kingdom}.', 'Год без соли у соседа — винят {kingdom}.', '隣民が一年塩なし——怨嗟が{kingdom}に向く。', 'Ein Jahr ohne Salz beim Nachbarn — der Zorn trifft {kingdom}.')
)
ev('grain_bridge', 2,
 ('粮道之开', '糧道之開', 'The Grain Bridge', 'Хлебный мост', '穀物の道', 'Die Kornbrücke'),
 ('邻国青黄不接，请开{kingdom}粮道，平价购粮度荒。', '鄰國青黃不接，請開{kingdom}糧道，平價購糧度荒。', 'The neighbor faces lean months; grain may travel the roads of {kingdom} for a fair price.', 'У соседа голодные месяцы; хлеб может поехать по дорогам {kingdom} за честную цену.', '隣国が端境期に突入——{kingdom}の糧道を開き適正価格で穀物を買いたいと願う。', 'Der Nachbar steht vor mageren Monaten; Getreide soll für fairen Preis über die Straßen von {kingdom} rollen.'),
 q('开道平粜', '開道平糶', 'Open the road', 'Открыть дорогу', '道を開き平糶', 'Die Straße öffnen'),
 q('平价售粮，课市税。', '平價售糧，課市稅。', 'Sell grain at fair prices, taxes aside.', 'Продавать хлеб по честной цене.', '適正価格で穀物を売り市税を課す。', 'Getreide zum fairen Preis verkaufen.'),
 q('粮车如流，邻邦盛赞{kingdom}有邻国之谊。', '糧車如流，鄰邦盛讚{kingdom}有鄰國之誼。', 'Grain wagons stream across; the neighbor praises the warmth of {kingdom}.', 'Хлебные обозы текут; сосед славит теплоту {kingdom}.', '穀物の車が続々と——隣国が{kingdom}の情誼を称える。', 'Getreidewagen strömen; der Nachbar rühmt die Wärme von {kingdom}.'),
 q('闭道自守', '閉道自守', 'Close the road', 'Закрыть дорогу', '道を閉ざす', 'Die Straße sperren'),
 q('恐粮价腾贵，严守关口。', '恐糧價騰貴，嚴守關口。', 'Fear price rises and watch the passes.', 'Боясь роста цен, стеречь перевалы.', '物価高騰を恐れ関所を固く守る。', 'Preissteigerungen fürchten und die Pässe bewachen.'),
 q('邻国艰难度荒，所幸未生怨怼。', '鄰國艱難渡荒，所幸未生怨懟。', 'The neighbor scrapes through, mercifully without grudge.', 'Сосед пробился, к счастью, без обиды.', '隣国は何とかしのぐ——幸い恨みは出ない。', 'Der Nachbar quält sich durch — glücklicherweise ohne Groll.')
)
ev('ship_licence', 2,
 ('船引之制', '船引之制', 'Ship Licences', 'Корабельные грамоты', '船引の制', 'Schiffs-Lizenzen'),
 ('邻国海商请领{kingdom}船引，按引纳金，即可通洋，利源可观。', '鄰國海商請領{kingdom}船引，按引納金，即可通洋，利源可觀。', 'Neighbor shipmasters seek licences of {kingdom} — gold for the right to sail the seas.', 'Соседние шкиперы просят грамоты {kingdom} — золото за право выходить в море.', '隣国の船商が{kingdom}の船引を求める——金を納めれば外洋へ出られ、利源は大きい。', 'Nachbarkapitäne bitten um Lizenzen von {kingdom} — Gold für das Recht zur See, eine fette Quelle.'),
 q('颁引课金', '頒引課金', 'Issue & levy', 'Выдать и облагать', '船引を発行し課金', 'Ausstellen & besteuern'),
 q('按引纳金，海船展限。', '按引納金，海船展限。', 'Collect the fee; let the ships sail on.', 'Собрать плату и отпустить корабли.', '船引の金を収め船を出させる。', 'Gebühr kassieren und Schiffe segeln lassen.'),
 q('金库渐盈，海贸之利初现。', '金庫漸盈，海貿之利初現。', 'The vault fattens; the first fruits of sea trade appear.', 'Казна полнеет; первые плоды морской торговли.', '国庫が潤い海の交易の利が見え始める。', 'Die Kasse schwillt; erste Früchte des Seehandels zeigen sich.'),
 q('严限船引', '嚴限船引', 'Restrict the licences', 'Урезать грамоты', '船引を厳しく制限', 'Lizenzen beschränken'),
 q('限定艘数，层层核验。', '限定艘數，層層核驗。', 'Cap the numbers and demand endless checks.', 'Ограничить число и требовать бесконечных проверок.', '艦数を絞り厳重に検分する。', 'Zahlen deckeln und endlose Prüfungen fordern.'),
 q('商船裹足，邻国海商怨声载道。', '商船裹足，鄰國海商怨聲載道。', 'Ships hold back; neighbor merchants grumble loudly.', 'Корабли задержались; соседние купцы громко ворчат.', '船が足を止め隣国の海商が憤る。', 'Schiffe zögern; Nachbarkaufleute murren laut.')
)
ev('ferry_treaty', 2,
 ('渡口之约', '渡口之約', 'The Ferry Treaty', 'Паромный договор', '渡しの約定', 'Der Fährvertrag'),
 ('两境一水相隔，渡船获利不均，争渡不止，邻国请订规章。', '兩境一水相隔，渡船獲利不均，爭渡不止，鄰國請訂規章。', 'One river divides the realms; ferries earn unevenly and quarrels continue — the neighbor proposes a charter.', 'Одна река разделяет державы; паромы зарабатывают неравно, ссоры не утихают — сосед предлагает устав.', '一つの川が両国を分かつ——渡し舟の利が偏り争いも絶えず、隣国が規則を望む。', 'Ein Fluss trennt die Reiche; Fähren verdienen ungleich, Zank hört nicht auf — der Nachbar schlägt eine Ordnung vor.'),
 q('共订章程', '共訂章程', 'Charter together', 'Устав сообща', '規約を共に定める', 'Gemeinsame Ordnung'),
 q('均分渡资，各设渡口。', '均分渡資，各設渡口。', 'Split the fares and run the crossings together.', 'Делить плату и держать переправы вместе.', '渡し賃を分け合い双方に渡船場を置く。', 'Fahrgelder teilen und die Überfahrten gemeinsam betreiben.'),
 q('行人称便，两邦边民交好。', '行人稱便，兩邦邊民交好。', 'Travelers praise the ease; border folk grow friendly.', 'Путники хвалят удобство; пограничники дружат.', '旅人が喜び国境の民が親しくなる。', 'Reisende loben den Komfort; Grenzleute werden freundlich.'),
 q('各守其渡', '各守其渡', 'Keep separate', 'Каждому своё', '各々に渡し', 'Jeder für sich'),
 q('维持旧例，渡资各取。', '維持舊例，渡資各取。', 'Keep the old ways; each keeps its fares.', 'Оставить по-старому; каждый берёт своё.', '旧例を守り渡し賃は各自が取る。', 'Beim Alten bleiben; jede Seite kassiert ihre Fahrgelder.'),
 q('渡口依旧，争渡之声未绝。', '渡口依舊，爭渡之聲未絕。', 'The crossings stay as they were; squabbles over passage continue.', 'Переправы прежние; споры о проходе продолжаются.', '渡し場は変わらず争いの声が絶えない。', 'Die Überfahrten bleiben; Gezänk um die Passage hört nicht auf.')
)
ev('exile_welcome', 2,
 ('流亡来投', '流亡來投', 'The Exiles', 'Изгнанники', '亡命者の到来', 'Die Verbannten'),
 ('邻邦内乱，旧臣携家眷奔至{kingdom}关前，坐地请命，观者如堵。', '鄰邦內亂，舊臣攜家眷奔至{kingdom}關前，坐地請命，觀者如堵。', 'Civil strife drives old officials with their kin to the gates of {kingdom}, petitioning as crowds watch.', 'Внутренняя смута гонит старых сановников с роднёй к воротам {kingdom} с челобитной, а зрители смотрят.', '隣国の内乱で旧臣たちが家族を連れ{kingdom}の関門に座り込み請願し、見物人で埋まる。', 'Bürgerkrieg treibt alte Beamte und Sippen an die Tore von {kingdom}; sie flehen, während Mengen zusehen.'),
 q('开边纳之', '開邊納之', 'Open the border', 'Открыть границу', '国境を開く', 'Die Grenze öffnen'),
 q('设棚授田，编入里甲。', '設棚授田，編入里甲。', 'Shelter them, grant fields, enroll them in the registers.', 'Приютить, дать поля, вписать в списки.', '小屋を設け田を授け戸籍に編入する。', 'Sie aufnehmen, Felder geben, in die Register eintragen.'),
 q('流亡安居，列国赞{kingdom}有先王之风。', '流亡安居，列國贊{kingdom}有先王之風。', 'The exiles settle; nations praise {kingdom} for the old kings\' virtue.', 'Изгнанники осели; страны славят {kingdom} за добродетель древних царей.', '亡命者が落ち着き各国が{kingdom}の古風を称える。', 'Die Verbannten siedeln; Nationen rühmen die Tugend alter Könige in {kingdom}.'),
 q('闭门逐客', '閉門逐客', 'Shut the gates', 'Закрыть ворота', '門を閉ざす', 'Die Tore schließen'),
 q('严令出境，不得收留。', '嚴令出境，不得收留。', 'Order them out; none may shelter them.', 'Приказать уйти; никто не вправе приютить.', '国外への退去を命じ収容を禁ずる。', 'Ausweisung befehlen und Aufnahme verbieten.'),
 q('流民哭于野，{kingdom}民心摇动。', '流民哭於野，{kingdom}民心动搖。', 'The exiles wail in the fields; the spirit of {kingdom} wavers.', 'Изгнанники плачут в полях; дух {kingdom} колеблется.', '亡命者が野に哭き{kingdom}の民の心が揺れる。', 'Die Verbannten weinen auf den Feldern; der Geist von {kingdom} wankt.')
)
ev('rival_envoy', 2,
 ('宿敌来使', '宿敵來使', 'The Rival Envoy', 'Посланец соперника', '宿敵の使者', 'Der Rivalen-Gesandte'),
 ('世仇之国遣使而来，国书言辞恭谨，怀中或藏他图，{king}举棋不定。', '世仇之國遣使而來，國書言辭恭謹，懷中或藏他圖，{king}舉棋不定。', 'The old foe sends an envoy; the letter is courteous and may hide designs, and {king} wavers.', 'Старый враг прислал посланника; письмо учтиво и может таить замыслы, и {king} колеблется.', '宿敵が使者を送る——国書は恭しいが胸に秘策があるやも。{king}は決めかねる。', 'Der alte Feind schickt einen Gesandten; der Brief ist höflich und birgt vielleicht Pläne — {king} schwankt.'),
 q('当廷斥之', '當廷斥之', 'Rebuke in court', 'Осадить при дворе', '廷上で罵倒', 'Vor dem Hof zurechtweisen'),
 q('掷还国书，逐客出境。', '擲還國書，逐客出境。', 'Throw the letter back and drive him out.', 'Швырнуть письмо обратно и выгнать.', '国書を投げ返し客を国外に逐う。', 'Den Brief zurückwerfen und ihn hinausweisen.'),
 q('快意一时，列国却视{kingdom}为轻躁。', '快意一時，列國卻視{kingdom}為輕躁。', 'Satisfying for a moment; nations now deem {kingdom} hotheaded.', 'Удовлетворение на миг; страны сочли {kingdom} горячей.', '一瞬の快意——だが各国は{kingdom}を軽率と見る。', 'Kurz befriedigend; doch die Nationen halten {kingdom} für hitzköpfig.'),
 q('折节相迎', '折節相迎', 'Receive with grace', 'Принять с достоинством', '身を屈して迎える', 'Mit Anstand empfangen'),
 q('宴请来使，以礼还礼。', '宴請來使，以禮還禮。', 'Feast the envoy and answer courtesy with courtesy.', 'Угостить посланника и ответить учтивостью на учтивость.', '使者を宴し礼をもって礼に報いる。', 'Den Gesandten bewirten und Höflichkeit mit Höflichkeit vergelten.'),
 q('杯酒释怨，列国称{kingdom}有容人之量。', '杯酒釋怨，列國稱{kingdom}有容人之量。', 'Wine dissolves old anger; nations praise {kingdom} for a generous heart.', 'Вино растворяет старый гнев; страны хвалят {kingdom} за щедрость души.', '杯酒で遺恨が溶け各国が{kingdom}の寛容を称える。', 'Wein löst alten Zorn; Nationen rühmen die Großzügigkeit von {kingdom}.')
)
ev('neutral_pledge', 2,
 ('中立之诺', '中立之諾', 'The Neutral Pledge', 'Нейтральное слово', '中立の誓約', 'Das Neutralitäts-Versprechen'),
 ('两强相争，邻国来求{king}具书中立，两不相帮，使者候于宫门。', '兩強相爭，鄰國來求{king}具書中立，兩不相幫，使者候於宮門。', 'Great powers contest; the neighbor begs {king} to swear neutrality in writing while envoys wait at the gate.', 'Великие державы спорят; сосед молит {king} письменно поклясться в нейтралитете, послы ждут у ворот.', '大国が争い隣国が{king}に中立を書面で誓うよう求める——使者は宮門で待つ。', 'Großmächte streiten; der Nachbar bittet {king} um schriftlich geschworene Neutralität, Gesandte warten am Tor.'),
 q('慨然应之', '慨然應之', 'Pledge at once', 'Поклясться сразу', '快く応じる', 'Sofort geloben'),
 q('国书具名，宣布中立。', '國書具名，宣佈中立。', 'Sign the letter and declare neutrality.', 'Подписать письмо и объявить нейтралитет.', '国書に署名し中立を宣する。', 'Das Schreiben unterzeichnen und Neutralität erklären.'),
 q('邻邦安心，{kingdom}亦得清静。', '鄰邦安心，{kingdom}亦得清靜。', 'The neighbor rests easy; {kingdom} finds quiet too.', 'Сосед спокоен; и {kingdom} обретает тишину.', '隣国が安心し{kingdom}も静けさを得る。', 'Der Nachbar atmet auf; auch {kingdom} findet Ruhe.'),
 q('模棱两可', '模稜兩可', 'Stay ambiguous', 'Остаться двусмысленным', '曖昧に構える', 'Vage bleiben'),
 q('含糊其辞，不置可否。', '含糊其辭，不置可否。', 'Speak in riddles and commit to nothing.', 'Говорить загадками и ни к чему не обязываться.', '言葉を濁し諾否を言わない。', 'In Rätseln sprechen und sich nichts verpflichten.'),
 q('使者归报，邻邦虽疑而未发。', '使者歸報，鄰邦雖疑而未發。', 'The envoy returns; the neighbor doubts but holds back.', 'Посланник вернулся; сосед сомневается, но сдерживается.', '使者が戻り隣国は疑いつつも出ない。', 'Der Gesandte kehrt heim; der Nachbar zweifelt, hält aber zurück.')
)
ev('betrothal_overture', 2,
 ('提亲联姻', '提親聯姻', 'The Betrothal', 'Предложение о браке', '縁談の申し込み', 'Der Heiratsantrag'),
 ('邻国太子慕{kingdom}宗女之名，遣媒致聘，礼书华美，阖宫议论。', '鄰國太子慕{kingdom}宗女之名，遣媒致聘，禮書華美，闔宮議論。', 'The neighbor prince admires a princess of {kingdom}; matchmakers come with ornate letters and the palace buzzes.', 'Соседний принц восхищается принцессой из {kingdom}; сваты явились с пышными письмами, дворец гудит.', '隣国の太子が{kingdom}の姫君に恋慕し媒人を遣わす——華麗な聘礼に宮中が騒ぐ。', 'Der Nachbarprinz bewundert eine Prinzessin von {kingdom}; Heiratsvermittler kommen mit prächtigen Briefen, der Palast summt.'),
 q('许婚结好', '許婚結好', 'Betroth them', 'Помолвить', '婚約を許す', 'Verloben'),
 q('允亲下聘，易子而盟。', '允親下聘，易子而盟。', 'Accept the betrothal; bind the houses.', 'Принять помолвку; связать дома.', '縁談を許し両家を結ぶ。', 'Die Verlobung annehmen; die Häuser verbinden.'),
 q('红绳既定，两邦之好如胶似漆。', '紅繩既定，兩邦之好如膠似漆。', 'The red thread is tied; the two realms share close friendship.', 'Красная нить связана; дружба двух держав крепка.', '赤い糸が結ばれ両国の親交が固まる。', 'Der rote Faden ist geknüpft; die Freundschaft der Reiche festigt sich.'),
 q('婉言辞谢', '婉言辭謝', 'Decline gently', 'Вежливо отказать', '辞退する', 'Höflich ablehnen'),
 q('以宗女年幼，暂缓其议。', '以宗女年幼，暫緩其議。', 'Plead tender age and defer the matter.', 'Сослаться на юный возраст и отложить.', '姫が幼いとし話を持ち越す。', 'Auf junges Alter verweisen und verschieben.'),
 q('媒使悻悻而归，然两邦未伤和气。', '媒使悻悻而歸，然兩邦未傷和氣。', 'The matchmaker leaves glum, yet the realms stay cordial.', 'Сват уходит кислым, но державы остаются любезны.', '媒人は悔しげに帰るが両国の仲は保たれる。', 'Der Vermittler zieht verdrossen ab, doch die Reiche bleiben höflich.')
)
ev('border_market', 2,
 ('边市之开', '邊市之開', 'The Border Market', 'Пограничный рынок', '辺境の市', 'Der Grenzmarkt'),
 ('边民请开互市，通关货税，利官利民，{king}可否其请。', '邊民請開互市，通關貨稅，利官利民，{king}可否其請。', 'Border folk petition for a shared market; tolls would benefit crown and people. Will {king} grant it?', 'Пограничники просят общий рынок; пошлины пойдут на пользу короне и народу. Разрешит ли {king}?', '辺民が互市の開設を願う——関税は官も民も潤す。{king}は許可するか。', 'Grenzbewohner bitten um einen gemeinsamen Markt; Zölle nützen Krone und Volk. Gewährt {king} es?'),
 q('开市互易', '開市互易', 'Open the market', 'Открыть рынок', '市を開く', 'Den Markt öffnen'),
 q('设关税司，听商往来。', '設關稅司，聽商往來。', 'Set up the toll office and let trade flow.', 'Учредить таможню и пустить торговлю.', '関税の役所を置き商を通す。', 'Zollamt einrichten und Handel fließen lassen.'),
 q('边市兴旺，关税入帑，{kingdom}民生渐活。', '邊市興旺，關稅入帑，{kingdom}民生漸活。', 'The market thrives; tolls fill the chest and life quickens in {kingdom}.', 'Рынок процветает; пошлины полнят сундук, жизнь в {kingdom} оживает.', '市場が栄え関税が国庫に入り{kingdom}の暮らしが活気づく。', 'Der Markt blüht; Zölle füllen die Truhe, das Leben in {kingdom} belebt sich.'),
 q('禁市自守', '禁市自守', 'Forbid the market', 'Запретить рынок', '市を禁ずる', 'Den Markt verbieten'),
 q('恐奸民滋事，闭市锁关。', '恐奸民滋事，閉市鎖關。', 'Fear troublemakers and shut the crossings.', 'Боясь смутьянов, закрыть переходы.', '奸民の乱を恐れ市を閉じ関を鍵める。', 'Unruhestifter fürchten und die Übergänge sperren.'),
 q('明市虽禁，私贩仍暗行于野。', '明市雖禁，私販仍暗行於野。', 'The open market is barred, yet smugglers prowl the fields.', 'Открытый рынок закрыт, но контрабандисты рыщут по полям.', '表向きの市は禁じても闇売買が野を這う。', 'Der offene Markt ist gesperrt, doch Schmuggler schleichen durch die Felder.')
)
ev('trade_embargo', 2,
 ('商路之断', '商路之斷', 'The Trade Embargo', 'Торговое эмбарго', '商路の断絶', 'Die Handelsblockade'),
 ('邻国商队行险走私，抵律当罚；朝臣请断其商路，庙堂争论不休。', '鄰國商隊行險走私，抵律當罰；朝臣請斷其商路，廟堂爭論不休。', 'A neighbor caravan risked smuggling and broke the law; ministers urge cutting off its trade while the court argues.', 'Караван соседа рискнул контрабандой и нарушил закон; министры требуют закрыть торговлю, двор спорит.', '隣国の隊商が密輸を犯した——朝臣が商路の遮断を求め、廷中で議論が尽きない。', 'Eine Nachbarkarawane riskierte Schmuggel und brach das Gesetz; Minister drängen auf Handelsstopp, der Hof streitet.'),
 q('断市禁运', '斷市禁運', 'Cut off trade', 'Разорвать торговлю', '商路を断つ', 'Handel abschneiden'),
 q('敕令闭关，商旅止步。', '敕令閉關，商旅止步。', 'Close the gates by decree; halt all caravans.', 'Закрыть ворота указом; остановить караваны.', '勅令で関を閉じ隊商を止める。', 'Die Tore per Erlass schließen; alle Karawanen stoppen.'),
 q('商市萧条，邻国受创，怨怒俱生。', '商市蕭條，鄰國受創，怨怒俱生。', 'The markets wilt; the neighbor bleeds — grievance and anger both grow.', 'Рынки вянут; сосед страдает — растут обида и злость.', '市場が冷え込み隣国が痛手を受けて怨みが募る。', 'Märkte welken; der Nachbar blutet — Groll und Zorn wachsen.'),
 q('罚后如旧', '罰後如舊', 'Fine & restore', 'Штраф и возврат', '罰して旧に復す', 'Strafen & wiederherstellen'),
 q('课以重罚，仍许通商。', '課以重罰，仍許通商。', 'Levy a heavy fine, then allow trade again.', 'Взыскать крупный штраф и снова пустить торговлю.', '重い罰金を課して交易は許す。', 'Eine schwere Strafe verhängen, dann Handel wieder erlauben.'),
 q('罚金入库，商道复通，邻邦称其得宜。', '罰金入庫，商道復通，鄰邦稱其得宜。', 'The fine enters the vault, trade resumes; the neighbor calls it fair.', 'Штраф вошёл в казну, торговля возобновилась; сосед считает это справедливым.', '罰金が国庫に入り商路が戻り、隣国は妥当と評する。', 'Die Strafe fließt in die Kasse, Handel kehrt zurück; der Nachbar nennt es gerecht.')
)
ev('embassy_code', 2,
 ('使节之礼', '使節之禮', 'The Envoy Rites', 'Посольские обряды', '使節の礼', 'Das Gesandtenzeremoniell'),
 ('列国使节仪仗无定制，屡生争执，{king}欲颁礼法以正其序。', '列國使節儀仗無定製，屢生爭執，{king}欲頒禮法以正其序。', 'Envoy processions lack fixed rules and quarrels keep breaking out; {king} would issue a code.', 'Посольские шествия без правил, ссоры не утихают; {king} хочет издать устав.', '使節の儀仗に定めがなく争いが絶えない——{king}が礼法を定めようとする。', 'Gesandtenzüge ohne feste Regeln, Streit bricht aus — {king} will eine Ordnung erlassen.'),
 q('颁行礼法', '頒行禮法', 'Issue the code', 'Издать устав', '礼法を発布', 'Die Ordnung erlassen'),
 q('按等定仪，使节循行。', '按等定儀，使節循行。', 'Fix ranks and rites; envoys follow them.', 'Установить ранги и обряды; посланники следуют им.', '階級に応じ儀を定め使者が従う。', 'Ränge und Riten festlegen; Gesandte folgen ihnen.'),
 q('礼节有序，使馆往来焕然一新。', '禮節有序，使館往來煥然一新。', 'Rites are ordered; embassy traffic takes on new polish.', 'Обряды упорядочены; посольские сношения засияли заново.', '礼が整い使館の往来が一新する。', 'Die Riten ordnen sich; der Gesandtenverkehr gewinnt neuen Glanz.'),
 q('谨守旧俗', '謹守舊俗', 'Keep old customs', 'Держать старые обычаи', '旧俗を守る', 'Alte Bräuche wahren'),
 q('不另立法，听其自便。', '不另立法，聽其自便。', 'No new law; let them do as they please.', 'Без нового закона; пусть живут как хотят.', '新しい法を出さず勝手にさせる。', 'Kein neues Gesetz; sie sollen tun, was sie wollen.'),
 q('仪仗夺路，市民侧目，物议沸腾。', '儀仗奪路，市民側目，物議沸騰。', 'Processions shove through streets; citizens glare and gossip boils.', 'Шествия проталкиваются по улицам; граждане ворчат, молва кипит.', '儀仗が道を奪い市民が白い目を向け物議が沸く。', 'Züge drängen durch die Straßen; Bürger glotzen, Klatsch kocht.')
)
ev('spy_swap', 2,
 ('细作交换', '細作交換', 'The Spy Swap', 'Обмен шпионами', '間者の交換', 'Der Spionagetausch'),
 ('邻国擒获{kingdom}细作三名，愿以彼国被擒者相易，各掩其丑。', '鄰國擒獲{kingdom}細作三名，願以彼國被擒者相易，各掩其醜。', 'The neighbor captured three agents of {kingdom} and offers their own captives in exchange, saving face both ways.', 'Сосед пленил трёх агентов {kingdom} и предлагает взамен своих, сохраняя лицо обеим сторонам.', '隣国が{kingdom}の間者三人を捕え、自国の捕虜と交換して互いに顔を守りたいと申し出る。', 'Der Nachbar fing drei Agenten von {kingdom} und bietet eigene Gefangene zum Tausch — beide wahren das Gesicht.'),
 q('换回细作', '換回細作', 'Swap them back', 'Обменять обратно', '間者を交換する', 'Zurücktauschen'),
 q('两处换俘，彼此心照。', '兩處換俘，彼此心照。', 'Exchange on the border, by unspoken accord.', 'Обмен на границе, по молчаливому уговору.', '国境で交換し互いに察する。', 'An der Grenze tauschen, nach stillem Einvernehmen.'),
 q('人归帐静，两邦暗结默契。', '人歸帳靜，兩邦暗結默契。', 'The agents return; a silent understanding binds the courts.', 'Агенты вернулись; молчаливое понимание связывает дворы.', '間者が戻り両国の間に暗黙の了解が生まれる。', 'Die Agenten kehren heim; stilles Einvernehmen verbindet die Höfe.'),
 q('矢口否认', '矢口否認', 'Deny it all', 'Всё отрицать', '否認を貫く', 'Alles leugnen'),
 q('坚称并无细作，严拒其议。', '堅稱並無細作，嚴拒其議。', 'Insist there are no agents and refuse the deal.', 'Настаивать, что агентов нет, и отказаться.', '間者はいないと強弁し拒否する。', 'Auf Abwesenheit von Agenten bestehen und ablehnen.'),
 q('来人悻悻而去，三名细作仍禁于邻狱。', '來人悻悻而去，三名細作仍禁於鄰獄。', 'The envoy leaves glum; the three remain in the neighbor dungeon.', 'Посланник уходит кислым; трое остаются в соседней темнице.', '使者は悔しげに去り三人は隣国の牢に残る。', 'Der Gesandte zieht verdrossen ab; die drei bleiben im Kerker des Nachbarn.')
)
ev('pirate_offer', 2,
 ('海盗投书', '海盜投書', 'The Pirate Offer', 'Предложение пиратов', '海賊の申し出', 'Das Piraten-Angebot'),
 ('一伙海盗遣使投书，请{king}准其纳金自赎、编入水师，海疆为之一动。', '一夥海盜遣使投書，請{king}准其納金自贖、編入水師，海疆為之一動。', 'A pirate band sends a letter asking {king} to let them pay gold in atonement and join the navy; the seas stir.', 'Пиратская ватага шлёт письмо с просьбой к {king} откупиться золотом и вступить во флот; моря волнуются.', '海賊の一団が書を送る——金を納めて水軍に加わりたいと{king}に願う。海がざわめく。', 'Eine Piratenbande sendet {king} einen Brief: Gold als Sühne zahlen und in die Marine eintreten; die See regt sich.'),
 q('许其招安', '許其招安', 'Accept surrender', 'Принять капитуляцию', '帰順を許す', 'Die Kapitulation annehmen'),
 q('授职正名，编入水营。', '授職正名，編入水營。', 'Give them rank and register them in the fleet.', 'Дать чины и вписать во флот.', '官職を与え水軍に編入する。', 'Rang verleihen und in die Flotte eintragen.'),
 q('海波渐宁，邻国感{kingdom}弭盗之德。', '海波漸寧，鄰國感{kingdom}弭盜之德。', 'The seas settle; neighbors feel the merit of {kingdom} in quelling pirates.', 'Моря утихают; соседи чувствуют заслугу {kingdom} в усмирении пиратов.', '海が静まり隣国が{kingdom}の盗賊討滅の徳を感じる。', 'Die See beruhigt sich; Nachbarn spüren das Verdienst von {kingdom} an der Piratenvertilgung.'),
 q('纳金放行', '納金放行', 'Take gold & let go', 'Взять золото и отпустить', '金を取って見逃す', 'Gold nehmen und ziehen lassen'),
 q('收其赎金，任其远遁。', '收其贖金，任其遠遁。', 'Collect the gold and let them sail away.', 'Взять золото и отпустить в море.', '身代金を収め遠くへ去らせる。', 'Das Gold kassieren und sie segeln lassen.'),
 q('赎金入库，然海中又添一伙强徒。', '贖金入庫，然海中又添一夥強徒。', 'The ransom fills the vault, but a new gang of rogues rides the waves.', 'Выкуп полнит казну, но новая ватага уже в море.', '身代金は国庫に入るが海にまた強盗が増える。', 'Das Lösegeld füllt die Kasse, doch eine neue Bande reitet die Wellen.')
)

# ===== 难民与边务 =====
ev('refugee_treaty', 2,
 ('难民之约', '難民之約', 'The Refugee Treaty', 'Договор о беженцах', '難民の約定', 'Der Flüchtlingsvertrag'),
 ('邻邦战乱，难民如潮涌至{kingdom}边境，民舍难容，官府请命。', '鄰邦戰亂，難民如潮湧至{kingdom}邊境，民舍難容，官府請命。', 'War in the neighbor realm sends refugees in floods to the border of {kingdom}; dwellings cannot hold them and officials petition.', 'Война у соседа гонит беженцев потоком к границе {kingdom}; жилищ не хватает, чиновники просят указаний.', '隣国の戦乱で難民が{kingdom}の国境に押し寄せ、家々に収まり切れず役所が裁決を仰ぐ。', 'Krieg im Nachbarreich treibt Flüchtlingsfluten an die Grenze von {kingdom}; Unterkünfte reichen nicht, Beamte bitten um Weisung.'),
 q('立约纳民', '立約納民', 'Sign & shelter', 'Подписать и приютить', '約定して迎える', 'Willkommen heißen'),
 q('设帐授粮，另立里甲安置。', '設帳授糧，另立里甲安置。', 'Pitch tents, give grain, and settle them in new hamlets.', 'Поставить шатры, дать хлеб, поселить в новые округа.', '天幕を張り糧を配り新たに里甲を設ける。', 'Zelte aufschlagen, Korn geben, in neuen Weilern ansiedeln.'),
 q('难民得所，列国盛赞{kingdom}仁政。', '難民得所，列國盛讚{kingdom}仁政。', 'The refugees find homes; nations extol benevolent rule in {kingdom}.', 'Беженцы обрели дом; страны превозносят милостивое правление {kingdom}.', '難民が住まいを得て各国が{kingdom}の仁政を称賛する。', 'Die Flüchtlinge finden Heime; Nationen rühmen die milde Herrschaft von {kingdom}.'),
 q('闭关严拒', '閉關嚴拒', 'Seal & refuse', 'Запереть и отказать', '関を閉ざして拒む', 'Sperren und ablehnen'),
 q('增兵守关，难民不得入境。', '增兵守關，難民不得入境。', 'Reinforce the gates; no refugee may enter.', 'Усилить заставы; беженцам вход закрыт.', '兵を増やし関で難民を止める。', 'Die Tore verstärken; kein Flüchtling darf einreisen.'),
 q('难民哭于关下，{kingdom}民情亦自恻然。', '難民哭於關下，{kingdom}民情亦自惻然。', 'Refugees weep below the gates; the folk of {kingdom} grieve too.', 'Беженцы плачут у ворот; и народ {kingdom} опечален.', '難民が関下に泣き{kingdom}の民も胸を痛める。', 'Flüchtlinge weinen vor den Toren; auch die Leute von {kingdom} trauern.')
)
ev('river_fishery', 2,
 ('河渔之共', '河漁之共', 'The River Fishery', 'Речное рыболовство', '川漁の共有', 'Die Flussfischerei'),
 ('界河鲜鱼之利，两国竞取，汛期将至，渔舟已各据一方。', '界河鮮魚之利，兩國競取，汛期將至，漁舟已各據一方。', 'The border river yields choice fish; both realms compete as the season nears and boats already hold their sides.', 'Пограничная река даёт отборную рыбу; оба царства соревнуются, сезон близко, лодки заняли свои стороны.', '国境の川の魚の利を両国が競う——漁期が近づき漁船が互いの位置を占める。', 'Der Grenzfluss liefert feine Fische; beide Reiche wetteifern, die Saison naht, Boote halten ihre Seiten.'),
 q('共约渔期', '共約漁期', 'Share the season', 'Делить сезон', '漁期を共にする', 'Die Saison teilen'),
 q('轮流下网，休渔有时。', '輪流下網，休漁有時。', 'Stagger the nets; keep set rest seasons.', 'Чередовать сети; держать сроки отдыха.', '網を交替にし禁漁期を設ける。', 'Netze abwechseln; feste Schonzeiten wahren.'),
 q('鱼利均沾，界河之讼竟然息鼓。', '魚利均霑，界河之訟竟然息鼓。', 'The catch is shared; the river lawsuits fall silent.', 'Улов общий; тяжбы о реке стихли.', '魚の利を分け合い川の争いが静まる。', 'Der Fang wird geteilt; die Flussprozesse verstummen.'),
 q('自行取利', '自行取利', 'Fish alone', 'Рыбачить в одиночку', '独りで漁る', 'Allein fischen'),
 q('不立约，各凭本事下水。', '不立約，各憑本事下水。', 'No pact; every boat on its own skill.', 'Без пакта; каждая лодка на свой страх.', '約定せず腕力で網を打つ。', 'Kein Pakt; jedes Boot nach eigenem Können.'),
 q('互有侵渔，小怨时起。', '互有侵漁，小怨時起。', 'Poaching on both sides; small grudges flare.', 'Взаимный браконьерский лов; вспыхивают мелкие обиды.', '互いに越境漁があり小さな不満が絶えない。', 'Beidseitiges Wildfischen; kleine Grollen lodern.')
)
ev('mountain_pass', 2,
 ('山道之通', '山道之通', 'The Mountain Pass', 'Горный перевал', '峠道の通行', 'Der Bergpass'),
 ('两国山道险隘，邻国请开栈道以便商旅，遣使持书而至。', '兩國山道險隘，鄰國請開棧道以便商旅，遣使持書而至。', 'The mountain road between the realms is craggy; the neighbor sends a letter asking to open the plankway for trade.', 'Горная дорога меж держав крута; сосед шлёт письмо с просьбой открыть мостки для торговли.', '両国の間の峠道は険しい——隣国が商旅のために桟道を開きたいと書を遣わしてくる。', 'Der Bergweg ist zerklüftet; der Nachbar sendet einen Brief und bittet, den Steig für den Handel zu öffnen.'),
 q('允开栈道', '允開棧道', 'Open the road', 'Открыть дорогу', '桟道を開く', 'Den Steig öffnen'),
 q('借工修路，减半关税。', '借工修路，減半關稅。', 'Lend workers for the road; halve the tolls.', 'Дать рабочих для дороги; пошлины вдвое ниже.', '人夫を貸し関税を半減する。', 'Arbeiter leihen und Zölle halbieren.'),
 q('栈道通商，商旅络绎于途。', '棧道通商，商旅絡繹於途。', 'The plankway opens; traders stream along it.', 'Мостки открыты; торговцы текут потоком.', '桟道が通り商人が途に連なる。', 'Der Steig öffnet sich; Händler strömen entlang.'),
 q('闭关守隘', '閉關守隘', 'Seal the pass', 'Запереть перевал', '峠を封鎖', 'Den Pass sperren'),
 q('设卡敛税，禁予通行。', '設卡斂稅，禁予通行。', 'Raise toll barriers and deny passage.', 'Поставить заставы и закрыть проход.', '関所を設け課税し通行を禁ずる。', 'Schranken errichten und den Durchgang verweigern.'),
 q('关隘虽固，邻邦商旅绕道相怨。', '關隘雖固，鄰邦商旅繞道相怨。', 'The gate holds, yet neighbor caravans detour and resent the realm.', 'Застава держится, но соседние караваны идут в обход и ропщут.', '関は固いが隣国の隊商が迂回して恨む。', 'Das Tor hält, doch Karawanen umgehen es und grollen dem Reich.')
)

# ===== 文化使团 =====
ev('temple_diplomacy', 2,
 ('界山梵寺', '界山梵寺', 'The Shrine Accord', 'Храмовый пакт', '界山の梵刹', 'Der Bergtempel-Akkord'),
 ('邻国遣僧来请，愿同修梵寺于界山，香火共奉，两境同沾。', '鄰國遣僧來請，願同修梵寺於界山，香火共奉，兩境同霑。', 'The neighbor sends monks: let both realms build a shrine on the border mountain and share its incense.', 'Сосед шлёт монахов: оба царства построят святыню на границе и разделят её фимиам.', '隣国から僧が来て願う——界山に寺院を共に建て香火を分かち合い両境で同じ恩恵を。', 'Der Nachbar schickt Mönche: beide Reiche errichten am Grenzberg ein Heiligtum und teilen den Weihrauch.'),
 q('同修梵寺', '同修梵寺', 'Build together', 'Строить вместе', '寺院を共に建つ', 'Gemeinsam bauen'),
 q('出木捐金，共塑金身。', '出木捐金，共塑金身。', 'Give timber and gold; cast the image together.', 'Дать лес и золото; отлить образ сообща.', '材と金を出し金像を共に鋳る。', 'Holz und Gold geben; das Bild gemeinsam gießen.'),
 q('两境僧俗同拜一山，{kingdom}香客络绎。', '兩境僧俗同拜一山，{kingdom}香客絡繹。', 'Monks and laity of both lands bow at one mountain; pilgrims crowd {kingdom}.', 'Монахи и миряне обеих земель кланяются одной горе; паломники толпятся в {kingdom}.', '両国の僧俗が一山を拝み{kingdom}に巡礼が絶えない。', 'Mönche und Laien beider Länder neigen sich vor einem Berg; Pilger füllen {kingdom}.'),
 q('各香各庙', '各香各廟', 'Own shrines', 'Каждому своя святыня', 'それぞれの寺', 'Jeweils eigene Tempel'),
 q('婉言辞谢，各奉其祀。', '婉言辭謝，各奉其祀。', 'Decline politely; each realm keeps its own rites.', 'Вежливо отказаться; у каждой державы свои обряды.', '辞退し各々の祭祀を守る。', 'Höflich ablehnen; jedes Reich hält seine eigenen Riten.'),
 q('界山仍是荒烟，两邦各有香火。', '界山仍是荒煙，兩邦各有香火。', 'The border mountain stays wild; each realm burns its own incense.', 'Гора остаётся дикой; каждая держава жжёт свой ладан.', '界山は荒れたままで両国各々が香を焚く。', 'Der Grenzberg bleibt wild; jedes Reich verbrennt seinen eigenen Weihrauch.')
)
ev('scholar_visit', 2,
 ('学者之访', '學者之訪', 'The Scholars', 'Визит учёных', '学者の来訪', 'Der Gelehrtenbesuch'),
 ('邻国博学之士携书至{kingdom}，欲与宿儒共治经义历数，馆阁瞩目。', '鄰國博學之士攜書至{kingdom}，欲與宿儒共治經義曆數，館閣矚目。', 'Learned men bring books, wishing to study classics and calendars with the sages of {kingdom}; the academies take notice.', 'Учёные мужи привозят книги, желая изучать классику и календари с мудрецами {kingdom}; академии настороже.', '隣国の博学の士が{kingdom}へ書を携え来て、碩学と経義や暦を究めたいと願う——館閣が注目する。', 'Gelehrte des Nachbarn bringen Bücher und wollen mit den Weisen von {kingdom} Klassiker und Kalender studieren; die Akademien merken auf.'),
 q('开馆论道', '開館論道', 'Open the hall', 'Открыть залы', '館を開き論じる', 'Die Halle öffnen'),
 q('延入太学，论难疑义。', '延入太學，論難疑義。', 'Welcome them to the academy; debate the hard questions.', 'Принять в академию; спорить о трудных вопросах.', '大学に迎え疑義を論じ合う。', 'In die Akademie aufnehmen und über schwere Fragen disputieren.'),
 q('典籍互赠，{kingdom}文脉再添薪火。', '典籍互贈，{kingdom}文脈再添薪火。', 'Books are exchanged; scholarship of {kingdom} gains fresh fuel.', 'Книги обменяны; учёность {kingdom} получает новое топливо.', '典籍を贈り合い{kingdom}の学問に新たな火が付く。', 'Bücher werden getauscht; die Gelehrsamkeit von {kingdom} erhält neuen Brennstoff.'),
 q('谨守门户', '謹守門戶', 'Guard the gates', 'Беречь свои двери', '門を固く守る', 'Die Tore hüten'),
 q('以国事为辞，婉拒不纳。', '以國事為辭，婉拒不納。', 'Plead state affairs and decline.', 'Сослаться на дела и отказать.', '政務を理由に丁重に断る。', 'Auf Staatsgeschäfte verweisen und ablehnen.'),
 q('学者怅然北归，两邦学问各守其界。', '學者悵然北歸，兩邦學問各守其界。', 'The scholars return north in dismay; the two realms keep their learning apart.', 'Учёные уходят на север в унынии; две державы держат учёность порознь.', '学者が悔しげに北へ帰り両国の学問は別々に残る。', 'Die Gelehrten ziehen betroffen heim; beide Reiche halten ihr Wissen getrennt.')
)
ev('doctor_mission', 2,
 ('医者之使', '醫者之使', 'The Doctors', 'Миссия лекарей', '医師の派遣', 'Die Arztmission'),
 ('邻国疫气方炽，遣医求援，愿悉输珍药，请{king}派员同行。', '鄰國疫氣方熾，遣醫求援，願悉輸珍藥，請{king}派員同行。', 'Plague blazes in the neighbor realm; doctors beg aid and rare drugs, asking {king} to send physicians along.', 'Чума пылает в соседнем царстве; лекари молят о помощи и редких зельях, прося {king} прислать врачей.', '隣国で疫病が猛威——医を遣い珍薬を捧げ、{king}に医官の同行を請う。', 'Die Pest brennt im Nachbarreich; Ärzte flehen um Hilfe und seltene Mittel und bitten {king}, Mediziner zu senden.'),
 q('遣医共救', '遣醫共救', 'Send physicians', 'Послать лекарей', '医師を遣わす', 'Ärzte senden'),
 q('带药出关，救疫万民。', '帶藥出關，救疫萬民。', 'Cross with medicines; save the plague-torn people.', 'Перейти с лекарствами; спасти поражённый народ.', '薬を担いで出関し疫民を救う。', 'Mit Arzneien hinüberziehen und das zerrissene Volk retten.'),
 q('疫气渐退，邻邦感{king}大德，传诵不绝。', '疫氣漸退，鄰邦感{king}大德，傳誦不絕。', 'The plague recedes; the neighbor praises the virtue of {king} far and wide.', 'Чума отступает; сосед повсюду славит добродетель {king}.', '疫病が退き隣国が{king}の大徳を口々に讃える。', 'Die Pest weicht; der Nachbar rühmt die Tugend von {king} allerorten.'),
 q('闭境自保', '閉境自保', 'Close the border', 'Закрыть границу', '境界を固める', 'Die Grenze schließen'),
 q('加严检疫，一医不发。', '加嚴檢疫，一醫不發。', 'Tighten pest checks and send no physician.', 'Ужесточить карантин и не слать лекарей.', '検疫を厳にし医は一人も出さない。', 'Pestkontrollen verschärfen und keinen Arzt senden.'),
 q('疫气被隔，然邻邦怨{kingdom}坐视。', '疫氣被隔，然鄰邦怨{kingdom}坐視。', 'The plague stays out, yet the neighbor resents the inaction of {kingdom}.', 'Чума не вошла, но сосед негодует на бездействие {kingdom}.', '疫病は防げたが隣国が{kingdom}の坐視を怨む。', 'Die Pest bleibt draußen, doch der Nachbar grollt {kingdom} wegen Untätigkeit.')
)
ev('astronomical_mission', 2,
 ('观星之请', '觀星之請', 'The Astronomers', 'Астрономы', '天文の要請', 'Die Astronomen'),
 ('邻国请{king}准其天官登{kingdom}灵台，合参星象、共订新历。', '鄰國請{king}准其天官登{kingdom}靈臺，合參星象、共訂新曆。', 'The neighbor asks {king} to let its astronomers ascend the observatory of the realm and co-write a new calendar.', 'Сосед просит {king} пустить астрономов на обсерваторию державы для совместного календаря.', '隣国が{king}に願う——天文官を{kingdom}の霊台に登らせ、共に星を観て新暦を定めたい。', 'Der Nachbar bittet {king}, Astronomen zur Sternwarte des Reiches zu lassen, um gemeinsam einen neuen Kalender zu schreiben.'),
 q('共观天象', '共觀天象', 'Watch together', 'Наблюдать вместе', '共に星を観る', 'Gemeinsam beobachten'),
 q('灵台并席，参合历法。', '靈臺並席，參合曆法。', 'Share the platform and merge the calendars.', 'Делить площадку и слить календари.', '霊台を共にし暦を合わせる。', 'Plattform teilen und die Kalender vereinen.'),
 q('新历既成，两邦历官皆称其精。', '新曆既成，兩邦曆官皆稱其精。', 'The new calendar is done; both realms deem it precise.', 'Новый календарь готов; обе державы чтут его точность.', '新暦が完成し両国の暦官が精妙と讃える。', 'Der neue Kalender vollendet sich; beide Reiche preisen seine Genauigkeit.'),
 q('秘藏天机', '祕藏天機', 'Keep the heavens', 'Беречь небо', '天の機を秘す', 'Den Himmel hüten'),
 q('星象乃国之秘，婉言谢绝。', '星象乃國之祕，婉言謝絕。', 'Heavens are state secrets; decline politely.', 'Небо — тайна державы; вежливо отказать.', '星は国の機密——丁重に断る。', 'Der Himmel ist Staatsgeheimnis; höflich ablehnen.'),
 q('使者抱憾而归，两邦各守其历。', '使者抱憾而歸，兩邦各守其曆。', 'The envoy returns regretfully; each realm keeps its own calendar.', 'Посланник уходит с сожалением; каждая держава при своём календаре.', '使者が残念がって帰り両国各々の暦を守る。', 'Der Gesandte kehrt bedauernd heim; jedes Reich hält seinen Kalender.')
)

# ===== 军事与和平 =====
ev('military_pact', 2,
 ('军事同盟', '軍事同盟', 'The Military Pact', 'Военный пакт', '軍事同盟', 'Der Militärpakt'),
 ('邻国受强敌环伺，愿与{kingdom}订攻守同盟，共担军费，使者待命。', '鄰國受強敵環伺，願與{kingdom}訂攻守同盟，共擔軍費，使者待命。', 'Surrounded by enemies, the neighbor offers {kingdom} a defensive-offensive pact with shared military costs; envoys await.', 'В окружении врагов сосед предлагает {kingdom} оборонительно-наступательный пакт с общими расходами; послы ждут.', '敵に囲まれた隣国が{kingdom}に攻守同盟を申し出る——軍費も分かち合う。使者は待機する。', 'Von Feinden umringt bietet der Nachbar {kingdom} einen Offensiv-Defensiv-Pakt mit geteilten Kosten; Gesandte warten.'),
 q('歃血为盟', '歃血為盟', 'Bind by oath', 'Скрепить клятвой', '血盟を結ぶ', 'Im Eid verbinden'),
 q('立约互援，同御外侮。', '立約互援，同禦外侮。', 'Pledge mutual aid against common foes.', 'Поклясться о взаимной помощи против общих врагов.', '相互救援の約を立て外敵を共に防ぐ。', 'Gegenseitige Hilfe gegen gemeinsame Feinde geloben.'),
 q('两邦同仇，列国侧目，{kingdom}声势为之一振。', '兩邦同仇，列國側目，{kingdom}聲勢為之一振。', 'The realms share one foe; nations take notice and the standing of {kingdom} leaps.', 'Державы близки; страны замечают, вес {kingdom} взлетает.', '両国が敵を同じくし各国が注目、{kingdom}の声望が上がる。', 'Die Reiche teilen einen Feind; Nationen merken auf, das Ansehen von {kingdom} springt.'),
 q('壁上旁观', '壁上旁觀', 'Stand aside', 'Смотреть со стороны', '旁観する', 'Zur Seite stehen'),
 q('托词难从，坐观成败。', '託詞難從，坐觀成敗。', 'Plead difficulty and watch the outcome.', 'Сослаться на трудности и смотреть исход.', '辞しておいて勝敗を見物する。', 'Auf Schwierigkeiten verweisen und den Ausgang beobachten.'),
 q('邻国独力难支，深怨{kingdom}坐视。', '鄰國獨力難支，深怨{kingdom}坐視。', 'Barely holding on, the neighbor resents the inaction of {kingdom}.', 'Еле сдерживаясь, сосед глубоко негодует на бездействие {kingdom}.', '隣国は独力で支え切れず{kingdom}の坐視を深く怨む。', 'Kaum standhaltend, grollt der Nachbar zutiefst der Untätigkeit von {kingdom}.')
)
ev('naval_truce', 2,
 ('海战休兵', '海戰休兵', 'The Naval Truce', 'Морское перемирие', '海戦の中休み', 'Der Seekriegs-Waffenstillstand'),
 ('两邦水师远海相持，冲突频发，邻国请约定期停战，各安其航。', '兩邦水師遠海相持，衝突頻發，鄰國請約定期停戰，各安其航。', 'Fleets of both realms hold off in far seas and clashes repeat; the neighbor asks for a fixed truce so each sails safe.', 'Флоты обеих держав стоят в дальних морях, стычки повторяются; сосед просит перемирие, чтобы каждый плавал спокойно.', '両国の水軍が遠海でにらみ合い衝突が続く——隣国が定期休戦を請い、互いの航行を守る。', 'Flotten beider Reiche lauern in fernen Meeren, Gefechte wiederholen sich; der Nachbar bittet um festen Waffenstillstand.'),
 q('如约休战', '如約休戰', 'Keep the truce', 'Соблюдать перемирие', '約定通り休戦', 'Den Waffenstillstand halten'),
 q('限定期限，各自撤回。', '限定期限，各自撤回。', 'Fix the term and pull back both sides.', 'Установить срок и отвести обе стороны.', '期限を定め互いに引き上げる。', 'Die Frist festlegen und beide Seiten abziehen.'),
 q('海波暂平，商船往来如故。', '海波暫平，商船往來如故。', 'The waves settle; merchant ships ply as before.', 'Волны утихают; торговые суда курсируют как прежде.', '海が凪ぎ商船が従来通り往き交う。', 'Die Wellen beruhigen sich; Handelsschiffe verkehren wie zuvor.'),
 q('阳奉阴违', '陽奉陰違', 'Feigned compliance', 'Притворное согласие', '表裏ある対応', 'Gleichgültiges Heucheln'),
 q('明允而私遣快船偷袭。', '明允而私遣快船偷襲。', 'Agree openly, then raid in secret with fast ships.', 'Согласиться открыто, но тайно напасть на быстрых судах.', '表向きは応じつつ高速船で夜襲する。', 'Öffentlich zustimmen, dann heimlich mit schnellen Schiffen zuschlagen.'),
 q('邻军察觉，暗怒之下更防{kingdom}。', '鄰軍察覺，暗怒之下更防{kingdom}。', 'The neighbor senses it and grows wary of {kingdom} in secret rage.', 'Сосед это чувствует и втайне злится на {kingdom}.', '隣国が察知し怒りを潜めて{kingdom}を警戒する。', 'Der Nachbar merkt es und grollt {kingdom} im Stillen.')
)
ev('garrison_border', 2,
 ('边戍之议', '邊戍之議', 'The Border Garrison', 'Пограничный гарнизон', '辺境の戍兵', 'Die Grenzgarnison'),
 ('边境盗匪出没，邻国请与{kingdom}共置戍军、同巡两境，军饷不菲。', '邊境盜匪出沒，鄰國請與{kingdom}共置戍軍、同巡兩境，軍餉不菲。', 'Bandits prowl the border; the neighbor asks {kingdom} to raise a joint garrison and patrol both sides, though pay runs high.', 'Разбойники рыщут у границы; сосед просит {kingdom} поднять совместный гарнизон и патрулировать обе стороны, но жалованье недешёво.', '辺境に盗賊が出没——隣国が{kingdom}と共に戍兵を置き両境を巡らせたいと願うが兵糧は馬鹿にならない。', 'Banditen streifen an der Grenze; der Nachbar bittet {kingdom} um gemeinsame Garnison und Patrouillen, doch der Sold kostet.'),
 q('共置戍军', '共置戍軍', 'Joint garrison', 'Общий гарнизон', '戍兵を共に置く', 'Gemeinsame Garnison'),
 q('拨饷设戍，双境同巡。', '撥餉設戍，雙境同巡。', 'Fund the garrison and patrol both borders.', 'Финансировать гарнизон и патрулировать обе границы.', '兵糧を出し両境を共に巡る。', 'Die Garnison finanzieren und beide Grenzen patrouillieren.'),
 q('盗匪敛迹，唯边军之饷使金库稍空。', '盜匪斂跡，唯邊軍之餉使金庫稍空。', 'Bandits vanish; only the pay of the soldiers thins the treasury.', 'Разбойники исчезли; лишь жалованье солдат опустошает казну.', '盗賊が影を潜めるが辺軍の兵糧で国庫が少し細る。', 'Banditen verschwinden; nur der Sold der Soldaten dünnt die Kasse.'),
 q('各守其境', '各守其境', 'Each guards its own', 'Каждый у себя', '各々に守る', 'Jeder bewacht sein Revier'),
 q('不立共约，各安疆界。', '不立共約，各安疆界。', 'No joint pact; watch your own line.', 'Без общего пакта; стеречь свою черту.', '共同の約を結ばず各々の境を固める。', 'Keinen gemeinsamen Pakt; jede Seite bewacht ihre Linie.'),
 q('盗贼逸走于两境之交，终为边患。', '盜賊逸走於兩境之交，終為邊患。', 'Bandits slip through the seams and stay a border curse.', 'Разбойники проскальзывают в щели и остаются проклятием границы.', '盗賊が境目をすり抜けいつしか辺患となる。', 'Banditen schlüpfen durch die Lücken und bleiben ein Grenzfluch.')
)
ev('diplomat_bride', 2,
 ('使聘之姻', '使聘之姻', 'The Diplomatic Bride', 'Династический брак', '使聘の婚姻', 'Die Diplomatenbraut'),
 ('邻国公卿遣使来聘，愿以名门之女嫁{king}，以结两邦，朝野侧目。', '鄰國公卿遣使來聘，願以名門之女嫁{king}，以結兩邦，朝野側目。', 'A minister of the neighbor sends a betrothal: a noble daughter for {king} to bind the realms, while all watch.', 'Сановник соседа шлёт сватовство: знатная дочь для {king}, связать державы, и все смотрят.', '隣国の公卿が{king}に名門の娘を嫁がせたいと使者を遣わす——両国を結ぶために、朝野が注目する。', 'Ein Minister des Nachbarn wirbt um die Hand einer Edlen für {king}, um die Reiche zu binden; alle schauen zu.'),
 q('纳聘成姻', '納聘成姻', 'Accept the match', 'Принять брак', '聘礼を納れて縁を結ぶ', 'Die Heirat annehmen'),
 q('册立妃位，两国同庆。', '冊立妃位，兩國同慶。', 'Raise her to consort rank; both realms rejoice.', 'Возвести в супруги; обе державы ликуют.', '妃に立て両国が共に祝う。', 'Zur Gemahlin erheben; beide Reiche jubeln.'),
 q('椒房之喜，两邦之好自此胶固。', '椒房之喜，兩邦之好自此膠固。', 'Wedding joy fills the palace; friendship of the realms turns firm.', 'Свадебная радость; дружба держав становится прочной.', '婚礼の喜びで両国の親交が固まった。', 'Hochzeitsfreude erfüllt das Reich; die Freundschaft der Reiche festigt sich.'),
 q('却聘立威', '卻聘立威', 'Refuse & assert', 'Отказать и поднять вес', '辞して威を示す', 'Ablehnen & behaupten'),
 q('以门第相讥，当面却聘。', '以門第相譏，當面卻聘。', 'Sneer at their station and refuse to their face.', 'Насмехаться над их родом и отказать в лицо.', '家柄を嘲笑い面と向かって断る。', 'Ihren Stand verspotten und vor Ort ablehnen.'),
 q('使臣拂袖，朝野议论纷纷，民心渐摇。', '使臣拂袖，朝野議論紛紛，民心动漸搖。', 'The envoy stalks off; court and commons argue, and hearts waver.', 'Посланник уходит; двор и народ спорят, сердца колеблются.', '使者が怒って去り朝野が噂し民心が揺らぐ。', 'Der Gesandte rauscht davon; Hof und Volk zanken, die Herzen wanken.')
)
ev('insult_word', 2,
 ('使节失言', '使節失言', 'The Insulting Word', 'Обидное слово', '使者の失言', 'Das beleidigende Wort'),
 ('{kingdom}使臣于邻国宴上讥其君王，失礼之言已成口实，邻邦索问。', '{kingdom}使臣於鄰國宴上譏其君王，失禮之言已成口實，鄰邦索問。', 'An envoy of {kingdom} mocked the neighbor king at a banquet; the rude word is now a grievance and the neighbor demands answers.', 'Посланник {kingdom} насмеялся над соседним королём на пиру; грубое слово стало обидой, сосед требует ответа.', '{kingdom}の使臣が隣国の王を宴席で嘲った——失礼な言葉が口実となり、隣国が説明を求める。', 'Ein Gesandter von {kingdom} verspottete den Nachbarkönig beim Bankett; das grobe Wort ist nun ein Anlass, der Nachbar fordert Antwort.'),
 q('抵死不认', '抵死不認', 'Deny it all', 'Всё отрицать', '否認し通す', 'Alles leugnen'),
 q('言为醉语，拒而不问。', '言為醉語，拒而不問。', 'Call it drunken talk; refuse to inquire.', 'Назвать пьяным бредом; отказаться разбираться.', '醉言とし取り合わない。', 'Betrunkene Worte nennen und nicht nachforschen.'),
 q('邻邦深衔之，列国皆评{kingdom}失礼。', '鄰邦深銜之，列國皆評{kingdom}失禮。', 'The neighbor harbors the grudge; nations all judge {kingdom} rude.', 'Сосед затаил обиду; все страны судят {kingdom} неучтивой.', '隣国が恨みを抱え各国が{kingdom}を無礼と評する。', 'Der Nachbar hegt den Groll; alle Nationen halten {kingdom} für unhöflich.'),
 q('遣使谢罪', '遣使謝罪', 'Send apology', 'Слать извинения', '謝罪の使者', 'Entschuldigung senden'),
 q('重责使臣，国书致歉。', '重責使臣，國書致歉。', 'Punish the envoy; apologize in the state letter.', 'Наказать посланника; извиниться в грамоте.', '使臣を重く責め国書で謝罪する。', 'Den Gesandten strafen; im Staatsbrief um Entschuldigung bitten.'),
 q('邻邦释怀，笑言尽销前嫌。', '鄰邦釋懷，笑言盡銷前嫌。', 'The neighbor relents; a laugh dissolves the old grievance.', 'Сосед смягчился; смех растворяет старую обиду.', '隣国が懐を解き笑顔が恨みを溶かす。', 'Der Nachbar gibt nach; ein Lachen löst den alten Groll.')
)
ev('gesture_of_peace', 2,
 ('修好之姿', '修好之姿', 'The Gesture of Peace', 'Жест мира', '修好の姿勢', 'Geste des Friedens'),
 ('邻国新君初立，遣使来献白璧，愿释多年宿怨，重开国书。', '鄰國新君初立，遣使來獻白璧，願釋多年宿怨，重開國書。', 'A new sovereign on the neighbor throne sends white jade, wishing to end years of feud and reopen the correspondence.', 'Новый государь соседа шлёт белую яшму, желая окончить годы вражды и возобновить переписку.', '隣国の新君が白璧を献じ——多年の遺恨を解き国書の往来を再開したいと願う。', 'Ein neuer Souverän im Nachbarthron sendet weiße Jade und will Jahre der Fehde beenden; die Korrespondenz neu beginnen.'),
 q('解怨修好', '解怨修好', 'Make peace', 'Заключить мир', '怨恨を解く', 'Frieden schließen'),
 q('纳璧释怨，重开和好。', '納璧釋怨，重開和好。', 'Accept the jade, renounce the feud, restore goodwill.', 'Принять яшму, отринуть вражду, вернуть добрую волю.', '璧を受け怨恨を捨て和好を再開する。', 'Die Jade annehmen, die Fehde aufgeben, Wohlwollen wiederherstellen.'),
 q('两邦罢兵言欢，{king}之名传于四邻。', '兩邦罷兵言歡，{king}之名傳於四鄰。', 'The realms lay down arms; the name of {king} rings through the neighbors.', 'Державы слагают оружие; имя {king} гремит у соседей.', '両国が兵を収め{king}の名が四隣に響く。', 'Die Reiche legen die Waffen nieder; der Name von {king} hallt durch die Nachbarschaft.'),
 q('虚与委蛇', '虛與委蛇', 'Play along', 'Подыграть', '当たり障りなく対応', 'Mitspielen'),
 q('受璧而不答，且观诚意。', '受璧而不答，且觀誠意。', 'Take the jade, give no answer; watch for sincerity.', 'Взять яшму без ответа; смотреть на искренность.', '璧を受けつつ答えず誠意を見る。', 'Die Jade annehmen, nicht antworten; auf Aufrichtigkeit achten.'),
 q('使者空候，新君疑{king}心志不坚。', '使者空候，新君疑{king}心志不堅。', 'The envoy waits in vain; the new king doubts the will of {king}.', 'Посланник ждёт впустую; новый государь сомневается в воле {king}.', '使者が待たされ新君が{king}の意志を疑う。', 'Der Gesandte wartet umsonst; der neue König zweifelt am Willen von {king}.')
)
ev('tax_for_trade', 2,
 ('税易之策', '稅易之策', 'Tax for Trade', 'Налог за торговлю', '税と交易', 'Steuer gegen Handel'),
 ('邻国请重订商税，薄征稳市则岁入反增，重取则民贫商散。', '鄰國請重訂商稅，薄徵穩市則歲入反增，重取則民貧商散。', 'The neighbor asks to revise the trade tax: light tariffs steady the market and even raise income; heavy ones drain it.', 'Сосед просит пересмотреть торговый налог: лёгкие пошлины стабильны и даже доходнее; тяжёлые — разоряют.', '隣国が商税の見直しを求める——薄い課税は市場を安定させ歳入も増え、重い課税は民を貧しく商を散らす。', 'Der Nachbar bittet um Neuordnung der Handelssteuer: milde Zölle stabilisieren den Markt und heben die Einkünfte; schwere plündern ihn.'),
 q('薄征通商', '薄徵通商', 'Light tariffs', 'Лёгкие пошлины', '薄税で通商', 'Leichte Zölle'),
 q('降关税以畅商流，岁入可期。', '降關稅以暢商流，歲入可期。', 'Lower tariffs to widen trade; income may follow.', 'Снизить пошлины, расширив торговлю; доход последует.', '関税を下げ交易を広げ歳入を見込む。', 'Zölle senken, Handel weiten; Einkünfte dürften folgen.'),
 q('商旅云集，税入反丰，{kingdom}市面一新。', '商旅雲集，稅入反豐，{kingdom}市面一新。', 'Caravans gather; taxes swell and the markets of {kingdom} shine anew.', 'Караваны стекаются; налоги растут, рынки {kingdom} сияют заново.', '隊商が集まり税収が増え{kingdom}の市が一新する。', 'Karawanen kommen; Steuern quellen, die Märkte von {kingdom} glänzen neu.'),
 q('重税自固', '重稅自固', 'Heavy tolls', 'Тяжёлые пошлины', '重税で固める', 'Schwere Zölle'),
 q('加征商税，以充府库。', '加徵商稅，以充府庫。', 'Raise the levies to fill the treasury.', 'Поднять поборы, наполняя казну.', '商税を増やし府庫を満たす。', 'Abgaben erhöhen, um die Kasse zu füllen.'),
 q('商旅绕道，税源反枯，邻国亦怨。', '商旅繞道，稅源反枯，鄰國亦怨。', 'Caravans detour; the tax source dries up and the neighbor resents it.', 'Караваны идут в обход; источник налогов мелеет, сосед негодует.', '隊商が迂回し税源が枯れ隣国の怨みも買う。', 'Karawanen umgehen; die Steuerquelle versiegt, der Nachbar grollt.')
)
ev('royal_letter', 2,
 ('王室之书', '王室之書', 'The Royal Letter', 'Королевское письмо', '王室の書簡', 'Der königliche Brief'),
 ('邻邦国王亲笔致书{king}，问安并赠海东青一羽，礼意殷殷。', '鄰邦國王親筆致書{king}，問安並贈海東青一羽，禮意殷殷。', 'The neighbor king writes {king} in his own hand, with greetings and a gyrfalcon as gift, in a warm style.', 'Соседний король пишет {king} собственной рукой с приветом и кречетом в дар, в душевном тоне.', '隣国の王が{king}に直筆の書を送る——安否を問い海東青一羽を添えて、礼意は厚い。', 'Der Nachbarkönig schreibt {king} in eigener Hand, mit Grüßen und einem Gerfalken als Gabe, in warmem Ton.'),
 q('复书答礼', '復書答禮', 'Reply in kind', 'Ответить с дарами', '返書と返礼', 'Mit Brief & Gaben antworten'),
 q('亲笔复书，另赠貂裘。', '親筆復書，另贈貂裘。', 'Write back by hand, with sable robes besides.', 'Ответить собственной рукой, приложив собольи шубы.', '自筆で返書し貂裘を添える。', 'In eigener Hand antworten und Zobelpelze beilegen.'),
 q('书来书往，两王折节相知。', '書來書往，兩王折節相知。', 'Letters flow both ways; the two kings come to know each other.', 'Письма текут в обе стороны; короли узнают друг друга.', '書簡が行き交い両王が親しくなる。', 'Briefe fließen beidseitig; die Könige lernen einander kennen.'),
 q('搁置不答', '擱置不答', 'Leave unanswered', 'Оставить без ответа', '返さずに置く', 'Unbeantwortet lassen'),
 q('案头积尘，只字不覆。', '案頭積塵，隻字不覆。', 'Let it gather dust; not a word in reply.', 'Дать пылиться; ни слова в ответ.', '机上に積もらせ一言も返さない。', 'Staub sammeln lassen; kein Wort der Antwort.'),
 q('邻君虽不悦，亦未形于色。', '鄰君雖不悅，亦未形於色。', 'The king is displeased, though he shows no color.', 'Король недоволен, хоть и не подаёт виду.', '王は不快でも顔には出さない。', 'Der König ist ungehalten, zeigt es aber nicht.')
)
ev('currency_pact', 2,
 ('币制之盟', '幣制之盟', 'The Currency Pact', 'Валютный пакт', '通貨の盟約', 'Der Währungspakt'),
 ('两邦铸币成色不一，市易折算太繁，邻国请定同制，商民翘首。', '兩邦鑄幣成色不一，市易折算太繁，鄰國請定同制，商民翹首。', 'Coins of the two realms differ in fineness and exchange ruins trade; the neighbor proposes one standard, merchants hope.', 'Монеты двух держав разной пробы, обмен губит торговлю; сосед предлагает единый стандарт, купцы надеются.', '両国の貨幣の品位が違い両替が煩雑——隣国が同一の制度を望み、商民は首を長くする。', 'Münzen der Reiche unterscheiden sich im Feingehalt, Umtausch ruiniert Handel; der Nachbar schlägt einen Standard vor, Händler hoffen.'),
 q('共立币制', '共立幣制', 'Unify the coin', 'Единая монета', '通貨を統一', 'Die Münze vereinen'),
 q('同炉铸币，照用通兑。', '同爐鑄幣，照用通兌。', 'Cast at one furnace; honor mutual exchange.', 'Чеканить в одной печи; признавать обоюдный обмен.', '同じ炉で鋳て通用を通す。', 'An einem Ofen gießen; gegenseitigen Kurs anerkennen.'),
 q('钱法一统，两邦市易无阻。', '錢法一統，兩邦市易無阻。', 'One coinage; trade between the realms flows without hindrance.', 'Единая монета; торговля держав течёт беспрепятственно.', '貨幣が統一され両国間の売買が滞りなくなる。', 'Eine Münze; Handel zwischen den Reichen fließt ohne Hindernis.'),
 q('各守钱法', '各守錢法', 'Keep own coins', 'Оставить свои монеты', '各々の銭法', 'Eigene Münzen behalten'),
 q('不更旧制，折算如故。', '不更舊制，折算如故。', 'Keep the old rules; conversion as before.', 'Оставить старые правила; обмен как прежде.', '旧制を改めず換算は従来通り。', 'Alte Regeln behalten; Umrechnung wie eh und je.'),
 q('商民仍苦折算，然两邦相安无事。', '商民仍苦折算，然兩邦相安無事。', 'Traders still suffer the conversion, yet the realms dwell in peace.', 'Торговцы так и мучаются обменом, но державы живут в мире.', '両替に苦しむ民はいまだだが両国は無事にすむ。', 'Händler leiden weiter unter Umrechnung, aber die Reiche leben in Frieden.')
)
ev('map_dispute', 2,
 ('舆图之争', '輿圖之爭', 'The Map Dispute', 'Спор о картах', '地図の紛争', 'Der Kartenstreit'),
 ('两国舆图界划互歧，边境牧人屡以越界互讼，官府不胜其扰。', '兩國輿圖界劃互歧，邊境牧人屢以越界互訟，官府不勝其擾。', 'The two maps mark the line differently; border herdsmen sue each other over crossings and offices grow weary.', 'Две карты межат линию по-разному; пограничные пастухи судятся из-за переходов, канцелярии устали.', '両国の地図の境が食い違い、辺境の牧民が越境で互いに訴え合い役所が疲れ果てる。', 'Die Karten ziehen die Linie verschieden; Grenzhirten verklagen einander wegen Übertritte, die Ämter ermüden.'),
 q('强执己图', '強執己圖', 'Hold the old map', 'Держать свою карту', '自国の図を強く引き', 'Auf der Karte beharren'),
 q('不允勘界，径行旧图。', '不允勘界，徑行舊圖。', 'Refuse survey; stick to the old chart.', 'Отказать съёмке; держаться старой карты.', '測量を許さず旧図のまま行う。', 'Vermessung verweigern; alte Karte behalten.'),
 q('疆界悬而未决，边讼累积如山。', '疆界懸而未決，邊訟累積如山。', 'The line stays unsettled; border suits pile like mountains.', 'Линия не решена; пограничные тяжбы громоздятся.', '境界は未定のまま辺境の訴訟が山積する。', 'Die Linie bleibt strittig; Grenzprozesse türmen sich.'),
 q('会勘定界', '會勘定界', 'Survey together', 'Совместная съёмка', '会勘して定める', 'Gemeinsam vermessen'),
 q('两界官会勘，重刻界石。', '兩界官會勘，重刻界石。', 'Officials of both sides survey and recut the stones.', 'Чиновники обеих сторон снимают и переставляют камни.', '両国の官が実測し界石を彫り直す。', 'Beamte beider Seiten vermessen und behauen die Steine neu.'),
 q('界石新立，牧民各归牧区。', '界石新立，牧民各歸牧區。', 'New stones set; the herdsmen keep to their pastures.', 'Новые камни поставлены; пастухи держатся своих пастбищ.', '新しい界石が立ち牧民が各々の牧地に戻る。', 'Neue Steine gesetzt; die Hirten bleiben bei ihren Weiden.')
)
ev('goodwill_feast', 2,
 ('万邦会宴', '萬邦會宴', 'The Grand Feast', 'Великий пир', '万邦の宴', 'Das Völkerfest'),
 ('列国使节云集{kingdom}，{king}欲张盛宴以联诸邦之谊，庖厨待命。', '列國使節雲集{kingdom}，{king}欲張盛宴以聯諸邦之誼，庖廚待命。', 'Envoys of many nations gather in {kingdom}; {king} would hold a grand feast to bind them in friendship, kitchens wait.', 'Посланники многих стран собрались в {kingdom}; {king} хочет устроить пир, связав их дружбой, кухни ждут.', '各国の使節が{kingdom}に集う——{king}は大宴を張り諸国の親交を結ぼうとし、厨房が待機する。', 'Gesandte vieler Nationen versammeln sich in {kingdom}; {king} will ein großes Fest, das sie freundschaftlich bindet, die Küchen warten.'),
 q('张宴授礼', '張宴授禮', 'Feast & gift', 'Пир и дары', '宴と贈物', 'Fest & Gaben'),
 q('四海之味毕陈，厚赠来使。', '四海之味畢陳，厚贈來使。', 'Set out delicacies of the world; give rich presents.', 'Выставить яства мира; одарить посланников.', '四方の味を並べ使者に厚く贈る。', 'Feinheiten der Welt auftischen; Gesandte reich beschenken.'),
 q('庖厨费钜，然列国使节欢然而归。', '庖廚費鉅，然列國使節歡然而歸。', 'The kitchens cost dear, yet envoys return happy and the realm gains grace.', 'Кухни дороги, но посланники вернулись довольны, а держава обрела милость.', '費用は嵩んだが使節たちは悦んで帰り、好誼が集まる。', 'Die Küche kostet teuer, doch die Gesandten kehren froh; das Reich gewinnt Gnade.'),
 q('简席自奉', '簡席自奉', 'Frugal table', 'Скромный стол', '質素な膳', 'Bescheidene Tafel'),
 q('水酒一席，礼节如仪。', '水酒一席，禮節如儀。', 'One frugal table, rites kept to the letter.', 'Один скромный стол, обряды по букве.', '粗酒一席、礼は式の通りに。', 'Ein bescheidener Tisch, die Riten nach Vorschrift.'),
 q('使节礼节尽备，唯情谊稍淡。', '使節禮節盡備，唯情誼稍淡。', 'Rites are complete; only the warmth runs thinner.', 'Обряды полны; лишь теплота тоньше.', '礼は行き届きつつ情誼は少し薄い。', 'Die Riten sind vollständig; nur die Wärme dünnt aus.')
)
ev('embassy_rotation', 2,
 ('使馆轮迁', '使館輪遷', 'Embassy Rotation', 'Ротация посольств', '使館の輪番', 'Botschafts-Rotation'),
 ('列国使馆久驻一城，渐成耳目；{king}欲定期轮迁，以杜其弊。', '列國使館久駐一城，漸成耳目；{king}欲定期輪遷，以杜其弊。', 'Embassies long fixed in one place grow into watchposts; {king} would rotate them by term to curb the mischief.', 'Посольства, застывшие в одном месте, превращаются в наблюдательные посты; {king} хочет их ротировать.', '各国の使館が長く一ヶ所に留まり耳目となる——{king}が定期輪番でその弊を断とうとする。', 'Botschaften, lange fest an einem Ort, werden zu Wachtposten; {king} will sie turnusmäßig rotieren, um dem Übel zu wehren.'),
 q('定期轮迁', '定期輪遷', 'Set the rotation', 'Назначить ротацию', '定期的に輪番', 'Die Rotation festlegen'),
 q('限定年限，依期更替。', '限定年限，依期更替。', 'Fix the terms and rotate on schedule.', 'Установить сроки и ротировать по графику.', '年限を定め期に応じ入れ替える。', 'Laufzeiten festlegen und turnusgemäß wechseln.'),
 q('耳目之弊稍减，使馆往来如常。', '耳目之弊稍減，使館往來如常。', 'Watchpost mischief thins; embassy traffic runs as usual.', 'Наблюдательное зло редеет; посольские сношения идут как обычно.', '耳目の弊が薄れ使館の往来は従来通り。', 'Der Spähunsug nimmt ab; der Gesandtenverkehr läuft wie gewohnt.'),
 q('因循旧例', '因循舊例', 'Keep old ways', 'По-старому', '旧例に従う', 'Beim Alten bleiben'),
 q('不更其制，各使安居。', '不更其制，各使安居。', 'No change; every envoy stays settled.', 'Без перемен; каждый посланник оседает.', '制度を変えず使者は落ち着く。', 'Nichts ändern; jeder Gesandte bleibt sesshaft.'),
 q('使馆如旧，诸邦耳目益深。', '使館如舊，諸邦耳目益深。', 'Embassies as before; the watchposts of the nations grow deeper.', 'Посольства прежние; наблюдательные посты стран глубже.', '使館は昔のまま各国の耳目が深まる。', 'Botschaften wie zuvor; die Wachtposten der Nationen werden tiefer.')
)
