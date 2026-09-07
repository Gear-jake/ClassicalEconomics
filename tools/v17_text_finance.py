# -*- coding: utf-8 -*-
"""v1.7.0 finance 文案（六语）"""
EVENTS = {}
def ev(eid, opts, title, desc, *texts):
    EVENTS[eid] = (opts, title, desc, list(texts))
def q(*args): return args

# ===== finance 01-14 =====
ev('embezzle_ring', 2,
 ('蛀虫蚀库', '蛀蟲蝕庫', 'The Vault Moths', 'Моль в казне', '庫を蝕む虫', 'Motten in der Schatzkammer'),
 ('户库盘点，揭出挪用集团一角——亏空牵连宗室亲贵，{king}震怒却投鼠忌器。', '戶庫盤點，揭出挪用集團一角——虧空牽連宗室親貴，{king}震怒卻投鼠忌器。', 'The vault count uncovers an embezzling ring—the shortfall touches royal kin, and {king} fumes yet treads softly.', 'Инвентаризация казны вскрывает растратчиков — недостача касается королевской родни, и {king} в ярости, но действует осторожно.', '金庫の棚卸しで横領団の一端が発覚——欠損は宗室の血縁にまで及び、{king}は激怒しつつも慎重を期す。', 'Die Kassenprüfung deckt einen Unterschleif-Ring auf—das Minus reicht bis an die königliche Verwandtschaft, und {king} kocht, tritt aber vorsichtig auf.'),
 q('按下不查', '按下不查', 'Hush it up', 'Замять', '不問に付す', 'Vertuschen'),
 q('以宗室体面为由，暂不追究，来日再议。', '以宗室體面為由，暫不追究，來日再議。', 'For the kin\'s sake, shelve the case for another day.', 'Ради чести родни отложить дело до лучших времён.', '宗室の体面を慮り、いまは追及せず追々再審する。', 'Der Familienehre wegen die Sache auf später verschieben.'),
 q('亏空仍无人认领，蛀虫就此埋伏得更深。', '虧空仍無人認領，蛀蟲就此埋伏得更深。', 'The shortfall stays unclaimed; the moths bore even deeper.', 'Недостача остаётся бесхозной; моль точит всё глубже.', '欠損のまま放置され、虫はさらに深く潜り込んだ。', 'Das Minus bleibt ungeklärt; die Motten fressen sich nur tiefer.'),
 q('彻查追赃', '徹查追贓', 'Probe and recover', 'Расследовать и вернуть', '徹底追及', 'Untersuchen und zurückholen'),
 q('不惜公帑，查办到底，追赃入库。', '不惜公帑，查辦到底，追贓入庫。', 'Spend freely: push the probe, recover the gold.', 'Не жалеть казны: довести расследование и вернуть золото.', '公費を惜しまず、徹底して追及し、着服分を回収する。', 'Nicht sparen: die Ermittlung durchziehen und das Gold zurückholen.'),
 q('蛀虫伏法，然查案所费不赀，{kingdom}金库再失一笔。', '蛀蟲伏法，然查案所費不貲，{kingdom}金庫再失一筆。', 'The moths are caught, but costs ran high—{kingdom}\'s vault loses another sum.', 'Моль поймана, но затраты велики — казна {kingdom} теряет ещё одну сумму.', '虫は捕まったが、捜査費用は膨らみ{kingdom}の金庫はさらに減った。', 'Die Motten sind gefasst, doch die Kosten waren hoch—die Kasse von {kingdom} verliert einen weiteren Betrag.')
)
ev('toll_rebellion', 2,
 ('关卡之乱', '關卡之亂', 'The Toll Mutiny', 'Бунт застав', '関所の騒乱', 'Die Zoll-Rebellion'),
 ('商路关卡林立，过路费层层盘剥，{kingdom}的车夫已现哗变之兆。', '商路關卡林立，過路費層層盤剝，{kingdom}的車夫已現嘩變之兆。', 'Toll posts crowd the roads, each one skinning travelers—across the realm of {kingdom}, carters murmur of mutiny.', 'Заставы на каждом шагу дерут с путников — по землям {kingdom} уже гуляют разговоры о бунте.', '関所が道を覆い、通行料の搾取が続く——{kingdom}の車夫たちに騒乱の兆しが見える。', 'Zollstellen reihen sich an den Straßen, jede schröpft die Reisenden—unter den Fuhrleuten von {kingdom} gärt der Aufruhr.'),
 q('照旧征收', '照舊徵收', 'Keep the tolls', 'Оставить заставы', '関所を維持', 'Die Zölle behalten'),
 q('强硬弹压车夫起事，维持关卡财源。', '強硬彈壓車夫起事，維持關卡財源。', 'Crush the carters\' unrest and keep the toll income.', 'Подавить недовольство возчиков и сохранить доход.', '車夫の不満を強硬に鎮め、関所の収入を守る。', 'Den Aufruhr der Fuhrleute niederschlagen und die Zolleinnahmen halten.'),
 q('弹压激起更多抗拒，关卡周遭几成乱局。', '彈壓激起更多抗拒，關卡周遭幾成亂局。', 'Repression breeds more defiance; the toll posts slide into chaos.', 'Репрессии рождают новое сопротивление; заставы тонут в хаосе.', '弾圧がさらなる反発を生み、関所の周りは騒乱と化した。', 'Die Repression erzeugt mehr Widerstand; die Zollstellen versinken im Chaos.'),
 q('赎买撤卡', '贖買撤卡', 'Buy out the tolls', 'Выкупить заставы', '関所を買い取る', 'Zölle auslösen'),
 q('花金库的钱赎买私卡，商路重开。', '花金庫的錢贖買私卡，商路重開。', 'Spend from the vault to buy out the posts and reopen the roads.', 'Выкупить частные заставы за счёт казны и открыть пути.', '私設関所を金庫の金で買い取り、街道を開き直す。', 'Die Privatposten aus der Kasse auslösen und die Straßen öffnen.'),
 q('重重关卡尽撤，{kingdom}商路畅通，国库为此破费。', '重重關卡盡撤，{kingdom}商路暢通，國庫為此破費。', 'The posts fall; {kingdom}\'s trade roads run free, though the treasury paid dearly.', 'Заставы сняты, пути {kingdom} свободны, но казна изрядно потратилась.', '関所はすべて撤去され、{kingdom}の商路は開けたが、国庫は大いに費えた。', 'Die Posten fallen; die Handelsstraßen von {kingdom} sind frei, doch die Kasse blutet.')
)
ev('debt_auction', 2,
 ('国债开拍', '國債開拍', 'The Bond Auction', 'Аукцион облигаций', '国債の競売', 'Die Anleihe-Auktion'),
 ('国库拮据，商贾献议发行官债——若{king}点头，市面或竞相认购。', '國庫拮据，商賈獻議發行官債——若{king}點頭，市面或競相認購。', 'The vault runs dry; merchants propose a state bond—if {king} nods, the market may scramble to subscribe.', 'Казна пустеет; купцы предлагают государственный заём — если {king} кивнёт, рынок может броситься подписываться.', '国庫は窮し、商人が官債の発行を進言——{king}がうなずけば市場が争って応募するだろう。', 'Die Kasse ist leer; Kaufleute schlagen eine Staatsanleihe vor—nickt {king}, könnte der Markt sich um die Zeichnung reißen.'),
 q('开拍官债', '開拍官債', 'Auction the bonds', 'Продать облигации', '国債を発行', 'Anleihen versteigern'),
 q('广开认购，价高者得，现银入库。', '廣開認購，價高者得，現銀入庫。', 'Open subscriptions wide; the highest bidder wins and silver flows in.', 'Открыть подписку; кто платит больше, тот и берёт.', '応募を広く募り、高値の者に与えて即銀を庫に入れる。', 'Die Zeichnung breit eröffnen; der Höchstbietende gewinnt, Silber fließt hinein.'),
 q('官债一经售罄，{kingdom}金库顿时充盈。', '官債一經售罄，{kingdom}金庫頓時充盈。', 'The bonds sell out; the vault of {kingdom} fills at once.', 'Облигации распроданы; казна {kingdom} сразу наполнилась.', '官債は完売し、{kingdom}の金庫はたちまち満ちた。', 'Die Anleihen sind ausverkauft; die Kasse von {kingdom} füllt sich sogleich.'),
 q('谢绝献议', '謝絕獻議', 'Refuse the advice', 'Отказать совету', '進言を退ける', 'Den Rat ablehnen'),
 q('不愿背新债，宁可守着旧账过日。', '不願背新債，寧可守著舊賬過日。', 'No new debt; better to live within the old accounts.', 'Без новых долгов; жить по старым книгам.', '新たな借金は避け、旧い帳簿の中で日々を送る。', 'Keine neuen Schulden; lieber im Rahmen der alten Bücher leben.'),
 q('献议搁置，商贾议论纷纷，国库仍是旧样。', '獻議擱置，商賈議論紛紛，國庫仍是舊樣。', 'The plan is shelved; merchants murmur, and the vault stays as it was.', 'Предложение отложено; купцы ворчат, казна прежняя.', '進言は棚上げされ、商人たちは噂し合うが国庫は変わりない。', 'Der Plan wird vertagt; die Kaufleute murren, die Kasse bleibt wie sie war.')
)
ev('specie_crisis', 2,
 ('银根骤紧', '銀根驟緊', 'The Specie Squeeze', 'Кризис наличности', '銀根の逼迫', 'Die Silberklemme'),
 ('铜钱成色渐乱，商号拒收，{kingdom}市面银根骤紧，物价惶恐。', '銅錢成色漸亂，商號拒收，{kingdom}市面銀根驟緊，物價惶恐。', 'Coins turn debased and shops refuse them; money tightens in {kingdom} and prices reel.', 'Монеты портятся, лавки отказываются их брать; в {kingdom} дорожают деньги, цены лихорадит.', '銅銭の品位が乱れ、店が受け取りを拒む——{kingdom}の市中は銀根が急に引き締まり、物価が慌てる。', 'Die Münzen verkommen und die Läden lehnen sie ab; in {kingdom} zieht sich das Geld zusammen, die Preise schwindeln.'),
 q('出银平抑', '出銀平抑', 'Release silver', 'Выпустить серебро', '銀を放出', 'Silber freigeben'),
 q('开库放银，收兑铜钱，安定市面。', '開庫放銀，收兌銅錢，安定市面。', 'Open the vault, release silver, redeem coins, settle the market.', 'Открыть казну, выпустить серебро и успокоить рынок.', '国庫の銀を放出し銅銭を引き取って、市況を落ち着かせる。', 'Die Kasse öffnen, Silber freigeben und den Markt beruhigen.'),
 q('市面渐安，然{kingdom}库银为之短少。', '市面漸安，然{kingdom}庫銀為之短少。', 'The market calms, yet {kingdom}\'s silver is thereby reduced.', 'Рынок успокоился, но казённое серебро в {kingdom} поубавилось.', '市況は落ち着いたが、{kingdom}の庫銀はその分減った。', 'Der Markt beruhigt sich, doch das Silber von {kingdom} schwindet dabei.'),
 q('强令流通', '強令流通', 'Force circulation', 'Принудить к хождению', '強制流通', 'Zirkulation erzwingen'),
 q('颁王令强制收受，违者重惩。', '頒王令強制收受，違者重懲。', 'Edict it: all shops must accept the coins, or face harsh penalty.', 'Указом заставить все лавки принимать монеты под страхом кары.', '王令で受け取りを強制し、違反者を重く罰する。', 'Per Edikt die Annahme erzwingen, Zuwiderhandelnde hart strafen.'),
 q('王令难行，商民怨声四起，市面更乱。', '王令難行，商民怨聲四起，市面更亂。', 'The edict fails; merchants and folk rail, and the market grows wilder.', 'Указ не действует; торговцы и народ ропщут, рынок безумствует.', '王令は行き渡らず、商人も民も怨みを口にし、市はますます乱れた。', 'Das Edikt greift nicht; Kaufleute wie Volk murren, der Markt wird wilder.')
)
ev('bank_discount', 2,
 ('钱庄让利', '錢莊讓利', 'The Bank\'s Discount', 'Скидка банка', '銭荘の優遇', 'Der Bankrabatt'),
 ('大钱庄愿以低息向列国商旅放款，只求{king}颁一纸御笔担保。', '大錢莊願以低息向列國商旅放款，只求{king}頒一紙御筆擔保。', 'A great banking house will lend to foreign merchants at low rates—if {king} puts the seal upon a guarantee.', 'Богатый банк готов ссужать иноземных купцов под низкий процент — если {king} скрепит печатью поручительство.', '大銭荘が外国商人への低利融資を申し出る——ただし{king}の御璽ある保証が条件だ。', 'Ein großes Bankhaus leiht ausländischen Händlern zu günstigem Zins—wenn {king} ein Garantieschreiben siegelt.'),
 q('御笔作保', '御筆作保', 'Endorse the bank', 'Поручиться', '保証を与える', 'Bürgschaft leisten'),
 q('官印作保，钱庄让利，商旅称便。', '官印作保，錢莊讓利，商旅稱便。', 'The seal endorses it; the bank rebates, and travelers rejoice.', 'Печать служит порукой; банк уступает, путники довольны.', '官印が保証となり、銭荘は利を譲り、旅商人は喜ぶ。', 'Das Siegel bürgt; das Haus gibt nach, und die Reisenden freuen sich.'),
 q('异地银票畅通，列国对{kingdom}好感大增。', '異地銀票暢通，列國對{kingdom}好感大增。', 'Bills run free across borders; other courts look far more kindly on {kingdom}.', 'Серебряные векселя ходят свободно; другие дворы смотрят на {kingdom} теплее.', '他国で銀票が通用し、列国は{kingdom}に好感を深めた。', 'Wechsel gelten überall; die Höfe blicken wohlwollender auf {kingdom}.'),
 q('婉言推拒', '婉言推拒', 'Decline softly', 'Мягко отказать', '辞退する', 'Höflich ablehnen'),
 q('官家不作保，宁可维持现状。', '官家不作保，寧可維持現狀。', 'No state guarantee; better to keep things as they are.', 'Без поручительства; пусть всё останется как есть.', '官は保証せず、現状を維持する。', 'Keine Staatsbürgschaft; man hält am Bestehenden fest.'),
 q('钱庄大失所望，然也无风雨也无晴。', '錢莊大失所望，然也無風雨也無晴。', 'The house is crestfallen—yet no storm rises, nor any sun.', 'Банк разочарован — но ни бури, ни ясного неба.', '銭荘は落胆したが、嵐も来ず晴れも来ない。', 'Das Haus ist enttäuscht—aber kein Sturm zieht auf, keine Sonne.')
)
ev('salt_law', 2,
 ('榷盐之利', '榷鹽之利', 'The Salt Monopoly', 'Соляная монополия', '塩の専売', 'Das Salzmonopol'),
 ('盐商漏税成风，户部献议行榷盐专卖——听与不听，全在{king}一念。', '鹽商漏稅成風，戶部獻議行榷鹽專賣——聽與不聽，全在{king}一念。', 'Salt merchants dodge the levy; the ministry proposes a salt monopoly—the yes or no rests on {king} alone.', 'Соляные купцы уклоняются от налога; ведомство предлагает соляную монополию — всё решает один кивок {king}.', '塩商人の脱税が横行し、戸部が塩の専売を進言——応じるか否かは{king}の一念にかかっている。', 'Salzhändler umgehen die Steuer; das Ministerium schlägt das Salzmonopol vor—das Ja oder Nein liegt allein bei {king}.'),
 q('行榷盐法', '行榷鹽法', 'Enact the monopoly', 'Ввести монополию', '専売を実施', 'Das Monopol erlassen'),
 q('盐引专卖，民食盐价稍涨。', '鹽引專賣，民食鹽價稍漲。', 'License the salt trade; the folk pay a coin or two more.', 'Соль по патентам; народ платит на монету больше.', '塩引による専売を布き、民の塩価はやや上がる。', 'Das Salz per Lizenz verkaufen; das Volk zahlt zwei Münzen mehr.'),
 q('盐利尽数归公，{kingdom}金库随之大涨。', '鹽利盡數歸公，{kingdom}金庫隨之大漲。', 'The salt profits flow to the crown; the vault of {kingdom} swells.', 'Соляная прибыль идёт короне; казна {kingdom} набухает.', '塩の利はすべて公に入り、{kingdom}の金庫が大いに満ちた。', 'Der Salzgewinn fließt der Krone zu; die Kasse von {kingdom} schwillt an.'),
 q('强征盐税', '強徵鹽稅', 'Force the salt tax', 'Выжать соляной налог', '塩税を強く課す', 'Salzsteuer erzwingen'),
 q('不分老幼皆加税赋，以充国库。', '不分老幼皆加稅賦，以充國庫。', 'Levy the salt tax on every mouth, young and old.', 'Обложить соляным налогом каждого, от мала до велика.', '老若を問わず塩税を課し、国庫を満たす。', 'Die Salzsteuer jedem Mund aufbürden, jung wie alt.'),
 q('盐税激起怨声，灶户盐商暗相抵制。', '鹽稅激起怨聲，灶戶鹽商暗相抵制。', 'The tax stirs wrath; salters and merchants quietly resist.', 'Налог рождает ропот; солевары и купцы тихо саботируют.', '塩税が怨みを呼び、塩田主と商人はひそかに抵抗する。', 'Die Steuer erregt Zorn; Salzwirker und Händler widerstehen still.')
)
ev('liquor_law', 2,
 ('酒禁之争', '酒禁之爭', 'The Liquor Edict', 'Винный указ', '酒の禁令', 'Das Branntwein-Edikt'),
 ('酒坊遍地而税难征，有司献议立《酒法》——是收是禁，静待{king}裁夺。', '酒坊遍地而稅難徵，有司獻議立《酒法》——是收是禁，靜待{king}裁奪。', 'Breweries crowd the land yet yield no tax; the ministry proposes a liquor code—levy or ban, awaits {king}.', 'Винокурни повсюду, но налог не собрать; ведомство предлагает винный кодекс — взимать или запрещать, решает {king}.', '酒蔵は至る所にあるのに税が取れない——役所が『酒法』の制定を進言し、課すか禁ずるか{king}の裁断を待つ。', 'Brennereien füllen das Land, bringen aber keine Steuer; das Amt schlägt ein Branntwein-Gesetz vor—besteuern oder verbannen, wartet auf {king}.'),
 q('立榷酒法', '立榷酒法', 'Tax the breweries', 'Обложить винокурни', '酒の専売令', 'Die Brauereien besteuern'),
 q('官酿官卖，酒税抽分入公。', '官釀官賣，酒稅抽分入公。', 'Official brewing and selling; the levy into the public purse.', 'Официальная варка и продажа; налог идёт в казну.', '官の醸造・販売として、酒税を公に組み入れる。', 'Branntwein von Amts wegen brauen und verkaufen; der Steueranteil fließt in die Kasse.'),
 q('酒榷大兴，{kingdom}库帑自此转丰。', '酒榷大興，{kingdom}庫帑自此轉豐。', 'The liquor levy thrives; the coffers of {kingdom} turn rich.', 'Винный сбор процветает; казна {kingdom} богатеет.', '酒の専売が盛んになり、{kingdom}の庫金が豊かになった。', 'Die Steuer gedeiht; die Kassen von {kingdom} werden reich.'),
 q('厉行禁酒', '厲行禁酒', 'Ban the liquor', 'Запретить вино', '禁酒を断行', 'Branntwein verbieten'),
 q('查封坊舍，私酿者从重论处。', '查封坊舍，私釀者從重論處。', 'Seal the breweries; punish home-brewing severely.', 'Закрыть винокурни; домашнее варение карать строго.', '酒蔵を封鎖し、自家醸造を重く罰する。', 'Die Brennereien versiegeln; Heimbrauen streng bestrafen.'),
 q('禁令一出，酒坊聚众哗然，怨声载道。', '禁令一出，酒坊聚眾嘩然，怨聲載道。', 'The ban falls; breweries gather and howl, complaints flood the roads.', 'Запрет пал; винокурни гудят, жалобы текут рекой.', '禁令が出ると酒蔵が集まって騒ぎ、怨嗟の声が道に満ちた。', 'Das Verbot fällt; die Brennereien lärmen, Klagen fluten die Straßen.')
)
ev('iron_law', 2,
 ('铁官之议', '鐵官之議', 'The Iron Debate', 'Железный спор', '鉄の専売論', 'Die Eisendebatte'),
 ('铁器为农战之本，立铁官抑或通铁，{king}一诏可定。', '鐵器為農戰之本，立鐵官抑或通鐵，{king}一詔可定。', 'Iron is the root of farm and war—an iron office or free trade, one edict of {king} decides.', 'Железо — корень земледелия и войны; железное ведомство или свободная торговля — решает один указ {king}.', '鉄器は農と戦の要——鉄官を置くか鉄の流通を許すか、{king}の詔一つで決まる。', 'Eisen ist die Wurzel von Ackerbau und Krieg—Eisenamt oder freier Handel, ein Erlass von {king} entscheidet.'),
 q('立铁官署', '立鐵官署', 'Establish the iron office', 'Учредить железное ведомство', '鉄官署を置く', 'Das Eisenamt errichten'),
 q('民间贾铁归官，私贩者重罚。', '民間賈鐵歸官，私販者重罰。', 'Iron trading channels through the crown; private dealers fined hard.', 'Торговля железом идёт через корону; частников штрафуют.', '民間の鉄売買を官に集め、私人売買を重く罰する。', 'Der Eisenhandel läuft über die Krone; Privatverkäufer schwer bestrafen.'),
 q('铁利入官，{kingdom}国用为之稍纾。', '鐵利入官，{kingdom}國用為之稍紓。', 'Iron profits pass to the crown; the state finances of {kingdom} ease a little.', 'Железная прибыль идёт короне; казна {kingdom} слегка вздыхает.', '鉄の利が官に入り、{kingdom}の国費が少し楽になった。', 'Die Eisengewinne gehen an die Krone; die Staatskasse von {kingdom} atmet auf.'),
 q('许民间通铁', '許民間通鐵', 'Free the iron trade', 'Открыть торговлю железом', '民間に鉄を許す', 'Eisenhandel freigeben'),
 q('商路通铁，惠及列国农战。', '商路通鐵，惠及列國農戰。', 'Open the iron roads; farm and war prosper everywhere.', 'Открыть железные пути; земледелие и война процветают.', '鉄の商路を開き、諸国の農と戦を潤す。', 'Die Eisenstraßen öffnen; Ackerbau und Krieg gedeihen überall.'),
 q('铁器流通四境，列国商贾称颂{kingdom}。', '鐵器流通四境，列國商賈稱頌{kingdom}。', 'Iron flows to every border; merchants praise {kingdom} to the skies.', 'Железо течёт во все края; купцы превозносят {kingdom}.', '鉄器が四方に流れ、他国の商人が{kingdom}を称えた。', 'Eisen fließt bis an alle Grenzen; die Kaufleute rühmen {kingdom}.')
)
ev('currency_ruin', 2,
 ('货币崩坏', '貨幣崩壞', 'Currency Collapse', 'Крах валюты', '通貨の崩壊', 'Währungskollaps'),
 ('劣币驱良币，市面拒用官铸钱，{king}闻报时已积重难返。', '劣幣驅良幣，市面拒用官鑄錢，{king}聞報時已積重難返。', 'Bad coin drives out good; the market refuses the state money, and by the time {king} hears of it the rot has gone deep.', 'Плохая монета вытесняет хорошую; рынок отказывается от государственных денег, и к вести {king} гниль уже въелась глубоко.', '悪貨が良貨を駆逐し、市場が官鋳銭を拒む——{king}が報せを聞いた時にはもう手遅れだった。', 'Schlechtes Geld verdrängt gutes; der Markt verweigert das staatliche Geld, und als {king} es hört, sitzt die Fäulnis tief.'),
 q('金库平抑', '金庫平抑', 'Stabilize with gold', 'Стабилизировать золотом', '金庫で平抑', 'Mit Gold stabilisieren'),
 q('开库抛金，收兑劣币，重铸钱。', '開庫拋金，收兌劣幣，重鑄錢。', 'Throw gold from the vault, call in bad coin, restrike the money.', 'Бросить золото из казны, собрать плохую монету, перечеканить.', '国庫の金を投じ、悪銭を回収して銭を改鋳する。', 'Gold aus der Kasse werfen, schlechtes Geld einziehen, neu prägen.'),
 q('金库白费巨资，钱价仍如惊弓——此事怕未了结。', '金庫白費鉅資，錢價仍如驚弓——此事怕未了結。', 'The vault pours out gold, and prices still startle—this affair, it seems, is far from over.', 'Казна тратит золото, а цены всё вздрагивают — похоже, дело далеко не закрыто.', '金庫が巨費を費やしても銭価はなお竦んだまま——この件はまだ終わった気配がない。', 'Die Kasse verströmt Gold, doch die Preise zucken weiter—diese Sache scheint längst nicht beendet.'),
 q('听其崩坏', '聽其崩壞', 'Let it collapse', 'Пусть рухнет', '崩壊に任せる', 'Zusammenbruch zulassen'),
 q('市价随行就市，官家不加干预。', '市價隨行就市，官家不加干預。', 'Let the market find its own price; the crown will not interfere.', 'Пусть рынок сам найдёт цену; корона не вмешивается.', '相場は相場として、官は干渉しない。', 'Der Markt findet seinen Preis; die Krone greift nicht ein.'),
 q('市井怨声潮涌，钱币之祸恐怕只是序章。', '市井怨聲潮湧，錢幣之禍恐怕只是序章。', 'Lamentation floods the streets—the coin calamity may be a mere prologue.', 'Стоны заливают улицы — беда с монетой, боюсь, лишь пролог.', '街に怨嗟が溢れ——銭貨の災いは序章に過ぎない恐れがある。', 'Klagen fluten die Gassen—das Münzenelend ist womöglich nur ein Vorspiel.')
)
ev('mint_clampdown', 2,
 ('查禁私铸', '查禁私鑄', 'Against Private Mints', 'Против частных дворов', '私鋳の取締り', 'Gegen Privatprägung'),
 ('劣币之源未除，私铸钱炉遍地开花，{king}决意整顿诸炉。', '劣幣之源未除，私鑄錢爐遍地開花，{king}決意整頓諸爐。', 'The wellspring of bad coin is still open—private furnaces bloom everywhere, and {king} is set on cleansing them.', 'Источник плохой монеты не закрыт — частные печи цветут повсюду, и {king} полон решимости всё вычистить.', '悪銭の源は残り、私鋳の炉が各地で咲く——{king}は諸炉の整理を決意する。', 'Die Quelle des schlechten Geldes ist offen—private Öfen blühen überall, und {king} ist entschlossen, sie zu säubern.'),
 q('重法严缉', '重法嚴緝', 'Strike hard', 'Ударить жёстко', '重法で締め上げ', 'Hart durchgreifen'),
 q('私铸者论死，窝主同罪连坐。', '私鑄者論死，窩主同罪連坐。', 'Death for private minting; accomplices bound by the same rope.', 'Смерть за частную чеканку; сообщники разделяют участь.', '私鋳は死罪、庇う者も同罪の連座。', 'Tod für Privatprägung; Helfer teilen dasselbe Schicksal.'),
 q('法刑虽峻，怨尤随之而生，市面暗流涌动。', '法刑雖峻，怨尤隨之而生，市面暗流湧動。', 'The law strikes, and resentment follows; under the surface the market stirs.', 'Закон бьёт, а вместе с ним растёт озлобление; под поверхностью рынок бурлит.', '法は峻烈だが怨みも生まれ、市には暗流がうごめく。', 'Das Gesetz trifft, Groll folgt; unter der Oberfläche gärt der Markt.'),
 q('赎买收炉', '贖買收爐', 'Buy up the furnaces', 'Выкупить печи', '炉を買い取る', 'Die Öfen aufkaufen'),
 q('出钱收编私炉，兼养匠户。', '出錢收編私爐，兼養匠戶。', 'Pay to absorb the private furnaces and feed their artisans.', 'Заплатить за поглощение частных печей и прокормить мастеров.', '私炉を金で買い上げ、職人も扶持する。', 'Die privaten Öfen bezahlen und ihre Handwerker versorgen.'),
 q('私炉渐熄，然{kingdom}为此又掏了一笔。', '私爐漸熄，然{kingdom}為此又掏了一筆。', 'The furnaces die down, but {kingdom} pays another tidy sum.', 'Печи гаснут, но {kingdom} платит ещё одну хорошую сумму.', '私炉は徐々に消えたが、{kingdom}はその分をまた出した。', 'Die Öfen erlöschen, doch {kingdom} zahlt dafür eine weitere Stange Geld.')
)
ev('customs_raise', 2,
 ('关税之争', '關稅之爭', 'The Customs Rise', 'Повышение пошлин', '関税の引き上げ', 'Die Zollanhebung'),
 ('口岸税则积弊已久，户部献议加征——是富国还是伤交，{king}自有掂量。', '口岸稅則積弊已久，戶部獻議加徵——是富國還是傷交，{king}自有掂量。', 'Port tariffs have festered for years; the ministry proposes a rise—to enrich the state or wound its friendships, {king} must weigh.', 'Портовые пошлины гниют годами; ведомство предлагает поднять их — обогатит ли это или ранит союзы, взвесить должен {king}.', '港の税則は積年の弊が深く、戸部が引き上げを進言——国を豊かにするか交誼を傷つけるか、{king}が量るべきだ。', 'Die Hafenzölle faulen seit Jahren; das Ministerium schlägt eine Erhöhung vor—reich machen oder Freundschaften verletzen, das muss {king} abwägen.'),
 q('加征关税', '加徵關稅', 'Raise the duties', 'Поднять пошлины', '関税を引き上げ', 'Die Zölle erhöhen'),
 q('商船过埠，一律按货加税。', '商船過埠，一律按貨加稅。', 'Every ship that berths pays a new levy on its cargo.', 'Каждый корабль платит новый сбор с груза.', '入港船は全て貨物に応じて増税される。', 'Jedes anlegende Schiff zahlt einen neuen Aufschlag auf die Ladung.'),
 q('关税入帑，{kingdom}金库为之一肥。', '關稅入帑，{kingdom}金庫為之一肥。', 'The duties pour in; the vault of {kingdom} grows fat.', 'Пошлины текут в казну; казна {kingdom} толстеет.', '関税が庫に入り、{kingdom}の金庫がひとまわり肥えた。', 'Die Zölle fließen hinein; die Kasse von {kingdom} wird fett.'),
 q('苛索外商', '苛索外商', 'Squeeze the traders', 'Выжать из торговцев', '外商を搾る', 'Die Händler auspressen'),
 q('另设关卡，百般留难洋商。', '另設關卡，百般留難洋商。', 'New checkpoints everywhere, foreign merchants harassed at every turn.', 'Новые заставы всюду; иностранных купцов притесняют на каждом углу.', '関所を新設し、外国商人をあらゆる手で滞らせる。', 'Neue Kontrollen überall, fremde Händler bei jedem Schritt schikaniert.'),
 q('外商怨声载道，列国对{kingdom}顿生鄙薄。', '外商怨聲載道，列國對{kingdom}頓生鄙薄。', 'Traders rail aloud; the nations eye {kingdom} with sudden contempt.', 'Торговцы ропщут громко; державы смотрят на {kingdom} с презрением.', '外国商人の怨嗟は道に溢れ、列国は{kingdom}を急に軽蔑し始めた。', 'Die Händler schreien laut; die Nationen blicken mit Verachtung auf {kingdom}.')
)
ev('treasury_moth', 2,
 ('库蠹为患', '庫蠹為患', 'Moths in the Treasury', 'Моль в казне', '庫の虫の害', 'Motten in der Kasse'),
 ('账实不符，管库蠹吏上下其手，亏空如滚雪——{king}的库房正被一口口吃掉。', '賬實不符，管庫蠹吏上下其手，虧空如滾雪——{king}的庫房正被一口口吃掉。', 'The books never match the stores; custodian clerks pilfer at every hand, the deficit snowballs—{king}\'s vault is being devoured bite by bite.', 'Книги расходятся с кладовыми; смотрители воруют на каждом шагу, недостача растёт как снежный ком — казну {king} пожирают кусок за куском.', '帳簿と実物が合わず、倉庫吏が手を回し、欠損は雪だるま式に膨らむ——{king}の金庫は一口ずつ食われている。', 'Die Bücher stimmen nie mit den Beständen; die Kustoden stehlen bei jeder Gelegenheit, das Defizit rollt an wie Schnee—die Kasse von {king} wird Brocken um Brocken verschlungen.'),
 q('清库查账', '清庫查賬', 'Audit the vaults', 'Ревизовать казну', '庫を監査', 'Die Kassen prüfen'),
 q('聘廉吏严加盘点，追赃补欠。', '聘廉吏嚴加盤點，追贓補欠。', 'Hire honest clerks to count everything and claw the sums back.', 'Нанять честных счетоводов, всё пересчитать и вернуть украденное.', '廉直の吏を雇い厳しく棚卸しし、不正分を回収する。', 'Ehrliche Schreiber anheuern, alles zählen und die Beträge zurückholen.'),
 q('查办既深，耗却公帑不少，亏空仍未填满。', '查辦既深，耗卻公帑不少，虧空仍未填滿。', 'The probe runs deep and the public purse bleeds—yet the hole remains unfilled.', 'Разбор глубокий, а казна кровоточит — дыра так и не заделана.', '追及は深く、公費も多く費えたが、欠損はまだ埋まらない。', 'Die Untersuchung geht tief, das Staatsgeld blutet—doch das Loch bleibt offen.'),
 q('维持旧账', '維持舊賬', 'Keep the books', 'Оставить как есть', '帳簿を据え置き', 'Die Bücher lassen'),
 q('佯作不知，维持旧簿旧案。', '佯作不知，維持舊簿舊案。', 'Feign ignorance and keep the dusty ledgers as they are.', 'Сделать вид, что не знаешь; пыльные книги пусть остаются.', '知らぬふりをし、旧い帳簿のままにしておく。', 'Unwissenheit vortäuschen und die staubigen Bücher so lassen.'),
 q('库蠹愈肥，民间怨声渐起，物议沸腾。', '庫蠹愈肥，民間怨聲漸起，物議沸騰。', 'The moths grow fat; the folk begin to murmur, and gossip boils over.', 'Моль жиреет; народ начинает роптать, пересуды кипят.', '虫はますます肥え、民の怨みが立ち、物議が沸き立つ。', 'Die Motten werden fett; das Volk beginnt zu murren, das Gerede kocht über.')
)
ev('war_debt', 2,
 ('军需之债', '軍需之債', 'The War Debt', 'Военный долг', '戦費の借金', 'Die Kriegsschuld'),
 ('{king}的军需赊欠如山——前方催饷，债主已经堵到宫门。', '{king}的軍需賒欠如山——前方催餉，債主已經堵到宮門。', 'War supplies lie unpaid in mountains—the front cries for wages, creditors crowd the very gates.', 'Военные поставки не оплачены горами — фронт требует жалованья, кредиторы толпятся у самых ворот.', '戦費の立て替えが山のように残り——前線は給与を求め、債主は宮門に押し寄せる。', 'Kriegslieferungen liegen unbezahlt wie Berge—die Front verlangt Sold, Gläubiger drängen sich vor den Toren.'),
 q('如约偿付', '如約償付', 'Honor the debt', 'Заплатить долг', '借りを返す', 'Die Schuld ehren'),
 q('变卖内帑凑钱，如约清账。', '變賣內帑湊錢，如約清賬。', 'Sell the privy fund, scrape it together, settle every account.', 'Распродать личные запасы и закрыть все счета.', '内帑を売り払って金を作り、約定通りに清算する。', 'Die Privatkasse veräußern, zusammenkratzen, alle Konten begleichen.'),
 q('债务清讫，{kingdom}库中为之一空。', '債務清訖，{kingdom}庫中為之一空。', 'The debts are cleared; the vault of {kingdom} stands bare.', 'Долги погашены; казна {kingdom} стоит пустой.', '借金は清算され、{kingdom}の国庫はからっぽになった。', 'Die Schulden sind getilgt; die Kasse von {kingdom} steht leer.'),
 q('缓期再议', '緩期再議', 'Delay payment', 'Отложить выплату', '猶予を求める', 'Zahlung verschieben'),
 q('令债权人稍安勿躁，再议。', '令債權人稍安勿躁，再議。', 'Tell the creditors to hold their patience—we shall discuss it.', 'Велить кредиторам набраться терпения — обсудим.', '債主には落ち着くよう命じ、改めて話し合う。', 'Den Gläubigern Geduld verordnen—man wird verhandeln.'),
 q('欠饷欠债并发，营中怨气几成哗变之势。', '欠餉欠債並發，營中怨氣幾成嘩變之勢。', 'Unpaid wages and debts together—the camp\'s fury nears open mutiny.', 'Неоплаченное жалованье и долги разом — ярость лагеря грозит мятежом.', '未払の給与と借金が重なり、陣営の怒りは叛乱の勢いに近づく。', 'Sold und Schulden zugleich—der Zorn im Lager ist dem Aufruhr nahe.')
)
ev('subsidy_cut', 2,
 ('岁赐裁减', '歲賜裁減', 'Cutting the Subsidies', 'Сокращение субсидий', '援助の削減', 'Subventionen kürzen'),
 ('诸藩仰赖{kingdom}岁赐，今议裁减——是断外援还是先紧自家？', '諸藩仰賴{kingdom}歲賜，今議裁減——是斷外援還是先緊自家？', 'The marches lean on yearly gifts from {kingdom}; cutting them now—sever the allies, or tighten the own belt first?', 'Окраины живут на ежегодные дары от {kingdom}; урезать их — разорвать союзников или утянуть собственный пояс?', '諸侯は{kingdom}の歳賜に頼る——これを削るのか、外援を断つか、それとも自らを引き締めるか。', 'Die Marken leben von jährlichen Gaben aus {kingdom}; sie nun kürzen—die Verbündeten verlieren oder den eigenen Gürtel enger schnallen?'),
 q('削诸侯岁赐', '削諸侯歲賜', 'Cut the tributes', 'Урезать дань', '諸侯の歳賜を削る', 'Den Tribut kürzen'),
 q('减截盟邦岁赐，先省公帑。', '減截盟邦歲賜，先省公帑。', 'Trim the gifts to allies and save the public purse.', 'Урезать дары союзникам и сберечь казну.', '盟邦への歳賜を減らし、まず公帑を節する。', 'Die Gaben an die Verbündeten kürzen und die Kasse schonen.'),
 q('列国愠怒，使节拂袖，{kingdom}威望为之一挫。', '列國慍怒，使節拂袖，{kingdom}威望為之一挫。', 'The nations bristle; envoys storm out, and the prestige of {kingdom} takes a blow.', 'Державы хмурятся; послы хлопают дверями, престиж {kingdom} страдает.', '列国は憤怒し、使節は袖を払って立ち去り、{kingdom}の威信が一撃を喰らった。', 'Die Nationen sträuben sich; Gesandte rauschen hinaus, das Ansehen von {kingdom} bekommt einen Stoß.'),
 q('削民生赈济', '削民生賑濟', 'Cut the relief', 'Урезать помощь', '救済を切る', 'Die Hilfe kürzen'),
 q('削减义仓赈济与坊铺补贴。', '削減義倉賑濟與坊鋪補貼。', 'Cut the relief granary and workshop subsidies.', 'Урезать помощь из амбаров и дотации мастерским.', '義倉の救済と工房への補助を削る。', 'Hilfsgetreide und Werkstatt-Zuschüsse kürzen.'),
 q('粥厂门可罗雀，饥民聚于市，怨声鼎沸。', '粥廠門可羅雀，饑民聚於市，怨聲鼎沸。', 'The soup kitchens stand empty; the hungry crowd the market, their fury deafening.', 'Столовые пусты; голодные толпятся на рынке, ярость оглушает.', '粥場には客が絶え、飢えた民が市に集まり、怨嗟の声が渦巻く。', 'Die Suppenküchen stehen leer; die Hungrigen drängen auf den Markt, ihr Zorn ist ohrenbetäubend.')
)
ev('trade_house_rise', 2,
 ('巨贾请愿', '巨賈請願', 'The Trade House', 'Торговый дом', '商館の請願', 'Das Handelshaus'),
 ('一巨商号愿出资修建商路市桥，只求{king}赐一特许招牌。', '一巨商號願出資修建商路市橋，只求{king}賜一特許招牌。', 'A mighty trading house will fund bridges and roads—for the boon of one charter from {king}.', 'Могучий торговый дом профинансирует мосты и дороги — за одну лишь хартию от {king}.', '一つの大商館が商路と市橋の建設資金を出す——求めは{king}からの特許の看板ただ一つ。', 'Ein mächtiges Handelshaus finanziert Brücken und Straßen—um den Preis einer einzigen Urkunde von {king}.'),
 q('赐特许状', '賜特許狀', 'Grant the charter', 'Дать хартию', '特許状を授ける', 'Den Freibrief gewähren'),
 q('授旗许照，商号大展宏图。', '授旗許照，商號大展宏圖。', 'Give the banner and the license; the house spreads its wings.', 'Дать знамя и лицензию; дом расправляет крылья.', '旗と許可証を授け、商館が大いに羽ばたく。', 'Banner und Lizenz gewähren; das Haus breitet die Flügel aus.'),
 q('商路畅通，列国商旅交口称誉{kingdom}。', '商路暢通，列國商旅交口稱譽{kingdom}。', 'The roads run free; travelers from every nation sing the praises of {kingdom}.', 'Дороги свободны; путники из всех земель восхваляют {kingdom}.', '商路は開け、列国の旅商人が口々に{kingdom}を称えた。', 'Die Straßen laufen frei; Reisende aller Nationen singen das Lob von {kingdom}.'),
 q('驳回请愿', '駁回請願', 'Deny the petition', 'Отклонить прошение', '請願を却下', 'Das Gesuch ablehnen'),
 q('官府不与商争利，维持如常。', '官府不與商爭利，維持如常。', 'The state will not chase merchant profit; all stays as before.', 'Государство не пойдёт в купеческие прибыли; всё остаётся по-прежнему.', '官は商と利を争わず、従来どおりとする。', 'Der Staat jagt nicht dem Kaufmannsgewinn; alles bleibt wie zuvor.'),
 q('巨贾悻悻而返，市面波澜不惊，徒留叹息。', '巨賈悻悻而返，市面波瀾不驚，徒留嘆息。', 'The great merchant leaves chagrined; the market stays calm, sighs only lingering.', 'Великий купец уходит досадливый; рынок спокоен, остаются лишь вздохи.', '大商人は悔しげに去り、市は平穏のまま、残ったのは嘆きばかり。', 'Der große Kaufmann geht verärgert; der Markt bleibt ruhig, nur Seufzer bleiben.')
)
ev('usury_ring', 2,
 ('高利盘剥', '高利盤剝', 'The Usury Ring', 'Ростовщический круг', '高利貸の輪', 'Der Wucherring'),
 ('{kingdom}的农户多被印子钱缠身，债台高筑者有增无减。', '{kingdom}的農戶多被印子錢纏身，債臺高築者有增無減。', 'Farmers throughout {kingdom} are snared in usurious notes, and their towers of debt grow taller by the day.', 'Крестьяне {kingdom} опутаны ростовщическими записями, и их долговые башни растут день ото дня.', '{kingdom}の農家は高利貸の縁に絡め取られ、借金の山は増えるばかりだ。', 'Die Bauern von {kingdom} sind in Wucherschuld verstrickt, und ihre Schuldentürme wachsen täglich.'),
 q('默许纵容', '默許縱容', 'Look the other way', 'Смотреть сквозь пальцы', '黙認する', 'Wegsehen'),
 q('债利由它，官家不闻不问。', '債利由它，官家不聞不問。', 'Let the interest run; the crown neither hears nor asks.', 'Пусть проценты идут; корона не слышит и не спрашивает.', '利息はそのままに、官は聞かず問わず。', 'Die Zinsen laufen lassen; die Krone hört nichts und fragt nichts.'),
 q('债主逼门，鬻儿卖女者众，怨气日渐郁结。', '債主逼門，鬻兒賣女者眾，怨氣日漸鬱結。', 'Creditors hammer the gates; children and daughters are sold, and resentment knots ever tighter.', 'Кредиторы ломятся в ворота; детей продают, обида затягивается узлом.', '債主が門を叩き、子を売る者が増え、怨念が日々に募る。', 'Gläubiger hämmern an die Tore; Töchter und Söhne werden verkauft, der Groll knotet sich enger.'),
 q('官营抽分', '官營抽分', 'Share the usury', 'Взять свою долю', '官で分け前を取る', 'Am Wucher teilhaben'),
 q('许之营业，官家按例抽分。', '許之營業，官家按例抽分。', 'License the trade; the crown takes its customary cut.', 'Разрешить дело; корона берёт обычную долю.', '営業を許し、官が例の分け前を取る。', 'Das Geschäft lizenzieren; die Krone nimmt ihren üblichen Anteil.'),
 q('息钱入库，{kingdom}库中反添一笔横财。', '息錢入庫，{kingdom}庫中反添一筆橫財。', 'The interest flows in—an unlooked windfall fills the vault of {kingdom}.', 'Проценты входят — нежданная прибыль наполняет казну {kingdom}.', '利息が庫に入り、{kingdom}の国庫に思いがけない大金が加わった。', 'Die Zinsen fließen hinein—ein unverhoffter Glücksfall füllt die Kasse von {kingdom}.')
)
ev('granary_speculation', 2,
 ('官仓投机', '官倉投機', 'Granary Speculation', 'Спекуляция зерном', '官倉の投機', 'Getreidespekulation'),
 ('粮价飞涨，官仓守粮待沽，{king}面前是一笔烫手的买卖。', '糧價飛漲，官倉守糧待沽，{king}面前是一筆燙手的買賣。', 'Grain prices soar; the granaries hold and wait to sell—a scorching deal now lies before {king}.', 'Цены на зерно взлетают; амбары держат зерно и ждут — горячая сделка теперь перед {king}.', '米価が急騰し、官倉は売り時を待つ——{king}の面前に焼け付くような取引が置かれた。', 'Die Getreidepreise steigen; die Speicher halten und warten—ein heißes Geschäft liegt nun vor {king}.'),
 q('官粮抛售', '官糧拋售', 'Sell the granary', 'Продать зерно', '官米を売る', 'Korn verkaufen'),
 q('趁高价抛售陈粮，落袋为安。', '趁高價拋售陳糧，落袋為安。', 'Sell the old grain while prices peak; get it in the bag.', 'Продать старое зерно на пике; деньги в мешок.', '高値のうちに古米を売り払い、懐へ収める。', 'Das alte Korn beim Höchststand verkaufen; es in den Sack holen.'),
 q('陈粮趁势出清，{kingdom}金库增厚。', '陳糧趁勢出清，{kingdom}金庫增厚。', 'The old stores clear out; the vault of {kingdom} thickens.', 'Старые запасы распроданы; казна {kingdom} толстеет.', '古米は出払い、{kingdom}の金庫が厚みを増した。', 'Die alten Bestände gehen weg; die Kasse von {kingdom} wird dicker.'),
 q('听任囤积', '聽任囤積', 'Let them hoard', 'Позволить скупать', '買い占めを放置', 'Horten zulassen'),
 q('巨贾囤粮，官家袖手旁观。', '巨賈囤糧，官家袖手旁觀。', 'Big merchants hoard; the crown folds its arms.', 'Крупные купцы скупают; корона складывает руки.', '大商人の買い占めを、官は袖を組んで見ている。', 'Große Händler horten; die Krone verschränkt die Arme.'),
 q('米价一日三涨，饥民沿街，叫骂连天。', '米價一日三漲，饑民沿街，叫罵連天。', 'Rice prices leap thrice a day; the hungry fill the streets, cursing to the heavens.', 'Цены на рис прыгают трижды в день; голодные высыпали на улицы, клянут небо.', '米価は一日に三度上がり、飢えた民が街に溢れて罵声が響く。', 'Die Reispreise springen dreimal am Tag; die Hungrigen füllen die Straßen, Flüche zum Himmel.')
)
ev('ferry_tax', 2,
 ('渡口之税', '渡口之稅', 'The Ferry Toll', 'Перевозной сбор', '渡しの税', 'Der Fährzoll'),
 ('{kingdom}的水路关卡议征渡税，船家与商旅皆屏息以待。', '{kingdom}的水路關卡議徵渡稅，船家與商旅皆屏息以待。', 'The river crossings of {kingdom} weigh a ferry toll—boatmen and travelers hold their breath.', 'Речные переправы {kingdom} взвешивают сбор — лодочники и путники затаили дыхание.', '{kingdom}の水路の関所が渡し税を検討中——船頭も旅人も息を殺して待つ。', 'Die Flussübergänge von {kingdom} erwägen einen Fährzoll—Fährleute und Reisende halten den Atem an.'),
 q('设卡征渡税', '設卡徵渡稅', 'Toll the ferries', 'Обложить переправы', '渡し税を課す', 'Fähren besteuern'),
 q('官设渡卡，船资加征一厘。', '官設渡卡，船資加徵一釐。', 'Official ferry posts; one cent added to every passage.', 'Официальные заставы; по копейке с каждой переправы.', '官の渡し場を設け、船賃に一厘を加える。', 'Offizielle Fährstationen; ein Cent mehr auf jede Überfahrt.'),
 q('渡税入帑，{kingdom}库中微有增益。', '渡稅入帑，{kingdom}庫中微有增益。', 'The toll enters the purse; the chest of {kingdom} gains a little.', 'Сбор идёт в казну; сундук {kingdom} слегка полнеет.', '渡し税が庫に入り、{kingdom}の国庫が少し増えた。', 'Der Zoll kommt in die Börse; die Truhe von {kingdom} gewinnt ein wenig.'),
 q('河上设卡刁难', '河上設卡刁難', 'Hassle the crossings', 'Затруднять переправы', '渡しを締め付ける', 'Die Überfahrt erschweren'),
 q('借税之名，反复勒索船家。', '借稅之名，反覆勒索船家。', 'Under the tax\'s name, squeeze the boatmen again and again.', 'Под именем налога выжимать лодочников снова и снова.', '税の名を借りて、船頭を何度もゆすり取る。', 'Unter dem Namen Steuer die Fährleute immer wieder schröpfen.'),
 q('船家罢渡，邻国商旅由此转恨{kingdom}。', '船家罷渡，鄰國商旅由此轉恨{kingdom}。', 'The boatmen stop sailing; travelers from every neighbor turn bitter against {kingdom}.', 'Лодочники бросают весла; путники соседей горько пеняют на {kingdom}.', '船頭は渡すのをやめ、隣国の旅商人が{kingdom}に恨みを持つようになった。', 'Die Fährleute legen die Riemen nieder; Reisende der Nachbarn vergehen bitter gegen {kingdom}.')
)
ev('market_margin', 2,
 ('市易之议', '市易之議', 'The Market Dues', 'Рыночные сборы', '市の課金', 'Die Marktgebühren'),
 ('市中牙行哄抬物价，{king}面前摆着两份章程：安民与敛财。', '市中牙行哄抬物價，{king}面前擺著兩份章程：安民與斂財。', 'Brokers jack up prices in the market; two charters lie before {king}: to calm the folk or to fill the purse.', 'Маклеры вздувают цены; перед {king} два устава — успокоить народ или наполнить казну.', '市中の仲買人が物価をつり上げる——{king}の前には二つの規程が並ぶ：民を安んずるか、税を取るか。', 'Makler treiben die Preise hoch; vor {king} liegen zwei Ordnungen: das Volk beruhigen oder die Kasse füllen.'),
 q('立平准市易', '立平準市易', 'Fix fair prices', 'Установить честные цены', '公定価格を布く', 'Faire Preise festlegen'),
 q('官定价格，牙人不得哄抬。', '官定價格，牙人不得哄抬。', 'Official prices; no broker may inflate them.', 'Официальные цены; маклерам нельзя вздувать.', '官が価格を定め、仲買人は釣り上げを禁じられる。', 'Amtliche Preise; kein Makler darf sie treiben.'),
 q('物价平稳，四方商旅称颂{kingdom}公允。', '物價平穩，四方商旅稱頌{kingdom}公允。', 'Prices hold steady; traders from every quarter praise {kingdom} for fairness.', 'Цены держатся; торговцы со всех сторон хвалят {kingdom} за справедливость.', '物価は落ち着き、四方の旅商人が{kingdom}の公明を称えた。', 'Die Preise stehen fest; Händler aus allen Himmelsrichtungen rühmen die Redlichkeit von {kingdom}.'),
 q('官收市租', '官收市租', 'Tax the stalls', 'Обложить лавки', '店賃を徴す', 'Stände besteuern'),
 q('列肆租税入官，立簿记账。', '列肆租稅入官，立簿記賬。', 'Rents from every stall into the state, booked and tallied.', 'Аренду с лавок в казну, в книги и счета.', '店々の賃税を官に納め、帳簿に記す。', 'Mieten aller Stände an den Staat, in Bücher gebucht.'),
 q('市租岁入渐丰，{kingdom}金库为之微涨。', '市租歲入漸豐，{kingdom}金庫為之微漲。', 'The market dues grow; the vault of {kingdom} rises a touch.', 'Рыночные сборы растут; казна {kingdom} слегка поднимается.', '店賃の歳入は増え、{kingdom}の金庫がわずかに上向いた。', 'Die Marktgebühren wachsen; die Kasse von {kingdom} hebt sich ein wenig.')
)
ev('tax_reform_court', 2,
 ('税制革新', '稅制革新', 'The Tax Overhaul', 'Налоговая реформа', '税制の改革', 'Die Steuerreform'),
 ('{king}的朝堂之上，并税改制的争议之声快要掀翻殿顶。', '{king}的朝堂之上，並稅改制的爭議之聲快要掀翻殿頂。', 'In the hall of {king}, the roar over merging and recasting the taxes threatens to lift the roof.', 'В зале {king} рокот о слиянии и перестройке налогов грозит сорвать крышу.', '{king}の朝堂では、税の一本化と改革をめぐる議論が屋根を吹き飛ばすほどだ。', 'In der Halle von {king} droht das Getöse über die Steuerzusammenlegung das Dach zu heben.'),
 q('颁行新税', '頒行新稅', 'Enact new taxes', 'Ввести новые налоги', '新税を布く', 'Neue Steuern erlassen'),
 q('并税改征，摊派入亩，速办。', '並稅改徵，攤派入畝，速辦。', 'Merge the levies, spread them over the fields, and do it fast.', 'Слить сборы, разложить по полям, и живо.', '税を一本化して畑に割り当て、速やかに施行する。', 'Die Abgaben zusammenlegen, auf die Felder verteilen, und zwar schnell.'),
 q('税收大涨，然乡野嗟怨，税吏如虎。', '稅收大漲，然鄉野嗟怨，稅吏如虎。', 'Revenue soars, yet the countryside groans and the tax chasers prowl like tigers.', 'Сборы взлетают, но деревня стонет, сборщики рыщут как тигры.', '税収は急増したが、郷野は嘆き、税吏は虎のようだ。', 'Die Einnahmen schießen hoch, doch das Land stöhnt und die Steuereintreiber streifen wie Tiger.'),
 q('暂且搁置', '暫且擱置', 'Shelve it', 'Отложить', '棚上げする', 'Aufschieben'),
 q('争论未决，留待来年再议。', '爭論未決，留待來年再議。', 'The quarrel stands; leave it for next year.', 'Спор не решён; отложить до будущего года.', '議論は決せず、来年に持ち越す。', 'Der Streit bleibt offen; vertagen bis zum nächsten Jahr.'),
 q('新税搁浅，国库仍是旧样子，朝议稍息。', '新稅擱淺，國庫仍是舊樣子，朝議稍息。', 'The new tax founders; the vault looks as before, and the court quiets.', 'Новый налог утонул; казна прежняя, двор притих.', '新税は棚上げされ、国庫は従来のまま、朝議も沈静した。', 'Die neue Steuer verschwindet; die Kasse ist wie zuvor, der Hof beruhigt sich.')
)
ev('royal_mint_debase', 2,
 ('铸币减重', '鑄幣減重', 'Debasing the Coin', 'Порча монеты', '鋳貨の減量', 'Die Münzverschlechterung'),
 ('{king}的铸钱局送来密奏：每枚铜钱削去三分，何愁库空？', '{king}的鑄錢局送來密奏：每枚銅錢削去三分，何愁庫空？', 'A secret memorial from the mint reaches {king}: clip three parts from every coin—what is there to fear of an empty vault?', 'Тайная записка двора достигает {king}: обрежь у каждой монеты треть — чего бояться пустой казны?', '{king}のもとに鋳銭局から密奏が届く：一枚の銭から三分削れば、国庫の空など何ほどのものか。', 'Eine geheime Denkschrift der Münze erreicht {king}: drei Teile von jeder Münze schneiden—was ist da schon Angst vor leerer Kasse?'),
 q('准减重铸钱', '准減重鑄錢', 'Debase the mint', 'Портить монету', '減鋳を許可', 'Die Münze verschlechtern'),
 q('密令减重，赶铸新钱如潮。', '密令減重，趕鑄新錢如潮。', 'Secret orders to lighten the coin; new money minted in great tides.', 'Тайный приказ облегчить монету; новая льётся приливом.', '密かに減鋳を命じ、新銭を潮のように鋳出す。', 'Geheimer Befehl zum Verschlechtern; neues Geld prägt in Fluten.'),
 q('铸息丰厚，{kingdom}金库随之暴增。', '鑄息豐厚，{kingdom}金庫隨之暴增。', 'The mint margin is fat; the vault of {kingdom} balloons.', 'Монетная маржа жирна; казна {kingdom} раздувается.', '鋳造の利が厚く、{kingdom}の金庫が急に膨らんだ。', 'Der Prägegewinn ist fett; die Kasse von {kingdom} bläht sich auf.'),
 q('维持足色', '維持足色', 'Keep full weight', 'Сохранить вес', '純度を維持', 'Volles Gewicht halten'),
 q('宁可库空，钱币足重如旧。', '寧可庫空，錢幣足重如舊。', 'Rather an empty vault than a coin of less than full weight.', 'Лучше пустая казна, чем облегчённая монета.', '庫が空でも、銭は従来どおりの重さを保つ。', 'Lieber eine leere Kasse als eine Münze unter vollem Gewicht.'),
 q('钱足价稳，但百官欠饷，怨声已起。', '錢足價穩，但百官欠餉，怨聲已起。', 'The coin stands true, but the officials\' wages fall in arrears and grumbling begins.', 'Монета честна, но жалованье чиновников в долгах, и ворчание началось.', '銭はまともで価も安定、だが官の俸給が滞り、怨みが立ち始めた。', 'Die Münze ist ehrlich und der Preis stabil, doch die Beamtengehälter stocken und das Murren beginnt.')
)
ev('savings_hunt', 2,
 ('搜刮民藏', '搜刮民藏', 'The Savings Hunt', 'Охота за сбережениями', '貯蓄狩り', 'Die Sparjagd'),
 ('有臣献《掘藏策》，言{kingdom}民家埋银无数，掘之可富国。', '有臣獻《掘藏策》，言{kingdom}民家埋銀無數，掘之可富國。', 'A minister submits a Hoard-Digging Plan: silver lies buried by the thousands in the homes of {kingdom}—dig and be rich.', 'Министр подаёт «План раскопок»: в домах {kingdom} зарыто серебро тысячами — копай и богатей.', '臣が『埋蔵金計画』を献じる——{kingdom}の民家に無数の銀が埋もれており、掘れば国が富むという。', 'Ein Minister trägt einen Schatzplan vor: In den Häusern von {kingdom} liege Silber zu Tausenden vergraben—graben und reich werden.'),
 q('发兵掘藏', '發兵掘藏', 'Dig for hoards', 'Копать за кладами', '埋蔵金を発掘', 'Nach Hortgeld graben'),
 q('按图索骥，起出埋银入库。', '按圖索驥，起出埋銀入庫。', 'Follow the map, dig up the silver, and carry it to the vault.', 'По карте выкопать серебро и снести в казну.', '目録に従って銀を掘り出し、庫に入れる。', 'Der Karte folgen, das Silber heben und in die Kasse tragen.'),
 q('掘得藏银无数，{kingdom}库藏陡增。', '掘得藏銀無數，{kingdom}庫藏陡增。', 'Hoards are dug up; the stores of {kingdom} leap suddenly higher.', 'Клады выкопаны; запасы {kingdom} вдруг подпрыгнули.', '埋蔵銀は掘り出され、{kingdom}の庫蔵が急に増えた。', 'Die Horte werden gehoben; die Vorräte von {kingdom} springen in die Höhe.'),
 q('强借商积', '強借商積', 'Force loans', 'Занять силой', '商家から強借', 'Zwangsanleihen'),
 q('向商贾强借周转，限期归还。', '向商賈強借週轉，限期歸還。', 'Borrow forcibly from the merchants, promised back within fixed days.', 'Сильно занять у купцов, обещая вернуть в срок.', '商人から強く借り入れ、期限を切って返す。', 'Von den Kaufleuten zwangsweise borgen, Rückzahlung in festen Tagen.'),
 q('商贾怨恚，列国商场对{kingdom}侧目而视。', '商賈怨恚，列國商場對{kingdom}側目而視。', 'The merchants seethe; the trading world casts sidelong eyes at {kingdom}.', 'Купцы кипят; торговый мир косится на {kingdom}.', '商人は怨み、列国の商界は{kingdom}を横目で見るようになった。', 'Die Kaufleute kochen; die Handelswelt blickt von der Seite auf {kingdom}.')
)
ev('wage_freeze', 2,
 ('冻结工钱', '凍結工錢', 'The Wage Freeze', 'Заморозка жалованья', '賃金の凍結', 'Der Lohnstopp'),
 ('坊市工钱日涨，有司奏请冻结——是苦了工匠，还是殷了{kingdom}。', '坊市工錢日漲，有司奏請凍結——是苦了工匠，還是殷了{kingdom}。', 'Wages in the workshops climb daily; the ministry petitions a freeze—hard on the craftsmen, or a boon to {kingdom}?', 'Жалованье в мастерских растёт с каждым днём; ведомство просит заморозку — беда мастерам или благо {kingdom}?', '工房の賃金は日々に上がり、役所が凍結を奏請——職人を苦しめるか、{kingdom}を潤すか。', 'Die Löhne in den Werkstätten steigen täglich; das Amt bittet um Einfrieren—hart für die Handwerker oder ein Segen für {kingdom}?'),
 q('颁令冻结', '頒令凍結', 'Freeze the wages', 'Заморозить жалованье', '凍結令を下す', 'Löhne einfrieren'),
 q('工钱三年不动，违令严惩。', '工錢三年不動，違令嚴懲。', 'Wages fixed for three years; violation punished hard.', 'Жалованье заморожено на три года; нарушение карается.', '賃金は三年据え置き、違反は厳罰。', 'Löhne für drei Jahre eingefroren; Verstoß hart bestrafen.'),
 q('工匠人心惶惶，坊市渐有停摆之兆。', '工匠人心惶惶，坊市漸有停擺之兆。', 'The craftsmen grow restless; the workshops show early signs of stalling.', 'Мастера тревожатся; мастерские уже показывают признаки остановки.', '職人たちは不安に駆られ、工房には止まりかけた兆しが見える。', 'Die Handwerker werden unruhig; die Werkstätten zeigen erste Anzeichen des Stillstands.'),
 q('听其自然', '聽其自然', 'Let it ride', 'Пусть идёт', '自然に任せる', 'Laufen lassen'),
 q('工钱随行就市，官府不干预。', '工錢隨行就市，官府不干預。', 'Let wages follow the market; no official hand.', 'Пусть жалованье идёт за рынком; без руки власти.', '賃金は市場に任せ、官は干渉しない。', 'Löhne dem Markt folgen lassen; keine Hand des Amtes.'),
 q('工钱微涨，坊市安然，两无风波。', '工錢微漲，坊市安然，兩無風波。', 'Wages nudge up; the workshops are at ease, no storms either way.', 'Жалованье чуть подросло; мастерские спокойны, ни бурь.', '賃金は少し上がったが、工房は穏やかで、波風も立たない。', 'Die Löhne steigen ein wenig; die Werkstätten sind ruhig, keine Stürme.')
)
ev('gdp_boom_report', 2,
 ('盛世之报', '盛世之報', 'The Boom Report', 'Доклад о процветании', '繁栄の奏上', 'Der Boom-Bericht'),
 ('司农奏报：仓廪丰、钱粮增——{king}如何发落这份盛世之报？', '司農奏報：倉廩豐、錢糧增——{king}如何發落這份盛世之報？', 'The ministry of agriculture reports: granaries full, money and grain up—how does {king} settle this report of fine times?', 'Ведомство земледелия докладывает: амбары полны, деньги и хлеб растут — как распорядится {king} этим донесением о благоденствии?', '司農の奏上：倉は満ち、銭と糧は増え——この盛世の報せを{king}はどう取り扱うのか。', 'Das Agrarministerium meldet: Speicher voll, Geld und Korn im Aufschwung—wie entscheidet {king} über diesen Bericht guter Zeiten?'),
 q('昭告天下', '昭告天下', 'Proclaim it', 'Возвестить миру', '天下に告げる', 'Verkünden'),
 q('刻石颂功，驿传四邻列国。', '刻石頌功，驛傳四鄰列國。', 'Carve it in stone and send the couriers to every neighbor.', 'Высечь в камне и разослать гонцов ко всем соседям.', '功を石に刻み、近隣諸国へ早馬で伝える。', 'In Stein meißeln und die Boten zu allen Nachbarn senden.'),
 q('列国闻之竞相道贺，称{kingdom}治政有方。', '列國聞之競相道賀，稱{kingdom}治政有方。', 'The nations hasten to congratulate, praising {kingdom} for wise governance.', 'Державы спешат поздравить, восхваляя мудрое правление {kingdom}.', '列国は相次いで祝し、{kingdom}の治政ぶりを称えた。', 'Die Nationen beeilen sich zu gratulieren und rühmen die weise Regierung von {kingdom}.'),
 q('大办庆典', '大辦慶典', 'Lavish festivals', 'Пышные празднества', '大祝典を挙げる', 'Feste feiern'),
 q('加派庆贺钱，举国为之大宴。', '加派慶賀錢，舉國為之大宴。', 'Impose a celebration levy; the whole realm feasts.', 'Ввести праздничный сбор; пир на всё царство.', '祝賀の臨時税を課し、国を挙げての大宴。', 'Eine Feierabgabe erheben; das ganze Reich schmaust.'),
 q('庆典奢华，民间怨其靡费，物议纷纷。', '慶典奢華，民間怨其靡費，物議紛紛。', 'The festival turns lavish; the folk grumble at the waste, and gossip stirs.', 'Празднества пышны; народ ворчит о расточительстве, пересуды шевелятся.', '祝典は豪華だが、民はその浪費を怨み、物議がわき起こる。', 'Das Fest gerät verschwenderisch; das Volk murrt über die Verschwendung, das Gerede gärt.')
)
ev('land_sale_urgent', 2,
 ('急售官田', '急售官田', 'Selling Crown Land', 'Продажа казённых земель', '官田の売却', 'Kronland verkaufen'),
 ('国库告急，有司奏请出售近畿官田——此田一卖，{kingdom}岁入又去一块。', '國庫告急，有司奏請出售近畿官田——此田一賣，{kingdom}歲入又去一塊。', 'The treasury calls for rescue; the ministry petitions sale of crown lands near the capital—once sold, {kingdom} loses a slice of income forever.', 'Казна требует спасения; ведомство просит продать казённые земли у столицы — проданные, они навсегда убавят доход {kingdom}.', '国庫が火急を告げ、役所が近畿の官田の売却を奏請——売れば{kingdom}の歳入はまた一つ減る。', 'Die Kasse ruft nach Rettung; das Amt bittet um Verkauf der Kronländer bei der Hauptstadt—einmal verkauft, verliert {kingdom} dauerhaft ein Stück Einkommen.'),
 q('速售官田', '速售官田', 'Sell it fast', 'Продать быстро', '即売する', 'Schnell verkaufen'),
 q('田价从优，银钱即刻入库。', '田價從優，銀錢即刻入庫。', 'A generous price; silver reaches the vault at once.', 'Щедрая цена; серебро сразу идёт в казну.', '良価格で売り、銀を即座に庫へ入れる。', 'Ein gütiger Preis; das Silber kommt sofort in die Kasse.'),
 q('官田出手，{kingdom}金库立解倒悬。', '官田出手，{kingdom}金庫立解倒懸。', 'The land changes hands; the vault of {kingdom} is pulled from the brink.', 'Земля сменила хозяина; казна {kingdom} отведена от края.', '官田は売れ、{kingdom}の金庫は窮地を脱した。', 'Das Land wechselt den Besitzer; die Kasse von {kingdom} ist von der Klippe gezogen.'),
 q('收回奏议', '收回奏議', 'Withdraw the plan', 'Отозвать план', '奏を引っ込める', 'Den Plan zurückziehen'),
 q('祖业不可轻弃，另寻他法。', '祖業不可輕棄，另尋他法。', 'Ancestral land is not lightly sold; seek another way.', 'Отеческие земли не продают легкомысленно; искать иное.', '先祖伝来の地を軽く売れず、他の道を探す。', 'Ahnenland veräußert man nicht leichtfertig; einen anderen Weg suchen.'),
 q('卖田作罢，国库仍捉襟见肘，只能另寻他途。', '賣田作罷，國庫仍捉襟見肘，只能另尋他途。', 'The sale is dropped; the treasury remains bare-grained, and other paths must be sought.', 'Продажа отменена; казна по-прежнему скудна, придётся искать иное.', '売却は取りやめ、国庫は相変わらず苦しく、他の道を探すしかない。', 'Der Verkauf fällt; die Kasse bleibt weiterhin knapp—man muss andere Wege suchen.')
)
ev('prize_fund', 2,
 ('重赏之下', '重賞之下', 'The Reward Fund', 'Фонд наград', '懸賞の財源', 'Der Preis-Fonds'),
 ('兵部议设军功赏金，户部言库空；两难之间，{king}静坐良久。', '兵部議設軍功賞金，戶部言庫空；兩難之間，{king}靜坐良久。', 'The war office would fund rewards for valor; the treasury says the chest is bare. Between the two, {king} sits long in silence.', 'Военное ведомство хочет наград за доблесть; казна говорит, что сундук пуст. Между ними {king} молча сидит долго.', '兵部が武功の懸賞金を設けようとし、戸部は国庫が空だと言う——その板挟みの中、{king}は長く黙って座っていた。', 'Das Kriegsamt will Tapferkeitsprämien; die Schatzkammer sagt, die Truhe sei leer. Dazwischen sitzt {king} lange stumm.'),
 q('拨银设赏', '撥銀設賞', 'Fund the rewards', 'Выделить на награды', '賞金を出す', 'Prämien finanzieren'),
 q('自帑中拨银，以重赏军功。', '自帑中撥銀，以重賞軍功。', 'Draw silver from the purse to lavish valor.', 'Взять серебро из казны на щедрые награды.', '国庫から銀を出し、武功に厚く報いる。', 'Silber aus der Kasse ziehen, Tapferkeit überhäufen.'),
 q('赏金发出，军中雀跃，库中微损。', '賞金發出，軍中雀躍，庫中微損。', 'The gold goes out; the army jubilates, the chest a little thinner.', 'Золото вышло; армия ликует, сундук слегка тоньше.', '賞金が渡り、軍は歓喜し、国庫は少し痩せた。', 'Das Gold geht hinaus; die Armee jubelt, die Truhe ist ein wenig dünner.'),
 q('旌表义烈', '旌表義烈', 'Honor with titles', 'Наградить званиями', '勲章で顕彰', 'Mit Ehrungen auszeichnen'),
 q('不费分文，只授匾额荣衔。', '不費分文，只授匾額榮銜。', 'Not a coin: only carved plaques and honorary titles.', 'Ни монеты: лишь резные доски и почётные звания.', '一文も使わず、扁額と栄誉の称号のみを授ける。', 'Keinen Heller: nur geschnitzte Tafeln und Ehrentitel.'),
 q('列国闻之，皆赞{kingdom}礼遇义士。', '列國聞之，皆贊{kingdom}禮遇義士。', 'The nations hear of it and praise {kingdom}\'s grace to the brave.', 'Державы слышат и хвалят милость {kingdom} к храбрецам.', '列国はこれを聞き、{kingdom}が義士を厚遇したと讃えた。', 'Die Nationen hören davon und rühmen die Gnade von {kingdom} gegenüber den Tapferen.')
)
ev('customs_leak', 2,
 ('关税漏卮', '關稅漏卮', 'The Customs Leak', 'Утечка пошлин', '関税の漏れ', 'Das Zoll-Leck'),
 ('走私货船如织，{kingdom}的关税如筛漏水，户部急如火燎。', '走私貨船如織，{kingdom}的關稅如篩漏水，戶部急如火燎。', 'Smuggling vessels weave like shuttles; the customs of {kingdom} leak through a sieve, and the ministry burns with alarm.', 'Контрабандные суда снуют как челноки; пошлины {kingdom} текут сквозь сито, ведомство горит тревогой.', '密輸船が行き交い、{kingdom}の関税は篩の水のように漏れ、戸部は焦りに火がついたようだ。', 'Schmugglerschiffe weben wie Spindeln; die Zölle von {kingdom} laufen durch ein Sieb, und die Amtsstube brennt vor Sorge.'),
 q('重金缉私', '重金緝私', 'Bulk up the patrols', 'Усилить досмотр', '巡検を増強', 'Patrouillen verstärken'),
 q('增船添丁，昼夜巡海严查。', '增船添丁，晝夜巡海嚴查。', 'More boats and men; patrol the coast day and night.', 'Больше судов и людей; патрулировать берег день и ночь.', '船と人を増やし、昼夜を問わず沿岸を厳しく見回る。', 'Mehr Boote und Männer; Tag und Nacht die Küste abpatrouillieren.'),
 q('私货渐敛，然缉私耗费惊人，库中又损。', '私貨漸斂，然緝私耗費驚人，庫中又損。', 'Contraband recedes, but the chase costs a fortune—the vault takes another blow.', 'Контрабанда отступает, но погоня стоит целое состояние — казна получает новый удар.', '密輸品は減ったが、取り締まりの費用は驚くほどで、国庫はまた損をした。', 'Die Schmuggelware weicht zurück, doch die Jagd kostet ein Vermögen—die Kasse nimmt einen weiteren Stoß.'),
 q('严刑连坐', '嚴刑連坐', 'Punish by association', 'Карать по круговой поруке', '連座で威す', 'Sippenhaft verhängen'),
 q('购私者连坐，十家互为担保。', '購私者連坐，十家互為擔保。', 'Buyers punished with the smugglers; ten households answer for each other.', 'Покупателей карают вместе с контрабандистами; десятки дворов в круговой поруке.', '私物を買う者も連座し、十戸で互いに保証する。', 'Käufer teilen die Strafe; zehn Haushalte bürgen füreinander.'),
 q('冤狱四起，海民生怨，风暴将起。', '冤獄四起，海民生怨，風暴將起。', 'Wrongful cases multiply; the coastal folk grow bitter, and a storm gathers.', 'Несправедливые дела множатся; прибрежные жители озлобляются, собирается буря.', '冤罪が続出し、海辺の民は怨み、嵐が起ころうとしている。', 'Unrecht betroffene Fälle häufen sich; die Küstenleute grollen, ein Sturm zieht auf.')
)
ev('debt_default', 2,
 ('国债失信', '國債失信', 'The Default', 'Дефолт', '国債の違約', 'Der Zahlungsausfall'),
 ('旧债到期，{kingdom}库空难偿——债主的脸色比债息冷得更快。', '舊債到期，{kingdom}庫空難償——債主的臉色比債息冷得更快。', 'The old bonds fall due and {kingdom} cannot pay—the creditors\' faces chill faster than the rates.', 'Старые облигации созрели, но {kingdom} не может платить — лица кредиторов стынут быстрее ставок.', '旧債が満期を迎え、{kingdom}は支払えない——債主たちの顔は金利より速く冷えていく。', 'Die alten Anleihen werden fällig und {kingdom} kann nicht zahlen—die Gesichter der Gläubiger erkalten schneller als die Zinssätze.'),
 q('推宕拖延', '推宕拖延', 'Stall the creditors', 'Тянуть с кредиторами', '支払いを引き延ばす', 'Gläubiger hinhalten'),
 q('以种种名目拖延偿债日期。', '以種種名目拖延償債日期。', 'Delay the repayment under one pretext or another.', 'Оттягивать выплату под тем или иным предлогом.', 'さまざまな名目で返済日を引き延ばす。', 'Die Rückzahlung unter diesem oder jenem Vorwand verschieben.'),
 q('债主哗然撤资，列国商界对{kingdom}的信誉降至冰点——暗流未止。', '債主嘩然撤資，列國商界對{kingdom}的信譽降至冰點——暗流未止。', 'Creditors reel and pull their money; with the merchant world of every nation, the credit of {kingdom} sinks to freezing—and the undercurrent has not yet stopped.', 'Кредиторы оторопели и отзывают деньги; в торговом мире всех держав кредит {kingdom} падает до точки замерзания — и подводное течение ещё не утихло.', '債主たちは騒然として資金を引き上げ、列国の商界における{kingdom}の信用は氷点まで落ちた——だが暗流はまだ止んでいない。', 'Die Gläubiger ziehen das Geld zurück; im Handelswesen aller Nationen sinkt der Kredit von {kingdom} auf den Gefrierpunkt—und die Unterströmung ist noch nicht zur Ruhe gekommen.'),
 q('变卖家底履约', '變賣家底履約', 'Sell off and pay', 'Распродать и заплатить', '身売りして返済', 'Verkaufen und zahlen'),
 q('典卖宫苑器物，如数偿还。', '典賣宮苑器物，如數償還。', 'Pledge the palace treasures and pay every coin.', 'Заложить дворцовые сокровища и выплатить всё до монеты.', '宮苑の器物を質に入れ、借りをきっちり返す。', 'Die Palastschätze verpfänden und jeden Heller begleichen.'),
 q('债务虽清，库藏尽空，民怨悄然酝酿——事情怕未到此为止。', '債務雖清，庫藏盡空，民怨悄然醞釀——事情怕未到此為止。', 'The debt is cleared, the vaults left bare, and resentment quietly brews—this, one fears, is not where it ends.', 'Долг погашен, казна пуста, обида тихо заваривается — боюсь, на этом дело не кончится.', '借金は返済されたが、庫蔵は空になり、民の怨みがひそかに醸される——事はこれで終わりではなさそうだ。', 'Die Schuld ist getilgt, die Kassen stehen leer, und der Groll gärt leise—dies, fürchtet man, wird nicht das Ende sein.')
)
ev('credit_ice', 2,
 ('信用寒潮', '信用寒潮', 'Credit Freeze', 'Кредитный холод', '信用の氷河期', 'Das Kredit-Eis'),
 ('{kingdom}的信誉如坠冰窖，各商号收钱袋如收镰刀。', '{kingdom}的信譽如墜冰窖，各商號收錢袋如收鐮刀。', 'The credit of {kingdom} has fallen into an ice cellar—each trading house draws its purse as one draws a scythe.', 'Кредит {kingdom} рухнул в ледник — каждый торговый дом прибирает кошель, как прибирают косу.', '{kingdom}の信用は氷庫に落ちた——どの商館も財布を鎌のように引き締める。', 'Der Kredit von {kingdom} ist in einen Eiskeller gefallen—jedes Handelshaus zieht den Geldbeutel zu, wie man die Sense zieht.'),
 q('金币赎信', '金幣贖信', 'Buy back trust', 'Откупиться золотом', '金で信用を買う', 'Vertrauen kaufen'),
 q('出资作保，先行取信于市。', '出資作保，先行取信於市。', 'Put gold up as guarantee to win over the market first.', 'Выставить золото в обеспечение, чтобы сперва завоевать рынок.', '資金で保証し、まず市場の信を取り戻す。', 'Gold als Sicherheit stellen, um zuerst den Markt zu gewinnen.'),
 q('大把金币撒下去，冰山才裂开一线。', '大把金幣撒下去，冰山才裂開一線。', 'Handfuls of gold go down—and the iceberg cracks by a hair.', 'Золото сыплется горстями — и ледник трескается лишь на волосок.', '大金を撒いても、氷山がひびくのは僅かばかり。', 'Gold rieselt in Händen—und der Eisberg reißt um Haaresbreite.'),
 q('强令放款', '強令放款', 'Force the lending', 'Заставить ссужать', '融資を強制', 'Kreditvergabe erzwingen'),
 q('颁令商号照旧放款，违者论处。', '頒令商號照舊放款，違者論處。', 'Edict: the houses must lend as before, or answer for it.', 'Указ: дома обязаны давать взаймы, иначе ответят.', '商館に従来通り貸し付けよと命じ、違反者は処断する。', 'Edikt: Die Häuser müssen wie zuvor leihen, oder sie werden zur Verantwortung gezogen.'),
 q('商号阳奉阴违，市面银根更是冻凝。', '商號陽奉陰違，市面銀根更是凍凝。', 'The houses obey in show and dodge in deed; the market\'s cash freezes all the harder.', 'Дома подчиняются показно и увиливают на деле; деньги на рынке замерзают ещё крепче.', '商館は表向き従い裏では逃げ、市中の銀根はいっそう凍りついた。', 'Die Häuser gehorchen zum Schein und weichen im Tun aus; das Geld am Markt gefriert nur noch härter.')
)
ev('merchant_gift', 2,
 ('商贾献金', '商賈獻金', 'The Merchant\'s Gift', 'Дары купцов', '商人の献金', 'Das Geschenk der Kaufleute'),
 ('巨贾候于宫门，献金三万两，只求{king}一纸通关特许。', '巨賈候於宮門，獻金三萬兩，只求{king}一紙通關特許。', 'A great merchant waits at the palace gate with thirty thousand in gold—asking but one passage charter from {king}.', 'Великий купец ждёт у ворот дворца с тридцатью тысячами золотом — прося у {king} всего одну хартию на проезд.', '大商人は宮門に待ち、三万両の金を献じ、{king}にあの関所通行の特許状をただ一つ求める。', 'Ein großer Kaufmann wartet am Palasttor mit dreißigtausend Gold—bittet {king} nur um einen Durchlass-Brief.'),
 q('照单全收', '照單全收', 'Take the gold', 'Взять золото', 'そのまま受ける', 'Das Gold nehmen'),
 q('收金入帑，特许状即时照发。', '收金入帑，特許狀即時照發。', 'Take the gold into the purse and issue the charter.', 'Взять золото в казну и выдать хартию.', '金を庫に納め、特許状をそのまま発行する。', 'Das Gold in die Kasse nehmen und den Brief ausstellen.'),
 q('{kingdom}金库添金，市井只道王恩浩荡。', '{kingdom}金庫添金，市井只道王恩浩蕩。', 'Gold joins the vault of {kingdom}, and the market speaks only of royal grace.', 'Золото входит в казну {kingdom}, и рынок говорит лишь о королевской милости.', '{kingdom}の金庫に金が加わり、市井はただ王の恩恵の広さを語る。', 'Gold fließt in die Kasse von {kingdom}, und der Markt spricht nur von der königlichen Gnade.'),
 q('御笔却礼', '御筆卻禮', 'Refuse politely', 'Вежливо отказать', '辞退する', 'Höflich ablehnen'),
 q('分文不受，礼单恭恭敬敬奉还。', '分文不受，禮單恭恭敬敬奉還。', 'Not a coin taken; the list of gifts returned with all courtesy.', 'Ни монеты; список даров возвращён с учтивостью.', '一銭も受け取らず、献上品の目録を丁重に返す。', 'Keinen Heller nehmen; die Geschenkliste mit aller Höflichkeit zurückgeben.'),
 q('巨贾拂袖而去，商界对{kingdom}颇有微词。', '巨賈拂袖而去，商界對{kingdom}頗有微詞。', 'The merchant sweeps off; the trading world has words of reproach for {kingdom}.', 'Купец уходит, взмахнув рукавом; торговый мир пеняет на {kingdom}.', '大商人は袖を払って去り、商界は{kingdom}にやや不満の声を漏らす。', 'Der Kaufmann rauscht davon; die Handelswelt hat Vorwürfe für {kingdom}.')
)
ev('tax_shield', 2,
 ('蠲免之请', '蠲免之請', 'The Tax Waiver', 'Просьба о льготах', '減免の請願', 'Die Steuerbefreiung'),
 ('灾乡联名上疏，请蠲租赋三成——{king}的笔杆悬在朱批之上。', '災鄉聯名上疏，請蠲租賦三成——{king}的筆桿懸在硃批之上。', 'The stricken counties petition together to waive three parts of the levies—the brush of {king} hovers over the vermilion assent.', 'Пострадавшие уезды скопом просят снять три части податей — кисть {king} висит над алым подписом.', '災害の郷が連名で上疏し、租の三割を免除せよと請う——{king}の筆は朱批の上に浮いて止まる。', 'Die geschlagenen Grafschaften betteln gemeinsam, drei Teile der Abgaben zu erlassen—der Pinsel von {king} schwebt über dem roten Zeichen.'),
 q('蠲免赋税', '蠲免賦稅', 'Grant the waiver', 'Даровать льготы', '賦税を免ずる', 'Erlass gewähren'),
 q('朱批减免三成，乡民得苏。', '硃批減免三成，鄉民得蘇。', 'The vermilion stroke grants three parts; the country folk draw breath.', 'Алый росчерк дарует три части; сельские жители переводят дух.', '朱批で三割を免じ、郷民が息を吹き返す。', 'Der rote Strich gewährt drei Teile; das Landvolk atmet auf.'),
 q('灾乡复苏，{kingdom}库藏为此又瘦一分。', '災鄉復甦，{kingdom}庫藏為此又瘦一分。', 'The counties revive; the stores of {kingdom} shrink by another notch.', 'Уезды оживают; запасы {kingdom} съёживаются ещё на одну ступень.', '郷は立ち直ったが、{kingdom}の庫蔵はその分また細くなった。', 'Die Grafschaften erholen sich; die Vorräte von {kingdom} schrumpfen um eine Kerbe.'),
 q('严词驳回', '嚴詞駁回', 'Refuse outright', 'Ответить отказом', 'はねつける', 'Ablehnen'),
 q('国用正紧，令其秋后再议。', '國用正緊，令其秋後再議。', 'The state\'s need is pressing; tell them to return after the autumn.', 'Нужда государства остра; велеть вернуться после осени.', '国費が逼迫しており、秋を過ぎてから議論せよと命じる。', 'Die Staatsnot ist drängend; man verweist sie auf nach dem Herbst.'),
 q('灾民聚于衙前，喊冤之声一日隆过一日。', '災民聚於衙前，喊冤之聲一日隆過一日。', 'The folk gather at the offices, and the cry of grievance swells day by day.', 'Народ сходится к управлениям, и крик обиды растёт день ото дня.', '被災民は役所の前に集まり、哀訴の声が日ごとに大きくなる。', 'Das Volk sammelt sich vor den Ämtern, und der Schrei der Not schwillt Tag um Tag.')
)
ev('relief_auction', 2,
 ('义卖筹赈', '義賣籌賑', 'The Relief Auction', 'Благотворительный аукцион', '義捐競売', 'Die Hilfs-Auktion'),
 ('洪水退去，饥民塞道——豪绅们命人抬着银子，来给{king}捧场。', '洪水退去，饑民塞道——豪紳們命人抬著銀子，來給{king}捧場。', 'The flood recedes and the hungry choke the roads—the rich gentlemen have silver carried in, to cheer on {king}.', 'Потоп отступил, голодные запрудили дороги — богачи везут серебро, чтобы порадовать {king}.', '洪水は引いたが飢えた民が道を塞ぐ——豪紳たちは銀を担がせて、{king}に花を持たせに来る。', 'Die Flut weicht zurück, die Hungrigen verstopfen die Straßen—die reichen Herren lassen Silber hereintragen, um {king} zu gefallen.'),
 q('开卖虚衔', '開賣虛銜', 'Sell honorary titles', 'Продать почётные чины', '名誉位を売る', 'Ehrentitel versteigern'),
 q('品官虚衔价高者得，款即充赈。', '品官虛銜價高者得，款即充賑。', 'Honorary ranks to the highest bidder; the sum goes straight to relief.', 'Почётные ранги за лучшую цену; сумма идёт напрямую на помощь.', '名誉の官位は高値の者に売り、その金はすぐに救済へ回す。', 'Ehrenränge an den Höchstbietenden; die Summe fließt direkt in die Hilfe.'),
 q('赈款入账，{kingdom}金库回血，举国称善。', '賑款入賬，{kingdom}金庫回血，舉國稱善。', 'Relief silver lands; the vault of {kingdom} recovers, and the realm approves.', 'Средства прибыли; казна {kingdom} оживает, страна одобряет.', '救済金が入り、{kingdom}の金庫が息を吹き返し、国を挙げて善しとする。', 'Das Hilfssilber landet; die Kasse von {kingdom} erholt sich, und das Reich lobt es.'),
 q('开仓放赈', '開倉放賑', 'Open the granaries', 'Открыть амбары', '倉を開いて救う', 'Die Speicher öffnen'),
 q('不分老幼，一律按户赈济。', '不分老幼，一律按戶賑濟。', 'Young and old alike, relief by every household.', 'Малым и старым одинаково; помощь каждому двору.', '老若を問わず、戸ごとに一律に救済する。', 'Jung wie alt, Hilfe für jeden Haushalt.'),
 q('四邻传颂，列国皆赞{kingdom}仁德。', '四鄰傳頌，列國皆贊{kingdom}仁德。', 'The neighbors spread the tale; every nation praises the benevolence of {kingdom}.', 'Соседи разносят весть; все державы хвалят милосердие {kingdom}.', '近隣が口々に伝え、列国は{kingdom}の仁徳を讃えた。', 'Die Nachbarn tragen die Kunde fort; alle Nationen rühmen die Mildtätigkeit von {kingdom}.')
)
ev('coin_cut', 2,
 ('剪边恶钱', '剪邊惡錢', 'The Clipped Coins', 'Обрезанные монеты', '剪銭の悪貨', 'Die beschnittenen Münzen'),
 ('{kingdom}的铜钱被剪刀一枚枚啃薄，市面已开始拒收。', '{kingdom}的銅錢被剪刀一枚枚啃薄，市面已開始拒收。', 'The copper coins of {kingdom} are gnawed thin by scissors, one by one—and the market has begun to refuse them.', 'Медные монеты {kingdom} изгрызены ножницами до тонкости, одну за другой — и рынок уже отказывается их брать.', '{kingdom}の銅銭は一銭ずつハサミで薄く削り取られ、市場は受け取りを拒み始めている。', 'Die Kupfermünzen von {kingdom} werden Schere um Schere dünn geknabbert—und der Markt beginnt, sie zu verweigern.'),
 q('放任流通', '放任流通', 'Let them circulate', 'Пустить в оборот', '流通を放任', 'Sie kursieren lassen'),
 q('剪边钱照旧流通，权当不知。', '剪邊錢照舊流通，權當不知。', 'Let the clipped coins run as before; feign not to know.', 'Пусть обрезанные ходят как прежде; сделать вид, что не знаешь.', '剪銭は従来どおり流通させ、知らないふりをする。', 'Die beschnittenen Münzen wie bisher laufen lassen; Unwissenheit vortäuschen.'),
 q('劣钱横行，物价如沸如煮，怨声四起。', '劣錢橫行，物價如沸如煮，怨聲四起。', 'Bad coin reigns; prices boil and seethe, and complaints rise on all sides.', 'Плохая монета правит; цены кипят и бурлят, жалобы со всех сторон.', '悪貨が横行し、物価は沸き立ち、怨みの声が四方に上がる。', 'Das schlechte Geld herrscht; die Preise kochen und sieden, Klagen steigen von allen Seiten.'),
 q('重铸新钱', '重鑄新錢', 'Restrike the coin', 'Перечеканить', '新貨に改鋳', 'Neu prägen'),
 q('召回剪边钱，如数改铸新钱。', '召回剪邊錢，如數改鑄新錢。', 'Call in the clipped coins and restrike them full.', 'Собрать обрезанные и перечеканить в полновесные.', '剪銭を回収し、同じ枚数を新銭に改鋳する。', 'Die beschnittenen einziehen und voll nachprägen.'),
 q('新钱鼓铸入市，市面重归安稳祥和。', '新錢鼓鑄入市，市面重歸安穩祥和。', 'Fresh coin is struck into circulation; the market settles back into calm.', 'Свежая монета входит в оборот; рынок возвращается к спокойствию.', '新銭が鋳出されて市中に入り、市場は再び平穏に戻った。', 'Frisches Geld wird geprägt und kursiert; der Markt kehrt in die Ruhe zurück.')
)
ev('enterprise_rent', 2,
 ('坊租之议', '坊租之議', 'The Workshop Rents', 'Оброк с мастерских', '工房の借賃', 'Die Werkstatt-Mieten'),
 ('百工兴旺，坊主盈仓——有司目光一转，盯上了{kingdom}的坊市。', '百工興旺，坊主盈倉——有司目光一轉，盯上了{kingdom}的坊市。', 'The crafts flourish and the workshop masters fill their stores—a glance turns from the ministry, and it is fixed on the factories of {kingdom}.', 'Ремёсла цветут, мастера заполняют склады — взгляд ведомства оборачивается и упирается в мастерские {kingdom}.', '百工は栄え、工房主は倉を満たす——役所の目が転じ、{kingdom}の工房街を捉えた。', 'Die Gewerke blühen, die Meister füllen die Vorräte—der Blick des Amtes schwenkt und haftet auf den Werkstätten von {kingdom}.'),
 q('按年征租', '按年徵租', 'Levy the rents', 'Взымать оброк', '年賃を徴す', 'Mieten erheben'),
 q('簿记岁入，按什一之制抽租。', '簿記歲入，按什一之制抽租。', 'Record the yearly gains; take a tenth as rent.', 'Записать годовые доходы; взять десятую часть оброком.', '歳入を帳簿に記し、十の一を借賃として取る。', 'Die Jahreseinnahmen buchen; den Zehnten als Miete nehmen.'),
 q('坊租依时入帑，{kingdom}库藏添彩。', '坊租依時入帑，{kingdom}庫藏添彩。', 'The rents come in on time and add luster to the vault of {kingdom}.', 'Обороки входят в срок, добавляя блеска казне {kingdom}.', '借賃は期どおりに庫へ入り、{kingdom}の庫蔵に彩りを添えた。', 'Die Mieten kommen fristgerecht und geben der Kasse von {kingdom} Glanz.'),
 q('加租倍征', '加租倍徵', 'Double the rents', 'Удвоить оброк', '借賃を倍に', 'Mieten verdoppeln'),
 q('趁兴加倍，坊主各各叫苦。', '趁興加倍，坊主各各叫苦。', 'In the flush, double it—and every master wails.', 'С наглостью удвоить — и каждый мастер стенает.', '調子に乗って倍にし、工房主たちは口々に叫ぶ。', 'In der Hochstimmung verdoppeln—und jeder Meister jammert.'),
 q('坊主纷纷罢市抗议，怨声直抵宫门。', '坊主紛紛罷市抗議，怨聲直抵宮門。', 'The masters close their workshops in protest; the clamor reaches the palace gate.', 'Мастера закрывают мастерские в знак протеста; гул достигает дворцовых ворот.', '工房主たちは次々に仕事を止めて抗議し、怨嗟は宮門まで届いた。', 'Die Meister schließen ihre Werkstätten im Protest; der Lärm erreicht das Palasttor.')
)
ev('tax_relief_petition', 2,
 ('减税请愿', '減稅請願', 'The Tax Petition', 'Ходатайство о налоге', '減税の請願', 'Die Steuerpetition'),
 ('商会与耆老联名请愿，言税重民艰，愿{king}蠲减一二。', '商會與耆老聯名請願，言稅重民艱，願{king}蠲減一二。', 'The guilds and the elders petition together: taxes weigh heavy and the people suffer—may {king} remit one part or two.', 'Гильдии и старейшины ходатайствуют вместе: налоги давят, народ страдает — пусть {king} снимет одну-другую часть.', '商会と耆老らが連名で請願する——税は重く民は苦しむ、{king}よ、少しでも免じてほしいと。', 'Zünfte und Älteste betteln gemeinsam: Die Steuern lasten schwer und das Volk leidet—möge {king} ein Teil oder zwei erlassen.'),
 q('俯允所请', '俯允所請', 'Grant the plea', 'Удовлетворить прошение', '請願を容れる', 'Dem Gesuch stattgeben'),
 q('轻徭薄赋，岁减租赋三成。', '輕徭薄賦，歲減租賦三成。', 'Light labor, thin levies; three parts off each year.', 'Лёгкие повинности, тонкие подати; три части в год — долой.', '徭役を軽く賦税を薄くし、年間三割を減らす。', 'Leichte Fron, dünne Lasten; drei Teile im Jahr erlassen.'),
 q('万民称颂，列国亦赞{kingdom}恤民，惜库中为此短了。', '萬民稱頌，列國亦贊{kingdom}恤民，惜庫中為此短了。', 'The folk sing praise; the nations too admire {kingdom}\'s care for its people—if only the vault had not thinned for it.', 'Народ поёт хвалу; державы тоже восхищаются заботой {kingdom} о людях — вот только казна похудела.', '万民が称え、列国も{kingdom}の民への慈しみを讃える——惜しむらくは国庫の減ったこと。', 'Das Volk singt Lob; auch die Nationen bewundern die Fürsorge von {kingdom}—wäre nur die Kasse dabei nicht dünner geworden.'),
 q('驳回请愿', '駁回請願', 'Deny the plea', 'Отклонить прошение', '請願を却下', 'Das Gesuch ablehnen'),
 q('国用如渴，此事恕难从命。', '國用如渴，此事恕難從命。', 'The state thirsts for funds; this plea, much as we regret, cannot be granted.', 'Государство жаждет средств; эту просьбу, как ни жаль, удовлетворить нельзя.', '国費は喉が渇いており、この願いはよほどのことがなければ承れない。', 'Der Staat dürstet nach Mitteln; diese Bitte kann man nicht gewähren.'),
 q('请愿者于宫前长跪，群情愤愤不平。', '請願者於宮前長跪，群情憤憤不平。', 'The petitioners kneel long before the palace; the crowd seethes, all injustice.', 'Просители долго стоят на коленях; толпа кипит, всё полно обиды.', '請願者たちは宮の前に長く跪き、群衆は憤りを募らせる。', 'Die Bittsteller knien lange vor dem Palast; die Menge gärt, alles ist voller Unbill.')
)
ev('state_pawnshop', 2,
 ('官营质库', '官營質庫', 'The State Pawnshop', 'Государственный ломбард', '官営の質屋', 'Die staatliche Pfandleihe'),
 ('当铺利厚，有司奏请设官营质库——利归{kingdom}，还是利归市井？', '當鋪利厚，有司奏請設官營質庫——利歸{kingdom}，還是利歸市井？', 'Pawnshops reap fat gains; the ministry petitions a state-owned house—profit to {kingdom}, or profit to the alleys?', 'Ломбарды собирают жирную прибыль; ведомство просит государственный дом — прибыль {kingdom} или прибыль переулкам?', '質屋の利は厚い——役所が官営の質庫の設置を奏請する。{kingdom}の利か、それとも市井の利か。', 'Pfandleihen ernten fette Gewinne; das Amt bittet um ein staatliches Haus—Gewinn für {kingdom} oder Gewinn für die Gassen?'),
 q('开办质库', '開辦質庫', 'Open the shops', 'Открыть ломбарды', '質屋を開く', 'Pfandleihen eröffnen'),
 q('拨官银为本，低息典押，利薄而稳。', '撥官銀為本，低息典押，利薄而穩。', 'State silver as the capital; pledges at a modest, steady rate.', 'Казённое серебро как капитал; заложенное под скромный стабильный процент.', '官銀を元手にし、低い利子で質入れを受ける——利は薄くとも確実だ。', 'Staatssilber als Kapital; Pfänder zu mäßigem, festem Zins.'),
 q('质库应时开张，{kingdom}薄有进项。', '質庫應時開張，{kingdom}薄有進項。', 'The shop opens in season; {kingdom} gains a modest income.', 'Ломбард открылся в срок; {kingdom} получает скромный доход.', '質屋は時節に開き、{kingdom}にはほどほどの収入がある。', 'Das Haus öffnet zur rechten Zeit; {kingdom} hat ein bescheidenes Einkommen.'),
 q('不予开办', '不予開辦', 'Decline to open', 'Не открывать', '開設を見送る', 'Nicht eröffnen'),
 q('官不与民争利，此议作罢。', '官不與民爭利，此議作罷。', 'The state will not vie with the people for gain; let the matter drop.', 'Государство не будет соперничать с народом; пусть вопрос замрёт.', '官は民と利を争わず、この議は立ち消えとする。', 'Der Staat wetteifert nicht mit dem Volk um Gewinn; die Sache verlaufen lassen.'),
 q('市井当铺照旧营生，国库仍多窘迫。', '市井當鋪照舊營生，國庫仍多窘迫。', 'The street pawnshops carry on as ever; the state chest stays pinched.', 'Уличные ломбарды живут как прежде; государственный сундук остаётся скудным.', '市井の質屋は従来どおり営み、国庫は相変わらず苦しい。', 'Die Gassen-Pfandleihen treiben es wie je; die Staatskasse bleibt gekniffen.')
)
ev('port_duties', 2,
 ('市舶之利', '市舶之利', 'The Port Duties', 'Портовые сборы', '市舶の税', 'Die Hafenzölle'),
 ('海舶云集，有司献议加征市舶税——{king}之笔，牵动万里海路。', '海舶雲集，有司獻議加徵市舶稅——{king}之筆，牽動萬里海路。', 'Ocean ships crowd the harbor; the ministry proposes higher port dues—the brush of {king} stirs ten thousand miles of sea road.', 'Морские суда теснятся в гавани; ведомство предлагает поднять портовые сборы — кисть {king} шевелит десять тысяч миль морских дорог.', '海船が港に集まり、役所が市舶税の増徴を進言——{king}の一筆が万里の海路を動かす。', 'Seeschiffe drängen sich im Hafen; das Amt schlägt höhere Hafenzölle vor—der Pinsel von {king} bewegt zehntausend Meilen Seeweg.'),
 q('抽分市舶', '抽分市舶', 'Tax the shipping', 'Обложить корабли', '船税を抽く', 'Schiffe besteuern'),
 q('按货值什一抽分，岁入可观。', '按貨值什一抽分，歲入可觀。', 'A tenth of cargo value; the year\'s income looks handsome.', 'Десятина со стоимости груза; годовой доход выглядит щедро.', '積み荷の価格の十の一を分け取り、歳入は見応えがある。', 'Ein Zehntel des Ladungswerts; das Jahreseinkommen sieht stattlich aus.'),
 q('市舶税依例入帑，{kingdom}金库见厚。', '市舶稅依例入帑，{kingdom}金庫見厚。', 'The port dues enter the purse as ordained; the vault of {kingdom} thickens.', 'Портовые сборы входят в казну, как велено; сундук {kingdom} толстеет.', '市舶税は例のとおり庫に入り、{kingdom}の金庫が厚くなる。', 'Die Hafenzölle fließen wie verordnet in die Börse; die Kasse von {kingdom} wird dick.'),
 q('苛待海商', '苛待海商', 'Browbeat the merchants', 'Притеснять купцов', '海商を虐げる', 'Händler drangsalieren'),
 q('验货留难，私罚暗扣成例。', '驗貨留難，私罰暗扣成例。', 'Delay the inspections, fine on the side, dock secretly—all by custom.', 'Задерживать досмотр, штрафовать втихую, срезать под шумок — по обычаю.', '検分を滞らせ、勝手な罰金と内密の差し引きが常態となる。', 'Kontrollen verzögern, nebenher bestrafen, heimlich abziehen—alles durch Brauch.'),
 q('海商怨声载道，邻港自此抢走了生意。', '海商怨聲載道，鄰港自此搶走了生意。', 'Sea merchants rail on all sides; neighboring ports steal the trade from then on.', 'Морские купцы ропщут на всех уровнях; соседние порты с тех пор перехватывают торговлю.', '海商の怨嗟は道に溢れ、隣の港が以来商売をかっさらった。', 'Die Seehändler schimpfen allenthalben; die Nachbarhäfen stehlen seither den Handel.')
)
ev('fraud_accounting', 2,
 ('账目疑云', '賬目疑雲', 'The False Books', 'Поддельные счета', '帳簿の偽造', 'Die falschen Bücher'),
 ('有人告发户部书吏做假账——{king}面前摊着两份相互矛盾的账册。', '有人告發戶部書吏做假賬——{king}面前攤著兩份相互矛盾的賬冊。', 'A whisper denounces the ministry clerks for forging books—two ledgers lie before {king}, one contradicting the other.', 'Шёпот доносит: писари ведомства подделывают книги — перед {king} лежат два гроссбуха, один противоречит другому.', '戸部の下役が帳簿を偽ったと密告がある——{king}の前には互いに矛盾する二冊の帳簿が広げられる。', 'Ein Flüstern denunziert die Kanzleischreiber, Bücher zu fälschen—zwei Kontobücher liegen vor {king}, eines widerspricht dem anderen.'),
 q('公开彻查', '公開徹查', 'Audit openly', 'Открытая ревизия', '公開で監査', 'Öffentlich prüfen'),
 q('三司会审，尽翻二十年旧账。', '三司會審，盡翻二十年舊賬。', 'Three courts in joint session; turn over twenty years of ledgers.', 'Три ведомства вместе; перевернуть двадцать лет гроссбухов.', '三司が合同で審理し、二十年分の旧帳をすべて引っくり返す。', 'Drei Gerichte zu gemeinsamer Sitzung; zwanzig Jahre Konten umwälzen.'),
 q('假账牵出旧案累累，官场人人自危，动荡将起。', '假賬牽出舊案累累，官場人人自危，動蕩將起。', 'The forged books drag up old cases one after another; every official fears for himself, and turmoil gathers.', 'Подделки вскрывают старые дела одно за другим; каждый чиновник боится за себя, смута собирается.', '偽帳簿から旧悪が次々と浮かび、官界は誰もが身を案じ、動乱が起ころうとしている。', 'Die Fälschungen schleppen alte Fälle nach oben; jeder Beamte fürchtet sich, der Aufruhr bündelt sich.'),
 q('密雇商办', '密雇商辦', 'Hire a private audit', 'Нанять частный аудит', '商人に密かに監査', 'Private Prüfung anheuern'),
 q('重金密雇账房查对，不声张。', '重金密雇賬房查對，不聲張。', 'Hush the matter; hire accounting clerks for a heavy price to check the books.', 'Замести следы; нанять счетоводов за немалую цену проверить книги.', '高額で帳房を密かに雇って照合させ、外には漏らさない。', 'Die Sache verschweigen; Rechnungsführer zu schwerem Preis anheuern, die Bücher zu prüfen.'),
 q('账查清了，{kingdom}的库银却为此见底了一角。', '賬查清了，{kingdom}的庫銀卻為此見底了一角。', 'The books come clean, but the silver of {kingdom} shows the bottom of one more corner.', 'Книги чисты, но казна {kingdom} похудела ещё на один уголок.', '帳簿は明らかになったが、{kingdom}の庫銀はその一角が底を見せた。', 'Die Bücher kommen klar, doch das Silber von {kingdom} zeigt aus einer Ecke den Boden.')
)
ev('budget_lean', 2,
 ('节流之政', '節流之政', 'The Lean Budget', 'Урезанный бюджет', '歳出の削減', 'Der Sparhaushalt'),
 ('入不敷出，户部献议节流——{king}的笔先落向哪里，哪里就会多出怨言。', '入不敷出，戶部獻議節流——{king}的筆先落向哪裡，哪裡就會多出怨言。', 'Income cannot meet outlay; the ministry petitions austerity—wherever the brush of {king} first falls, there the grumbling grows.', 'Доход не покрывает расход; ведомство просит строгой экономии — куда ни упадёт кисть {king}, там и растёт ворчание.', '入りが足りず、戸部が歳出削減を進言——{king}の筆が落ちた先には、必ず怨み言が増える。', 'Das Einkommen deckt die Ausgaben nicht; das Amt bittet um Sparsamkeit—wohin der Pinsel von {king} zuerst fällt, dort gedeiht das Murren.'),
 q('裁汰冗费', '裁汰冗費', 'Trim the fat', 'Срезать лишнее', '無駄を削る', 'Fett abspecken'),
 q('停驿站、减仪仗、并衙门。', '停驛站、減儀仗、並衙門。', 'Halt the courier stops, cut the processions, merge the offices.', 'Остановить ямские станции, урезать процессии, слить конторы.', '駅亭を止め、儀仗を減らし、役所を併合する。', 'Kurierstationen anhalten, Prozessionen kürzen, Ämter verschmelzen.'),
 q('岁费骤省，{kingdom}库中渐有起色。', '歲費驟省，{kingdom}庫中漸有起色。', 'The yearly outlay falls at a stroke; the chest of {kingdom} slowly brightens.', 'Годовой расход падает разом; сундук {kingdom} понемногу светлеет.', '歳費は急に省かれ、{kingdom}の国庫にようやく光が差す。', 'Die Jahresausgaben fallen mit einem Schlag; die Truhe von {kingdom} hellt sich langsam auf.'),
 q('减俸裁员', '減俸裁員', 'Cut pay and staff', 'Срезать жалованья', '俸禄を削減', 'Gehälter kürzen'),
 q('吏员俸禄折半，冗役遣散。', '吏員俸祿折半，冗役遣散。', 'Halve the clerks\' wages; send the surplus hands home.', 'Урезать жалованье писцов наполовину; лишних отправить по домам.', '吏員の俸禄を半減し、余剰の役人を解き放つ。', 'Die Schreibergehälter halbieren; die überzähligen Hände heimsenden.'),
 q('吏员怨声鼎沸，衙门公务几近停滞。', '吏員怨聲鼎沸，衙門公務幾近停滯。', 'The clerks\' fury boils; the business of the offices all but stalls.', 'Ярость писарей кипит; дела контор почти стоят.', '吏員の怨嗟が沸き立ち、役所の公務はほぼ止まった。', 'Der Zorn der Schreiber kocht; die Geschäfte der Ämter stocken fast.')
)
