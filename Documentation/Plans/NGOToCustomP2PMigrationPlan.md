# NGO 与 Unity Services 移除及自定义 P2P 迁移方案

更新时间：2026-08-25

状态：讨论中，尚未实施。按启动与依赖链逐处分析；当前已确认 ApplicationScope 启动调整、旧服务与 NGO 根注册移除、玩法场景拥有完整玩法与网络会话，以及只读平台资料契约。

## 一、总体目标

- 将 `Xoderony.Networking` 与项目侧 Steam Lobby、Transport、Session 实现作为项目唯一的网络基础设施。
- 移除 NGO 主路径以及 Unity Services Authentication、Lobby、Matchmaker、Multiplayer、Player Accounts、QoS、Relay 等服务。
- 最终移除不再使用的 NGO、Unity Services 相关包依赖、程序集引用、代码接线以及 Scene/Prefab 配置。
- 保留应用启动、数据加载、场景切换和正式玩法所需能力，并逐步迁移仍依赖旧网络栈的实际消费者。

## 二、讨论与实施方式

- 从 `ApplicationScope` 开始，沿真实启动和依赖链逐步分析。
- 一次尽量只分析一个文件或其中一个明确职责；确认当前决策后再进入下一处。
- 当前源码与可达消费者是事实依据；旧文档仅作辅助。
- 先完成并确认整份方案，再交给其他模型实施；本阶段不修改源码、Prefab、Scene 或包依赖。
- 实施迁移期间不要求项目保持可启动；允许先解除旧注册，再逐步迁移消费者，不为中间态增加兼容注册或临时门面。
- 后续确认的决定继续追加到本文档，不只保留在对话或记忆中。

## 三、已确认步骤

### 第一步：移除 ApplicationScope 的 Unity Services 启动前置条件

#### 当前流程

```text
ApplicationScope.Awake
→ 初始化 Unity Services，失败则每五秒重试
→ Build VContainer
→ 并行初始化 IAsyncBootstrapModule
→ 加载 MainScene，失败则每五秒重试
→ 关闭初始化界面
```

当前 `Build()` 位于 `UnityServices.InitializeAsync()` 之后，因此 Unity Services 未初始化成功时，自定义 P2P 容器和其余应用服务也不会创建。

#### 目标流程

```text
ApplicationScope.Awake
→ Build VContainer
→ 并行初始化保留下来的 IAsyncBootstrapModule
→ 加载 MainScene，失败则每五秒重试
→ 关闭初始化界面
```

#### 实施内容

- 保留 `ApplicationScope` 作为应用根启动负责人。
- 删除 `InitializeUnityServicesWithRetryAsync()` 及其调用。
- 删除启动界面的 `Initializing Unity Services...` 状态。
- `InitializeApplicationAsync()` 直接从 `Building application services...` 和 `Build()` 开始。
- 删除本文件因此不再需要的 `using Unity.Services.Core`。
- 保留 `RetryDelay`，因为 MainScene 加载失败后的重试仍使用它。
- 保留现有异步模块初始化、MainScene 加载、取消传播和最终异常显示结构。

#### 本步骤边界

- 本步骤本身不修改 `Configure()` 中的 Unity Services、NGO 或旧会话注册；对应决定见第二步。
- 本步骤不决定哪些 `IAsyncBootstrapModule` 保留、迁移或删除。
- 本步骤不修改自定义 P2P 的构造与启动时机。

### 第二步：删除 ApplicationScope 中的 Unity Services 及旧服务注册

#### 已确认决定

- 从 `ApplicationScope.Configure()` 直接删除 `UnityServices.Instance` 以及 Authentication、Lobby、Matchmaker、Multiplayer、Player Accounts、QoS、Relay 的全部实例注册。
- 删除 `AuthenticationController` 注册；不保留 Unity 匿名登录兼容层。
- 删除 `SessionService` 注册；不保留基于 Unity Multiplayer、Relay 和 QoS 的旧会话实现。其 `ISessionService` 消费者随后迁移到后续确定的自定义 P2P/平台大厅入口；中间态允许无法解析该接口。
- 删除 `ApplicationScope` 中的 `UnityProfileService` 注册及实现；不在根 Scope 注册替代资料服务，当前平台对应的只读资料实现随玩法场景注册。
- 暂不在本步骤处理 NGO `NetworkManager` 与项目侧 NGO `NetworkObjectFactory` 注册；它们在后续步骤单独分析。

#### 平台无关的只读资料契约

玩家昵称只用于 UI 和网络对象中的显示，不提供游戏内编辑。保留平台无关的 `IProfileService`，但收窄为只读昵称：

```csharp
public interface IProfileService {
    string Nickname { get; }
}
```

- 删除 Unity Authentication 专属的 `Profile` 属性。
- 删除未被调用的 `GetNicknameAsync()`。
- 删除 `Nickname` setter 和 `SetNicknameAsync()`。
- 当前 Steam 构建在 `GameplayScope` 中注册 `SteamProfileService`，由其实现该接口并读取 Steam 平台显示名。
- 未来 Epic 构建由玩法场景注册对应实现并提供同一契约；UI、玩法和网络对象不直接引用 Steam 或 Epic API。
- 平台实现若需要异步取得资料，应在玩法场景的平台初始化阶段完成并缓存；玩法场景中的 UI 仍使用同步只读属性。
- 从 MainScene 移除 `ProfileController` 及昵称展示；需要昵称的 UI 转移到玩法场景。
- `ProfileController` 后续改为只显示昵称，不再注册输入结束回调；因此失去消费者的昵称校验异常与本地化错误键在实施时一并清理。

#### 本步骤边界

- 本步骤只确定组合根最终删除项和资料契约，不迁移 `ISessionService` 消费者。
- 当前阶段只使用 Steam。平台组合在 `GameplayScope.Build()` 前确定，Scope 存活期间不支持 Steam/Epic 动态切换；未来需要更换平台时，通过退出并重新进入玩法场景、重建整个 `GameplayScope` 完成，不引入运行时代理或可变路由。
- 本步骤不处理网络中的玩家昵称同步方式；对应 NGO 消费者迁移时再决定。

### 第三步：先解除 ApplicationScope 的 NGO 根注册

#### 已确认决定

