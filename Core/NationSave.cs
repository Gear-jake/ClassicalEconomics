using System.Collections.Generic;
using System.Globalization;
using System.Text;
using EconomyMod.Services;
using HarmonyLib;

namespace EconomyMod.Core
{
    /// <summary>
    /// 中央银行家存档持久化（v1.3.0）：把认领国状态（金库/认领/政策槽/建筑记录/政绩/
    /// 外交协定与好感）与 GDP 折线历史写入王国 data（rb_nat_* / rb_hist 键），
    /// 经 MapBox.saveSave（前缀写盘）与 loadSave（后缀读回）手动 Harmony 补丁——
    /// 与 LawSave 同机制（注解补丁对预编译 DLL 不可靠，首帧手动 Patch）。
    /// 任一环节异常则回退本局记忆（日志警告一次，不阻塞游戏）。
    /// </summary>
    public static class NationSave
    {
        private const string HarmonyId = "com.classicaleconomics.nationsave";
        private const int MaxSlotKeys = 5;   // 与 NationEngine.MaxSlots 一致（避免读档跨版本爆键）
        private static bool _installed;
        private static bool _loadWarned;

        /// <summary>
        /// 最近一次读档的实际路径（补丁 SaveManager.loadWorld(string,bool) 前缀捕获）。
        /// 手动槽= saves\saveN，自动槽= autosaves\&lt;epoch&gt;，工坊= main_path——
        /// 旁挂恢复端用它优先定位（读哪个存档就找哪个目录，与保存端 pFolder 天然对齐）。
        /// </summary>
        public static string LastLoadedDir { get; private set; }

        /// <summary>读档发生计数器：LoadPrefix 每次读档自增；懒加载对比它区分"读档 vs 新世界"。</summary>
        public static int LoadCounter { get; private set; }

        /// <summary>幂等安装；由 EconomyTickRunner 首帧调用。</summary>
        public static void TryInstall()
        {
            if (_installed) return;
            _installed = true;
            try
            {
                // WorldBox 0.51.2 真实存档 API：保存=SaveManager.saveWorldToDirectory(public static)，
                // 读档=SaveManager.loadWorld()（无参实例方法，手动/自动/工坊读档的唯一入口）。
                // 旧的 MapBox.saveSave/loadSave 在 0.51.2 已不存在——钩子从未安装过。
                var save = AccessTools.Method(typeof(SaveManager), "saveWorldToDirectory");
                var loadNoArg = AccessTools.Method(typeof(SaveManager), "loadWorld", new System.Type[0]);
                var loadWithArg = AccessTools.Method(typeof(SaveManager), "loadWorld", new System.Type[] { typeof(string), typeof(bool) });
                if (save == null || loadNoArg == null || loadWithArg == null)
                {
                    UnityEngine.Debug.LogWarning("[ClassicalEconomics] 中央银行家存档：SaveManager.saveWorldToDirectory/loadWorld 未找到，回退本局记忆");
                    return;
                }
                var harmony = new Harmony(HarmonyId);
                harmony.Patch(save, prefix: new HarmonyMethod(typeof(NationSave), nameof(SavePrefix)));
                harmony.Patch(loadNoArg, postfix: new HarmonyMethod(typeof(NationSave), nameof(LoadPostfix)));
                harmony.Patch(save, postfix: new HarmonyMethod(typeof(NationSave), nameof(SavePostfixSidecar)));
                // 读档路径捕获：带参重载前缀记录 pPath（无参版内部最终调它）
                harmony.Patch(loadWithArg, prefix: new HarmonyMethod(typeof(NationSave), nameof(LoadPrefix)));
                UnityEngine.Debug.Log("[ClassicalEconomics] 中央银行家存档补丁已安装（saveWorldToDirectory/loadWorld）");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning("[ClassicalEconomics] 中央银行家存档补丁安装失败: " + e.Message);
            }
        }

        /// <summary>读档前缀：记录实际路径（手动槽/自动槽/工坊）+ 递增读档计数，供旁挂恢复端区分读档/新世界。</summary>
        private static void LoadPrefix(string pPath)
        {
            try
            {
                if (string.IsNullOrEmpty(pPath)) return;
                string dir = SaveManager.folderPath(pPath);
                if (!string.IsNullOrEmpty(dir))
                {
                    LastLoadedDir = dir;
                    LoadCounter++;
                }
            }
            catch (System.Exception) { }
        }

