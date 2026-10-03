# URP Toolkit 2.0.0 yeniden test raporu

3 Ekim 2026 — Unity 6000.3.15f1 / Windows Editor.

Test edilen commit: `bcdb5f30e62809c3f3265c2395bea149ca503e8b`. Paket UPM üzerinden indirilerek ortak Event Bus ve önceki sekiz paketle birlikte projeye kuruldu; kurulu bırakıldı.

**Sonuç: 9/9 Toolkit Play Mode testi ve 67/67 mevcut EditMode testi geçti.** Son Console kontrolünde hata veya uyarı yok.

## Uyumluluk

Eski Toolkit'in `EventSystem.EventBus` ve `EventSystem.IEventBus` kopyaları yeni sürümde yok. Her iki tipin yalnızca ortak `EventSystem` assembly'si tarafından yayımlandığı Play Mode'da doğrulandı. Ortak `EventSystem.IEventBus` ve Toolkit'in `IPostProcessEventBus` arayüzünü aynı anda kullanan gerçek tüketici kodu hatasız derlendi; önceki CS0433 çakışması giderilmiş.

Yeni Toolkit `Game.Core.PostProcessing.IPostProcessEventBus` sözleşmesini kullanır. Ortak Event Bus bu arayüzü doğrudan uygulamaz. Event tabanlı kullanımda `Subscribe`, `Unsubscribe` ve `Publish` çağrılarını ortak `IEventBus` nesnesine aktaran bir adaptör VContainer'a `IPostProcessEventBus` olarak kaydedilmelidir. Bu bağlantı test adaptörüyle doğrulandı; projeye kalıcı runtime adaptörü eklenmedi.

Yeni runtime assembly VContainer, URP ve DOTween kullanıyor; UniTask veya ortak Event Bus assembly'sine doğrudan referans vermiyor. Installer'ın Toolkit önkoşulları VContainer ve tamamlanmış DOTween setup'ı olarak güncellendi.

## Play Mode testleri

| Senaryo | Sonuç |
| --- | --- |
| Ortak Event Bus tiplerinin tek assembly'den gelmesi | Geçti |
| Bloom Set/Reset ve başlangıç override durumunun geri yüklenmesi | Geçti |
| Gerçek ortak Event Bus'tan adaptör üzerinden Toolkit'e request iletimi | Geçti |
| VContainer adaptör enjeksiyonu, tekrar enjeksiyonda çift abonelik olmaması | Geçti |
| Disable'da abonelik kaldırma, Enable'da tek abonelik ve event iletimi | Geçti |
| Bloom Pulse'un tepe değerine ulaşması ve başlangıç değerine dönmesi | Geçti |
| Volume ağırlığı clamp ve ResetAll ile başlangıç durumuna dönüş | Geçti |
| Preset event'leriyle Volume oluşturma ve preset kaydını kaldırma | Geçti |
| İki stackable presetin birlikte kullanılması ve reset ile temizlenmesi | Geçti |

Pulse tween'i deterministik `Goto` çağrılarıyla ilerletildi. Preset testleri runtime kaydını ve Volume profil/ağırlığını kontrol eder; görsel frame değerlendirmesi değildir.

## Kanıtlar ve son durum

- [Toolkit Play Mode sonuçları](toolkit-v2-tests.json)
- [Mevcut 67 testin tekrar sonuçları](toolkit-v2-regression.json)
- [Tüketici derleme sonucu](toolkit-v2-compiler.txt)
- Tekrar kullanılabilecek test kaynakları: `Validation/ToolkitV2Tests`. Yeniden çalıştırmak için bu klasörü Assets altına kopyalayıp Unity Test Runner'da PlayMode testlerini çalıştırın.

Installer kataloğu yeni commit'e sabitlendi; eski karşılıklı Event Bus çakışma engeli kaldırıldı. Toolkit dahil dokuz paket artık aynı projede kurulu. Geçici test assembly'si Assets'ten kaldırıldı. Uzak repolar veya kurulan paketin kaynakları değiştirilmedi.

Player build, performans/yük testleri ve tüm post-processing efektlerinin görsel değerlendirmesi bu çalışmanın kapsamında değildi.
