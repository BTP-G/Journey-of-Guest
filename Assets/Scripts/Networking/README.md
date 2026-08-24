# 网络与会话

本项目当前同时存在 NGO/Unity Services 玩法网络和开发中的 `Xoderony.Networking` 包；两者不能混为一个已完成迁移的系统。

## 当前游戏接线（NGO）

- `JoG/Networking/SessionService.cs` 使用 Unity Services Multiplayer 创建、加入、查询和离开会话，以 Distributed Authority 网络和 Relay/QoS 选择低延迟区域。
- `JoG/Networking/AuthenticationController.cs` 作为 `IAsyncBootstrapModule` 完成匿名登录。
- `JoG/Networking/NetworkObjectFactory.cs` 管理 PrefabHandler 注册、移除和实例化；DA 模式下 owner 使用 LocalClientId。
- `JoG/Networking/GenericPrefabInstanceHandler.cs` 池化实例，并处理 LifetimeScope/Entity 父级注入；`NetworkPlayerPrefabHandler.cs` 处理 PlayerPrefab。
- `JoG/Networking/SessionOwnerObjectSpawner.cs` 只在 IsSessionOwner 时从 YooAsset 引用生成对象。
- 包内 `UnnamedMessageBroker` 使用 byte 类型和 FastBufferWriter/Reader，由服务端中继：1=Chat、2=HealthChange、3=Hit。
- 稳定启动链路为 Unity Services 登录 → `SessionService` → `SessionOwnerObjectSpawner` / `PlayerSpawner`。

## Steam 大厅与 P2P

本目录（`Assets/Scripts/Networking`，`JoG.Networking.P2P`）为 P2P 栈实现；NGO 接线见 `JoG/Networking/`。

三层拆分：`SteamNetworkLobby`（平台 Lobby 事实）→ `SteamNetworkTransport`（连接与收发）→ `NetworkSession` : `INetworkSession`（玩法层，组合前两者；`LocalPeerId` 取自 Transport，`IsOwner` 为 `OwnerPeerId == LocalPeerId`）。

- `SteamNetworkLobby`：纯 Steam Matchmaking；`Started`/`Stopped`/`OwnerChanged`/`MemberJoined`/`MemberLeft`（大厅成员）、`Lobby`、`LobbyDataChanged`/`LobbyMemberDataChanged`。
- Steam Lobby Data 仍是字符串 KV；`SteamLobbyDataExtensions` 对 `Steamworks.Data.Lobby` 提供 PlayerPrefs 风格的 `HasKey` / `Get*` / `Set*` / `TryGet*`（含 Member 变体）。缺键或解析失败时 Get 返回 default。Bool 写入 `"1"`/`"0"`。
- `NetworkSession`：Lobby 成员进出时直接 `ConnectPeer` / `DisconnectPeer`；对外 `MemberJoined` / `MemberLeft` 仍来自 Transport 的 `PeerConnected` / `PeerDisconnected`。
- 平台进出由 `SteamLobbyController`（或其它调用方）负责：`Join` / `LeaveCurrentLobby`；Lobby `Started`/`Stopped` 仅由回调收敛。换大厅时 Lobby 先停再启；Controller 经 `[Inject]` 取得 `SteamNetworkLobby`，并缓存 `_leaveLobby` 供销毁/退出时 `Leave`。
- `SteamNetworkObjectIdAllocator`：Id = `(Steam AccountId << 32) | Sequence`，本端自增，不经主机。订阅 `INetworkObjectManager.Spawned`，对本端 Account 前缀对象抬高 Sequence（重连恢复）；会话 `Stopped` 后 Sequence 重置为 1，重连后须先收到相关快照再 Spawn。（待优化：水位恢复/持久化等）
- `SteamNetworkTransport` 在每帧玩法 Update 前自行调用 `Poll`，处理收包与连接回调；NV 在固定步末尾按自身节奏发送，二者不共用调度时机。
- `Lobby/SteamLobbyController.cs` 只保留大厅配置、邀请和 UI 命令，并从 `SteamNetworkLobby` 读取 Lobby 状态。
- `ApplicationScope` 已注册 `SteamNetworkLobby`、`SteamNetworkTransport`、`NetworkSession`、Allocator、消息/对象管理、NV/RPC 模块和 `P2PNetworkRuntime`。模块以普通 Root 单例构造并订阅；Runtime 负责启动 Transport，并在固定步末尾每两个固定步调用 `NetworkVariableModule.Flush()`，失败时只禁用 P2P。
- `DefaultPackageManager` 在 `AssetsUtility.LoadDataFromPackage` 后登记带包级 `NetworkObject` 的 `network_prefab`，释放时先注销 P2P Prefab 再释放资源句柄。对象是否带 NV/RPC 能力组件不影响注册。
- `P2PValidationNetworkObject` / `P2PValidationVariables` / `P2PValidationRpcs` / `P2PValidationSpawner` 仅用于后续双 Steam 客户端验收，不属于正式玩法接线。验证 Prefab 的能力组件挂载和 YooAsset 重建仍待 Unity 侧完成。
- `UI/FacepunchTransportController.cs` 可手动切换 Transport 并 StartHost/Server/Client。
- Unity Services 是当前主路径；两条路径并存行为尚未验证。

## Xoderony.Networking 开发包

