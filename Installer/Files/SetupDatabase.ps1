# ============================================================
# RVMS Database Setup Script
# Called by Inno Setup installer to:
#   1. Test SQL Server connectivity
#   2. Create SQL Server login (if using SQL Authentication)
#   3. Create database if it doesn't exist
#   4. Grant login access to the database
#   5. Run RVMSService.exe --setup (applies EF Core migrations)
# ============================================================
param(
    [string]$SqlServer,
    [string]$DatabaseName,
    [string]$UseIntegrated,
    [string]$SqlUser,
    [string]$SqlPassword,
    [string]$AppPath,
    [string]$IsUpgrade = "no",
    [string]$HttpPort = "5050"   # <-- NEW PARAMETER
)

$ErrorActionPreference = "Stop"
$logFile = Join-Path $AppPath "Logs\setup.log"

function Write-Log {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $entry = "$timestamp - $Message"
    Write-Host $entry
    Add-Content -Path $logFile -Value $entry -ErrorAction SilentlyContinue
}

# Ensure Logs folder exists
New-Item -ItemType Directory -Path (Join-Path $AppPath "Logs") -Force | Out-Null

Write-Log "=== RVMS Database Setup Starting ==="
Write-Log "SQL Server: $SqlServer"
Write-Log "Database: $DatabaseName"
Write-Log "Integrated Security: $UseIntegrated"
Write-Log "Is Upgrade: $IsUpgrade"

# ── Step 0: Update Settings.json — ONLY on fresh install ──
if ($IsUpgrade -eq "yes") {
    Write-Log "Upgrade detected — skipping Settings.json modification (existing config preserved)."
} else {
    Write-Log "Fresh install — updating Settings.json with target database configuration..."
    $settingsPath = Join-Path $AppPath "Settings\Settings.json"
    if (Test-Path $settingsPath) {
        try {
            $jsonContent = Get-Content -Path $settingsPath -Raw
            $jsonObj = ConvertFrom-Json -InputObject $jsonContent

            # Apply the user database overrides natively to the JSON object
            $jsonObj.Server = $SqlServer
            $jsonObj.Database = $DatabaseName
             # --- UPDATE PORT HERE ---
            if (-not [string]::IsNullOrWhiteSpace($HttpPort)) {
                $jsonObj.ServerAddresshttp = "0.0.0.0:$HttpPort"
            }

            if ($UseIntegrated -eq "yes") {
                $jsonObj.IntegratedSecurity = "true"
            } else {
                $jsonObj.IntegratedSecurity = "false"
                $jsonObj.UserID = $SqlUser
                $jsonObj.Password = $SqlPassword
            }

            # Safely update the DefaultConnection fallback just in case
            $cs = ""
            if ($UseIntegrated -eq "yes") {
                $cs = "Server=$SqlServer;Database=$DatabaseName;Integrated Security=True;TrustServerCertificate=True"
            } else {
                $cs = "Server=$SqlServer;Database=$DatabaseName;User ID=$SqlUser;Password=$SqlPassword;Integrated Security=False;TrustServerCertificate=True"
            }
            if ($null -ne $jsonObj.ConnectionStrings) {
                $jsonObj.ConnectionStrings.DefaultConnection = $cs
            }

            # Convert back to JSON and overwrite the file
            $jsonObj | ConvertTo-Json -Depth 10 | Set-Content -Path $settingsPath -Encoding UTF8
            Write-Log "Settings.json securely updated."
        } catch {
            Write-Log "WARNING: Could not update Settings.json via PowerShell. $_"
        }
    } else {
        Write-Log "WARNING: Settings.json not found at '$settingsPath'."
    }
}

# ── Step 1: Test SQL Server connectivity ──
Write-Log "Testing SQL Server connectivity..."
try {
    $masterConn = New-Object System.Data.SqlClient.SqlConnection
    if ($UseIntegrated -eq "yes") {
        $masterConn.ConnectionString = "Server=$SqlServer;Database=master;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;Connection Timeout=10;"
    } else {
        $masterConn.ConnectionString = "Server=$SqlServer;Database=master;User ID=$SqlUser;Password=$SqlPassword;Integrated Security=False;Encrypt=False;TrustServerCertificate=True;Connection Timeout=10;"
    }
    $masterConn.Open()
    Write-Log "Connected to SQL Server successfully."
}
catch {
    Write-Log "ERROR: Cannot connect to SQL Server at '$SqlServer'. Ensure SQL Server is running."
    Write-Log "Details: $_"
    exit 1
}

# ── Step 2: Create SQL Login if using SQL Authentication ──
if ($UseIntegrated -eq "no") {
    Write-Log "Creating SQL login '$SqlUser'..."
    try {
        $cmd = $masterConn.CreateCommand()
        $cmd.CommandText = "SELECT COUNT(*) FROM sys.server_principals WHERE name = @name"
        $cmd.Parameters.AddWithValue("@name", $SqlUser) | Out-Null
        $loginExists = $cmd.ExecuteScalar()

        if ($loginExists -eq 0) {
            $escapedPassword = $SqlPassword.Replace("'", "''")
            $cmd2 = $masterConn.CreateCommand()
            $cmd2.CommandText = @"
                CREATE LOGIN [$SqlUser] WITH PASSWORD = N'$escapedPassword',
                DEFAULT_DATABASE = [$DatabaseName],
                CHECK_POLICY = OFF, CHECK_EXPIRATION = OFF
"@
            $cmd2.ExecuteNonQuery() | Out-Null
            Write-Log "SQL login '$SqlUser' created."
        }
        else {
            Write-Log "SQL login '$SqlUser' already exists. Skipping."
        }
    }
    catch {
        Write-Log "WARNING: Could not create SQL login. $_"
        Write-Log "You may need to create the login manually."
    }
}