        /// <summary>写盘前把内存状态同步进认领国 data（未认领则跳过）。</summary>
        private static void SavePrefix()
        {
            try
            {
                if (World.world == null) return;
                long nationId = NationEngine._nationKingdomId;
                var nation = GameHelpers.FindKingdom(nationId);

                // GDP 历史：全局数据，挂认领国（若有）否则第一个王国；无王国则不存
                Kingdom histHost = nation;
                if (histHost == null)
                {
                    var snapshot = GameHelpers.KingdomSnapshot();
                    if (snapshot != null && snapshot.Count > 0) histHost = snapshot[0];
                }
                if (histHost != null && histHost.data != null)
                {
                    try { histHost.data.set("rb_hist", HistoryService.Serialize()); }
                    catch (System.Exception) { }
                    // 事件流（时间线）：与历史同宿主，读档按时间正序恢复（普通+重大+类型计数）
                    try { histHost.data.set("rb_ev_stream", EventStreamService.Serialize()); }
                    catch (System.Exception) { }
                }

                if (nation == null || nation.data == null || nationId == 0) return;

                try { nation.data.set("rb_nat_kingdom", nationId.ToString(CultureInfo.InvariantCulture)); }
                catch (System.Exception) { }
                try { nation.data.set("rb_nat_treasury", NationEngine._treasury.ToString(CultureInfo.InvariantCulture)); }
                catch (System.Exception) { }

                // 政策槽：固定写满 MaxSlotKeys 个键——空槽写空串覆盖盘上旧值，
                // 防止"删除槽位后再存档，读档时旧键复活已删槽位"
                for (int i = 0; i < MaxSlotKeys; i++)
                {
                    string v = "";
                    if (i < NationEngine._slots.Count)
                    {
                        var s = NationEngine._slots[i];
                        v = string.Format(CultureInfo.InvariantCulture, "{0}:{1}:{2}:{3}",
                            (int)s.Kind, s.Tier, s.StartYear, s.TotalSpent);
                    }
                    try { nation.data.set("rb_nat_slot_" + i, v); }
                    catch (System.Exception) { }
                }

                // 建筑记录（cityId:kind;...）
                if (NationEngine._cityBuildings.Count > 0)
                {
                    var sb = new StringBuilder(256);
                    foreach (var kv in NationEngine._cityBuildings)
                        sb.Append(kv.Key).Append(':').Append(kv.Value).Append(';');
                    try { nation.data.set("rb_nat_buildings", sb.ToString()); }
                    catch (System.Exception) { }
                }

                // 政绩记录（环形按时间正序：year|key|amount|giniB|avgB|giniA|avgA|closed;...）
                // 注：v1.6.2 及以前为含 priceB/priceA 的 10 槽位，读档兼容，写盘不再含价格。
                int rc = NationEngine._recordCount;
                if (rc > 0)
                {
                    var sb = new StringBuilder(512);
                    int head = NationEngine._recordHead;
                    int start = (head - rc + NationEngine.RecordCapacity) % NationEngine.RecordCapacity;
                    var inv = CultureInfo.InvariantCulture;
                    for (int i = 0; i < rc; i++)
                    {
                        var r = NationEngine._records[(start + i) % NationEngine.RecordCapacity];
                        if (r == null) continue;
                        sb.Append(r.Year).Append('|')
                          .Append(r.Key ?? "").Append('|')
                          .Append(r.Amount).Append('|')
                          .Append(r.GiniBefore.ToString("F4", inv)).Append('|')
                          .Append(r.AvgBefore.ToString("F2", inv)).Append('|')
                          .Append(r.GiniAfter.ToString("F4", inv)).Append('|')
                          .Append(r.AvgAfter.ToString("F2", inv)).Append('|')
                          .Append(r.Closed ? '1' : '0').Append(';');
                    }
                    try { nation.data.set("rb_nat_records", sb.ToString()); }
                    catch (System.Exception) { }
                }

                // 外交协定与好感
                if (NationDiplomacy._pacts.Count > 0)
                {
                    var sb = new StringBuilder(64);
                    foreach (var kv in NationDiplomacy._pacts)
                        sb.Append(kv.Key).Append(':').Append(kv.Value).Append(';');
                    try { nation.data.set("rb_nat_pacts", sb.ToString()); }
                    catch (System.Exception) { }
                }
                if (NationDiplomacy._goodwill.Count > 0)
                {
                    var sb = new StringBuilder(64);
                    foreach (var kv in NationDiplomacy._goodwill)
                        sb.Append(kv.Key).Append(':').Append(kv.Value).Append(';');
                    try { nation.data.set("rb_nat_goodwill", sb.ToString()); }
                    catch (System.Exception) { }
                }

                // 抉择事件：挂起/冷却/全局冷却（rb_ev_*，挂认领国 data）
                DecisionEvents.Serialize((key, value) =>
                {
                    try { nation.data.set(key, value ?? ""); }
                    catch (System.Exception) { }
                });

                // 银行：三要素/商业政策/城市账本/AI 池（rb_bank_*）
                BankEngine.Serialize((key, value) =>
                {
                    try { nation.data.set(key, value ?? ""); }
                    catch (System.Exception) { }
                });
            }
            catch (System.Exception) { }
        }

