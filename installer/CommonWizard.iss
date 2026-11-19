#ifndef ExistingCleanTask
[Tasks]
Name: "cleandata"; Description: "Temiz kurulum — eski kayıtları, yedekleri ve çıktıları sil"; GroupDescription: "Veriler (yalnız bu Windows kullanıcısı):"; Flags: unchecked
#endif
[Files]
Source: "Prepare-DesktopData.ps1"; Flags: dontcopy
[Code]
var
  ResponsibilityPage: TWizardPage;
  ResponsibilityCheck: TNewCheckBox;
  ResponsibilityConfirm: TNewButton;
  ResponsibilityAccepted: Boolean;
  CleanConfirmed: Boolean;

procedure ResponsibilityChanged(Sender: TObject);
begin
  ResponsibilityAccepted := False;
  ResponsibilityConfirm.Enabled := ResponsibilityCheck.Checked;
  WizardForm.NextButton.Enabled := False;
end;

procedure ConfirmResponsibility(Sender: TObject);
begin
  ResponsibilityAccepted := ResponsibilityCheck.Checked;
  WizardForm.NextButton.Enabled := ResponsibilityAccepted;
end;

<event('InitializeSetup')>
function RequireInteractiveConsent: Boolean;
begin
  Result := not WizardSilent;
  if not Result then
    Log('Kurulum, sorumluluk metni için etkileşimli kullanıcı kabulü gerektirir.');
end;

<event('InitializeWizard')>
procedure CreateResponsibilityPage;
var TextBox: TNewMemo;
begin
  ResponsibilityAccepted := False;
  ResponsibilityPage := CreateCustomPage(wpWelcome,
    'Kullanım ve Çıktılara İlişkin Sorumluluk Bildirimi',
    'Devam etmek için metni okuyun, kabul kutusunu işaretleyin ve Tamam düğmesine basın.');
  TextBox := TNewMemo.Create(ResponsibilityPage);
  TextBox.Parent := ResponsibilityPage.Surface;
  TextBox.SetBounds(0, 0, ResponsibilityPage.SurfaceWidth, ResponsibilityPage.SurfaceHeight - ScaleY(72));
  TextBox.ReadOnly := True;
  TextBox.ScrollBars := ssVertical;
  TextBox.WordWrap := True;
  TextBox.Text := 'Bu program aracılığıyla oluşturulan hesaplama, rapor, belge ve diğer çıktıların doğruluğunu, eksiksizliğini ve kullanım amacına uygunluğunu kontrol etmek kullanıcının sorumluluğundadır.' + #13#10#13#10 +
    'Kullanıcı, bu çıktıları herhangi bir işlemde kullanmadan önce gerekli kontrolleri yapmak ve ilgili onayları almakla yükümlüdür.' + #13#10#13#10 +
    'Program çıktıları esas alınarak gerçekleştirilen işlemler ve alınan kararlar ile bunların sonuçlarından kullanıcı sorumludur.';
  ResponsibilityCheck := TNewCheckBox.Create(ResponsibilityPage);
  ResponsibilityCheck.Parent := ResponsibilityPage.Surface;
  ResponsibilityCheck.SetBounds(0, TextBox.Height + ScaleY(12), ResponsibilityPage.SurfaceWidth, ScaleY(24));
  ResponsibilityCheck.Caption := 'Metni okudum ve kabul ediyorum';
  ResponsibilityCheck.Checked := False;
  ResponsibilityCheck.OnClick := @ResponsibilityChanged;
  ResponsibilityConfirm := TNewButton.Create(ResponsibilityPage);
  ResponsibilityConfirm.Parent := ResponsibilityPage.Surface;
  ResponsibilityConfirm.SetBounds(0, TextBox.Height + ScaleY(40), ScaleX(100), ScaleY(28));
  ResponsibilityConfirm.Caption := 'Tamam';
  ResponsibilityConfirm.Enabled := False;
  ResponsibilityConfirm.OnClick := @ConfirmResponsibility;
end;