- `Packages/io.github.xoderony.networking` 是独立仓库 `Xoderony/io.github.xoderony.networking` 的本地 clone，程序集 `Xoderony.Networking`，不依赖 JoG、NGO 或 Steamworks。
- 主仓库通过 `.git/info/exclude` 忽略该目录，包代码应在自己的仓库提交；目前尚未接入游戏玩法。
- 包负责会话事实契约、消息路由、对象生成/销毁、Prefab、对象 id 解析、派生对象快照，以及可选的对象级 NetworkVariable/RPC 组件与模块。不提供具体 Lobby 或帧驱动策略；`Flush()` 由项目侧决定调用节奏。
- `NetworkObjectManager` 实现统一的 `INetworkObjectManager`，提供对象管理、生命周期事件与 id 解析；扩展模块不由 Manager 登记或驱动。本端对象权威比较使用 `INetworkSession.LocalPeerId`，对象本身不暴露 `IsOwner`。
- `NetworkObjectManager` 以 Session 的 `MemberJoined` 补发本端对象快照。`MemberLeft` 和 `OwnerChanged` 按当前源码将离开者拥有的全部对象转移给当前会话房主；玩家对象的延迟销毁由对象脚本决定，持久对象继续存在。源码没有 `PersistOnOwnerLeave` 分支。
- 本地 `Spawn` 接收已登记 Prefab，由 `INetworkObjectFactory.Create` 构造实例并在绑定前调用初始化委托；随后发送初始快照、绑定网络身份并发布 `Spawned`。远端先应用快照再绑定；`Despawned` 在从表移除后、解绑与工厂销毁前发布，回调期间对象仍持有网络身份。
- `NetworkObject` 在 `Awake` 中对本对象 `GetComponents<INetworkSynchronize>()` 收集快照贡献者并冻结数组；Spawn 与晚加入只遍历该数组。`NetworkVariableComponent` 实现此接口，RPC 组件不实现。自定义附加字节另挂贡献者组件，不从 `NetworkObject` 虚方法写入。
- `NetworkObject.Id` 是会话内稳定的 `ulong`，0 保留；项目侧当前编码为高 32 位 Steam AccountId、低 32 位本端 Sequence。当前权威身份独立存于 `OwnerPeerId`，NetworkVariable 与 RPC 当前收包不校验发送者是否为当前 Owner。
- PrefabId 为 `Animator.StringToHash(prefab.name)` 的 int，0 保留，冲突断言，YooAsset 预制体名必须全局唯一。
- Spawn/Despawn 临时发送缓冲使用固定容量 `stackalloc`；objectId 为 `ulong`；发送方组装 `type + payload` 完整消息，`NetworkMessageManager` 直接交给 Transport，接收方以 Transport 的直连 PeerId 作为发送者身份。

## 对象能力组件

- 项目 P2P 栈位于 `Assets/Scripts/Networking`，命名空间 `JoG.Networking.P2P`，已由 `ApplicationScope` 组合，但尚未替换当前 NGO 玩法主路径。
- `NetworkObject` 只保留身份、Owner、Prefab、Spawn/Despawn 和自定义快照。NV/RPC 是同 GameObject 上的可选组件：`NetworkVariableComponent` 与 `NetworkRpcComponent` 各最多一个，模块用 `TryGetComponent` 发现，不扫描子节点。
- 派生能力组件在 `Awake` 中按声明顺序收集端点并写入所属组件与 `byte` 索引；收集完成后结构不可修改。`enabled` 不改变协议结构。同一 Prefab 在所有 Peer 上必须具有相同组件类型、端点数量和收集顺序。
- `NetworkVariable<T>` 仅接受 `unmanaged`，值实际变化时置脏并触发 `ValueChanged`；默认编码由包内 `Serializer<T>`/`Deserializer<T>` 提供，自定义稳定协议须成对覆盖。
- `NetworkVariableModule` 在构造时订阅对象事件并注册消息 Handler。模块只维护本端拥有且待 Flush 的 `NetworkVariableComponent` 集合。`P2PNetworkRuntime` 在固定步末尾每两个固定步调用 `Flush()`。收包不校验发送者是否为当前 Owner。
- `NetworkRpcModule` 在构造时写入组件 Sender 并注册消息 Handler。`NetworkOthersRpc<T>`、`NetworkAllRpc<T>`、`NetworkOwnerRpc<T>` 分别固定发给 Others、All、当前 Owner，`NetworkPeerRpc<T>` 由调用方指定 Peer；收包经 `INetworkObjectManager` 查找后按 RPC 索引投递。RPC 只允许在对象完成 Spawn、模块已写入 Sender 后发送。
- Spawn 快照顺序为本对象 `INetworkSynchronize` 的组件顺序。NV 消息为 `type + objectId + variableIndex + payload`（类型 3），RPC 同为 `type + objectId + rpcIndex + payload`（类型 4）；应用消息仍从 `NetworkMessages.User`（16）起。
- Id 分配不占用 P2P 消息类型；本端 `Allocate`，项目侧经 `Spawned` 抬高水位。Spawn/Despawn/NV/RPC 消息中的 objectId 为 `ulong`。
- `NetworkObject` 不内置 Transform 同步；需要同步位姿时由具体项目对象或组件直接实现。

## 未完成项

- `SteamNetworkObjectIdAllocator` 待优化：水位恢复与持久化（如 PlayerPrefs）、与 ObjectManager 的循环依赖解法、会话停止是否重置 Sequence 等。
- Xoderony.Networking 与项目侧对象扩展尚未进行 Unity 编译/运行验证；LoopbackTransport 仍为空壳，P2P 双 Steam 客户端 Spawn/Owner 转移/清理验收仍待执行。NGO API 有疑问时应查当前包源码或对应版本官方文档，不依赖旧版本记忆。
