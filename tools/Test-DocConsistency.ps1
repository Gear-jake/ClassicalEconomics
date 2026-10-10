param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

# W3 文档一致性门禁：防止"事件数"等口径在 mod.json / README / docs 之间再次漂移。
# 事实来源是 events.json —— 文档必须与它一致。
# 历史 changelog（docs\CHANGELOG_*.md）记录的是当时的数字，**不参与**校验。

$ErrorActionPreference = 'Stop'

$failures = New-Object System.Collections.Generic.List[string]

function Assert-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) { $failures.Add($Message) }
}

$utf8 = New-Object System.Text.UTF8Encoding($false)

$evPath = Join-Path $Root 'events.json'
if (-not (Test-Path -LiteralPath $evPath -PathType Leaf)) {
    Write-Host 'DOC_CONSISTENCY_RED: events.json not found'
    exit 1
}
$ev = Get-Content -LiteralPath $evPath -Raw -Encoding UTF8 | ConvertFrom-Json
$count = @($ev.events).Count
Assert-True ($count -gt 0) 'events.json 事件数为 0（解析失败或文件损坏）'

# 必须声明当前事件数的文件（新增文档时在此追加）
$targets = @(
    @{ File = 'mod.json';          Label = 'mod.json 描述' },
    @{ File = 'README.md';         Label = 'README.md' },
    @{ File = 'README_en.md';      Label = 'README_en.md' },
    @{ File = 'docs\Q群介绍.md';  Label = 'docs/Q群介绍.md' },
    @{ File = 'docs\架构说明.md'; Label = 'docs/架构说明.md' }
)

foreach ($t in $targets) {
    $p = Join-Path $Root $t.File
    if (-not (Test-Path -LiteralPath $p -PathType Leaf)) {
        $failures.Add("missing file: $($t.File)")
        continue
    }
    $text = [System.IO.File]::ReadAllText($p, $utf8)
    if ($text -notmatch [regex]::Escape("$count")) {
        $failures.Add("$($t.Label) 未包含当前事件数 $count（文档漂移；事实来源 events.json）")
    }
}

# 反向校验：当前状态文档里不应再出现已废弃的口径
foreach ($t in $targets) {
    $p = Join-Path $Root $t.File
    if (-not (Test-Path -LiteralPath $p -PathType Leaf)) { continue }
    $text = [System.IO.File]::ReadAllText($p, $utf8)
    if ($count -ne 330 -and $text -match '\*\*330 个事件\*\*|\*\*330 events\*\*') {
        $failures.Add("$($t.Label) 仍标注已废弃的 330 事件口径")
    }
}

if ($failures.Count -gt 0) {
    foreach ($f in $failures) { Write-Host "DOC_CONSISTENCY_RED: $f" }
    Write-Host "DOC_CONSISTENCY_RED: $($failures.Count) check(s) failed"
    exit 1
}

Write-Host "DOC_CONSISTENCY_GREEN: 事件数 $count 在 mod.json / README / docs 四处口径一致"
exit 0