- 直接删除 `builder.RegisterInstance(NetworkManager.Singleton)`。
- 直接删除项目侧旧 `builder.Register<NetworkObjectFactory>(Lifetime.Singleton)`。
- 迁移期间不启动游戏，因此允许 `GameplayScope` 和旧玩法消费者暂时无法从容器解析这两个类型。
- 不增加兼容注册、空实现或临时 NGO/P2P 适配门面来维持中间态运行。
- 保留已有 P2P 类型本身，但按第五步将网络会话和网络对象子系统整体移入需要联机的玩法场景；`ApplicationScope` 不再注册 P2P 会话类型。

#### 后续迁移方向

- 旧 `NetworkObjectFactory` 的玩法消费者改用 `INetworkObjectManager.Spawn()` / `Despawn()`。
- `VContainerNetworkObjectFactory` 仅是 `NetworkObjectManager` 内部创建和释放 Unity Prefab 实例的适配器，不作为旧玩法 Factory 的替代注入类型。
- `NetworkManager` 消费者按实际职责分别迁移；会话、时间、场景、消息和玩家生命周期不能统一替换成某一个 P2P 类型。
- 所有消费者迁移完成后，删除项目侧旧 `NetworkObjectFactory.cs`；NGO `NetworkManager` 类型随 NGO 包和剩余引用一并移除。

#### 本步骤边界

- 解除注册发生在消费者迁移之前，项目中间态不可启动是已接受状态。
- 本步骤只决定 ApplicationScope 根注册，不在同一步修改 `GameplayScope` 或玩法消费者。

### 第四步：删除 GameplayScope 的 NGO PrefabHandler 接线

#### 已确认决定

- 删除 `_prefabHandlers`。
- 删除 `RegisterBuildCallback(OnBuilt)` 与 `RegisterDisposeCallback(OnDispose)`。
- 删除 `OnBuilt()` 与 `OnDispose()`，不再遍历 NGO `NetworkConfig.Prefabs`，也不再安装或移除 `GenericPrefabInstanceHandler`。
- 删除因此失去用途的 NGO、旧 Factory 和集合引用。
- 本步骤只删除旧 Handler 接线；P2P 对象子系统和场景所需 Prefab 的注册由第五步及后续资源步骤接管。
- 中间态不要求能够生成正式玩法网络对象，不为此保留 NGO Handler 接线。

#### 被移除逻辑原本承担的职责

- 为不同 Prefab 保存各自的生成/反生成处理器。
- 让生成对象使用当前 `GameplayScope` 作为父 Scope，从而解析场景级依赖。
- 通过每个 Handler 自己的栈缓存实例，反生成时停用并回收对象。

这些职责不会自动由当前 Root `VContainerNetworkObjectFactory` 覆盖。第五步通过将完整网络会话与网络对象子系统移入 GameplayScope 解决创建上下文与对象生命周期错位；是否恢复池化后续单独决定。

### 第五步：玩法场景拥有完整玩法与网络会话

#### 已确认生命周期模型

- 一个玩法场景承担一局完整玩法；同一局玩法不跨越多个 Unity Scene。
- 主菜单只负责选择并切换到具体玩法场景，不创建或持有 Lobby、Transport、Session 或网络对象世界。
- 玩家先进入目标玩法场景，再在该场景内创建或加入 Lobby，并始终在本场景完成本局联机。
- 玩家进入玩法场景后，由该场景内的流程决定创建只有自己的 Lobby 进入单机玩法，还是创建或加入允许其他玩家参与的 Lobby；主菜单不提前持有或协调该选择。
- 支持联机的玩法场景在构建自己的 `GameplayScope` 时预先注册完整 P2P 会话与网络对象子系统，不为联机过程额外创建场景内子 Scope。
- 注册不等于启动：刚进入玩法场景、尚未选择玩法方式时，网络栈保持未连接、未加入 Lobby；选择单机或联机并在本场景内创建或加入 Lobby 时才启动会话。全部已注册类型最终随 `GameplayScope` 统一释放。
- `GameplayScope.Awake()` 设置 `autoRun = false`，先调用 `base.Awake()` 建立 LifetimeScope 基础状态，再初始化当前平台，成功后才调用 `Build()`。这样平台账号 Id 在任何平台相关类型首次构造前已经有效。
- 第一阶段从 `SteamNetworkTransport` 中提取 `SteamClient.Init()` 及其一次性状态管理，提供可在 `GameplayScope.Build()` 前调用的平台初始化入口；Transport 不再负责首次初始化平台。
- 平台初始化失败时不构建玩法容器，不进入单机或联机会话；显示平台不可用错误并返回 MainScene。具体错误 UI 与返回流程在场景启动流程中落实。
- 平台选择在 `Build()` 前固定，当前 `GameplayScope` 生命周期内不允许动态切换。未来 Steam/Epic 切换必须先退出当前玩法场景，再以另一套平台注册重建新的 `GameplayScope`。
- 退出玩法场景即结束本局：离开 Lobby、停止 Transport、销毁 Session、消息管理器、全部网络对象和网络模块，不把连接或玩法资源带入下一个场景。
- `ApplicationScope` 不注册 P2P 会话类型，只保留应用启动、公共数据、场景切换和其他真正跨场景的应用级能力。
- `ApplicationScope` 不为昵称单独提前初始化 Steam/Epic 平台；首次进入需要平台能力的玩法场景时再确保平台就绪。Profile、Lobby、Transport 等服务实例属于该 `GameplayScope`，但 `SteamClient` 这类按进程只初始化一次的平台 SDK 状态不等同于网络会话生命周期。
- 不再采用 Root 代理 Factory、Root 持有 Transport 或运行时接管 Factory 的方案。

#### 第一阶段的单机模式

- 为减少首轮迁移代码，暂不实现 `LocalNetworkSession`、`LoopbackTransport` 或另一套离线组合根。
- 单机暂时复用完整 Steam 网络路径，定义为只有本地玩家的活动 Lobby/会话；玩法代码仍按普通会话、Owner 和网络对象规则运行，只是没有远端成员。
- 因此第一阶段的单机也依赖 Steam 初始化和平台可用性，不承诺离线运行。这是明确接受的阶段性限制，不增加兼容层掩盖。
- 未来确有离线需求时，再以 `INetworkSession`、`INetworkTransport` 等现有契约补充本地后端；不为这项未来可能性提前实现代码。

#### 玩法菜单的退出语义

