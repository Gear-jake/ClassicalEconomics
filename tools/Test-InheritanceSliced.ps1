param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'
$scriptPath = [System.IO.Path]::GetFullPath($PSCommandPath)
$repoRoot = [System.IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$isRepoRoot = ([System.IO.Path]::GetFullPath($Root) -eq $repoRoot)

# ===== Required files =====
$enginePath = Join-Path $Root 'Core\InheritanceEngine.cs'
$configPath = Join-Path $Root 'Models\UnrestConfig.cs'
$cbPath = Join-Path $Root 'Services\ConfigCallbacks.cs'
$jsonPath = Join-Path $Root 'default_config.json'
$localeDir = Join-Path $Root 'Locales'
$benchmarkPath = Join-Path $Root 'tools\InheritanceSlicedBenchmark.cs'

foreach ($p in @($enginePath, $configPath, $cbPath, $jsonPath, $benchmarkPath)) {
    if (-not (Test-Path -LiteralPath $p -PathType Leaf)) {
        Write-Host "INHERITANCE_SLICED_RED: required file not found: $p"
        exit 1
    }
}
foreach ($loc in @('ch.json', 'en.json', 'zh_tw.json', 'ru.json')) {
    $locPath = Join-Path $localeDir $loc
    if (-not (Test-Path -LiteralPath $locPath -PathType Leaf)) {
        Write-Host "INHERITANCE_SLICED_RED: locale file not found: $locPath"
        exit 1
    }
}

# ===== Engine anchors (sliced scan) =====
$engine = [System.IO.File]::ReadAllText($enginePath)

if ($engine -notmatch 'int cap = Mathf\.Clamp\(UnrestConfig\.Instance\.InheritanceScanPerFrame, 1, 100000\);') {
    Write-Host 'INHERITANCE_SLICED_RED: cap clamp line missing (must read InheritanceScanPerFrame with 1..100000 clamp)'
    exit 1
}
if (-not [Regex]::IsMatch($engine, '_timer = 0f;\s*_aliveMap\.Clear\(\);\s*_scanCursor = 0;\s*_scanActive = true;', [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'INHERITANCE_SLICED_RED: window-open block missing (timer reset, aliveMap clear, cursor reset, active=true)'
    exit 1
}
if (-not [Regex]::IsMatch($engine, '_staleIds\.Clear\(\);[\s\S]*?_timer = 0f;\s*_scanActive = false;\s*_scanCursor = 0;', [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'INHERITANCE_SLICED_RED: ClearWorldReferences reset lines missing (staleIds, timer, scanActive, cursor)'
    exit 1
}
if ($engine -notmatch 'while \(_scanCursor < aliveList\.Count && \(scanned < cap \|\| deadline\)\)') {
    Write-Host 'INHERITANCE_SLICED_RED: sliced scan loop header missing'
    exit 1
}
if ($engine -notmatch '_scanCursor\+\+;') {
    Write-Host 'INHERITANCE_SLICED_RED: _scanCursor++ missing'
    exit 1
}
if ($engine -notmatch 'scanned\+\+;') {
    Write-Host 'INHERITANCE_SLICED_RED: scanned++ missing'
    exit 1
}
if ($engine -notmatch 'bool deadline = _timer >= 3f;') {
    Write-Host 'INHERITANCE_SLICED_RED: deadline line missing (bool deadline = _timer >= 3f;)'
    exit 1
}
if ($engine -notmatch 'DamageTracker\.CheckActor\(actor\);') {
    Write-Host 'INHERITANCE_SLICED_RED: ScanActor must call DamageTracker.CheckActor(actor)'
    exit 1
}
if (-not [Regex]::IsMatch($engine, 'if \(_aliveMap\.ContainsKey\(id\)\) continue;[\s\S]*?ScanActor\(actor\);', [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'INHERITANCE_SLICED_RED: reconciliation pass missing (aliveMap skip then ScanActor)'
    exit 1
}
if (-not [Regex]::IsMatch($engine, 'foreach \(var kv in _aliveMap\)[\s\S]*?staleIds\.Add\(kv\.Key\);[\s\S]*?foreach \(var id in staleIds\) _aliveMap\.Remove\(id\);', [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'INHERITANCE_SLICED_RED: stale cleanup loop missing'
    exit 1
}
if ($engine -notmatch 'if \(!aliveMap\.ContainsKey\(kv\.Key\)\) deadIds\.Add\(kv\.Key\);') {
    Write-Host 'INHERITANCE_SLICED_RED: death pass line missing (deadIds.Add when missing from aliveMap)'
    exit 1
}

# ===== Config chain anchors =====
$config = [System.IO.File]::ReadAllText($configPath)
if ($config -notmatch 'public int InheritanceScanPerFrame = 2000;') {
    Write-Host 'INHERITANCE_SLICED_RED: UnrestConfig must declare public int InheritanceScanPerFrame = 2000;'
    exit 1
}

$cbText = [System.IO.File]::ReadAllText($cbPath)
if ($cbText -notmatch 'u\.InheritanceScanPerFrame = ParseInt\(ispf\.TextVal, u\.InheritanceScanPerFrame, 1, 100000\);') {
    Write-Host 'INHERITANCE_SLICED_RED: SyncFromModConfig must parse inheritance_scan_per_frame via bounded ParseInt'
    exit 1
}
if ($cbText -notmatch 'public static void OnInheritanceScanPerFrameChanged\(string pValue\)') {
    Write-Host 'INHERITANCE_SLICED_RED: OnInheritanceScanPerFrameChanged callback missing'
    exit 1
}
if ($cbText -notmatch '"inheritance_scan_per_frame"') {
    Write-Host 'INHERITANCE_SLICED_RED: AllConfigIds must register inheritance_scan_per_frame'
    exit 1
}

$json = Get-Content -LiteralPath $jsonPath -Raw -Encoding UTF8 | ConvertFrom-Json
$group = $json.economy_general
if (-not $group) {
    Write-Host 'INHERITANCE_SLICED_RED: default_config.json must expose an economy_general group'
    exit 1
}
$item = $group | Where-Object { $_.Id -eq 'inheritance_scan_per_frame' }
if (-not $item) {
    Write-Host 'INHERITANCE_SLICED_RED: default_config.json missing config key inheritance_scan_per_frame'
    exit 1
}
if ($item.Type -ne 'TEXT') {
    Write-Host 'INHERITANCE_SLICED_RED: inheritance_scan_per_frame must be a TEXT config entry'
    exit 1
}
if ($item.TextVal -ne '2000') {
    Write-Host 'INHERITANCE_SLICED_RED: inheritance_scan_per_frame default must be 2000'
    exit 1
}
if ($item.Callback -ne 'EconomyConfigCallbacks:OnInheritanceScanPerFrameChanged') {
    Write-Host 'INHERITANCE_SLICED_RED: inheritance_scan_per_frame callback must be EconomyConfigCallbacks:OnInheritanceScanPerFrameChanged'
    exit 1
}

# ===== Locale labels in all four locale files =====
foreach ($loc in @('ch.json', 'en.json', 'zh_tw.json', 'ru.json')) {
    $locPath = Join-Path $localeDir $loc
    $text = [System.IO.File]::ReadAllText($locPath)
    foreach ($key in @('inheritance_scan_per_frame', 'inheritance_scan_per_frame Description')) {
        if ($text -notmatch ('"' + [regex]::Escape($key) + '"')) {
            Write-Host "INHERITANCE_SLICED_RED: $loc missing locale key $key"
            exit 1
        }
    }
}

# ===== Compile and run the sliced-scan mirror benchmark =====
$cscCandidates = @(
    'C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe',
    'C:\Program Files\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe',
    'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
)
$csc = $cscCandidates | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1
if (-not $csc) {
    Write-Host "INHERITANCE_SLICED_RED: csc.exe not found. Checked: $($cscCandidates -join ', ')"
    exit 1
}

$runDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ("ClassicalEconomics.InheritanceSliced." + [Guid]::NewGuid().ToString('N'))
[System.IO.Directory]::CreateDirectory($runDirectory) | Out-Null
$out = Join-Path $runDirectory 'InheritanceSlicedBenchmark.exe'
try {
    & $csc /nologo /optimize+ "/out:$out" $benchmarkPath
    if ($LASTEXITCODE -ne 0) {
        Write-Host 'INHERITANCE_SLICED_RED: benchmark compile failed'
        exit $LASTEXITCODE
    }
    & $out
    if ($LASTEXITCODE -ne 0) {
        Write-Host 'INHERITANCE_SLICED_RED: benchmark run failed'
        exit $LASTEXITCODE
    }
} finally {
    if (Test-Path -LiteralPath $runDirectory) { Remove-Item -LiteralPath $runDirectory -Recurse -Force }
}

# ===== Mutation tests: each mutated engine copy must fail the gate (exit 1) =====
if ($isRepoRoot) {
    $mutationCases = @(
        @{ Name = 'cap hardcoded'; Apply = { param($t) $t -replace 'int cap = Mathf\.Clamp\(UnrestConfig\.Instance\.InheritanceScanPerFrame, 1, 100000\);', 'int cap = 1000;' } },
        @{ Name = 'cursor increment removed'; Apply = { param($t) $t -replace '_scanCursor\+\+;\r?\n', '' } },
        @{ Name = 'deadline false'; Apply = { param($t) $t -replace 'bool deadline = _timer >= 3f;', 'bool deadline = false;' } },
        @{ Name = 'CheckActor removed'; Apply = { param($t) $t -replace 'DamageTracker\.CheckActor\(actor\);', '' } },
        @{ Name = 'reconciliation removed'; Apply = { param($t) $t -replace 'if \(_aliveMap\.ContainsKey\(id\)\) continue;', '' } }
    )
    foreach ($case in $mutationCases) {
        $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("ClassicalEconomics.InheritanceSlicedMutation." + [Guid]::NewGuid().ToString('N'))
        try {
            [System.IO.Directory]::CreateDirectory((Join-Path $tempRoot 'Core')) | Out-Null
            [System.IO.Directory]::CreateDirectory((Join-Path $tempRoot 'Models')) | Out-Null
            [System.IO.Directory]::CreateDirectory((Join-Path $tempRoot 'Services')) | Out-Null
            [System.IO.Directory]::CreateDirectory((Join-Path $tempRoot 'Locales')) | Out-Null
            [System.IO.Directory]::CreateDirectory((Join-Path $tempRoot 'tools')) | Out-Null
            Copy-Item -LiteralPath $enginePath -Destination (Join-Path $tempRoot 'Core\InheritanceEngine.cs')
            Copy-Item -LiteralPath $configPath -Destination (Join-Path $tempRoot 'Models\UnrestConfig.cs')
            Copy-Item -LiteralPath $cbPath -Destination (Join-Path $tempRoot 'Services\ConfigCallbacks.cs')
            Copy-Item -LiteralPath $jsonPath -Destination (Join-Path $tempRoot 'default_config.json')
            Copy-Item -LiteralPath (Join-Path $localeDir 'ch.json') -Destination (Join-Path $tempRoot 'Locales\ch.json')
            Copy-Item -LiteralPath (Join-Path $localeDir 'en.json') -Destination (Join-Path $tempRoot 'Locales\en.json')
            Copy-Item -LiteralPath (Join-Path $localeDir 'zh_tw.json') -Destination (Join-Path $tempRoot 'Locales\zh_tw.json')
            Copy-Item -LiteralPath (Join-Path $localeDir 'ru.json') -Destination (Join-Path $tempRoot 'Locales\ru.json')
            Copy-Item -LiteralPath $benchmarkPath -Destination (Join-Path $tempRoot 'tools\InheritanceSlicedBenchmark.cs')

            $mutatedPath = Join-Path $tempRoot 'Core\InheritanceEngine.cs'
            $mutated = & $case.Apply ([System.IO.File]::ReadAllText($mutatedPath))
            [System.IO.File]::WriteAllText($mutatedPath, $mutated)

            & powershell -NoProfile -ExecutionPolicy Bypass -File $scriptPath -Root $tempRoot
            if ($LASTEXITCODE -ne 1) {
                Write-Host "INHERITANCE_SLICED_RED: mutation '$($case.Name)' was not caught (exit $LASTEXITCODE)"
                exit 1
            }
        } finally {
            if (Test-Path -LiteralPath $tempRoot) { Remove-Item -LiteralPath $tempRoot -Recurse -Force }
        }
    }
}

Write-Host 'INHERITANCE_SLICED_GREEN: sliced inheritance scan mirrors engine behavior and fails closed on every mutation'
exit 0