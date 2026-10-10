using System.Collections.Generic;
using EconomyMod.Models;
using EconomyMod.Services;
using Newtonsoft.Json;
using UnityEngine;

namespace EconomyMod.Core
{
    /// <summary>抉择事件：**存档读写**部分（W3.4 拆分自 DecisionEvents.cs）。
    /// 键位由 Core/SaveSchema.cs 单一真源定义（rb_ev_pending / rb_ev_cooldown /
    /// rb_ev_lastGlobal / rb_ev_chains），读写两侧必须成对修改。</summary>
    public static partial class DecisionEvents
    {
        // ===== 存档（NationSave 调用；rb_ev_* 三键）=====

        /// <summary>序列化挂起/冷却/全局冷却（culture 不变式）。</summary>
        public static void Serialize(System.Action<string, string> write)
        {
            var sb = new System.Text.StringBuilder(128);
            for (int i = 0; i < _pending.Count; i++)
            {
                var p = _pending[i];
                sb.Append(p.Def.id).Append('|').Append(p.KingdomId).Append('|').Append(p.ElapsedYears).Append(';');
            }
            write(SaveSchema.EventPending, sb.ToString());

            sb.Length = 0;
            foreach (var kv in _readyYear)
                sb.Append(kv.Key).Append(':').Append(kv.Value).Append(';');
            write(SaveSchema.EventCooldown, sb.ToString());

            write(SaveSchema.EventLastGlobal, _lastGlobalYear == int.MinValue
                ? ""
                : _lastGlobalYear.ToString(System.Globalization.CultureInfo.InvariantCulture));

            var csb = new System.Text.StringBuilder(64);
            for (int i = 0; i < _chains.Count; i++)
            {
                var c = _chains[i];
                csb.Append(c.DefId).Append('|').Append(c.KingdomId).Append('|').Append(c.DueYear).Append(';');
            }
            write(SaveSchema.EventChains, csb.ToString());
        }

        /// <summary>读档恢复（缺失/解析失败回退本局记忆）。</summary>
        public static void Restore(string pending, string cooldown, string lastGlobal, string chains)
        {
            _pending.Clear();
            try
            {
                if (!string.IsNullOrEmpty(pending))
                {
                    foreach (var item in pending.Split(';'))
                    {
                        if (string.IsNullOrEmpty(item)) continue;
                        var f = item.Split('|');
                        if (f.Length < 3) continue;
                        var def = FindDef(f[0]);
                        if (def == null) continue; // 事件已从 JSON 移除 → 丢弃
                        if (!long.TryParse(f[1], out long kid)) continue;
                        if (!int.TryParse(f[2], out int elapsed)) continue;
                        if (_pending.Count >= MaxPending) break;
                        var kingdom = GameHelpers.FindKingdom(kid);
                        _pending.Add(new PendingEvent
                        {
                            Def = def,
                            KingdomId = kid,
                            KingdomName = kingdom != null ? GameHelpers.SafeKingdomName(kingdom) : "",
                            ElapsedYears = System.Math.Max(0, elapsed)
                        });
                    }
                }
            }
            catch (System.Exception) { }

            _readyYear.Clear();
            try
            {
                if (!string.IsNullOrEmpty(cooldown))
                {
                    foreach (var item in cooldown.Split(';'))
                    {
                        if (string.IsNullOrEmpty(item)) continue;
                        var f = item.Split(':');
                        if (f.Length < 2) continue;
                        if (FindDef(f[0]) == null) continue;
                        if (int.TryParse(f[1], out int y)) _readyYear[f[0]] = y;
                    }
                }
            }
            catch (System.Exception) { }

            _lastGlobalYear = int.MinValue;
            try
            {
                if (!string.IsNullOrEmpty(lastGlobal) && int.TryParse(lastGlobal, out int y)) _lastGlobalYear = y;
            }
            catch (System.Exception) { }

            _chains.Clear();
            try
            {
                if (!string.IsNullOrEmpty(chains))
                {
                    foreach (var item in chains.Split(';'))
                    {
                        if (string.IsNullOrEmpty(item)) continue;
                        var f = item.Split('|');
                        if (f.Length < 3) continue;
                        if (FindDef(f[0]) == null) continue;
                        if (!long.TryParse(f[1], out long kid)) continue;
                        if (!int.TryParse(f[2], out int due)) continue;
                        _chains.Add(new ChainSpawn { DefId = f[0], KingdomId = kid, DueYear = due });
                    }
                }
            }
            catch (System.Exception) { }
        }

    }
}
