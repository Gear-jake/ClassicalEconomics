param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

# W3 补丁清单一致性门禁：PatchInstaller.Registry 与 docs/patches.md 必须一一对应。
# 防止"加了补丁但没写文档"或"删了补丁但文档还在"。

$ErrorActionPreference = 'Stop'

$failures = New-Object System.Collections.Generic.List[string]
function Assert-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) { $failures.Add($Message) }
}

$utf8 = New-Object System.Text.UTF8Encoding($false)
$piPath = Join-Path $Root 'Core\PatchInstaller.cs'
$docPath = Join-Path $Root 'docs\patches.md'

foreach ($p in @($piPath, $docPath)) {
    if (-not (Test-Path -LiteralPath $p -PathType Leaf)) {
        Write-Host "PATCHES_MANIFEST_RED: required file not found: $p"
        exit 1
    }
}

$piText  = [System.IO.File]::ReadAllText($piPath, $utf8)
$docText = [System.IO.File]::ReadAllText($docPath, $utf8)

# 抽出 Registry 里的补丁标识（每行形如 ("Id", "用途", () => ...)）
$registryBlock = [regex]::Match($piText, 'Registry\s*=\s*\{[\s\S]*?\};').Value
Assert-True ($registryBlock.Length -gt 0) 'PatchInstaller.cs 中未找到 Registry 块'

$ids = @()
foreach ($m in [regex]::Matches($registryBlock, '\(\s*"([A-Za-z0-9_]+)"\s*,')) { $ids += $m.Groups[1].Value }
Assert-True ($ids.Count -ge 1) 'PatchInstaller.Registry 未解析出任何补丁'

$dup = @($ids | Group-Object | Where-Object { $_.Count -gt 1 })
foreach ($d in $dup) { $failures.Add("PatchInstaller.Registry 存在重复标识: $($d.Name)") }

# 每个补丁标识都必须在文档中出现
foreach ($id in $ids) {
    if (-not $docText.Contains($id)) {
        $failures.Add("docs/patches.md 缺少补丁说明: $id")
    }
}

# 反向：文档里出现的 HarmonyId 都必须能在代码里找到
foreach ($m in [regex]::Matches($docText, 'HarmonyId\s*\|\s*\x60([^\x60]+)\x60')) {
    $hid = $m.Groups[1].Value.Trim()
    if ($hid -eq '') { continue }
    $sources = @(Get-ChildItem -LiteralPath (Join-Path $Root 'Core') -Filter '*.cs' -File) +
               @(Get-ChildItem -LiteralPath (Join-Path $Root 'Services') -Filter '*.cs' -File)
    $found = $false
    foreach ($f in $sources) {
        if ([System.IO.File]::ReadAllText($f.FullName, $utf8).Contains($hid)) { $found = $true; break }
    }
    if (-not $found) { $failures.Add("docs/patches.md 记载的 HarmonyId 在源码中不存在: $hid") }
}

if ($failures.Count -gt 0) {
    foreach ($f in $failures) { Write-Host "PATCHES_MANIFEST_RED: $f" }
    Write-Host "PATCHES_MANIFEST_RED: $($failures.Count) check(s) failed"
    exit 1
}

Write-Host "PATCHES_MANIFEST_GREEN: $($ids.Count) 个补丁在 PatchInstaller.Registry 与 docs/patches.md 两处一致"
exit 0