# ── Step 3: Create database if it doesn't exist ──
Write-Log "Checking if database '$DatabaseName' exists..."
try {
    $cmd = $masterConn.CreateCommand()
    $cmd.CommandText = "SELECT DB_ID(@dbname)"
    $cmd.Parameters.AddWithValue("@dbname", $DatabaseName) | Out-Null
    $dbExists = $cmd.ExecuteScalar()

    if ($null -eq $dbExists -or $dbExists -is [DBNull]) {
        Write-Log "Creating database '$DatabaseName'..."
        $cmd2 = $masterConn.CreateCommand()
        $cmd2.CommandText = "CREATE DATABASE [$DatabaseName]"
        $cmd2.ExecuteNonQuery() | Out-Null
        Write-Log "Database '$DatabaseName' created."
        Start-Sleep -Seconds 3
    }
    else {
        Write-Log "Database '$DatabaseName' already exists."
    }
}
catch {
    Write-Log "WARNING: Could not create database. $_"
}

# ── Step 4: Grant login access to the database ──
if ($UseIntegrated -eq "no") {
    Write-Log "Granting '$SqlUser' access to '$DatabaseName'..."
    try {
        $dbConn = New-Object System.Data.SqlClient.SqlConnection
        $dbConn.ConnectionString = "Server=$SqlServer;Database=$DatabaseName;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
        $dbConn.Open()

        $cmd = $dbConn.CreateCommand()
        $cmd.CommandText = "SELECT COUNT(*) FROM sys.database_principals WHERE name = @name"
        $cmd.Parameters.AddWithValue("@name", $SqlUser) | Out-Null
        $userExists = $cmd.ExecuteScalar()

        if ($userExists -eq 0) {
            $cmd2 = $dbConn.CreateCommand()
            $cmd2.CommandText = @"
                CREATE USER [$SqlUser] FOR LOGIN [$SqlUser];
                ALTER ROLE [db_owner] ADD MEMBER [$SqlUser];
"@
            $cmd2.ExecuteNonQuery() | Out-Null
            Write-Log "Database user '$SqlUser' created with db_owner role."
        }
        else {
            Write-Log "Database user '$SqlUser' already exists."
        }
        $dbConn.Close()
    }
    catch {
        Write-Log "WARNING: Could not grant database access. $_"
    }
}
else {
    Write-Log "Granting NT AUTHORITY\SYSTEM access to '$DatabaseName'..."
    try {
        $dbConn = New-Object System.Data.SqlClient.SqlConnection
        $dbConn.ConnectionString = "Server=$SqlServer;Database=$DatabaseName;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
        $dbConn.Open()

        $cmd = $dbConn.CreateCommand()
        $cmd.CommandText = @"
            IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'NT AUTHORITY\SYSTEM')
                CREATE LOGIN [NT AUTHORITY\SYSTEM] FROM WINDOWS;
            IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'NT_SYSTEM')
                CREATE USER [NT_SYSTEM] FOR LOGIN [NT AUTHORITY\SYSTEM];
            IF IS_ROLEMEMBER('db_owner', 'NT_SYSTEM') = 0
                ALTER ROLE [db_owner] ADD MEMBER [NT_SYSTEM];
"@
        $cmd.ExecuteNonQuery() | Out-Null
        Write-Log "NT AUTHORITY\SYSTEM granted db_owner access."
        $dbConn.Close()
    }
    catch {
        Write-Log "WARNING: Could not grant system account access. $_"
    }
}

$masterConn.Close()

# ── Step 5: Run EF Core migrations via the app ──
Write-Log "Running EF Core migrations (applies only pending changes — existing data is preserved)..."
$exePath = Join-Path $AppPath "RVMSService.exe"
if (-not (Test-Path $exePath)) {
    Write-Log "ERROR: RVMSService.exe not found at '$exePath'"
    exit 1
}

try {
    $stdOut = Join-Path $AppPath "Logs\migration-output.log"
    $stdErr = Join-Path $AppPath "Logs\migration-error.log"

    $process = Start-Process -FilePath $exePath -ArgumentList "--setup" `
        -WorkingDirectory $AppPath -Wait -PassThru -NoNewWindow `
        -RedirectStandardOutput $stdOut `
        -RedirectStandardError $stdErr

    $outputContent = Get-Content $stdOut -ErrorAction SilentlyContinue
    if ($outputContent) {
        foreach ($line in $outputContent) { Write-Log "  [migrate] $line" }
    }

   if ($process.ExitCode -eq 0) {
        Write-Log "EF Core migrations applied successfully."
    }
    else {
        Write-Log "ERROR: Migration exited with code $($process.ExitCode)."
        $errorContent = Get-Content $stdErr -ErrorAction SilentlyContinue
        if ($errorContent) {
            foreach ($line in $errorContent) { Write-Log "  [error] $line" }
        }
        Write-Log "=== RVMS Database Setup FAILED ==="
        exit 1
    }
}
catch {
    Write-Log "ERROR: Could not run migrations. $_"
    Write-Log "=== RVMS Database Setup FAILED ==="
    exit 1
}

Write-Log "=== RVMS Database Setup Complete ==="
exit 0