玩法场景中的菜单提供三种彼此独立的退出操作：

- **断开连接**：离开当前 Lobby，结束当前 Session，断开全部远端连接并清理当前网络对象世界；保留当前玩法场景、`GameplayScope` 及其预注册类型，回到本场景的未开始/模式选择状态。再次选择单机或联机时建立新的会话。
- **返回主菜单**：若当前会话仍活动，先完成断开连接流程，再切换到 MainScene；玩法场景卸载时销毁其 `GameplayScope` 及全部场景资源。
- **退出游戏**：若当前会话仍活动，先完成断开连接流程，再释放玩法场景与应用级资源并退出进程。

没有活动 Lobby 时，“断开连接”不可执行；“返回主菜单”和“退出游戏”始终可用。三种操作共用同一个明确的会话停止入口，不各自复制 Lobby、Transport 和网络对象清理逻辑。

#### 支持联机的 GameplayScope 所拥有的完整网络栈

- `SteamNetworkLobby`
- `SteamNetworkTransport`
- `SteamProfileService` / `IProfileService`
- Build 前使用的 Steam 平台初始化入口；它不作为可在 Scope 内动态替换的平台路由
- `NetworkSession` / `INetworkSession`
- `NetworkMessageManager` / `INetworkMessageManager`
- 场景级 P2P Runtime，负责本场景 Transport、会话和网络模块的启动、更新与停止
- `VContainerNetworkObjectFactory` / `INetworkObjectFactory`
- 项目侧 `SteamNetworkObjectManager` / `INetworkObjectManager`
- `NetworkVariableModule`
- `NetworkRpcModule`
- GameplayScope 自己持有的固定步 `NetworkVariableModule.Flush()` 驱动
- 当前玩法场景所需网络 Prefab 的注册与注销

这些类型在联机玩法的 `GameplayScope` 内以该 Scope 的单例生命周期构造。`VContainerNetworkObjectFactory` 因此直接获得 GameplayScope 的 `IObjectResolver`，会话层与对象层也不会出现 Root 和场景 Scope 生命周期不同步。

#### 场景退出约束

- 阻止新的 Lobby、Session 和对象级操作，并停止该场景继续发送网络消息。
- 清理并释放该 `NetworkObjectManager` 管理的全部本地实例。
- 注销该场景的网络 Prefab。
- Dispose NV/RPC 模块、Manager 与 Factory，停止并释放消息管理器、Session、Transport 与 Lobby，然后释放该场景资源并销毁 GameplayScope。
- `NetworkObjectManager.Dispose()` 当前只注销消息与 Session 事件，实施时必须补齐全部存活对象的本地释放，或由同 Scope 的明确生命周期拥有者在 Manager Dispose 前完成等价清理。

#### 直接后果

- 当前 Root `P2PNetworkRuntime` 整体迁移到联机玩法的 `GameplayScope`，不再拆出 Root Transport 生命周期。
- Root `DefaultPackageManager` 不能继续注入 `INetworkObjectManager` 并注册全部 P2P Prefab。公共数据加载与玩法场景网络资源所有权需要分开，具体资源流程后续讨论。
- Root 类型不得解析 GameplayScope 中的会话或对象类型；玩法场景独立构造和释放自己的完整网络世界。

### 第六步：NetworkObjectManager 直接拥有场景级对象 Id 分配

#### 已确认决定

- 网络对象 Id 的唯一范围由“整个逻辑会话”收窄为“当前 GameplayScope 的网络对象世界”。
- 切换玩法场景时销毁旧场景全部网络对象和旧 `NetworkObjectManager`；新 Manager 可以从初始 Sequence 重新分配并复用旧场景 Id。
- 将本地 Sequence 状态和分配逻辑合并进 `NetworkObjectManager`，删除构造函数的 `INetworkObjectIdAllocator` 依赖。
- 删除 networking 包的 `INetworkObjectIdAllocator` 契约。
- 删除项目侧 `SteamNetworkObjectIdAllocator`、其 Root 注册、延迟解析 `INetworkObjectManager` 和 `Spawned` 水位订阅。
- 包侧 `NetworkObjectManager` 改为抽象类型；包仍统一拥有 Sequence、Id 组合、Spawn/Despawn、快照、Owner 转移与对象表，不把 Steam API 引入基础包。
- `NetworkObjectManager` 不声明用于读取 Prefix 的抽象属性或抽象方法。其 `protected` 构造函数接收 `uint localIdPrefix`，断言其非零后缓存到只读字段；后续分配始终使用该字段，以 `((ulong)localIdPrefix << 32) | sequence` 生成 Id，并断言 Sequence 有效。
- 除构造时传入 Prefix 外，不把 Spawn、Despawn、消息处理或所有权流程改为可重写；平台子类不能改变通用对象协议。
- 项目侧新增 `SteamNetworkObjectManager : NetworkObjectManager`；子类构造函数读取一次 `SteamClient.SteamId.AccountId`，并通过 `base(prefix, ...)` 连同基础依赖传入，由基类完成校验和缓存。基类构造过程不调用虚成员或抽象成员。
- `GameplayScope` 注册 `SteamNetworkObjectManager` 并暴露为 `INetworkObjectManager`；不再注册包侧 Manager 或独立 Allocator。
- 未来 Epic 等平台通过自己的 Manager 子类在构造时向 `base(prefix, ...)` 提供当前场景内唯一的 32 位 Prefix；UI 与包代码不感知具体平台。
- 更新 networking README 和项目网络 README，将 Id 语义从会话级稳定改为当前网络对象世界内稳定；Owner 转移仍不得改变对象 Id。

#### 场景切换边界

同一网络会话不跨场景存在。退出玩法场景时必须先停止并释放本场景的完整网络栈，再销毁 GameplayScope；下一个玩法场景建立的是新的 Lobby、Transport、Session 与对象世界，因此不需要为跨场景会话设计对象消息屏障或场景代际。

### 第七步：场景级 P2PNetworkRuntime 区分 Ready 与 Running

#### 已确认状态模型

```text
GameplayScope Build
→ Ready：平台已经初始化，Steam 平台回调持续驱动
→ Runtime.Start()：启动 Transport 与 NetworkVariable Flush
→ 创建或加入 Lobby
→ Lobby / Session 活动
→ 离开 Lobby，触发 Session.Stopped 并清理网络对象
→ Runtime.Stop()：停止 Transport 与 Flush
→ 回到 Ready，可再次选择单机或联机
```

