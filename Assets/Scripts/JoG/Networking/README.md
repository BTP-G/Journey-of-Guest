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
- `INetworkObjectManager`/`NetworkObjectManager` 负责对象生成、销毁、查找、状态/RPC 投递、晚加入快照与离开清理。
- 本地 `Spawn` 接收外部已创建实例，远端收包经 `INetworkObjectFactory.Create`；`SpawnLocal`/`DestroyLocal` 只负责绑定和解绑。
- 对象状态是由 `NetworkObject.Register`/`Unregister` 显式登记的 `NetworkVariableBase` 列表，按下标写入快照与 Flush；一帧多次置脏只在 Flush 发送最终值。
- `NetworkObject.Serialize`/`Deserialize` 只追加于 Spawn/晚加入快照，不参与 Flush，必须与变量列表顺序成对读取。
- RPC 使用对象通道 `Register`/`SendToOthers`，每次立即发送。
- State 帧为 `Sequence + index + payload`（类型 4）；Rpc 帧为 `Sequence + channel + payload`（类型 5）。Spawn/Despawn 为类型 2/3。
- `NetworkObjectId` 由 PeerId 和 Sequence 构成，载荷只传 Sequence。PrefabId 为 `Animator.StringToHash(prefab.name)` 的 int，0 保留，冲突断言，YooAsset 预制体名必须全局唯一。
- 状态自定义数据上限 1024。对象 Rpc/State 与 Spawn/Despawn 临时发送缓冲使用 `ArrayPool<byte>`；会话信封按 PayloadCapacity 常驻分配。
- 位姿变量在 Awake 登记一次，不随 Bind/Unbind 插拔。

## 未完成项

Xoderony.Networking 尚未进行 Unity 编译/运行验证；Loopback 样例会话 API 仍待迁移，游戏项目接线仍待办。NGO API 有疑问时应查当前包源码或对应版本官方文档，不依赖旧版本记忆。
