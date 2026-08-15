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

- `Lobby/SteamLobbyController.cs` 使用 Facepunch；`OnLobbyEntered` 中 transport 自动启动仍被注释，因此加入大厅不会自动联网。
- `UI/FacepunchTransportController.cs` 可手动切换 Transport 并 StartHost/Server/Client。
- Unity Services 是当前主路径；两条路径并存行为尚未验证。

## Xoderony.Networking 开发包

- `Packages/io.github.xoderony.networking` 是独立仓库 `Xoderony/io.github.xoderony.networking` 的本地 clone，程序集 `Xoderony.Networking`，不依赖 JoG、NGO 或 Steamworks。
- 主仓库通过 `.git/info/exclude` 忽略该目录，包代码应在自己的仓库提交；目前尚未接入游戏玩法。
- 包只负责会话消息、对象生成/销毁、Prefab、对象 id 解析和派生对象快照，不提供 NV、RPC 或帧驱动策略。
- `NetworkObjectManager` 实现 `INetworkObjectEvents` 与 `INetworkObjectResolver`；扩展模块只依赖窄契约，不由 Manager 登记或驱动。
- 本地 `Spawn` 接收外部已创建实例，远端收包经 `INetworkObjectFactory.Create`；`Spawned` 在快照完成后发布，`Despawning` 在解绑和销毁前发布。
- `NetworkObject.OnSerializeSnapshot`/`OnDeserializeSnapshot` 只用于 Spawn 与晚加入，布局由项目派生类型拥有且必须成对。
- `NetworkObjectId` 由 PeerId 和 Sequence 构成，载荷只传 Sequence。PrefabId 为 `Animator.StringToHash(prefab.name)` 的 int，0 保留，冲突断言，YooAsset 预制体名必须全局唯一。
- Spawn/Despawn 临时发送缓冲使用 `ArrayPool<byte>`，会话信封按会话常驻；应用协议自行选择缓冲和调度策略。

## JoG 对象扩展协议

- 项目实现位于 `Assets/Scripts/Networking`，使用 `JoG.Networking.P2P` 命名空间以避免与并存的 NGO 类型歧义，尚未接入当前 NGO RootScope。
- `JoGNetworkObject` 按需创建并直接保存有序 `NetworkVariableBase` 列表和 RPC channel handler 数组；变量与 handler 在 Spawn 前登记，快照覆写直接序列化变量，不经过全局映射。
- `NetworkVariable<T>` 仅接受 `unmanaged`，值实际变化时置脏并触发 `ValueChanged`；默认编码由包内 `Serializer<T>`/`Deserializer<T>` 提供，自定义稳定协议须成对覆盖。
- `NetworkVariableModule` 通过 `INetworkObjectEvents` 只维护本端拥有且包含变量的 `JoGNetworkObject` 列表，通过 VContainer `ITickable` Flush；每个对象仅在确认存在脏变量后 `stackalloc` 发送缓冲。
- `NetworkRpcModule` 不保存对象映射；收包经 `INetworkObjectResolver` 查找后直接投递到 `JoGNetworkObject`，仅 owner 可发送。
- 项目 State 帧为 `Sequence + index + payload`（类型 `NetworkMessageType.User`），Rpc 帧为 `Sequence + channel + payload`（下一类型）；包内 Spawn/Despawn 仍为类型 2/3。
- `JoGNetworkObject` 不内置 Transform 同步；需要同步位姿时由具体项目对象或组件直接实现。

## 未完成项

Xoderony.Networking 与项目侧对象扩展尚未进行 Unity 编译/运行验证；LoopbackTransport 仍为空壳，P2P 会话的 VContainer 组合入口仍待接入。NGO API 有疑问时应查当前包源码或对应版本官方文档，不依赖旧版本记忆。
