$src = "e:\code\new\EconomyMod"
$dst = "D:\Program Files (x86)\Steam\steamapps\common\worldbox\Mods\ClassicalEconomics"

Write-Host "Deploying ClassicalEconomics..." -ForegroundColor Cyan

# 清理旧名称（EconomyMod）遗留目录，避免 NML 重复加载旧版
$oldDst = "D:\Program Files (x86)\Steam\steamapps\common\worldbox\Mods\EconomyMod"
if (Test-Path $oldDst) {
    Remove-Item $oldDst -Recurse -Force
    Write-Host "Removed old deploy folder: $oldDst" -ForegroundColor Yellow
}
$oldSA = "D:\Program Files (x86)\Steam\steamapps\common\worldbox\worldbox_Data\StreamingAssets\mods\EconomyMod"
if (Test-Path $oldSA) {
    Remove-Item $oldSA -Recurse -Force
    Write-Host "Removed old StreamingAssets copy: $oldSA" -ForegroundColor Yellow
}

# 确保目标目录存在（若上次误写为文件则删除重建，.NET API 规避脚本权限限制）
if ([System.IO.File]::Exists($dst)) { [System.IO.File]::Delete($dst) }
if (-not (Test-Path $dst)) {
    New-Item -ItemType Directory -Path $dst -Force | Out-Null
}

# 始终部署 bin\ 下最新编译产物（根目录 EconomyMod.dll 可能是旧版）
Copy-Item "$src\bin\EconomyMod.dll" $dst -Force
Copy-Item "$src\mod.json" $dst -Force
Copy-Item "$src\icon.png" $dst -Force
Copy-Item "$src\default_config.json" $dst -Force -ErrorAction SilentlyContinue

$localeDst = "$dst\Locales"
if (-not (Test-Path $localeDst)) { New-Item -ItemType Directory -Path $localeDst -Force }
Copy-Item "$src\Locales\*.json" $localeDst -Force

$iconsDst = "$dst\Icons"
if (-not (Test-Path $iconsDst)) { New-Item -ItemType Directory -Path $iconsDst -Force }
Copy-Item "$src\Icons\*.png" $iconsDst -Force

# 特质/资源用 GameResources（NML 挂载为游戏 Resources，供 SpriteTextureLoader 按 ui/... 路径加载）
# 先删除旧目标再整目录复制，避免 Copy-Item 目标已存在时产生嵌套目录
$gameResDst = "$dst\GameResources"
if (Test-Path $gameResDst) { Remove-Item $gameResDst -Recurse -Force }
if (Test-Path "$src\GameResources") { Copy-Item "$src\GameResources" $gameResDst -Recurse -Force }

Write-Host "Deploy complete!" -ForegroundColor Green
Write-Host "Files in destination:"
Get-ChildItem $dst | Select-Object Name, Length | Format-Table
