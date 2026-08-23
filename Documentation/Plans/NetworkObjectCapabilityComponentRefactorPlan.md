# NetworkObject 对象能力组件化重构实施方案

更新时间：2026-08-24

状态：方案已确认，可交由其他模型实施。当前文件只记录实施要求；尚未按本方案修改源码、Unity Prefab 或 Scene，也未执行 Unity/Steam 运行验证。

## 一、目标与边界

### 本轮目标

- `Packages/io.github.xoderony.networking` 保留最小 `NetworkObject` 身份、Owner、Prefab、Spawn/Despawn 和自定义快照能力。
- 将通用 NetworkVariable、RPC、对应消息协议和运行模块从 `Assets/Scripts/Networking` 移入 networking 包。
- NetworkVariable 与 RPC 不进入 `NetworkObject` 基类，也不再通过 `JoGNetworkObject` / `ReplicatedNetworkObject` 继承组合。
- 增加两个对象级可选 Unity Component：
  - `NetworkVariableComponent`
  - `NetworkRpcComponent`
- 每个 `NetworkObject` GameObject 最多各挂一个上述组件；对象可以选择无能力、仅 NV、仅 RPC或二者都有。
- `NetworkVariableModule` / `NetworkRpcModule` 通过 `TryGetComponent` 判断对象是否具有对应能力。
- 移除 RPC/NV 包代码对 VContainer 的依赖；VContainer 只负责项目侧构造、释放和 PlayerLoop 接线。

### 本轮明确不做

- 不迁移当前 NGO 主路径。
- 不重构 `Packages/io.github.xoderony.jog/Runtime/Entities/Entity.cs`、`CharacterEntity`、`ProjectileEntity` 或正式玩法 Prefab。
- 不移除 `NetworkObject : MonoBehaviour`；Unity Prefab、GameObject 与组件仍是当前对象工厂边界。
- 不新增 `ReplicatedNetworkObject`、Provider 列表、多组件索引或泛型 `NetworkObjectManager<T>`。
- 不改变 Owner 离开统一转移策略。
- 不增加 RPC/NV sender 权限校验。
- 不处理 Steam 双向连接去重。
- 不执行 Git 提交、推送或清理用户现有改动。

## 二、最终对象模型

### 类型关系

```text
Xoderony.Networking.NetworkObject : MonoBehaviour
└── 未来的 JoG.Entity
    ├── CharacterEntity
    └── ProjectileEntity
```

RPC/NV 使用同 GameObject 上的能力组件，不进入上述继承链：

```text
GameObject
├── NetworkObject（或其派生类型）
├── 0..1 NetworkVariableComponent（或派生类型）
└── 0..1 NetworkRpcComponent（或派生类型）
```

允许的组合：

| Prefab 组件 | 网络能力 |
| --- | --- |
| `NetworkObject` | 基础对象功能 |
| `NetworkObject` + `NetworkVariableComponent` | 基础功能 + NV |
| `NetworkObject` + `NetworkRpcComponent` | 基础功能 + RPC |
| 三者都有 | 基础功能 + NV + RPC |

### 组件约束

- 两个能力组件均为公开、非抽象、可派生的 `MonoBehaviour`。
- 均添加 `[DisallowMultipleComponent]` 和 `[RequireComponent(typeof(NetworkObject))]`。
- 只允许与 `NetworkObject` 位于同一个 GameObject；模块使用 `networkObject.TryGetComponent(...)`，不扫描子节点。
- 基类允许直接挂载；不派生、不收集端点时表示空能力组件。
- 派生组件通过当前 `JoGNetworkObject` 相同的 `Collect...` 模式，以显式顺序登记端点。
- 端点数组在组件 `Awake` 中构建一次，此后结构不可修改；`enabled` 不改变协议结构。
- 同一 Prefab 在所有 Peer 上必须具有相同组件类型、端点数量和收集顺序。
- networking 包不得为一次性的 `Awake` 收集重新依赖 `Xoderony.ObjectPool.Generic`；直接使用短生命周期 `List<T>` 完成收集并转为数组即可。

