$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $PSScriptRoot

function Assert-True($cond, $msg) {
    if (-not $cond) { Write-Host "RULER_PERSONALITY_RED: $msg"; exit 1 }
}

# ===== 1. RulerEngine：读原版性格机制（不虚构、不掷骰、不落盘）=====
$eng = [System.IO.File]::ReadAllText((Join-Path $Root 'Core\RulerEngine.cs'))
foreach ($trait in @('"greedy"', '"deceitful"', '"honest"', '"content"', '"ambitious"', '"wise"')) {
    Assert-True ($eng -match ('HasTrait\(king, ' + [regex]::Escape($trait) + '\)')) "RulerEngine must read vanilla king trait $trait via HasTrait"
}
foreach ($stat in @('personality_administration', 'personality_aggression', 'personality_diplomatic', 'personality_rationality')) {
    Assert-True ($eng -match [regex]::Escape($stat)) "RulerEngine must read vanilla personality stat $stat"
}
Assert-True ($eng -match 'law_parliament') 'parliament damping must read the codex parliament law'
Assert-True ($eng -match '_profiles\.Clear\(\)') 'RulerEngine.Reset must clear the profile cache (derived state, no save keys)'

# ===== 2. 修正器齐全 =====
foreach ($m in @('AdjustLaw', 'PolicySuccessMult', 'BlocksRedistribution', 'TreasurySkim', 'OptionBias', 'DescribeRuler')) {
    Assert-True ($eng -match ('public static ' + '.*' + $m)) "RulerEngine must expose $m"
}

# ===== 3. 挂点 =====
$lawAi = [System.IO.File]::ReadAllText((Join-Path $Root 'Core\LawAi.cs'))
Assert-True ($lawAi -match 'RulerEngine\.AdjustLaw\(kingdom\.data\.id, key, curPre, suggest\)') 'LawAi must apply the ruler bias layer after StyleAdjust'

$policy = [System.IO.File]::ReadAllText((Join-Path $Root 'Core\PolicyEngine.cs'))
Assert-True ($policy -match 'RollSuccess\(long kingdomId, float gini, PolicyKind kind\)') 'RollSuccess must take kingdomId'
Assert-True ($policy -match 'success \* RulerEngine\.PolicySuccessMult') 'policy success must be scaled by ruler personality'
Assert-True ($policy -match 'RulerEngine\.BlocksRedistribution') 'greedy rulers must be able to block redistribution'

$nation = [System.IO.File]::ReadAllText((Join-Path $Root 'Core\NationEngine.cs'))
Assert-True ($nation -match 'RulerEngine\.TreasurySkim') 'NationEngine must apply treasury skimming'
Assert-True ($nation -match '"ruler_skim"') 'treasury skimming must be recorded in the track record'

$de = [System.IO.File]::ReadAllText((Join-Path $Root 'Core\DecisionEvents.cs'))
Assert-True ($de -match 'RulerEngine\.OptionBias') 'AI event choices must apply ruler personality bias'

# ===== 4. 配置键接线 =====
$cfg = [System.IO.File]::ReadAllText((Join-Path $Root 'Models\UnrestConfig.cs'))
Assert-True ($cfg -match 'public bool RulerPersonalityEnabled = true;') 'UnrestConfig must declare RulerPersonalityEnabled'
$cb = [System.IO.File]::ReadAllText((Join-Path $Root 'Services\ConfigCallbacks.cs'))
Assert-True ($cb -match 'OnRulerPersonalityEnabledChanged') 'ConfigCallbacks must expose the toggle callback'
Assert-True ($cb -match '"ruler_personality_enabled"') 'AllConfigIds must contain ruler_personality_enabled'

# ===== 5. 四语键 =====
$localeDir = Join-Path $Root 'Locales'
$required = @('cabinet_ruler', 'ruler_trait_greedy', 'ruler_trait_deceitful', 'ruler_trait_honest',
    'ruler_trait_content', 'ruler_trait_ambitious', 'ruler_trait_wise', 'ruler_trait_none',
    'ruler_personality_enabled', 'ruler_personality_enabled Description', 'ruler_skim')
foreach ($loc in @('ch.json', 'zh_tw.json', 'en.json', 'ru.json')) {
    $lp = Join-Path $localeDir $loc
    if (-not (Test-Path -LiteralPath $lp -PathType Leaf)) { Fail "locale file not found: $loc" }
    $locData = Get-Content -LiteralPath $lp -Raw -Encoding UTF8 | ConvertFrom-Json
    $props = $locData.PSObject.Properties.Name
    foreach ($key in $required) {
        if ($props -notcontains $key) { Fail "$loc missing key '$key'" }
    }
}

Write-Host 'RULER_PERSONALITY_GREEN: vanilla trait/stat reads, all modifier hooks, config wiring and 4-locale coverage pass'
exit 0