- `P2PNetworkRuntime` 属于 `GameplayScope`，其依赖图在平台初始化成功后的 `Build()` 中构造。
- `IInitializable.Initialize()` 不再立即调用 `SteamNetworkTransport.Start()`；它只建立场景级 Ready 状态和整场景持续的平台回调驱动。
- `Start()` 在任何创建或加入 Lobby 操作之前调用。它启动 Transport、注册固定步 `NetworkVariableModule.Flush()`，成功后进入 Running；启动失败时回滚到 Ready，不能继续创建或加入 Lobby。
- 创建或加入 Lobby 失败时调用 `Stop()`，停止本次尚未形成会话的 Transport 与 Flush，并回到模式选择状态。
- 正常断开连接时必须先离开 Lobby；`NetworkSession` 统一断开其维护的全部远端 Peer，再发出 `Stopped` 以清理全部网络对象；随后才调用 `Runtime.Stop()` 关闭 Transport 设施与 Flush。
- `Stop()` 不销毁已注册的 Session、消息管理器、Manager、Factory 或 NV/RPC 模块；这些实例保留在当前 `GameplayScope` 中，供下一次会话复用，并最终随场景统一释放。
- `Dispose()` 是玩法场景卸载时的最终兜底停止，不替代正常的 Lobby 离开流程。
- Steam 平台回调在 Ready 和 Running 状态下都持续执行；断开连接后仍需接收邀请、Lobby 查询以及下一次创建/加入所需回调。Transport 的连接收发与平台级回调驱动在实施时分开，不能通过 `Transport.Stop()` 一并停止后者。

#### 职责边界

- Runtime 只管理技术栈的 Ready/Running 状态以及 Transport、Flush 的成对启停。
- Runtime 不持有 Lobby UI、创建参数、加入目标或场景切换职责。
- 玩法菜单或 Lobby 控制器负责按顺序调用 Runtime 与平台 Lobby API，并处理创建、加入失败。
- 单人 Lobby 与多人 Lobby 使用完全相同的 Runtime 启停流程。

### 第八步：SteamNetworkLobby 收回 Lobby 生命周期所有权

#### 已确认职责

- `SteamNetworkLobby` 自己持有当前 `Steamworks.Data.Lobby`，并提供创建、加入和离开的平台操作入口；外部类型不缓存用于清理的 Lobby 句柄。
- 新增 `CreateAsync(...)`、`JoinAsync(...)` 与 `Leave()`。具体参数只表达 Steam Lobby 所需事实，不混入 UI 控件、提示框或场景切换逻辑。
- `Leave()` 调用当前 Lobby 的平台离开操作，随后立即同步清空本地 Lobby、Owner 等状态并发出 `Stopped`。方法返回时本次同步停止事件链已经执行完成。
- Steam 回调报告本端已离开时，也复用同一个本地停止入口；已经清空的旧 Lobby 回调被忽略，不重复发出 `Stopped`。
- `Dispose()` 在仍有活动 Lobby 时调用同一个 `Leave()`，不另写一套销毁清理路径。

#### Controller 与 UI 调整

- 删除 `SteamLobbyController._leaveLobby` 及其“缓存句柄供 OnDestroy Leave”的补丁逻辑。
- `SteamLobbyController` 只编排命令顺序：创建或加入前先调用 `P2PNetworkRuntime.Start()`；创建/加入失败时调用 `Runtime.Stop()`；正常断开时按 `Lobby.Leave()`、`Session.Stopped`、`Runtime.Stop()` 的顺序完成。
- Controller 不直接订阅并维护 Steam Lobby 的底层生命周期事实，不拥有 Lobby 资源；场景销毁的最终清理由 `SteamNetworkLobby.Dispose()` 与 `P2PNetworkRuntime.Dispose()` 负责。
- `SteamLobbyCard` 等 UI 不再直接调用 `Steamworks.Data.Lobby.Join()` / `Leave()`，只把用户选择交给 Controller。
- 旧 UI 属于待迁移代码，不为其保留兼容 API、缓存句柄或旧事件流程；新玩法菜单只表达创建、加入、断开、返回主菜单和退出游戏意图。

#### 直接后果

- `SteamNetworkLobby` 在发出 `Stopped` 前先清空本地 Lobby 状态，因此 `NetworkSession.OnLobbyStopped()` 不能继续通过 `_lobby.Lobby.Members` 枚举远端。
- 本端离开会话时，逐 Peer 的逻辑断开由 `NetworkSession` 完成，网络对象清理由 `Session.Stopped` 触发；`P2PNetworkRuntime.Stop()` 随后只关闭监听、轮询和残余 Transport 设施，不代替 Session 的成员连接管理。

### 第九步：NetworkSession 统一拥有 Peer 连接与断开

#### 已确认职责

- `NetworkSession` 是 Lobby 逻辑成员与 Transport 物理连接之间的唯一协调者；同一个类型同时决定 `ConnectPeer()` 与 `DisconnectPeer()`，不把建立连接放在 Session、批量断开放到 Runtime。
- Lobby 启动时，Session 枚举当前远端成员并记录其 PeerId，然后逐个调用 `Transport.ConnectPeer()`。
- 远端成员加入时，Session 记录 PeerId 并发起连接；成员离开时，Session 移除 PeerId 并调用 `Transport.DisconnectPeer()`。
- Session 自己维护当前会话的远端 PeerId 集合，不能在 `SteamNetworkLobby.Stopped` 后重新读取已经清空的 Lobby 成员列表。
- 本端离开整个 Lobby 时，Session 逐个断开其记录的全部远端 Peer、清空集合，然后同步发出 `Session.Stopped`。
- 全会话停止导致的主动批量断开不再对外重复解释为一组普通 `MemberLeft`；`MemberLeft` 保留为活动会话中单个远端成员失去物理连接的事件。
- `SteamNetworkTransport.DisconnectPeer()` 必须完整终止指定 Peer 的已建立连接和仍在建立中的出站连接，不能只处理 `_connections` 中已经连通的项。

#### Runtime 边界