## 三、NetworkVariableComponent

### 新类型职责

在 networking 包新增 `NetworkVariableComponent`，接管原 `JoGNetworkObject` 的全部 NV 对象级职责：

- 缓存所在 GameObject 的 `NetworkObject`；
- 以声明顺序收集 `NetworkVariableBase`；
- 检查最大数量和重复收集；
- 为每个变量写入所属组件与 `byte` 索引；
- 保存固定的变量数组；
- 序列化/反序列化全部变量，供 Spawn 和晚加入快照使用；
- 将 Dirty 通知转发给 `NetworkVariableModule`。

建议结构：

```csharp
[DisallowMultipleComponent]
[RequireComponent(typeof(NetworkObject))]
public class NetworkVariableComponent : MonoBehaviour {
    private const int MaxVariableCount = byte.MaxValue + 1;

    internal NetworkObject networkObject;
    internal INetworkVariableSyncScheduler scheduler;

    private NetworkVariableBase[] _variables = Array.Empty<NetworkVariableBase>();

    internal ReadOnlySpan<NetworkVariableBase> Variables => _variables;

    protected virtual void Awake() {
        // GetComponent<NetworkObject>()；收集、断言、直接写入 variable.component/index。
    }

    protected virtual void CollectNetworkVariables(List<NetworkVariableBase> variables) {
    }

    internal void MarkDirty() {
        scheduler?.Schedule(this);
    }

    internal void Serialize(ref BufferWriter writer) {
        // 按固定顺序序列化全部变量。
    }

    internal void Deserialize(ref BufferReader reader) {
        // 按相同顺序反序列化全部变量。
    }
}
```

字段赋值规则：

- 依赖和绑定状态优先使用包内可见字段，不增加只负责转交字段的 `Bind`、`Attach` 或 `InitializeReplication` 方法。
- `NetworkVariableBase` 改为直接保存：
  - `internal NetworkVariableComponent component;`
  - `internal byte index;`
  - Dirty 状态仍由变量自身保存。
- `NetworkVariableComponent.Awake` 直接写入 `variable.component` 与 `variable.index`。
- `NetworkVariableBase.MarkDirty()` 先标记 Dirty，再调用 `component.MarkDirty()`。
- Spawn 初始化委托发生在模块绑定 Scheduler 之前是正常状态：此时只修改值和 Dirty；初始快照会发送并清理该值。

### 派生组件示例

```csharp
public sealed class P2PValidationVariables : NetworkVariableComponent {
    public readonly NetworkVariable<int> SnapshotValue = new();

    protected override void CollectNetworkVariables(List<NetworkVariableBase> variables) {
        base.CollectNetworkVariables(variables);
        variables.Add(SnapshotValue);
    }
}
```

## 四、NetworkRpcComponent

### 新类型职责

在 networking 包新增 `NetworkRpcComponent`，对称接管原 `JoGNetworkObject` 的 RPC 对象级职责：

- 缓存所在 GameObject 的 `NetworkObject`；
- 以声明顺序收集 `NetworkRpcBase`；
- 检查最大数量和重复收集；
- 为每个 RPC 写入所属组件与 `byte` 索引；
- 保存固定的 RPC 数组；
- 保存由 `NetworkRpcModule` 写入的 Sender；
- 提供发给 Others、All、Owner、指定 Peer 的包内发送方法。

建议结构：

