$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

# ─────────────────────────────────────────────────────────────────────────────
# W2.8 契约门禁：面板暂停必须**只恢复自己造成的暂停**、按可见窗口计数、且世界退出必归还。
#
# 保护的行为（删掉任一条都会让玩家真的受害）：
#   1. 绝不覆盖玩家自己按的暂停（Config.paused 已为 true 时不接管）；
#   2. 可见窗口计数：多窗口时任一窗口关闭不得提前恢复时间；
#   3. 世界加载中不抢暂停；
#   4. 重复 Show/Hide 不得把计数打歪；
#   5. 世界退出无条件归还（否则玩家会永久卡在暂停）。
# ─────────────────────────────────────────────────────────────────────────────

$coordPath  = Join-Path $root 'UI\UIPauseCoordinator.cs'
$windowPath = Join-Path $root 'UI\FloatingWindow.cs'
$cfgPath    = Join-Path $root 'default_config.json'
$cbPath     = Join-Path $root 'Services\ConfigCallbacks.cs'

foreach ($p in @($coordPath, $windowPath, $cfgPath, $cbPath)) {
    if (-not (Test-Path -LiteralPath $p -PathType Leaf)) {
        Write-Host "PAUSE_PANEL_RED: source file not found: $p"
        exit 1
    }
}

function Get-PauseContractViolation([string]$coord, [string]$window, [string]$cfg, [string]$cb) {
    $sl = [System.Text.RegularExpressions.RegexOptions]::Singleline

    # 1) 绝不覆盖玩家的暂停 + 加载中不抢
    if (-not [Regex]::IsMatch($coord, 'if \(Config\.paused\) return;', $sl)) { return 'must-not-override-player-pause' }
    if (-not [Regex]::IsMatch($coord, 'if \(Config\.worldLoading\) return;', $sl)) { return 'must-not-pause-while-world-loading' }

    # 2) 只在持有权为真时恢复（否则会把玩家的暂停一起解除）
    if (-not [Regex]::IsMatch($coord, 'if \(!_ownsPause\) return;', $sl)) { return 'restore-must-check-ownership' }
    if (-not [Regex]::IsMatch($coord, '_ownsPause = true;', $sl)) { return 'must-record-ownership-on-pause' }

    # 3) 可见窗口计数：0→1 暂停、1→0 恢复
    if (-not [Regex]::IsMatch($coord, '_visibleWindows\+\+;\s*\r?\n\s*if \(_visibleWindows == 1\) TryPause\(\);', $sl)) { return 'pause-on-first-visible-window' }
    if (-not [Regex]::IsMatch($coord, 'if \(_visibleWindows > 0\) _visibleWindows--;\s*\r?\n\s*if \(_visibleWindows == 0\) RestorePause\(\);', $sl)) { return 'restore-only-when-last-window-closed' }

    # 4) 世界退出无条件归还 + 开关即时生效
    if (-not [Regex]::IsMatch($coord, 'public static void ResetAll\(\)[\s\S]*?RestorePause\(\);', $sl)) { return 'ResetAll-must-restore' }
    if (-not [Regex]::IsMatch($coord, 'public static void OnSwitchChanged\(bool enabled\)', $sl)) { return 'switch-change-must-apply-immediately' }

    # 5) 窗口侧：只有可见性**真正变化**时才通知（重复 Show/Hide 不得打歪计数）
    if (-not [Regex]::IsMatch($window, 'private static void NotifyPauseCoordinator\(bool wasVisible, bool nowVisible\)[\s\S]*?if \(wasVisible == nowVisible\) return;', $sl)) { return 'window-must-notify-only-on-transition' }
    if (-not [Regex]::IsMatch($window, 'NotifyPauseCoordinator\(was, _visible\);', $sl)) { return 'Toggle-must-notify-coordinator' }
    if (-not [Regex]::IsMatch($window, 'NotifyPauseCoordinator\(was, true\);', $sl)) { return 'Show-must-notify-coordinator' }
    if (-not [Regex]::IsMatch($window, 'NotifyPauseCoordinator\(was, false\);', $sl)) { return 'Hide-must-notify-coordinator' }
    if (-not [Regex]::IsMatch($window, 'public virtual void OnWorldUnavailable\(\)[\s\S]*?UIPauseCoordinator\.ResetAll\(\);', $sl)) { return 'world-exit-must-reset-pause' }

    # 6) 配置接线：默认关 + 回调转发到协调器
    if (-not [Regex]::IsMatch($cfg, '"Id":\s*"pause_on_panel_open"[\s\S]{0,120}?"BoolVal":\s*false', $sl)) { return 'config-default-must-be-false' }
    if (-not [Regex]::IsMatch($cb, 'public static void OnPauseOnPanelOpenChanged\(bool pValue\)[\s\S]*?UnrestConfig\.Instance\.PauseOnPanelOpen = pValue;[\s\S]*?UIPauseCoordinator\.OnSwitchChanged\(pValue\);', $sl)) { return 'callback-must-forward-to-coordinator' }

    return ''
}

$coord  = [System.IO.File]::ReadAllText($coordPath)
$window = [System.IO.File]::ReadAllText($windowPath)
$cfg    = [System.IO.File]::ReadAllText($cfgPath)
$cb     = [System.IO.File]::ReadAllText($cbPath)

$violation = Get-PauseContractViolation $coord $window $cfg $cb
if ($violation -ne '') {
    Write-Host "PAUSE_PANEL_RED: $violation"
    exit 1
}

function Test-Mutation([string]$name, [string]$c, [string]$w, [string]$g, [string]$b) {
    $v = Get-PauseContractViolation $c $w $g $b
    if ($v -eq '') {
        Write-Host "PAUSE_PANEL_RED: mutation '$name' did not turn RED (gate not sensitive)"
        exit 1
    }
    Write-Host "MUTATION_RED_OK: $name -> $v"
}

Test-Mutation 'overrides-player-pause' ($coord -replace 'if \(Config\.paused\) return;', '') $window $cfg $cb
Test-Mutation 'restores-without-ownership' ($coord -replace 'if \(!_ownsPause\) return;', '') $window $cfg $cb
Test-Mutation 'notify-on-every-call' $coord ($window -replace 'if \(wasVisible == nowVisible\) return;', '') $cfg $cb
Test-Mutation 'world-exit-leaves-paused' $coord ($window -replace 'UIPauseCoordinator\.ResetAll\(\);', '') $cfg $cb
Test-Mutation 'default-true' $coord $window ($cfg -replace '("Id":\s*"pause_on_panel_open"[\s\S]{0,120}?"BoolVal":\s*)false', '$1true') $cb

Write-Host 'PAUSE_PANEL_GREEN: pause-on-panel-open contract holds + 5 mutations RED'
exit 0
