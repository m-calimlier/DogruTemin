# DoğruTemin 4.4.1

- Dosya doğrulaması PowerShell Get-FileHash modülüne bağımlı olmadan SHA-256 ile yapılır.
- Yönlendirilmiş Masaüstü ve Belgeler klasörlerinde veri hazırlama kontrolü düzeltildi.
- Bulut yer tutucuları ile gerçek dizin bağlantıları ayrı değerlendirilir.
- Veri klasörünün içindeki dizin bağlantılarının izlenmesi ve temizlenmesi engellenir.
- Veri hazırlama işlemi kurulum öncesinde kontrol edilir. Hata durumunda kurulum ekranından yeniden denenebilir.
- Veri hazırlama hatalarının teknik ayrıntıları %LOCALAPPDATA%\DoğruTeminKurulum\veri-hazirlama.log dosyasına yazılır.
- Türkçe ve köşeli parantez içeren yollar, yönlendirilmiş Masaüstü, kilitli klasörler, eski kayıt geçişi ve temiz kurulum için Windows PowerShell 5.1 testleri eklendi.

Windows x64 kurulum paketi Authenticode imzası içermez. SHA-256 doğrulama dosyası pakete eklenir.