```csharp
[DisallowMultipleComponent]
[RequireComponent(typeof(NetworkObject))]
public class NetworkRpcComponent : MonoBehaviour {
    private const int MaxRpcCount = byte.MaxValue + 1;

    internal NetworkObject networkObject;
    internal INetworkRpcSender sender;

    private NetworkRpcBase[] _rpcs = Array.Empty<NetworkRpcBase>();

    internal ReadOnlySpan<NetworkRpcBase> Rpcs => _rpcs;

    protected virtual void Awake() {
        // GetComponent<NetworkObject>()；收集、断言、直接写入 rpc.component/index。
    }

    protected virtual void CollectNetworkRpcs(List<NetworkRpcBase> rpcs) {
    }

    // internal SendToOthers/All/Owner/Peer，转发给 sender。
}
```

字段赋值规则：

- `NetworkRpcBase` 改为直接保存：
  - `internal NetworkRpcComponent component;`
  - `internal byte index;`
- 各强类型 RPC 端点通过所属 `NetworkRpcComponent` 发送，不再依赖 `JoGNetworkObject`。
- RPC 只允许在对象完成 Spawn、模块已写入 Sender 后发送；不为 Spawn 前 RPC 增加恢复路径，按约定错误断言。

### 派生组件示例

```csharp
public sealed class P2PValidationRpcs : NetworkRpcComponent {
    public readonly NetworkAllRpc<int> Broadcast = new();

    protected override void Awake() {
        base.Awake();
        Broadcast.Received = OnBroadcastReceived;
    }

    protected override void CollectNetworkRpcs(List<NetworkRpcBase> rpcs) {
        base.CollectNetworkRpcs(rpcs);
        rpcs.Add(Broadcast);
    }
}
```

## 五、NetworkVariableModule

### 去除 VContainer 与 PlayerLoop 依赖

- 移入 networking 包。
- 删除 `using VContainer.Unity` 和 `IInitializable`。
- 构造函数直接完成以下订阅：
  - 注册 NV 消息 Handler；
  - 订阅 `INetworkObjectManager.Spawned`；
  - 订阅 `Despawned`；
  - 订阅 `OwnerChanged`。
- `Dispose` 继续成对注销并清空集合；`IDisposable` 不依赖 VContainer。
- 删除对 `Xoderony.Unity.PostUpdateLoop` 的直接依赖。
- 将当前发送循环改为公开 `Flush()`；`Flush()` 立即发送当前 Owner 的全部 Dirty 变量并清空调度集合，不在包内决定“每两个固定步”的项目策略。

### 数据结构与查找

- Dirty 集合由 `HashSet<JoGNetworkObject>` 改为 `HashSet<NetworkVariableComponent>`。
- `Schedule` 接收 `NetworkVariableComponent`，通过 `component.networkObject.OwnerPeerId` 判断本端 Owner。
- 所有对象事件与收包路径统一：

```csharp
if (!networkObject.TryGetComponent(out NetworkVariableComponent component)) {
    return;
}
```

- `OnObjectSpawned`：
  - TryGet 成功后写入 `component.scheduler = this`；
  - 组件存在变量时调用 `Schedule(component)`，保留初始/Bind 后 Dirty 收敛行为。
- `OnObjectDespawned`：从 Dirty 集合移除并清空 `component.scheduler`。
- `OnObjectOwnerChanged`：先移除，再按新 Owner 调用 `Schedule`。
- 收包：按 Object Id 找到 `NetworkObject`，TryGet NV 组件，再按单一 `byte index` 投递。
- sender 权限保持当前协议，不增加 Owner 校验。

## 六、NetworkRpcModule

- 移入 networking 包。
- 删除 `using VContainer.Unity` 和 `IInitializable`。
- 构造函数直接注册 RPC 消息 Handler，并订阅 ObjectManager 的 `Spawned` / `Despawned`。
- `OnObjectSpawned` 使用 `TryGetComponent<NetworkRpcComponent>`，成功后写入 `component.sender = this`。
- `OnObjectDespawned` 清空对应 Sender。
- 收包按 Object Id 找到 `NetworkObject`，再 TryGet RPC 组件并按单一 `byte index` 投递。
- SendToOwner 继续比较 `component.networkObject.OwnerPeerId` 与 `INetworkSession.LocalPeerId`；本端 Owner 直接 Dispatch，其余发送给对应 Peer。
- `Dispose` 成对注销 Handler 和对象事件。
- sender 权限保持当前协议，不增加接收端 Owner 校验。

