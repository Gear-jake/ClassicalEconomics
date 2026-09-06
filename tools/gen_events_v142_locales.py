# -*- coding: utf-8 -*-
"""v1.4.2：为 40 个新事件生成四语文案（含 {king}/{kingdom} 占位）。"""
import io, json, collections

T = {}
def ev(eid, opts, title, desc, *texts):
    T['ev_' + eid] = title
    T['ev_' + eid + '_desc'] = desc
    assert len(texts) == opts * 3, eid
    for oi in range(opts):
        name, d, res = texts[oi * 3], texts[oi * 3 + 1], texts[oi * 3 + 2]
        i = oi
        T['ev_%s_opt%d' % (eid, i + 1)] = name
        T['ev_%s_opt%d_desc' % (eid, i + 1)] = d
        T['ev_%s_res%d' % (eid, i + 1)] = res

def q(zh, tw, en, ru):
    return (zh, tw, en, ru)

# ============ 财政 ============
ev('merchant_loan', 2,
 q('外债上门', '外債上門', 'The Foreign Loan', 'Иностранный заём'),
 q('{king}的使者带回一个提议：富商联盟愿借出一笔巨款，解{kingdom}燃眉之急——天下没有白借的钱。', '{king}的使者帶回一個提議：富商聯盟願借出一筆巨款，解{kingdom}燃眉之急——天下沒有白借的錢。', 'An envoy brings an offer: the merchant league will lend {kingdom} a fortune — no loan comes free.', 'Посол приносит предложение: лига купцов одолжит {kingdom} состояние — даром не бывает.'),
 q('立约借债', '立約借債', 'Sign the loan', 'Подписать заём'),
 q('收下巨款，两年后连本带利一并归还。', '收下巨款，兩年後連本帶利一併歸還。', 'Take the gold now; repay with interest in two years.', 'Взять золото; вернуть с процентами через два года.'),
 q('巨款入库，{king}在契约上盖下了国玺。', '巨款入庫，{king}在契約上蓋下了國璽。', 'The gold is in; {king} sealed the contract.', 'Золото в казне; {king} скрепил договор печатью.'),
 q('婉言谢绝', '婉言謝絕', 'Decline politely', 'Вежливо отказаться'),
 q('不借外债，{kingdom}的钱袋自己管。', '不借外債，{kingdom}的錢袋自己管。', 'No foreign debts; {kingdom} minds its own purse.', 'Никаких чужих долгов; {kingdom} сам ведёт кошелёк.'),
 q('借议被婉拒，{king}说{kingdom}不欠人情的债。', '借議被婉拒，{king}說{kingdom}不欠人情的債。', 'The offer is declined; {king} says {kingdom} owes no favors.', 'Предложение отклонено; {king} не любит долгов.'))
ev('loan_due', 2,
 q('债主上门', '債主上門', 'The Debt Comes Due', 'Срок платы'),
 q('两年之期已到，富商联盟的讨债人堵在了王宫门口，{king}必须做出交代。', '兩年之期已到，富商聯盟的討債人堵在了王宮門口，{king}必須做出交代。', 'Two years are up; the league collectors stand at the palace gates, and {king} must answer.', 'Срок вышел; сборщики у ворот дворца, и {king} должен платить.'),
 q('如约还清', '如約還清', 'Repay in full', 'Выплатить сполна'),
 q('刮空一部分国库，保住{kingdom}的信用。', '刮空一部分國庫，保住{kingdom}的信用。', 'Bleed the treasury but keep the crown\'s credit.', 'Опустошить казну, но сохранить кредит короны.'),
 q('债款连利还清，{kingdom}借债的名声反而更好了。', '債款連利還清，{kingdom}借債的名聲反而更好了。', 'The debt is paid with interest; {kingdom}\'s credit grew.', 'Долг выплачен; кредит {kingdom} вырос.'),
 q('赖账不还', '賴賬不還', 'Repudiate it', 'Отказаться платить'),
 q('撕毁契约，承担富商联盟与列国的白眼。', '撕毀契約，承擔富商聯盟與列國的白眼。', 'Tear up the contract and eat the scorn of every court.', 'Разорвать договор и снести презрение дворов.'),
 q('契约被当众撕碎，各国商人记住了{kingdom}。', '契約被當眾撕碎，各國商人記住了{kingdom}。', 'The contract is torn up publicly; merchants everywhere remember {kingdom}.', 'Договор разорван публично; купцы запомнили {kingdom}.'))
ev('tax_farm', 2,
 q('包税之议', '包稅之議', 'The Tax-Farming Deal', 'Откуп налогов'),
 q('有豪商愿预付三年税金承包税吏，账面好看，民心难说。', '有豪商願預付三年稅金承包稅吏，賬面好看，民心難說。', 'A magnate offers three years of taxes up front to farm the collectors — good ledger, risky hearts.', 'Магнат предлагает три года налогов вперёд — счёт хорош, сердца рискованны.'),
 q('许其承包', '許其承包', 'Grant the farm', 'Отдать на откуп'),
 q('税金先收，怨气后到。', '稅金先收，怨氣後到。', 'Take the gold now; the resentment arrives later.', 'Золото сейчас, ропот позже.'),
 q('三年税金一次入库，民间却多了不少咒骂。', '三年稅金一次入庫，民間卻多了不少咒罵。', 'Three years of tax fill the vault; curses fill the streets.', 'Три года налогов в казне; проклятия на улицах.'),
 q('驳回其议', '駁回其議', 'Reject it', 'Отклонить'),
 q('税吏依旧归王室节制，分文不让。', '稅吏依舊歸王室節制，分文不讓。', 'The collectors stay under the crown, coin by coin.', 'Сборщики остаются у короны, монета за монетой.'),
 q('包税之议被驳回，王室稳住了吏治。', '包稅之議被駁回，王室穩住了吏治。', 'The deal is refused; the crown keeps clean hands.', 'Сделка отклонена; корона чиста.'))
ev('war_bonds', 2,
 q('战争债券', '戰爭債券', 'War Bonds', 'Военные облигации'),
 q('战事吃紧，户部献策：向国民发债筹饷，胜利后兑付。', '戰事吃緊，戶部獻策：向國民發債籌餉，勝利後兌付。', 'The war strains the purse: issue bonds to the people, redeem on victory.', 'Война сушит казну: выпустить облигации для народа.'),
 q('发行债券', '發行債券', 'Issue bonds', 'Выпустить облигации'),
 q('国民踊跃认购，军饷一时充盈。', '國民踴躍認購，軍餉一時充盈。', 'The people subscribe eagerly; the war chest fills.', 'Народ подписывается охотно; военная казна полна.'),
 q('债券募得军饷，前线士气大振。', '債券募得軍餉，前線士氣大振。', 'The bonds buy supplies; morale at the front soars.', 'Облигации дали харчи; дух фронта высок.'),
 q('拒绝发债', '拒絕發債', 'Refuse', 'Отказаться'),
 q('不愿欠国民的债，军费只能一省再省。', '不願欠國民的債，軍費只能一省再省。', 'No debts to the people; the war purse is cut to the bone.', 'Не влезать в долги к народу; военные траты урезаны.'),
 q('军费捉襟见肘，营中颇有微词。', '軍費捉襟見肘，營中頗有微詞。', 'The thin purse breeds grumbling in camp.', 'Тонкий кошелёк — ропот в лагере.'))
