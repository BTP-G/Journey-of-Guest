# Journey of Guest - 项目上下文（AI 导航）

> 定位与速览文件：先看「任务导航」找入口，再沿直接依赖扩展；具体行为以源码、Prefab、Scene 和配置为准。
> 本文只记录项目事实、模块职责、关键入口与当前状态，不写协作规则（规则见 AGENTS.md）。

## 快速事实

| 项 | 值 |
| --- | --- |
| Unity | `6000.0.80f1`（`ProjectSettings/ProjectVersion.txt`） |
| 渲染 / 网络 | URP `17.0.4`；Netcode for GameObjects `2.13.1`（manifest 与 packages-lock 一致）；NGO 分布式权威（Distributed Authority） |
| 玩法 | 多人合作冒险肉鸽；会话基于 Unity Services Multiplayer + Relay + QoS 选最低延迟区域 |
| 技术栈 | VContainer（git）、Animancer Pro `8.3.0`（本地包）、UniTask、MessagePipe、Input System `1.19.0`、YooAsset `3.0.5`、ZString、FastReflection、Facepunch Transport、Newtonsoft.Json |
| 程序集 | `Assets/Scripts/JoG`（264 个 .cs，无 asmdef）→ `Assembly-CSharp`；`Packages/io.github.xoderony.jog` → `JoG`（Mod 契约，不反向依赖 Assembly-CSharp）；`io.github.xoderony.*` → `Xoderony.*` |
| 场景 | `BootstrapScene.unity` + `MainScene.unity` 启用（Build Settings 入口）；`GameplayScene_1/2.unity` 未启用；`Test.unity` 不在 Build Settings |
| DI 入口 | `Assets/Scripts/JoG/RootScope.cs`（根）；`LifetimeScopes/GameplaySceneScope.cs`（玩法场景）；`LifetimeScopes/MainSceneScope.cs`（空占位） |
| 数据注册 | YooAsset DefaultPackage 标签 `item_data` / `character_data` / `gameplay_effect_def` / `periodic_health_change_def` / `network_prefab` → `AssetsUtility.LoadDataFromPackage` → 各 Shared 注册表 + NGO PrefabHandler |
| 效果 ID | `GameplayEffectDefinitionRegistry.Shared`：`Animator.StringToHash(name)`，冲突抛异常，Id 0 保留 |
| 网络消息 | `UnnamedMessageBroker` 按 byte 类型分发（服务端中继，FastBufferWriter/Reader）：`1`=Chat、`2`=HealthChange、`3`=Hit |
| 定点数 | `Xoderony.Numerics.Q16`（16 位小数，`Xoderony.Unity` 程序集）；包内 `Q16Serializer.cs` 提供网络读写 |

> NGO API 有疑问时优先查当前包源码或官方文档，不依赖旧版本记忆。
> 迁移中代码以「已知迁移状态」为准，不视为稳定协议。

## 任务导航（优先）

