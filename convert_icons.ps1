# convert_icons.ps1 - Convert Jimeng AI generated icons to WorldBox style 128x128 pixel PNG
# USAGE:
#   1. Generate icons in Jimeng AI, download PNG/JPG
#   2. Save into  e:\code\new\EconomyMod\ai_icons\  with EXACT target name
#   3. Run:  powershell -ExecutionPolicy Bypass -File convert_icons.ps1
#   4. Run deploy.ps1 to deploy
#
# Background removal: corner-average color + color distance + hole restore
# (holes fully surrounded by foreground are restored, so ring-shaped icons keep their inner area)
# Watermark removal: bottom band pixels not connected to the main body get deleted
Add-Type -AssemblyName System.Drawing

$rootDir  = "e:\code\new\EconomyMod"
$srcDir   = Join-Path $rootDir "ai_icons"
$uiDir    = Join-Path $rootDir "Icons"
$eraDir   = Join-Path $rootDir "GameResources\ui\Icons"

# target name -> output dir
$targets = @{
    "icon"                 = $rootDir
    "iconEraGolden"        = $eraDir
    "iconEraRevival"       = $eraDir
    "iconEraFlourish"      = $eraDir
    "iconEraCollapse"      = $eraDir
    "coin"                 = $uiDir
    "ledger"               = $uiDir
    "flame"                = $uiDir
    "suppress"             = $uiDir
    "collect"              = $uiDir
    "trash"                = $uiDir
    "crown"                = $uiDir
    "bell"                 = $uiDir
    "phase_boom"           = $uiDir
    "phase_recession"      = $uiDir
    "phase_depression"     = $uiDir
    "phase_recovery"       = $uiDir
}

$SIZE      = 128          # output size
$BG_TOL    = 90           # background color tolerance (0-255)
$QUANTIZE  = $true        # quantize colors to 4x4x4 palette
$WM_RATIO  = 0.08         # bottom band ratio where watermark lives

function Color-Dist($c1, $c2) {
    $dr = [Math]::Abs($c1.R - $c2.R)
    $dg = [Math]::Abs($c1.G - $c2.G)
    $db = [Math]::Abs($c1.B - $c2.B)
    return ($dr + $dg + $db)
}

# Remove background: mark pixels close to corner-average color, keep only the
# region connected to image border; restore any enclosed holes.
function Remove-Background($bmp, $tol) {
    $w = $bmp.Width
    $h = $bmp.Height
    $wm = $w - 1
    $hm = $h - 1

    # 1) corner average as background color
    $c00 = $bmp.GetPixel(0, 0)
    $c10 = $bmp.GetPixel($wm, 0)
    $c01 = $bmp.GetPixel(0, $hm)
    $c11 = $bmp.GetPixel($wm, $hm)
    $bg = [System.Drawing.Color]::FromArgb(255,
        [int](($c00.R + $c10.R + $c01.R + $c11.R) / 4),
        [int](($c00.G + $c10.G + $c01.G + $c11.G) / 4),
        [int](($c00.B + $c10.B + $c01.B + $c11.B) / 4))

    # 2) candidate deletion mask
    $del = New-Object 'bool[,]' $w, $h
    for ($y = 0; $y -lt $h; $y++) {
        for ($x = 0; $x -lt $w; $x++) {
            $c = $bmp.GetPixel($x, $y)
            if ((Color-Dist $c $bg) -le $tol) { $del[$x, $y] = $true }
        }
    }

    # 3) flood fill from border: only border-connected candidates get deleted
    $real = New-Object 'bool[,]' $w, $h
    $vis  = New-Object 'bool[,]' $w, $h
    $q = New-Object System.Collections.Queue
    for ($x = 0; $x -lt $w; $x++) {
        if ($del[$x, 0] -and -not $vis[$x, 0]) { $vis[$x, 0] = $true; $q.Enqueue(@($x, 0)) }
        if ($del[$x, $hm] -and -not $vis[$x, $hm]) { $vis[$x, $hm] = $true; $q.Enqueue(@($x, $hm)) }
    }
    for ($y = 0; $y -lt $h; $y++) {
        if ($del[0, $y] -and -not $vis[0, $y]) { $vis[0, $y] = $true; $q.Enqueue(@(0, $y)) }
        if ($del[$wm, $y] -and -not $vis[$wm, $y]) { $vis[$wm, $y] = $true; $q.Enqueue(@($wm, $y)) }
    }
    $dx = @(1, -1, 0, 0)
    $dy = @(0, 0, 1, -1)
    while ($q.Count -gt 0) {
        $p = $q.Dequeue()
        $px = $p[0]; $py = $p[1]
        $real[$px, $py] = $true
        for ($i = 0; $i -lt 4; $i++) {
            $nx = $px + $dx[$i]
            $ny = $py + $dy[$i]
            if ($nx -ge 0 -and $nx -lt $w -and $ny -ge 0 -and $ny -lt $h) {
                if ($del[$nx, $ny] -and -not $vis[$nx, $ny]) {
                    $vis[$nx, $ny] = $true
                    $q.Enqueue(@($nx, $ny))
                }
            }
        }
    }

    # 4) apply
    for ($y = 0; $y -lt $h; $y++) {
        for ($x = 0; $x -lt $w; $x++) {
            if ($real[$x, $y]) { $bmp.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(0, 0, 0, 0)) }
        }
    }
}

