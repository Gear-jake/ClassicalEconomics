# Deploy EconomyMod (fixed DLL) to game Mods folder
# Usage: powershell -ExecutionPolicy Bypass -File "e:\code\new\deploy_now.ps1"

$src = "e:\code\new\_ce_src\ClassicalEconomics-main"
$dst = "D:\Program Files (x86)\Steam\steamapps\common\worldbox\Mods\EconomyMod"

if (-not (Test-Path $dst)) { New-Item -ItemType Directory -Path $dst -Force | Out-Null }

# 清理历史递归复制产生的嵌套冗余目录（Copy-Item -Recurse 到已存在目标时会生成 子\子\...）
Remove-Item "$dst\GameResources\GameResources" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$dst\Icons\Icons" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$dst\Locales\Locales" -Recurse -Force -ErrorAction SilentlyContinue

Copy-Item "$src\bin\EconomyMod.dll" $dst -Force
Copy-Item "$src\mod.json" $dst -Force
Copy-Item "$src\icon.png" $dst -Force
Copy-Item "$src\default_config.json" $dst -Force
# 用通配符复制内容到已存在目录，避免嵌套
if (-not (Test-Path "$dst\Locales")) { New-Item -ItemType Directory -Path "$dst\Locales" -Force | Out-Null }
Copy-Item "$src\Locales\*" "$dst\Locales" -Recurse -Force
if (-not (Test-Path "$dst\GameResources")) { New-Item -ItemType Directory -Path "$dst\GameResources" -Force | Out-Null }
Copy-Item "$src\GameResources\*" "$dst\GameResources" -Recurse -Force
if (-not (Test-Path "$dst\Icons")) { New-Item -ItemType Directory -Path "$dst\Icons" -Force | Out-Null }
Copy-Item "$src\Icons\*" "$dst\Icons" -Recurse -Force

Write-Host "Deploy done. Files:"
Get-ChildItem $dst -Recurse -File | ForEach-Object { Write-Host ("  " + $_.FullName.Replace($dst + '\', '')) }