- `P2PNetworkRuntime.Start()` / `Stop()` 只成对启动和停止 Transport 整体设施及 NV Flush。
- `Runtime.Stop()` 在 `Session.Stopped` 之后执行，用于关闭监听 Socket、轮询及任何异常残留连接；正常的逐 Peer 断开已经由 Session 完成。
- `Runtime.Dispose()` 仍是场景销毁兜底，不成为第二套成员连接管理逻辑。

### 第十步：拆分 Steam 平台回调与 Transport 收发生命周期

#### 当前问题

`SteamNetworkTransport.Poll()` 当前同时执行 `SteamClient.RunCallbacks()`、Relay 初始化、出站连接收包和监听 Socket 收包。这样无法在断开会话并停止 Transport 后继续接收 Lobby 邀请、查询和下一次加入所需的平台回调。

#### 已确认职责

- `GameplayScope.Awake()` 在 `Build()` 前通过提取后的 Steam 平台初始化入口完成 `SteamClient.Init()`，并按进程只初始化一次 Relay 网络能力。
- `P2PNetworkRuntime.Initialize()` 在 GameplayScope Build 后注册场景级 Steam 平台回调驱动；只要玩法场景仍存在，无论 Runtime 处于 Ready 还是 Running，都持续执行 `SteamClient.RunCallbacks()`。
- `P2PNetworkRuntime.Dispose()` 在玩法场景退出时注销平台回调；`Runtime.Stop()` 不注销平台回调。
- `SteamNetworkTransport` 删除 `IInitializable`、`EnsureSteamInitialized()`、`s_steamInitialized` 和自身的 Relay 初始化职责。`Start()` 只断言平台已经就绪，不再隐式初始化 Steam。
- `SteamNetworkTransport.Start()` 创建 Relay Socket，并注册只负责 Transport 出站连接与监听 Socket 收包的轮询；成功后进入可收发状态。
- `SteamNetworkTransport.Stop()` 注销 Transport 收包轮询，关闭监听 Socket、出站连接和异常残留连接，并清理其运行时集合；它不停止 Steam 平台回调。
- Transport 的收包 `Poll()` 不再调用 `SteamClient.RunCallbacks()`，也不负责平台或 Relay 的一次性初始化。

#### 状态边界

```text
Ready   = Steam 平台与 Lobby 回调可用，P2P Socket 未启动
Running = Ready + Transport Socket/收包轮询 + NetworkVariable Flush
```

因此断开连接并回到模式选择状态后仍能查询 Lobby、接收邀请并开始下一次会话，同时不存在继续运行的 P2P Socket。

### 第十一步：使用 SteamNetworkingSockets 原生对称连接

#### 依赖与封装边界

- 不在项目侧通过比较 SteamID 指定唯一发起方；Lobby 内双方仍可同时调用 `ConnectPeer()`，由 SteamNetworkingSockets 的原生对称连接机制合并竞争连接。
- 当前由 `com.community.netcode.transport.facepunch` 间接带入的 Facepunch.Steamworks 版本不包含 `SymmetricConnect` 配置，且该包本身属于待移除的 NGO Transport 依赖；迁移时改为项目直接持有一个更新后的 Facepunch.Steamworks 依赖，避免 NGO 包继续决定 Steamworks 版本。
- 对 Facepunch.Steamworks 做最小源码补丁：生成的 SteamNetworkingSockets 绑定必须包含 `SymmetricConnect = 37`，并增加专用的对称 Relay 入口；不在游戏层复制 P/Invoke，也不通过运行时反射访问内部类型。
- 专用入口暂定为 `CreateRelaySocketSymmetric<T>(int virtualPort = 0)` 与 `ConnectRelaySymmetric<T>(SteamId peerId, int virtualPort = 0)`；二者都在创建 Socket/Connection 时传入 `SymmetricConnect = 1`，并使用相同的 Virtual Port。
- 保留 Facepunch 原有的普通 `CreateRelaySocket()` / `ConnectRelay()` 行为；对称模式只由项目明确选择，不修改第三方 API 的默认语义。

#### SteamNetworkTransport 调整

- `Start()` 改用对称 Relay Listen Socket，`ConnectPeer()` 改用对称 Relay Connection；不再增加“仅较小/较大 SteamID 主动连接”之类的项目侧选主规则。
- 同一 Peer 在 Transport 内只能对应一个逻辑连接和一次 `PeerConnected`。原生层解决物理连接竞争，Transport 仍需合并入站与出站回调，不能因两条回调路径重复覆盖连接或重复发出事件。
- 对称连接可能让一次出站调用直接采用已经到达的入站连接，随后排队的入站回调再执行 `Accept()` 时可能返回 `DuplicateRequest`；该结果视为已被原生层接管，不作为连接失败，也不创建第二条逻辑连接。
- 对称连接不使用 Linger 关闭；当前主动断开继续执行无 Linger 的 `Close()`，并统一清理活动连接与尚在连接中的出站管理器。

#### 验证边界

- 必须使用两个 Steam 客户端同时加入同一 Lobby，并让双方近同时调用 `ConnectPeer()`；每对 Peer 最终只能保留一个连接句柄，且每端只发出一次 `PeerConnected`。
- 额外覆盖连接建立中主动断开、断开后重新加入、`DuplicateRequest` 回调路径与场景退出清理。静态检查不能替代该双客户端运行时验证。

### 第十二步：统一 Peer 连接状态与断开所有权

#### 状态所有权

- `SteamNetworkTransport` 为每个 Peer 只创建一个内部 `PeerConnection` 状态对象，由它统一持有 PeerId、可选的出站 `ConnectionManager`、当前 `Connection` 句柄和 `Connecting` / `Connected` / `Disconnecting` 状态。
- 以 PeerId 为键的字典是当前 Peer 状态的唯一所有者；出站 `ConnectionManager` 只承载该连接的状态回调和关闭入口，不单独驱动收包。
- 入站与出站连接回调都绑定到同一个 `PeerConnection`。只有第一次从 `Connecting` 进入 `Connected` 时发出 `PeerConnected`，原生对称连接产生的重复回调只更新或核对同一状态，不重复发出事件。

#### DisconnectPeer 行为

