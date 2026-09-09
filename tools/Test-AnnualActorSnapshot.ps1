param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'
$failures = New-Object System.Collections.Generic.List[string]
$utf8 = New-Object System.Text.UTF8Encoding($false)

function Assert-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) { $failures.Add($Message) }
}

$collector = [System.IO.File]::ReadAllText((Join-Path $Root 'Core\DataCollector.cs'), $utf8)
$cycle = [System.IO.File]::ReadAllText((Join-Path $Root 'Core\EconomyCycleModulator.cs'), $utf8)
$tax = [regex]::Match($collector, 'public static void ApplyWealthTax\((?<body>.*?)private static void UpdateTopRich\(', [System.Text.RegularExpressions.RegexOptions]::Singleline).Groups['body'].Value

Assert-True ($collector -match 'public static readonly List<Actor> AllCivPool') 'DataCollector must expose a retained all-civilized-actor pool'
Assert-True ($collector -match 'AllCivPool\.Clear\(\)') 'AllCivPool must be cleared before a new collection/world reset'
Assert-True ($collector -match 'AllCivPool\.Add\(actor\)') 'Collect must retain each civilized actor reference'
Assert-True ($tax -match 'foreach \(var actor in AllCivPool\)') 'ApplyWealthTax must iterate the retained actor pool'
Assert-True ($tax -notmatch 'units_only_alive') 'ApplyWealthTax must not reacquire the full alive-list'
Assert-True ($cycle -match 'TriggerBubbleBurst[\s\S]*?AllCivPool') 'bubble burst must consume the retained actor pool'
Assert-True ($cycle -match 'InjectCoinsToAllCiv[\s\S]*?AllCivPool') 'boom injection must consume the retained actor pool'

if ($failures.Count -gt 0) {
    foreach ($failure in $failures) { Write-Host "ANNUAL_ACTOR_SNAPSHOT_RED: $failure" }
    Write-Host "ANNUAL_ACTOR_SNAPSHOT_RED: $($failures.Count) check(s) failed"
    exit 1
}

Write-Host 'ANNUAL_ACTOR_SNAPSHOT_GREEN: retained actor pool lifecycle and downstream consumers are wired'
exit 0
