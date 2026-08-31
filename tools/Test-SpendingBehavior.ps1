$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = [System.IO.File]::ReadAllText((Join-Path $root 'Core\SpendingEngine.cs'))
$buy = [Regex]::Match($source, 'private static bool TryBuyWeapon.*?private static bool TryPayTax', [Text.RegularExpressions.RegexOptions]::Singleline).Value
$craft = [Regex]::Match($source, 'private static bool TryCraftArsenal.*?private static bool TryWholesaleWeapons', [Text.RegularExpressions.RegexOptions]::Singleline).Value
$wholesale = [Regex]::Match($source, 'private static bool TryWholesaleWeapons.*?private static bool TryEraEvent', [Text.RegularExpressions.RegexOptions]::Singleline).Value

$valid = $buy -notmatch 'CanUseCityAction'
$valid = $valid -and [Regex]::IsMatch($craft, 'int craftCount = Mathf\.Clamp\(spend / 30, 3, 5\).*for \(int i = 0; i < craftCount; i\+\+\)', [Text.RegularExpressions.RegexOptions]::Singleline)
$valid = $valid -and [Regex]::IsMatch($wholesale, 'int craftCount = Mathf\.Clamp\(spend / 40, 6, 10\).*for \(int i = 0; i < craftCount; i\+\+\)', [Text.RegularExpressions.RegexOptions]::Singleline)
if (-not $valid) {
    Write-Host 'SPENDING_BEHAVIOR_RED: city cooldown or single-item crafting changes actor-level spending behavior'
    exit 1
}
Write-Host 'SPENDING_BEHAVIOR_GREEN: actor-level weapon spending and batch outcomes are preserved'
exit 0
