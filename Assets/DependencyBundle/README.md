# Package Structure dependency installer

Import the unitypackage into a Unity 6000.3 project, then open **Tools > Package Structure > Dependency Installer**. Every dependency has an individual install button. Missing prerequisites disable the button and explain the required action. The window scrolls to accommodate the full catalog.

Git packages use pinned tags or commit hashes from their upstream repositories. Package identities and subfolder paths were checked against their `package.json` files on 2026-10-03. Unity MCP, UniTask, VContainer, Event Bus, and DOTween remain available.

| Added package | Repository and UPM folder | Required before installation |
| --- | --- | --- |
| PinnedPrefab | [PinnedPrefab](https://github.com/kameryurdakull/PinnedPrefab), repository root | None |
| TimeScaleToolbar | [TimeScaleToolbar](https://github.com/kameryurdakull/TimeScaleToolbar), repository root | None |
| URP PostProcessing Toolkit 2.0.0 | [URP-PostProcessing-Toolkit](https://github.com/kameryurdakull/URP-PostProcessing-Toolkit), `Packages/com.kameryurdakull.post-process-service` | VContainer, DOTween setup |
| Component Pool | [unity-component-pool](https://github.com/kameryurdakull/unity-component-pool), `Packages/com.kameryurdakull.pool-service` | VContainer |
| Camera Effects | [unity-camera-effects](https://github.com/kameryurdakull/unity-camera-effects), `Distribution/com.kameryurdakull.camera-framework` | UniTask, VContainer, DOTween setup |
| State Machine | [state-machine](https://github.com/kameryurdakull/state-machine), `Packages/com.kameryurdakull.state-machine` | None |
| Scene Loader | [unity-scene-loader](https://github.com/kameryurdakull/unity-scene-loader), `Packages/com.kameryurdakull.sceneloader` | UniTask |
| Audio System | [unity-audio-system](https://github.com/kameryurdakull/unity-audio-system), `Packages/com.kameryurdakull.audiosystem` | UniTask, VContainer, Event Bus, DOTween setup |
| Prefab Exporter | [unity-prefab-exporter](https://github.com/kameryurdakull/unity-prefab-exporter), `Packages/com.kameryurdakull.prefab-exporter` | None |

URP 17.3.0 and Cinemachine 3.1.7 are resolved automatically by the Toolkit and Camera Effects package manifests respectively. Scene Loader's VContainer/Event Bus integrations and DOTween sample are optional. Camera Effects has its own namespaced Event Bus and does not require the shared Event Bus package.

**Toolkit compatibility:** version 2.0.0 removes the bundled Event Bus and UniTask dependency. Its `Game.Core.PostProcessing.IPostProcessEventBus` contract connects to the host's event bus through an adapter forwarding `Subscribe`, `Unsubscribe`, and `Publish`. Register that adapter with VContainer when using event-driven post-processing. Direct API calls remain available. The shared Event Bus and Toolkit can now be installed together; the installer pins the updated `bcdb5f30e62809c3f3265c2395bea149ca503e8b` commit.

To extend the installer, add a `DependencyPackage` entry to `DependencyInstallCatalog.Packages` with its verified package name, pinned Git URL and prerequisites. The window and service consume the same catalog; prerequisite enforcement also runs inside the installation service.

The Event Bus button installs `com.kameryurdakull.eventbus` from `https://github.com/kameryurdakull/unity-eventbus.git?path=/Packages/com.kameryurdakull.eventbus#v1.0.0` through Unity Package Manager. No Event Bus source templates are bundled. When updating a project that used the previous installer, remove its old `Assets/Scripts/EventBus` source files and assembly definition before installing the Git package.

After DOTween imports, open **Tools > Demigiant > DOTween Utility Panel** and click **Setup DOTween...** as required by Demigiant.

This installer contains no local UPM tarballs. The online Git packages and DOTween download require network access. Unity MCP's external Python/uv server runtime must be configured separately in its setup window.