- `DisconnectPeer(peerId)` 同时覆盖尚在连接的出站管理器和已经建立的连接，不再因活动连接字典中没有句柄而提前返回。
- 主动断开时先将对应 `PeerConnection` 从当前 Peer 字典移除并标记为 `Disconnecting`，随后关闭它持有的出站管理器和连接句柄；从移除开始，该 Peer 不再可发送数据，并允许同一 Peer 立即建立新的状态对象。
- 只有已经进入 `Connected` 的状态才同步发出一次 `PeerDisconnected`；连接建立前被取消不发出该事件，因为上层从未观察到 `PeerConnected`。
- `PeerConnection` 不进入第二个出站轮询集合，因此主动断开不需要维护延迟移除列表；回调期间的整体停止由 Transport 的待停止状态处理。

#### 延迟回调隔离

- 出站回调通过 `PeerConnection` 实例身份确认自己仍是该 Peer 的当前状态；入站回调通过连接句柄确认。仅凭 PeerId 处理断开回调是不安全的。
- 主动断开后到达的旧 `OnConnected` / `OnDisconnected` 回调只完成旧状态的清理，不得发出重复事件，也不得移除同一 Peer 已经创建的新连接。
- 远端断开与本地主动断开复用同一内部终结流程，由状态转换保证事件和清理都只执行一次。

### 第十三步：SteamNetworkTransport 整体停止与重启隔离

#### Stop 状态与可重入边界

- `SteamNetworkTransport` 显式维护 Running、正在执行收包轮询和待完成停止状态；`Stop()` 可重复调用，已经停止或正在停止时不重复关闭资源和发出事件。
- `Stop()` 一经调用便立即退出 Running 状态，使后续发送和连接请求失效。若此时不在 Transport 收包回调栈内，则同步完成资源关闭。
- 若 `Stop()` 从 `DataReceived`、连接事件或 `SocketManager.Receive()` 回调内部调用，只记录停止请求；同一次 Facepunch `Receive()` 内随后到达的消息回调不再向上层分发，待该调用返回后再在 `Poll()` 的 `finally` 阶段完成关闭。不得在 Facepunch 的原生收包回调栈内直接销毁正在收包的 Socket 或 ConnectionManager。

#### 完整关闭顺序

1. 注销 Transport 收包轮询；Steam 平台回调驱动保持运行，玩法场景仍处于 Ready。
2. 分离当前 Relay Listen Socket 的活动身份，使其随后到达的回调立即成为旧回调。
3. 对当前 Peer 字典中的每个 `PeerConnection` 调用与 `DisconnectPeer()` 相同的内部终结流程；正常路径中 Session 已经逐 Peer 断开，此处只处理异常残留。
4. 关闭 Relay Listen Socket，清空 Peer 状态和本轮停止标记。
5. 已经连接的异常残留仍按逐 Peer 规则发出一次 `PeerDisconnected`；从未连接成功的残留只清理资源，不发出事件。

#### 旧回调与再次 Start

- 每次 `Start()` 创建独立的内部 Relay Socket 回调适配器，由适配器持有对应 `SocketManager` 并向 Transport 转发；`SteamNetworkTransport` 不再直接充当所有代次 Socket 共用的 `ISocketManager`。
- 所有监听回调先确认其适配器仍是当前活动 Relay Socket；所有出站回调先确认其 `PeerConnection` 仍是当前 Peer 状态。旧代次回调不得接受连接、写入当前字典或发出事件。
- `Stop()` 完成后可在同一玩法场景重新 `Start()`。新旧代次可以收到延迟交错回调，但通过 Socket 适配器实例、`PeerConnection` 实例和连接句柄共同隔离，不引入额外的全局 Generation ID。
- 旧代次在停止后收到迟到的成功连接时，只关闭该旧连接；不得把它恢复为当前 Peer。

### 第十四步：统一使用 Relay Socket Poll Group 收包

#### 源码结论

- Facepunch 的 `ConnectionManager.Receive()` 调用 `ReceiveMessagesOnConnection()`，每次只消费一个连接的消息；`SocketManager.Receive()` 调用 `ReceiveMessagesOnPollGroup()`，一次消费其 Poll Group 内所有连接的消息。
- Facepunch 会在入站连接的 `SocketManager.OnConnected()` 中自动把连接加入该 SocketManager 的 Poll Group，但本地主动创建的出站连接不会自动加入。
- SteamNetworkingSockets 的两个接收 API 最终消费同一连接消息队列；同时轮询不会产生两份消息副本，但会让消息由先执行的一侧不确定地消费，并造成每个出站 `ConnectionManager.Receive()` 各自分配接收缓冲区的额外成本。

#### 唯一收包入口

- 在 Facepunch.Steamworks 最小补丁中，为 `SocketManager` 增加一个直接对应原生 `SetConnectionPollGroup()` 的窄入口，例如 `AssignConnection(Connection connection)`，只负责把指定连接加入本 SocketManager 已创建的 Poll Group 并返回成功与否。
- 入站连接继续由 Facepunch 自动加入 Poll Group；`ConnectRelaySymmetric()` 返回出站连接后，`SteamNetworkTransport.ConnectPeer()` 立即调用活动 Relay Socket 的 `AssignConnection()`。若原生对称连接采用了已经存在的入站句柄，重复设置到同一个 Poll Group 是幂等的。
- `SteamNetworkTransport.Poll()` 只调用一次当前 Relay `SocketManager.Receive()`；不再遍历调用各个 `ConnectionManager.Receive()`，也不维护出站轮询列表和延迟移除列表。
- `PeerConnection` 继续持有可选的出站 `ConnectionManager`，但其用途仅限连接状态回调、来源身份校验和关闭。

#### 消息与发送边界

- `SocketManager.OnMessage()` 使用消息携带的 Steam Identity 作为 `transportPeerId`，并核对该 Peer 当前状态为 `Connected` 且连接句柄一致；来自旧连接或未知连接的消息直接丢弃，不交给上层。
- 收到的原生数据复制到 Transport 复用缓冲区后，同步触发 `DataReceived`；该 `ReadOnlySpan<byte>` 仍只在回调期间有效。
- `SendData()` 只读取当前 `Connected` 状态保存的连接句柄。连接中、断开中或未知 Peer 不尝试发送；原生发送失败只记录 Transport 错误，连接生命周期仍以状态回调为准。
- `NetworkDelivery` 只决定发送时采用 `SendType.Unreliable` 或 `SendType.Reliable`。Facepunch 当前接收消息结构不暴露对应的发送投递标记，而且 `NetworkMessageManager` 接收端也不使用该参数，因此从 `NetworkDataReceivedHandler` 移除 `NetworkDelivery`，避免把所有入站消息错误标记为 `Reliable`；该枚举保留为发送侧参数。
- 当前 `INetworkTransport` 没有 RTT 查询契约，本阶段不新增 `GetRtt()` 或其他未被消费者需要的接口。

