# Journey of Guest - 项目上下文

## 使用方式

本文件是任务入口导航，只记录当前项目事实、核心数据流和已确认风险。先从“任务导航”选择入口，再沿直接依赖扩展；具体行为始终以源码、Prefab、Scene 和配置为准。

- 版本以 `ProjectSettings/ProjectVersion.txt`、`Packages/manifest.json`、`Packages/packages-lock.json` 为准。
- 默认排除第三方 Demo、Samples、`Assets/Plugins` 和 Unity 生成目录。
- 当前实现与目标设计不一致时，以“已知迁移状态”为准，不把未完成代码视为稳定协议。

## 项目速览

- Unity `6000.0.80f1`，Universal Render Pipeline `17.0.4`。
- 多人合作冒险肉鸽；Netcode for GameObjects `2.13.0`，倾向分布式权威。
- VContainer 负责依赖注入；Animancer Pro 负责角色动画；另使用 UniTask、MessagePipe、Input System、YooAsset。
- 项目业务代码位于 `Assets/Scripts/JoG` 并统一编入 `Assembly-CSharp`；Mod 可独立引用的稳定契约位于 `Packages/io.github.xoderony.jog` 的 `JoG` 程序集；通用基础设施位于其他 `Packages/io.github.xoderony.*`。

## 启动入口

| 入口 | 职责 |
| --- | --- |
| `Assets/Scenes/BootstrapScene.unity` | Build Settings 入口场景 |
| `Assets/Scenes/MainScene.unity` | 主场景 |
| `Assets/Scripts/JoG/RootScope.cs` | 根容器：Unity Services、输入、NetworkManager、数据字典和全局服务 |
| `Assets/Scripts/JoG/LifetimeScopes/GameplaySceneScope.cs` | 玩法场景：MessagePipe、战斗、聊天、UI、网络 Prefab Handler |
| `Assets/Scripts/JoG/LifetimeScopes/MainSceneScope.cs` | 主场景作用域，目前内容较少 |

`GameplayScene_1.unity` 和 `GameplayScene_2.unity` 当前未启用。Build Settings 还包含第三方演示场景，分析正式流程时不要将其视为项目入口。

## 模块地图

| 路径 | 主要职责 |
| --- | --- |
| `Assets/Scripts/JoG` | 编入 `Assembly-CSharp` 的项目实现：角色、Buff、AI、物品、UI、场景服务和具体网络玩法 |
| `Packages/io.github.xoderony.jog` | `JoG`：PropertyHub、实体、生命、状态机、属性、输入槽、交互、未命名网络消息和 Mod 公共入口 |
| `Packages/io.github.xoderony.foundation` | `Xoderony.Foundation`：无 Unity 依赖的集合、委托通道、扩展和对象池 |
| `Packages/io.github.xoderony.unity` | `Xoderony.Unity` / `Xoderony.Unity.Editor`：Unity 通用组件、序列化集合、编辑器控件、`Xoderony.Numerics` |
| `Packages/io.github.xoderony.movement` | CharacterMotor、碰撞扫描和地面检测 |
| `Packages/io.github.xoderony.netcode`、`logging`、`localization`、`navigation`、`yooasset` | 对应领域的可复用基础设施 |

`Assembly-CSharp` 可以引用启用 `autoReferenced` 的 asmdef 程序集，但 asmdef 程序集不能反向引用预定义程序集。因此依赖方向固定为 `Assembly-CSharp` / 外部 Mod → `JoG` 包；需要被独立 Mod 使用的稳定类型应提升进 `JoG` 包，具体项目实现保留在 `Assets/Scripts/JoG`。Xoderony 包按部署、平台和第三方依赖边界划分程序集，命名空间仅用于组织 API；`foundation` 使用单一 `Xoderony.Foundation`，`unity` 仅保留 `Xoderony.Unity` 与 `Xoderony.Unity.Editor`。

## 核心结构

### 实体与角色

