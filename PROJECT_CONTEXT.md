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
| 场景 | `BootstrapScene.unity` + `MainScene.unity` 启用（Build Settings 入口）；`GameplayScene_1/2.unity` 未启用；另启用 3 个第三方演示场景（非入口）；`Test.unity` 不在 Build Settings |
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
| 角色整体、输入、能力 | `Assets/Scripts/JoG/Character/CharacterEntity.cs`、`CharacterSpawner.cs`、`CharacterInputBinding.cs`、包 `Runtime/Character/InputBanks`、项目 `Character/Components` |
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
| `Packages/io.github.xoderony.jog` | `JoG` 契约：Entity/IComponent、Stat/StatModifier、Health 路由与消息、StateMachines/States、InputBanks、Interaction、Faction、Modding、UnnamedMessageBroker、Q16Serializer、EntitySerializer |
| `io.github.xoderony.foundation` | `Xoderony.Foundation`：无 Unity 依赖集合（IntMap/SpanList/SpanIntMap 等）、委托通道（DelegateChannel/IDelegateDispatcher/IDelegateSubscriber）、扩展、对象池 |
| `io.github.xoderony.unity` | `Xoderony.Unity`/`.Editor`：Q16、PlayerLoop（PostUpdateLoop/PreUpdateLoop）、GameObject/ComponentPool、通用组件（Billboarder/ColliderEvents/ParticleSystemEvents）、ArrayList、编辑器控件与属性 |
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
| 包 `Character/InputBanks` + `InputBankHub` | 输入银行（Move/Aim/Jump/Sprint/Primary/Secondary/Special/Interact/Boolean/Vector3），按类型懒创建 |

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
| `Projectiles/ProjectileEntity.cs` | Entity 子类；PropertyHub（Owner/Attacker/DamageValue 等）；OnSynchronize 同步 Owner；Owner 端 PreUpdateLoop 到期 DeferDespawn |
| `Projectiles/ProjectileMotor.cs` | 仅 Owner 启用碰撞；继承速度/忽略碰撞体；OnCollisionEnter 分发 `ICollisionMessageHandler` + DeferDespawn |
| `Projectiles/ProjectileDamageOnCollision.cs` | 碰撞伤害逻辑整体注释（禁用） |
| `Projectiles/ProjectileExplosion.cs` | `Detonate`：OverlapSphere → `CanTakeDamage` → `TakeDamage` + onDamage（爆炸伤害有效；impulse 注释） |
| `Projectiles/ProjectileApplyEffectOnDamage.cs` / `ProjectileApplyDotOnDamage.cs` | 效果/DoT 挂载器（`OnDamage` 无调用方，未接通） |
| `Character/States/Mage/MageSkillController.cs`、`Spitter/SpitterSkillController.cs` | 创建弹体并 `SetProperty(Attacker/DamageValue)` |

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
- 弹体战斗：`ProjectileDamageOnCollision` 伤害块被注释 → 碰撞伤害禁用（爆炸伤害可用）；`ProjectileApplyEffectOnDamage` 仅加载无挂钩；`ProjectileApplyDotOnDamage.OnDamage` 无调用方；`Gameplay/Attacker.cs` 整体注释（旧 Source 类型已移除）。
- 祭坛：`DemonAltarInteraction` 的生命代价与效果施加被注释，仅广播交互事件（`CanInteract` 仍检查血量比例）。
- Steam 大厅：`SteamLobbyController.OnLobbyEntered` 中 transport 启动被注释 → 加入大厅不会自动联网；`FacepunchTransportController` 可手动 Start。Unity Services 会话为当前主路径（两路径共存行为未验证）。
- 占位实现：`MainSceneScope`（空）、`IngameOverlayController`（空）、`JoGApplication.Initialize`（空）。
- 旧目录：`Assets/Scripts/JoG/Buff` 仅剩 `.meta`（旧 Buff 脚本已迁移至 GameplayEffects/CharacterEffects）。
- 风险点（代码事实，未验证是否预期）：`CharacterPeriodicHealthChanges.ApplyTick` 直接 `Router.Route`，不经 `CanDamage`/`CanHeal` 准入；`HealthChangeReport.value` 为修改后的请求值，`deltaValue` 为实际 HP 变化，消费方需按语义选择（如满血治疗/过量伤害）。
