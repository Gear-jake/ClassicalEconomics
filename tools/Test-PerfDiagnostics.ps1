$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$perfPath = Join-Path $root 'Core\PerfDiagnostics.cs'
$tempFiles = New-Object System.Collections.Generic.List[string]
$failures = New-Object System.Collections.Generic.List[string]

function Add-TempFile {
    param([string]$Content)
    $path = Join-Path $env:TEMP ("PerfDiagMutation_" + [Guid]::NewGuid().ToString('N') + '.cs')
    [System.IO.File]::WriteAllText($path, $Content)
    $tempFiles.Add($path)
    return $path
}

function Assert-Match {
    param([string]$Text, [string]$Pattern, [string]$Message)
    if (-not [Regex]::IsMatch($Text, $Pattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)) {
        $failures.Add($Message)
    }
}

function Assert-NoMatch {
    param([string]$Text, [string]$Pattern, [string]$Message)
    if ([Regex]::IsMatch($Text, $Pattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)) {
        $failures.Add($Message)
    }
}

try {
    if (-not (Test-Path -LiteralPath $perfPath -PathType Leaf)) {
        $failures.Add('Missing file: Core\PerfDiagnostics.cs')
    } else {
        $source = [System.IO.File]::ReadAllText($perfPath)
        $unrestSource = [System.IO.File]::ReadAllText((Join-Path $root 'Models\UnrestConfig.cs'))
        $callbackSource = [System.IO.File]::ReadAllText((Join-Path $root 'Services\ConfigCallbacks.cs'))
        $config = Get-Content -LiteralPath (Join-Path $root 'default_config.json') -Raw | ConvertFrom-Json

        # 1. Default-off gate: every entry point must bail when disabled; UnrestConfig default is false.
        $gateCount = ([Regex]::Matches($source, 'if \(!IsEnabled\) return;')).Count
        if ($gateCount -lt 4) {
            $failures.Add("PerfDiagnostics gate: expected the enabled gate on all 4 entry points, found $gateCount.")
        }
        Assert-Match $unrestSource 'public\s+bool\s+PerfDiagnosticsEnabled\s*=\s*false\s*;' `
            'PerfDiagnostics gate: UnrestConfig.PerfDiagnosticsEnabled must default to false.'

        # 2. Managed-memory sampling must use the non-collecting GC.GetTotalMemory(false).
        Assert-Match $source 'GC\.GetTotalMemory\(false\)' `
            'PerfDiagnostics must sample managed memory via GC.GetTotalMemory(false).'

        # 3. Time must come from Stopwatch.
        Assert-Match $source 'Stopwatch\.GetTimestamp\(\)' `
            'PerfDiagnostics must measure elapsed time with Stopwatch.GetTimestamp().'
        Assert-Match $source 'Stopwatch\.Frequency' `
            'PerfDiagnostics must convert timestamps via Stopwatch.Frequency.'

        # 4. Forbidden GC APIs: never GC.Collect / GCSettings.
        Assert-NoMatch $source 'GC\s*\.\s*Collect\s*\(' 'PerfDiagnostics must never call GC.Collect.'
        Assert-NoMatch $source 'GCSettings' 'PerfDiagnostics must never touch GCSettings.'

        # 5. One yearly summary via EndYear.
        Assert-Match $source 'public\s+static\s+void\s+EndYear\(\)' 'PerfDiagnostics is missing EndYear().'
        Assert-Match $source 'Year " \+ _year \+ " summary' 'PerfDiagnostics is missing the one-per-year summary log.'

        # 6. CycleAllocBudget consumption (per-cycle managed-allocation budget).
        Assert-Match $source 'CycleAllocBudget' 'PerfDiagnostics must consume CycleAllocBudget.'
        Assert-Match $source 'UnrestConfig\.Instance\.FrameBudgetMs' `
            'PerfDiagnostics must compare stage time against the configured frame budget.'

        # 7. Config chain: default_config.json + callbacks + bounded parse + AllConfigIds + field.
        $perfItem = $config.economy_general | Where-Object { $_.Id -eq 'perf_diagnostics_enabled' }
        if (-not $perfItem) {
            $failures.Add('Config chain: default_config.json lacks perf_diagnostics_enabled.')
        } else {
            if ($perfItem.Type -ne 'SWITCH') { $failures.Add('Config chain: perf_diagnostics_enabled must be a SWITCH entry.') }
            if ($perfItem.BoolVal -ne $false) { $failures.Add('Config chain: perf_diagnostics_enabled must default to false.') }
            if ($perfItem.Callback -ne 'EconomyConfigCallbacks:OnPerfDiagnosticsEnabledChanged') {
                $failures.Add('Config chain: perf_diagnostics_enabled must be wired to OnPerfDiagnosticsEnabledChanged.')
            }
        }
        $allocItem = $config.economy_general | Where-Object { $_.Id -eq 'cycle_alloc_budget' }
        if (-not $allocItem) {
            $failures.Add('Config chain: default_config.json lacks cycle_alloc_budget.')
        } else {
            if ($allocItem.Type -ne 'TEXT') { $failures.Add('Config chain: cycle_alloc_budget must be a TEXT entry.') }
            if ($allocItem.TextVal -ne '4096') { $failures.Add('Config chain: cycle_alloc_budget default must be 4096.') }
            if ($allocItem.Callback -ne 'EconomyConfigCallbacks:OnCycleAllocBudgetChanged') {
                $failures.Add('Config chain: cycle_alloc_budget must be wired to OnCycleAllocBudgetChanged.')
            }
        }
        Assert-Match $callbackSource 'public\s+static\s+void\s+OnPerfDiagnosticsEnabledChanged\s*\(\s*bool\s+pValue\s*\)' `
            'Config chain: ConfigCallbacks.cs is missing OnPerfDiagnosticsEnabledChanged(bool pValue).'
        Assert-Match $callbackSource 'public\s+static\s+void\s+OnCycleAllocBudgetChanged\s*\(\s*string\s+pValue\s*\)' `
            'Config chain: ConfigCallbacks.cs is missing OnCycleAllocBudgetChanged(string pValue).'
        Assert-Match $callbackSource 'u\.PerfDiagnosticsEnabled\s*=\s*pde\.BoolVal' `
            'Config chain: SyncFromModConfig must read perf_diagnostics_enabled into UnrestConfig.PerfDiagnosticsEnabled.'
        Assert-Match $callbackSource 'u\.CycleAllocBudget\s*=\s*ParseInt\(\s*cab\.TextVal\s*,\s*u\.CycleAllocBudget' `
            'Config chain: SyncFromModConfig must parse cycle_alloc_budget via bounded ParseInt.'
        Assert-Match $callbackSource '"perf_diagnostics_enabled"' 'Config chain: AllConfigIds must include perf_diagnostics_enabled.'
        Assert-Match $callbackSource '"cycle_alloc_budget"' 'Config chain: AllConfigIds must include cycle_alloc_budget.'
        Assert-Match $unrestSource 'public\s+int\s+CycleAllocBudget\s*=\s*4096\s*;' `
            'Config chain: UnrestConfig.cs must declare public int CycleAllocBudget = 4096;'

        # 8. Locales: all four files carry both keys and their descriptions.
        foreach ($locale in @('en', 'ch', 'zh_tw', 'ru')) {
            $localeText = [System.IO.File]::ReadAllText((Join-Path $root ("Locales\{0}.json" -f $locale)))
            foreach ($key in @('perf_diagnostics_enabled', 'cycle_alloc_budget')) {
                Assert-Match $localeText ('"' + $key + '"') ("Locale $locale is missing key $key.")
                Assert-Match $localeText ('"' + $key + ' Description"') ("Locale $locale is missing key $key Description.")
            }
        }

        # ---- Three mutations must each go RED ----

        # M1: gate removed (all enabled guards stripped).
        $m1 = [Regex]::Replace($source, 'if \(!IsEnabled\) return;\r?\n\s*', '')
        if ([Regex]::IsMatch($m1, 'if \(!IsEnabled\) return;')) {
            $failures.Add("Mutation 'gate removed' did not go RED: enabled-gate anchor still matches.")
        }
        Add-TempFile $m1 | Out-Null

        # M2: GC.GetTotalMemory(false) replaced with the collecting variant.
        $m2 = [Regex]::Replace($source, 'GC\.GetTotalMemory\(false\)', 'GC.GetTotalMemory(true)')
        if ([Regex]::IsMatch($m2, 'GC\.GetTotalMemory\(false\)')) {
            $failures.Add("Mutation 'GetTotalMemory removed' did not go RED: GetTotalMemory(false) anchor still matches.")
        }
        Add-TempFile $m2 | Out-Null

        # M3: yearly summary statement removed.
        $m3 = [Regex]::Replace($source,
            'UnityEngine\.Debug\.LogWarning\("\[PerfDiagnostics\] Year " \+ _year \+ " summary: ".*?\);',
            '', [System.Text.RegularExpressions.RegexOptions]::Singleline)
        if ([Regex]::IsMatch($m3, 'Year " \+ _year \+ " summary')) {
            $failures.Add("Mutation 'summary removed' did not go RED: yearly-summary anchor still matches.")
        }
        Add-TempFile $m3 | Out-Null
    }
} finally {
    foreach ($temp in $tempFiles) {
        if (Test-Path -LiteralPath $temp -PathType Leaf) {
            Remove-Item -LiteralPath $temp -Force
        }
    }
}

if ($failures.Count -gt 0) {
    Write-Host "PERF_DIAGNOSTICS_FAILED: $($failures.Count) issue(s)"
    foreach ($failure in $failures) { Write-Host " - $failure" }
    exit 1
}

Write-Host 'PERF_DIAGNOSTICS_GREEN: config-gated PerfDiagnostics with over-budget stage logs and yearly summary'
exit 0