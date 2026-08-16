# 网络与会话

本项目当前同时存在 NGO/Unity Services 玩法网络和开发中的 `Xoderony.Networking` 包；两者不能混为一个已完成迁移的系统。

## 当前游戏接线（NGO）

- `SessionService.cs` 使用 Unity Services Multiplayer 创建、加入、查询和离开会话，以 Distributed Authority 网络和 Relay/QoS 选择低延迟区域。
- `AuthenticationController.cs` 作为 `IAsyncBootstrapModule` 完成匿名登录。
- `NetworkObjectFactory.cs` 管理 PrefabHandler 注册、移除和实例化；DA 模式下 owner 使用 LocalClientId。
- `GenericPrefabInstanceHandler.cs` 池化实例，并处理 LifetimeScope/Entity 父级注入；`NetworkPlayerPrefabHandler.cs` 处理 PlayerPrefab。
- `SessionOwnerObjectSpawner.cs` 只在 IsSessionOwner 时从 YooAsset 引用生成对象。
- 包内 `UnnamedMessageBroker` 使用 byte 类型和 FastBufferWriter/Reader，由服务端中继：1=Chat、2=HealthChange、3=Hit。
- 稳定启动链路为 Unity Services 登录 → `SessionService` → `SessionOwnerObjectSpawner` / `PlayerSpawner`。

## Steam 大厅

- `SteamNetworkSession` 是 Steam Lobby 状态的唯一所有者，实现包内 `INetworkSession`，并发布会话启停、成员加入/离开和 Owner 变化；Owner 离开时先发布 `OwnerChanged`，再发布 `MemberLeft`。
- `SteamNetworkObjectIdAllocator` 只依赖 `SteamNetworkSession`：Peer 以 Lobby Member Data 发布当前 `ReservedEnd`，Session Owner 在 Lobby 全局数据中按 SteamID 分别保存 `RangeId` 与新的 `ReservedEnd`；每次预留 `2^18` 个 Sequence，重连和重新加入从已预留上界继续。
- Allocator 首次取得区间后写入 Member Data `network.id.ready=1`；`SteamNetworkPeerConnector` 只依赖 Session 和 Transport，在本端 Ready 标志发布后连接 Lobby 中已 Ready 的成员，不直接依赖 Allocator。
- `Lobby/SteamLobbyController.cs` 只保留大厅配置、邀请和 UI 命令，并从 `SteamNetworkSession` 读取 Lobby 状态。
- Steam Runtime、Transport 监听与 VContainer 组合入口尚未接线；接线后应先启动 Steam 回调与 Relay Listener，再加入 Lobby，由 `SteamNetworkPeerConnector` 通过 Member Data Ready 标志建立出站连接。
- `UI/FacepunchTransportController.cs` 可手动切换 Transport 并 StartHost/Server/Client。
- Unity Services 是当前主路径；两条路径并存行为尚未验证。

## Xoderony.Networking 开发包

- `Packages/io.github.xoderony.networking` 是独立仓库 `Xoderony/io.github.xoderony.networking` 的本地 clone，程序集 `Xoderony.Networking`，不依赖 JoG、NGO 或 Steamworks。
- 主仓库通过 `.git/info/exclude` 忽略该目录，包代码应在自己的仓库提交；目前尚未接入游戏玩法。
- 包只负责会话事实契约、消息路由、对象生成/销毁、Prefab、对象 id 解析和派生对象快照，不提供具体 Lobby、NV、RPC 或帧驱动策略。
- `NetworkObjectManager` 实现统一的 `INetworkObjectManager`，提供对象管理、生命周期事件与 id 解析；扩展模块不由 Manager 登记或驱动。
- `NetworkObjectManager` 以 Transport 的 `PeerConnected` 补发本端对象快照，以 Session 的 `MemberLeft` 清理离开成员的对象；物理断线本身不再销毁对象，重连收到重复 Spawn 时刷新既有对象快照。
- 本地 `Spawn` 接收已登记 Prefab，由 `INetworkObjectFactory.Create` 构造实例并在绑定前调用强类型初始化委托；随后绑定网络身份、发送初始快照并发布 `Spawned`。远端同样经工厂创建；`Despawned` 在移除并解绑后、工厂销毁前发布，并携带解绑前的原 `uint Id`。
- `NetworkObject.OnSerializeSnapshot`/`OnDeserializeSnapshot` 只用于 Spawn 与晚加入，布局由项目派生类型拥有且必须成对。
- `NetworkObject.Id` 是会话内稳定的 `uint`，高 8 位为 Owner 分配且会话内不回收的 `RangeId`，低 24 位为该区间的 Sequence，0 保留；当前权威身份独立存于 `OwnerPeerId`，State/RPC/Despawn 接收时必须校验发送者为当前 Owner。
- PrefabId 为 `Animator.StringToHash(prefab.name)` 的 int，0 保留，冲突断言，YooAsset 预制体名必须全局唯一。
- Spawn/Despawn 临时发送缓冲使用固定容量 `stackalloc`；发送方组装 `type + payload` 完整消息，`NetworkMessageManager` 直接交给 Transport，接收方以 Transport 的直连 PeerId 作为发送者身份。

## JoG 对象扩展协议

- 项目实现位于 `Assets/Scripts/Networking`，使用 `JoG.Networking.P2P` 命名空间以避免与并存的 NGO 类型歧义，尚未接入当前 NGO RootScope。
- `JoGNetworkObject` 按需创建并直接保存有序 `NetworkVariableBase` 列表和 RPC channel handler 数组；变量与 handler 在 Spawn 前登记，快照覆写直接序列化变量，不经过全局映射。
- `NetworkVariable<T>` 仅接受 `unmanaged`，值实际变化时置脏并触发 `ValueChanged`；默认编码由包内 `Serializer<T>`/`Deserializer<T>` 提供，自定义稳定协议须成对覆盖。
- `NetworkVariableModule` 通过 `INetworkObjectManager` 的生命周期事件只维护本端拥有且包含变量的 `JoGNetworkObject` 列表，通过 VContainer `ITickable` Flush；每个对象仅在确认存在脏变量后 `stackalloc` 发送缓冲。
- `NetworkRpcModule` 不保存对象映射；收包经 `INetworkObjectManager` 查找后直接投递到 `JoGNetworkObject`，仅 owner 可发送。
- 项目 State 消息为 `type + Id + index + payload`（类型 `NetworkMessageType.User`），Rpc 消息为 `type + Id + channel + payload`（下一类型）；包内 Spawn/Despawn 仍为类型 2/3。
- Id Range 不占用 P2P 消息类型；Peer 在剩余 `2^16` 个 Sequence 时以当前 `ReservedEnd` 提前续租，Owner 只在请求值仍匹配已发布上界时预留下一块，Lobby Data 不进入对象生成热路径。
- `JoGNetworkObject` 不内置 Transform 同步；需要同步位姿时由具体项目对象或组件直接实现。

## 未完成项

Xoderony.Networking 与项目侧对象扩展尚未进行 Unity 编译/运行验证；LoopbackTransport 仍为空壳，P2P 会话的 VContainer 组合入口、Steam Runtime/Transport 启停和 Owner 转移后的对象权威迁移仍待实现。NGO API 有疑问时应查当前包源码或对应版本官方文档，不依赖旧版本记忆。
