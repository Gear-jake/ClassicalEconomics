# -*- coding: utf-8 -*-
import io, json, collections

# 1) 事件卡：族别 + 年份 + 选项描述渲染
t = io.open('UI/EventChoiceWindow.cs', encoding='utf-8').read()

old = """            // 事件卡头：事件名 + 国名（多件时附计数）
            string header = pending.Count > 1
                ? UIHelpers.Lf("event_choice_header", UIHelpers.L("ev_" + def.id), _index + 1, pending.Count)
                : UIHelpers.L("ev_" + def.id);
            AddLine(header, UIStyles.Gold, 16f);
            AddLine(UIHelpers.Lf("event_choice_kingdom", p.KingdomName), Muted, 12f);"""
new = """            // 事件卡头：族别（族别色）+ 事件名 + 国名 + 当前年（多件时附计数）
            var familyColor = FamilyColor(def.family);
            AddLine(UIHelpers.L("event_family_" + def.family), familyColor, 12f);
            string header = pending.Count > 1
                ? UIHelpers.Lf("event_choice_header", UIHelpers.L("ev_" + def.id), _index + 1, pending.Count)
                : UIHelpers.L("ev_" + def.id);
            AddLine(header, UIStyles.Gold, 16f);
            AddLine(UIHelpers.Lf("event_choice_kingdom", p.KingdomName), Muted, 12f);
            try { AddLine(UIHelpers.Lf("event_choice_year", EconomyModMain.GetCurrentGameYear()), Muted, 11f); }
            catch (System.Exception) { }"""
assert old in t, 'header'
t = t.replace(old, new, 1)

old = """                string summary = OptionSummary(def, i, gdp);
                if (!string.IsNullOrEmpty(summary))
                    UIHelpers.CreateText(summary, row.transform, 10f * s, Muted, _gameFont, 16f * s);"""
new = """                string optDesc = UIHelpers.L("ev_" + def.id + "_" + opt.key + "_desc");
                if (optDesc != "ev_" + def.id + "_" + opt.key + "_desc")
                    UIHelpers.CreateText(optDesc, row.transform, 11f * s, UIStyles.TextSecondary, _gameFont, 16f * s);
                string summary = OptionSummary(def, i, gdp);
                if (!string.IsNullOrEmpty(summary))
                    UIHelpers.CreateText(summary, row.transform, 10f * s, Muted, _gameFont, 16f * s);"""
assert old in t, 'optdesc render'
t = t.replace(old, new, 1)

old = """        private static float Scale()
        {"""
new = """        /// <summary>族别色（事件卡头/事件流过滤同源语义）。</summary>
        private static Color FamilyColor(string family)
        {
            switch (family)
            {
                case "finance": return UIStyles.Gold;
                case "disaster": return UIStyles.EvDisaster;
                case "court": return new Color(0.85f, 0.6f, 0.95f);
                case "military": return UIStyles.EvPlunder;
                case "civil": return UIStyles.Info;
                case "diplomacy": return UIStyles.Positive;
                default: return UIStyles.TextPrimary;
            }
        }

        private static float Scale()
        {"""
assert old in t, 'familycolor'
t = t.replace(old, new, 1)
io.open('UI/EventChoiceWindow.cs', 'w', encoding='utf-8', newline='').write(t)
print('EventChoiceWindow polished')

# 2) 去掉 CabinetWindow 重复 Fs（继承基类即可）
t = io.open('UI/CabinetWindow.cs', encoding='utf-8').read()
old = """        /// <summary>UI 整体缩放（设置页 ui_scale，0.8~1.6，默认 1.2）：字号/按钮宽高/行高统一乘此系数。</summary>
        private static float Fs(float size)
        {
            var cfg = UnrestConfig.Instance;
            float scale = cfg != null ? cfg.UiScale : 1.2f;
            return size * Mathf.Clamp(scale, 0.8f, 1.6f);
        }

"""
assert old in t, 'cabinet fs'
t = t.replace(old, '', 1)
io.open('UI/CabinetWindow.cs', 'w', encoding='utf-8', newline='').write(t)
print('CabinetWindow Fs unified')