        /// <summary>
        /// 保存后把历史/事件流写成存档目录旁挂文件（诡秘之主-宿命之环同款方案）：
        /// 不依赖王国 data 键能否被原版序列化，也不依赖读档钩子时序——原版保存成功后
        /// （__result != null）直接落盘到真实存档目录；读档侧由 EconomyModMain 跟踪
        /// SaveManager.currentSavePath 变化懒加载。任何异常吞掉，绝不阻断原版保存。
        /// </summary>
        private static void SavePostfixSidecar(string pFolder, SavedMap __result)
        {
            if (__result == null) return;
            try
            {
                string dir = SaveManager.folderPath(pFolder);
                if (string.IsNullOrEmpty(dir)) return;
                HistoryService.SaveToFile(dir);
                EventStreamService.SaveToFile(dir);
                UnityEngine.Debug.Log("[ClassicalEconomics] 旁挂已写入 dir=" + dir
                    + " events=" + EventStreamService.Count
                    + " major=" + EventStreamService.MajorCount);
            }
            catch (System.Exception) { }
        }

        /// <summary>读档后从王国 data 恢复中央银行家状态（缺失＝默认；失败回退本局记忆）。</summary>
        private static void LoadPostfix()
        {
            try
            {
                if (World.world == null) return;

                // ===== 旁挂面板恢复（世界已加载完，时机正确）=====
                // 读档完成：先清空内存面板（显示副本），再尝试从"该存档目录"载入 txt；
                // 有则覆盖显示，无则从零记录。存档文件夹 txt 永不被清空影响。
                // LastLoadedDir 由 LoadPrefix（loadWorld 前缀）捕获，手动/自动/工坊天然对齐。
                try
                {
                    string dir = LastLoadedDir;
                    if (string.IsNullOrEmpty(dir)) dir = SaveManager.currentSavePath;
                    if (!string.IsNullOrEmpty(dir))
                    {
                        HistoryService.ClearHistory();
                        EventStreamService.Clear();
                        HistoryService.LoadFromFile(dir);
                        EventStreamService.LoadFromFile(dir);
                        UnityEngine.Debug.Log("[ClassicalEconomics] 旁挂读档恢复 dir=" + dir
                            + " load#" + LoadCounter
                            + " hist=" + HistoryService.GetRecent(1).Count
                            + " events=" + EventStreamService.Count);
                    }
                    else
                    {
                        HistoryService.ClearHistory();
                        EventStreamService.Clear();
                        UnityEngine.Debug.Log("[ClassicalEconomics] 旁挂读档恢复 无路径（清空，从零记录）");
                    }
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogWarning("[ClassicalEconomics] 旁挂读档恢复异常: " + e.Message);
                }

                // 历史：从任意王国读 rb_hist（写盘时挂认领国或第一个王国）
                string hist = ReadAnyKingdomKey("rb_hist");
                if (hist != null) HistoryService.Restore(hist);

                // 事件流：与历史同宿主，读档恢复时间线
                string evStream = ReadAnyKingdomKey("rb_ev_stream");
                if (evStream != null) EventStreamService.Restore(evStream);

                // 抉择事件状态（挂认领国 data，与写盘同键位）
                string evPending = ReadAnyKingdomKey("rb_ev_pending");
                string evCooldown = ReadAnyKingdomKey("rb_ev_cooldown");
                string evLastGlobal = ReadAnyKingdomKey("rb_ev_lastGlobal");
                string evChains = ReadAnyKingdomKey("rb_ev_chains");
                if (evPending != null || evCooldown != null || evChains != null)
                    DecisionEvents.Restore(evPending, evCooldown, evLastGlobal, evChains);

                // 银行状态（与写盘同键位）
                string bankSet = ReadAnyKingdomKey("rb_bank_set");
                string bankLedger = ReadAnyKingdomKey("rb_bank_ledger");
                string bankAi = ReadAnyKingdomKey("rb_bank_ai");
                if (bankSet != null || bankLedger != null || bankAi != null)
                    BankEngine.Restore(bankSet, bankLedger, bankAi);

                // 认领国状态：遍历王国找到写有 rb_nat_kingdom 键的数据
                var snapshot = GameHelpers.KingdomSnapshot();
                if (snapshot == null) return;
                for (int i = 0; i < snapshot.Count; i++)
                {
                    var k = snapshot[i];
                    if (k == null || k.data == null) continue;
                    string kingIdStr = null;
                    try { k.data.get("rb_nat_kingdom", out kingIdStr); }
                    catch (System.Exception) { }
                    if (string.IsNullOrEmpty(kingIdStr)) continue;

                    RestoreNation(k);
                    return; // 只有一个认领国
                }

                // 读档恢复完成后触发年份基线对齐：防止 Tick 把读档年份误判为"新地图"
                // 而触发 ResetAllEngines 清掉刚恢复的历史/事件流/法典。
                var loaded = EconomyModMain.OnSaveLoaded;
                if (loaded != null)
                {
                    try { loaded(); } catch (System.Exception) { }
                }
            }
            catch (System.Exception)
            {
                if (!_loadWarned)
                {
                    _loadWarned = true;
                    UnityEngine.Debug.LogWarning("[ClassicalEconomics] 中央银行家存档恢复失败，本局按默认状态运行");
                }
            }
        }

