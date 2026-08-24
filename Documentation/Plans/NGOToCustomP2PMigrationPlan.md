# NGO 与 Unity Services 移除及自定义 P2P 迁移方案

更新时间：2026-08-24

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
- 删除 `UnityProfileService` 注册及实现，改为注册当前平台对应的只读资料实现。
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
- 当前 Steam 构建由 `SteamProfileService` 实现该接口并读取 Steam 平台显示名。
- 未来 Epic 构建由对应实现提供同一契约；UI、玩法和网络对象不直接引用 Steam 或 Epic API。
- 平台实现若需要异步取得资料，应在平台初始化阶段完成并缓存；MainScene 中的 UI 仍使用同步只读属性。
- `ProfileController` 后续改为只显示昵称，不再注册输入结束回调；因此失去消费者的昵称校验异常与本地化错误键在实施时一并清理。

#### 本步骤边界

- 本步骤只确定组合根最终删除项和资料契约，不迁移 `ISessionService` 消费者。
- 本步骤不决定 Steam/Epic 后端的运行时选择机制；当前先注册 Steam 实现，未来在组合根替换。
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
- 退出玩法场景即结束本局：离开 Lobby、停止 Transport、销毁 Session、消息管理器、全部网络对象和网络模块，不把连接或玩法资源带入下一个场景。
- `ApplicationScope` 不注册 P2P 会话类型，只保留应用启动、公共数据、场景切换和其他真正跨场景的应用级能力。
- 不再采用 Root 代理 Factory、Root 持有 Transport 或运行时接管 Factory 的方案。

#### 第一阶段的单机模式

- 为减少首轮迁移代码，暂不实现 `LocalNetworkSession`、`LoopbackTransport` 或另一套离线组合根。
- 单机暂时复用完整 Steam 网络路径，定义为只有本地玩家的活动 Lobby/会话；玩法代码仍按普通会话、Owner 和网络对象规则运行，只是没有远端成员。
- 因此第一阶段的单机也依赖 Steam 初始化和平台可用性，不承诺离线运行。这是明确接受的阶段性限制，不增加兼容层掩盖。
- 未来确有离线需求时，再以 `INetworkSession`、`INetworkTransport` 等现有契约补充本地后端；不为这项未来可能性提前实现代码。

#### 支持联机的 GameplayScope 所拥有的完整网络栈

- `SteamNetworkLobby`
- `SteamNetworkTransport`
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
- 抽象 Manager 只开放一个最小的 `protected abstract uint LocalIdPrefix { get; }` 扩展点。基础实现以 `((ulong)LocalIdPrefix << 32) | sequence` 生成 Id，并断言 Prefix 与 Sequence 有效。
- 除该 Prefix 外，不把 Spawn、Despawn、消息处理或所有权流程改为可重写；平台子类不能改变通用对象协议。
- 项目侧新增 `SteamNetworkObjectManager : NetworkObjectManager`，在构造时缓存 `SteamClient.SteamId.AccountId` 并实现 `LocalIdPrefix`。
- `GameplayScope` 注册 `SteamNetworkObjectManager` 并暴露为 `INetworkObjectManager`；不再注册包侧 Manager 或独立 Allocator。
- 未来 Epic 等平台通过自己的 Manager 子类提供当前场景内唯一的 32 位 Prefix；UI 与包代码不感知具体平台。
- 更新 networking README 和项目网络 README，将 Id 语义从会话级稳定改为当前网络对象世界内稳定；Owner 转移仍不得改变对象 Id。

#### 场景切换边界

同一网络会话不跨场景存在。退出玩法场景时必须先停止并释放本场景的完整网络栈，再销毁 GameplayScope；下一个玩法场景建立的是新的 Lobby、Transport、Session 与对象世界，因此不需要为跨场景会话设计对象消息屏障或场景代际。

## 四、下一项待讨论

分析场景级 `P2PNetworkRuntime`：确定预注册的 Transport、Session、消息与对象模块在创建或加入 Lobby 时如何按顺序启动，以及退出 Lobby 时如何停止并回到同场景单机状态；暂不展开 Lobby UI。
