using System.Collections.Generic;
using EconomyMod.Core;
using EconomyMod.Models;
using EconomyMod.Services;
using UnityEngine;
using UnityEngine.UI;

namespace EconomyMod.UI
{
    /// <summary>【W2 分层】内阁窗口的"外交页"层：外交列表/详情、旗帜图标、待处理事件行，以及政策/法令/建筑行构件。</summary>
    public partial class CabinetWindow : FloatingWindow
    {

        private void BuildDiplomacyPage()
        {
            if (_dipTargetId != 0)
            {
                var target = GameHelpers.FindKingdom(_dipTargetId);
                if (target != null && target.data != null) { BuildDiplomacyDetail(target); return; }
                _dipTargetId = 0;
            }
            AddLine(UIHelpers.L("cabinet_diplomacy"), UIStyles.Gold, 13f);
            AddLine(UIHelpers.L("cabinet_diplomacy_hint"), Muted, 11f);
            AddDivider(DividerColor);
            BuildDiplomacyList();
        }

        /// <summary>外交列表：他国（按 GDP 前 12），每国一行：好感 + 5 个动作按钮。</summary>
        /// <summary>
        /// 旗帜图腾 Sprite（原版「图腾」部分）：data.get_banner_icon_id() → kingdom_banners_library
        /// 的 BannerAsset.icons[index]（元素可能是 Sprite 或含 sprite 字段的对象）。
        /// 全反射定位（编译期签名不稳定），失败返回 null（调用方回退 getElementIcon）。
        /// </summary>
        private static Sprite GetBannerIcon(Kingdom kingdom)
        {
            try
            {
                var data = kingdom.data;
                if (data == null) return null;
                const System.Reflection.BindingFlags F = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
                var idMethod = data.GetType().GetMethod("get_banner_icon_id", F);
                if (idMethod == null) return null;
                int iconId = System.Convert.ToInt32(idMethod.Invoke(data, null));

                var amType = typeof(AssetManager);
                var libField = amType.GetField("kingdom_banners_library",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);
                if (libField == null) return null;
                var lib = libField.GetValue(null);
                if (lib == null) return null;
                var getter = lib.GetType().GetMethod("get", F);
                if (getter == null || getter.GetParameters().Length != 1) return null;
                var banner = getter.Invoke(lib, new object[] { "kingdom" });
                if (banner == null) return null;

                var iconsField = banner.GetType().GetField("icons", F);
                if (iconsField == null) return null;
                var icons = iconsField.GetValue(banner) as System.Collections.IList;
                if (icons == null || iconId < 0 || iconId >= icons.Count)
                {
                    UnityEngine.Debug.LogWarning("[ClassicalEconomics] 图腾诊断：iconId=" + iconId
                        + " icons=" + (icons != null ? icons.Count : -1) + " banner=" + banner.GetType().Name);
                    return null;
                }
                var part = icons[iconId];
                if (part == null) return null;
                var spr = part as Sprite;
                if (spr != null) return spr;
                var p = part.GetType().GetProperty("sprite", F) ?? part.GetType().GetProperty("sprite", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (p == null) return null; // 元素类型无 sprite：诊断一次
                return p.GetValue(part) as Sprite;
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning("[ClassicalEconomics] 图腾诊断异常: " + e.Message);
            }
            return null;
        }

        /// <summary>详情页：国徽 + 该国数据 + 可用外交动作，返回按钮回列表。</summary>
        private void BuildDiplomacyDetail(Kingdom target)
        {
            long kid = target.data.id;
            string name = GameHelpers.SafeKingdomName(target);

            var topRow = NewRow(2, 34f);
            // 行不强制拉伸子项，防止旗章被拉成竖条
            topRow.GetComponent<HorizontalLayoutGroup>().childForceExpandHeight = false;
            // 国徽：RulerBox 同款双层旗章（最先创建 = 排最左）——底色背景（getElementBackground + 主色）+ 徽记图标
            // （getElementIcon + 旗帜色）叠加于固定 26x26 容器，防止被行布局拉伸成大色块
            try
            {
                var flagWrap = new GameObject("FlagWrap", typeof(RectTransform));
                flagWrap.transform.SetParent(topRow.transform, false);
                var fwRt = flagWrap.GetComponent<RectTransform>();
                fwRt.anchorMin = new Vector2(0, 0.5f); fwRt.anchorMax = new Vector2(0, 0.5f);
                fwRt.pivot = new Vector2(0, 0.5f);
                fwRt.sizeDelta = new Vector2(26f, 26f);
                var fwLe = flagWrap.AddComponent<LayoutElement>();
                fwLe.preferredWidth = 26f; fwLe.preferredHeight = 26f;
                fwLe.flexibleWidth = 0f; fwLe.flexibleHeight = 0f;
                fwLe.minWidth = 26f; fwLe.minHeight = 26f;

                var bgGo = new GameObject("FlagBg", typeof(RectTransform), typeof(Image));
                bgGo.transform.SetParent(flagWrap.transform, false);
                var bgRt = bgGo.GetComponent<RectTransform>();
                bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
                bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
                var bgImg = bgGo.GetComponent<Image>();
                try { bgImg.sprite = target.getElementBackground(); } catch (System.Exception) { }
                try { bgImg.color = target.kingdomColor.getColorMain32(); } catch (System.Exception) { }

                var icGo = new GameObject("FlagIcon", typeof(RectTransform), typeof(Image));
                icGo.transform.SetParent(flagWrap.transform, false);
                var icRt = icGo.GetComponent<RectTransform>();
                icRt.anchorMin = Vector2.zero; icRt.anchorMax = Vector2.one;
                icRt.offsetMin = Vector2.zero; icRt.offsetMax = Vector2.zero;
                var icImg = icGo.GetComponent<Image>();
                // 图腾：优先官方旗帜库（banner_icon_id -> kingdom_banners_library.icons），失败回退 getElementIcon
                Sprite totem = GetBannerIcon(target);
                if (totem == null) { try { totem = target.getElementIcon(); } catch (System.Exception) { } }
                icImg.sprite = totem;
                try { icImg.color = target.kingdomColor.getColorBanner(); } catch (System.Exception) { }
            }
            catch (System.Exception) { }

            var crest = UIHelpers.CreateText(name, topRow.transform, Fs(15f), UIStyles.Gold, _gameFont, Fs(28f));
            var crestLe = crest.GetComponent<LayoutElement>();
            if (crestLe == null) crestLe = crest.AddComponent<LayoutElement>();
            crestLe.flexibleWidth = 1f;
            AddRowButton(topRow, UIHelpers.L("cabinet_dip_back"), BtnColor,
                () => { _dipTargetId = 0; RefreshNow(); }, 90f);
            AddDivider(DividerColor);

            EconomyMod.Models.KingdomStats ks;
            bool hasStats = EconomyEngine.KingdomStats.TryGetValue(kid, out ks) && ks != null;
            AddLine(UIHelpers.Lf("cabinet_dip_stat_relation",
                NationDiplomacy.GetRelationScore(target), NationDiplomacy.GetGoodwill(kid)), UIStyles.Info, 12f);
            int style = LawEngine.GetStyle(kid);
            AddLine(UIHelpers.Lf("cabinet_dip_stat_style",
                UIHelpers.L(LawEngine.StyleKeys[style])), Muted, 11f);
            if (hasStats)
            {
                AddLine(UIHelpers.Lf("cabinet_dip_stat_gdp", ks.GDP.ToString("N0")), UIStyles.Gold, 13f);
                AddLine(UIHelpers.Lf("cabinet_dip_stat_pop", ks.Population, ks.AvgWealth.ToString("F1")), Muted, 11f);
            }
            int laws = 0, pols = 0;
            for (int i = 0; i < LawEngine.LawKeys.Length; i++)
                if (LawEngine.GetLawLevel(kid, LawEngine.LawKeys[i]) > 0) laws++;
            for (int i = 0; i < LawEngine.PolicyKeys.Length; i++)
                if (LawEngine.GetPolicyLevel(kid, LawEngine.PolicyKeys[i]) > 0) pols++;
            AddLine(UIHelpers.Lf("cabinet_dip_stat_laws", laws, pols), Muted, 11f);
            bool atWar = NationDiplomacy.IsAtWarWith(target);
            AddLine(UIHelpers.L(atWar ? "cabinet_dip_stat_war" : "cabinet_dip_stat_peace"), UIStyles.Warning, 11f);
            AddDivider(DividerColor);

            AddLine(UIHelpers.L("cabinet_dip_actions"), UIStyles.Gold, 12f);
            var row1 = NewRow(3, 30f, fill: true);
            AddRowButton(row1, UIHelpers.L("cabinet_dip_war"), BtnBad, () =>
            {
                string msg; bool ok = NationDiplomacy.DeclareWar(target, out msg);
                GameHelpers.NotifyLocalized(msg, name);
                if (ok) RefreshNow();
            }, 110f, fill: true);
            AddRowButton(row1, UIHelpers.L("cabinet_dip_peace"), BtnColor, () =>
            {
                string msg; bool ok = NationDiplomacy.SueForPeace(target, out msg);
                GameHelpers.NotifyLocalized(msg, name);
                if (ok) RefreshNow();
            }, 110f, fill: true);
            AddRowButton(row1, UIHelpers.L("cabinet_dip_alliance"), BtnGood, () =>
            {
                string msg; bool ok = NationDiplomacy.FormAlliance(target, out msg);
                GameHelpers.NotifyLocalized(msg, name);
                if (ok) RefreshNow();
            }, 110f, fill: true);
            var row2 = NewRow(3, 30f, fill: true);
            int pactTier = NationDiplomacy.PactTier(kid);
            AddRowButton(row2, pactTier >= 0
                ? UIHelpers.Lf("cabinet_dip_pact_on", pactTier + 1)
                : UIHelpers.L("cabinet_dip_pact"), BtnGood, () =>
                {
                    string msg; bool ok;
                    if (pactTier >= 0) ok = NationDiplomacy.SignPact(target, pactTier + 1, out msg);
                    else ok = NationDiplomacy.SignPact(target, 0, out msg);
                    GameHelpers.NotifyLocalized(msg, name);
                    if (ok) RefreshNow();
                }, 110f);
            AddRowButton(row2, UIHelpers.L("cabinet_dip_gift"), BtnGood, () =>
            {
                string msg; bool ok = NationDiplomacy.GiveGift(target, out msg);
                GameHelpers.NotifyLocalized(msg, name);
                if (ok) RefreshNow();
            }, 110f, fill: true);
        }

        private void BuildDiplomacyList()
        {
            var mineId = NationEngine.NationKingdomId;
            if (World.world == null || World.world.kingdoms == null)
            {
                AddLine(UIHelpers.L("picker_empty"), Muted, 11f);
                return;
            }
            var kingdoms = new List<Kingdom>(World.world.kingdoms);
            kingdoms.Sort((a, b) =>
            {
                long ga = 0, gb = 0;
                if (EconomyEngine.KingdomStats.TryGetValue(a.data.id, out var ka)) ga = (long)ka.GDP;
                if (EconomyEngine.KingdomStats.TryGetValue(b.data.id, out var kb)) gb = (long)kb.GDP;
                return gb.CompareTo(ga);
            });

            int shown = 0;
            foreach (var k in kingdoms)
            {
                if (k == null || k.data == null) continue;
                if (k.data.id == mineId) continue;
                if (shown >= 40) break;

                long kid = k.data.id;
                string name = GameHelpers.SafeKingdomName(k);
                EconomyMod.Models.KingdomStats ks;
                string gdpTxt = "?";
                if (EconomyEngine.KingdomStats.TryGetValue(kid, out ks) && ks != null)
                    gdpTxt = ks.GDP.ToString("N0");
                string line = UIHelpers.Lf("cabinet_dip_list_row", name, gdpTxt,
                    NationDiplomacy.GetRelationScore(k), NationDiplomacy.GetGoodwill(kid));

                // 整行按钮：点击进入详情（列表仅按页重建；详情单独取对象，无每帧开销）
                var btn = UIHelpers.CreateButton(line, CurPage.transform, -1, Fs(30f), _gameFont, BtnColor, Fs(12f));
                btn.onClick.AddListener(() => { _dipTargetId = kid; RefreshNow(); });
                CurLines.Add(btn.gameObject);
                shown++;
            }
        }

        /// <summary>待决事件行（S3-2）：显示挂起数与最早一件的剩余年，点击打开抉择小窗。</summary>
        private void BuildPendingEventsRow()
        {
            var first = DecisionEvents.FirstPending;
            int left = first != null ? System.Math.Max(0, first.Def.timeoutYears - first.ElapsedYears) : 0;
            var row = NewRow(2, 28f);
            var txt = UIHelpers.CreateText(
                UIHelpers.Lf("cabinet_pending_row", DecisionEvents.PendingCount, left),
                row.transform, Fs(12f), UIStyles.Warning, _gameFont, Fs(24f));
            var le = txt.GetComponent<LayoutElement>();
            if (le == null) le = txt.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;
            AddRowButton(row, UIHelpers.L("cabinet_pending_open"), BtnGood, () =>
            {
                UI.EventChoiceWindow.Instance?.ShowPending();
            }, 90f);
        }

        private void BuildPolicyRow(NationEngine.PolicyKind kind, int year)
        {
            int tier = NationEngine.GetPolicyTier(kind);
            string name = NationEngine.PolicyName(kind);
            string costText = UIHelpers.Lf("cabinet_policy_cost",
                NationEngine.FormatGold((long)NationEngine.PolicyAnnualCost(kind, 0)),
                NationEngine.FormatGold((long)NationEngine.PolicyAnnualCost(kind, 1)),
                NationEngine.FormatGold((long)NationEngine.PolicyAnnualCost(kind, 2)));

            // 一行：名称+档位状态 + 费用（右侧弱色）——不再用两行大按钮
            var row = NewRow(2, 28f);
            var nameTxt = UIHelpers.CreateText(
                tier >= 0 ? UIHelpers.Lf("cabinet_policy_active", name, tier + 1) : name,
                row.transform, Fs(12f), tier >= 0 ? UIStyles.Positive : UIStyles.TextPrimary, _gameFont, Fs(24f));
            var nameLe = nameTxt.GetComponent<LayoutElement>();
            if (nameLe == null) nameLe = nameTxt.AddComponent<LayoutElement>();
            nameLe.flexibleWidth = 1f;
            var costTxt = UIHelpers.CreateText(costText, row.transform, Fs(11f), Muted, _gameFont, Fs(20f));
            var costLe = costTxt.GetComponent<LayoutElement>();
            if (costLe == null) costLe = costTxt.AddComponent<LayoutElement>();
            costLe.flexibleWidth = 1f;

            // 按钮行：小号按钮横排（启用→升档→取消），不再拉满整行
            var btnRow = NewRow(3, 28f);
            if (tier < 0)
            {
                AddRowButton(btnRow, UIHelpers.L("cabinet_enable"), BtnGood, () =>
                {
                    if (NationEngine.EnablePolicy(kind, 0, SafeYear())) RefreshNow();
                }, 90f);
                AddRowSpacer(btnRow);
            }
            else if (tier < NationEngine.TierCount - 1)
            {
                AddRowButton(btnRow, UIHelpers.L("cabinet_upgrade"), BtnGood, () =>
                {
                    if (NationEngine.EnablePolicy(kind, tier + 1, SafeYear())) RefreshNow();
                }, 90f);
                AddRowSpacer(btnRow);
            }
            AddRowButton(btnRow, UIHelpers.L("cabinet_disable"), BtnBad,
                () => { if (NationEngine.DisablePolicy(kind)) RefreshNow(); }, 90f);
            if (btnRow.transform.childCount == 1) AddRowSpacer(btnRow);
        }

        private void BuildDecreeRow(string nameKey, string costText, bool cooling, System.Action action)
        {
            string name = UIHelpers.L(nameKey);
            var row = NewRow(2, 28f);
            var nameTxt = UIHelpers.CreateText(
                cooling ? UIHelpers.Lf("cabinet_decree_cooling", name) : name,
                row.transform, Fs(13f), cooling ? Muted : UIStyles.TextPrimary, _gameFont, Fs(24f));
            var nameLe2 = nameTxt.GetComponent<LayoutElement>();
            if (nameLe2 == null) nameLe2 = nameTxt.AddComponent<LayoutElement>();
            nameLe2.flexibleWidth = 1f;
            var costTxt = UIHelpers.CreateText(costText, row.transform, Fs(11f), Muted, _gameFont, Fs(20f));
            var costLe2 = costTxt.GetComponent<LayoutElement>();
            if (costLe2 == null) costLe2 = costTxt.AddComponent<LayoutElement>();
            costLe2.flexibleWidth = 1f;
            AddRowButton(row, UIHelpers.L("cabinet_execute"), cooling ? BtnColor : BtnGood, () => action(), 100f);
        }

        private void BuildBuildingRows()
        {
            AddLine(UIHelpers.L("cabinet_build_title"), UIStyles.TextPrimary, 12f);
            var kingdom = GameHelpers.FindKingdom(NationEngine.NationKingdomId);
            if (kingdom == null) return;
            var cities = new List<City>();
            try
            {
                var cs = kingdom.getCities();
                if (cs != null) foreach (City c in cs) if (c != null) cities.Add(c);
            }
            catch (System.Exception) { }
            if (cities.Count == 0)
            {
                AddLine(UIHelpers.L("cabinet_no_cities"), Muted, 11f);
                return;
            }
            int shown = 0;
            foreach (var city in cities)
            {
                if (shown >= 12) break;
                long cityId;
                try { cityId = city.id; } catch (System.Exception) { continue; }
                string cname = GameHelpers.SafeCityName(city);
                bool has = NationEngine.IsMarketCity(cityId) || NationEngine.IsGranaryCity(cityId);
                string status = NationEngine.IsMarketCity(cityId) ? UIHelpers.L("nation_build_market")
                    : NationEngine.IsGranaryCity(cityId) ? UIHelpers.L("nation_build_granary") : null;
                AddLine(status != null ? UIHelpers.Lf("cabinet_city_built", cname, status) : cname,
                    status != null ? UIStyles.Positive : UIStyles.TextPrimary, 11f);
                if (!has)
                {
                    var row = NewRow(2);
                    AddRowButton(row, UIHelpers.L("nation_build_market"), BtnGood, () =>
                    {
                        if (NationEngine.TryBuild(GameHelpers.FindKingdom(NationEngine.NationKingdomId), city, NationEngine.BuildingKind.Market, SafeYear())) RefreshNow();
                    });
                    AddRowButton(row, UIHelpers.L("nation_build_granary"), BtnGood, () =>
                    {
                        if (NationEngine.TryBuild(GameHelpers.FindKingdom(NationEngine.NationKingdomId), city, NationEngine.BuildingKind.Granary, SafeYear())) RefreshNow();
                    });
                }
                shown++;
            }
            if (cities.Count > shown) AddLine(UIHelpers.L("cabinet_more_cities"), Muted, 11f);
        }
    }
}
