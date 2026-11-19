# Kurulum
Inno Setup 6.6 veya üstü kullanılır. Ana betik: DogruTemin.iss.
Koyu arayüz, program ikonu, masaüstü kısayolu ve başlangıçta boş temiz kurulum seçeneği.
Sorumluluk metni + kabul kutusu + Tamam zorunludur. Sessiz kurulum desteklenmez.
Kalıcı kullanıcı verileri yalnız Masaüstü/DoğruTemin altında tutulur.
Normal kurulum eski verileri hash doğrulamasıyla taşır; çakışmaları Yedekler/EskiKonumlar altında korur. TEHYS kendi veri hazırlayıcısını kullanır.
Temiz kurulum yalnız bilinen veri alt klasörlerini, ayrı onaydan sonra siler. Program kaldırıldığında kullanıcı verileri korunur.
Yalnız temiz dotnet publish klasörünü paketleyin; kullanıcı veri klasörünü paketlemeyin.
Eski DoğruFatura/DoğruTemin NSIS betikleri kapatılmıştır. SETUP_HAZIRLA.ps1 ve DoğruFatura Publish-Installers.ps1 artık Inno Setup kullanır.
4.4.1 ile Windows tarafından yönlendirilmiş üst klasörler desteklenir. Veri ağacındaki gerçek bağlantılar reddedilir. Hatalar kurulumun hazırlık sayfasında yeniden denenebilir; teknik günlük %LOCALAPPDATA%\DoğruTeminKurulum\veri-hazirlama.log konumundadır.
