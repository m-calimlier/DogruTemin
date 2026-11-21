# Atlas tasarımının gerçek WPF ekranlarına uygulanması

Kullanıcının indirdiği JSON dosyasında seçilen tasarım Atlas. Tercihler: köşe/boşluk/iç boşluk20, yazı16, sol menü200, vurgu#8fb9c2, kenarlık%30, gölge0; tutar30sol; giriş40, başlık50, satır50; etiketüstte, yardımkapalı, dönüşümlüsatır ve hücreçizgileri açık, tekniközellikaçık.

- [x] Gerçek pencere ölçüleri, Atlas yan yana giriş ve tablo yerleşimi için kabul kontrolünü önce başarısız çalıştır.
- [x] Ortak Atlas kaynaklarını ve kart şablonlarını uygula; giriş, tablo, bilgi ve gezinme stillerini eşitle.
- [x] Mal/hizmet ekranını Atlas düzenine taşı; diğer modüllerin kartlarını ortak şablonla güncelle.
- [x] Tüm kontrolleri çalıştır, örnek verilerle gerçek ekran görüntülerini üret ve güncel programı aç.

Kayıt ve yedekleri koru; açık pencereyi kapatırken kaydedilmemiş bilgi kaybolmasın. PDF belge tipografisi ve hesaplamalar bu arayüz değişikliğinin parçası değildir. Normal veri kökü ve otomatik çıktı davranışı korunur.

Doğrulama: 150 test geçti. Son hizalama ve Tab sırası dahil WPF arayüz kabul kontrolü ayrıca geçti. İnceleme derlemesi0hata/0uyarı. Gerçek program Masaüstü veri kökündeki kayıtla açıldı, MAL/HİZMET ekranı seçildi; kaydedilmemiş değişiklik yok.
