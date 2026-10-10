$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

# ─────────────────────────────────────────────────────────────────────────────
# W3.2 契约门禁：存档键必须单一真源（只在 Core\SaveSchema.cs 出现字面量），
# 且**键值逐字不变**（改了键值 = 玩家旧存档的数据全部读不到，且不会报错）。
#
# 三层保护：
#   1. 字面量收口：其它 .cs 里不得再出现 "rb_*" 字符串字面量（注释不算）；
#   2. 键值冻结：把 2026-10-10 收敛前的历史键值清单写死在门禁里做**逐字比对**；
#   3. 读写对称：每个数据键都必须同时有写入点与读取点（防"只写不读"或"只读不写"）。
# ─────────────────────────────────────────────────────────────────────────────

$schemaPath = Join-Path $root 'Core\SaveSchema.cs'
$nationPath = Join-Path $root 'Core\NationSave.cs'

foreach ($p in @($schemaPath, $nationPath)) {
    if (-not (Test-Path -LiteralPath $p -PathType Leaf)) {
        Write-Host "SAVE_SCHEMA_RED: source file not found: $p"
        exit 1
    }
}

# 收敛前（v2.2.0 工作树，2026-10-10 盘点）真实存在的键值 —— 一个字都不能变。
$FrozenKeys = @(
    'rb_world_id', 'rb_hist', 'rb_ev_stream',
    'rb_nat_kingdom', 'rb_nat_treasury', 'rb_nat_buildings', 'rb_nat_records',
    'rb_nat_pacts', 'rb_nat_goodwill',
    'rb_ev_pending', 'rb_ev_cooldown', 'rb_ev_lastGlobal', 'rb_ev_chains',
    'rb_bank_set', 'rb_bank_ledger', 'rb_bank_ai',
    'rb_law_style'
)
$FrozenPrefixes = @('rb_nat_slot_', 'rb_law_law_', 'rb_law_policy_')
$AddedKeys = @('rb_schema_ver')   # v3.0 新增（纯追加键）

function Get-CodeText([string]$path) {
    # 去掉整行注释与行尾注释后再做字面量扫描：文档注释里提到键名是允许的
    $lines = [System.IO.File]::ReadAllLines($path)
    $kept = New-Object System.Collections.Generic.List[string]
    foreach ($ln in $lines) {
        $t = $ln.TrimStart()
        if ($t.StartsWith('//') -or $t.StartsWith('///') -or $t.StartsWith('*')) { continue }
        $kept.Add($ln)
    }
    return ($kept -join "`n")
}

