using System.Collections.Generic;
using EconomyMod.Models;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 贸易军力引擎（v0.13）：把贸易顺差/逆差映射为国民战斗能力——
    /// 顺差国（净出口占 GDP 比例 ≥ 阈值）国民获得伤害/护甲加成，
    /// 逆差国（净进口占 GDP 比例 ≥ 阈值）国民获得伤害惩罚。
    /// 数据源为 EconomyEngine.KingdomStats（主线程纯数据，后台已算好 TradeBalance/GDP），
    /// 每年评估一次 O(K) 查表；仅在档位变化时遍历王国成员加/移除特质，其余年份 O(1) 早退。
    /// 特质用真实 Actor 特质（damage/armor base_stats），与 EraEngine 国民加成同机制，真实生效。
    /// </summary>
    internal static class TradePowerEngine
    {
        /// <summary>国民 Actor 特质 id（EconomyModMain.RegisterEraTraits 注册）。</summary>
        public const string ActorTraitSurplus = "trait_trade_surplus"; // 贸易顺差：国民战斗加成
        public const string ActorTraitDeficit = "trait_trade_deficit"; // 贸易逆差：国民战斗惩罚

        /// <summary>档位：1=顺差，-1=逆差，0=中性（无特质）。</summary>
        private static readonly Dictionary<long, int> _state = new Dictionary<long, int>();

        // 复用缓冲（单例 + 主线程 Evaluate，无跨线程共享）
        private static readonly List<Actor> _memberPool = new List<Actor>(64);
        private static readonly List<long> _staleIds = new List<long>();

        /// <summary>每年评估（FinishCycle 里、EraEngine 之后调用）。</summary>
        public static void Evaluate()
        {
            try
            {
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.TradePowerEnabled) return;
            if (World.world == null) return;

            var stats = EconomyEngine.KingdomStats;
            foreach (var kv in stats)
            {
                long kid = kv.Key;
                var ks = kv.Value;
                if (kid == 0 || ks.ActorCount <= 0) continue;

                int target = ComputeTier(ks, cfg);
                int cur;
                _state.TryGetValue(kid, out cur);
                if (cur == target) continue; // 档位未变，O(1) 跳过

                var kingdom = GameHelpers.FindKingdom(kid);
                if (kingdom == null) { _state.Remove(kid); continue; }

                // 切换：先移除旧特质，再加新特质（中性=两者都无）
                if (cur == 1) RemoveMemberTrait(kingdom, ActorTraitSurplus);
                else if (cur == -1) RemoveMemberTrait(kingdom, ActorTraitDeficit);

                if (target == 1) AddMemberTrait(kingdom, ActorTraitSurplus);
                else if (target == -1) AddMemberTrait(kingdom, ActorTraitDeficit);

                _state[kid] = target;
                GameHelpers.Log($"[ClassicalEconomics] 贸易军力 <{ks.KingdomName}> 顺差率={RatioOf(ks):F2} → 档位{target}");
            }

            // 清理已消失王国的档位状态（防字典缓慢膨胀）
            _staleIds.Clear();
            foreach (var kid in _state.Keys)
                if (!stats.ContainsKey(kid)) _staleIds.Add(kid);
            foreach (var kid in _staleIds) _state.Remove(kid);
            }
            finally
            {
                ClearWorldReferences();
            }
        }

        /// <summary>清空仅用于当前世界的 Actor 引用，保留王国 ID 档位状态。</summary>
        public static void ClearWorldReferences()
        {
            _memberPool.Clear();
        }

        /// <summary>计算顺差率 = 净顺差 / GDP（GDP≤0 时为 0，中性）。</summary>
        private static float RatioOf(KingdomStats ks)
        {
            long gdp = ks.GDP;
            if (gdp <= 0) return 0f;
            return (float)ks.TradeBalance / gdp;
        }

        /// <summary>按顺差率分档（正=顺差，负=逆差，中间=中性）。</summary>
        private static int ComputeTier(KingdomStats ks, UnrestConfig cfg)
        {
            // 法典：军力修正（征兵/军国主义整体抬档，和平主义/武器管制压档；仅在顺差侧生效）
            float mil = CodexEngine.GetMods(ks.KingdomId).Military;
            if (mil != 0f && ks.TradeBalance > 0f)
                return mil > 0f ? 1 : 0;

            float ratio = RatioOf(ks);
            if (ratio >= cfg.TradeSurplusRatio) return 1;
            if (ratio <= -cfg.TradeDeficitRatio) return -1;
            return 0;
        }

        /// <summary>给王国存活文明成员补贸易特质（已有则跳过）。</summary>
        private static void AddMemberTrait(Kingdom kingdom, string actorTrait)
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
                try { if (!pool[i].hasTrait(actorTrait)) pool[i].addTrait(actorTrait, true); }
                catch (System.Exception) { }
            }
        }

        /// <summary>给王国存活文明成员移除贸易特质。</summary>
        private static void RemoveMemberTrait(Kingdom kingdom, string actorTrait)
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
                try { if (pool[i].hasTrait(actorTrait)) pool[i].removeTrait(actorTrait); }
                catch (System.Exception) { }
            }
        }

        /// <summary>重置（新地图/新游戏）。</summary>
        public static void Reset()
        {
            ClearWorldReferences();
            _state.Clear();
        }
    }
}
