#define MyAppName "RVMS Service"
#define MyAppVersion "1.5.9"
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
; Main application files
Source: "Files\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Settings.json is deployed by the wildcard above on fresh install.
; On upgrade the [Code] section backs it up before copy and restores it after,
; so user customisations are never lost.
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
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -File ""{app}\Installer\SetupDatabase.ps1"" -SqlServer ""{code:GetSqlServer}"" -DatabaseName ""{code:GetDatabaseName}"" -UseIntegrated ""{code:GetUseIntegrated}"" -SqlUser ""{code:GetSqlUser}"" -SqlPassword ""{code:GetSqlPassword}"" -AppPath ""{app}"" -IsUpgrade ""{code:GetIsUpgrade}"""; Flags: runhidden waituntilterminated; StatusMsg: "Setting up database..."

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
  UpgradeDetected: Boolean;

{ ── Helper: extract a JSON string value by key name ── }
function ExtractJsonStringValue(const Content, Key: String): String;
var
  P, Q: Integer;
  SearchKey: String;
begin
  Result := '';
  SearchKey := '"' + Key + '"';
  P := Pos(SearchKey, Content);
  if P = 0 then Exit;
  P := P + Length(SearchKey);
  while (P <= Length(Content)) and (Content[P] <> ':') do
    P := P + 1;
  if P > Length(Content) then Exit;
  P := P + 1;
  while (P <= Length(Content)) and ((Content[P] = ' ') or (Content[P] = #9) or (Content[P] = #10) or (Content[P] = #13)) do
    P := P + 1;
  if (P > Length(Content)) or (Content[P] <> '"') then Exit;
  P := P + 1;
  Q := P;
  while (Q <= Length(Content)) and (Content[Q] <> '"') do
    Q := Q + 1;
  Result := Copy(Content, P, Q - P);
end;

{ ── Read existing Settings.json into wizard-page values so every
     Get* function returns the correct value on upgrade ── }
procedure ReadExistingSettings;
var
  SettingsPath: String;
  AnsiContent: AnsiString;
  Content, Val, ServerAddr: String;
  ColonPos: Integer;
begin
  SettingsPath := ExpandConstant('{app}\Settings\Settings.json');
  if not FileExists(SettingsPath) then Exit;
  if not LoadStringFromFile(SettingsPath, AnsiContent) then Exit;
  Content := String(AnsiContent);

  Val := ExtractJsonStringValue(Content, 'Server');
  if Val <> '' then ConfigPage.Values[0] := Val;

  Val := ExtractJsonStringValue(Content, 'Database');
  if Val <> '' then ConfigPage.Values[1] := Val;

  ServerAddr := ExtractJsonStringValue(Content, 'ServerAddresshttp');
  if ServerAddr <> '' then
  begin
    ColonPos := Pos(':', ServerAddr);
    if ColonPos > 0 then
      ConfigPage.Values[2] := Copy(ServerAddr, ColonPos + 1, Length(ServerAddr) - ColonPos);
  end;

  Val := ExtractJsonStringValue(Content, 'IntegratedSecurity');
  if Lowercase(Val) = 'true' then
    ConfigPage.Values[3] := 'yes'
  else if Lowercase(Val) = 'false' then
    ConfigPage.Values[3] := 'no';

  Val := ExtractJsonStringValue(Content, 'UserID');
  if Val <> '' then AuthPage.Values[0] := Val;

  Val := ExtractJsonStringValue(Content, 'Password');
  if Val <> '' then AuthPage.Values[1] := Val;
end;

procedure InitializeWizard;
begin
  UpgradeDetected := False;

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
  { On upgrade skip both configuration pages — existing settings are preserved }
  if (PageID = ConfigPage.ID) or (PageID = AuthPage.ID) then
  begin
    if FileExists(ExpandConstant('{app}\Settings\Settings.json')) then
    begin
      Result := True;
      Exit;
    end;
  end;
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

function GetIsUpgrade(Param: String): String;
begin
  if UpgradeDetected then
    Result := 'yes'
  else
    Result := 'no';
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

{ ── Test SQL Server connection before proceeding ── }
function TestSqlConnection(const Server, UseIntegrated, SqlUser, SqlPassword: String): Boolean;
var
  TempScript, TempResult: String;
  PSScript: String;
  ResultCode: Integer;
  AnsiResult: AnsiString;
  ResultContent: String;
begin
  Result := False;
  TempScript := ExpandConstant('{tmp}\RVMSConnTest.ps1');
  TempResult := ExpandConstant('{tmp}\RVMSConnTest.txt');

  if Lowercase(UseIntegrated) = 'yes' then
    PSScript :=
      '$cs = "Server=' + Server + ';Database=master;Integrated Security=True;' +
      'Encrypt=False;TrustServerCertificate=True;Connection Timeout=8;"'
  else
    PSScript :=
      '$cs = "Server=' + Server + ';Database=master;User ID=' + SqlUser +
      ';Password=' + SqlPassword + ';Integrated Security=False;' +
      'Encrypt=False;TrustServerCertificate=True;Connection Timeout=8;"';

  PSScript := PSScript + #13#10 +
    'try {' + #13#10 +
    '  $conn = New-Object System.Data.SqlClient.SqlConnection($cs)' + #13#10 +
    '  $conn.Open()' + #13#10 +
    '  $conn.Close()' + #13#10 +
    '  Set-Content -Path "' + TempResult + '" -Value "OK"' + #13#10 +
    '} catch {' + #13#10 +
    '  Set-Content -Path "' + TempResult + '" -Value ("FAIL: " + $_.Exception.Message)' + #13#10 +
    '}';

  SaveStringToFile(TempScript, AnsiString(PSScript), False);

  Exec('powershell.exe',
    '-ExecutionPolicy Bypass -NonInteractive -WindowStyle Hidden -File "' + TempScript + '"',
    '', SW_HIDE, ewWaitUntilTerminated, ResultCode);

  if FileExists(TempResult) then
  begin
    LoadStringFromFile(TempResult, AnsiResult);
    ResultContent := Trim(String(AnsiResult));
    if Copy(ResultContent, 1, 2) = 'OK' then
      Result := True
    else
      MsgBox(
        'Cannot connect to SQL Server "' + Server + '".' + #13#10 + #13#10 +
        ResultContent + #13#10 + #13#10 +
        'Please verify the server name and credentials, then try again.',
        mbError, MB_OK);
  end
  else
    MsgBox('Connection test failed: the test script did not produce a result.' + #13#10 +
      'Ensure PowerShell is available and try again.', mbError, MB_OK);

  DeleteFile(TempScript);
  DeleteFile(TempResult);
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;

  { ── Test on ConfigPage when using Integrated Security ── }
  if CurPageID = ConfigPage.ID then
  begin
    if Lowercase(ConfigPage.Values[3]) = 'yes' then
    begin
      Result := TestSqlConnection(
        ConfigPage.Values[0], 'yes', '', '');
    end;
    { SQL Auth: defer test to AuthPage so credentials are filled first }
  end

  { ── Test on AuthPage when using SQL Authentication ── }
  else if CurPageID = AuthPage.ID then
  begin
    Result := TestSqlConnection(
      ConfigPage.Values[0], 'no',
      AuthPage.Values[0], AuthPage.Values[1]);
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  SettingsPath, BackupPath: String;
begin
  SettingsPath := ExpandConstant('{app}\Settings\Settings.json');
  BackupPath  := SettingsPath + '.bak';

  if CurStep = ssInstall then
  begin
    { Detect upgrade BEFORE files are copied }
    UpgradeDetected := FileExists(SettingsPath);
    if UpgradeDetected then
    begin
      { Populate wizard-page values from existing config so every
        Get* function (used by [Run]) returns the correct value }
      ReadExistingSettings;
      { Back up the user's Settings.json before the file-copy overwrites it }
      FileCopy(SettingsPath, BackupPath, False);
    end;
  end;

  if CurStep = ssPostInstall then
  begin
    if UpgradeDetected then
    begin
      { Restore the original Settings.json — user customisations preserved }
      if FileExists(BackupPath) then
      begin
        DeleteFile(SettingsPath);
        RenameFile(BackupPath, SettingsPath);
      end;
    end
    else
      { Fresh install — apply wizard values to the template Settings.json }
      UpdateSettingsFile;
  end;
end;