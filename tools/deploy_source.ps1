param(
    [string]$ModsRoot  = 'D:\Program Files (x86)\Steam\steamapps\common\worldbox\Mods',
    [string]$ModName   = '古典经济学',
    [switch]$SkipCacheClear
)

# ─────────────────────────────────────────────────────────────────────────────
# 源码式部署：把仓库按「发布包布局」装进游戏 Mods 目录。
#
# 为什么需要这个脚本：
#   · 本模组在 0.51 平台是**源码式模组** —— Mods\古典经济学\ 下没有 DLL，
#     NML 启动时用 Roslyn 现场编译 Source\（见 Player.log: "Compile Mod 古典经济学"）。
#   · 旧的 deploy_local.ps1 找的是「含 EconomyMod.dll 的目录」，与当前形态不符，会 DEPLOY_RED。
#   · NML 把编译产物缓存到 <游戏>\worldbox_Data\StreamingAssets\mods\NML\CompiledMods\，
#     并记录在 mod_compile_records.json。**不清缓存可能仍然跑旧代码**，故本脚本默认清掉。
#
# 布局与 tools\pack_release.py 保持一致：
#   根     mod.json / default_config.json / events.json / icon.png / STEAM_WORKSHOP_DESC_*.txt
#   资源   GameResources/ Icons/ Locales/
#   源码   Source/{EconomyModMain.cs, Core, Services, UI, Models, tools, docs}
# ─────────────────────────────────────────────────────────────────────────────

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
if (-not (Test-Path -LiteralPath (Join-Path $root 'mod.json') -PathType Leaf)) {
    Write-Host "DEPLOY_RED: mod.json not found under $root"
    exit 1
}

# ── 1) 认领目标目录：先按目录名，再按 mod.json 的 name 扫 ──
$target = Join-Path $ModsRoot $ModName
if (-not (Test-Path -LiteralPath $target -PathType Container)) {
    $target = $null
    foreach ($d in (Get-ChildItem -LiteralPath $ModsRoot -Directory -ErrorAction SilentlyContinue)) {
        $mj = Join-Path $d.FullName 'mod.json'
        if (-not (Test-Path -LiteralPath $mj)) { continue }
        try {
            $j = Get-Content -LiteralPath $mj -Raw -Encoding UTF8 | ConvertFrom-Json
            if ($j.name -eq $ModName) { $target = $d.FullName; break }
        } catch { }
    }
}
if (-not $target) {
    Write-Host "DEPLOY_RED: no mod folder named or declaring ""$ModName"" under $ModsRoot"
    exit 1
}
Write-Host "DEPLOY_TARGET: $target"

# ── 2) 保留玩家配置（NML 写的 config.json 不在发布包内，覆盖会丢设置）──
$configPath = Join-Path $target 'config.json'
$hadConfig = Test-Path -LiteralPath $configPath
$configBackup = $null
if ($hadConfig) { $configBackup = [System.IO.File]::ReadAllText($configPath) }

# ── 3) 清掉受管内容（只清发布包会覆盖的那些，不动未知文件）──
foreach ($d in @('Source', 'GameResources', 'Icons', 'Locales')) {
    $p = Join-Path $target $d
    if (Test-Path -LiteralPath $p) { Remove-Item -LiteralPath $p -Recurse -Force }
}
foreach ($f in @('mod.json', 'default_config.json', 'events.json', 'icon.png')) {
    $p = Join-Path $target $f
    if (Test-Path -LiteralPath $p) { Remove-Item -LiteralPath $p -Force }
}
Get-ChildItem -LiteralPath $target -File -Filter 'STEAM_WORKSHOP_DESC_*.txt' -ErrorAction SilentlyContinue | Remove-Item -Force

# ── 4) 投放 ──
foreach ($f in @('mod.json', 'default_config.json', 'events.json', 'icon.png')) {
    Copy-Item -LiteralPath (Join-Path $root $f) -Destination $target -Force
}
Get-ChildItem -LiteralPath $root -File -Filter 'STEAM_WORKSHOP_DESC_*.txt' | Copy-Item -Destination $target -Force
foreach ($d in @('GameResources', 'Icons', 'Locales')) {
    $s = Join-Path $root $d
    if (Test-Path -LiteralPath $s) { Copy-Item -LiteralPath $s -Destination (Join-Path $target $d) -Recurse -Force }
}
$srcRoot = Join-Path $target 'Source'
New-Item -ItemType Directory -Path $srcRoot -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $root 'EconomyModMain.cs') -Destination $srcRoot -Force
foreach ($d in @('Core', 'Services', 'UI', 'Models', 'tools', 'docs')) {
    $s = Join-Path $root $d
    if (Test-Path -LiteralPath $s) { Copy-Item -LiteralPath $s -Destination (Join-Path $srcRoot $d) -Recurse -Force }
}
# 清掉编译/缓存产物，别把 __pycache__ 之类带进游戏目录
foreach ($junk in @('__pycache__', 'obj', 'bin', '.git')) {
    Get-ChildItem -LiteralPath $srcRoot -Directory -Recurse -Force -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -eq $junk } | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
}