        private static void RestoreNation(Kingdom nation)
        {
            var data = nation.data;
            string v = null;

            // 认领国 + 金库
            try { data.get("rb_nat_kingdom", out v); } catch (System.Exception) { }
            long kid = 0;
            if (v != null && long.TryParse(v, out kid) && GameHelpers.FindKingdom(kid) != null)
            {
                NationEngine._nationKingdomId = kid;
                NationEngine._nationName = GameHelpers.SafeKingdomName(GameHelpers.FindKingdom(kid));
            }
            else
            {
                NationEngine._nationKingdomId = 0;
                NationEngine._nationName = null;
            }
            try { data.get("rb_nat_treasury", out v); } catch (System.Exception) { }
            long treasury = 0;
            if (v != null && long.TryParse(v, out treasury)) NationEngine._treasury = treasury;

            // 政策槽
            NationEngine._slots.Clear();
            for (int i = 0; i < MaxSlotKeys; i++)
            {
                try { data.get("rb_nat_slot_" + i, out v); } catch (System.Exception) { v = null; }
                if (string.IsNullOrEmpty(v)) continue;
                string[] f = v.Split(':');
                if (f.Length < 4) continue;
                int kind, tier, startYear;
                long spent;
                if (!int.TryParse(f[0], out kind) || !int.TryParse(f[1], out tier)
                    || !int.TryParse(f[2], out startYear) || !long.TryParse(f[3], out spent)) continue;
                if (kind < 0 || kind > (int)NationEngine.PolicyKind.Tariff) continue;
                if (tier < 0 || tier >= NationEngine.TierCount) continue;
                NationEngine._slots.Add(new NationEngine.PolicySlot
                {
                    Kind = (NationEngine.PolicyKind)kind,
                    Tier = tier,
                    StartYear = startYear,
                    TotalSpent = spent
                });
            }

            // 建筑记录
            NationEngine._cityBuildings.Clear();
            try { data.get("rb_nat_buildings", out v); } catch (System.Exception) { v = null; }
            if (!string.IsNullOrEmpty(v))
            {
                foreach (var pair in v.Split(';'))
                {
                    if (string.IsNullOrEmpty(pair)) continue;
                    string[] f = pair.Split(':');
                    long cid;
                    int kind;
                    if (f.Length >= 2 && long.TryParse(f[0], out cid) && int.TryParse(f[1], out kind))
                        NationEngine._cityBuildings[cid] = kind;
                }
            }

            // 政绩记录
            NationEngine._recordHead = 0;
            NationEngine._recordCount = 0;
            for (int i = 0; i < NationEngine._records.Length; i++) NationEngine._records[i] = null;
            try { data.get("rb_nat_records", out v); } catch (System.Exception) { v = null; }
            if (!string.IsNullOrEmpty(v))
            {
                var inv = CultureInfo.InvariantCulture;
                foreach (var entry in v.Split(';'))
                {
                    if (string.IsNullOrEmpty(entry)) continue;
                    string[] f = entry.Split('|');
                    // 兼容两种格式：
                    // - 新（v1.6.3+）：year|key|amount|giniB|avgB|giniA|avgA|closed（8 槽）
                    // - 旧（≤v1.6.2）：year|key|amount|giniB|avgB|priceB|giniA|avgA|priceA|closed（10 槽，price 忽略）
                    if (f.Length < 8) continue;
                    int year;
                    long amount;
                    float gb, ab, ga, aa;
                    if (!int.TryParse(f[0], out year) || !long.TryParse(f[2], out amount)) continue;
                    if (!float.TryParse(f[3], NumberStyles.Float, inv, out gb)) gb = 0f;
                    if (!float.TryParse(f[4], NumberStyles.Float, inv, out ab)) ab = 0f;
                    if (f.Length >= 10)
                    {
                        // 旧格式：priceB 在 5、priceA 在 8，忽略；giniA/avgA 在 6/7
                        if (!float.TryParse(f[6], NumberStyles.Float, inv, out ga)) ga = 0f;
                        if (!float.TryParse(f[7], NumberStyles.Float, inv, out aa)) aa = 0f;
                    }
                    else
                    {
                        if (!float.TryParse(f[5], NumberStyles.Float, inv, out ga)) ga = 0f;
                        if (!float.TryParse(f[6], NumberStyles.Float, inv, out aa)) aa = 0f;
                    }
                    var r = new NationEngine.NationRecord
                    {
                        Year = year,
                        Key = f[1],
                        Amount = amount,
                        GiniBefore = gb, AvgBefore = ab,
                        GiniAfter = ga, AvgAfter = aa,
                        Closed = f.Length >= 10 ? f[9] == "1" : f[7] == "1"
                    };
                    NationEngine._records[NationEngine._recordHead] = r;
                    NationEngine._recordHead = (NationEngine._recordHead + 1) % NationEngine._records.Length;
                    if (NationEngine._recordCount < NationEngine._records.Length) NationEngine._recordCount++;
                }
            }

            // 外交协定与好感
            NationDiplomacy._pacts.Clear();
            try { data.get("rb_nat_pacts", out v); } catch (System.Exception) { v = null; }
            if (!string.IsNullOrEmpty(v))
            {
                foreach (var pair in v.Split(';'))
                {
                    if (string.IsNullOrEmpty(pair)) continue;
                    string[] f = pair.Split(':');
                    long pid;
                    int tier;
                    if (f.Length >= 2 && long.TryParse(f[0], out pid) && int.TryParse(f[1], out tier)
                        && tier >= 0 && tier < NationEngine.TierCount)
                        NationDiplomacy._pacts[pid] = tier;
                }
            }
            NationDiplomacy._goodwill.Clear();
            try { data.get("rb_nat_goodwill", out v); } catch (System.Exception) { v = null; }
            if (!string.IsNullOrEmpty(v))
            {
                foreach (var pair in v.Split(';'))
                {
                    if (string.IsNullOrEmpty(pair)) continue;
                    string[] f = pair.Split(':');
                    long pid;
                    int val;
                    if (f.Length >= 2 && long.TryParse(f[0], out pid) && int.TryParse(f[1], out val))
                        NationDiplomacy._goodwill[pid] = val;
                }
            }

            UnityEngine.Debug.Log($"[ClassicalEconomics] 中央银行家存档已恢复：金库={NationEngine._treasury} 政策={NationEngine._slots.Count} 建筑={NationEngine._cityBuildings.Count} 政绩={NationEngine._recordCount} 协定={NationDiplomacy._pacts.Count}");
        }

        /// <summary>遍历王国 data 读取某键（用于挂载王国不确定的全局键）。</summary>
        private static string ReadAnyKingdomKey(string key)
        {
            var snapshot = GameHelpers.KingdomSnapshot();
            if (snapshot == null) return null;
            for (int i = 0; i < snapshot.Count; i++)
            {
                var k = snapshot[i];
                if (k == null || k.data == null) continue;
                string v = null;
                try { k.data.get(key, out v); } catch (System.Exception) { }
                if (!string.IsNullOrEmpty(v)) return v;
            }
            return null;
        }
    }
}
