# -*- coding: utf-8 -*-
"""v1.5.0：银行·商务页签 + 5 新事件的四语文案。幂等，可重复运行。"""
import io, json, collections

T = collections.OrderedDict()
def add(d):
    for k, v in d.items():
        T[k] = v

# ===== 页签与银行 UI =====
add({
 'cabinet_tab_bank': ('银行·商务', '銀行·商務', 'Bank & Trade', 'Банк и торговля'),
 'bank_stats_reserves': ('银行储备：{0}', '銀行儲備：{0}', 'Bank reserves: {0}', 'Резервы банка: {0}'),
 'bank_stats_loans': ('在外贷款：{0}', '在外貸款：{0}', 'Loans outstanding: {0}', 'Выданные кредиты: {0}'),
 'bank_stats_default': ('上年违约率：{0}%', '上年違約率：{0}%', 'Last-year defaults: {0}%', 'Дефолты за год: {0}%'),
 'bank_risk_label': ('挤兑风险：{0}', '擠兌風險：{0}', 'Bank-run risk: {0}', 'Риск паники: {0}'),
 'bank_risk_0': ('安全', '安全', 'Safe', 'Безопасно'),
 'bank_risk_1': ('警戒', '警戒', 'Warning', 'Тревога'),
 'bank_risk_2': ('危险', '危險', 'Danger', 'Опасность'),
 'bank_rate_label': ('基准利率', '基準利率', 'Base rate', 'Ставка'),
 'bank_rate_high': ('高(稳)', '高(穩)', 'High (safe)', 'Высокая'),
 'bank_rate_mid': ('中', '中', 'Mid', 'Средняя'),
 'bank_rate_low': ('低(险)', '低(險)', 'Low (risky)', 'Низкая'),
 'bank_quota_label': ('放贷额度', '放貸額度', 'Lending quota', 'Квота кредитов'),
 'bank_quota_tight': ('紧', '緊', 'Tight', 'Тесно'),
 'bank_quota_mid': ('中', '中', 'Mid', 'Средне'),
 'bank_quota_loose': ('松', '鬆', 'Loose', 'Свободно'),
 'bank_reserve_label': ('准备金率', '準備金率', 'Reserve ratio', 'Норма резервов'),
 'bank_reserve_high': ('高', '高', 'High', 'Высокая'),
 'bank_reserve_mid': ('中', '中', 'Mid', 'Средняя'),
 'bank_reserve_low': ('低', '低', 'Low', 'Низкая'),
 'bank_preset_stimulus': ('刺激通道', '刺激通道', 'Stimulus', 'Стимул'),
 'bank_preset_neutral': ('中性通道', '中性通道', 'Neutral', 'Нейтрально'),
 'bank_preset_suppress': ('抑制通道', '抑制通道', 'Restrain', 'Сдерживание'),
 'bank_preset_applied': ('银行通道已调整。', '銀行通道已調整。', 'Bank channel adjusted.', 'Канал банка изменён.'),
 'bank_commerce_title': ('商业', '商業', 'Commerce', 'Торговля'),
 'bank_commerce_tax': ('本年商业税：{0}', '本年商業稅：{0}', 'This year\'s commerce tax: {0}', 'Торговый налог за год: {0}'),
 'bank_policy_franchise': ('特许经营权【{0}】', '特許經營權【{0}】', 'Franchise rights [{0}]', 'Франшиза [{0}]'),
 'bank_policy_fairprice': ('低价法案【{0}】', '低價法案【{0}】', 'Fair-price act [{0}]', 'Закон о ценах [{0}]'),
 'bank_policy_on': ('开', '開', 'ON', 'ВКЛ'),
 'bank_policy_off': ('关', '關', 'OFF', 'ВЫКЛ'),
 'bank_disabled_note': ('（银行系统已在设置中关闭，本页仅作展示）', '（銀行系統已在設定中關閉，本頁僅作展示）', '(Banking is disabled in settings; this page is display-only.)', '(Банковская система отключена в настройках.)'),
 'toast_event_chain': ('前事之因，结出了新的果——{kingdom}又出一桩待决事件。', '前事之因，結出了新的果——{kingdom}又出一樁待決事件。', 'An earlier choice bears new fruit — {kingdom} faces a fresh decision.', 'Прошлый выбор дал новые плоды — {kingdom} ждёт новое решение.'),
})