# ── 5) 恢复玩家配置 ──
if ($hadConfig -and $null -ne $configBackup) {
    [System.IO.File]::WriteAllText($configPath, $configBackup)
    Write-Host "DEPLOY_KEEP: player config.json preserved"
}

# ── 6) 清 NML 编译缓存（否则可能仍加载旧程序集）──
if (-not $SkipCacheClear) {
    $gameRoot = Split-Path -Parent $ModsRoot
    $nmlDir = Join-Path $gameRoot 'worldbox_Data\StreamingAssets\mods\NML'
    $cacheDir = Join-Path $nmlDir 'CompiledMods'
    $recordPath = Join-Path $nmlDir 'mod_compile_records.json'
    $cleared = @()
    if (Test-Path -LiteralPath $cacheDir) {
        Get-ChildItem -LiteralPath $cacheDir -File -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -like "*$ModName*" } |
            ForEach-Object { $cleared += $_.Name; Remove-Item -LiteralPath $_.FullName -Force }
    }
    if (Test-Path -LiteralPath $recordPath) {
        try {
            $rec = Get-Content -LiteralPath $recordPath -Raw -Encoding UTF8 | ConvertFrom-Json
            $keep = [ordered]@{}
            $removed = @()
            foreach ($prop in $rec.PSObject.Properties) {
                if ($prop.Name -like "*$ModName*") { $removed += $prop.Name } else { $keep[$prop.Name] = $prop.Value }
            }
            if ($removed.Count -gt 0) {
                $keep | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $recordPath -Encoding UTF8
                Write-Host ("DEPLOY_CACHE: record entry removed: " + ($removed -join ", "))
            }
        } catch {
            Write-Host "DEPLOY_WARN: mod_compile_records.json rewrite skipped: $($_.Exception.Message)"
        }
    }
    if ($cleared.Count -gt 0) { Write-Host ("DEPLOY_CACHE: compiled cache removed: " + ($cleared -join ", ")) }
    else { Write-Host "DEPLOY_CACHE: no compiled cache entry found (first run?)" }
}

# ── 7) 校验：Source 下 .cs 与仓库逐文件哈希比对 ──
$diff = 0; $count = 0
foreach ($d in @("", "Core", "Services", "UI", "Models")) {
    $rel = if ($d) { $d } else { "" }
    $a = if ($d) { Join-Path $root $d } else { $root }
    $b = if ($d) { Join-Path $srcRoot $d } else { $srcRoot }
    Get-ChildItem -LiteralPath $a -File -Filter *.cs -ErrorAction SilentlyContinue | ForEach-Object {
        $count++
        $t = Join-Path $b $_.Name
        if (-not (Test-Path -LiteralPath $t)) { $diff++ }
        elseif ((Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash -ne (Get-FileHash -LiteralPath $t -Algorithm SHA256).Hash) { $diff++ }
    }
}
if ($diff -ne 0) {
    Write-Host "DEPLOY_RED: $diff of $count source files differ after copy"
    exit 7
}
# ── 8) 写构建溯源文件（游戏里据此区分装的是哪一版 dev 构建）──
try {
    $gitHash = (git -C $root rev-parse --short HEAD 2>$null)
    $gitDirty = if ((git -C $root status --porcelain 2>$null)) { "dirty" } else { "clean" }
    # 注意：PowerShell 里「字符串 + 数组」会做数组拼接而不是字符串连接，
    # 所以这里一律用插值，避免 git 输出（数组）把每个值拆成单独一行。
    $buildTime = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    $modVersion = (Get-Content -LiteralPath (Join-Path $root "mod.json") -Raw -Encoding UTF8 | ConvertFrom-Json).version
    $info = @(
        "DEV BUILD (deployed by tools\deploy_source.ps1)",
        "time      : $buildTime",
        "git       : $gitHash ($gitDirty)",
        "sources   : $count .cs files, all hash-verified",
        "mod.json  : $modVersion",
        "note      : NML cache cleared; game recompiles Source\ on next launch"
    )
    [System.IO.File]::WriteAllLines((Join-Path $target "_dev-build.txt"), $info)
} catch { Write-Host "DEPLOY_WARN: build info not written: $($_.Exception.Message)" }

Write-Host "DEPLOY_GREEN: $count source files identical; installed to "
Write-Host "               $target"
Write-Host "DEPLOY_NOTE: 启动游戏后 NML 会重新编译 Source\（首次约 0.8s），请确认 Player.log 无编译错误。"
exit 0
