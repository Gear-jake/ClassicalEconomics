param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

# 【W3】ScrollRect 遮罩门禁
#
# 背景（真实 bug）：CreateScrollContent 的 Image 同时充当 Mask 模板，
# Unity 的 Mask 靠该 Image 的 alpha 写入 stencil。曾把它设成 alpha=0 以"去掉暗色蒙版"，
# 结果 stencil 为空、**所有子物体被裁掉**，窗口一片空白（用户报告"事件文字消失了"）。
#
# 不变量：CreateScrollContent 里给遮罩 Image 的 alpha 必须 **严格大于 0**。

$ErrorActionPreference = 'Stop'

$failures = New-Object System.Collections.Generic.List[string]
function Assert-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) { $failures.Add($Message) }
}

$utf8 = New-Object System.Text.UTF8Encoding($false)
$uhPath = Join-Path $Root 'UI\UIHelpers.cs'
if (-not (Test-Path -LiteralPath $uhPath -PathType Leaf)) {
    Write-Host "SCROLL_MASK_RED: required file not found: $uhPath"
    exit 1
}
$uh = [System.IO.File]::ReadAllText($uhPath, $utf8)

$body = [regex]::Match($uh, 'public static RectTransform CreateScrollContent\([\s\S]*?\n        \}').Value
Assert-True ($body.Length -gt 0) 'CreateScrollContent 未找到'

# 遮罩 Image 的颜色赋值必须存在
$colorLine = [regex]::Match($body, 'scrollObj\.GetComponent<Image>\(\)\.color\s*=\s*([^;]+);').Groups[1].Value
Assert-True ($colorLine.Length -gt 0) 'CreateScrollContent 未设置遮罩 Image 的颜色'
Assert-True ($colorLine -notmatch 'new Color\(0,\s*0,\s*0,\s*0f\)') '遮罩 Image 的 alpha 不得为 0（会导致 Mask 裁掉全部子物体）'
Assert-True ($colorLine -match 'transparentBg') '遮罩颜色应按 transparentBg 分支'

# Mask 组件必须仍在（用 RectMask2D 则不需要该 Image，此处保持 Mask 方案）
Assert-True ($body -match 'typeof\(Mask\)') 'CreateScrollContent 应保留 Mask 组件'

if ($failures.Count -gt 0) {
    foreach ($f in $failures) { Write-Host "SCROLL_MASK_RED: $f" }
    Write-Host "SCROLL_MASK_RED: $($failures.Count) check(s) failed"
    exit 1
}

Write-Host 'SCROLL_MASK_GREEN: 滚动内容遮罩 alpha > 0（Mask stencil 有效，内容不会被裁光）'
exit 0