ev('land_survey', 2,
 q('清丈田亩', '清丈田畝', 'The Land Survey', 'Межевание земель'),
 q('户部请旨清丈全国田亩，隐田匿赋无所遁形。', '戶部請旨清丈全國田畝，隱田匿賦無所遁形。', 'The treasury asks leave to survey every field; hidden land, hidden tax — no more.', 'Казна просит измерить все поля; скрытые земли кончатся.'),
 q('准奏清丈', '准奏清丈', 'Approve the survey', 'Одобрить'),
 q('先拨勘验费，来年税基更实。', '先撥勘驗費，來年稅基更實。', 'Pay the surveyors now; next year the tax base is honest.', 'Заплатить землемерам; в будущем году база налогов честная.'),
 q('田亩清丈完毕，隐匿的赋税尽数归仓。', '田畝清丈完畢，隱匿的賦稅盡數歸倉。', 'The survey is done; hidden taxes flow to the vault.', 'Межевание окончено; скрытые налоги в казне.'),
 q('暂缓推行', '暫緩推行', 'Defer it', 'Отложить'),
 q('动静太大，{king}选择不动祖产旧账。', '動靜太大，{king}選擇不動祖產舊賬。', 'Too great a stir — {king} leaves the old ledgers alone.', 'Слишком большой шум — {king} не трогает старые книги.'),
 q('清丈之议搁置，田亩旧账依旧一团糊涂。', '清丈之議擱置，田畝舊賬依舊一團糊塗。', 'The survey is shelved; the old ledgers stay a muddle.', 'Межевание отложено; книги в прежнем хаосе.'))
ev('guild_bank', 2,
 q('行会钱庄', '行會錢莊', 'The Guild Bank', 'Банк гильдии'),
 q('手工业行会欲设钱庄放贷，请求王室入股分红。', '手工業行會欲設錢莊放貸，請求王室入股分紅。', 'The craft guilds want a lending bank and offer the crown a share.', 'Ремесленные гильдии хотят банк и зовут корону в долю.'),
 q('王室入股', '王室入股', 'Invest the crown', 'Вложить казну'),
 q('出资入股，坐收红利——也与行会绑上了一条船。', '出資入股，坐收紅利——也與行會綁上了一條船。', 'Invest and take dividends — and share the guilds\' boat.', 'Вложить и получать дивиденды — в одной лодке с гильдией.'),
 q('钱庄开张，王室每年坐分红利。', '錢莊開張，王室每年坐分紅利。', 'The bank opens; the crown collects yearly.', 'Банк открыт; корона получает свою долю.'),
 q('婉拒入股', '婉拒入股', 'Decline', 'Отклонить'),
 q('不放王室的名声在钱庄生意上，转而济贫安民。', '不放王室的名聲在錢莊生意上，轉而濟貧安民。', 'Keep the crown\'s name off moneylending; comfort the poor instead.', 'Не марать имя короны ростовщичеством — утешить бедных.'),
 q('王室婉拒入股，转手把心思放在了济贫上。', '王室婉拒入股，轉手把心思放在了濟貧上。', 'The crown declines and turns to charity instead.', 'Корона отказалась и занялась бедными.'))

# ============ 天灾 ============
ev('flood', 2,
 q('河水泛滥', '河水泛濫', 'The Flood', 'Наводнение'),
 q('连日暴雨，河水漫过堤岸，低洼村镇一片汪洋。', '連日暴雨，河水漫過堤岸，低窪村鎮一片汪洋。', 'Days of rain; the river swallows the lowland villages.', 'Дни дождей; река поглотила низинные деревни.'),
 q('抢修堤坝', '搶修堤壩', 'Repair the dikes', 'Чинить дамбы'),
 q('征发人手昼夜抢修，保住下游。', '徵發人手晝夜搶修，保住下游。', 'Put every hand on the dikes to save the lowlands.', 'Все руки на дамбы — спасти низины.'),
 q('堤坝合龙，洪水退去，{kingdom}守住了家园。', '堤壩合龍，洪水退去，{kingdom}守住了家園。', 'The dikes hold; the flood recedes and {kingdom} keeps its homes.', 'Дамбы устояли; наводнение спало.'),
 q('任其泛滥', '任其泛濫', 'Let it be', 'Оставить как есть'),
 q('灾民四散，怨言随水漫进每一座城。', '災民四散，怨言隨水漫進每一座城。', 'The displaced scatter, carrying anger to every city.', 'Беженцы разнесли злость по городам.'),
 q('洪水退去后，废墟与怨言一起留在了河岸。', '洪水退去後，廢墟與怨言一起留在了河岸。', 'The flood leaves ruins and grudge on the banks.', 'Наводнение оставило руины и злобу.'))
ev('wildfire', 2,
 q('山火蔓延', '山火蔓延', 'The Wildfire', 'Пожар'),
 q('天干物燥，山火借风势直逼村落与林场。', '天乾物燥，山火借風勢直逼村落與林場。', 'Dry winds push the wildfire toward villages and timber.', 'Сухой ветер гонит пожар к деревням и лесу.'),
 q('伐木开隔离带', '伐木開隔離帶', 'Cut firebreaks', 'Рубить просеки'),
 q('征伐木匠开隔离带，保住林场。', '徵伐木匠開隔離帶，保住林場。', 'Pay woodcutters to cut firebreaks and save the timber.', 'Заплатить лесорубам за просеки — лес спасён.'),
 q('隔离带拦住火头，林场无损。', '隔離帶攔住火頭，林場無損。', 'The firebreaks stop the flames; the timber survives.', 'Просеки остановили огонь; лес цел.'),
 q('听火自灭', '聽火自滅', 'Let it burn', 'Пусть горит'),
 q('不花一钱，任火自生自灭。', '不花一錢，任火自生自滅。', 'Spend nothing; let the fire burn out.', 'Ни монеты; пусть огонь догорит сам.'),
 q('火过之处一片焦土，村人哭声连野。', '火過之處一片焦土，村人哭聲連野。', 'The burn leaves ash fields and weeping villagers.', 'Пожар оставил пепел и плач.'))
ev('blizzard', 2,
 q('暴风雪', '暴風雪', 'The Blizzard', 'Метель'),
 q('暴雪连宵，牲畜冻毙于野，粮道为雪所阻。', '暴雪連宵，牲畜凍斃於野，糧道為雪所阻。', 'The blizzard kills herds and blocks the grain roads.', 'Метель губит стада и запирает хлебные дороги.'),
 q('开仓赊粮', '開倉賒糧', 'Open credit granaries', 'Открыть амбары в кредит'),
 q('赊粮给各家熬冬，开春再还。', '賒糧給各家熬冬，開春再還。', 'Lend grain for the winter, repay in spring.', 'Дать зерно в долг до весны.'),
 q('各家熬过寒冬，来年如数还粮。', '各家熬過寒冬，來年如數還糧。', 'The winter is survived; the grain returns in spring.', 'Зиму пережили; зерно вернулось весной.'),
 q('闭门避雪', '閉門避雪', 'Shelter in place', 'Переждать дома'),
 q('省下仓粮，冻毙者不计其数。', '省下倉糧，凍斃者不計其數。', 'Save the grain; count the frozen instead.', 'Сберечь зерно; сосчитать замёрзших.'),
 q('雪停之后，村村都有白事。', '雪停之後，村村都有白事。', 'When the snow stops, every village mourns.', 'Когда снег сошёл, все деревни плакали.'))
ev('mine_collapse', 2,
 q('矿坑塌陷', '礦坑塌陷', 'The Mine Collapse', 'Обвал в шахте'),
 q('矿坑深夜塌陷，矿工被困井下，家属围聚坑口。', '礦坑深夜塌陷，礦工被困井下，家屬圍聚坑口。', 'The mine caves in at night; families crowd the pit mouth.', 'Шахта обрушилась ночью; семьи у входа.'),
 q('全力施救', '全力施救', 'Dig them out', 'Спасать изо всех сил'),
 q('不计成本掘井救人，抚恤遗属。', '不計成本掘井救人，撫恤遺屬。', 'Spare no coin digging and comfort the widows.', 'Не считать затрат — спасать и утешить.'),
 q('被困矿工大多获救，{kingdom}上下称义。', '被困礦工大多獲救，{kingdom}上下稱義。', 'Most miners are pulled out alive; {kingdom} is praised.', 'Большинство спасено; {kingdom} хвалят.'),
 q('封坑止损', '封坑止損', 'Seal the pit', 'Запечатать шахту'),
 q('放弃被困者，封坑保住矿脉。', '放棄被困者，封坑保住礦脈。', 'Abandon the trapped to save the seam.', 'Бросить людей ради жилы.'),
 q('坑口封石之日，哭声三日不绝。', '坑口封石之日，哭聲三日不絕。', 'When the pit is sealed, the weeping lasts three days.', 'Когда шахту замуровали, плач шёл три дня.'))