### 第十五步：传输层授权、失败处理与最终边界

#### 入站授权

- `SteamNetworkTransport` 维护独立的允许 PeerId 集合。该集合只表达当前 Session 声明的连接意图，不持有 Connection 或其他原生资源，因此不构成第二套连接所有权。
- `ConnectPeer(peerId)` 同时完成两件事：把 Peer 加入允许集合，并在当前没有连接状态时创建对称出站连接。Peer 已被允许但连接已经失败时，再次调用仍可重新发起连接。
- Relay Socket 收到入站 `OnConnecting` 时，只有 PeerId 已在允许集合内才接受；未知 Peer、已经撤销授权的 Peer 和本地 Peer 直接关闭，不创建 `PeerConnection`，也不发出上层事件。
- Lobby 事件与连接回调可能乱序。连接失败不会自动撤销允许状态，因此一侧较早发出的连接被另一侧暂时拒绝后，后者稍后执行 `ConnectPeer()` 发起的反向连接仍可被前者接受。
- `DisconnectPeer(peerId)` 先从允许集合移除，再终结当前 `PeerConnection`；这样成员离开后的在途入站连接和旧连接回调都不能重新建立连接。`Stop()` 清空全部允许状态。
- `NetworkSession` 仍是唯一调用 `ConnectPeer()` / `DisconnectPeer()` 的类型，授权事实来自当前 Lobby 成员关系；Transport 不直接读取或依赖 Lobby。

#### 原生失败处理

- `Accept()` 返回 `OK` 或对称连接竞争产生的 `DuplicateRequest` 都继续等待统一连接状态；其他结果关闭该句柄并记录英文错误日志。
- `ConnectRelaySymmetric()` 返回无效句柄或 `AssignConnection()` 失败时，关闭已经创建的原生资源并移除本次 `PeerConnection`，但保留允许状态供后续显式重试或反向入站连接使用；尚未 Connected 时不发出 `PeerDisconnected`。
- `SendMessage()` 的非 `OK` 结果记录错误并丢弃本次发送，不凭单次发送结果推断连接已经断开；连接终结仍只由 Steam 连接状态回调决定。
- 重复逻辑连接、当前 Peer 与回调实例或句柄不一致等本地状态违反使用断言表达；无效 Steam 身份、连接失败和远端断开属于外部结果，使用分支、日志和清理处理。

#### 最终 Transport 结构

```text
SteamNetworkTransport
├─ 当前 RelaySocketContext（每次 Start 独立）
│  └─ SocketManager + 唯一接收 Poll Group
├─ HashSet<ulong> allowedPeerIds
└─ Dictionary<ulong, PeerConnection>
   └─ PeerId + State + Connection + optional ConnectionManager
```

- `INetworkTransport` 最终保留 `LocalPeerId`、`Start()`、`Stop()`、`Poll()`、`ConnectPeer()`、`DisconnectPeer()`、`SendData()`、`PeerConnected`、`PeerDisconnected` 和不含 `NetworkDelivery` 的 `DataReceived`。
- `NetworkDelivery` 是发送策略，不属于接收事实；本阶段不增加 RTT、重连策略、超时策略或其他没有当前消费者的能力。
- Facepunch.Steamworks 补丁严格限定为：更新包含 `SymmetricConnect` 的生成绑定、增加对称 Relay 创建/连接入口、为 `SocketManager` 增加 `AssignConnection()`；不把项目会话或 Lobby 概念写入第三方封装。

#### 必须完成的运行时验证

- 两个 Steam 客户端近同时连接时，每对 Peer 只建立一个逻辑连接且每端只发出一次 `PeerConnected`。
- 未经 `ConnectPeer()` 授权的入站连接被拒绝；授权与连接乱序时最终仍能连接。
- 连接中主动断开不发出虚假的 `PeerDisconnected`；已连接后主动或远端断开只发出一次。
- 在 `DataReceived` 回调内停止 Transport 不销毁回调栈内资源，随后消息不再向上层分发。
- 同一玩法场景内执行 Stop、再次 Start 后，旧 Socket 和旧 Connection 的延迟回调不能影响新连接。

## 四、下一阶段：项目侧组合与玩法流程

底层 Transport 设计到此封闭。下一步回到 `GameplayScope`，依据已经确认的场景级生命周期列出需要预注册的具体类型、接口映射和启动入口，并逐项替换当前 Root/ApplicationScope 中的旧 NGO 与 Unity Services 组合。

### 第十六步：GameplayScope 从全部资源包注册网络 Prefab

#### 资源包与网络注册解耦

- `DefaultPackageManager` 只负责 DefaultPackage 的创建、初始化与销毁，不注入 `INetworkObjectManager`，不扫描或登记任何网络 Prefab，也不向玩法层提供网络 Prefab 列表；其他类型资源的加载职责不与本次网络注册重新绑定。
- Mod 可以各自创建并初始化 `ResourcePackage`；`NetworkPrefabRegistrar` 不依赖 `DefaultPackageManager` 或 `ModManager`，直接以 `YooAssets.GetPackages()` 作为当前全部资源包的权威来源。
- 通用 `AssetsUtility.LoadDataFromPackage()` 不再把 `network_prefab` 当作全局数据提前加载，也删除其中 NGO `NetworkManager.PrefabHandler` 接线。网络 Prefab 的加载、登记和 Handle 生命周期全部归场景级 Registrar。

#### NetworkPrefabRegistrar 生命周期

