# Package Structure

Unity projeleri için Editor içinde çalışan online dependency installer. Paketleri tek tek kurar, kurulu durumlarını gösterir ve eksik önkoşulları açıklar. Git paketleri doğrulanmış tag veya commit'lere sabitlenir; DOTween resmi Demigiant kaynağından indirilir.

## Gereksinimler

- Unity **6000.3 veya üzeri**. Geliştirme ve paket kontrolleri Unity **6000.3.15f1** ile yapıldı.
- Git kurulu ve Unity'nin erişebildiği PATH üzerinde olmalı.
- Paket indirmeleri için internet erişimi.

Installer'ı açmak için UniTask, VContainer veya DOTween'in önceden kurulu olması gerekmez. Kurulumları pencere içinden başlatabilirsiniz.

## Package Manager ile kurulum

Unity'de **Window > Package Management > Package Manager** penceresini açın. **+ > Install package from Git URL...** seçeneğine şu adresi girin:

```text
https://github.com/kameryurdakull/ProjectStructure.git?path=/Packages/com.kameryurdakull.package-structure
```

URL, repo içindeki installer paketini kurar. Geliştirme projesinin sahneleri, test raporları veya `Assets` içindeki üçüncü taraf dosyalar hedef projeye import edilmez.

Alternatif olarak projenizin `Packages/manifest.json` dosyasındaki mevcut `dependencies` nesnesine ekleyin:

```json
"com.kameryurdakull.package-structure": "https://github.com/kameryurdakull/ProjectStructure.git?path=/Packages/com.kameryurdakull.package-structure"
```

Mevcut dependencies satırlarını koruyun. Belirli bir sürüme sabitlemek için Git URL'sinin sonuna yayımlanmış bir commit SHA veya tag ekleyebilirsiniz: `#<commit-sha>`. Yukarıdaki URL repo'nun varsayılan branch'ini kullanır; repo public olduktan ve paket dosyaları GitHub'a push edildikten sonra herkese açık kurulum yapılabilir.

Önceden `.unitypackage` ile kurduysanız, Git paketini eklemeden önce eski `Assets/DependencyBundle` klasörünü kaldırın. Aynı installer kaynaklarını iki yerde tutmayın.

## Kullanım

1. **Tools > Package Structure > Dependency Installer** menüsünü açın.
2. İhtiyacınız olan paketlerin **Install** butonlarını kullanın.
3. UniTask ve VContainer'ı bunlara ihtiyaç duyan paketlerden önce kurun. Her satır eksik bağımlılıklarını gösterir.
4. DOTween import edildikten sonra **Open DOTween Setup** ile Utility Panel'i açıp **Setup DOTween...** işlemini tamamlayın. Assembly definition kullanan modüller için panelde **Create ASMDEF** seçeneğini de etkinleştirin.
5. Kurulum sonuçlarını Unity Console'dan kontrol edin.

Installer aynı anda tek bir kurulum çalıştırır. Paketler otomatik olarak topluca kurulmaz. Pencere `Installed` durumunu gösterdiğinde paket kurulu demektir; sahne yapılandırması ve servis kayıtları ilgili paketin dokümanına göre yapılır.

## Paket kataloğu

| Paket | Sabitlenen sürüm | Önkoşullar |
| --- | --- | --- |
| Unity MCP | Doğrulanmış commit | Yok; harici server kurulumu ayrıca gerekir |
| UniTask | 2.5.11 | Yok |
| VContainer | 1.19.0 | Yok |
| DOTween | 1.3.030 | Import sonrası setup |
| Event Bus | 1.0.0 | UniTask, VContainer |
| PinnedPrefab | 1.0.0 | Yok |
| TimeScaleToolbar | 1.0.0 | Yok |
| URP PostProcessing Toolkit | 2.0.0 | VContainer, DOTween setup |
| Component Pool | 0.1.0 | VContainer |
| Camera Effects | 1.0.0 | UniTask, VContainer, DOTween setup |
| State Machine | 1.0.0 | Yok |
| Scene Loader | 1.0.0 | UniTask |
| Audio System | 1.0.0 | UniTask, VContainer, Event Bus, DOTween setup |
| Prefab Exporter | 1.0.0 | Yok |

URP Toolkit, UPM üzerinden URP **17.3.0**; Camera Effects ise Cinemachine **3.1.7** bağımlılığını ister. Bu paketlerin kurulumu mevcut render pipeline ve Cinemachine sürümlerinizi etkileyebilir. Paket katalog sürümleri installer'ın 1.0.0 sürümüne aittir; kesin Git adresleri [DependencyInstallCatalog.cs](Packages/com.kameryurdakull.package-structure/Editor/DependencyInstallCatalog.cs) dosyasındadır.

URP Toolkit 2.0.0 kendi Event Bus uygulamasını içermez. Ortak Event Bus'a bağlamak için `IPostProcessEventBus` adaptörünü VContainer'a kaydedin. Camera Effects kendi ayrı namespace'inde bir event modülüne sahiptir. Bu servislerin sahne ve entegrasyon adımlarını kendi README dosyalarından takip edin.

## Repo düzeni

```text
Packages/com.kameryurdakull.package-structure/
  package.json
  Editor/        # Installer penceresi, katalog ve kurulum servisi
  README.md
  CHANGELOG.md
Assets/          # Yerel geliştirme ve doğrulama projesi
ProjectSettings/ # Geliştirme projesi ayarları
Validation/      # Test sonuçları ve tekrar kullanılabilecek test kaynakları
```

Repo'yu clone edip Unity ile açtığınızda installer embedded package olarak yüklenir. Başka projelerde yukarıdaki Git URL'sini kullanın. Dağıtılan UPM klasörü Editor kodunu ve dokümanlarını içerir; üçüncü taraf paket kaynakları veya DOTween ikilileri bu klasörde dağıtılmaz.

Yeni bir paket eklemek için `DependencyInstallCatalog.Packages` listesine doğrulanmış paket kimliği, sabitlenmiş Git URL'si ve önkoşullarını tanımlayan bir `DependencyPackage` ekleyin. UI ve kurulum servisi aynı kataloğu kullanır.

## Doğrulama

Dependency paketleriyle yapılan çalışmada **67 EditMode testi** ve URP Toolkit 2.0.0 için **9 PlayMode testi** geçti. Ayrıntılar [Toolkit test raporu](Validation/ToolkitV2Validation.md) ve [ilk paket kurulum raporu](Validation/DependencyValidation.md) içinde kayıtlıdır. Bu sonuçlar Player build, farklı platformlar veya performans garantisi anlamına gelmez.

Installer'ın UPM 1.0.0 yapısı geliştirme projesinde embedded package olarak doğrulandı: Unity paketi tanıdı, dört script `PackageStructure.Editor` assembly'sinde yalnızca Editor için derlendi ve installer penceresi açıldı. Son Console kontrolünde hata veya uyarı yoktu. [Paket doğrulama kaydı](Validation/upm-package-validation.txt). GitHub URL'siyle uzaktan kurulum, yeni paket dosyaları push edildikten sonra ayrıca doğrulanabilir.

Her dependency kendi lisansına ve dağıtım koşullarına tabidir. DOTween resmi [Demigiant](https://dotween.demigiant.com/) kaynağından indirilir.