| 任务 | 优先读取 |
| --- | --- |
| 启动、依赖注入 | `Assets/Scripts/JoG/RootScope.cs`、`LifetimeScopes/GameplaySceneScope.cs`、相关 Scene |
| 实体、组件生命周期 | `Packages/io.github.xoderony.jog/Runtime/Entities/Entity.cs`、`IComponent.cs`、`EntitySerializer.cs`、`JoG.asmdef` |
| 角色整体、输入、能力 | `Assets/Scripts/JoG/Character/CharacterEntity.cs`、`CharacterSpawner.cs`、`CharacterInputBinding.cs`、`InputKeys.cs`、包 `io.github.xoderony.foundation/Runtime/InputChannels`、项目 `Character/Components` |
| 状态机、动画、移动 | 包 `Runtime/StateMachines`、`Assets/Scripts/JoG/StateMachines/CharacterRootStateMachine.cs`、`Character/States`、`Packages/io.github.xoderony.movement` |
| 状态网络同步 | 包 `Runtime/StateMachines/NetworkStateMachine.cs`、`Entity.OnSynchronize` 链 |
| 属性、生命 | 包 `Runtime/Character/Stat.cs`、`StatModifier.cs`、`Runtime/Health/*`（HealthComponent、HealthChangeRouter、Damageable/Healable、HealthComponentChangeResolver） |
| 角色效果（Buff） | `Assets/Scripts/JoG/Character/CharacterEffects.cs`、`CharacterTimedEffects.cs`、`CharacterPeriodicHealthChanges.cs`、`GameplayEffects/Controllers` |
| 战斗、命中、弹体 | 包 `Runtime/Health/HitRouter.cs`、`HurtBox.cs`；`Character/States/*/SkillController.cs`、`Assets/Scripts/JoG/Projectiles` |
| 物品、库存、掉落 | `Assets/Scripts/JoG/Item/*`、`Inventory/*` |
| 会话、网络消息、Prefab | `Assets/Scripts/JoG/Networking/*`（SessionService、NetworkObjectFactory、GenericPrefabInstanceHandler、NetworkPlayerPrefabHandler）、包 `Runtime/Networking/UnnamedMessageBroker.cs` |
| 大厅（Steam） | `Assets/Scripts/JoG/Lobby/*`、`UI/FacepunchTransportController.cs` |
| 交互 | 包 `Runtime/Interaction/*`、`Assets/Scripts/JoG/Character/CharacterInteractor.cs`、`Props/*` |
| AI | `Assets/Scripts/JoG/AI/*`（TargetFinder、EnemySpawner、Patrol） |
| UI | `Assets/Scripts/JoG/UI/*`、对应 UXML/Prefab/Scene |
| Mod API | 包 `Runtime/Modding/*`、`Assets/Scripts/JoG/Modding` |
| Q16 与编辑器 Drawer | `Packages/io.github.xoderony.unity/Runtime/Numerics/Q16.cs`、`Editor/Numerics/Q16Drawer.cs` |
| 创建本地 UPM 包 | `PackageTemplates/io.github.xoderony.feature-template`、目标包 asmdef 和 `.meta` |

## 模块地图

| 路径 | 主要职责 |
| --- | --- |
| `Assets/Scripts/JoG` | `Assembly-CSharp` 项目实现：角色、效果、战斗、弹体、物品、AI、交互、大厅、UI、场景服务、具体网络玩法 |
| `Packages/io.github.xoderony.jog` | `JoG` 契约：Entity/IComponent、Stat/StatModifier、Health 路由与消息、StateMachines/States、Interaction、Faction、Modding、UnnamedMessageBroker、Q16Serializer、EntitySerializer |
| `io.github.xoderony.jog` 的 `Runtime/Combat` | `JoG.Combat`：HitQuery 形状查询（按实体去重）+ CombatDamage 伤害施加（正伤害量→负 Value；Route/Broadcast 双模式） |
| `io.github.xoderony.foundation` | `Xoderony.Foundation`：无 Unity 依赖集合（IntMap/SpanList/SpanIntMap 等）、委托通道（DelegateChannel/IDelegateDispatcher/IDelegateSubscriber）、扩展、对象池、输入通道（InputChannel<T>/InputChannelHub） |
| `io.github.xoderony.unity` | `Xoderony.Unity`/`.Editor`：Q16、PlayerLoop（PostUpdateLoop/PreUpdateLoop）、GameObject/ComponentPool、通用组件（Billboarder/ColliderEvents/ParticleSystemEvents）、ArrayList、AimInput（输入通道 Unity 载荷）、编辑器控件与属性 |
| `io.github.xoderony.gameplay-effects` | `Xoderony.GameplayEffects`：GameplayEffectData/Definition/Controller 契约 + 全局注册表（不依赖 JoG/NGO/VContainer） |
| `io.github.xoderony.movement` | CharacterMotor、地面检测/扫掠（GroundDetectionResult/SweepResult） |
| `io.github.xoderony.netcode` | NetworkBehaviour 编辑器扩展、NetworkObjectReferenceExtensions |
| `io.github.xoderony.logging` / `localization` / `navigation` / `yooasset` / `integrations` | Logger / Localizer+LocalizationKey / PathQueryFilter / YooAssetReference+Utility / ZString Utf16ValueStringBuilderExtensions |

依赖方向固定为 `Assembly-CSharp` / 外部 Mod → `JoG` 包 → `Xoderony.*`；`JoG.asmdef` 直接引用 `Xoderony.Foundation/.Unity/.Logging`，其余依赖以 asmdef GUID 列表为准。需要被独立 Mod 使用的稳定类型提升进 `JoG` 包，具体实现保留在 `Assets/Scripts/JoG`。

