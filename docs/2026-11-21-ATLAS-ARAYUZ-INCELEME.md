# Atlas — gerçek uygulama incelemesi

Kullanıcının indirdiği DoğruTemin tasarım JSON dosyasındaki **Atlas** seçimi WPF arayüzüne uygulandı. Seçili değerlerin kaynak kopyası `DogruTemin.Wpf/Features/Shell/Themes/AtlasPreferences.json`; çalışan WPF kaynakları `AtlasTokens.xaml` ve `ApplicationTheme.xaml` içindedir. Program İndirilenler klasörüne erişmez.

- Kart köşesi 15 px, iç boşluğu 14 px, kart aralığı 10 px; menü 170 px.
- Giriş yazıları 14 px, tutar 28 px/sol, giriş yüksekliği 36 px, kart başlığı 44 px, tablo satırı 36 px.
- Üçüncü renk#8fb9c2; kenarlık koyu zemine %40 karışım; gölge 10 px. Gölge boş arka yüzeye uygulanır; metinler filtrelenmez.
- Tablo satırlarında dönüşümlü zemin ve hücre çizgileri; teknik özellikler ve giriş yardım metni görünür.
- Mal/hizmet ekranında solda giriş, sağda liste; dar ekranda alt alta. Tab sırası görsel sırayla eşleşir.
- Kurum ekranındaki dört kart ve diğer modüller aynı Atlas şablonunu kullanır. Teklif durumları koyu zeminde simge ve metinle gösterilir.

PDF sayfa şablonları ve hesaplama kuralları değiştirilmedi. Normal veri kökü, kullanıcı kayıtları ve yedekler korundu. Örnek ekran görüntüleri ayrı inceleme veri köküyle gerçek WPF pencerelerinden üretildi.