# 3) 四语：族别键 + 年份键 + 32 条选项描述
FAM = {
 'event_family_finance': ('财政','財政','Finance','Финансы'),
 'event_family_disaster': ('天灾','天災','Disaster','Катастрофы'),
 'event_family_court': ('宫廷','宮廷','Court','Двор'),
 'event_family_military': ('军事','軍事','Military','Война'),
 'event_family_civil': ('民生','民生','Civil','Народ'),
 'event_family_diplomacy': ('外交','外交','Diplomacy','Дипломатия'),
 'event_choice_year': ('当前 第 {0} 年','當前 第 {0} 年','Year {0}','Год {0}'),
}
OPT = collections.OrderedDict()
def o(eid, n, ch, tw, en, ru):
    OPT['ev_%s_opt%d_desc' % (eid, n)] = (ch, tw, en, ru)

o('treasury_gap', 1, '向全国居民临时征收一笔战时税，充实国库。', '向全國居民臨時徵收一筆戰時稅，充實國庫。', 'Levy a one-off emergency tax on all residents to refill the treasury.', 'Разовый чрезвычайный налог с жителей наполнит казну.')
o('treasury_gap', 2, '什么也不做，勒紧裤腰带——但民怨会积累。', '什麼也不做，勒緊褲腰帶——但民怨會積累。', 'Do nothing and tighten belts — unrest will grow.', 'Ничего не делать — недовольство будет расти.')
o('tax_corruption', 1, '彻查税吏，追回赃款归公，吏治一新。', '徹查稅吏，追回贓款歸公，吏治一新。', 'Purge the collectors; the crown pays for the investigation but honor is restored.', 'Наказать сборщиков: расследование стоит денег, но порядок восстановлен.')
o('tax_corruption', 2, '默许税吏截留，国库反而多收——腐蚀从此开始。', '默許稅吏截留，國庫反而多收——腐蝕從此開始。', 'Let them skim; the treasury nets more — corruption takes root.', 'Позволить воровать: казна получит больше — но это начало гнили.')
o('rich_petition', 1, '收下重金，授予商人们专卖特权。', '收下重金，授予商人們專賣特權。', 'Take the gold and grant the merchants their monopoly.', 'Взять золото и дать купцам монополию.')
o('rich_petition', 2, '驳回请愿并对带头者课以罚金。', '駁回請願並對帶頭者課以罰金。', 'Reject the petition and fine the ringleaders.', 'Отклонить и оштрафовать зачинщиков.')
o('drought', 1, '开仓放粮平抑饥荒，代价是库存粮金。', '開倉放糧平抑饑荒，代價是庫存糧金。', 'Open the granaries to feed the people — it will cost the treasury.', 'Открыть амбары — это опустошит казну.')
o('drought', 2, '省下钱粮，让灾民自谋生路。', '省下錢糧，讓災民自謀生路。', 'Save the grain and let the people fend for themselves.', 'Сберечь зерно, народ выкрутится сам.')
o('locust', 1, '以粮代酬发动全民捕蝗。', '以糧代酬發動全民捕蝗。', 'Pay households in grain to hunt the locusts.', 'Раздать зерно за отлов саранчи.')
o('locust', 2, '置之不理，收成听天由命。', '置之不理，收成聽天由命。', 'Ignore it and leave the harvest to fate.', 'Не вмешиваться, урожай как повезёт.')
o('quake_aftermath', 1, '以工代赈，重修屋墙道路。', '以工代賑，重修屋牆道路。', 'Fund paid repair works to rebuild homes and roads.', 'Оплатить восстановление домов и дорог.')
o('quake_aftermath', 2, '不拨款，让坊间流言自生自灭。', '不撥款，讓坊間流言自生自滅。', 'Release no funds; let the rumors die on their own.', 'Ничего не выделять; слухи улягутся сами.')
o('royal_wedding', 1, '倾国库之力大办婚宴，扬威列国。', '傾國庫之力大辦婚宴，揚威列國。', 'Throw a lavish feast the whole world will remember.', 'Устроить пир, который запомнит весь мир.')
o('royal_wedding', 2, '一切从简，礼数周全即可。', '一切從簡，禮數周全即可。', 'Keep the rites modest but proper.', 'Скромный, но приличный обряд.')
o('minister_power', 1, '设局削权，将党羽逐出朝堂。', '設局削權，將黨羽逐出朝堂。', 'Move against the minister and purge his faction.', 'Атаковать сановника и вычистить его клан.')
o('minister_power', 2, '维持现状，避免朝局震荡。', '維持現狀，避免朝局震盪。', 'Leave things be to avoid shaking the court.', 'Оставить как есть, не трясти двор.')
o('succession_dispute', 1, '依祖制立长嗣，快刀斩乱麻。', '依祖制立長嗣，快刀斬亂麻。', 'Follow tradition and crown the elder heir.', 'По традиции — старший наследник.')
o('succession_dispute', 2, '择贤而立，重金安抚其余皇子。', '擇賢而立，重金安撫其餘皇子。', 'Pick the worthier heir and buy off the rest.', 'Выбрать достойного и задобрить остальных золотом.')
o('army_pay', 1, '足额补发欠饷，稳固军心。', '足額補發欠餉，穩固軍心。', 'Pay the arrears in full and steady the army.', 'Выплатить долг сполна — армия успокоится.')
o('army_pay', 2, '继续拖欠，把钱留给国库。', '繼續拖欠，把錢留給國庫。', 'Keep delaying; the gold stays in the treasury.', 'Тянуть дальше; золото остаётся в казне.')
o('mercenary_default', 1, '如约补足佣金，留住这批老兵。', '如約補足傭金，留住這批老兵。', 'Honor the contract and keep the veterans.', 'Соблюсти договор и удержать ветеранов.')
o('mercenary_default', 2, '借机遣散佣兵，省下军费。', '藉機遣散傭兵，省下軍費。', 'Disband them and save the wages.', 'Распустить их и сэкономить.')
o('prisoner_ransom', 1, '支付赎金换回被俘的贵族与将士。', '支付贖金換回被俘的貴族與將士。', 'Pay the ransom and bring our people home.', 'Заплатить выкуп и вернуть своих.')
o('prisoner_ransom', 2, '拒绝赎回，显示强硬姿态。', '拒絕贖回，顯示強硬姿態。', 'Refuse — show an iron face.', 'Отказаться — показать твёрдость.')
o('bread_riot', 1, '开仓平价售粮，安抚市民。', '開倉平價售糧，安撫市民。', 'Open the granaries and sell grain at fair prices.', 'Открыть амбары и продавать хлеб по честной цене.')
o('bread_riot', 2, '武力弹压，驱散人群。', '武力彈壓，驅散人群。', 'Disperse the rioters by force.', 'Разогнать бунт силой.')
o('plague', 1, '出资施药设棚，御疫于城外。', '出資施藥設棚，禦疫於城外。', 'Fund physicians and medicine to keep the plague outside.', 'Оплатить лекарей — мор останется за воротами.')
o('plague', 2, '封城观望，不做破费。', '封城觀望，不做破費。', 'Seal the gates and spend nothing.', 'Запереть ворота и не тратиться.')
o('neighbor_extort', 1, '缴纳岁币，破财免灾。', '繳納歲幣，破財免災。', 'Pay the tribute and buy peace.', 'Заплатить дань и купить мир.')
o('neighbor_extort', 2, '断然拒绝，宁可一战。', '斷然拒絕，寧可一戰。', 'Refuse outright — war if it must be.', 'Отказать наотрез — пусть будет война.')
o('marriage_alliance', 1, '应允联姻，以姻亲之盟结好强邻。', '應允聯姻，以姻親之盟結好強鄰。', 'Accept the marriage and bind the strong neighbor by blood.', 'Принять брак и связать соседа родством.')
o('marriage_alliance', 2, '婉言谢绝，保持独立自主。', '婉言謝絕，保持獨立自主。', 'Politely decline and keep independence.', 'Вежливо отказаться и сохранить независимость.')

for lang, idx in (('ch',0),('zh_tw',1),('en',2),('ru',3)):
    p = 'Locales/%s.json' % lang
    d = json.load(io.open(p, encoding='utf-8'), object_pairs_hook=collections.OrderedDict)
    added = 0
    for src in (FAM, OPT):
        for k, v in src.items():
            if k not in d:
                d[k] = v[idx]; added += 1
    json.dump(d, io.open(p, 'w', encoding='utf-8'), ensure_ascii=False, indent=2)
    io.open(p, 'a', encoding='utf-8').write('\n')
    print(lang, 'added', added, 'total', len(d))