## 关键文件速查

### 实体与角色

| 文件 | 职责 / 关键约束 |
| --- | --- |
| 包 `Entities/Entity.cs` | 每实体 VContainer 子作用域；`DefaultExecutionOrder(-5000)`；注册子 GameObject 的 `IComponent` 与 `Entity.Components`（SerializeReference，按 `IComponent.Key` 键控）；转发 Spawn/Despawn/Ownership/Authority/Synchronize；静态 `IdToEntity` |
| 包 `Entities/IComponent.cs` | 序列化组件接口（`object Key`） |
| 包 `Entities/EntitySerializer.cs` | Entity 引用按 ID 读写（null → `ulong.MaxValue`） |
| `Character/CharacterEntity.cs` | 缓存 Animator/Animancer/Motor/Rigidbody；注入 spawn/despawn 委托；OnBuilt 解析 Health/Effects/PeriodicHealthChanges/TimedEffects/Model/HealthChangeRouter/HitRouter |
| `Character/CharacterSpawner.cs` | `NetworkVariable<NetworkObjectReference>`（Owner 写）；公开 `TrySpawnBody`/`TryRecycleBody`（仅权威）；`CharacterInputBinding` 优先 Spawner Driver，无则回退 Body 旧 Driver |
| `Character/PlayerSpawner.cs` | 玩家角色创建（DebugCommand、UI 卡片）；`OnBodyAssigned` 满血 |
| `AI/EnemySpawner.cs` | 敌人重生（`_respawnCount`/`_respawnAt`）；按 DifficultyManager 差值施加 MaxHealth/AttackPower 效果 |
| `Character/CharacterBody.cs` | 旧 Body 组件（迁移中，仍挂在角色 Prefab） |
| `io.github.xoderony.foundation` 包 `Runtime/InputChannels`（+ `io.github.xoderony.unity` 的 `AimInput`；key 常量 `InputKeys` 在项目 `JoG.Character`） | 输入通道（`Xoderony.InputChannels`，string key + 泛型 `InputChannel<T>`），按 key 懒创建；key 常量 `InputKeys`（Move/Aim/Jump/Sprint/PrimarySkill/SecondarySkill/SpecialSkill/Interact）；Aim 通道用 `AimInput`（position+target） |

### 状态机与移动

| 文件 | 职责 |
| --- | --- |
| 包 `StateMachines/StateMachine.cs` | 当前状态 + Enter/Exit |
| 包 `StateMachines/MonoStateMachine.cs` | MonoBehaviour 启停表达状态层级 |
| 包 `StateMachines/NetworkStateMachine.cs` | 权威端 `TransitionTo` vs 接收端 `ApplyTransition` |
| `StateMachines/CharacterRootStateMachine.cs` | LifeStart/LifeStop 委托 → Life/Death 状态切换 |
| `Character/States/CharacterLocomotionStateMachine.cs` | `Motor.IsStable` + Move 输入 → Air/Move/Idle（未挂 Prefab） |
| `Character/States/*/SkillController.cs` | 各角色技能：HitBox 命中 → `IHittable.TakeHit`（HitRouter）+ `IDamageable.TakeDamage`（HealthChangeRouter） |
| `Character/States/HitBox.cs` | 触发式命中盒（UnityEvent，忽略父级碰撞体） |

### 属性与生命

