<#
    Nkanyezi Tours Information System
    setup-database.ps1  -  one-click database setup for a new machine
    CMPG223 Group 6

    WHY YOU ARE RUNNING THIS
    ------------------------
    The database does not travel with the code. Web.config points at a SQL
    Server instance on YOUR OWN laptop, and that instance starts out empty.
    That is why the app works on one machine and says "connection string
    error" or "Login failed for user ..." on another one.

    This script:
      1. Finds whichever SQL Server you actually have (LocalDB or Express).
      2. Creates the NkanyeziTours database if it is not there yet.
      3. Runs 01-schema.sql and then 03-seed-data.sql.
      4. Writes the correct Server= value into Web.config for you.

    HOW TO RUN IT
    -------------
    Easiest:  double-click setup-database.bat (next to this file).
    Or in PowerShell, from this folder:
        powershell -ExecutionPolicy Bypass -File .\setup-database.ps1

    If you know your server name and want to skip auto-detection:
        powershell -ExecutionPolicy Bypass -File .\setup-database.ps1 -Server ".\SQLEXPRESS"

    Safe to run again. The SQL scripts drop and recreate everything, so any
    test data you typed in yourself will be lost. That is on purpose - we all
    need identical data.
#>

[CmdletBinding()]
param(
    # Force a specific instance instead of auto-detecting, e.g. ".\SQLEXPRESS"
    [string] $Server,

    # Skip 03-seed-data.sql (schema only, empty tables)
    [switch] $NoSeed,

    # Do not touch Web.config
    [switch] $NoConfigUpdate,

    # Build-safe. If the database already has tables, change nothing and exit 0.
    # The build passes this, and it is not optional there: without it a rebuild
    # would run 01-schema.sql, which DROPS every table.
    [switch] $IfMissing
)

$ErrorActionPreference = 'Stop'

function Write-Step ($m) { Write-Host "`n==> $m" -ForegroundColor Cyan }
function Write-Ok   ($m) { Write-Host "    OK  $m" -ForegroundColor Green }
function Write-Warn ($m) { Write-Host "    !!  $m" -ForegroundColor Yellow }
function Write-Fail ($m) { Write-Host "`nFAILED: $m" -ForegroundColor Red }

$DbName     = 'NkanyeziTours'
$Root       = $PSScriptRoot
$SchemaFile = Join-Path $Root '01-schema.sql'
$SeedFile   = Join-Path $Root '03-seed-data.sql'
$WebConfig  = Join-Path $Root '..\Program\Nkanyezi Tours 2.0\NkanyeziTours\Web.config'

Write-Host ''
Write-Host '  Nkanyezi Tours - database setup' -ForegroundColor White
Write-Host '  --------------------------------'

# ---------------------------------------------------------------- 1. sqlcmd
Write-Step 'Looking for sqlcmd.exe'

$sqlcmd = (Get-Command sqlcmd.exe -ErrorAction SilentlyContinue).Source
if (-not $sqlcmd) {
    $search = @(
        "$env:ProgramFiles\Microsoft SQL Server\Client SDK\ODBC\*\Tools\Binn\sqlcmd.exe",
        "${env:ProgramFiles(x86)}\Microsoft SQL Server\Client SDK\ODBC\*\Tools\Binn\sqlcmd.exe",
        "$env:ProgramFiles\Microsoft SQL Server\*\Tools\Binn\sqlcmd.exe"
    )
    $sqlcmd = Get-ChildItem -Path $search -ErrorAction SilentlyContinue |
              Sort-Object FullName -Descending |
              Select-Object -First 1 -ExpandProperty FullName
}
if (-not $sqlcmd) {
    Write-Fail 'sqlcmd.exe was not found on this machine.'
    Write-Host '    You do not have SQL Server (or its command line tools) installed yet.'
    Write-Host '    Install ONE of these, then run this script again:'
    Write-Host ''
    Write-Host '      * SQL Server Express (recommended, includes LocalDB):'
    Write-Host '        https://www.microsoft.com/en-us/sql-server/sql-server-downloads'
    Write-Host '      * Or just the tools:'
    Write-Host '        winget install Microsoft.SQLServer.2022.CommandLineUtilities'
    Write-Host ''
    Write-Host '    Visual Studio users: LocalDB ships with the "Data storage and'
    Write-Host '    processing" workload - add it from the Visual Studio Installer.'
    exit 1
}
Write-Ok $sqlcmd

# ------------------------------------------------------- 2. find an instance
function Test-Instance ($name) {
    & $sqlcmd -S $name -E -C -l 8 -b -Q "SET NOCOUNT ON; SELECT 1" 2>&1 | Out-Null
    return ($LASTEXITCODE -eq 0)
}

Write-Step 'Looking for a SQL Server instance'

