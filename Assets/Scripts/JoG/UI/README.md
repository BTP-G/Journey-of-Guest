# UI 与表现

## 关键入口

- `Popup/PopupManager.cs`：Toast、Confirm、Message、Loader，使用池化并跨场景保留。
- `FloatingTextController.cs`：订阅 `HealthChangeReport`，按 `deltaValue`、颜色和位置显示飘字。
- `Health/ScreenHealthBar.cs`、`WorldHealthBar.cs`：屏幕与世界血条，世界血条使用 Ratio。
- `Buff/ScreenBuffBar.cs`、`WorldBuffBar.cs`、`BuffIcon.cs`：Buff 图标；CharacterNameplate 和 PlayerCharacterOverlay 当前每 4 帧更新。
- `Character/CharacterNameplate.cs`：WorldHealthBar、WorldBuffBar 和 Billboarder，随 LifeStart/LifeStop 显隐。
- `Character/PlayerCharacterOverlay.cs`：玩家 HUD，随 Ownership 显隐。
- `Audio/NetworkAudioSource.cs`、`Video/NetworkVideoPlayer.cs`：RPC 播放/暂停，并通过 OnSynchronize 对齐时间或帧。
- `Effects/EffectSpawner.cs`、`Networking/Components/NetworkEffectSpawner.cs`：本地和网络粒子池化生成。

## 当前状态

- `IngameOverlayController.cs` 仍为空占位。
- `PlayerCharacterOverlay.prefab` 当前有 Missing Script，具体情况见 [角色上下文](../Character/README.md)。
- 核心玩法组件不通过序列化 UnityEvent 通信；UnityEvent/UnityEvent2 原则上只用于 UI 和 Inspector 表现层。