| 文件 | 职责 / 关键约束 |
| --- | --- |
| 包 `Character/Stat.cs` | int 基础/上下限/当前值 + Q16 倍率槽（池化）；写路径标脏并立即触发 `ValueChanged`（通知语义，回调中读 `Value` 拿最新值）；`Value` 按需重算，消费边界才转 float |
| 包 `Character/StatModifier.cs` | `Stat.AddModifier` 返回实例句柄；`SetValue`/`Remove`（幂等）；槽位 API 为包内 internal |
| 包 `Health/HealthComponent.cs` | Current/Max/Ratio/IsAlive；Owner 写 `NetworkVariable`；Max 变化按比例缩放 Current；OnSynchronize 同步本地值 |
| 包 `Health/HealthChangeRouter.cs` | 消息类型 2；`CanDamage`/`CanHeal` 准入；`Broadcast`（先发远端再本地 Route）；modifier 委托 → resolver → report 委托 → `IPublisher<HealthChangeReport>` |
| 包 `Health/Damageable.cs` / `Healable.cs` | MonoBehaviour+IComponent 入口，先 `CanTakeDamage`/`CanTakeHeal` 再 Broadcast |
| 包 `Health/HealthComponentChangeResolver.cs` | `Current += value`；`report.deltaValue` 为实际 HP 变化（value 是修改后的请求值） |
| 包 `Health/HitRouter.cs` | 消息类型 3；Outgoing/IncomingHitMessageHandler 委托链 |
| 包 `Factions/Faction.cs` | 整数 Id；同 Id 友方、异 Id 敌方；伤害/治疗/AI 选敌用 Faction，不用 Unity Tag |
| `Character/Components/CharacterLifeController.cs` | 零点跨越 → `DeathMessage` + LifeStart/Stop 委托 |
| `Character/Components/CharacterMaxHealthController.cs` | MaxHealth Stat → `HealthComponent.Max` |
| `Character/Components/CharacterHealthRegenerationController.cs` | Regen Stat，PostUpdateLoop 推进 |
| `Character/Components/CharacterHitImpulseController.cs` | IncomingHit → `Motor.AddImpulse`（仅权威） |
| `Character/Components/CharacterHurtBoxLifeController.cs` / `CharacterMotorNetworkController.cs` | 承接旧 CharacterBody 的 HurtBox 生命周期 / Motor Spawn-Despawn-Ownership |

### 效果与物品

| 文件 | 职责 / 关键约束 |
| --- | --- |
| 包 `Xoderony.GameplayEffects` | Data/Definition/Controller 契约；Definition `_dataArray`（SerializeReference）+ `DataSpan`；Registry `Id = Animator.StringToHash(name)` |
| `Character/CharacterEffects.cs` | NetworkBehaviour+IComponent；Definition→Count 唯一所有者；`Dictionary<Type, IGameplayEffectController>` 按 Data 运行时类型分发；Add/RemoveEffect + Rpc + OnSynchronize 快照；VContainer 注入 `IReadOnlyList<IGameplayEffectController>` 后取具体数组遍历 |
| `Character/CharacterTimedEffects.cs` | 限时批次（ServerTime 过期），PostUpdateLoop 清理；本地 + RPC 两套 Add/Remove |
| `Character/CharacterPeriodicHealthChanges.cs` | 周期伤害 Source/Tick；TickCount/TickValue 按 `MergeMode` 合并；展示 Count = 剩余 Tick；Tick 直接 `Router.Route`（不经准入检查，见迁移状态风险） |
| `GameplayEffects/Controllers/*` | StatEffectController、DamageDealtEffectController、DamageDealtPeriodicDamageController、DamageReflectEffectController、HealthChangeValueModifier（outgoing/incoming）、ConditionalAreaDamageEffectController |
| `GameplayEffects/Data/StatEffectData.cs` | MultiplierBonus Q16 ∈ [-99.9%, +999%]；层数 ≤ 1000；负加成 `(1+bonus)^count` 复利 |
| `Item/ItemData.cs` | 继承 `GameplayEffectDefinition` + ITooltipSource；pickupPrefab/icon；`item_data` 标签 |
| `Inventory/CharacterInventory.cs` | `ItemData -> Count` 唯一持有者；`ItemCountChanged` |
| `Inventory/CharacterInventoryEffectController.cs` | 差量投影到 `CharacterEffects`（AddEffectRpc/RemoveEffectRpc） |
| `Inventory/CharacterInventoryNetwork.cs` | AddItemRpc/RemoveItemRpc（SendTo.Owner） |
| `Item/ItemPickupBehaviour.cs` | 拾取 → `GivePickupRpc`（Authority）→ `AddItemRpc` → 销毁 |
| `Inventory/CharacterItemDropController.cs` / `Item/ItemDropController.cs` | 玩家丢出 / 死亡掉落表（DeathMessage，仅 Enemy 阵营） |
| `Inventory/InventorySaveController.cs` | `persistentDataPath/InventorySaves/{Session.Code}.json` 持久化 |

### 战斗与弹体

