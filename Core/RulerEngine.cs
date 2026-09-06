using System.Collections.Generic;
using EconomyMod.Models;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 统治者性格引擎（v1.5.1）：读取**原版机制**的在位国王性格，让列国决策不再"最优解"——
    /// 数据源（全部原版，fail-closed）：
    ///   1) 国王 Actor 性格特质：greedy / deceitful / honest / content / ambitious / wise（hasTrait 读取）；
    ///   2) 原版领导者性格统计：personality_administration / _aggression / _diplomatic / _rationality
    ///      （数据键形式，反射试探 kingdom.data.get，读不到即中性 0.5）。
    /// 修正面（中强度）：
    ///   - 法典演化第三层偏差（AdjustLaw）：贪婪 税制+/教育医疗−/反腐−；欺诈 反腐−/司法−/新闻+；
    ///     诚实 反腐+/司法+；野心 军国+/常备军+；英明 教育+/司法+/反垄断+；知足 变动幅度减半；
    ///   - 政策成功率（PolicySuccessMult）：理性/行政/英明 +，欺诈 −，clamp 0.7~1.35；
    ///   - 贫富调节转向（PrefersRedistribution）：贪婪王不搞劫富济贫，诚实王高基尼时更倾向；
    ///   - 国库侵蚀（TreasurySkim）：贪婪扣国库收入 10%、欺诈 5% 进国王私囊（议会法典≥2 档减半，诚实为 0）；
    ///   - AI 事件选项倾向（OptionBias）：贪婪嫌花钱、诚实/知足厌动荡。
    /// 制度制衡：法典议会（law_parliament）≥2 档 → 全部偏差强度减半。
    /// 性格是原版持久数据，本引擎零新增存档键。
    /// </summary>
    internal static class RulerEngine
    {
        // ===== 性格统计键（原版数据键名）=====
        private static readonly string[] KeyAdministration = { "personality_administration" };
        private static readonly string[] KeyAggression = { "personality_aggression" };
        private static readonly string[] KeyDiplomatic = { "personality_diplomatic" };
        private static readonly string[] KeyRationality = { "personality_rationality" };

        /// <summary>每国缓存的统治者画像（按年在位国王 id 失效；性格是原版数据，不落盘）。</summary>
        private class RulerProfile
        {
            public long KingId;
            public bool Greedy, Deceitful, Honest, Content, Ambitious, Wise;
            public float Administration = 0.5f; // 归一化 0~1，0.5 中性
            public float Aggression = 0.5f;
            public float Diplomatic = 0.5f;
            public float Rationality = 0.5f;
            public bool StatsAvailable;         // 原版四维统计是否读得到
        }

        private static readonly Dictionary<long, RulerProfile> _profiles = new Dictionary<long, RulerProfile>(32);

        /// <summary>取在位国王画像（按年在位国王 id 缓存；未认领/AI 一视同仁）。</summary>
        private static RulerProfile GetProfile(Kingdom kingdom)
        {
            if (kingdom == null || kingdom.data == null) return null;
            long kid = kingdom.data.id;
            long kingId = 0;
            Actor king = null;
            try { king = kingdom.king; if (king != null) kingId = king.id; } catch (System.Exception) { }

            if (_profiles.TryGetValue(kid, out var p) && p.KingId == kingId) return p;

            p = new RulerProfile { KingId = kingId };
            if (king != null)
            {
                p.Greedy = HasTrait(king, "greedy");
                p.Deceitful = HasTrait(king, "deceitful");
                p.Honest = HasTrait(king, "honest");
                p.Content = HasTrait(king, "content");
                p.Ambitious = HasTrait(king, "ambitious");
                p.Wise = HasTrait(king, "wise");
            }
            p.Administration = ReadStat(kingdom, KeyAdministration);
            p.Aggression = ReadStat(kingdom, KeyAggression);
            p.Diplomatic = ReadStat(kingdom, KeyDiplomatic);
            p.Rationality = ReadStat(kingdom, KeyRationality);
            p.StatsAvailable = !(p.Administration < 0f);
            _profiles[kid] = p;
            return p;
        }

        private static bool HasTrait(Actor a, string traitId)
        {
            try { return a != null && a.hasTrait(traitId); }
            catch (System.Exception) { return false; }
        }

        /// <summary>反射试探读取原版性格统计（float，归一化 0~1）；读不到返回 -1（不可用）。</summary>
        private static float ReadStat(Kingdom kingdom, string[] keyCandidates)
        {
            if (kingdom.data == null) return -1f;
            foreach (var key in keyCandidates)
            {
                try
                {
                    var d = kingdom.data;
                    var t = d.GetType();
                    // 形态 1：get(string, out float)
                    foreach (var m in t.GetMethods())
                    {
                        if (m.Name != "get") continue;
                        var ps = m.GetParameters();
                        if (ps.Length == 2 && ps[0].ParameterType == typeof(string)
                            && ps[1].ParameterType.IsByRef && ps[1].ParameterType.GetElementType() == typeof(float))
                        {
                            var args = new object[] { key, 0f };
                            if (m.Invoke(d, args) is bool ok && ok)
                                return Mathf.Clamp((float)args[1], 0f, 1f);
                        }
                    }
                    // 形态 2：get(string) 直接返回 float
                    var m2 = t.GetMethod("get", new System.Type[] { typeof(string) });
                    if (m2 != null && m2.ReturnType == typeof(float))
                    {
                        var v = m2.Invoke(d, new object[] { key });
                        if (v is float f) return Mathf.Clamp(f, 0f, 1f);
                    }
                }
                catch (System.Exception) { }
            }
            return -1f;
        }

        /// <summary>议会制衡系数：法典议会 ≥2 档 → 0.5，否则 1。</summary>
        private static float ParliamentDamp(long kingdomId)
        {
            return LawEngine.GetLawLevel(kingdomId, LawEngine.LawParliament) >= 2 ? 0.5f : 1f;
        }

        // ===== 修正器（外部消费面）=====

        /// <summary>法典演化第三层偏差（LawAi.StyleAdjust 之后调用）。content 减半变动幅度。</summary>
        public static int AdjustLaw(long kingdomId, string key, int cur, int suggest)
        {
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.RulerPersonalityEnabled) return suggest;
            var k = GameHelpers.FindKingdom(kingdomId);
            var p = GetProfile(k);
            if (p == null) return suggest;
            float damp = ParliamentDamp(kingdomId);
            if (damp < 1f)
            {
                // 议会制衡：偏差幅度减半
                int before = suggest;
                suggest = cur + System.Math.Sign(before - cur)
                    * (int)(System.Math.Abs(before - cur) * damp + 0.5f);
            }

            if (p.Greedy)
            {
                if (key == LawEngine.LawTaxSystem) suggest += 1;
                if (key == LawEngine.LawEducation || key == LawEngine.LawHealthcare) suggest -= 1;
                if (key == LawEngine.LawAntiCorrupt) suggest -= 1;
            }
            if (p.Deceitful)
            {
                if (key == LawEngine.LawAntiCorrupt || key == LawEngine.LawJudicial) suggest -= 1;
                if (key == LawEngine.LawPress) suggest += 1;
            }
            if (p.Honest)
            {
                if (key == LawEngine.LawAntiCorrupt || key == LawEngine.LawJudicial
                    || key == LawEngine.LawPress) suggest += 1;
            }
            if (p.Ambitious)
            {
                if (key == LawEngine.LawMilitarism || key == LawEngine.LawStandingArmy) suggest += 1;
            }
            if (p.Wise)
            {
                if (key == LawEngine.LawEducation || key == LawEngine.LawJudicial
                    || key == LawEngine.LawAntimonopoly) suggest += 1;
            }
            if (p.Content)
            {
                // 知足：减半变动幅度（少折腾）
                suggest = cur + System.Math.Sign(suggest - cur)
                    * ((System.Math.Abs(suggest - cur) + 1) / 2);
            }
            return System.Math.Max(0, System.Math.Min(LawEngine.LawTiers - 1, suggest));
        }

        /// <summary>政策成功率乘数（0.7~1.35）：理性/行政/英明加成，欺诈减益。</summary>
        public static float PolicySuccessMult(long kingdomId)
        {
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.RulerPersonalityEnabled) return 1f;
            var k = GameHelpers.FindKingdom(kingdomId);
            var p = GetProfile(k);
            if (p == null) return 1f;
            float m = 1f;
            if (p.StatsAvailable)
            {
                m += (p.Rationality - 0.5f) * 0.3f;
                m += (p.Administration - 0.5f) * 0.2f;
            }
            if (p.Wise) m += 0.1f;
            if (p.Deceitful) m -= 0.15f;
            return Mathf.Clamp(m, 0.7f, 1.35f);
        }

        /// <summary>是否回避"贫富调节"：贪婪王永不主动劫富济贫；诚实王在高基尼时更倾向。</summary>
        public static bool BlocksRedistribution(long kingdomId, float gini)
        {
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.RulerPersonalityEnabled) return false;
            var k = GameHelpers.FindKingdom(kingdomId);
            var p = GetProfile(k);
            if (p == null) return false;
            if (p.Greedy) return true;
            if (p.Honest && gini > 0.6f) return false; // 诚实王在贫富悬殊时更倾向推动调节
            return false;
        }

        /// <summary>国库侵蚀（贪婪/欺诈王私吞财政收入的nfirma比例）；议会≥2 减半，诚实为 0。</summary>
        public static long TreasurySkim(long kingdomId, long income)
        {
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.RulerPersonalityEnabled || income <= 0) return 0;
            var k = GameHelpers.FindKingdom(kingdomId);
            var p = GetProfile(k);
            if (p == null || p.Honest) return 0;
            float ratio = p.Greedy ? 0.10f : p.Deceitful ? 0.05f : 0f;
            if (ratio <= 0f) return 0;
            ratio *= ParliamentDamp(kingdomId);
            return (long)(income * ratio);
        }

        /// <summary>AI 事件选项倾向：贪婪嫌支出、诚实/知足厌动荡。返回权重增量（可负）。</summary>
        public static float OptionBias(long kingdomId, DecisionEvents.EventOption option)
        {
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.RulerPersonalityEnabled || option == null) return 0f;
            var k = GameHelpers.FindKingdom(kingdomId);
            var p = GetProfile(k);
            if (p == null) return 0f;
            float bias = 0f;
            if (p.Greedy && option.treasuryGdpRatio < 0f) bias -= 0.6f;       // 贪婪嫌花钱
            if (p.Greedy && option.treasuryGdpRatio > 0f) bias += 0.3f;       // 贪婪爱进账
            if ((p.Honest || p.Content) && option.unrest) bias -= 0.6f;       // 诚实/知足厌动荡
            return bias;
        }

        /// <summary>统治者描述（财税页展示）：在位国王名 + 原版性格特质名。</summary>
        public static string DescribeRuler(long kingdomId)
        {
            var k = GameHelpers.FindKingdom(kingdomId);
            if (k == null || k.data == null) return null;
            Actor king = null;
            try { king = k.king; } catch (System.Exception) { }
            var p = GetProfile(k);
            var parts = new List<string>();
            if (p != null)
            {
                if (p.Greedy) parts.Add(Services.LocalizationService.Get("ruler_trait_greedy"));
                if (p.Deceitful) parts.Add(Services.LocalizationService.Get("ruler_trait_deceitful"));
                if (p.Honest) parts.Add(Services.LocalizationService.Get("ruler_trait_honest"));
                if (p.Content) parts.Add(Services.LocalizationService.Get("ruler_trait_content"));
                if (p.Ambitious) parts.Add(Services.LocalizationService.Get("ruler_trait_ambitious"));
                if (p.Wise) parts.Add(Services.LocalizationService.Get("ruler_trait_wise"));
            }
            string name = king != null ? GameHelpers.SafeName(king) : GameHelpers.SafeKingdomName(k);
            string traitStr = parts.Count > 0 ? string.Join("·", parts.ToArray())
                : Services.LocalizationService.Get("ruler_trait_none");
            return name + "（" + traitStr + "）";
        }

        public static void Reset()
        {
            _profiles.Clear();
        }
    }
}