if ($Server) {
    $candidates = @($Server)
} else {
    $candidates = @('(localdb)\MSSQLLocalDB', '.\SQLEXPRESS', 'localhost', '.')

    # LocalDB may exist but be stopped or not created yet - fix that first.
    $localdb = (Get-Command sqllocaldb.exe -ErrorAction SilentlyContinue).Source
    if ($localdb) {
        $instances = & $localdb info 2>$null
        if ($instances -notcontains 'MSSQLLocalDB') {
            Write-Warn 'LocalDB instance MSSQLLocalDB does not exist - creating it'
            & $localdb create MSSQLLocalDB 2>&1 | Out-Null
        }
        & $localdb start MSSQLLocalDB 2>&1 | Out-Null
    }
}

$instance = $null
foreach ($c in $candidates) {
    Write-Host "    trying $c ..." -NoNewline
    if (Test-Instance $c) {
        Write-Host ' reachable' -ForegroundColor Green
        $instance = $c
        break
    }
    Write-Host ' no' -ForegroundColor DarkGray
}

if (-not $instance) {
    Write-Fail 'Could not connect to any SQL Server instance on this machine.'
    Write-Host '    Things to check:'
    Write-Host '      * Open Services (services.msc) and make sure a service starting'
    Write-Host '        with "SQL Server (" is Running.'
    Write-Host '      * Run  sqllocaldb info  to list your LocalDB instances.'
    Write-Host '      * If your instance has a different name, pass it in yourself:'
    Write-Host '            .\setup-database.ps1 -Server ".\YOURINSTANCE"'
    exit 1
}
Write-Ok "Using instance: $instance"

# ------------------------------------------------------ 3. create the database
Write-Step "Creating database [$DbName] if it does not exist"

& $sqlcmd -S $instance -E -C -b -Q "IF DB_ID('$DbName') IS NULL CREATE DATABASE [$DbName];"
if ($LASTEXITCODE -ne 0) {
    Write-Fail "Could not create the database on $instance."
    Write-Host '    Your Windows account may not have permission on that instance.'
    exit 1
}
Write-Ok "$DbName is ready"

# ------------------------------------------------------------- 4. run scripts
function Invoke-SqlFile ($file, $label) {
    if (-not (Test-Path $file)) { Write-Fail "Missing SQL file: $file"; exit 1 }
    Write-Step "Running $label"
    & $sqlcmd -S $instance -E -C -b -d $DbName -i $file
    if ($LASTEXITCODE -ne 0) {
        Write-Fail "$label reported an error (see the messages above)."
        exit 1
    }
    Write-Ok "$label completed"
}

if ($IfMissing) {
    $old = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    $existing = & $sqlcmd -S $instance -E -C -b -d $DbName -h -1 -W `
                          -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.tables;" 2>$null
    $countQueryWorked = ($LASTEXITCODE -eq 0)
    $ErrorActionPreference = $old

    $count = -1
    $parsed = 0
    if ($countQueryWorked -and $existing -and
        [int]::TryParse(($existing | Select-Object -First 1).ToString().Trim(), [ref] $parsed)) {
        $count = $parsed
    }

    if ($count -lt 0) {
        Write-Warn "Could not check whether $DbName already has tables, so nothing was changed."
        Write-Warn 'Start SQL Server or LocalDB and build again, or run this script without -IfMissing to rebuild on purpose.'
        exit 0
    }

    if ($count -gt 0) {
        Write-Ok "$DbName already has $count tables - nothing to do"
        exit 0
    }
}

Invoke-SqlFile $SchemaFile '01-schema.sql (tables)'
if ($NoSeed) { Write-Warn 'Skipping seed data (-NoSeed given)' }
else         { Invoke-SqlFile $SeedFile '03-seed-data.sql (test data)' }

# ---------------------------------------------------------- 5. patch Web.config
if (-not $NoConfigUpdate) {
    Write-Step 'Updating Web.config'
    if (-not (Test-Path $WebConfig)) {
        Write-Warn "Web.config not found at $WebConfig - set Server= by hand."
    } else {
        $full    = (Resolve-Path $WebConfig).Path
        $text    = Get-Content -Path $full -Raw
        $escaped = $instance -replace '\$', '$$$$'   # protect $ in the replacement
        $new     = [regex]::Replace($text, '(?i)(connectionString="Server=)[^;"]*', "`${1}$escaped")

        if ($new -ne $text) {
            Copy-Item $full "$full.bak" -Force
            Set-Content -Path $full -Value $new -Encoding UTF8 -NoNewline
            Write-Ok "Server= set to $instance (previous file saved as Web.config.bak)"
        } else {
            Write-Ok "Already pointing at $instance - no change needed"
        }
    }
}

# ------------------------------------------------------------------ 6. verify
Write-Step 'Checking that the tables are really there'
& $sqlcmd -S $instance -E -C -b -d $DbName -h -1 -W -Q "SET NOCOUNT ON; SELECT CONCAT(COUNT(*), ' tables, ', (SELECT COUNT(*) FROM sys.objects WHERE type = 'F'), ' foreign keys') FROM sys.tables;"

Write-Host ''
Write-Host '  Done. Open the solution in Visual Studio and press F5.' -ForegroundColor Green
Write-Host "  Server:   $instance"
Write-Host "  Database: $DbName"
Write-Host ''