| 文件 | 职责 |
| --- | --- |
| `Projectiles/ProjectileEntity.cs` | 射弹基类（abstract）：同步 Owner、lifetime 超时销毁；弹种类型继承本类 |
| `Projectiles/LinearProjectile.cs` / `PlacedProjectile.cs` | 射弹类型（按输入参数集划分）：直线飞行 / 定点放置；`Initialize` 强类型初始化 |
| `Projectiles/ProjectileExplosion.cs`、`Projectiles/Components/ProjectileDamage.cs` / `ProjectileDot.cs` / `ProjectilePenetration.cs` / `ProjectileDespawn.cs` | 能力组件（普通 IComponent，序列化在 Entity.Components）：区域爆炸 / 单目标直伤 / DoT / 穿透 / 销毁，由射弹类型被动调用 |
| `Runtime/Combat/HitQuery.cs` / `HitResult.cs` | 统一攻击检测：Sphere/Box 一次性查询，按实体去重保留最近点；HitResult 持有 Entity/Collider/命中点，能力组件由调用方从 Collider 解析 |
| `Runtime/Combat/CombatDamage.cs` | 伤害施加：阵营准入 + falloff + Route（Effects 内部）/ Broadcast（射弹、近战）；含 `ApplySingle` |
| `Character/States/Mage/MageSkillController.cs`、`Spitter/SpitterSkillController.cs` | 创建弹体并 `Initialize` 强类型初始化 |

### 网络与会话

| 文件 | 职责 / 关键约束 |
| --- | --- |
| 包 `Networking/UnnamedMessageBroker.cs` | byte 类型分发；服务端中继；`SendMessageToOthers` |
| `Networking/SessionService.cs` | Unity Services Multiplayer 会话（Create/Join/Query/Leave）；`WithDistributedAuthorityNetwork(region)`；Relay+QoS 选最低延迟区域 |
| `Networking/AuthenticationController.cs` | 匿名登录（IAsyncBootstrapModule） |
| `Networking/NetworkObjectFactory.cs` | PrefabHandler 注册/移除/实例化；DA 模式下 owner 强制 LocalClientId |
| `Networking/GenericPrefabInstanceHandler.cs` | 池化实例；LifetimeScope/Entity 父级注入 |
| `Networking/NetworkPlayerPrefabHandler.cs` | PlayerPrefab 专用 handler（RootScope 注册） |
| `Networking/SessionOwnerObjectSpawner.cs` | IsSessionOwner 时生成对象（YooAsset 引用） |
| `Networking/Components/*` | 网络粒子/事件/断线弹窗（NetworkDisconnector → LeaveSession） |
| `Lobby/SteamLobbyController.cs` | Steam 大厅（Facepunch）；加入大厅后 transport 启动被注释（迁移中） |
| `UI/FacepunchTransportController.cs` | 手动切换 Facepunch Transport 并 StartHost/Server/Client |

### 交互与 AI

| 文件 | 职责 |
| --- | --- |
| 包 `Interaction/IInteractable.cs`、`InteractionHandler.cs` | 交互契约与委托 |
| `Character/CharacterInteractor.cs` | Aim 目标 + Interact 输入 → `IInteractable.CanInteract/OnInteracted` + WorldTooltip/Outline |
| `Props/Teleporter.cs` / `GameEndRock.cs` | 目标达成切场景（ObjectiveController）/ 离场（LeaveSession） |
| `Props/DemonAltarInteraction.cs` / `HolyAltarInteraction.cs` | 祭坛交互（DemonAltar 效果施加被注释，见迁移状态） |
| `AI/TargetFinder.cs` | Faction 敌我 + HurtBox 最近目标；无目标回巡逻路线 |
| `AI/Patrol/*` | PatrolService / PatrolRoute / IPatrolBehavior |
| `AI/AITarget.cs`、`NavMeshAgentController.cs`、`PathFinder.cs` | 目标 Transform / NavMesh 驱动 / 寻路 |

### UI 与表现

