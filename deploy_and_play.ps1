# EconomyMod 一键部署并启动游戏
# 用法：powershell -ExecutionPolicy Bypass -File "e:\code\new\EconomyMod\deploy_and_play.ps1"

$src = "e:\code\new\EconomyMod"
$dst = "D:\Program Files (x86)\Steam\steamapps\common\worldbox\Mods\ClassicalEconomics"
$gameExe = "D:\Program Files (x86)\Steam\steamapps\common\worldbox\worldbox.exe"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  ClassicalEconomics 部署并启动游戏" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. 部署文件
Write-Host "[1/3] 部署模组文件..." -ForegroundColor Yellow

if (-not (Test-Path $dst)) {
    New-Item -ItemType Directory -Path $dst -Force | Out-Null
}

# 清理旧名称（EconomyMod）遗留目录，避免重复加载
$oldDst = "D:\Program Files (x86)\Steam\steamapps\common\worldbox\Mods\EconomyMod"
if (Test-Path $oldDst) {
    Remove-Item $oldDst -Recurse -Force
    Write-Host "  已清理旧目录: $oldDst" -ForegroundColor Yellow
}

Copy-Item "$src\bin\EconomyMod.dll" $dst -Force
Copy-Item "$src\mod.json" $dst -Force
Copy-Item "$src\icon.png" $dst -Force
Copy-Item "$src\default_config.json" $dst -Force -ErrorAction SilentlyContinue

$localeDst = "$dst\Locales"
if (-not (Test-Path $localeDst)) {
    New-Item -ItemType Directory -Path $localeDst -Force | Out-Null
}
Copy-Item "$src\Locales\*.json" $localeDst -Force

Write-Host "  已部署文件:" -ForegroundColor Green
Get-ChildItem $dst | ForEach-Object {
    $size = if ($_.PSIsContainer) { "<DIR>" } else { "$([math]::Round($_.Length/1024, 1)) KB" }
    Write-Host ("    {0,-20} {1}" -f $_.Name, $size)
}
Write-Host ""

# 2. 检查游戏是否已运行
Write-Host "[2/3] 检查游戏进程..." -ForegroundColor Yellow

$gameProcess = Get-Process -Name "worldbox" -ErrorAction SilentlyContinue

if ($gameProcess) {
    Write-Host "  检测到游戏已在运行 (PID: $($gameProcess.Id))" -ForegroundColor Yellow
    $choice = Read-Host "  是否关闭并重启游戏？(y/N)"

    if ($choice -eq 'y' -or $choice -eq 'Y') {
        Write-Host "  正在关闭游戏..." -ForegroundColor Yellow
        Stop-Process -Name "worldbox" -Force
        Start-Sleep -Seconds 2
        Write-Host "  游戏已关闭" -ForegroundColor Green
    } else {
        Write-Host "  跳过启动，游戏保持运行" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "提示：需重启游戏才能加载新模组" -ForegroundColor Yellow
        Write-Host "完成。" -ForegroundColor Cyan
        exit 0
    }
} else {
    Write-Host "  游戏未运行" -ForegroundColor Green
}

# 3. 启动游戏
Write-Host "[3/3] 启动 WorldBox..." -ForegroundColor Yellow

if (Test-Path $gameExe) {
    Start-Process $gameExe
    Write-Host "  游戏已启动" -ForegroundColor Green
} else {
    Write-Host "  错误：找不到游戏可执行文件 $gameExe" -ForegroundColor Red
    Write-Host "  请手动启动 Steam 中的 WorldBox" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  部署完成，游戏已启动" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
