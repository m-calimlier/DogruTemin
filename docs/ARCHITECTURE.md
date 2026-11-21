# DoğruTemin geliştirme rehberi

Program dokuz sunum modülünden oluşur. Mevcut ekranlar, hesaplama ve kayıt davranışları korunmuştur. Modüller aynı WPF uygulamasında çalışır; ayrı çalıştırılabilir programlar değildir.

| Modül | Klasör | Sorumluluk |
|---|---|---|
| Uygulama kabuğu | `DogruTemin.Wpf/Features/Shell` | Gezinme, pencere, klavye, ortak tema, erişilebilirlik, tablolar, kurum imza kartı |
| Aktif işlem | `DogruTemin.Wpf/Features/Workspace` | Form-veri eşleme, kayıt, kaydedilmemiş değişiklikler, ana ekran özeti |
| Mal ve firma kataloğu | `DogruTemin.Wpf/Features/Catalog` | Ürün ve firma ekleme, düzenleme, silme, giriş denetimi |
| Teklifler | `DogruTemin.Wpf/Features/Offers` | Firma seçimi, teklif düzenleyicisi, fiyatların kalıcı kaydı |
| Belgeler ve tarihler | `DogruTemin.Wpf/Features/Documents` | Tarih akışı, onay belgesi, altı belge şablonu, PDF, canlı/büyük önizleme |
| Arşiv | `DogruTemin.Wpf/Features/Archive` | Listeleme, arama, sıralama, satır işlemleri, arşiv kaydı açma/silme |
| Kurum ve ayarlar | `DogruTemin.Wpf/Features/Institution` | Kurum, komisyon ve sunan bilgileri, pencere ayarı |
| İstatistik | `DogruTemin.Wpf/Features/Statistics` | Dönem hesapları, halka grafik, yönetim raporu PDF/Excel |
| Güvenlik | `DogruTemin.Wpf/Features/Safety` | Bütünlük, yedekleme, kurtarma, tanılama, klasör erişimi |

## Ortak sözleşmeler

`MainWindow.xaml` yalnız kabuğu ve sayfa yerleşimini içerir. `MainWindow.xaml.cs` bağımlılıkları oluşturur ve kabuk olaylarını yönlendirir; buraya iş kuralları eklenmez. On bağımsız `*Page.xaml` görünümü kendi `UserControl` koduyla olayları yönlendirir. Teklif ve firma kartları ortak ekranı paylaşır.

`FeatureModule` ortak modül tabanıdır. `WorkspaceContext` dokuz modülü bir kez oluşturur; kayıt deposu, güvenlik servisi, oturum ve ortak ekran durumu açık özelliklerle sunulur. `WorkspaceState` bağlı koleksiyonları ve yükleme/kapatma durumunu, Core içindeki `ProcurementSession` kayıt kimliği, revizyon ve değişiklik bayraklarını taşır. Modüller `MainWindow` sınıfını miras almaz ve ona bağımlı değildir.

Görünümler bağımsız WPF ad kapsamları oluşturur. `WorkspaceElements` ve `Context.View<T>("kontrolAdı")` kapalı sayfalardaki alanları da çözer. Kontrol bulunana kadar önbelleğe alınmaz; oluşturulduktan sonra aynı örnek kullanılır. Pencerenin `FindName` köprüsü mevcut ortak arayüz araçlarına aynı çözümlemeyi sağlar.

Olay akışı: **XAML → sayfa olay işleyicisi → WorkspaceActionRouter → WorkspaceContext.HandleAction → ilgili modül**. Eylem adlarında `nameof` kullanılır; yönlendirme çalışma zamanında reflection kullanmaz. Yeni eylem eklerken hem sayfa işleyicisini hem bağlamdaki açık yönlendirmeyi ekleyin. Bilinmeyen eylem sessizce yutulmaz.

Modüller arası çağrılar `Context.Offers`, `Context.Workspace` gibi açık özelliklerden yapılır. Yeni genel servis bulucu, `dynamic`, pencere sınıfı mirası veya ikinci bir paralel veri listesi eklemeyin. UI doğrulaması sunum modülünde, hesap/veri kuralları Core içinde kalır.

## Core ve çıktı düzeni

