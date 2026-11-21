# Modülerleşme Başlangıcı ve Kurum Kartı Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Referans sol alt kartı DoğruTemin adıyla uygulamak; oturum, kayıt erişimi ve arşiv listeleme sorumluluklarını ekrandan ayırmak.
**Architecture:** Core/Wpf/Tests korunur. Core/Modules altında Session, Records, Archive sınırları oluşturulur. WPF yalnız biçimlendirme ve kullanıcı etkileşimini yönetir.
**Tech Stack:** C#, .NET 8, WPF, JSON, xUnit.

- [x] Core/Modules/Session/ProcurementSession: kayıt kimliği, revizyon, editör değişiklikleri, arşiv düzenleme durumu; saf birim testleri.
- [x] Core/Modules/Records/IProcurementRecordStore: kayıt iş akışlarının somut dosya servisinden ayrılması; StorageService uyarlaması, mevcut sözleşme testleri.
- [x] Core/Modules/Archive/ArchiveQueryService: listeleme, hesaplanan özetler ve bozuk kayıt uyarıları; bellek içi testler.
- [x] WPF MainWindow alanlarını oturuma, arşiv satırı üretimini listeleme servisine bağla; kayıt ve kaynak bütünlüğü davranışını koru.
- [x] WPF/Features/Shell/InstitutionSignatureCard: referansın kurum satırı, altın simge, gövde, ayraç, Created by satırı; uygulama adı kullanıcı seçimiyle DoğruTemin.
- [x] Tüm regresyonları çalıştır, 1920x1080 ve küçük ekranları görsel olarak doğrula, güncel programı aç.

Kapsam: dokuz modüllü yol haritasının ilk işlevsel adımıdır. Diğer ekranların ViewModel'e taşınması sonraki aşamalardır. JSON biçimi, kullanıcı veri kökü ve otomatik çıktı yolu değişmez.
