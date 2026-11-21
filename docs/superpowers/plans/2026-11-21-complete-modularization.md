# Tam Modülerleştirme Uygulama Planı

**Amaç:** Mevcut davranışı koruyarak pencereye bağlı tüm işleyişi sorumluluk sahibi modüllere, ekranları bağımsız XAML görünümlerine taşımak; kullanılmayan arayüz yollarını temizlemek.
**Mimari:** Shell, Workspace, Catalog, Offers, Documents, Archive, Institution, Statistics, Safety olmak üzere dokuz sunum modülü. WorkspaceContext açık bağımlılıkları, ortak oturumu ve görünüm çözümlemesini sağlar. MainWindow yalnız oluşturma, yaşam döngüsü ve olay yönlendirme görevlerini taşır. Core ve çıktı servisleri kendi modül klasörlerinde kalır; kalıcı JSON sözleşmeleri korunur.
**Teknoloji:** Mevcut .NET 8 / WPF; yeni çalışma zamanı paketi yok.

- [x] Mimari sınırlar ve olay yönlendirme için regresyon testleri ekle; önce başarısızlığı gör.
- [x] Dokuz bağımsız modülü ve ortak bağlamı oluştur; ekran işleyişini MainWindow'dan çıkar.
- [x] Görünümleri UserControl dosyalarına, tema kaynaklarını sözlüğe ayır; ad alanı ve canlı veri bağlarını koru.
- [x] Core ve platform servislerini sorumluluk klasörlerinde topla; eski kullanılmayan olayları ve PDF uyumluluk sarmalayıcısını kaldır.
- [x] Tüm testleri ve farklı pencere boyutlarını doğrula; belge önizleme ve kayıt/arşiv akışını denetle.
- [x] Mimari geliştirme rehberini ve temizlenen kod listesini yaz; güncel uygulamayı aç.

**Korunacaklar:** Masaüstü/DoğruTemin veri kökü, veri/yedek/çıktı klasörleri, kayıt yeri sormadan dışa aktarma, mevcut kayıt ve yedekler, şema geçişleri, açık penceredeki kaydedilmemiş veriler.
**Kararlar:** Kullanıcının tüm modülerleştirmeyi tamamlama talimatı doğrultusunda aynı oturumda uygulama. Eski kayıt uyumluluğu gereksiz kod sayılmaz. Geri yükleme gibi servis yetenekleri erişilmeyen düğme koduyla karıştırılmaz.

**Doğrulama sonucu:** 150 test geçti, 0 başarısız. Son inceleme derlemesi 0 hata / 0 uyarı. On ekranın güncel örnek görüntüleri ve büyük önizleme üretildi. Git diff boşluk kontrolü geçti. Gerçek uygulama Masaüstü veri kökündeki mevcut örnek kayıtla açıldı.

**İnceleme notu:** Sayfa ad kapsamları, kayıt ve editör bayrakları, kullanıcı veri kökü, altı PDF kaynağı ve simge kaynak URI denetlendi. Önizleme penceresinin taşınmasından doğan göreli kaynak sorunu testte yakalandı; derleme adı içeren kaynak URI ile düzeltildi ve tüm testler tekrar geçti. Geçici dönüşüm araçları geri dönüşüme gönderildi.