ev('river_dry', 2,
 q('河道干涸', '河道乾涸', 'The Dying River', 'Пересохшая река'),
 q('主河道断流，水车停转，两岸稻田龟裂。', '主河道斷流，水車停轉，兩岸稻田龜裂。', 'The river fails; wheels stop and rice fields crack.', 'Река иссякла; колёса стоят, рис трескается.'),
 q('开渠引水', '開渠引水', 'Cut a channel', 'Копать канал'),
 q('征民夫开渠引上游活水。', '徵民夫開渠引上游活水。', 'Put crews on a channel from the upper waters.', 'Отрядить людей за водой с верховий.'),
 q('渠成水到，两岸稻田得救。', '渠成水到，兩岸稻田得救。', 'The channel runs; the rice fields live.', 'Канал заработал; рис спасён.'),
 q('改种旱作', '改種旱作', 'Switch to dry crops', 'Перейти на сухие культуры'),
 q('改种耐旱杂粮，收成减半但不必破费。', '改種耐旱雜糧，收成減半但不必破費。', 'Plant hardy crops — half the harvest, no expense.', 'Сухие культуры — половина урожая без затрат.'),
 q('改种之田收成减半，乡民默默承受。', '改種之田收成減半，鄉民默默承受。', 'The dry fields yield half; the villages endure.', 'Сухие поля дали половину; деревни терпят.'))
ev('rockslide', 2,
 q('山体滑坡', '山體滑坡', 'The Rockslide', 'Оползень'),
 q('连日阴雨后山体滑坡，埋了商道必经的隘口。', '連日陰雨後山體滑坡，埋了商道必經的隘口。', 'After days of rain, a slide buries the pass on the trade road.', 'После дождей оползень запер торговый перевал.'),
 q('征集石匠清障', '徵集石匠清障', 'Hire stonecutters', 'Нанять каменотёсов'),
 q('出钱雇石匠开山清道，商路早日复通。', '出錢雇石匠開山清道，商路早日復通。', 'Pay stonecutters to clear the pass quickly.', 'Заплатить за быструю расчистку.'),
 q('隘口复通，过往商队交口称赞。', '隘口復通，過往商隊交口稱讚。', 'The pass reopens to passing praise.', 'Перевал открыт; караваны хвалят.'),
 q('绕行山脊', '繞行山脊', 'Detour the ridge', 'Обойти по гребню'),
 q('不花钱清障，商队绕行多走十日。', '不花錢清障，商隊繞行多走十日。', 'Save the coin; caravans detour ten days.', 'Сэкономить; караваны пойдут кружным путём десять дней.'),
 q('商路绕行十日，商税锐减。', '商路繞行十日，商稅銳減。', 'The detour costs ten days and the tolls shrink.', 'Кружной путь съел десять дней и пошлины.'))

# ============ 宫廷（全部 onlyPlayer）============
ev('heir_plot', 3,
 q('王子密谋', '王子密謀', 'The Heir\'s Plot', 'Заговор наследника'),
 q('密报传来：{king}的王子暗中结交将领，图谋不轨。宫闱之内，刀已出鞘。', '密報傳來：{king}的王子暗中結交將領，圖謀不軌。宮闈之內，刀已出鞘。', 'Secret word: {king}\'s heir courts the generals. The blade is halfway from its sheath.', 'Донесение: наследник {king} подкупает генералов. Клинок наполовину вынут.'),
 q('雷霆锁拿', '雷霆鎖拿', 'Arrest him', 'Арестовать'),
 q('即刻锁拿问罪——朝野震动，后患立除。', '即刻鎖拿問罪——朝野震動，後患立除。', 'Arrest him at once — the court trembles, but the threat dies today.', 'Схватить немедленно — двор содрогнётся, но угроза умрёт.'),
 q('图谋败露，{king}的雷霆手段震住朝野。', '圖謀敗露，{king}的雷霆手段震住朝野。', 'The plot is crushed with an iron hand.', 'Заговор раздавлен железной рукой.'),
 q('怀柔笼络', '懷柔籠絡', 'Buy his loyalty', 'Задобрить его'),
 q('厚赏安抚，把野心喂饱。', '厚賞安撫，把野心餵飽。', 'Gold and honors to feed his ambition.', 'Золото и почести, чтобы накормить честолюбие.'),
 q('重赏之下王子暂时安分，但野心未必喂得饱。', '重賞之下王子暫時安分，但野心未必餵得飽。', 'Gold quiets the heir — for now.', 'Золото утишило наследника — пока.'),
 q('假意不知，暗布眼线', '假意不知，暗布眼線', 'Watch and wait', 'Наблюдать и ждать'),
 q('佯装不知，暗设罗网——打草惊蛇最忌。', '佯裝不知，暗設羅網——打草驚蛇最忌。', 'Feign blindness and set the net; never startle the snake.', 'Слепить глаза и ставить сеть — не спугнуть змею.'),
 q('{king}静观其变，网已张开。', '{king}靜觀其變，網已張開。', '{king} watches and waits; the net is set.', '{king} ждёт; сеть расставлена.'))
ev('heir_plot_crushed', 2,
 q('余党清算', '餘黨清算', 'Loose Ends', 'Концы заговора'),
 q('王子一党既溃，其党羽还散布在朝中各地，如何处置？', '王子一黨既潰，其黨羽還散布在朝中各地，如何處置？', 'The heir\'s faction is broken, but his men remain in every office. What now?', 'Фракция наследника разбита, но её люди в каждой конторе. Что дальше?'),
 q('罚没家产，充入国库', '罰沒家產，充入國庫', 'Confiscate their estates', 'Конфисковать имения'),
 q('株连罚没，国库为之一实。', '株連罰沒，國庫為之一實。', 'Confiscate across the board; the treasury swells.', 'Конфисковать всех; казна пополнена.'),
 q('抄没的田产充入国库，一时称盛。', '抄沒的田產充入國庫，一時稱盛。', 'The seized estates fill the treasury.', 'Изъятые имения наполнили казну.'),
 q('法外开恩，只除首恶', '法外開恩，只除首惡', 'Spare the small fish', 'Пощадить мелких'),
 q('只办首恶，余者赦之，朝野感念宽仁。', '只辦首惡，餘者赦之，朝野感念寬仁。', 'Punish only the chiefs; mercy earns gratitude.', 'Наказать только главных; милость запомнят.'),
 q('宽仁之名传遍列国，人心归附。', '寬仁之名傳遍列國，人心歸附。', 'Mercy is remembered in every court.', 'Милость запомнили все дворы.'))
ev('concubine_rivalry', 2,
 q('嫔妃争宠', '嬪妃爭寵', 'The Rival Consorts', 'Соперницы'),
 q('两位嫔妃争宠斗法，后宫乱成一团，骂声传到了前朝。', '兩位嬪妃爭寵鬥法，後宮亂成一團，罵聲傳到了前朝。', 'Two consorts feud openly; the quarrels reach the throne room.', 'Две наложницы грызутся; ссоры слышны в тронном зале.'),
 q('厚赏平息', '厚賞平息', 'Buy peace', 'Купить мир'),
 q('重赏两头，勉强摆平。', '重賞兩頭，勉強擺平。', 'Gifts for both — an uneasy peace.', 'Подарки обеим — шаткий мир.'),
 q('后宫暂安，库房却轻了一圈。', '後宮暫安，庫房卻輕了一圈。', 'The harem quiets; the vault feels lighter.', 'Гарем стих; казна похудела.'),
 q('各打五十大板', '各打五十大板', 'Punish them both', 'Наказать обеих'),
 q('降位罚俸，立后宫规矩。', '降位罰俸，立後宮規矩。', 'Demote and fine them both; restore order.', 'Разжаловать и оштрафовать — порядок восстановлен.'),
 q('铁腕立规，后宫从此噤声。', '鐵腕立規，後宮從此噤聲。', 'Iron rules hush the harem.', 'Жёсткие правила утихомирили гарем.'))
