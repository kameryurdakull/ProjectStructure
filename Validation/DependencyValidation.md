# Paket kurulum ve uyumluluk raporu

> Bu rapor Toolkit 1.0.0 ile yapılan ilk çalışmayı kaydeder. Toolkit 2.0.0 ile Event Bus çakışması giderildi; dokuz paket artık birlikte kurulu. Güncel sonuçlar: [ToolkitV2Validation.md](ToolkitV2Validation.md).

Tarih: 3 Ekim 2026. Ortam: Unity 6000.3.15f1, Windows Editor, StandaloneWindows64.

Dokuz yeni paketin tamamı Git kaynaklarından indirildi ve kuruldu. URP Toolkit önce ortak Event Bus olmadan test edildi. Ardından ortak Event Bus ile birlikte kullanımındaki tip çakışması doğrulandı ve Toolkit son kurulumdan kaldırıldı. Diğer sekiz paket, UniTask, VContainer, Event Bus ve DOTween ile kurulu bırakıldı.

## Otomatik testler

Unity Test Framework EditMode çalışması: **67 geçti, 0 başarısız, 0 atlandı**.

| Paket | Geçen test |
| --- | ---: |
| PinnedPrefab | 3 |
| TimeScaleToolbar | 7 |
| State Machine | 4 |
| Scene Loader | 13 |
| Audio System | 40 |

Testler `Packages/manifest.json` içindeki `testables` listesiyle etkinleştirildi. Ayrıntılı test adları ve sonuçları [package-tests.json](package-tests.json) dosyasında bulunur. Audio testleri havuz sınırları, eski handle güvenliği, öncelik, cooldown, 3D takip, volume/mute, müzik geçişi iptali, Event Bus abonelikleri, VContainer yaşam döngüsü ve ID üretimini kapsar. Scene Loader testleri iptal, eşzamanlı istek, başarısızlık sonrası temizlik ve sahne referanslarını kapsar.

## Ek kullanım kontrolleri

| Paket | Kontrol | Sonuç |
| --- | --- | --- |
| URP PostProcessing Toolkit | Geçici Volume ve Bloom ile sıfır süreli Set/Reset; başlangıç değerine dönüş | Geçti, ortak Event Bus olmadan |
| Component Pool | Play Mode'da prewarm, spawn/despawn callback'leri, konum, aynı nesnenin yeniden kullanılması ve çift despawn reddi | Geçti |
| Camera Effects | Play Mode'da FOV pulse ve geri dönüş, recoil, shake/recoil durdurma ve kamera önceliği geçişi | Geçti |
| Scene Loader | Play Mode'da gerçek bir sahnenin additive yüklenmesi ve kaldırılması | Geçti |
| Prefab Exporter | Materyalli prefabın bağımlılık planı, `.unitypackage` export, arşiv kontrolü ve yeniden import sonrası materyal bağlantısı | Geçti |

Camera Effects FOV tween'i deterministik olarak `Goto` ile ilerletildi. Kamera geçiş kontrolü sıfır blend süresiyle yapıldı. Bu kontroller tüm efektlerin görsel kalitesini, uzun süreli davranışını veya performansını doğrulamaz.

## Doğrulanan çakışma

URP Toolkit'in `Kamer.PostProcessService` assembly'si ve ortak Event Bus'ın `EventSystem` assembly'si aynı tam ada sahip `EventSystem.IEventBus` ve `EventSystem.EventBus` tiplerini yayımlar. Her iki assembly'ye referans veren gerçek bir C# tüketici kodu derlendiğinde **CS0433** alındı:

```text
IEventBus türü hem EventSystem hem de Kamer.PostProcessService içinde bulunur.
```

Derleyici çıktısı [conflict-compiler.txt](conflict-compiler.txt) dosyasında saklandı. Paket assembly'leri kendi başlarına derlenebildiği için boş Unity Console bu uyumluluk sorununu tek başına ortaya çıkarmaz; tüketici koduyla ayrıca doğrulandı. Installer'ın Event Bus kurulumunu Toolkit mevcutken engellediği de kontrol edildi.

Toolkit'in ortak Event Bus ile kullanılabilmesi için kendi Event Bus tiplerinin çıkarılması, runtime assembly'sinin ortak `EventSystem` assembly'sine referans vermesi ve ortak paket bağımlılığının tanımlanması gerekir. Bu çalışmada uzak repolar veya indirilen paket kaynakları değiştirilmedi.

## Son proje durumu

- URP Toolkit installer kataloğunda kalır; ortak Event Bus ile çakıştığı için mevcut projede kurulu değildir.
- Diğer sekiz yeni paket kurulu. UniTask 2.5.11, VContainer 1.19.0, Event Bus 1.0.0, DOTween 1.3.030 ve Cinemachine 3.1.7 mevcut.
- DOTween resmi installer indirmesiyle import edildi; settings, modül setup'ı, `DOTWEEN` define'ı ve `DOTween.Modules` assembly definition oluşturuldu.
- Camera Effects'in ilk indirmesinde Cinemachine önbelleği taşıma işlemi `EPERM` verdi; tekrar denemede kurulum tamamlandı.
- Son kontrol: Unity Console'da 0 error / 0 warning; derleme ve Play Mode kapalı; `EventSystem.IEventBus` yalnızca ortak Event Bus assembly'sinden geliyor.
- Geçici test scriptleri, prefab, materyal ve sahne kaldırıldı. Build scene listesi özgün SampleScene listesine geri döndürüldü.
- Paket kimlikleri ve Git adresleri [installed-packages.txt](installed-packages.txt) dosyasında. Test export'u [prefab-export.unitypackage](prefab-export.unitypackage) dosyasında korunuyor.

Bu sonuçlar Editor derlemesi, mevcut paket testleri ve belirtilen API / Play Mode kontrolleriyle sınırlıdır. Player build, farklı platformlar, uzun süreli yük testleri, görsel efekt değerlendirmesi ve sesin dinlenerek kontrolü yapılmadı.