- `Packages/io.github.xoderony.jog/Runtime/Entities/Entity.cs` 为每个实体创建 VContainer 子作用域，按需提供 `Xoderony` 委托通道，注册子 GameObject 上及 `Entity.Components` 中的 `IComponent`，并转发 NGO Spawn、Despawn、Ownership、Synchronize 生命周期。
- `Assets/Scripts/JoG/Character/CharacterEntity.cs` 注册角色自身、Rigidbody、Animator、Animancer、CharacterMotor，并缓存生命、Buff、模型、输入和战斗入口。
- `Assets/Scripts/JoG/Character/PlayerSpawner.cs` 创建玩家角色并处理拥有权实例的初始生命。
- `Packages/io.github.xoderony.jog/Runtime/Character/InputBanks` 保存角色输入；`Assets/Scripts/JoG/Character/Components` 提供移动、冲刺、跳跃、朝向和受击等组合能力。

### 状态机与移动

- `Packages/io.github.xoderony.jog/Runtime/StateMachines/StateMachine.cs` 只管理当前状态及 Enter/Exit；`MonoStateMachine.cs` 用组件启用状态表达层级。
- 包内 `NetworkStateMachine.cs` 区分权威端主动切换与 RPC 接收端本地应用，具体 RPC 和初始快照由项目子类实现。
- `CharacterLocomotionStateMachine.cs` 根据 CharacterMotor 和移动输入决定 Idle、Move、Air；具体状态位于 `Assets/Scripts/JoG/Character/States`。
- 状态转移和网络边界的稳定设计规则见 `AGENTS.md`。

### 角色属性

- 包内 `StatBase<TMultiplier>` 管理名称、变化事件、倍率槽和序列化生命周期，只提供类型确定的槽操作。
- `Stat` 用于整数离散值，倍率存储为 Q16，使用 `long` 中间值重算。
- `FloatStat` 用于连续值，倍率存储为 `float`，使用 `double` 中间值重算。
- `IStat` 提供 float 与 Q16 两套倍率重载；具体属性直接实现，只在表示不匹配时转换。
- 普通 `IComponent` `CharacterMaxHealthController` 将最大生命属性变化写入 `HealthComponent.Max`，属性对象本身不依赖生命组件。
- `HealthChangeRouter` 负责生命变化的网络广播、实体局部委托路由和全局报告发布；目标实体的 `IHealthChangeResolver` 负责结算变化并生成报告，默认可序列化的普通 `IComponent`——`HealthComponentChangeResolver`——通过 `Entity.Components` 将其连接到 `HealthComponent`。
- `Faction` 是 `JoG` 包内可序列化的普通 `IComponent`，使用整数 ID 表达阵营；当前 PVE 关系规则是同 ID 友方、不同 ID 敌方，空来源可造成环境伤害但不能治疗。伤害/治疗、AI 选敌、击杀目标和敌人掉落均使用该组件，Unity Tag 不再承担阵营语义。
- `CharacterLifeController` 根据 `HealthComponent` 的存活/死亡零点跨越发布 Life Start、Life Stop 和 `DeathMessage`；初始死亡实例只同步本地生命表现，不重复发布死亡事实。`CharacterHealthRegenerationController` 独立处理回复，`CharacterRootStateMachine` 订阅生命事件切换根状态，不再逐帧轮询生命值。
- `CharacterHurtBoxLifeController` 和 `CharacterMotorNetworkController` 分别承接旧 `CharacterBody` 的 HurtBox 生命周期与 Motor Spawn、Despawn、Ownership 职责；`CharacterHitImpulseController` 独立处理受击冲量。
- `HitRouter` 负责物理命中消息的网络广播及来源、目标实体局部委托路由；`IHittable` 保留碰撞部位的局部处理入口，命中检测端只在攻击者拥有权威时产生消息。普通 `IComponent` `CharacterHitImpulseController` 订阅目标实体的 Incoming Hit，并只在目标权威端向 `CharacterMotor` 提交冲量。
- 当前类型映射：最大生命、攻击力、防御使用 `Stat`；最大移动速度、移动加速度、生命恢复速率使用 `FloatStat`。