ev('royal_astrologer', 2,
 q('占星官进言', '佔星官進言', 'The Court Astrologer', 'Придворный астролог'),
 q('占星官夜观天象，称将有大吉之兆，然须王室出资设坛祭星。', '占星官夜觀天象，稱將有大吉之兆，然須王室出資設壇祭星。', 'The astrologer reads a great omen in the stars — for a price to build the altar.', 'Астролог видит великое знамение — за плату за алтарь.'),
 q('出资设坛', '出資設壇', 'Fund the rite', 'Оплатить обряд'),
 q('宁信其有，花钱买个心安。', '寧信其有，花錢買個心安。', 'Better safe than sorry — pay for peace of mind.', 'Лучше перестраховаться — купить покой.'),
 q('祭坛立起，国人皆言天命在{kingdom}。', '祭壇立起，國人皆言天命在{kingdom}。', 'The altar stands; all say heaven favors {kingdom}.', 'Алтарь воздвигнут; все говорят — небо за {kingdom}.'),
 q('逐出占星官', '逐出占星官', 'Banish him', 'Изгнать его'),
 q('斥为妖言，逐出宫廷——士人叫好，巫祝寒心。', '斥為妖言，逐出宮廷——士人叫好，巫祝寒心。', 'Call it sorcery and banish him; scholars cheer, priests seethe.', 'Обозвать чепухой и изгнать; учёные рады, жрецы в ярости.'),
 q('占星官被逐，钦天监从此不敢妄言。', '占星官被逐，欽天監從此不敢妄言。', 'The astrologer is gone; the stargazers mind their tongues.', 'Астролог изгнан; звездочёты держат язык за зубами.'))
ev('hunt_accident', 3,
 q('猎场惊驾', '獵場驚駕', 'The Hunt Accident', 'Несчастье на охоте'),
 q('秋狝围猎，{king}的马受惊狂奔，随驾一名平民被踏伤，围观者众。', '秋獮圍獵，{king}的馬受驚狂奔，隨駕一名平民被踏傷，圍觀者眾。', 'At the autumn hunt, {king}\'s horse bolts and tramples a commoner — before a crowd.', 'На осенней охоте конь {king} понёс и затоптал простолюдина — при всех.'),
 q('重金抚恤，亲自探视', '重金撫恤，親自探視', 'Generous compensation', 'Щедрая компенсация'),
 q('以王家之礼厚葬抚恤，并亲往其家致歉。', '以王家之禮厚葬撫恤，並親往其家致歉。', 'A royal funeral, rich compensation, and {king} bows to the family.', 'Царские похороны, щедрая плата и поклон семьи.'),
 q('万民称颂{king}仁德。', '萬民稱頌{king}仁德。', 'The people praise {king}\'s virtue.', 'Народ хвалит добродетель {king}.'),
 q('依律办理', '依律辦理', 'By the letter of the law', 'По закону'),
 q('交有司按律抚恤，不增不减。', '交有司按律撫恤，不增不減。', 'Let the magistrates pay the standard sum.', 'Чиновники выплатят по ставке.'),
 q('按律办事，无人有怨，亦无人称颂。', '按律辦事，無人有怨，亦無人稱頌。', 'The law is followed — no anger, no applause.', 'Закон соблюдён — без гнева и без славы.'),
 q('封口不理', '封口不理', 'Silence the matter', 'Замять дело'),
 q('驱散围观，压下消息。', '驅散圍觀，壓下消息。', 'Disperse the crowd and bury the story.', 'Разогнать толпу и похоронить историю.'),
 q('消息被压下，但总有人记得那天马蹄声。', '消息被壓下，但總有人記得那天馬蹄聲。', 'The story is buried — but the hoofbeats are remembered.', 'Историю замяли — но топот копыт помнят.'))
ev('old_regent', 2,
 q('摄政请旨', '攝政請旨', 'The Old Regent', 'Старый регент'),
 q('辅政多年的老摄政上表，请{king}赐其荣归故里——朝中大气都不敢出。', '輔政多年的老攝政上表，請{king}賜其榮歸故里——朝中大氣都不敢出。', 'The old regent who ruled for years asks leave to retire home — the court holds its breath.', 'Старый регент просит отпустить его на покой — двор затаил дыхание.'),
 q('厚赐荣归', '厚賜榮歸', 'Honor him home', 'Отпустить с почётом'),
 q('赐金帛仪仗，风光送归，全其体面。', '賜金帛儀仗，風光送歸，全其體面。', 'Gold, honors, and a grand farewell.', 'Золото, почести и пышные проводы.'),
 q('老臣含泪拜别，列国皆赞{king}恩义两全。', '老臣含淚拜別，列國皆讚{king}恩義兩全。', 'The old man leaves in tears; every court praises {king}\'s grace.', 'Старик уплакал; все дворы хвалят милость {king}.'),
 q('顺势收权', '順勢收權', 'Take back power', 'Забрать власть'),
 q('趁机收回大权，老臣怨望难平。', '趁機收回大權，老臣怨望難平。', 'Reclaim the reins; the old man\'s grudge runs deep.', 'Забрать поводья; обида старика глубока.'),
 q('权柄归王室，但朝中多了记恨的老臣。', '權柄歸王室，但朝中多了記恨的老臣。', 'Power returns to the crown — and a grudge to the court.', 'Власть у короны — и обида при дворе.'))
ev('bastard_claim', 2,
 q('私生子认祖', '私生子認祖', 'The Bastard\'s Claim', 'Бастард требует имени'),
 q('一名青年持旧信物自称{king}血脉，要求认入宗籍。', '一名青年持舊信物自稱{king}血脈，要求認入宗籍。', 'A young man with an old token claims {king}\'s blood and a place in the line.', 'Юноша со старым знаком называет себя кровью {king}.'),
 q('验明认入', '驗明認入', 'Acknowledge him', 'Признать его'),
 q('验明正身后认入宗籍，给名分也给是非。', '驗明正身後認入宗籍，給名分也給是非。', 'Verify and acknowledge him — the name and the scandal together.', 'Проверить и признать — имя и скандал вместе.'),
 q('认祖归宗成了，市井议论纷纷。', '認祖歸宗成了，市井議論紛紛。', 'He is acknowledged; the markets buzz.', 'Его признали; базар гудит.'),
 q('斥为诈骗，逐出王畿', '斥為詐騙，逐出王畿', 'Call it fraud, expel him', 'Обозвать мошенником и выгнать'),
 q('指其伪造信物，乱棍逐出。', '指其偽造信物，亂棍逐出。', 'Denounce the token as forged and drive him out.', 'Объявить подделкой и выгнать.'),
 q('青年被逐出王畿，临行前高喊冤枉。', '青年被逐出王畿，臨行前高喊冤枉。', 'He is expelled, crying injustice at the gates.', 'Его выгнали, крича о неправде у ворот.'))
ev('spy_ring', 2,
 q('密探网络', '密探網絡', 'The Spy Ring', 'Сеть шпионов'),
 q('内廷建言：设密探网监视列国使节与本国重臣。', '內廷建言：設密探網監視列國使節與本國重臣。', 'The inner court proposes a spy ring over envoys and ministers.', 'Внутренний двор предлагает сеть шпионов за послами и сановниками.'),
 q('拨款设网', '撥款設網', 'Fund the ring', 'Оплачивать сеть'),
 q('出钱养耳目，列国动静尽入{king}耳中。', '出錢養耳目，列國動靜盡入{king}耳中。', 'Pay for ears everywhere; all lands whisper to {king}.', 'Платить за уши; все земли шепчут {king}.'),
 q('耳目四布，{king}先知先觉。', '耳目四布，{king}先知先覺。', 'The ears are everywhere; {king} hears first.', 'Уши повсюду; {king} слышит первым.'),
 q('斥为不德', '斥為不德', 'Refuse as unbecoming', 'Отвергнуть как недостойное'),
 q('王室行事要光明正大，密探之流不配{kingdom}。', '王室行事要光明正大，密探之流不配{kingdom}。', 'The crown acts in daylight; spies are beneath {kingdom}.', 'Корона действует при свете дня; шпионы недостойны {kingdom}.'),
 q('密探之议作罢，{king}守住了一条底线。', '密探之議作罷，{king}守住了一條底線。', 'The proposal dies; {king} keeps one line uncrossed.', 'Предложение отклонено; {king} не перешёл черту.'))

