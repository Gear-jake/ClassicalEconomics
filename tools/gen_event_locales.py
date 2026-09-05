# -*- coding: utf-8 -*-
import io, json, collections

T = {}
def ev(eid, title, desc, o1, o2, r1, r2):
    T['ev_' + eid] = title; T['ev_' + eid + '_desc'] = desc
    T['ev_' + eid + '_opt1'] = o1; T['ev_' + eid + '_opt2'] = o2
    T['ev_' + eid + '_res1'] = r1; T['ev_' + eid + '_res2'] = r2

# ===== 财政 finance =====
ev('treasury_gap',
 ('国库亏空','國庫虧空','Empty Treasury','Пустая казна'),
 ('金库几近见底，大臣们围着一本空账簿争执不下。','金庫幾近見底，大臣們圍著一本空帳簿爭執不下。','The treasury is nearly empty; the ministers argue over a blank ledger.','Казна почти пуста; министры спорят над пустой книгой.'),
 ('开征临时税','開徵臨時稅','Levy an emergency tax','Ввести чрезвычайный налог'),
 ('咬牙硬撑','咬牙硬撐','Tough it out','Терпеть'),
 ('临时税已征入库中，国库暂时缓了口气。','臨時稅已徵入庫中，國庫暫時緩了口氣。','The emergency tax has been collected into the treasury.','Чрезвычайный налог собран в казну.'),
 ('国库空转，民间怨声渐起。','國庫空轉，民間怨聲漸起。','The treasury sits empty; grumbling spreads among the people.','Казна пуста, в народе растёт ропот.'))
ev('tax_corruption',
 ('税吏贪腐','稅吏貪腐','Corrupt Tax Collectors','Казнокрады-сборщики'),
 ('税吏上下其手，入库的税金一年比一年薄。','稅吏上下其手，入庫的稅金一年比一年薄。','The collectors skim; less gold reaches the treasury each year.','Сборщики воруют; до казны доходит всё меньше золота.'),
 ('严惩税吏','嚴懲稅吏','Punish them','Наказать'),
 ('默许分成','默許分成','Take a cut','Делить взятку'),
 ('贪吏被清办，王室威信反而上升。','貪吏被清辦，王室威信反而上升。','The corrupt were purged; the crown\'s standing rose.','Казнокрады наказаны; авторитет короны вырос.'),
 ('王室与贪吏同流，民间暗生不满。','王室與貪吏同流，民間暗生不滿。','The crown shares the spoils; discontent simmers.','Корона делит добычу; зреет недовольство.'))
ev('rich_petition',
 ('富商请愿','富商請願','The Merchants\' Petition','Ходатайство богачей'),
 ('富商巨贾联名请愿，愿以重金换取专卖特权。','富商巨賈聯名請願，願以重金換取專賣特權。','Rich merchants offer gold for trading privileges.','Богатые купцы предлагают золото за привилегии.'),
 ('准其所请','准其所請','Grant it','Удовлетворить'),
 ('驳回请愿','駁回請願','Reject it','Отклонить'),
 ('重金入库，商人们弹冠相庆。','重金入庫，商人們彈冠相慶。','Gold flows in; the merchants celebrate.','Золото в казне; купцы ликуют.'),
 ('请愿被驳回，商界与王室渐生嫌隙。','請願被駁回，商界與王室漸生嫌隙。','The petition is rejected; the merchants grow cold to the crown.','Петиция отклонена; купцы охладели к короне.'))

# ===== 天灾 disaster =====
ev('drought',
 ('大旱','大旱','Great Drought','Великая засуха'),
 ('赤地千里，河床龟裂，粮价一日三涨。','赤地千里，河床龜裂，糧價一日三漲。','The land cracks; grain prices climb daily.','Земля трескается; хлеб дорожает на глазах.'),
 ('开仓平粜','開倉平糶','Open the granaries','Открыть амбары'),
 ('诏令祈雨','詔令祈雨','Pray for rain','Молить о дожде'),
 ('仓粮散发四方，灾民暂得活路。','倉糧散發四方，災民暫得活路。','Granary grain is handed out; the people survive.','Зерно роздано; народ спасён.'),
 ('祭台搭起又拆去，雨没有来，怨气来了。','祭臺搭起又拆去，雨沒有來，怨氣來了。','The altars are built and torn down; no rain, only anger.','Алтари построены и снесены; дождя нет — есть злоба.'))