### 网络入口

- `Packages/io.github.xoderony.jog/Runtime/Networking/UnnamedMessageBroker.cs`：通用 NGO 非命名消息分发。
- `SessionService.cs`：多人会话。
- `NetworkObjectFactory.cs`、`GenericPrefabInstanceHandler.cs`、`NetworkPlayerPrefabHandler.cs`：网络 Prefab 注册、实例化和 Spawn。
- `Assets/Scripts/JoG/Networking/Components`：粒子、特效和事件等网络表现。
- NGO API 有疑问时优先检查当前包源码或官方文档，不依赖旧版本记忆。

## 已知迁移状态

- `HitRouter` 已接入网络广播和实体局部路由；`CharacterHitImpulseController` 尚未加入角色的 `Entity.Components` 时，命中仍不会产生实际击退。
- 现有角色 Prefab 仍挂载旧 `CharacterHealth` 和 `CharacterBody`；新的 `HealthComponent` 尚未挂载，`Faction`、`HealthComponentChangeResolver`、`CharacterLifeController`、`CharacterHealthRegenerationController`、`CharacterHurtBoxLifeController`、`CharacterMotorNetworkController` 与 `CharacterHitImpulseController` 尚未完整加入 `Entity.Components`。完成 Prefab 迁移前不能删除旧脚本，也不能同时启用旧 `CharacterBody` 与其拆分组件。
- 角色仍同时保留 Animator 与 Animancer，迁移尚未完成。
- 角色 Prefab 的旧 `stats` 是 `CharacterEntity` 已删除字段的遗留数据；`Entity.Components` 尚未配置新的 `Stat`、`FloatStat` 和最大生命连接组件。
- 角色 Prefab 仍由 `CharacterMoveInputHandler` 驱动物理和 Animator，尚未接入新的移动状态机；迁移时不能让两套逻辑同时写入 CharacterMotor。

## 任务导航

| 任务 | 优先读取 |
| --- | --- |
| 启动、依赖注入 | `Assets/Scripts/JoG/RootScope.cs`、`Assets/Scripts/JoG/LifetimeScopes`、相关 Scene |
| 实体、组件生命周期 | `Packages/io.github.xoderony.jog/Runtime/Entities/Entity.cs`、`IComponent.cs`、`JoG.asmdef` |
| 角色整体、输入、能力 | `Assets/Scripts/JoG/Character/CharacterEntity.cs`、`Packages/io.github.xoderony.jog/Runtime/Character/InputBanks`、项目 `Components` |
| 状态机、动画、移动 | `Packages/io.github.xoderony.jog/Runtime/StateMachines`、`States`、`Assets/Scripts/JoG/Character/States`、`Packages/io.github.xoderony.movement` |
| 状态网络同步 | `Packages/io.github.xoderony.jog/Runtime/StateMachines/NetworkStateMachine.cs`、具体项目子类、包内 `Entity.cs` |
| 属性、Buff、生命 | `Packages/io.github.xoderony.jog/Runtime/Character`、`Assets/Scripts/JoG/Buff`、`Packages/io.github.xoderony.jog/Runtime/Health` |
| 会话、网络消息、Prefab | `Assets/Scripts/JoG/Networking`、`Packages/io.github.xoderony.jog/Runtime/Networking` |
| UI | `Assets/Scripts/JoG/UI`、对应 UXML、Prefab、Scene |
| Xoderony 包或程序集 | 目标 `Packages/io.github.xoderony.*`、使用方 asmdef、包清单 |
| JoG 包与 Mod API | `Packages/io.github.xoderony.jog`、`Assets/Scripts/JoG/Modding` |
| Q16 与编辑器 Drawer | `Packages/io.github.xoderony.unity/Runtime/Numerics/Q16.cs`、`Packages/io.github.xoderony.unity/Editor/Numerics/Q16Drawer.cs` |
| 创建本地 UPM 包 | `PackageTemplates/io.github.xoderony.feature-template`、目标包 asmdef 和 `.meta` |
