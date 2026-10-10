$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

# ─────────────────────────────────────────────────────────────────────────────
# Infra 门禁：仓库内所有 .ps1 必须是「带 UTF-8 BOM」或「纯 ASCII」。
#
# 由来（2026-10-10 真实事故）：新建的门禁 Test-ChartHostPersistent.ps1 无 BOM 且含中文注释，
# PowerShell 5.1 按 ANSI(CP936) 解码 → 中文注释末字节吞掉换行 → 紧跟的断言被并进注释行，
# **那条检查静默失效**（门禁照样输出 GREEN，只是少检查了一条）。
# 这类失败不会报错，只能靠本门禁拦住。
#
# 判定规则（精确、可证）：
#   · 有 BOM              → 通过（并要求整文件是合法 UTF-8）
#   · 无 BOM 但全字节 <0x80 → 通过（纯 ASCII 在 ANSI 下解码结果相同，无风险）
#   · 无 BOM 且含非 ASCII   → RED（就是上面那个坑）
# ─────────────────────────────────────────────────────────────────────────────

$script:Failures = New-Object System.Collections.Generic.List[string]

function Test-Ps1Encoding([string]$path, [System.Collections.Generic.List[string]]$failures) {
    $bytes = [System.IO.File]::ReadAllBytes($path)
    $hasBom = ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)
    if ($hasBom) {
        try {
            $strict = New-Object System.Text.UTF8Encoding($false, $true)
            [void]$strict.GetString($bytes, 3, $bytes.Length - 3)
            return $true
        } catch {
            $failures.Add("BOM but not valid UTF-8: $path")
            return $false
        }
    }
    for ($i = 0; $i -lt $bytes.Length; $i++) {
        if ($bytes[$i] -ge 0x80) {
            $failures.Add("non-ASCII without BOM (PowerShell 5.1 reads it as ANSI and may merge lines): $path")
            return $false
        }
    }
    return $true
}

# ── 扫描（排除构建产物目录与发布快照）──
$exclude = '\\(release|bin|obj|evidence|\.git)\\'
$files = @(Get-ChildItem -LiteralPath $root -Recurse -File -Filter '*.ps1' |
    Where-Object { $_.FullName -notmatch $exclude })

if ($files.Count -eq 0) {
    Write-Host 'PS1_BOM_RED: no .ps1 files found (gate would be vacuous)'
    exit 1
}

foreach ($f in $files) { [void](Test-Ps1Encoding $f.FullName $script:Failures) }

if ($script:Failures.Count -gt 0) {
    foreach ($msg in $script:Failures) { Write-Host "PS1_BOM_RED: $msg" }
    exit 1
}

# ── 变异测试：无 BOM + 含中文 的文件必须被判 RED（证明门禁不是摆设）──
$tmpOk = [System.IO.Path]::Combine($env:TEMP, 'ps1bom_mutation_ok.ps1')
$tmpBad = [System.IO.Path]::Combine($env:TEMP, 'ps1bom_mutation_bad.ps1')
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
$utf8Bom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($tmpOk, "# 注释 (mutation fixture)`nWrite-Host 'ok'`n", $utf8Bom)
[System.IO.File]::WriteAllText($tmpBad, "# 无 BOM 的中文注释`nWrite-Host 'bad'`n", $utf8NoBom)

$okList = New-Object System.Collections.Generic.List[string]
$badList = New-Object System.Collections.Generic.List[string]
$okPass = Test-Ps1Encoding $tmpOk $okList
$badPass = Test-Ps1Encoding $tmpBad $badList
Remove-Item -LiteralPath $tmpOk, $tmpBad -Force -ErrorAction SilentlyContinue

if (-not $okPass -or $okList.Count -ne 0) {
    Write-Host 'PS1_BOM_RED: mutation fixture with BOM was flagged (gate over-sensitive)'
    exit 1
}
if ($badPass -or $badList.Count -eq 0) {
    Write-Host 'PS1_BOM_RED: BOM-less non-ASCII fixture was NOT flagged (gate not sensitive)'
    exit 1
}

Write-Host ("PS1_BOM_GREEN: {0} scripts checked; BOM-less non-ASCII fixture correctly flagged" -f $files.Count)
exit 0