function Get-SaveSchemaViolation([string]$schema, [string]$repoCode, [string]$nation) {
    $sl = [System.Text.RegularExpressions.RegexOptions]::Singleline

    # 1) 键值冻结：历史键值必须逐字仍在 SaveSchema.cs 中
    foreach ($k in $FrozenKeys) {
        if (-not $schema.Contains('"' + $k + '"')) { return "frozen-key-changed-or-missing: $k" }
    }
    foreach ($k in $FrozenPrefixes) {
        if (-not $schema.Contains('"' + $k + '"')) { return "frozen-prefix-changed-or-missing: $k" }
    }
    foreach ($k in $AddedKeys) {
        if (-not $schema.Contains('"' + $k + '"')) { return "added-key-missing: $k" }
    }

    # 2) 字面量收口：SaveSchema.cs 之外不得再出现 rb_ 字面量（$repoCode 已排除该文件）
    foreach ($hit in [Regex]::Matches($repoCode, '"rb_[A-Za-z0-9_]*"')) {
        return "rb-literal-outside-SaveSchema: $($hit.Value)"
    }

    # 3) 每个固定键都必须被真正使用（≥1 处引用）
    foreach ($k in ($FrozenKeys + $AddedKeys)) {
        $constName = [Regex]::Match($schema, 'public const string ([A-Za-z_][A-Za-z0-9_]*) = "' + [Regex]::Escape($k) + '";')
        if (-not $constName.Success) { return "no-const-for-key: $k" }
        $name = $constName.Groups[1].Value
        if (-not $repoCode.Contains('SaveSchema.' + $name)) { return "const-never-used: $name" }
    }

    # 4) 读写对称：数据键必须既有写入点也有读取点
    $setUses = [Regex]::Matches($repoCode, '\.set\(SaveSchema\.')
    if ($setUses.Count -lt 8) { return "too-few-write-sites ($($setUses.Count))" }
    $readUses = [Regex]::Matches($repoCode, '(\.get\(SaveSchema\.|ReadAnyKingdomKey\(SaveSchema\.)')
    if ($readUses.Count -lt 8) { return "too-few-read-sites ($($readUses.Count))" }

    # 5) 版本键必须写入（SavePrefix）并读取（恢复路径）
    if (-not [Regex]::IsMatch($nation, 'SavePrefix\(\)[\s\S]*?set\(SaveSchema\.SchemaVersion', $sl)) { return 'version-not-written-on-save' }
    if (-not [Regex]::IsMatch($nation, 'RestoreNation\(Kingdom[\s\S]*?get\(SaveSchema\.SchemaVersion', $sl)) { return 'version-not-read-on-load' }

    # 6) 参数化键必须走构造函数（禁止在调用点拼字符串）
    if ([Regex]::IsMatch($repoCode, 'SaveSchema\.NationSlotPrefix\s*\+', $sl)) { return 'parameterized-key-concatenated-at-callsite' }
    if ([Regex]::IsMatch($repoCode, 'SaveSchema\.LawLevelPrefix\s*\+', $sl)) { return 'law-key-concatenated-at-callsite' }
    if (-not [Regex]::IsMatch($repoCode, 'SaveSchema\.NationSlot\(i\)', $sl)) { return 'nation-slot-builder-unused' }
    if (-not [Regex]::IsMatch($repoCode, 'SaveSchema\.LawLevel\(i\)', $sl)) { return 'law-level-builder-unused' }

    return ''
}

# 全仓源码（排除构建产物/发布快照），逐文件去注释后拼接
$exclude = '\\(release|bin|obj|evidence|tools|\.git)\\'
$files = @(Get-ChildItem -LiteralPath $root -Recurse -File -Filter '*.cs' |
    Where-Object { $_.FullName -notmatch $exclude })
if ($files.Count -eq 0) {
    Write-Host 'SAVE_SCHEMA_RED: no .cs files found (gate would be vacuous)'
    exit 1
}

$schema = Get-CodeText $schemaPath
$nation = Get-CodeText $nationPath
$others = New-Object System.Text.StringBuilder
foreach ($f in $files) {
    if ($f.FullName -eq $schemaPath) { continue }
    [void]$others.AppendLine((Get-CodeText $f.FullName))
}
$repoCode = $others.ToString()

$violation = Get-SaveSchemaViolation $schema $repoCode $nation
if ($violation -ne '') {
    Write-Host "SAVE_SCHEMA_RED: $violation"
    exit 1
}

function Test-Mutation([string]$name, [string]$s, [string]$c, [string]$n) {
    $v = Get-SaveSchemaViolation $s $c $n
    if ($v -eq '') {
        Write-Host "SAVE_SCHEMA_RED: mutation '$name' did not turn RED (gate not sensitive)"
        exit 1
    }
    Write-Host "MUTATION_RED_OK: $name -> $v"
}

Test-Mutation 'key-value-changed' ($schema -replace '"rb_nat_treasury"', '"rb_nation_treasury"') $repoCode $nation
Test-Mutation 'stray-literal-reintroduced' $schema ($repoCode + "`n" + 'k.data.set("rb_hist", v);') $nation
Test-Mutation 'version-not-written' $schema $repoCode ($nation -replace 'set\(SaveSchema\.SchemaVersion', 'set("rb_schema_ver"')
Test-Mutation 'key-concatenated-at-callsite' $schema ($repoCode + "`n" + 'k.data.get(SaveSchema.NationSlotPrefix + i, out v);') $nation
Test-Mutation 'const-never-used' $schema ($repoCode -replace 'SaveSchema\.BankAi', 'X') $nation

Write-Host ("SAVE_SCHEMA_GREEN: {0} fixed keys frozen + literals centralised + read/write symmetry + 5 mutations RED" -f ($FrozenKeys.Count + $AddedKeys.Count))
exit 0
