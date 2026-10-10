param(
    [switch]$WhatIfOnly
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

# ─────────────────────────────────────────────────────────────────────────────
# 幂等修复脚本：给「含非 ASCII 但没有 UTF-8 BOM」的 .ps1 补上 BOM。
#
# 为什么需要（2026-10-10 真实事故）：
#   · 用编辑器/工具改完 .ps1 后 BOM 会丢；PowerShell 5.1 会把无 BOM 含中文的脚本按 ANSI 读，
#     中文注释末字节可能吞掉换行 → **紧跟的断言被并进注释，静默失效**（门禁照样输出 GREEN）。
#   · 纯 ASCII 脚本无 BOM 无害，本脚本不动它们。
#
# 用法：powershell -File tools\Fix-Ps1Bom.ps1 [-WhatIfOnly]
# 门禁：tools\Test-Ps1Bom.ps1 会拦住遗漏（Infra 层，fail-closed）。
# ─────────────────────────────────────────────────────────────────────────────

$exclude = '\\(release|bin|obj|evidence|\.git)\\'
$utf8Bom = New-Object System.Text.UTF8Encoding($true)
$utf8Strict = New-Object System.Text.UTF8Encoding($false, $true)

$fixed = New-Object System.Collections.Generic.List[string]
$skipped = New-Object System.Collections.Generic.List[string]

foreach ($f in (Get-ChildItem -LiteralPath $root -Recurse -File -Filter '*.ps1' |
        Where-Object { $_.FullName -notmatch $exclude })) {
    $bytes = [System.IO.File]::ReadAllBytes($f.FullName)
    $hasBom = ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)
    if ($hasBom) { continue }

    $nonAscii = $false
    foreach ($b in $bytes) { if ($b -ge 0x80) { $nonAscii = $true; break } }
    if (-not $nonAscii) { continue }

    $rel = $f.FullName.Replace($root + '\', '')
    try {
        $text = $utf8Strict.GetString($bytes)   # 严格 UTF-8 解码：解不开就说明本来就不是 UTF-8，不能乱改
    } catch {
        $skipped.Add("$rel (not valid UTF-8: $($_.Exception.Message))")
        continue
    }

    if (-not $WhatIfOnly) { [System.IO.File]::WriteAllText($f.FullName, $text, $utf8Bom) }
    $fixed.Add($rel)
}

foreach ($s in $skipped) { Write-Host "PS1_BOM_SKIP: $s" }
if ($fixed.Count -eq 0) {
    Write-Host 'PS1_BOM_FIX_OK: nothing to fix (all scripts are BOM-prefixed or pure ASCII)'
    exit 0
}
foreach ($f in $fixed) { Write-Host ("PS1_BOM_FIXED: {0}{1}" -f $f, $(if ($WhatIfOnly) { ' (dry-run)' } else { '' })) }
Write-Host ("PS1_BOM_FIX_OK: {0} file(s) {1}" -f $fixed.Count, $(if ($WhatIfOnly) { 'need a BOM' } else { 'fixed' }))
exit 0