# Remove watermark: in the bottom band, delete foreground pixels that are NOT
# connected (4-neighbour) to the main body above the band. Watermark text is
# detached from the body, so it gets removed while body parts survive.
function Remove-Watermark($bmp, $bottomRatio) {
    $w = $bmp.Width
    $h = $bmp.Height
    $cut = $h - [int]($h * $bottomRatio)
    if ($cut -ge $h) { return }

    $keep = New-Object 'bool[,]' $w, $h
    $vis  = New-Object 'bool[,]' $w, $h
    $q = New-Object System.Collections.Queue
    # seed: every non-transparent pixel above the band
    for ($y = 0; $y -lt $cut; $y++) {
        for ($x = 0; $x -lt $w; $x++) {
            if ($bmp.GetPixel($x, $y).A -gt 0 -and -not $vis[$x, $y]) {
                $vis[$x, $y] = $true
                $q.Enqueue(@($x, $y))
            }
        }
    }
    $dx = @(1, -1, 0, 0)
    $dy = @(0, 0, 1, -1)
    while ($q.Count -gt 0) {
        $p = $q.Dequeue()
        $px = $p[0]; $py = $p[1]
        $keep[$px, $py] = $true
        for ($i = 0; $i -lt 4; $i++) {
            $nx = $px + $dx[$i]
            $ny = $py + $dy[$i]
            if ($nx -ge 0 -and $nx -lt $w -and $ny -ge 0 -and $ny -lt $h) {
                if (-not $vis[$nx, $ny] -and $bmp.GetPixel($nx, $ny).A -gt 0) {
                    $vis[$nx, $ny] = $true
                    $q.Enqueue(@($nx, $ny))
                }
            }
        }
    }
    # delete band foreground pixels not connected to the body
    for ($y = $cut; $y -lt $h; $y++) {
        for ($x = 0; $x -lt $w; $x++) {
            if ($bmp.GetPixel($x, $y).A -gt 0 -and -not $keep[$x, $y]) {
                $bmp.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(0, 0, 0, 0))
            }
        }
    }
}

function Quantize-Color([System.Drawing.Color]$c) {
    $r = [int]([Math]::Round($c.R / 85.0) * 85); if ($r -gt 255) { $r = 255 }
    $g = [int]([Math]::Round($c.G / 85.0) * 85); if ($g -gt 255) { $g = 255 }
    $b = [int]([Math]::Round($c.B / 85.0) * 85); if ($b -gt 255) { $b = 255 }
    return [System.Drawing.Color]::FromArgb($c.A, $r, $g, $b)
}

function Convert-Icon($name, $outDir) {
    $srcPath = Join-Path $srcDir "$name.png"
    if (!(Test-Path $srcPath)) { $srcPath = Join-Path $srcDir "$name.jpg" }
    if (!(Test-Path $srcPath)) {
        Write-Host "  [SKIP] $name : no source file (png/jpg) in ai_icons" -ForegroundColor DarkYellow
        return
    }
    $src = [System.Drawing.Image]::FromFile($srcPath)

    # 1) center crop to square
    $side = [Math]::Min($src.Width, $src.Height)
    $ox = [int](($src.Width  - $side) / 2)
    $oy = [int](($src.Height - $side) / 2)
    $crop = New-Object System.Drawing.Bitmap($side, $side)
    $cg = [System.Drawing.Graphics]::FromImage($crop)
    $cg.DrawImage($src, (New-Object System.Drawing.Rectangle(0, 0, $side, $side)),
                  (New-Object System.Drawing.Rectangle($ox, $oy, $side, $side)),
                  [System.Drawing.GraphicsUnit]::Pixel)
    $cg.Dispose()

    # 2) high quality resize to 128
    $bmp = New-Object System.Drawing.Bitmap($SIZE, $SIZE)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.DrawImage($crop, 0, 0, $SIZE, $SIZE)
    $g.Dispose()

    # 3) background removal (border-connected only, holes restored)
    Remove-Background $bmp $BG_TOL

    # 3.5) watermark removal (bottom band, detached foreground)
    Remove-Watermark $bmp $WM_RATIO

    # 4) quantize
    if ($QUANTIZE) {
        for ($y = 0; $y -lt $SIZE; $y++) {
            for ($x = 0; $x -lt $SIZE; $x++) {
                $c = $bmp.GetPixel($x, $y)
                if ($c.A -gt 0) { $bmp.SetPixel($x, $y, (Quantize-Color $c)) }
            }
        }
    }

    if (!(Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir -Force | Out-Null }
    $outPath = Join-Path $outDir "$name.png"
    $bmp.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose(); $crop.Dispose(); $src.Dispose()
    Write-Host "  [OK] $name.png" -ForegroundColor Green
}

Write-Host "Converting icons from $srcDir" -ForegroundColor Cyan
Write-Host ""
if (!(Test-Path $srcDir)) { New-Item -ItemType Directory -Path $srcDir -Force | Out-Null }
foreach ($k in $targets.Keys) { Convert-Icon $k $targets[$k] }
Write-Host ""
Write-Host "Done. Run deploy.ps1 to deploy to the game." -ForegroundColor Green