# ============ 军事 ============
ev('war_defector', 3,
 q('敌将投诚', '敵將投誠', 'The Defector', 'Перебежчик'),
 q('敌国一员宿将阵前来投，愿献其所知——也带着没人说得清的心思。', '敵國一員宿將陣前來投，願獻其所知——也帶著沒人說得清的心思。', 'A veteran general defects with secrets — and intentions nobody can read.', 'Ветеран врага перебегает с секретами — и с неясными замыслами.'),
 q('绑送军前立威', '綁送軍前立威', 'Bind him as a trophy', 'Связать его для устрашения'),
 q('绑送阵前斩首立威，敌胆必寒。', '綁送陣前斬首立威，敵膽必寒。', 'Execute him before the lines; the enemy will shudder.', 'Казнить перед строем; враг содрогнётся.'),
 q('阵前斩使，敌军震恐。', '陣前斬使，敵軍震恐。', 'The execution chills the enemy army.', 'Казнь ужаснула вражескую армию.'),
 q('重用其才', '重用其才', 'Take his service', 'Принять его службу'),
 q('以礼相待纳入帐下，其才可用其心难测。', '以禮相待納入帳下，其才可用其心難測。', 'Honor him and take his talents — with his heart untested.', 'Принять с почётом — талант ценен, сердце неизвестно.'),
 q('叛将来投，{king}亲自帐前相迎。', '叛將來投，{king}親自帳前相迎。', '{king} greets the defector at the command tent.', '{king} встретил перебежчика у шатра.'),
 q('礼送出境', '禮送出境', 'Send him away politely', 'Вежливо выпроводить'),
 q('不纳降将，礼送出境以全信义。', '不納降將，禮送出境以全信義。', 'Refuse the defector; keep the honor clean.', 'Не принимать перебежчиков — честь чище.'),
 q('降将被礼送出境，两军皆称{king}有古风。', '降將被禮送出境，兩軍皆稱{king}有古風。', 'Sent away with honors; both armies call {king} old-fashioned honorable.', 'Его выпроводили с почётом; обе армии ценят благородство.'))
ev('defector_ambition', 2,
 q('降将坐大', '降將坐大', 'The Defector\'s Ambition', 'Честолюбие перебежчика'),
 q('投诚的降将拥兵自重，渐有不听号令之势——当年留下的祸根发芽了。', '投誠的降將擁兵自重，漸有不聽號令之勢——當年留下的禍根發芽了。', 'The defector has grown his own power and obeys reluctantly. The old seed sprouts.', 'Перебежчик оброс силой и слушается неохотно. Старое семя проросло.'),
 q('先发制人削其兵', '先發制人削其兵', 'Strike first, break his force', 'Ударить первым и отобрать войска'),
 q('不惜代价收其兵权，永绝后患。', '不惜代價收其兵權，永絕後患。', 'Pay any price to break his command for good.', 'Заплатить что угодно — отобрать войско навсегда.'),
 q('兵权收回，隐患尽除。', '兵權收回，隱患盡除。', 'His command is broken; the danger ends.', 'Войско отобрано; угроза кончилась.'),
 q('隐忍暂避', '隱忍暫避', 'Bide your time', 'Переждать'),
 q('强攻恐生内乱，暂且隐忍。', '強攻恐生內亂，暫且隱忍。', 'A purge risks civil strife — endure for now.', 'Чистка грозит смутой — терпеть.'),
 q('隐忍换得一时安稳，可谁都看得出裂痕。', '隱忍換得一時安穩，可誰都看得出裂痕。', 'The calm holds — and everyone sees the crack.', 'Тишь держится — но трещину видят все.'))
ev('supply_convoy', 2,
 q('粮道遇袭', '糧道遇襲', 'The Convoy Ambush', 'Засада на обоз'),
 q('前线粮道遇袭，押粮官哀求增派护卫。', '前線糧道遇襲，押糧官哀求增派護衛。', 'The supply convoy is ambushed; the escort begs for reinforcements.', 'Обоз атакован; конвой молит о подмоге.'),
 q('增派护卫', '增派護衛', 'Send guards', 'Послать охрану'),
 q('从城中抽调人手护送粮道。', '從城中抽調人手護送糧道。', 'Pull men from the city to guard the grain roads.', 'Взять людей из города на охрану обоза.'),
 q('粮道复通，前线军心已定。', '糧道復通，前線軍心已定。', 'The roads reopen; the front steadies.', 'Обозы пошли; фронт успокоился.'),
 q('无兵可派', '無兵可派', 'No men to spare', 'Людей нет'),
 q('婉言回绝，让押粮官自求多福。', '婉言回絕，讓押糧官自求多福。', 'Refuse and wish the convoy luck.', 'Отказать и пожелать удачи.'),
 q('粮道再袭，前线断了三日军粮。', '糧道再襲，前線斷了三日軍糧。', 'The convoy is hit again; the front starves three days.', 'Обоз снова атакован; фронт три дня без хлеба.'))
ev('fort_rebuild', 2,
 q('残堡重建', '殘堡重建', 'Rebuild the Old Fort', 'Отстроить старый форт'),
 q('边境线上的老堡垒残破不堪，守将请旨重修。', '邊境線上的老堡壘殘破不堪，守將請旨重修。', 'The border fort is falling apart; the warden asks to rebuild.', 'Пограничный форт рушится; комендант просит отстройки.'),
 q('拨专款重建', '撥專款重建', 'Fund the rebuild', 'Выделить средства'),
 q('重金重建棱堡，边民安枕。', '重金重建稜堡，邊民安枕。', 'Pour gold into the bastion; the border sleeps easy.', 'Золото в бастион — граница спит спокойно.'),
 q('堡垒焕然一新，边民夜夜安枕。', '堡壘煥然一新，邊民夜夜安枕。', 'The fort stands new; the border sleeps.', 'Форт как новый; граница спит.'),
 q('暂缓工事', '暫緩工事', 'Postpone the works', 'Отложить работы'),
 q('边务从缓，把钱花在别处。', '邊務從緩，把錢花在別處。', 'Border works can wait; spend elsewhere.', 'Пограничные дела подождут; тратить в другое место.'),
 q('堡垒依旧残破，边民夜里听得见风声。', '堡壘依舊殘破，邊民夜裡聽得見風聲。', 'The fort stays broken; the border hears wolves at night.', 'Форт ветшает; на границе слышны волки.'))
ev('veteran_company', 2,
 q('老兵请愿', '老兵請願', 'The Veterans\' Petition', 'Ходатайство ветеранов'),
 q('一批解甲老兵请愿：愿组一个常年戍边的老兵连，只要一份军饷。', '一批解甲老兵請願：願組一個常年戍邊的老兵連，只要一份軍餉。', 'Old soldiers ask to form a standing veteran company — for ordinary pay.', 'Старые солдаты просят роту ветеранов — за обычное жалованье.'),
 q('准组老兵连', '准組老兵連', 'Charter the company', 'Дать роту'),
 q('军饷照发，老兵守边。', '軍餉照發，老兵守邊。', 'Pay them and let the veterans hold the border.', 'Платить жалованье — ветераны держат границу.'),
 q('老兵连戍边，边境盗匪绝迹。', '老兵連戍邊，邊境盜匪絕跡。', 'The veterans hold the border; banditry dies.', 'Ветераны на границе; разбой исчез.'),
 q('婉言谢绝', '婉言謝絕', 'Decline kindly', 'Вежливо отказать'),
 q('国库吃紧，请老兵们解甲归田。', '國庫吃緊，請老兵們解甲歸田。', 'The purse is thin; send the old soldiers home.', 'Кошелёк тонок; отпустить солдат по домам.'),
 q('老兵散归田里，酒馆里多了许多当年勇。', '老兵散歸田裡，酒館裡多了許多當年勇。', 'The veterans go home; the taverns gain old war stories.', 'Ветераны по домам; в тавернах — старые байки.'))
