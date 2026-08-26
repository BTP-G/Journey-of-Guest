# Expriverse - 项目导航

## 导航

| 范围 | 上下文 | 入口 |
| --- | --- | --- |
| 项目、启动、DI、数据加载 | [总览](Assets/Scripts/Expriverse/README.md) | `RootScope.cs`、`LifetimeScopes/GameplaySceneScope.cs` |
| 实体、角色、输入、状态、移动、生命 | [角色](Assets/Scripts/Expriverse/Character/README.md) | `CharacterEntity.cs`、`CharacterSpawner.cs`、`StateMachines/CharacterRootStateMachine.cs` |
| Gameplay Effects | [效果](Assets/Scripts/Expriverse/GameplayEffects/README.md) | `Character/CharacterEffects.cs`、`Character/CharacterTimedEffects.cs`、`Character/CharacterPeriodicHealthChanges.cs` |
| 物品、库存、掉落、存档 | [库存](Assets/Scripts/Expriverse/Inventory/README.md) | `Inventory/CharacterInventory.cs`、`Item/ItemData.cs` |
| 战斗、命中、弹体 | [战斗与弹体](Assets/Scripts/Expriverse/Projectiles/README.md) | Expriverse 包 `Runtime/Combat`、`Projectiles` |
| 会话、消息、网络对象、大厅 | [网络](Assets/Scripts/Networking/README.md) | `Expriverse/Networking/SessionService.cs`、`Packages/io.github.xoderony.networking/Runtime/NetworkObject.cs`、`NetworkVariableComponent` / `NetworkRpcComponent`、Expriverse 包 `UnnamedMessageBroker.cs` |
| 交互、AI、寻路、道具 | [交互与 AI](Assets/Scripts/Expriverse/AI/README.md) | `Character/CharacterInteractor.cs`、`AI/TargetFinder.cs`、`Props` |
| HUD、音视频、表现 | [UI 与表现](Assets/Scripts/Expriverse/UI/README.md) | `UI`、`Character/CharacterNameplate.cs`、`Character/PlayerCharacterOverlay.cs` |
| 数据注册、Mod API | [数据与 Modding](Assets/Scripts/Expriverse/Modding/README.md) | `Utilities/AssetsUtility.cs`、`Modding/ModManager.cs` |

## 包级上下文

公开职责优先读包文档：[Expriverse](Packages/io.github.xoderony.expriverse/README.md)、[Networking](Packages/io.github.xoderony.networking/README.md)、[Foundation](Packages/io.github.xoderony.foundation/README.md)、[Gameplay Effects](Packages/io.github.xoderony.gameplay-effects/README.md)、[Movement](Packages/io.github.xoderony.movement/README.md)、[Unity](Packages/io.github.xoderony.unity/README.md)。本地包从 `PackageTemplates/io.github.xoderony.feature-template` 开始。P2P 网络细节以 [网络模块 README](Assets/Scripts/Networking/README.md) 为准。
