#define MyAppName "RVMS Service"
#define MyAppVersion "1.0.4"
#define MyAppPublisher "Total Optima Solusi"
#define MyAppExeName "RVMSService.exe"
#define ServiceName "RVMSService"

[Setup]
AppId={{B3F7E2A1-9C4D-4E8B-A1D6-7F2E3B4C5D6E}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=RVMSServiceSetup_{#MyAppVersion}
Compression=lzma2
SolidCompression=yes
PrivilegesRequired=admin
WizardStyle=modern
DisableWelcomePage=no
DisableDirPage=no
DisableReadyPage=yes

[Files]
; Single source — use ONLY the publish output folder
Source: "Files\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Always overwrite Settings.json so StringChangeEx patterns match
Source: "Files\Settings\Settings.json"; DestDir: "{app}\Settings"; Flags: ignoreversion
Source: "SetupDatabase.ps1"; DestDir: "{app}\Installer"; Flags: ignoreversion

[Dirs]
Name: "{app}\Logs"; Permissions: everyone-full
Name: "{app}\Photos"; Permissions: everyone-full
Name: "{app}\Settings"; Permissions: everyone-full
Name: "{app}\Installer"

[Run]
; --- Stop & remove existing service (safe on fresh install too) ---
Filename: "sc.exe"; Parameters: "stop {#ServiceName}"; Flags: runhidden waituntilterminated; StatusMsg: "Stopping existing service..."
Filename: "sc.exe"; Parameters: "delete {#ServiceName}"; Flags: runhidden waituntilterminated; StatusMsg: "Removing old service..."

; --- Set file permissions ---
Filename: "icacls.exe"; Parameters: """{app}\Settings\Settings.json"" /grant *S-1-5-32-545:(M)"; Flags: runhidden waituntilterminated; StatusMsg: "Setting file permissions..."

; --- Database setup (create DB + login + migrations) ---
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -File ""{app}\Installer\SetupDatabase.ps1"" -SqlServer ""{code:GetSqlServer}"" -DatabaseName ""{code:GetDatabaseName}"" -UseIntegrated ""{code:GetUseIntegrated}"" -SqlUser ""{code:GetSqlUser}"" -SqlPassword ""{code:GetSqlPassword}"" -AppPath ""{app}"""; Flags: runhidden waituntilterminated; StatusMsg: "Setting up database..."

; --- Firewall rule ---
Filename: "netsh.exe"; Parameters: "advfirewall firewall delete rule name=""{#MyAppName}"""; Flags: runhidden waituntilterminated
Filename: "netsh.exe"; Parameters: "advfirewall firewall add rule name=""{#MyAppName}"" dir=in action=allow protocol=TCP localport={code:GetHttpPort}"; Flags: runhidden waituntilterminated; StatusMsg: "Configuring firewall..."

; --- Install and start Windows Service ---
Filename: "sc.exe"; Parameters: "create {#ServiceName} binPath=""{app}\{#MyAppExeName}"" start=auto DisplayName=""{#MyAppName}"""; Flags: runhidden waituntilterminated; StatusMsg: "Installing service..."
Filename: "sc.exe"; Parameters: "description {#ServiceName} ""RVMS Visitor Management Service API"""; Flags: runhidden waituntilterminated
Filename: "sc.exe"; Parameters: "failure {#ServiceName} reset=86400 actions=restart/60000/restart/60000/restart/60000"; Flags: runhidden waituntilterminated
Filename: "sc.exe"; Parameters: "start {#ServiceName}"; Flags: runhidden waituntilterminated; StatusMsg: "Starting service..."

[UninstallRun]
Filename: "sc.exe"; Parameters: "stop {#ServiceName}"; Flags: runhidden waituntilterminated
Filename: "sc.exe"; Parameters: "delete {#ServiceName}"; Flags: runhidden waituntilterminated
Filename: "netsh.exe"; Parameters: "advfirewall firewall delete rule name=""{#MyAppName}"""; Flags: runhidden waituntilterminated

[Code]
var
  ConfigPage: TInputQueryWizardPage;
  AuthPage: TInputQueryWizardPage;

procedure InitializeWizard;
begin
  ConfigPage := CreateInputQueryPage(wpSelectDir,
    'Database Configuration',
    'Configure the SQL Server connection',
    'Enter the database server, database name, and service port:');
  ConfigPage.Add('SQL Server:', False);
  ConfigPage.Add('Database Name:', False);
  ConfigPage.Add('HTTP Port:', False);
  ConfigPage.Add('Use Integrated Security (yes/no):', False);
  ConfigPage.Values[0] := 'localhost\SQLEXPRESS';
  ConfigPage.Values[1] := 'RVMS';
  ConfigPage.Values[2] := '5050';
  ConfigPage.Values[3] := 'yes';

  AuthPage := CreateInputQueryPage(ConfigPage.ID,
    'SQL Server Authentication',
    'Enter SQL Server credentials (a new login will be created automatically)',
    'These credentials will be created in SQL Server for the service:');
  AuthPage.Add('SQL Username:', False);
  AuthPage.Add('SQL Password:', True);
  AuthPage.Values[0] := 'RVMSUser';
  AuthPage.Values[1] := '';
end;

function ShouldSkipPage(PageID: Integer): Boolean;
begin
  Result := False;
  if PageID = AuthPage.ID then
    Result := (Lowercase(ConfigPage.Values[3]) = 'yes');
end;

function GetSqlServer(Param: String): String;
begin
  Result := ConfigPage.Values[0];
end;

function GetDatabaseName(Param: String): String;
begin
  Result := ConfigPage.Values[1];
end;

function GetHttpPort(Param: String): String;
begin
  Result := ConfigPage.Values[2];
end;

function GetUseIntegrated(Param: String): String;
begin
  if Lowercase(ConfigPage.Values[3]) = 'yes' then
    Result := 'yes'
  else
    Result := 'no';
end;

function GetSqlUser(Param: String): String;
begin
  Result := AuthPage.Values[0];
end;

function GetSqlPassword(Param: String): String;
begin
  Result := AuthPage.Values[1];
end;

procedure UpdateSettingsFile;
var
  SettingsPath: String;
  AnsiContent: AnsiString;
  Content: String;
begin
  SettingsPath := ExpandConstant('{app}\Settings\Settings.json');
  if not FileExists(SettingsPath) then Exit;
  if not LoadStringFromFile(SettingsPath, AnsiContent) then Exit;
  Content := String(AnsiContent);

  { Update server }
  StringChangeEx(Content, '"Server": "ASUSZENLAPTOP"',
    '"Server": "' + ConfigPage.Values[0] + '"', True);

  { Update database }
  StringChangeEx(Content, '"Database": "RVMS"',
    '"Database": "' + ConfigPage.Values[1] + '"', True);

  { Update port }
  StringChangeEx(Content, '"ServerAddresshttp": "0.0.0.0:5050"',
    '"ServerAddresshttp": "0.0.0.0:' + ConfigPage.Values[2] + '"', True);

  { Update the DefaultConnection string }
  StringChangeEx(Content, 'Server=Surface3\\SQLEXPRESS',
    'Server=' + ConfigPage.Values[0], True);
  StringChangeEx(Content, 'Database=RVMS',
    'Database=' + ConfigPage.Values[1], True);

  { Update auth mode and credentials }
  if Lowercase(ConfigPage.Values[3]) <> 'yes' then
  begin
    StringChangeEx(Content, '"IntegratedSecurity": "true"',
      '"IntegratedSecurity": "false"', True);
    StringChangeEx(Content, '"UserID": ""',
      '"UserID": "' + AuthPage.Values[0] + '"', True);
    StringChangeEx(Content, '"SqlPassword": ""',
      '"SqlPassword": "' + AuthPage.Values[1] + '"', True);
    StringChangeEx(Content, 'Integrated Security=True',
      'Integrated Security=False;User ID=' + AuthPage.Values[0] + ';Password=' + AuthPage.Values[1], True);
  end;

  SaveStringToFile(SettingsPath, AnsiString(Content), False);
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
    UpdateSettingsFile;
end;