ev('privateer_licence', 2,
 q('私掠许可', '私掠許可', 'The Privateer Licence', 'Каперский патент'),
 q('海盗头目献金请领私掠许可，愿以劫掠敌国船队为报。', '海盜頭目獻金請領私掠許可，願以劫掠敵國船隊為報。', 'A pirate chief offers gold for a privateer licence to prey on enemy shipping.', 'Пиратский вожак предлагает золото за патент грабить вражеские корабли.'),
 q('颁发许可', '頒發許可', 'Grant the licence', 'Выдать патент'),
 q('收钱发证，{kingdom}的海上多了群有编制的狼。', '收錢發證，{kingdom}的海上多了群有編制的狼。', 'Take the gold; {kingdom}\'s seas gain licensed wolves.', 'Взять золото; у морей {kingdom} — лицензированные волки.'),
 q('私掠船下水，库金与骂名一同入账。', '私掠船下水，庫金與罵名一同入賬。', 'The privateers sail; gold and notoriety arrive together.', 'Каперы вышли; золото и дурная слава вместе.'),
 q('焚船驱盗', '焚船驅盜', 'Burn their ships', 'Сжечь их корабли'),
 q('断然拒绝，焚其舟以正视听。', '斷然拒絕，焚其舟以正視聽。', 'Refuse and burn their ships as an example.', 'Отказать и сжечь корабли в назидание.'),
 q('海盗船化为火炬，海上商路清净了。', '海盜船化為火炬，海上商路清淨了。', 'The pirate ships burn like torches; the sea lanes are clean.', 'Пиратские корабли как факелы; морские пути чисты.'))
ev('hero_funeral', 2,
 q('英雄葬礼', '英雄葬禮', 'The Hero\'s Funeral', 'Похороны героя'),
 q('戍边一生的老将星陨，举国哀恸，葬礼规格举棋未定。', '戍邊一生的老將星隕，舉國哀慟，葬禮規格舉棋未定。', 'The old warden of the border has died; the nation mourns and debates the rites.', 'Старый страж границы умер; страна скорбит, споря об обрядах.'),
 q('国葬规格', '國葬規格', 'A state funeral', 'Государственные похороны'),
 q('以国葬送行，费用王室全出。', '以國葬送行，費用王室全出。', 'A full state funeral at the crown\'s expense.', 'Государственные похороны за счёт короны.'),
 q('万民沿街送别，列国使节亦至。', '萬民沿街送別，列國使節亦至。', 'The whole nation lines the streets; envoys attend.', 'Народ заполнил улицы; прибыли послы.'),
 q('从简下葬', '從簡下葬', 'A quiet burial', 'Тихое погребение'),
 q('遗愿从简，一切依老将遗命。', '遺願從簡，一切依老將遺命。', 'Honor his will: a simple grave.', 'Воля героя: простая могила.'),
 q('老将长眠于普通墓穴，碑上无名，心中有名。', '老將長眠於普通墓穴，碑上無名，心中有名。', 'The old general sleeps in a nameless grave — remembered anyway.', 'Старый полководец в безымянной могиле — но памятен всем.'))

# ============ 民生 ============
ev('market_fire', 2,
 q('市场大火', '市場大火', 'The Market Fire', 'Пожар на рынке'),
 q('夜半市场走水，半个市集化为焦土，商户血本无归。', '夜半市場走水，半個市集化為焦土，商戶血本無歸。', 'A night fire guts half the market; the merchants lose everything.', 'Ночной пожар спалил полрынка; торговцы разорены.'),
 q('王室助建', '王室助建', 'Rebuild with crown gold', 'Отстроить за счёт короны'),
 q('出王库金助商户重建市集。', '出王庫金助商戶重建市集。', 'Crown gold rebuilds the market.', 'Золото короны отстроит рынок.'),
 q('新市集开张，比从前更热闹。', '新市集開張，比從前更熱鬧。', 'The new market opens busier than before.', 'Новый рынок шумнее прежнего.'),
 q('听商户自立', '聽商戶自立', 'Let them manage', 'Пусть справятся сами'),
 q('市集自建，王室不出一钱。', '市集自建，王室不出一錢。', 'The merchants rebuild alone; the crown pays nothing.', 'Торговцы отстроятся сами; корона не платит.'),
 q('市集草草复业，商户见了税吏就叹气。', '市集草草復業，商戶見了稅吏就嘆氣。', 'A shabby market returns; merchants sigh at tax time.', 'Рынок отстроили кое-как; торговцы вздыхают при налогах.'))
ev('water_shortage', 2,
 q('水荒', '水荒', 'The Water Shortage', 'Нужда в воде'),
 q('入夏少雨，井水见底，城中日日排长队打水。', '入夏少雨，井水見底，城中日日排長隊打水。', 'A dry summer empties the wells; lines form daily in the city.', 'Сухое лето; колодцы пусты, очереди за водой.'),
 q('开凿深井引渠', '開鑿深井引渠', 'Dig deep wells', 'Копать глубокие колодцы'),
 q('重金请工匠凿深井、修引水渠。', '重金請工匠鑿深井、修引水渠。', 'Pay wellmasters to dig deep and channel water.', 'Щедро заплатить за колодцы и водоканалы.'),
 q('清泉入城，水荒立解。', '清泉入城，水荒立解。', 'Sweet water reaches the city; the shortage ends.', 'Чистая вода в городе; нужда кончилась.'),
 q('限量供水', '限量供水', 'Ration the water', 'Нормировать воду'),
 q('按户限量供水，省工省料但怨声四起。', '按戶限量供水，省工省料但怨聲四起。', 'Ration by household — cheap, but unpopular.', 'Норма на двор — дёшево, но непопулярно.'),
 q('水配给了，排队的人少了，抱怨的人多了。', '水配給了，排隊的人少了，抱怨的人多了。', 'Rations cut the queues and raised the complaints.', 'Нормы сократили очереди и подняли ропот.'))
ev('tenant_strike', 2,
 q('佃户抗租', '佃戶抗租', 'The Tenant Strike', 'Бунт арендаторов'),
 q('田租连涨，佃户相约不缴，田庄告到王庭。', '田租連漲，佃戶相約不繳，田莊告到王庭。', 'Rents rose once too often; the tenants refuse to pay and the estates sue.', 'Оброк подняли лишний раз; арендаторы бастуют, помещики в суде.'),
 q('劝减田租', '勸減田租', 'Press for lower rents', 'Продавить снижение'),
 q('王室出面劝业主减租，息事宁人。', '王室出面勸業主減租，息事寧人。', 'The crown leans on the landlords to cut rents.', 'Корона надавит на помещиков снизить оброк.'),
 q('租约重订，佃户欢声雷动。', '租約重訂，佃戶歡聲雷動。', 'Rents are cut; the tenants cheer.', 'Оброк снижен; арендаторы ликуют.'),
 q('为王庭强制催缴', '為王庭強制催繳', 'Enforce collection', 'Взыскать по закону'),
 q('派员强制催缴，王法为先。', '派員強制催繳，王法為先。', 'Send officers to collect; law first.', 'Послать приставов; закон прежде всего.'),
 q('租银如数入庄，田埂上的眼神冷了下来。', '租銀如數入莊，田埂上的眼神冷了下來。', 'The rents arrive in full — and so do the cold stares.', 'Оброк собран — вместе с холодными взглядами.'))
