$ErrorActionPreference = 'Continue'
$libs = 'E:\code\new\libs'
$script:loaded = @{}
$script:resolving = @{}

$handler = [System.ResolveEventHandler] {
    param($s, $e)
    $name = (New-Object System.Reflection.AssemblyName($e.Name)).Name
    if ($script:loaded.ContainsKey($name)) { return $script:loaded[$name] }
    if ($script:resolving.ContainsKey($name)) { return $null }
    $script:resolving[$name] = $true
    $p = Join-Path $libs ($name + '.dll')
    if (Test-Path $p) {
        try {
            $a = [System.Reflection.Assembly]::LoadFrom($p)
            $script:loaded[$name] = $a
            return $a
        } catch { return $null }
    }
    return $null
}
[System.AppDomain]::CurrentDomain.add_AssemblyResolve($handler)

$asm = [System.Reflection.Assembly]::LoadFrom((Join-Path $libs 'Assembly-CSharp.dll'))
$script:loaded['Assembly-CSharp'] = $asm
try { $types = $asm.GetTypes() } catch [System.Reflection.ReflectionTypeLoadException] { $types = $_.Exception.Types | Where-Object { $_ -ne $null } }
$bm = $types | Where-Object { $_.Name -eq 'BuildingManager' } | Select-Object -First 1
Write-Host ('BuildingManager found: ' + ($bm -ne $null))
if ($bm) {
    $bm.GetMethods([System.Reflection.BindingFlags]'Public,NonPublic,Instance,Static,DeclaredOnly') |
        Where-Object { $_.Name -match 'build|Build' } |
        ForEach-Object { $_.IsStatic.ToString().Substring(0,1) + ' ' + $_.Name + '(' + (($_.GetParameters() | ForEach-Object { $_.ParameterType.Name }) -join ',') + ')' }
}
Write-Host '=== City build methods ==='
$city = $types | Where-Object { $_.Name -eq 'City' } | Select-Object -First 1
if ($city) {
    $city.GetMethods([System.Reflection.BindingFlags]'Public,NonPublic,Instance,DeclaredOnly') |
        Where-Object { $_.Name -match 'build|Build' } |
        ForEach-Object { $_.Name + '(' + (($_.GetParameters() | ForEach-Object { $_.ParameterType.Name }) -join ',') + ')' }
}