- `GameplayScope` 将 `NetworkPrefabRegistrar` 注册为场景级 EntryPoint，并注入同 Scope 的 `INetworkObjectManager`。
- Registrar 初始化时遍历 `YooAssets.GetPackages()` 中所有初始化成功的 `ResourcePackage`，查询各包的 `network_prefab` 标签，加载对应 Prefab，并只接受带 `Xoderony.Networking.NetworkObject` 的资源。
- 每个加载成功的 Prefab 立即向当前场景的 `INetworkObjectManager.RegisterPrefab()` 登记；Registrar 保存 Prefab 与对应 `AssetHandle`，确保整个 `GameplayScope` 生命周期内资源有效。
- Registrar 释放时先逐个 `UnregisterPrefab()`，再释放自己持有的全部 `AssetHandle`；不得销毁或卸载资源包本身，资源包仍由 DefaultPackageManager 或对应 Mod 所有。
- Mod 的启用与禁用只允许在主菜单进行；进入玩法场景后资源包集合固定。`NetworkPrefabRegistrar` 使用进入场景时已经初始化完成的资源包快照，不监听运行时包增删事件；变更 Mod 后由下一次进入玩法场景时的新 `GameplayScope` 重建注册表。

#### 启动顺序

- ApplicationScope 的异步启动阶段先完成 DefaultPackage 与已启用 Mod 的资源包初始化，随后才允许进入玩法场景。
- `GameplayScope.Build()` 构造场景网络栈，VContainer 初始化 `NetworkPrefabRegistrar` 完成全部 Prefab 登记；此时 `P2PNetworkRuntime` 仍处于 Ready，尚未启动 Transport 或加入 Lobby。
- 用户选择单机或联机后才进入 Running，因此网络会话开始前 Prefab 注册表已经完整。

### 第十七步：当前玩法的 GameplayScope 组合与可替换边界

#### GameplayScope 不是跨玩法基类

- 当前 `GameplayScope` 是当前这一种玩法场景的具体组合根，只承担该玩法所需的 UI、战斗、场景服务和网络栈，不承诺覆盖项目未来的所有玩法。
- 新玩法需要不同依赖图时，新增另一个直接继承 `LifetimeScope` 的具体 Scope，并由对应场景挂载；不要求继承当前 `GameplayScope`，也不为复用少量注册提前提取公共 GameplayScope 基类。
- 当前源码中没有任何其他 C# 类型直接依赖 `GameplayScope`；VContainer 父子关系依赖 `LifetimeScope`，`VContainerNetworkObjectFactory` 依赖当前容器的 `IObjectResolver`。因此场景可以切换具体 Scope 类型，不影响消费者契约。
- 某个玩法是否包含 Steam/P2P、使用哪些网络模块和 Prefab 集合，由该玩法自己的 Scope 决定；主菜单只切换场景，不解析或持有任何具体 GameplayScope。

#### 当前 GameplayScope 的普通 Singleton

- `SteamProfileService` → `IProfileService`
- `SteamNetworkTransport` → Self、`INetworkTransport`
- `NetworkMessageManager` → Self、`INetworkMessageManager`
- `VContainerNetworkObjectFactory` → `INetworkObjectFactory`
- `SteamNetworkObjectManager` → `INetworkObjectManager`
- `NetworkVariableModule`
- `NetworkRpcModule`

所有类型使用当前 `GameplayScope` 的 `Lifetime.Singleton`。Steam 平台初始化入口不注册到容器，因为它必须在 `Build()` 前执行。

#### EntryPoint 顺序

1. `SteamNetworkLobby` → Self：先订阅 Steam Lobby 回调。
2. `NetworkSession` → Self、`INetworkSession`：再订阅 Lobby 与 Transport 事件。
3. `NetworkPrefabRegistrar` → Self：遍历当前全部资源包并完成 Prefab 登记。
4. `P2PNetworkRuntime` → Self：最后进入 Ready，注册持续的平台回调驱动；其构造依赖 `NetworkVariableModule` 与 `NetworkRpcModule`，保证两个普通模块在任何会话开始前已经构造并完成订阅。

#### 从当前 GameplayScope 删除

- 删除 `UnnamedMessageBroker` EntryPoint 注册；Chat、HealthChange 与 Hit 等消费者后续改用 P2P 消息契约。
- 删除 `_prefabHandlers`、`RegisterBuildCallback(OnBuilt)`、`RegisterDisposeCallback(OnDispose)`、`OnBuilt()` 与 `OnDispose()`。
- 删除 NGO `NetworkManager`、旧 `NetworkObjectFactory`、`GenericPrefabInstanceHandler` 及相关集合引用和 `using`。
- 不注册已经删除的 `SteamNetworkObjectIdAllocator`；Id 分配由 `SteamNetworkObjectManager` 内部完成。
- UI、战斗、MessagePipe、场景组件等非网络注册暂时保持原样，随各自消费者迁移单独调整。

### 第十八步：静态 SceneTransitionService 与玩法场景启动

#### SceneTransitionService 不再进入 DI 容器

- `SceneTransitionService` 改为进程级静态单例，唯一持有当前场景 Handle 与场景切换状态；调用方通过 `SceneTransitionService.Instance` 发起切换，不再从 VContainer 注入。
- 从 `ApplicationScope` 删除 `SceneTransitionService` 的注册，并将现有消费者改为静态访问。该类型只负责场景切换，不持有玩法、平台或网络会话状态。
- 场景切换必须串行执行。同一次切换尚未完成时产生的返回主菜单请求先排队，待当前场景完成激活并更新当前 Handle 后再执行；不得并发操作两个 Scene Handle。

#### 玩法场景自行决定并初始化平台

- 主菜单只根据玩家选择加载对应玩法场景，不需要提前知道该玩法是否使用 Steam，也不传递平台元数据或建立额外的场景握手契约。
- 当前 `GameplayScope` 设置 `autoRun = false` 并重写 `Awake()`：先调用 `base.Awake()` 完成 `LifetimeScope` 自身初始化，再初始化当前玩法所需的 Steam 平台；只有初始化成功后才调用 `Build()`。
- Steam 初始化失败时不得构建当前玩法容器。场景直接显示错误，并通过静态 `SceneTransitionService` 返回 MainScene；若加载事务仍在进行，则由该服务按上述串行规则延后返回请求。
- Steam SDK 是进程级平台设施，成功初始化后不随 `GameplayScope` 销毁而关闭；离开玩法场景只释放该场景的 Lobby、Transport、NetworkSession、网络对象与 Prefab 注册资源。
- 未来其他玩法由各自具体 `LifetimeScope` 选择是否初始化 Steam、Epic 或不初始化任何平台；本阶段不支持运行时动态切换平台。

#### 下一项

回到 `ApplicationScope`，逐项确认移除 NGO、Unity Services、Profile 与 `SceneTransitionService` 注册后仍需保留的应用级注册和启动顺序。