ev('festival_request', 2,
 q('庆典请愿', '慶典請願', 'The Festival Petition', 'Просьба о празднике'),
 q('百姓联名请愿：丰年已至，请{king}准办一场全城庆典。', '百姓聯名請願：豐年已至，請{king}准辦一場全城慶典。', 'The people petition {king}: a good harvest deserves a city-wide festival.', 'Народ просит {king}: урожай удался — устроить праздник.'),
 q('钦定庆典', '欽定慶典', 'Proclaim the festival', 'Объявить праздник'),
 q('王室出资办庆典，与民同乐。', '王室出資辦慶典，與民同樂。', 'Crown gold, public joy.', 'Золото короны — радость народа.'),
 q('庆典三日，举城若狂，人人念{king}的好。', '慶典三日，舉城若狂，人人念{king}的好。', 'Three days of feasting; all speak well of {king}.', 'Три дня гулянья; все хвалят {king}.'),
 q('丰年不办', '豐年不辦', 'Save it for hard times', 'Сберечь на чёрный день'),
 q('好日子要过，钱要留到坏年景。', '好日子要過，錢要留到壞年景。', 'Good times can wait; save for the bad ones.', 'Хорошим временем потерпеть; беречь на лихолетье.'),
 q('请愿被婉拒，街谈巷议说{king}抠。', '請願被婉拒，街談巷議說{king}摳。', 'The petition is declined; the streets call {king} stingy.', 'Просьба отклонена; улицы зовут {king} скупым.'))
ev('night_patrol', 2,
 q('夜盗频发', '夜盜頻發', 'The Night Thieves', 'Ночные воры'),
 q('入夜盗贼横行，商铺接连失窃，商会请设夜巡。', '入夜盜賊橫行，商鋪接連失竊，商會請設夜巡。', 'Thieves rule the night; shops are robbed nightly and the guilds demand patrols.', 'Ночью хозяйничают воры; гильдии требуют дозоры.'),
 q('组建夜巡队', '組建夜巡隊', 'Charter the watch', 'Создать ночную стражу'),
 q('出钱养一支夜巡队，还商铺一个安稳夜。', '出錢養一支夜巡隊，還商鋪一個安穩夜。', 'Pay for a night watch and give the shops their sleep.', 'Оплачивать ночную стражу — магазинам спокойный сон.'),
 q('夜巡上岗，盗贼绝迹，商铺安心。', '夜巡上崗，盜賊絕跡，商鋪安心。', 'The watch walks; thieves vanish and shops rest.', 'Стража вышла; воры исчезли, лавки спят.'),
 q('让商户自保', '讓商戶自保', 'Let shops fend', 'Пусть лавки сами'),
 q('各商铺自雇护院，王室不出钱。', '各商鋪自雇護院，王室不出錢。', 'The shops hire their own guards; the crown pays nothing.', 'Лавки наймут свою стражу; корона не платит.'),
 q('护院各自为战，盗贼挑软处下手。', '護院各自為戰，盜賊挑軟處下手。', 'Scattered guards lose to organized thieves.', 'Разрозненная стража проигрывает ворам.'))
ev('bathhouse_fad', 2,
 q('澡堂风潮', '澡堂風潮', 'The Bathhouse Fad', 'Мода на бани'),
 q('城中忽然兴起澡堂风潮，士庶争相沐浴，行会请王室倡导扩建。', '城中忽然興起澡堂風潮，士庶爭相沐浴，行會請王室倡導擴建。', 'A bathhouse craze sweeps the city; the guilds ask the crown to champion more.', 'Город охватила мода на бани; гильдии просят корону поддержать.'),
 q('倡建新澡堂', '倡建新澡堂', 'Champion new baths', 'Поддержать новые бани'),
 q('王室倡导扩建，卫生之利泽被全城。', '王室倡導擴建，衛生之利澤被全城。', 'Champion the baths; health for the whole city.', 'Поддержать бани — здоровье всему городу.'),
 q('澡堂林立，{kingdom}以洁净闻于列国。', '澡堂林立，{kingdom}以潔淨聞於列國。', 'Baths everywhere; {kingdom} is famous for cleanliness.', 'Бани повсюду; {kingdom} славится чистотой.'),
 q('风潮自会退去', '風潮自會退去', 'Let the fad pass', 'Пусть мода пройдёт'),
 q('王不与民争澡堂，风潮自退。', '王不與民爭澡堂，風潮自退。', 'The crown doesn\'t chase fashions; they fade.', 'Корона не гонится за модой; мода пройдёт.'),
 q('风潮如期退去，只剩几家澡堂苦撑。', '風潮如期退去，只剩幾家澡堂苦撐。', 'The fad fades; a few baths struggle on.', 'Мода прошла; остались немногие бани.'))
ev('traveling_fair', 2,
 q('巡游集市', '巡遊集市', 'The Traveling Fair', 'Бродячая ярмарка'),
 q('一支巡游集市浩浩荡荡进城，请求王室准许开市抽税。', '一支巡遊集市浩浩蕩蕩進城，請求王室准許開市抽稅。', 'A traveling fair rolls into the city, asking leave to trade under crown tax.', 'Бродячая ярмарка входит в город, прося права торговать под коронным налогом.'),
 q('准许开市', '准許開市', 'Permit the fair', 'Разрешить ярмарку'),
 q('准其开市，王室按成抽税。', '准其開市，王室按成抽稅。', 'Permit the fair and take the crown share.', 'Разрешить и брать коронную долю.'),
 q('集市半月，税金与欢声一并入库。', '集市半月，稅金與歡聲一併入庫。', 'A fortnight of fair — taxes and cheers together.', 'Полмесяца ярмарки — налоги и смех вместе.'),
 q('婉拒入城', '婉拒入城', 'Turn them away', 'Отвернуть их'),
 q('恐扰市集秩序，婉拒其入城。', '恐擾市集秩序，婉拒其入城。', 'Fear the disorder; turn the fair away.', 'Порядок дороже; ярмарку отвернуть.'),
 q('集市去了邻国，{kingdom}的街市冷清了几分。', '集市去了鄰國，{kingdom}的街市冷清了幾分。', 'The fair moves to the neighbor; the streets of {kingdom} feel quieter.', 'Ярмарка ушла к соседям; улицы {kingdom} притихли.'))

# ============ 外交 ============
ev('royal_visit', 2,
 q('王室来访', '王室來訪', 'The Royal Visit', 'Королевский визит'),
 q('邻国{kingdom_b}王室宣布来访——排场不能输，礼数更不能输。', '鄰國王室宣布來訪——排場不能輸，禮數更不能輸。', 'A neighboring royal house announces a visit — the pomp must match theirs.', 'Соседний двор объявил визит — пышность не должна уступить.'),
 q('倾力接待', '傾力接待', 'Spare no expense', 'Не жалеть расходов'),
 q('以国礼倾力接待，宾主尽欢。', '以國禮傾力接待，賓主盡歡。', 'Full state honors; guest and host delight.', 'Государственные почести; гости в восторге.'),
 q('宾主尽欢，列国传为佳话。', '賓主盡歡，列國傳為佳話。', 'A perfect visit, told fondly in every court.', 'Идеальный визит; дворы пересказывают.'),
 q('循例接待', '循例接待', 'Standard protocol', 'Обычный протокол'),
 q('按常例接待，不失礼也不逾矩。', '按常例接待，不失禮也不逾矩。', 'Protocol by the book — polite, never lavish.', 'Протокол по книге — вежливо, без излишеств.'),
 q('礼数周全却无惊喜，来访贵宾印象平平。', '禮數周全卻無驚喜，來訪貴賓印象平平。', 'Correct but cold; the guests leave unimpressed.', 'Верно, но сухо; гости уехали равнодушными.'))
