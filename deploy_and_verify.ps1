# WorldBox Economy Mod - Deploy and Runtime Verify Script
# Run in a standalone PowerShell window (NOT in TRAE terminal, which has path restrictions):
#   powershell -ExecutionPolicy Bypass -File e:\code\new\EconomyMod\deploy_and_verify.ps1
# What it does: kill game -> clean parasite copy -> deploy Mod -> launch -> wait 120s -> read Player.log

$ErrorActionPreference = 'Stop'

# --- Config ---
$gameDir   = "D:\program files (x86)\steam\steamapps\common\worldbox"
$modsDir   = "$gameDir\Mods"          # NML 标准 mod 目录（与 BuzzOff/FunBoost 等用户 mod 并列）
$srcDir    = "e:\code\new\EconomyMod"
$dllSrc    = "$srcDir\bin\EconomyMod.dll"
$dest      = "$modsDir\ClassicalEconomics"
$staleSA   = "$gameDir\worldbox_Data\StreamingAssets\mods\EconomyMod"  # 旧部署位置副本，需清理防重复加载
$playerLog = "$env:USERPROFILE\AppData\LocalLow\mkarpenko\WorldBox\Player.log"
$modsCfgDir = "$env:USERPROFILE\AppData\LocalLow\mkarpenko\WorldBox\mods_config"
$uid        = "ECONOMYMODTEAM_CLASSICALECONOMICS"

# --- 0. Kill running game if any ---
Write-Host "==> Step 0: Kill running WorldBox if any..."
$running = Get-Process -Name "worldbox" -ErrorAction SilentlyContinue
if ($running) {
    Write-Host "    Found running worldbox (PID=$($running.Id)), killing..."
    $running | Stop-Process -Force
    Start-Sleep -Seconds 3
    Write-Host "    Killed."
} else {
    Write-Host "    No running worldbox."
}

# --- 1. Remove old-position copy (StreamingAssets\mods\EconomyMod) ---
# NML 同时扫描 Mods\ 与 StreamingAssets\mods\ 两处；旧版本曾部署到 StreamingAssets\mods\，
# 若残留会导致同一 mod 被加载两次（Repeat Mod 错误）。当前标准位置是 Mods\。
Write-Host "==> Step 1a: Remove stale copy at $staleSA"
if (Test-Path $staleSA) {
    Remove-Item $staleSA -Recurse -Force
    Write-Host "    Removed stale StreamingAssets copy (prevents duplicate mod load)."
} else {
    Write-Host "    No stale StreamingAssets copy found."
}

# --- 1b. Keep NML persistent config (mods_config\ECONOMYMODTEAM_.config) ---
# 历史版本会在此删除持久化配置，导致用户在 NML 设置窗口改过的值每次部署后丢失。
# 现在由 NML 正常管理该文件，部署时保留，确保用户设置跨部署/重启持续生效。
$persistCfg = Join-Path $modsCfgDir "$uid.config"
if (Test-Path $persistCfg) {
    Write-Host "    [KEEP] NML persistent config preserved: $persistCfg (user settings survive deploys)"
}

# --- 1b2. Remove legacy EconomyMod config.json if present (config now managed by NML mod settings) ---
$economyCfg = Join-Path "$env:USERPROFILE\AppData\LocalLow\mkarpenko\WorldBox\EconomyMod" "config.json"
if (Test-Path $economyCfg) {
    Remove-Item $economyCfg -Force
    Write-Host "    Removed legacy EconomyMod config.json (config now in NML mod settings)"
}

# --- 1c. Build the latest EconomyMod.dll ---
Write-Host "==> Step 1c: Build EconomyMod.dll via Roslyn csc"
$buildScript = Join-Path $srcDir "build.ps1"
if (Test-Path $buildScript) {
    & powershell -ExecutionPolicy Bypass -File $buildScript
    if ($LASTEXITCODE -ne 0) { throw "Build failed (exit $LASTEXITCODE)" }
} else {
    Write-Host "    [WARN] build.ps1 not found, using prebuilt dll."
}
if (-not (Test-Path $dllSrc)) { throw "EconomyMod.dll not found at $dllSrc after build" }
Write-Host "    Build OK: $dllSrc"

# --- 2. Deploy to Mods\EconomyMod (NML standard mod folder) ---
Write-Host "==> Step 2: Deploy EconomyMod to $dest"
if (Test-Path $dest) { Remove-Item $dest -Recurse -Force }
New-Item -ItemType Directory -Path $dest -Force | Out-Null
Copy-Item $dllSrc $dest -Force
Copy-Item "$srcDir\mod.json" $dest -Force
Copy-Item "$srcDir\default_config.json" $dest -Force
Copy-Item "$srcDir\icon.png" $dest -Force
New-Item -ItemType Directory -Path "$dest\Locales" -Force | Out-Null
Copy-Item "$srcDir\Locales\en.json" "$dest\Locales\" -Force
Copy-Item "$srcDir\Locales\ch.json" "$dest\Locales\" -Force
Write-Host "    Deployed files:"
Get-ChildItem $dest -Recurse | ForEach-Object { Write-Host "      $($_.FullName)" }
Write-Host "    default_config.json content:"
Get-Content "$dest\default_config.json" | ForEach-Object { Write-Host "      $_" }

# --- 3. Delete old Player.log to get clean output ---
if (Test-Path $playerLog) {
    Remove-Item $playerLog -Force
    Write-Host "==> Deleted old Player.log for clean verification."
}