`INetworkRpcSender` 与 `INetworkVariableSyncScheduler` 只服务包内组件/模块时改为 `internal`；RootScope 不再注册或解析这两个接口。

## 七、初始快照与协议

### 快照顺序

保留当前实际顺序：

```text
NetworkVariableComponent 的全部变量
→ NetworkObject.OnSerializeSnapshot 自定义数据
```

反序列化严格使用相同顺序。

在 `NetworkObjectManager` 增加私有快照辅助逻辑，所有路径统一调用，避免某条路径漏掉 NV：

- 本地 Spawn；
- 晚加入快照；
- 已存在对象收到重复 Spawn 快照；
- 远端首次 Spawn。

辅助逻辑先 `TryGetComponent<NetworkVariableComponent>` 并处理变量组件，再调用 `NetworkObject.SerializeSnapshot` / `DeserializeSnapshot`。`NetworkObject` 本身不保存变量数组、Scheduler 或 RPC Sender。

### 消息布局

保持扁平对象级索引，不增加 ComponentIndex：

```text
NV  = type + objectId + variableIndex + payload
RPC = type + objectId + rpcIndex + payload
```

- 每个对象最多 256 个 NV、256 个 RPC。
- NV 与 RPC 各自只有一个对象级组件，因此不需要二级索引。
- 发送缓冲容量和可靠投递策略保持当前实现。

### 消息类型

RPC/NV 进入基础包后不再占用应用侧 `NetworkMessageType.User`：

```csharp
NetworkMessageType.Spawn = 2;
NetworkMessageType.Despawn = 3;
NetworkMessageType.NetworkVariable = 4;
NetworkMessageType.Rpc = 5;
NetworkMessageType.User = 32;
```

本栈尚未形成兼容版本协议，本轮允许双端同步更新消息类型；不得只改单侧。

## 八、包内文件调整

### 移入 `Packages/io.github.xoderony.networking/Runtime/Replication`

从 `Assets/Scripts/Networking` 移动并改命名空间为 `Xoderony.Networking`：

- `NetworkVariableBase.cs`
- `NetworkVariable.cs`
- `NetworkVariableModule.cs`
- `INetworkVariableSyncScheduler.cs`
- `NetworkRpcBase.cs`
- `NetworkAllRpc.cs`
- `NetworkOthersRpc.cs`
- `NetworkOwnerRpc.cs`
- `NetworkPeerRpc.cs`
- `NetworkRpcModule.cs`
- `INetworkRpcSender.cs`

新增：

- `NetworkVariableComponent.cs`
- `NetworkRpcComponent.cs`

删除项目侧 `JoGNetworkObject.cs`；不在包内增加同义替代基类。

### 协议文件拆分

- 将 `NetworkObjectMessageType.NetworkVariable/Rpc` 合并到包内 `NetworkMessageType`。
- `NetworkObjectIdLobbyKeys` 仍是 Steam Lobby 项目实现，保留在 `Assets/Scripts/Networking`，将 `NetworkObjectProtocol.cs` 收窄/改名为 `NetworkObjectIdLobbyKeys.cs`。

### 包元数据与文档

- 更新 networking `package.json` 描述，删除“no RPC/NV”，说明 RPC/NV 是可选对象组件。
- networking 包不新增 VContainer 依赖。
- networking 包不依赖 `io.github.xoderony.unity`；固定步 PlayerLoop 驱动留在项目侧。
- 当前代码可继续放在现有 `Xoderony.Networking.asmdef`，不为本轮额外拆分程序集。
- 更新包 README：对象组合、模块构造/Dispose、`Flush()` 驱动、消息布局和最小示例。
- 移动已有 Unity 脚本时连同 `.meta` 一起移动并保留 GUID；全新脚本的 `.meta` 交给 Unity 生成。
- 只规范本轮修改文件的 UTF-8/LF/末尾换行，不批量处理包内其他文件。

