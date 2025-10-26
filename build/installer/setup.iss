; Inno Setup Script for Equipment Tracker Desktop
#define MyAppName "Equipment Tracker"
#define MyAppVersion GetStringDef("AppVersion", "1.0.0")
#define MyAppPublisher "Equipment Tracker"
#define MyAppExeName "EquipmentTracker.exe"
#define BuildPayload GetStringDef("BuildPayload", "{#SourcePath}..\build\release")

[Setup]
AppId={{F0C0F0C0-F0C0-F0C0-F0C0-F0C0F0C0F0C0}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableDirPage=no
DisableProgramGroupPage=no
OutputBaseFilename=EquipmentTracker-Setup-{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop icon"; GroupDescription: "Additional icons:"; Flags: unchecked

[Files]
Source: "{#BuildPayload}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