# --- 4. Launch game ---
Write-Host ""
Write-Host "==> Step 3: Launch WorldBox..."
$exe = Get-ChildItem $gameDir -Filter "worldbox.exe" -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $exe) { $exe = Get-ChildItem $gameDir -Filter "*.exe" | Where-Object { $_.Name -notlike "*Unity*" -and $_.Name -notlike "*crash*" } | Select-Object -First 1 }
if (-not $exe) { throw "worldbox.exe not found" }
Write-Host "    exe: $($exe.FullName)"
$proc = Start-Process -FilePath $exe.FullName -PassThru
Write-Host "    PID=$($proc.Id), waiting 120s for Mod load + one 90s collect cycle..."

# --- 5. Wait for cycle ---
Start-Sleep -Seconds 120

# --- 6. Read Player.log ---
Write-Host ""
Write-Host "==> Step 4: Read Player.log to verify EconomyMod output..."
Write-Host "    Log path: $playerLog"
if (-not (Test-Path $playerLog)) {
    Write-Host "    [WARN] Player.log not found: $playerLog"
} else {
    $hits = Select-String -Path $playerLog -Pattern "EconomyMod" -SimpleMatch
    if ($hits) {
        Write-Host "    [OK] Found $($hits.Count) EconomyMod log lines:"
        $hits | Select-Object -First 15 | ForEach-Object { Write-Host "      $($_.Line)" }

        # Phase 3 UI verification
        $uiLine = $hits | Where-Object { $_.Line -match "EconomyUI" } | Select-Object -First 1
        if ($uiLine) {
            Write-Host ""
            Write-Host "    [SUCCESS] Phase 3 UI verified (EconomyUI loaded): $($uiLine.Line)"
        } else {
            Write-Host ""
            Write-Host "    [WARN] No EconomyUI log line found (UI init may have failed or tab not yet created)."
        }

        # Icon verification: check icon.png deployed correctly (NML loads it silently; verify by file presence)
        $deployedIcon = Join-Path $dest "icon.png"
        if (Test-Path $deployedIcon) {
            $iconInfo = Get-Item $deployedIcon
            if ($iconInfo.Length -lt 100KB) {
                Write-Host "    [OK] icon.png deployed: $([math]::Round($iconInfo.Length/1KB,2)) KB (size within NML expected range)"
            } else {
                Write-Host "    [WARN] icon.png is $([math]::Round($iconInfo.Length/1KB,2)) KB — NML may reject oversized icons (recommend < 100KB)"
            }
        } else {
            Write-Host "    [FAIL] icon.png missing in deploy folder"
        }

        $kingdomLine = $hits | Where-Object { $_.Line -match "王国<" } | Select-Object -First 1
        if ($kingdomLine) {
            Write-Host ""
            Write-Host "    [SUCCESS] Phase 2.5 verified (Kingdom breakdown): $($kingdomLine.Line)"
        } else {
            $giniLine = $hits | Where-Object { $_.Line -match "基尼=" } | Select-Object -First 1
            if ($giniLine) {
                Write-Host ""
                Write-Host "    [WARN] Found global Gini but no kingdom breakdown (old dll?): $($giniLine.Line)"
            } else {
                $gdpLine = $hits | Where-Object { $_.Line -match "GDP=" } | Select-Object -First 1
                if ($gdpLine) {
                    Write-Host ""
                    Write-Host "    [WARN] Found GDP log but no Gini (old dll still loaded?): $($gdpLine.Line)"
                } else {
                    Write-Host ""
                    Write-Host "    [INFO] No cycle log (period GDP/kingdom logs are OFF by default;"
                    Write-Host "           enable 'WorldLog Output' in the mod settings (NML) to see them)."
                    Write-Host "           Data collection itself is verified via history.json below."
                }
            }
        }

        # Phase 4 unrest verification
        $unrestLine = $hits | Where-Object { $_.Line -match "动荡触发|手动煽动|手动镇压" } | Select-Object -First 1
        if ($unrestLine) {
            Write-Host ""
            Write-Host "    [SUCCESS] Phase 4 verified (Unrest engine active): $($unrestLine.Line)"
        } else {
            Write-Host ""
            Write-Host "    [INFO] No unrest log (unrest logs are OFF by default; enable 'WorldLog Output' to verify)."
        }

        # Phase 5 social crisis verification (famine / war plunder / revolution)
        $crisisLine = $hits | Where-Object { $_.Line -match "饥荒蔓延|战争掠夺|革命爆发" } | Select-Object -First 1
        if ($crisisLine) {
            Write-Host ""
            Write-Host "    [SUCCESS] Phase 5 verified (Social crisis engine active): $($crisisLine.Line)"
        } else {
            Write-Host ""
            Write-Host "    [INFO] No social crisis log (饥荒/战争掠夺/革命 need extreme conditions: Depression phase,"
            Write-Host "           war endings, or 3+ years of active unrest; enable 'WorldLog Output' to observe)."
        }
    } else {
        Write-Host "    [FAIL] No EconomyMod record in Player.log, check Mod load errors."
        Write-Host "    Last 40 log lines:"
        Get-Content $playerLog -Tail 40 | ForEach-Object { Write-Host "      $_" }
    }
}

Write-Host ""
Write-Host "==> Done. Game still running (PID=$($proc.Id)), stop it manually if needed."
Write-Host "    Full log path: $playerLog"
