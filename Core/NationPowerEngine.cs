using System.Collections.Generic;
using EconomyMod.Models;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 富国强兵引擎（v1.4.1）：把认领国的经济优势转化为国民战斗加成——"小国打大国"。
    /// 每年（年度管线 Nation 阶段之后）评估一次：
    ///   基础档 = 本国人均财富 / 全球人均财富（小国人均高 → 档位高）；
    ///   军事法典加成：LawMods.Military 每 +0.2 升一档；
    ///   战争金库：金库 ≥ 人口×人均×0.2 额外升一档；档位 clamp 0~3。
    /// 档位变化时才遍历国民增删特质（其余年份 O(1)），特质为真实 Actor 特质（damage/armor）。
    /// 三档加成（大加强，正面硬撼人口大国）：
    ///   壹档 伤害+25 护甲+15 ｜ 贰档 +50/+30 ｜ 叁档 +90/+55。
    /// </summary>
    internal static class NationPowerEngine
    {
        public const string TraitTier1 = "nat_power_1";
        public const string TraitTier2 = "nat_power_2";
        public const string TraitTier3 = "nat_power_3";

        private static readonly Dictionary<long, int> _state = new Dictionary<long, int>();

        // 复用缓冲（单线程年度评估）
        private static readonly List<Actor> _memberPool = new List<Actor>(64);
        private static readonly List<long> _staleIds = new List<long>();

        /// <summary>每年评估（AnnualStage.Nation 内、LawEngine.RunAnnual 之后）。</summary>
        public static void Evaluate()
        {
            var cfg = UnrestConfig.Instance;
            if (World.world == null) return;
            long kid = NationEngine.NationKingdomId;

            if (cfg == null || !cfg.NationPlayEnabled || kid == 0)
            {
                ClearAllTraits();
                return;
            }

            var kingdom = GameHelpers.FindKingdom(kid);
            if (kingdom == null)
            {
                ClearAllTraits();
                return;
            }

            int tier = ComputeTier(kid);
            int cur;
            _state.TryGetValue(kid, out cur);
            if (cur == tier) return; // 档位未变，O(1) 早退

            string oldTrait = TraitOf(cur);
            string newTrait = TraitOf(tier);
            if (oldTrait != null) RemoveMemberTrait(kingdom, oldTrait);
            if (newTrait != null) AddMemberTrait(kingdom, newTrait);
            _state[kid] = tier;

            var stats = NationEngine.NationStats();
            GameHelpers.Log($"[ClassicalEconomics] 富国强兵 <{NationEngine.NationName}> 经济档位 {cur}→{tier}" +
                            $"（人均比={(stats != null && EconomyEngine.AvgWealth > 0f ? stats.AvgWealth / EconomyEngine.AvgWealth : 0f):F2}）");
        }

        /// <summary>档位：人均比定基础档，军事法典与战争金库各可加档。</summary>
        private static int ComputeTier(long kid)
        {
            var stats = NationEngine.NationStats();
            float worldAvg = EconomyEngine.AvgWealth;
            int tier = 0;
            if (stats != null && worldAvg > 0.01f)
            {
                float ratio = stats.AvgWealth / worldAvg;
                if (ratio >= 2.0f) tier = 3;
                else if (ratio >= 1.4f) tier = 2;
                else if (ratio >= 0.9f) tier = 1;
            }
            // 军事法典：每 +0.2 军力乘数加一档（军国主义/常备军/征兵堆叠）
            float mil = LawEngine.GetMods(kid).Military;
            if (mil > 0f) tier += Mathf.FloorToInt(mil / 0.2f);
            // 战争金库：金库 ≥ 人口×人均×0.2 加一档
            if (stats != null)
            {
                float chest = (stats.ActorCount * stats.AvgWealth) * 0.2f;
                if (chest > 0f && NationEngine.Treasury >= (long)chest) tier++;
            }
            return Mathf.Clamp(tier, 0, 3);
        }

        private static string TraitOf(int tier)
        {
            switch (tier)
            {
                case 1: return TraitTier1;
                case 2: return TraitTier2;
                case 3: return TraitTier3;
                default: return null;
            }
        }

        /// <summary>世界失效：仅清 Actor 引用缓冲，保留档位状态。</summary>
        public static void ClearWorldReferences()
        {
            _memberPool.Clear();
        }

        /// <summary>全量重置（新地图/关玩法）：清档位并移除已挂特质由下一次评估兜底。</summary>
        public static void Reset()
        {
            ClearAllTraits();
            _state.Clear();
        }

        private static void ClearAllTraits()
        {
            if (_state.Count == 0) return;
            var stale = _staleIds;
            stale.Clear();
            foreach (var kv in _state)
            {
                var k = GameHelpers.FindKingdom(kv.Key);
                string trait = TraitOf(kv.Value);
                if (k != null && trait != null) RemoveMemberTrait(k, trait);
                stale.Add(kv.Key);
            }
            foreach (var id in stale) _state.Remove(id);
        }

        private static void AddMemberTrait(Kingdom kingdom, string traitId)
        {
            if (kingdom == null || kingdom.units == null) return;
            var pool = _memberPool;
            pool.Clear();
            foreach (var a in kingdom.units)
            {
                if (a == null || !a.isAlive()) continue;
                if (!GameHelpers.IsCivilizedActor(a)) continue;
                pool.Add(a);
            }
            for (int i = 0; i < pool.Count; i++)
            {
                try { if (!pool[i].hasTrait(traitId)) pool[i].addTrait(traitId, true); }
                catch (System.Exception) { }
            }
        }

        private static void RemoveMemberTrait(Kingdom kingdom, string traitId)
        {
            if (kingdom == null || kingdom.units == null) return;
            var pool = _memberPool;
            pool.Clear();
            foreach (var a in kingdom.units)
            {
                if (a == null || !a.isAlive()) continue;
                if (!GameHelpers.IsCivilizedActor(a)) continue;
                pool.Add(a);
            }
            for (int i = 0; i < pool.Count; i++)
            {
                try { if (pool[i].hasTrait(traitId)) pool[i].removeTrait(traitId); }
                catch (System.Exception) { }
            }
        }
    }
}