ev('locust',
 ('蝗灾','蝗災','Locust Swarm','Саранча'),
 ('蝗群蔽日而过，田间只剩光秃秃的秸秆。','蝗群蔽日而過，田間只剩光禿禿的稭稈。','A swarm blots out the sun; the fields are stripped.','Тучи саранчи скрыли солнце; поля голы.'),
 ('赏捕蝗令','賞捕蝗令','Pay for locusts','Платить за саранчу'),
 ('听天由命','聽天由命','Let it be','Покориться судьбе'),
 ('家家捕蝗换粮，灾情没有蔓延。','家家捕蝗換糧，災情沒有蔓延。','Every household hunts locusts; the blight is contained.','Каждый дом ловит саранчу; беда остановлена.'),
 ('蝗群过境如洗，村落间已有饥声。','蝗群過境如洗，村落間已有飢聲。','The swarm leaves nothing; hunger spreads in the villages.','Саранча ушла досуха; в деревнях голод.'))
ev('quake_aftermath',
 ('地震余波','地震餘波','After the Earthquake','После землетрясения'),
 ('地动之后余震不断，坊间传言大祸将至。','地動之後餘震不斷，坊間傳言大禍將至。','Aftershocks continue; rumors of doom spread.','Толчки продолжаются; ходят слухи о беде.'),
 ('以工代赈','以工代賑','Work relief','Оплаченные работы'),
 ('听其自便','聽其自便','Leave them be','Оставить как есть'),
 ('以工代赈重修屋墙，人心渐安。','以工代賑重修屋牆，人心漸安。','Paid repairs rebuild homes and calm the people.','Ремонт успокоил людей.'),
 ('废墟无人过问，流言越传越凶。','廢墟無人過問，流言越傳越凶。','The ruins are ignored; rumors grow wilder.','Развалины брошены; слухи пуще.'))

# ===== 宫廷 court =====
ev('royal_wedding',
 ('王室婚宴','王室婚宴','A Royal Wedding','Королевская свадьба'),
 ('王室将办婚宴，司礼官呈上两份开支单。','王室將辦婚宴，司禮官呈上兩份開支單。','A royal wedding approaches; the steward brings two budgets.','Скоро свадьба; распорядитель подал две сметы.'),
 ('倾力大办','傾力大辦','Grand feast','Пышный пир'),
 ('从简操办','從簡操辦','Modest rites','Скромный обряд'),
 ('婚宴轰动列国，来使盈门，声望日隆。','婚宴轟動列國，來使盈門，聲望日隆。','The feast impresses every court; envoys fill the gates.','Пир впечатлил все дворы; послы теснятся у врат.'),
 ('婚宴简办，各国觉得王室风光不再。','婚宴簡辦，各國覺得王室風光不再。','The modest rites leave the courts unimpressed.','Скромный обряд не впечатлил никого.'))
ev('minister_power',
 ('权臣干政','權臣干政','An Overmighty Minister','Всесильный царедворец'),
 ('某大臣广植党羽，政令不出其门。','某大臣廣植黨羽，政令不出其門。','A minister packs the courts; decrees bear his seal alone.','Сановник оброс кланом; указы за его печатью.'),
 ('削其权柄','削其權柄','Break him','Обломить его'),
 ('虚与委蛇','虛與委蛇','Tolerate him','Терпеть его'),
 ('权柄收归王室，但其党羽怨望，朝局暗涌。','權柄收歸王室，但其黨羽怨望，朝局暗湧。','Power returns to the crown; the minister\'s faction seethes.','Власть у короны; клан сановника кипит.'),
 ('权臣依旧只手遮天，各国皆知王命不行。','權臣依舊隻手遮天，各國皆知王命不行。','The minister still rules; every court knows whose word goes.','Сановник правит; все дворы знают, чьё слово.'))