| 文件 | 职责 |
| --- | --- |
| `UI/Popup/PopupManager.cs` | Toast/Confirm/Message/Loader（池化，DontDestroyOnLoad） |
| `UI/FloatingTextController.cs` | 订阅 `HealthChangeReport` → 飘字（deltaValue/color/position） |
| `UI/Health/ScreenHealthBar.cs`、`WorldHealthBar.cs` | 血条（World 按 Ratio） |
| `UI/Buff/ScreenBuffBar.cs`、`WorldBuffBar.cs`、`BuffIcon.cs` | Buff 图标条（CharacterNameplate/PlayerCharacterOverlay 每 4 帧更新） |
| `Character/CharacterNameplate.cs` | 名牌：WorldHealthBar + WorldBuffBar + Billboarder；LifeStart/Stop 显隐 |
| `Character/PlayerCharacterOverlay.cs` | 玩家 HUD：ScreenHealthBar + ScreenBuffBar；Ownership 显隐 |
| `Audio/NetworkAudioSource.cs`、`Video/NetworkVideoPlayer.cs` | RPC 播放/暂停 + OnSynchronize 时间/帧同步 |
| `Effects/EffectSpawner.cs`、`Networking/Components/NetworkEffectSpawner.cs` | 本地/网络粒子池化生成 |

### 数据注册与 Modding

| 文件 | 职责 |
| --- | --- |
| `Utilities/AssetsUtility.cs` | 按标签加载数据到 Shared 注册表与 NGO PrefabHandler；`LoadLanguageFromHjson` |
| `DefaultPackageManager.cs` | 创建 YooAsset DefaultPackage 并 `LoadDataFromPackage` |
| `Character/CharacterDataDictionary.cs`、`Item/ItemDataDictionary.cs`、`GameplayEffects/PeriodicHealthChangeDefinitionDictionary.cs` | Shared 注册表（含 DebugCommand） |
| `Modding/ModManager.cs` | `Assets/Mods` 扫描 mod.json → 拓扑排序 → `Assembly.LoadFrom` → Mod 启用/禁用（enabled.txt） |
| 包 `Modding/Mod.cs`、`IModManager.cs` | Mod 基类与契约 |

## 已知迁移状态

- 2026-08-12 开发用嵌入式包：`Packages/io.github.xoderony.networking`（独立仓库 `Xoderony/io.github.xoderony.networking`，程序集 `Xoderony.Networking`，无 JoG/NGO/Steamworks 依赖）以本地 clone 形式放入项目内开发，尚未接入玩法；主仓库 `.git/info/exclude` 忽略该目录，包代码在其自身仓库提交。
- 2026-08-14 生成模块迁移：`NetworkObject`/`NetworkSpawnManager` 按对等模型重写，`networkObjectId` 定案为复合 id `NetworkObjectId(PeerId, LocalId)`（PeerId 取信封 sender，载荷只传 LocalId）；Spawn/Despawn/EntityState（类型 2/3/4）由 `NetworkSpawnManager` 构造时注册，新对等端加入时补发本端对象、离开/停止时清理；协议载荷上限 1024。包内 Runtime 静态编译验证通过，未在 Unity 编译/运行；样例与 README 重写、游戏项目接线仍待办。
- 2026-08-10 冗余清理：移除零引用第三方资源包与演示场景（含 Build Settings 中 3 个演示场景），文件备份于仓库外 `E:\UnityProjects\Journey of Guest Backup\2026-08-10-redundant`；`GameplayScene_2` 烘焙光照贴图（`.exr/.png`）改为本地生成并加入 `.gitignore`，克隆不再下载。同日用 `git filter-repo` 重写历史（清除全部已删除路径，pack 3.34 GiB → 0.47 GiB）并 force push，旧克隆需重新同步。

### 稳定实现（静态核对，未运行验证）

- 实体链路：Entity 子作用域 + IComponent 注册 + 委托通道；CharacterEntity 缓存入口。
- 生命链路：Damageable/Healable → HealthChangeRouter（类型 2）→ modifier → resolver → report 委托 + `IPublisher<HealthChangeReport>`；FloatingText/CharacterLifeController 消费。
- 命中链路：HitRouter（类型 3）+ IncomingHitMessageHandler（CharacterHitImpulseController 未挂 Prefab 前命中不产生击退）。
- 效果链路：CharacterEffects 分发 + 各 Controller（Stat/伤害响应/反射/值修正/区域伤害）；TimedEffects/PeriodicHealthChanges 独立批次。
- 物品链路：拾取 RPC → CharacterInventory → 差量投影 CharacterEffects；掉落表/丢出/存档已实现。
- 会话链路：Unity Services 登录 → SessionService（DA）→ SessionOwnerObjectSpawner / PlayerSpawner。
- 数据注册：YooAsset 标签加载 → Shared 注册表 + 网络 Prefab 注册。
- 移动：CharacterMoveInputHandler 驱动 CharacterMotor（当前 Prefab 状态），MaxMoveSpeed/MoveAcceleration Stat 生效。