## 九、项目侧组合调整

### RootScope

将 RPC/NV 模块从 VContainer EntryPoint 改为普通 Root 单例：

```csharp
builder.Register<NetworkVariableModule>(Lifetime.Singleton).AsSelf();
builder.Register<NetworkRpcModule>(Lifetime.Singleton).AsSelf();
```

删除：

```text
As<INetworkVariableSyncScheduler>()
As<INetworkRpcSender>()
```

`P2PNetworkRuntime` 构造函数直接依赖两个具体模块，确保在 Transport 启动和任何 Spawn 之前完成模块构造与事件订阅。

### P2PNetworkRuntime

- 保存 `NetworkVariableModule`。
- Transport 成功启动后，通过现有 `PostUpdateLoop<FixedUpdate.ScriptRunDelayedFixedFrameRate>` 注册项目侧回调。
- 回调保持当前策略：每两个固定步调用一次 `NetworkVariableModule.Flush()`。
- Dispose 时成对注销 PlayerLoop 回调，再停止 Transport。
- `NetworkRpcModule` 作为构造参数确保已构造，无需保存字段。
- `P2PNetworkRuntime` 仍可保留 VContainer `IInitializable`，因为它负责外部资源 Start，而不是单纯事件订阅。

### DefaultPackageManager

- Prefab 过滤从 `TryGetComponent<JoGNetworkObject>` 改为 `TryGetComponent<Xoderony.Networking.NetworkObject>`。
- 注册列表改为 `List<Xoderony.Networking.NetworkObject>`。
- 对象是否带 NV/RPC 不影响 Prefab 注册。
- 注销顺序和 YooAsset 句柄生命周期保持不变。

### VContainerNetworkObjectFactory

- 保持项目侧 VContainer 适配器，不移入基础 networking 包。
- Factory 仍返回 `NetworkObject`；能力组件由 ObjectManager/模块通过同 GameObject 的 `TryGetComponent` 发现。

## 十、P2P 验证代码迁移

### 类型调整

- `P2PValidationNetworkObject` 改为直接继承 `Xoderony.Networking.NetworkObject`。
- 新增项目侧：
  - `P2PValidationVariables : NetworkVariableComponent`
  - `P2PValidationRpcs : NetworkRpcComponent`
- SnapshotValue 移入 `P2PValidationVariables`。
- Broadcast RPC 与 Received 回调移入 `P2PValidationRpcs`。
- `P2PValidationNetworkObject` 可以缓存这两个同 GameObject 组件并保留 `SetSnapshotValue` / `SendBroadcast` 外观，减少 Spawner 调用变化。
- `P2PValidationPlayerObject` 的 Owner 转移后延迟 Despawn 逻辑保持不变。
- `P2PValidationPersistentObject` 行为保持不变。

### Unity Prefab 后续操作

代码实施后由用户在 Unity 中完成：

- 为 Player 验证 Prefab 挂载对应 `P2PValidationVariables` 与 `P2PValidationRpcs`。
- 为 Persistent 验证 Prefab 按验证范围挂载相同能力组件。
- 清理旧脚本引用并确认无 Missing Script。
- 确认每个 GameObject 最多一个 NV 组件、一个 RPC 组件。
- 重新构建包含这些 Prefab 的 YooAsset DefaultPackage。

实施模型不得擅自编辑 Prefab、Scene、YooAsset 配置或手工伪造新脚本 `.meta`。

## 十一、实施顺序