ev('succession_dispute',
 ('继承争议','繼承爭議','The Succession Dispute','Спор о наследстве'),
 ('王位继承起争议，长幼两派各执一词。','王位繼承起爭議，長幼兩派各執一詞。','The succession is disputed; two factions deadlock.','Престол оспаривается; две партии в тупике.'),
 ('依例立长，压下争议','依例立長，壓下爭議','Back the elder heir','Поддержать старшего'),
 ('立贤并厚赏诸弟','立賢並厚賞諸弟','Back the worthier, buy off the rest','Поддержать достойного, задобрить остальных'),
 ('长嗣继位名正言顺，但败者心怀怨恨。','長嗣繼位名正言順，但敗者心懷怨恨。','The elder heir prevails; the losing faction broods.','Старший наследует; проигравшие затаили зло.'),
 ('重赏之下争议暂平，国库却为此付出了代价。','重賞之下爭議暫平，國庫卻為此付出了代價。','Gold settles the dispute—at the treasury\'s expense.','Золото утишило спор — ценой казны.'))

# ===== 军事 military =====
ev('army_pay',
 ('边军索饷','邊軍索餉','The Army Demands Pay','Армия требует жалованье'),
 ('前线边军欠饷已久，军使者接连入城催饷。','前線邊軍欠餉已久，軍使者接連入城催餉。','The frontier army is owed months of pay; envoys press for gold.','Пограничной армии долго не платят; посланцы требуют золота.'),
 ('补发军饷','補發軍餉','Pay in full','Выплатить сполна'),
 ('许诺缓发','許諾緩發','Promise it later','Обещать позже'),
 ('军饷足额发放，边军士气大振。','軍餉足額發放，邊軍士氣大振。','The army is paid in full; morale soars.','Армия получила всё; дух войска высок.'),
 ('军饷一拖再拖，营中已有怨言。','軍餉一拖再拖，營中已有怨言。','Pay is postponed again; grumbling fills the camps.','Плата откладывается; в лагерах ропот.'))
ev('mercenary_default',
 ('佣兵索酬','傭兵索酬','Mercenaries Demand Their Due','Наёмники требуют платы'),
 ('雇来的佣兵在战线吃紧时索要欠酬，语气不善。','雇來的傭兵在戰線吃緊時索要欠酬，語氣不善。','Hired mercenaries demand back-pay at the worst moment.','Наёмники требуют долг в самый неподходящий момент.'),
 ('如约补足','如約補足','Pay what is owed','Заплатить долг'),
 ('寻由遣散','尋由遣散','Disband them','Распустить их'),
 ('佣金结清，佣兵各归其营。','傭金結清，傭兵各歸其營。','The debt is settled; the mercenaries stand down.','Расчёт произведён; наёмники утихли.'),
 ('佣兵被遣散，散兵游勇成了地方祸患。','傭兵被遣散，散兵游勇成了地方禍患。','Disbanded, the mercenaries turn to brigandage.','Распущенные наёмники грабят дороги.'))
ev('prisoner_ransom',
 ('战俘赎金','戰俘贖金','Ransom the Prisoners','Выкуп пленных'),
 ('敌国愿以重金换回被俘的贵族，我方亦可反赎己方重臣。','敵國願以重金換回被俘的貴族，我方亦可反贖己方重臣。','The enemy offers ransom for captive nobles—or we may buy our own back.','Враг предлагает выкуп за пленных дворян.'),
 ('出金赎回己方被俘者','出金贖回己方被俘者','Ransom our own','Выкупить своих'),
 ('拒绝赎回','拒絕贖回','Refuse','Отказаться'),
 ('被俘者归国，两国关系缓和了一分。','被俘者歸國，兩國關係緩和了一分。','The captives come home; tensions ease a little.','Пленные вернулись; напряжение спало.'),
 ('赎金被拒，敌国扬言报复。','贖金被拒，敵國揚言報復。','The ransom is refused; the enemy vows revenge.','Выкуп отклонён; враг грозит местью.'))

# ===== 民生 civil =====
ev('bread_riot',
 ('粮价骚乱','糧價騷亂','The Bread Riot','Хлебный бунт'),
 ('粮价飞涨，市民聚在粮仓外喧哗不止。','糧價飛漲，市民聚在糧倉外喧譁不止。','Bread prices soar; a crowd gathers at the granary gates.','Хлеб вздорожал; толпа у амбаров.'),
 ('开仓平抑粮价','開倉平抑糧價','Open the granaries','Открыть амбары'),
 ('弹压驱散','彈壓驅散','Disperse the crowd','Разогнать толпу'),
 ('仓粮平价出售，市面渐渐安定。','倉糧平價出售，市面漸漸安定。','Grain is sold at fair prices; the streets calm down.','Хлеб по честной цене; улицы стихли.'),
 ('人群被驱散，怨气却散不掉。','人群被驅散，怨氣卻散不掉。','The crowd is dispersed; the anger is not.','Толпу разогнали; злость осталась.'))