<event('CurPageChanged')>
procedure RefreshResponsibilityPage(CurPageID: Integer);
begin
  if CurPageID = ResponsibilityPage.ID then
    WizardForm.NextButton.Enabled := ResponsibilityAccepted and ResponsibilityCheck.Checked;
  if CurPageID = wpSelectTasks then CleanConfirmed := False;
end;

<event('NextButtonClick')>
function CheckResponsibilityAndClean(CurPageID: Integer): Boolean;
begin
  Result := True;
  if CurPageID >= ResponsibilityPage.ID then
    Result := ResponsibilityAccepted and ResponsibilityCheck.Checked;
  if (CurPageID = wpReady) and WizardIsTaskSelected('cleandata') then
  begin
    CleanConfirmed := MsgBox('TEMİZ KURULUM' + #13#10#13#10 +
      'Bu programa ait kayıtlar, ayarlar, arşivler, yedekler ve çıktılar silinecektir.' + #13#10 +
      'Veri klasörü: ' + ExpandConstant('{userdesktop}\{#DataFolderName}') + #13#10#13#10 +
      'Bu işlem geri alınamaz. Devam edilsin mi?', mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES;
    Result := Result and CleanConfirmed;
  end;
end;

<event('PrepareToInstall')>
function EnforceResponsibility(var NeedsRestart: Boolean): String;
begin
  Result := '';
  if WizardSilent or not ResponsibilityAccepted or not ResponsibilityCheck.Checked then
    Result := 'Sorumluluk metni kabul edilmeden kurulum yapılamaz.';
  if WizardIsTaskSelected('cleandata') and not CleanConfirmed then
    Result := 'Temiz kurulum için ayrıca kullanıcı onayı gereklidir.';
end;

#ifndef CustomDataPreparation
<event('PrepareToInstall')>
function PrepareDesktopData(var NeedsRestart: Boolean): String;
var ExitCode: Integer; Arguments, DiagnosticLog, PowerShellPath: String;
begin
  Result := '';
  if WizardSilent or not ResponsibilityAccepted or not ResponsibilityCheck.Checked then Exit;
  if WizardIsTaskSelected('cleandata') and not CleanConfirmed then Exit;
  DiagnosticLog := ExpandConstant('{localappdata}\DoğruTeminKurulum\veri-hazirlama.log');
  DeleteFile(DiagnosticLog);
  ExtractTemporaryFile('Prepare-DesktopData.ps1');
  Arguments := '-NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File "' +
    ExpandConstant('{tmp}\Prepare-DesktopData.ps1') + '" -Desktop "' + ExpandConstant('{userdesktop}') +
    '" -DiagnosticLog "' + DiagnosticLog + '"';
  if WizardIsTaskSelected('cleandata') then Arguments := Arguments + ' -Clean';
  PowerShellPath := ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe');
  if not Exec(PowerShellPath, Arguments, ExpandConstant('{tmp}'), SW_HIDE, ewWaitUntilTerminated, ExitCode) then
  begin
    Log(Format('Veri hazırlama aracı başlatılamadı. Windows hata kodu: %d', [ExitCode]));
    Result := Format('Veri hazırlama aracı başlatılamadı (Windows hata kodu %d).', [ExitCode]) + #13#10 +
      'Kurumunuzun teknik desteğinden Windows PowerShell erişimini kontrol etmesini isteyin.';
    Exit;
  end;
  if ExitCode <> 0 then
  begin
    Log(Format('Veri hazırlama başarısız. Çıkış kodu: %d. Günlük: %s', [ExitCode, DiagnosticLog]));
    Result := 'Veri klasörü hazırlanamadı. Açık DoğruTemin pencerelerini kapatın ve Masaüstü klasörüne dosya kaydedebildiğinizi kontrol edin.' +
      '' + #13#10#13#10 + 'Ardından Yeniden Dene düğmesine basın.';
    if FileExists(DiagnosticLog) then
      Result := Result + #13#10 + 'Teknik ayrıntılar: ' + DiagnosticLog
    else
      Result := Result + #13#10 + Format('Windows PowerShell çıkış kodu: %d. Teknik desteğiniz betik çalıştırma izinlerini kontrol edebilir.', [ExitCode]);
  end;
end;
#endif