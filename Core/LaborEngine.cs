using EconomyMod.Models;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>
    /// 劳动分工引擎：读取原生 Actor.citizen_job（CitizenJobAsset），
    /// 按职业挂钩财富生产率——职业人口每年创造"工资"（劳动创造财富），
    /// 无业人口生产率低、不创造工资。职业 → 代码映射与生产率表均为纯数据，
    /// 供后台线程（TradeSimulationWorker）聚合每王国劳动统计复用。
    /// </summary>
    public static class LaborEngine
    {
        public const byte CodeNone = 0;   // 无业
        public const byte CodeFarm = 1;   // farmer
        public const byte CodeHunt = 2;   // hunter
        public const byte CodeWood = 3;   // woodcutter
        public const byte CodeMine = 4;   // miner / miner_deposit
        public const byte CodeBuild = 5;  // builder
        public const byte CodeOther = 6;  // 其他职业

        /// <summary>职业代码 → 财富生产率倍率（单一来源：后台线程的纯数据表）。</summary>
        /// <summary>法典工资乘数（义务教育/征兵等聚合）。</summary>
        private static float WageMult(Actor actor)
        {
            if (actor == null || actor.kingdom == null || actor.kingdom.data == null) return 1f;
            return CodexEngine.GetMods(actor.kingdom.data.id).Wage;
        }

        public static float ProductivityOf(byte code) => TradeSimulationWorker.ProductivityOf(code);

        /// <summary>读取 Actor 的原生公民职业并映射为职业代码（0=无业）；半销毁对象返回 0。</summary>
        public static byte JobCodeOf(Actor actor)
        {
            try
            {
                var job = actor.citizen_job;
                if (job == null) return CodeNone;
                switch (job.id)
                {
                    case "farmer":            return CodeFarm;
                    case "hunter":            return CodeHunt;
                    case "woodcutter":        return CodeWood;
                    case "miner":
                    case "miner_deposit":     return CodeMine;
                    case "builder":           return CodeBuild;
                    default:                  return CodeOther;
                }
            }
            catch (System.Exception) { return CodeNone; }
        }

        /// <summary>
        /// 按职业发放年度工资（劳动创造财富）。工资 = 基础工资 × 职业生产率，
        /// 最低 1 金，受 LaborEnabled / LaborWageBase 配置控制。
        /// </summary>
        public static void PayWage(Actor actor, byte code)
        {
            if (code == CodeNone) return;
            var cfg = UnrestConfig.Instance;
            if (cfg == null || !cfg.LaborEnabled) return;
            int wage = Mathf.Max(1, Mathf.RoundToInt(cfg.LaborWageBase * ProductivityOf(code) * WageMult(actor)));
            if (wage <= 0) return;
            try { actor.addMoney(wage); } catch (System.Exception) { }
        }
    }
}