### 迁移中 / 未完成（不视为稳定协议）

- 8 个角色 Prefab（Fighter/Ghost/Golem/GiantDummy/MegaspikanLarvae/Mage/Skeleton/Spitter）：
  - 已删脚本（含 `CharacterHealth`，2026-08-08 删除；`PlayerCharacterInventory` 已删）残留 12 个不同 Missing Script GUID（8 个 Prefab 共有的 5 个）；`PlayerObject.prefab` 另有 1 个；`PlayerCharacterOverlay.prefab` 有 7 个。具体 GUID→类名未逐项解析，需 Unity 刷新确认。
  - 新组件未挂载：`HealthComponent`、`Faction`、`HealthComponentChangeResolver`、`CharacterLifeController`、`CharacterMaxHealthController`、`CharacterHealthRegenerationController`、`CharacterHurtBoxLifeController`、`CharacterMotorNetworkController`、`CharacterHitImpulseController`、`CharacterEffects`/`CharacterTimedEffects`/`CharacterPeriodicHealthChanges`、`CharacterInventory` 等；`Entity.Components` 目前为空引用列表（如 Fighter 含 4 个 null），未配置 `Stat`。
  - 旧 `CharacterBody` 仍挂载，迁移完成前不能与拆分组件同时启用。
  - 角色同时保留 Animator 与 Animancer，迁移未完成。
  - 仍由 `CharacterMoveInputHandler` 驱动 Motor，新移动状态机（`CharacterLocomotionStateMachine`/Idle/Move/Air）未挂 Prefab；不能让两套逻辑同时写 CharacterMotor。
  - 输入 Driver 仍在角色 Prefab 上（兼容旧序列化）；`CharacterInputBinding` 优先用 Spawner Driver，无则回退 Body 旧 Driver；后续迁至 Spawner。
- 弹体战斗（2026-08-09 重构）：类型继承 `ProjectileEntity`（`LinearProjectile`/`PlacedProjectile`，按输入参数集划分）+ 能力组件被动组合（普通 IComponent，配置在 `Entity.Components` 列表，类型经容器 `TryGetComponent` 解析）；伤害统一走 `CombatDamage`（`ApplySingle`/`ApplySphere`，Broadcast）。旧脚本（Motor/DamageOnCollision/ExplosionOn*/EffectOn*/ApplyDot/Properties 等）已删除，旧 `.meta` 待 Unity 刷新清理；4 个射弹 Prefab 需重新组装（主脚本 + `Entity.Components` 配置能力 + 网络骨架，由用户处理）。
- 战斗检测（2026-08-09 新增 `JoG.Combat`）：`HitQuery`/`CombatDamage` 已接入 Golem/Ghost/Bite/ConditionalArea；Fighter/Skeleton（HitBox 触发式）与 Projectile 模块待各自重构时迁移。
- 符号风险（代码事实）：`HealthChangeMessage.Value` 负值为伤害；射弹已迁移至 `CombatDamage`（传正伤害量，内部取负）；未迁移的近战（Fighter/Skeleton）仍以正 `Value` 调 `TakeDamage`，会走治疗分支，待 HitBox 重构时统一。
- 祭坛：`DemonAltarInteraction` 的生命代价与效果施加被注释，仅广播交互事件（`CanInteract` 仍检查血量比例）。
- Steam 大厅：`SteamLobbyController.OnLobbyEntered` 中 transport 启动被注释 → 加入大厅不会自动联网；`FacepunchTransportController` 可手动 Start。Unity Services 会话为当前主路径（两路径共存行为未验证）。
- 占位实现：`MainSceneScope`（空）、`IngameOverlayController`（空）、`JoGApplication.Initialize`（空）。
- 旧目录：`Assets/Scripts/JoG/Buff` 仅剩 `.meta`（旧 Buff 脚本已迁移至 GameplayEffects/CharacterEffects）。
- 风险点（代码事实，未验证是否预期）：`CharacterPeriodicHealthChanges.ApplyTick` 直接 `Router.Route`，不经 `CanDamage`/`CanHeal` 准入；`HealthChangeReport.value` 为修改后的请求值，`deltaValue` 为实际 HP 变化，消费方需按语义选择（如满血治疗/过量伤害）。
