# generate_icons.ps1 - WorldBox style pixel art icons (16x16 grid -> 128/64 PNG)
Add-Type -AssemblyName System.Drawing

# ===== Palette =====
$PAL = @{}
$PAL[' '] = [System.Drawing.Color]::FromArgb(0,0,0,0)
$PAL['K'] = [System.Drawing.Color]::FromArgb(255,25,20,20)
$PAL['G'] = [System.Drawing.Color]::FromArgb(255,255,200,50)
$PAL['D'] = [System.Drawing.Color]::FromArgb(255,180,130,30)
$PAL['W'] = [System.Drawing.Color]::FromArgb(255,245,240,220)
$PAL['R'] = [System.Drawing.Color]::FromArgb(255,220,60,50)
$PAL['r'] = [System.Drawing.Color]::FromArgb(255,140,30,25)
$PAL['B'] = [System.Drawing.Color]::FromArgb(255,60,120,200)
$PAL['b'] = [System.Drawing.Color]::FromArgb(255,30,60,120)
$PAL['g'] = [System.Drawing.Color]::FromArgb(255,70,180,70)
$PAL['d'] = [System.Drawing.Color]::FromArgb(255,35,100,35)
$PAL['O'] = [System.Drawing.Color]::FromArgb(255,240,150,40)
$PAL['o'] = [System.Drawing.Color]::FromArgb(255,180,100,20)
$PAL['Y'] = [System.Drawing.Color]::FromArgb(255,250,230,100)
$PAL['N'] = [System.Drawing.Color]::FromArgb(255,120,80,50)
$PAL['n'] = [System.Drawing.Color]::FromArgb(255,70,45,25)
$PAL['S'] = [System.Drawing.Color]::FromArgb(255,180,180,200)
$PAL['s'] = [System.Drawing.Color]::FromArgb(255,100,100,120)
$PAL['C'] = [System.Drawing.Color]::FromArgb(255,180,130,70)
$PAL['P'] = [System.Drawing.Color]::FromArgb(255,150,80,180)