1. 在 networking 包实现两个能力组件，迁移 NV/RPC 端点类型并改为直接 `internal` 字段绑定。
2. 重构两个 Module：构造函数订阅、TryGetComponent、Dispose、`Flush()`；移除 VContainer/PlayerLoop。
3. 修改 `NetworkObjectManager` 的所有快照路径和内置消息类型。
4. 更新 networking 包 README、package.json 和 asmdef，确认包内无 VContainer/Xoderony.Unity 依赖。
5. 更新 RootScope、P2PNetworkRuntime、DefaultPackageManager 和项目侧协议键文件。
6. 重构 P2P 验证脚本并新增两个验证能力组件。
7. 更新 `Assets/Scripts/Networking/README.md`、`PROJECT_INDEX.md` 以及旧初始化方案中已被本文件取代的对象说明。
8. 只进行源码静态核对；Unity Prefab 和双端运行验证交给后续步骤。

## 十二、静态验收清单

- 有效 C# 中不存在 `JoGNetworkObject` 或 `ReplicatedNetworkObject`。
- networking 包有效 C# 中不存在 `VContainer`、`IInitializable`、`PostUpdateLoop` 或 `Xoderony.Unity`。
- `NetworkVariableComponent`、`NetworkRpcComponent` 均为对象级 `[DisallowMultipleComponent]`。
- `NetworkVariableModule` 的所有 Spawn/Despawn/OwnerChanged/收包路径都使用 `TryGetComponent<NetworkVariableComponent>`。
- `NetworkRpcModule` 的 Spawn/Despawn/收包路径都使用 `TryGetComponent<NetworkRpcComponent>`。
- `NetworkObjectManager` 的本地 Spawn、远端 Spawn、重复 Spawn、晚加入四类快照路径全部包含可选 NV 组件。
- RPC/NV 消息仍为 `objectId + byte index + payload`，没有增加组件索引。
- RootScope 不再注册 RPC/NV Sender/Scheduler 接口。
- P2PNetworkRuntime 明确构造两个模块，并在项目侧驱动 `Flush()`。
- DefaultPackageManager 注册所有带包级 `NetworkObject` 的 `network_prefab`。
- networking 包 README/source 对 RPC/NV 支持状态一致。
- 主项目和 networking 独立仓库的用户现有修改均被保留。
- `git diff --check` 仅在用户允许验证命令时针对本轮文件执行；不运行 Unity、Steam 或双端验证。

## 十三、运行验收边界

静态实现完成后仍必须由 Unity/双端环境验证：

- 新组件脚本导入及 Prefab 序列化；
- `TryGetComponent` 能获取派生的对象级能力组件；
- 初始快照、晚加入快照和 NV Delta 顺序一致；
- RPC Only、NV Only、二者都有、二者都无四种 Prefab 组合；
- 每两个固定步 Flush 的行为与原实现一致；
- Spawn、Despawn、Owner 转移及验证玩家延迟销毁；
- 双 Steam 客户端消息类型 4/5 一致。

源码静态完成不能表述为 Unity 或 Steam 运行验证完成。

## 十四、交给实施模型的执行指令

1. 先完整阅读项目根 `AGENTS.md`、`PROJECT_INDEX.md`、本方案、两个 Networking README、相关接口/实现/调用方。
2. 以当前源码为准；README 与源码冲突时先报告，不按旧文档反向实现。
3. 不重新发散已确认架构：对象级单例组件、TryGetComponent、扁平 byte 索引、包内无 VContainer 已确定。
4. 保留当前脏工作树和两个仓库中的用户修改，只处理本方案列出的文件和直接消费者。
5. 按第十一节顺序实施；完成一个模块后核对其全部调用方，再继续下一个模块。
6. 不修改现有 NGO Entity/Character/Projectile 正式玩法代码和 Prefab。
7. 不执行 Git 操作，不运行 Unity/Steam；验证命令仅在用户明确授权时执行。
8. 最终报告必须区分：包源码完成、主项目接线完成、Unity 资产待迁移、双端运行待验证。