# ===== 5 新事件 =====
def ev(eid, opts, title, desc, *texts):
    T['ev_' + eid] = title
    T['ev_' + eid + '_desc'] = desc
    for oi in range(opts):
        name, d, res = texts[oi * 3], texts[oi * 3 + 1], texts[oi * 3 + 2]
        T['ev_%s_opt%d' % (eid, oi + 1)] = name
        T['ev_%s_opt%d_desc' % (eid, oi + 1)] = d
        T['ev_%s_res%d' % (eid, oi + 1)] = res

def q(zh, tw, en, ru):
    return (zh, tw, en, ru)

ev('caravan_ambush', 2,
 q('商队遇袭', '商隊遇襲', 'The Caravan Ambush', 'Засада на караван'),
 q('{king}的商队在边境遭袭，货队失散——商路人心惶惶。', '{king}的商隊在邊境遭襲，貨隊失散——商路人心惶惶。', '{king}\'s caravan is ambushed at the border; the trade roads are afraid.', 'Караван {king} атакован на границе; торговые дороги в панике.'),
 q('出金护商', '出金護商', 'Pay for escorts', 'Заплатить за охрану'),
 q('雇护卫沿路护送，商路立稳。', '雇護衛沿路護送，商路立穩。', 'Hire escorts along the roads; trade steadies at once.', 'Нанять охрану; торговля сразу крепнет.'),
 q('护卫上路，商队复行，市场安心。', '護衛上路，商隊復行，市場安心。', 'Escorts ride out; caravans move and markets relax.', 'Охрана вышла; караваны идут, рынки спокойны.'),
 q('自求多福', '自求多福', 'They fend for themselves', 'Пусть сами'),
 q('不做担保，商路断绝两年。', '不做擔保，商路斷絕兩年。', 'No guarantees; the trade roads suffer for two years.', 'Не гарантировать; дороги страдают два года.'),
 q('商路断绝，市面萧条，怨声归咎于{kingdom}。', '商路斷絕，市面蕭條，怨聲歸咎於{kingdom}。', 'The roads stay cut; the slump is blamed on {kingdom}.', 'Пути перекрыты; спад вменяют {kingdom}.'))
ev('trade_route_cut', 2,
 q('商路断绝', '商路斷絕', 'The Cut Routes', 'Перекрытые пути'),
 q('商路中断已半年，商铺缺货，税金锐减。{king}要么花钱疏通，要么硬扛。', '商路中斷已半年，商鋪缺貨，稅金銳減。{king}要么花錢疏通，要么硬扛。', 'Half a year of cut routes: shops run dry, taxes shrink. {king} pays to reopen or holds firm.', 'Полгода пути перекрыты: лавки пусты, налоги падают. {king} платит или держится.'),
 q('重金疏通商路', '重金疏通商路', 'Pay to reopen', 'Заплатить за открытие'),
 q('买通沿途势力，商路复通。', '買通沿途勢力，商路復通。', 'Grease the right palms; the routes reopen.', 'Уладить с местными; пути откроются.'),
 q('商路复通，商业税回稳。', '商路復通，商業稅回穩。', 'The routes reopen; commerce tax recovers.', 'Пути открыты; торговый налог восстановлен.'),
 q('改走陆路硬扛', '改走陸路硬扛', 'Rely on land routes', 'Перейти на сухопутные'),
 q('绕行山陆，耗时而昂贵，但不用低头。', '繞行山陸，耗時而昂貴，但不用低頭。', 'The land detour is slow and dear — but no one is paid off.', 'Сухопутный обход долог и дорог — но никому не платим.'),
 q('陆路艰难维持，商业勉强没有归零。', '陸路艱難維持，商業勉強沒有歸零。', 'The land routes hold on barely; commerce stays above zero.', 'Обход еле держит торговлю на нуле.'))
ev('guild_petition2', 2,
 q('商会请愿', '商會請願', 'The Guild Petition', 'Ходатайство гильдий'),
 q('商会联名请愿：请{king}授予特许经营权，商会愿以重金相报。', '商會聯名請願：請{king}授予特許經營權，商會願以重金相報。', 'The guilds petition {king} for franchise rights — and offer heavy gold.', 'Гильдии просят {king} франшизу — и предлагают золото.'),
 q('授予特许权', '授予特許權', 'Grant the franchise', 'Дать франшизу'),
 q('收下重金授予特权——富商得势，贫者更难。', '收下重金授予特權——富商得勢，貧者更難。', 'Take the gold and grant the rights; the rich gain, the poor lose.', 'Взять золото и дать права: богатым — воля, бедным — тесно.'),
 q('特许权授予，商税大涨，市井怨声渐起。', '特許權授予，商稅大漲，市井怨聲漸起。', 'The franchise is granted; taxes soar, so do the grumbles.', 'Франшиза дана; налоги вверх, ропот тоже.'),
 q('谢绝请愿', '謝絕請願', 'Decline politely', 'Вежливо отказать'),
 q('维持公平市面，谢绝特许。', '維持公平市面，謝絕特許。', 'Keep the market fair; decline the deal.', 'Сохранить ярмарку честной — отказаться.'),
 q('请愿被谢绝，商会闷声不乐，民间称许。', '請願被謝絕，商會悶聲不樂，民間稱許。', 'The petition is declined; guilds sulk, the people approve.', 'Ходатайство отклонено; гильдии в обиде, народ за.'))