# ===== Render function =====
function Render-Icon($name, $grid, $outDir, $size=128) {
    $gs = $grid.Count
    $scale = [int]($size / $gs)
    $bmp = New-Object System.Drawing.Bitmap($size, $size)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::None
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $g.Clear([System.Drawing.Color]::Transparent)

    for ($y = 0; $y -lt $gs; $y++) {
        $line = $grid[$y]
        for ($x = 0; $x -lt $gs; $x++) {
            $ch = $line[$x]
            if ($ch -ne ' ' -and $PAL.ContainsKey($ch)) {
                $g.FillRectangle($PAL[$ch], $x*$scale, $y*$scale, $scale, $scale)
            }
        }
    }
    $g.Dispose()
    if (!(Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir -Force | Out-Null }
    $path = Join-Path $outDir "$name.png"
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Host "  [OK] $name.png"
}

# ===== Icon data (16x16 grid, each line exactly 16 chars) =====

# --- Main icon: scale + coins ---
$icon_main = @(
    "                ",
    "       K        ",
    "      KDK       ",
    "     KDWDK      ",
    "      KDK       ",
    "    KK   KK     ",
    "   KGGGK KGGGK  ",
    "  KGGGGK KGGGGK ",
    "   KGGGK KGGGK  ",
    "    KKK   KKK   ",
    "  KK  KKK  KK   ",
    "  KK       KK   ",
    "  KKKKKKKKKKKK  ",
    "  KNNNNNNNNNNK  ",
    "   KNNNNNNNNK   ",
    "    KKKKKKKK    "
)

# --- Era traits ---

# Golden: sun
$iconEraGolden = @(
    "                ",
    "        G       ",
    "     G  Y  G    ",
    "                ",
    "   G  KYYYK  G  ",
    "    KYYYYYYYK   ",
    "    KYWYYYYYK   ",
    "  G KYYYYYYYK G ",
    "  G KYYYYYYYK G ",
    "    KYWYYYYYK   ",
    "    KYYYYYYYK   ",
    "   G  KYYYK  G  ",
    "                ",
    "     G  Y  G    ",
    "        G       ",
    "                "
)

# Revival: sprout on ruins
$iconEraRevival = @(
    "                ",
    "                ",
    "       gg       ",
    "      gGGg      ",
    "     gGGGGg     ",
    "      gGGg      ",
    "       g        ",
    "      nKn       ",
    "    nNNNNNn     ",
    "   nNnnnnNn     ",
    "  KNNNnnnNNNK   ",
    "  KNn    nNNK   ",
    "  KNNKK KKNNK   ",
    "   KK       K   ",
    "                ",
    "                "
)

# Flourish: banner
$iconEraFlourish = @(
    "                ",
    "  KKKK          ",
    "  KbbK          ",
    "  KbbK  KKKKKK  ",
    "  KbbK  KBBBBK  ",
    "  KbbK  KBBBbK  ",
    "  KbbK  KBBBBK  ",
    "  KbbK  KBBBBK  ",
    "  KbbK  KBBBBK  ",
    "  KbbK  KKKKKK  ",
    "  KbbK          ",
    "  KbbK          ",
    "  KbbK          ",
    "  KNNK          ",
    "  KNNK          ",
    "  KKKK          "
)

# Collapse: cracked coin
$iconEraCollapse = @(
    "                ",
    "     KKKKKK     ",
    "   KKGGGGGGKK   ",
    "  KGGK GGGG GGK ",
    " KGG KGGGGGGG K ",
    " KGGGG KGGGGGGK ",
    " KGGGGG KGGGGGK ",
    " KGGGGGg KGGGGK ",
    " KGGGGG gKGGGGK ",
    " KGGGGG  KGGGGK ",
    " KGGGG KGGGGGGK ",
    "  KGG KGGGGGG K ",
    "   K  KGGGG  K  ",
    "     KK    KK   ",
    "       KK       ",
    "                "
)

# --- UI button icons ---

# coin
$coin = @(
    "                ",
    "                ",
    "     KKKKKK     ",
    "   KKGGGGGGKK   ",
    "  KGGGGGGGGGGK  ",
    " KGGGWWGGGGGGGK ",
    " KGGGWWGGGGGGGK ",
    " KGGGGGGGGGGGGK ",
    " KGGGGGGGGGGGGK ",
    " KGGGGGGGGGGGGK ",
    " KGGGGGGGGGGGGK ",
    "  KGGGGGGGGGGK  ",
    "  KKGGGGGGGGKK  ",
    "   KKDDDDDDKK   ",
    "     KKKKKK     ",
    "                "
)

# ledger
$ledger = @(
    "                ",
    "                ",
    "   KKKKKKKKKK   ",
    "  KWWWWWWWWWWK  ",
    "  KWWKKWWKKWWK  ",
    " KWWKKWWKKWWWWK ",
    " KWWWWWWWWWWWWK ",
    " KNNNNNNNNNNNNK ",
    " KNNnNNNNnNNNNK ",
    " KNNNNNNNNNNNNK ",
    " KNNNNNNNNNNNNK ",
    " KNNNNNNNNNNNNK ",
    "  KNNNNNNNNNNK  ",
    "   KKKKKKKKKK   ",
    "                ",
    "                "
)

# flame
$flame = @(
    "                ",
    "                ",
    "      KKKK      ",
    "     KOOOOK     ",
    "    KOOOOOOK    ",
    "    KORROOOK    ",
    "   KORRRROOOK   ",
    "   KORRrrROOK   ",
    "   KORrrrrROK   ",
    "    KRrrrrRK    ",
    "    KKrrrrKK    ",
    "     KKnnKK     ",
    "      KnnK      ",
    "       KK       ",
    "                ",
    "                "
)

# suppress: mallet
$suppress = @(
    "                ",
    "       KK       ",
    "      KNNK      ",
    "     KNNNNK     ",
    "     KNnNnK     ",
    "     KNNNNK     ",
    "      KNNK      ",
    "       KK       ",
    "       NK       ",
    "       NK       ",
    "       NK       ",
    "       NK       ",
    "       NK       ",
    "       KK       ",
    "                ",
    "                "
)

# collect: treasure chest
$collect = @(
    "                ",
    "                ",
    "   KKKKKKKKKK   ",
    "  KGGGGGGGGGGK  ",
    " KGGGGGGGGGGGGK ",
    " KKKKKKKKKKKKKK ",
    " KNNNNNNNNNNNNK ",
    " KNNNNNGNNNNNNK ",
    " KNNNNNGGNNNNNK ",
    " KNNNNNGNNNNNNK ",
    " KNNNNNNNNNNNNK ",
    " KNNNNNNNNNNNNK ",
    " KNNNNNNNNNNNNK ",
    " KKKKKKKKKKKKKK ",
    "                ",
    "                "
)

# trash
$trash = @(
    "                ",
    "                ",
    "    KKKKKKKK    ",
    "   KKNNNNNNKK   ",
    "   KNNNNNNNNK   ",
    "  KKNNNNNNNNKK  ",
    "  KNNNNnNNNNNK  ",
    "  KNNNnNnNNNNK  ",
    "  KNNNNnNNNNNK  ",
    "  KNNNNNNNNNNK  ",
    "  KNNNNNNNNNNK  ",
    "  KKNNNNNNNNKK  ",
    "   KKNNNNNNKK   ",
    "    KKKKKKKK    ",
    "                ",
    "                "
)

# crown
$crown = @(
    "                ",
    "                ",
    "  K    K    K   ",
    " KgGK KgGK KgGK ",
    " KGGGKKGGGKGGGK ",
    " KGGGGGGGGGGGGK ",
    " KGRGGGGGGGRGGK ",
    " KGGGGGGGGGGGGK ",
    " KGGGGGGGGGGGGK ",
    " KNNNNNNNNNNNNK ",
    " KNNNNNNNNNNNNK ",
    " KNNNNNNNNNNNNK ",
    " KKKKKKKKKKKKKK ",
    "                ",
    "                ",
    "                "
)

# bell
$bell = @(
    "                ",
    "                ",
    "       KK       ",
    "      KNNK      ",
    "     KNNNNK     ",
    "    KNNNNNNK    ",
    "   KNNNNNNNNK   ",
    "   KNNNNNNNNK   ",
    "  KKNNNNNNNNKK  ",
    "  KNNNNNNNNNNK  ",
    "  KNNNNNNNNNNK  ",
    "  KKKKKKKKKKKK  ",
    "       KK       ",
    "       KK       ",
    "                ",
    "                "
)

# phase_boom: green arrow up
$phase_boom = @(
    "                ",
    "                ",
    "        g       ",
    "       gGg      ",
    "      gGGGg     ",
    "     gGGGGGg    ",
    "    gGGGGGGGg   ",
    "        g       ",
    "        g       ",
    "        g       ",
    "        g       ",
    "        g       ",
    "       gGg      ",
    "      gGGGg     ",
    "     gGGGGGg    ",
    "                "
)

# phase_recession: orange arrow down
$phase_recession = @(
    "                ",
    "                ",
    "     oOOOOOo    ",
    "      oOOOo     ",
    "       oOo      ",
    "        o       ",
    "        o       ",
    "        O       ",
    "        O       ",
    "        O       ",
    "        O       ",
    "        O       ",
    "    oOOOOOOOo   ",
    "     oOOOOOo    ",
    "      oOOOo     ",
    "                "
)

# phase_depression: cracked hollow circle
$phase_depression = @(
    "                ",
    "                ",
    "     KKKKKK     ",
    "   KK      KK   ",
    "  K   KK    K   ",
    " K  K    KK  K  ",
    " K      KK   K  ",
    " K  KK       K  ",
    " K      KK   K  ",
    " K   KK      K  ",
    " K  K    KK  K  ",
    "  K   KK    K   ",
    "   KK      KK   ",
    "     KKKKKK     ",
    "                ",
    "                "
)

# phase_recovery: sprout
$phase_recovery = @(
    "                ",
    "                ",
    "                ",
    "       gg       ",
    "      gGGg      ",
    "     gGGGGg     ",
    "      gGGg      ",
    "       gGg      ",
    "       gGg      ",
    "        g       ",
    "       nKn      ",
    "      nNNNn     ",
    "     nNNNNNn    ",
    "    nNNNNNNNn   ",
    "                ",
    "                "
)

# ===== Main =====
$rootDir = "e:\code\new\EconomyMod"
$uiDir = Join-Path $rootDir "Icons"
$eraDir = Join-Path $rootDir "GameResources\ui\Icons"

Write-Host "Generating pixel art icons..." -ForegroundColor Cyan
Write-Host ""

# Main icon
Render-Icon "icon" $icon_main $rootDir 128

# Era trait icons
Render-Icon "iconEraGolden" $iconEraGolden $eraDir 64
Render-Icon "iconEraRevival" $iconEraRevival $eraDir 64
Render-Icon "iconEraFlourish" $iconEraFlourish $eraDir 64
Render-Icon "iconEraCollapse" $iconEraCollapse $eraDir 64

# UI button icons
Render-Icon "coin" $coin $uiDir 64
Render-Icon "ledger" $ledger $uiDir 64
Render-Icon "flame" $flame $uiDir 64
Render-Icon "suppress" $suppress $uiDir 64
Render-Icon "collect" $collect $uiDir 64
Render-Icon "trash" $trash $uiDir 64
Render-Icon "crown" $crown $uiDir 64
Render-Icon "bell" $bell $uiDir 64
Render-Icon "phase_boom" $phase_boom $uiDir 64
Render-Icon "phase_recession" $phase_recession $uiDir 64
Render-Icon "phase_depression" $phase_depression $uiDir 64
Render-Icon "phase_recovery" $phase_recovery $uiDir 64

Write-Host ""
Write-Host "All icons generated!" -ForegroundColor Green
Write-Host "  Main icon:     $rootDir\icon.png (128x128)"
Write-Host "  Era traits:    $eraDir\ (64x64)"
Write-Host "  UI buttons:    $uiDir\ (64x64)"
