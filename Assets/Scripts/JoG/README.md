# JoG 项目实现总览

本目录是 Journey of Guest 的项目侧实现，统一编入 `Assembly-CSharp`。本文记录跨模块事实；角色、效果、网络等细节见各子目录 `README.md`。

## 快速事实

| 项 | 当前事实 |
| --- | --- |
| 项目 | Unity 6 多人合作冒险肉鸽 |
| 版本来源 | `ProjectSettings/ProjectVersion.txt`、`Packages/manifest.json`、`Packages/packages-lock.json` |
| 当前快照 | Unity `6000.0.80f1`、URP `17.0.4`、NGO `2.13.1` |
| 构建后端 | Mono，不考虑 IL2CPP |
| 场景 | Player Build Settings 只内置 `BootstrapScene.unity`；`MainScene.unity` 由 YooAsset `DefaultPackage` 运行期加载；`GameplayScene_1/2.unity` 未启用 |
| DI | `RootScope.cs` 为根入口；`LifetimeScopes/MainSceneScope.cs` 与 `GameplaySceneScope.cs` 为场景入口 |
| 主要技术栈 | VContainer、Animancer Pro `8.3.0`、UniTask、MessagePipe、Input System `1.19.0`、YooAsset `3.0.5`、ZString、FastReflection、Facepunch Transport、Newtonsoft.Json |

## 程序集与依赖方向

- `Assets/Scripts/JoG`：项目具体实现，命名空间使用 `JoG.*`，无 asmdef，编入 `Assembly-CSharp`。
- `Packages/io.github.xoderony.jog`：可供独立 Mod 引用的稳定 JoG 契约，程序集 `JoG`，不得反向依赖 `Assembly-CSharp`。
- `Packages/io.github.xoderony.*`：跨项目基础设施，程序集使用 `Xoderony.*`。
- 固定依赖方向为 `Assembly-CSharp` / 外部 Mod → `JoG` → `Xoderony.*`。具体 asmdef 引用以各包配置为准。

## 模块导航

- [角色、状态、属性与生命](Character/README.md)
- [Gameplay Effects](GameplayEffects/README.md)
- [物品与库存](Inventory/README.md)
- [战斗与弹体](Projectiles/README.md)
- [网络与会话](../Networking/README.md)
- [交互与 AI](AI/README.md)
- [UI 与表现](UI/README.md)
- [数据注册与 Modding](Modding/README.md)

## 启动与数据注册

- `RootScope.cs` 由 VContainerSettings 自动创建，负责 Unity Services、根容器、并发异步启动模块、P2P Runtime 和 YooAsset 场景切换；`MainSceneScope.cs` / `GameplaySceneScope.cs` 注册各自场景对象与 Popup。
- `DefaultPackageManager.cs` 创建 YooAsset DefaultPackage，再由 `Utilities/AssetsUtility.cs` 按标签加载数据。
- 当前标签包括 `item_data`、`character_data`、`gameplay_effect_def`、`periodic_health_change_def`、`network_prefab`，数据和 NGO PrefabHandler 保持原有注册行为；带 `JoGNetworkObject` 的 `network_prefab` 另注册到 P2P `INetworkObjectManager`。
- `GameplayEffectDefinitionRegistry.Shared` 使用 `Animator.StringToHash(name)` 生成 ID，`0` 保留，冲突抛异常。

## 包职责速查

| 包 | 主要职责 |
| --- | --- |
| `io.github.xoderony.jog` | Entity/IComponent、Stat、Health、StateMachines、Interaction、Faction、Modding、旧消息 Broker、序列化契约 |
| `io.github.xoderony.networking` | P2P 核心：会话事实、消息路由、对象生命周期/Prefab/id 解析与入网快照；不含 Lobby、NV、RPC；本地独立仓库 clone，细节见 [`Assets/Scripts/Networking/README.md`](../Networking/README.md) |
| `io.github.xoderony.foundation` | 集合、委托通道、对象池、`InputChannel<T>`/`InputChannelHub` |
| `io.github.xoderony.unity` | Q16、PlayerLoop、Unity 组件与池、AimInput、编辑器控件 |
| `io.github.xoderony.gameplay-effects` | 与 JoG/NGO/VContainer 解耦的效果 Data/Definition/Controller 契约与注册表 |
| `io.github.xoderony.movement` | CharacterMotor、地面检测和扫掠 |
| `io.github.xoderony.netcode` | NetworkBehaviour 编辑器扩展、NetworkObjectReference 扩展（仍服务当前 NGO 路径） |
| logging/localization/navigation/yooasset/integrations | 日志、本地化、寻路过滤、YooAsset 工具和 ZString 扩展 |

## 仓库级已知状态

- 2026-08-10 已移除零引用第三方资源包和演示场景，备份位于仓库外 `E:\UnityProjects\Journey of Guest Backup\2026-08-10-redundant`。
- `GameplayScene_2` 烘焙光照贴图改为本地生成并被 `.gitignore` 忽略。
- 同日使用 `git filter-repo` 清理历史并 force push，pack 从约 3.34 GiB 降至 0.47 GiB；旧克隆需要重新同步。
- 2026-08-17：`Xoderony.Networking` 与项目侧 `JoG.Networking.P2P`（`Assets/Scripts/Networking`）已落地会话/消息/对象 id 分配与 NV/RPC 模块，玩法仍走 NGO；P2P 未接线、未做 Unity 验证。`PROJECT_CONTEXT.md` 已拆为 `PROJECT_INDEX.md` + 各模块 README。
- 2026-08-23：RootScope、场景切换、场景级 Popup 和 P2P Root Runtime 已完成源码接线；Unity 编译、Prefab/场景序列化和双 Steam 客户端仍待验证。
- 当前占位实现包括 `UI/IngameOverlayController.cs`、`JoGApplication.Initialize`。
- `Assets/Scripts/JoG/Buff` 只剩旧目录元数据；实现已迁到 GameplayEffects/CharacterEffects。

以上状态是静态核对结果；未特别说明时，不代表已经通过 Unity 编译或运行验证。