ev('bank_run', 2,
 q('银行挤兑', '銀行擠兌', 'The Bank Run','Банковская паника'),
 q('违约频传，储户涌向银行挤兑——储备告急，恐慌蔓延。{king}必须立刻决断。', '違約頻傳，儲戶湧向銀行擠兌——儲備告急，恐慌蔓延。{king}必須立刻決斷。', 'Defaults spread and depositors storm the banks. Reserves run dry — {king} must act now.','Дефолты множатся; вкладчики штурмуют банки. {king} должен решать.'),
 q('王室注资救市', '王室注資救市', 'Crown bailout','Королевская помощь'),
 q('掏王室金库填补储备，把信心买回来。', '掏王室金庫填補儲備，把信心買回來。', 'Fill the reserves from the crown vault and buy back confidence.', 'Наполнить резервы из казны и вернуть доверие.'),
 q('王室金库注资，挤兑平息，市场缓过气来。', '王室金庫注資，擠兌平息，市場緩過氣來。', 'The bailout calms the run; the market breathes again.','Помощь короны утишила панику; рынок вздохнул.'),
 q('听任倒闭', '聽任倒閉', 'Let them fail','Дать рухнуть'),
 q('不救银行救国库——但民怨会像雪球一样滚大。', '不救銀行救國庫——但民怨會像雪球一樣滾大。', 'Save the vault, not the banks — and watch the anger snowball.', 'Сберечь казну, не банки — и смотреть, как растёт гнев.'),
 q('银行倒闭潮爆发，街上尽是愤怒的储户。', '銀行倒閉潮爆發，街上盡是憤怒的儲戶。', 'Bank failures erupt; the streets fill with furious depositors.','Волна крахов; улицы полны вкладчиков.'))
ev('bank_run_aftermath', 2,
 q('挤兑余波', '擠兌餘波','After the Run','После паники'),
 q('挤兑虽平，市场信用却碎了——{king}要如何收拾人心？', '擠兌雖平，市場信用卻碎了——{king}要如何收拾人心？','The run is over but trust is shattered. How will {king} mend it?','Паника улеглась, но доверие разбито. Как {king} это починит?'),
 q('开仓济困','開倉濟困','Relief for the ruined','Помощь разорённым'),
 q('散财救济被挤兑毁掉的储户。','散財救濟被擠兌毀掉的儲戶。','Spend gold to relief the ruined depositors.','Раздать золото разорённым вкладчикам.'),
 q('受济之民重拾信心，市面渐渐回暖。','受濟之民重拾信心，市面漸漸回暖。','The ruined regain faith; the market warms again.','Разорённые вернут доверие; рынок оживает.'),
 q('任市场自愈','任市場自癒','Let the market heal','Пусть рынок сам залечит'),
 q('不干预，让时间抹平一切。','不干預，讓時間抹平一切。','No interference; time heals all ledgers.','Не вмешиваться; время всё излечит.'),
 q('市场自愈缓慢，{kingdom}的信用之伤仍未愈。','市場自癒緩慢，{kingdom}的信用之傷仍未癒。','The healing is slow; {kingdom}\'s credit scar remains.','Рынок лечится медленно; шрам доверия у {kingdom} остался.'))

langs = {'ch':0, 'zh_tw':1, 'en':2, 'ru':3}
for lang, idx in langs.items():
    p = 'Locales/%s.json' % lang
    d = json.load(io.open(p, encoding='utf-8'), object_pairs_hook=collections.OrderedDict)
    added = 0
    for k, v in T.items():
        if k in d: continue
        d[k] = v[idx]; added += 1
    json.dump(d, io.open(p, 'w', encoding='utf-8'), ensure_ascii=False, indent=2)
    io.open(p, 'a', encoding='utf-8').write('\n')
    print(lang, 'added', added, 'total', len(d))
