#ifndef AppVersion
 #define AppVersion "4.4.0"
#endif
#ifndef PayloadDir
 #define PayloadDir "..\dist\payload"
#endif
#define DataFolderName "DoğruTemin"
[Setup]
AppId={{4B5374E9-9DB8-4B4F-93D6-57B0BA5704C4}
AppName=DoğruTemin
AppVersion={#AppVersion}
VersionInfoVersion={#AppVersion}.0
AppPublisher=Murat ÇALIMLIER
AppPublisherURL=https://github.com/m-calimlier/DogruTemin
DefaultDirName={localappdata}\Programs\DoğruTemin
DefaultGroupName=DoğruTemin
PrivilegesRequired=lowest
DisableProgramGroupPage=yes
DisableWelcomePage=no
UsePreviousTasks=no
OutputDir=..\dist\installer
OutputBaseFilename=DogruTemin-Kurulum-{#AppVersion}
SetupIconFile=..\DogruTemin.Wpf\Assets\AppIcon.ico
UninstallDisplayIcon={app}\DogruTemin.exe
WizardStyle=modern dark polar includetitlebar hidebevels
WizardBackColor=#10263d
WizardSizePercent=115,115
WizardImageFile=Assets\setup-sidebar.png
WizardSmallImageFile=Assets\setup-mark.png
WizardImageBackColor=#10263d
WizardSmallImageBackColor=#10263d
WizardImageStretch=yes
ShowLanguageDialog=no
#ifdef FastBuild
Compression=zip/1
SolidCompression=no
#else
Compression=lzma2
SolidCompression=yes
#endif
CloseApplications=yes
CloseApplicationsFilter=DogruTemin.exe
RestartApplications=no
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"
[Tasks]
Name: "desktopicon"; Description: "Masaüstü kısayolu oluştur"; GroupDescription: "Kısayollar:"
[Files]
; Payload must be freshly published application binaries, never a user workspace.
Source: "{#PayloadDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "Veri\*,Veriler\*,Yedekler\*,Ciktilar\*,Çıktılar\*,PDF\*,Loglar\*,InitialData\*,distribution.profile,*.pdb,*.log"
[Icons]
Name: "{userprograms}\DoğruTemin"; Filename: "{app}\DogruTemin.exe"
Name: "{userprograms}\DoğruTemin Kaldır"; Filename: "{uninstallexe}"
Name: "{userdesktop}\DoğruTemin"; Filename: "{app}\DogruTemin.exe"; Tasks: desktopicon
[Run]
Filename: "{app}\DogruTemin.exe"; Description: "DoğruTemin programını aç"; Flags: nowait postinstall skipifsilent
[Messages]
SetupWindowTitle=DoğruTemin · Kurulum
WelcomeLabel1=DoğruTemin programına hoş geldiniz
WelcomeLabel2=Tüm kayıtlar, ayarlar, belgeler, arşivler ve yedekler masaüstündeki "DoğruTemin" klasöründe tutulur.%n%nNormal kurulum mevcut verileri korur. Temiz kurulum seçeneği programın veri alt klasörlerini siler ve ayrıca onay gerektirir.%n%nSürüm {#AppVersion} · Windows x64
FinishedHeadingLabel=DoğruTemin kullanıma hazır
FinishedLabel=Kurulum tamamlandı.%n%nProgramı Başlat menüsünden veya masaüstü kısayolundan açabilirsiniz.%n%nCreated by Murat ÇALIMLIER
#include "CommonWizard.iss"