ev('plague',
 ('瘟疫防治','瘟疫防治','Plague Precautions','Поветрие'),
 ('邻郡疫病流行，医官请命提前布防。','鄰郡疫病流行，醫官請命提前布防。','Plague spreads in the next province; physicians ask to act early.','Мор рядом; лекари просят действовать.'),
 ('出资施药设医棚','出資施藥設醫棚','Fund physicians','Оплатить лекарей'),
 ('封城观望','封城觀望','Seal the gates and wait','Запереть ворота'),
 ('药施医设，疫病被挡在了城外。','藥施醫設，疫病被擋在了城外。','Physicians are funded; the plague stays outside.','Лекари оплачены; мор за воротами.'),
 ('城门紧闭商旅绝迹，市面萧条。','城門緊閉商旅絕跡，市面蕭條。','The gates are shut; trade withers.','Ворота закрыты; торговля умерла.'))

# ===== 外交 diplomacy =====
ev('neighbor_extort',
 ('邻国勒索','鄰國勒索','The Neighbor Ultimatum','Ультиматум соседа'),
 ('强邻遣使索要岁币，声称不允便要巡边。','強鄰遣使索要歲幣，聲稱不允便要巡邊。','A strong neighbor demands tribute—pay, or they will patrol the border.','Сосед требует дань — заплати, или мы пойдём в поход.'),
 ('纳币消灾','納幣消災','Pay the tribute','Заплатить дань'),
 ('严词拒绝','嚴詞拒絕','Refuse flatly','Отказать наотрез'),
 ('岁币送出，强邻暂时安分，各国暗笑软弱者亦有之。','歲幣送出，強鄰暫時安分，各國暗笑軟弱者亦有之。','The tribute is paid; the neighbor quiets down—for now.','Дань уплачена; сосед утих — пока.'),
 ('使节被逐，边境剑拔弩张。','使節被逐，邊境劍拔弩張。','The envoy is expelled; the border heats up.','Посол изгнан; граница накалилась.'))
ev('marriage_alliance',
 ('联姻提议','聯姻提議','A Marriage Proposal','Предложение о браке'),
 ('邻国提议联姻，愿以姻亲之盟换取世代修好。','鄰國提議聯姻，願以姻親之盟換取世代修好。','A neighbor proposes a marriage alliance.','Сосед предлагает брачный союз.'),
 ('应允联姻','應允聯姻','Accept the match','Принять предложение'),
 ('婉言谢绝','婉言謝絕','Politely decline','Вежливо отказаться'),
 ('姻缘缔结，两国使节往来不绝。','姻緣締結，兩國使節往來不絕。','The marriage binds the two courts; envoys never cease.','Брак скрепил дворы; послы не переводятся.'),
 ('联姻被婉拒，姻亲之盟就此作罢。','聯姻被婉拒，姻親之盟就此作罷。','The proposal is politely declined.','Предложение вежливо отклонено.'))