Core altında Catalog, Institution, Calculation, Validation, Documents, Records, Archive ve Session sorumlulukları `Modules` altında; sayı/metin yardımcıları `Shared` altındadır. Kayıt iş akışları `IProcurementRecordStore` sözleşmesini tüketir. Disk uygulaması `StorageService`, arşiv özetleri `ArchiveQueryService` içindedir.

Model ve servislerin mevcut genel C# ad alanları ve JSON alan adları korunmuştur. Böylece dosyaları ilgili modüle taşımak kalıcı kayıt sözleşmesini değiştirmez. Eski kayıt sürümlerini okuyan `SchemaMigrationService`, kurtarma, bütünlük ve güvenli yedek kodları kullanılmayan kod değildir.

PDF şablonları `Features/Documents/Services/OfficialDocumentService.{ApproximateRequest,ApproximateCost,FirmOffer,Approval,MarketResearch,Inspection}.cs` dosyalarındadır. Ortak yazı, hücre, imza ve sayfaya sığdırma düzeni `OfficialDocumentService.Layout.cs`; yazı tipi/punto sabitleri ve belge seçimi `OfficialDocumentService.cs` içindedir. PDF yazıcısı `PdfVectorWriter`, dosya ve kaynak kaydı `SavedDocumentService` sorumluluğundadır. Önizleme ve dışa aktarma aynı belge üreticisini kullanır.

Ortak WPF stili `Features/Shell/Themes/ApplicationTheme.xaml` dosyasındadır ve `App.xaml` tarafından birleştirilir. Bir karta özel görünüm kendi sayfasında; tüm programı ilgilendiren stil bu sözlükte düzenlenir.

## Veri güvenliği kuralları

Normal kök Windows Masaüstü özel klasörünün altındaki `DoğruTemin` klasörüdür; `Veri`, `Yedekler`, `Çıktılar` kullanılır. Çıktı konumu sorulmaz; tarih ve işlem alt klasörleri otomatik açılır. Testler geçici kök kullanır. Bu refaktör kullanıcı kaydı veya yedek silmez; mevcut JSON şeması değişmez.

## Temizlenen kod

- MainWindow'a yayılmış işleyiş ve sonradan menü oluşturan `OutputNavigationEnhancement` kaldırıldı; ÇIKTILAR düğmesi doğrudan XAML'dedir.
- Kaldırılmış arşiv düğmelerinin filtre temizleme, kopyalama, dışa aktarma, yedek dışa aktarma, geri alma ve yenileme olayları ile yalnız bunların kullandığı yardımcılar kaldırıldı. Kayıt/yedek servislerinin veri koruma yetenekleri tutuldu.
- Boş firma seçim olayı ve kullanılmayan eski komisyon gezinme olayı kaldırıldı.
- Kullanılmayan `OutputDocument` seçim modeli, eski `PdfImageWriter` uyumluluk sarmalayıcısı ve çağrılmayan `WorkspacePrinting` kaldırıldı.
- MainWindow'daki kullanılmayan tasarım boyutu sabitleri ve eski pencere durum alanları kaldırıldı; editör durumu kendi modülüne taşındı.
- Tarih/çıktı sayfasında eskiden kodla gizlenen gereksiz ayraç kaynağından kaldırıldı.

Silme onayının güvenli Silinenler yedeğini anlatan metni, arayüzde artık bulunmayan eski geri-al düğmesine yönlendirme yapmayacak şekilde güncellenmiştir.

## Doğrulama

```powershell
dotnet test DogruTemin.Tests -c Release --no-restore
dotnet build DogruTemin.Wpf -c Release --no-restore
git diff --check
```

Regresyonlar kayıt üzerine yazma, arşiv değişikliklerinin kaydı, kaydedilmemiş editörler, kaynak bütünlüğü, otomatik çıktı yolları, altı PDF, ortak tipografi, canlı önizleme, çoklu pencere boyutu, sayfa olay yönlendirme ve dokuz modül sınırını kapsar. Değişiklik sonrası güncel program inceleme için açılır; önceki penceredeki kaydedilmemiş veriler korunur.
`DocumentPreviewWindow` gibi taşınabilir XAML pencerelerinde uygulama simgesi `pack://application:,,,/DogruTemin;component/Assets/AppIcon.ico` mutlak kaynak URI ile kullanılır; sayfanın yeni klasörüne göre değişen göreli simge yolu kullanılmaz.