ev('hostage_request', 2,
 q('质子之请', '質子之請', 'The Hostage Request', 'Просьба о заложнике'),
 q('强盟来书：请送一名王室子弟为质，以证两国之好。', '強盟來書：請送一名王室子弟為質，以證兩國之好。', 'A strong alliance demands a royal hostage as proof of friendship.', 'Могущественный союз требует королевского заложника.'),
 q('忍痛送质', '忍痛送質', 'Send a hostage', 'Отправить заложника'),
 q('骨肉分离换强盟安心，宫中一片哭声。', '骨肉分離換強盟安心，宮中一片哭聲。', 'Send royal blood abroad for the alliance\'s ease — the palace weeps.', 'Отправить свою кровь на чужбину — дворец плачет.'),
 q('质子成行，盟约愈坚，宫闱之痛唯自知。', '質子成行，盟約愈堅，宮闈之痛唯自知。', 'The hostage departs; the pact hardens, the palace grieves.', 'Заложник уехал; союз крепок, дворец скорбит.'),
 q('婉言回绝', '婉言回絕', 'Decline softly', 'Вежливо отказать'),
 q('以幼子年弱为辞婉拒，盟友脸上无光。', '以「幼子年弱」為辭婉拒，盟友臉上無光。', 'Plead the child\'s tender years; the ally loses face.', 'Сослаться на малолетство; союзник потерял лицо.'),
 q('回绝之辞传回，盟友心生嫌隙。', '回絕之辭傳回，盟友心生嫌隙。', 'The refusal spreads; the ally takes offense.', 'Отказ дошёл; союзник обиделся.'))
ev('border_treaty', 2,
 q('边界之盟', '邊界之盟', 'The Border Treaty', 'Пограничный договор'),
 q('邻国提议签订边界条约，勘定界碑，永息边衅。', '鄰國提議簽訂邊界條約，勘定界碑，永息邊釁。', 'The neighbor proposes a border treaty with fixed markers — an end to feuds.', 'Сосед предлагает договор о границах — конец распрям.'),
 q('签约定界', '簽約定界', 'Sign the treaty', 'Подписать договор'),
 q('让出争议之地，换百年边安。', '讓出爭議之地，換百年邊安。', 'Cede the disputed strip for a century of peace.', 'Уступить спорную полосу ради века мира.'),
 q('界碑立定，两国边民再无械斗。', '界碑立定，兩國邊民再無械鬥。', 'The markers stand; border brawls end.', 'Столбы стоят; стычки кончились.'),
 q('寸土不让', '寸土不讓', 'Not an inch', 'Ни пяди земли'),
 q('祖产寸土不让，边军加倍戒备。', '祖產寸土不讓，邊軍加倍戒備。', 'Not an inch of the ancestors\' land; double the border watch.', 'Ни пяди земли предков; стража вдвое.'),
 q('谈判破裂，边境哨所互相戒备。', '談判破裂，邊境哨所互相戒備。', 'The talks collapse; the outposts eye each other.', 'Переговоры рухнули; заставы смотрят друг на друга.'))
ev('pirate_bribe', 2,
 q('海盗买路钱', '海盜買路錢', 'The Pirate Toll', 'Пиратская пошлина'),
 q('海盗送来分账清单：交钱，{kingdom}的商船可保平安。', '海盜送來分賬清單：交錢，{kingdom}的商船可保平安。', 'The pirates send terms: pay, and {kingdom}\'s ships sail safe.', 'Пираты прислали счёт: плати — и корабли {kingdom} ходят спокойно.'),
 q('交钱买平安', '交錢買平安', 'Pay the toll', 'Заплатить пошлину'),
 q('破财免灾，商船插上安全旗。', '破財免災，商船插上安全旗。', 'Pay the toll; the ships fly the safe-passage flag.', 'Заплатить; корабли под флагом безопасности.'),
 q('买路钱一出，{kingdom}商船通行无阻。', '買路錢一出，{kingdom}商船通行無阻。', 'The toll paid; {kingdom}\'s ships sail untouched.', 'Пошлина уплачена; корабли {kingdom} ходят свободно.'),
 q('悬赏剿盗', '懸賞剿盜', 'Post a bounty instead', 'Объявить награду'),
 q('对海盗悬赏剿捕，以牙还牙。', '對海盜懸賞剿捕，以牙還牙。', 'Bounty on pirate heads instead.', 'Награда за головы пиратов.'),
 q('海盗被剿数股，剩余的记恨在心。', '海盜被剿數股，剩餘的記恨在心。', 'Several pirate bands fall; the rest remember.', 'Несколько банд пали; остальные злопамятны.'))
ev('pilgrim_wave', 2,
 q('朝圣之潮', '朝聖之潮', 'The Pilgrim Wave', 'Волна паломников'),
 q('圣地显灵之说四起，大批朝圣者涌向{kingdom}边境。', '聖地顯靈之說四起，大批朝聖者湧向{kingdom}邊境。', 'Word of miracles draws pilgrims to the borders of {kingdom}.', 'Слухи о чудесах гонят паломников к границам {kingdom}.'),
 q('设棚接待', '設棚接待', 'Shelter the pilgrims', 'Приютить паломников'),
 q('沿途设粥棚驿舍，善待过境朝圣者。', '沿途設粥棚驛舍，善待過境朝聖者。', 'Wayside shelters and soup for the pilgrims passing through.', 'Приюты и похлёбка для паломников.'),
 q('朝圣者感恩戴德，{kingdom}善名远播。', '朝聖者感恩戴德，{kingdom}善名遠播。', 'The pilgrims bless {kingdom} far and wide.', 'Паломники благословляют {kingdom} повсюду.'),
 q('闭关驱离', '閉關驅離', 'Close the border', 'Закрыть границу'),
 q('恐流民生乱，闭关驱离朝圣队伍。', '恐流民生亂，閉關驅離朝聖隊伍。', 'Fear the crowds; close the border and turn them away.', 'Бояться толпы; закрыть границу.'),
 q('朝圣者被拦在界外，圣地名声归了别国。', '朝聖者被攔在界外，聖地名聲歸了別國。', 'The pilgrims are turned away — and the holy fame with them.', 'Паломников отвернули — и святая слава ушла к другим.'))
ev('tribute_envoy', 2,
 q('贡使团', '貢使團', 'The Tribute Envoy', 'Посольство с данью'),
 q('远方小国遣使称臣纳贡，只求{kingdom}庇佑。', '遠方小國遣使稱臣納貢，只求{kingdom}庇佑。', 'A small far kingdom sends tribute and asks for {kingdom}\'s protection.', 'Малое далёкое королевство шлёт дань, прося защиты {kingdom}.'),
 q('受贡纳护', '受貢納護', 'Accept the tribute', 'Принять дань'),
 q('收下贡品，庇护之名也揽下了。', '收下貢品，庇護之名也攬下了。', 'Take the gifts — and the obligations of a protector.', 'Взять дары — и бремя защитника.'),
 q('贡品入库，庇护之责也记在了{kingdom}头上。', '貢品入庫，庇護之責也記在了{kingdom}頭上。', 'The tribute is stored; the duty is written on {kingdom}.', 'Дань в казне; обязанность записана на {kingdom}.'),
 q('谦辞贡礼', '謙辭貢禮', 'Decline gently', 'Вежливо отказаться'),
 q('不贪小国之物，以平等之礼相待。', '不貪小國之物，以平等之禮相待。', 'Take nothing from the small; treat them as equals.', 'Не брать у малых; чтить их как равных.'),
 q('贡礼被谦辞，小国反而更敬{kingdom}三分。', '貢禮被謙辭，小國反而更敬{kingdom}三分。', 'The tribute is declined; the small kingdom respects {kingdom} all the more.', 'Дань отклонена; малое королевство уважает {kingdom} ещё больше.'))

UI = {
 'toast_event_chain': ('前事之因，结出了新的果——{kingdom}又出一桩待决事件。', '前事之因，結出了新的果——{kingdom}又出一樁待決事件。', 'An earlier choice bears new fruit — {kingdom} faces a fresh decision.', 'Прошлый выбор дал новые плоды — {kingdom} ждёт новое решение.'),
}

langs = {'ch': 0, 'zh_tw': 1, 'en': 2, 'ru': 3}
for lang, idx in langs.items():
    p = 'Locales/%s.json' % lang
    d = json.load(io.open(p, encoding='utf-8'), object_pairs_hook=collections.OrderedDict)
    added = 0
    for src in (T, UI):
        for k, v in src.items():
            if k not in d:
                d[k] = v[idx]; added += 1
    json.dump(d, io.open(p, 'w', encoding='utf-8'), ensure_ascii=False, indent=2)
    io.open(p, 'a', encoding='utf-8').write('\n')
    print(lang, 'added', added, 'total', len(d))