UI = {
 'event_choice_title': ('抉择事件','抉擇事件','Decision Event','Решение'),
 'event_choice_header': ('〈{0}〉 事件 {1}/{2}','〈{0}〉 事件 {1}/{2}','〈{0}〉 event {1}/{2}','〈{0}〉 событие {1}/{2}'),
 'event_choice_countdown': ('剩余 {0} 年，逾期将自动执行保守选项','剩餘 {0} 年，逾期將自動執行保守選項','{0} year(s) left, then the cautious option is taken','Осталось {0} г., затем выберут осторожный вариант'),
 'event_choice_none': ('当前没有待决事件。','當前沒有待決事件。','No pending decisions.','Нет ожидающих решений.'),
 'event_choice_next': ('还有 {0} 件待决 →','還有 {0} 件待決 →','{0} more pending →','Ещё {0} →'),
 'event_choice_cost': ('花费 {0}','花費 {0}','Costs {0}','Стоимость: {0}'),
 'event_choice_gain': ('入库 {0}','入庫 {0}','Gains {0}','В казну: {0}'),
 'event_choice_tax': ('向居民征税','向居民徵稅','Tax residents','Налог с жителей'),
 'event_choice_relief': ('散财济贫','散財濟貧','Poor relief','Раздать бедным'),
 'event_choice_goodwill': ('各国好感 {0}{1}','各國好感 {0}{1}','Goodwill {0}{1}','Симпатия {0}{1}'),
 'event_choice_unrest': ('民怨滋生','民怨滋生','Raises unrest','Рост недовольства'),
 'toast_event_pending': ('王国出了一桩待决事件，请打开内阁处理。','王國出了一樁待決事件，請打開內閣處理。','A decision awaits the kingdom — open the Cabinet.','Королевство ждёт решения — откройте Кабинет.'),
 'cabinet_pending_row': ('待决事件 ×{0}（最早一件剩 {1} 年）','待決事件 ×{0}（最早一件剩 {1} 年）','Pending events ×{0} (soonest: {1} y left)','Ожидают решения ×{0} (первое: {1} г.)'),
 'cabinet_pending_open': ('去处理','去處理','Handle','Разобраться'),
 'events_filter_all': ('全部','全部','All','Все'),
 'events_filter_decision': ('抉择','抉擇','Decisions','Решения'),
 'events_filter_politics': ('国家·战争','國家·戰爭','States & Wars','Страны и войны'),
 'events_filter_economy': ('经济·民生','經濟·民生','Economy','Экономика'),
 'events_fold_year': ('第 {0} 年（{1} 条）▸','第 {0} 年（{1} 條）▸','Year {0} ({1} entries) ▸','Год {0} ({1}) ▸'),
 'events_year_hdr': ('—— 第 {0} 年 ——','—— 第 {0} 年 ——','—— Year {0} ——','—— Год {0} ——'),
 'ev_desc_decision': ('王国做出了抉择（选项 {0}）。','王國做出了抉擇（選項 {0}）。','The kingdom made its choice (option {0}).','Королевство сделало выбор (вариант {0}).'),
 'event_chance_player': ('玩家事件概率','玩家事件機率','Player Event Chance','Шанс событий (игрок)'),
 'event_chance_player Description': ('玩家认领的国家每年触发抉择事件的概率（0~1，0=关闭）。','玩家認領的國家每年觸發抉擇事件的機率（0~1，0=關閉）。','Yearly chance of a decision event for your claimed kingdom (0-1; 0 disables).','Годовой шанс события для вашей страны (0-1; 0 = выкл).'),
 'event_chance_ai': ('AI 事件概率','AI 事件機率','AI Event Chance','Шанс событий (AI)'),
 'event_chance_ai Description': ('AI 各国每年触发抉择事件的概率（0~1），AI 按国性自动决策并记入事件流。','AI 各國每年觸發抉擇事件的機率（0~1），AI 按國性自動決策並記入事件流。','Yearly chance for AI kingdoms (0-1); they decide by national character and the result lands in the event feed.','Годовой шанс для стран AI (0-1); они решают сами, итог попадает в ленту.'),
 'event_cooldown_years': ('事件全局冷却','事件全局冷卻','Global Event Cooldown','Глобальная пауза событий'),
 'event_cooldown_years Description': ('任意两个王国事件之间至少相隔的年数（1~10）。','任意兩個王國事件之間至少相隔的年數（1~10）。','Minimum years between any two kingdom events (1-10).','Минимум лет между любыми событиями (1-10).'),
}

langs = {'ch':0, 'zh_tw':1, 'en':2, 'ru':3}
for lang, idx in langs.items():
    p = 'Locales/%s.json' % lang
    d = json.load(io.open(p, encoding='utf-8'), object_pairs_hook=collections.OrderedDict)
    added = 0
    for src in (T, UI):
        for k, v in src.items():
            if k in d: continue
            d[k] = v[idx]
            added += 1
    json.dump(d, io.open(p, 'w', encoding='utf-8'), ensure_ascii=False, indent=2)
    io.open(p, 'a', encoding='utf-8').write('\n')
    print(lang, 'added', added, 'total', len(d))
