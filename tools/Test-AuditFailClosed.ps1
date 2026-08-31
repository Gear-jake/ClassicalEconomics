$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = [System.IO.File]::ReadAllText((Join-Path $root 'performance_audit.ps1'))

if ($source -notmatch '\$ErrorActionPreference\s*=\s*''Stop''') {
    Write-Host 'AUDIT_FAIL_CLOSED_RED: audit does not stop on tool errors'
    exit 1
}
if ($source -match '(?<!System\.Text\.RegularExpressions\.)\[RegexOptions\]') {
    Write-Host 'AUDIT_FAIL_CLOSED_RED: audit uses an unresolved RegexOptions type'
    exit 1
}

Write-Host 'AUDIT_FAIL_CLOSED_GREEN: audit tool errors terminate the gate'
exit 0
