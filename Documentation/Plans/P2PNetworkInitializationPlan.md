# P2P 网络栈项目侧初始化实施方案

更新时间：2026-08-23

状态：方案已确认，可以进入实现。当前尚未修改相关源码，也未执行 Unity、Steam 或双端运行验证。

> 2026-08-24 更新：本文中的 `ExpriverseNetworkObject`、RPC/NV 继承组合、Prefab 过滤和验证对象实现说明，已由 [NetworkObjectCapabilityComponentRefactorPlan.md](NetworkObjectCapabilityComponentRefactorPlan.md) 取代。应用初始化、资源、场景、Popup、Root Runtime 和验证边界仍以本文为准。

## 实施原则

- 不替换现有 NGO / Unity Services 主路径。
- 将 `Assets/Scripts/Networking` 与 `Packages/io.github.xoderony.networking` 作为并行 P2P 技术验证栈接入。
- 第一阶段只完成最小双端 P2P 联调闭环，不扩展协议安全、完整玩法对象注入或连接去重机制。
- 实现前按 `AGENTS.md`、`PROJECT_INDEX.md`、相关 README 和当前源码确认契约；保留用户其他修改。
- 按模块分阶段修改。静态完成不代表 Unity 编译、Prefab 序列化或双 Steam 客户端运行验证完成。

## 一、应用初始化入口

### RootScope 接替 BootstrapManager

- 删除独立 `BootstrapManager` 后，由 `RootScope` 统一负责应用启动编排。
- 初始化入口继续位于 `Assembly-CSharp`。
- 保留 `VContainerSettings.RootLifetimeScope`。RootScope 使用 Prefab，由 VContainerSettings 自动实例化并跨场景保留。
- RootScope 不直接放入 BootstrapScene，避免与 VContainerSettings 创建的实例重复。
- 保留 RootLifetimeScope 设置，使官方场景和 Mod 场景在 Parent 为空时自动继承根容器，无需引用项目 `RootScope` 类型。

### autoRun 与 Awake

- RootScope Prefab 的 `autoRun` 保持为 `true`，用于触发 VContainerSettings 自动创建根实例。
- `RootScope.Awake()` 在实例上先将 `autoRun` 改为 `false`，再调用 `base.Awake()`，避免基类立即构建容器。
- 随后由 RootScope 启动私有异步初始化流程；禁止 `async void`，Unity 生命周期入口只负责启动并集中观察 UniTask 异常。

### 启动顺序

```text
VContainerSettings 创建 RootScope
→ RootScope 使用 Unity 原生图形 API 显示初始化 UI
→ UnityServices.InitializeAsync
→ RootScope.Build
→ VContainer 执行同步 IInitializable
→ 并发启动并等待全部 IAsyncBootstrapModule
→ DefaultPackageManager 已完成默认数据及 P2P Prefab 注册
→ 从 YooAssets 获取 DefaultPackage
→ Additive 加载并激活 MainScene
→ 将 MainScene 设为 Active Scene
→ 卸载 BootstrapScene
→ 结束初始化 UI
```

- 容器内各 `IAsyncBootstrapModule` 不建立显式先后顺序，通过 `UniTask.WhenAll` 统一等待。
- 当前模块包括 `AuthenticationController`、`ModManager`、`DefaultPackageManager`。
- P2P Prefab 注册属于 `DefaultPackageManager` 自身初始化工作，不增加第二套异步模块排序系统。

### 启动失败策略

- Unity Services 和 DefaultPackage 是进入主菜单的必要条件，失败时由初始化 UI 表达失败，并进入后续确定的重试或退出流程。
- Steam / P2P 是并行技术验证栈。其初始化失败时记录错误、禁用本次运行的 P2P 功能，但不阻断 NGO 主路径和主菜单进入。
- 第一阶段不为各失败类型设计复杂恢复状态机；实现应保证异常被集中观察，不能由 `async void` 静默丢失。

## 二、资源与场景边界

### Player 内置内容

- Player 只内置 BootstrapScene、RootScope Prefab、启动代码和必要配置。
- 从 `EditorBuildSettings` 移除 MainScene；BootstrapScene 保持唯一启动场景。
- 初始化期间 UI 使用 Unity 原生图形 API 绘制，不引用项目字体、图片、材质、动画或普通 UI Prefab。
- RootScope Prefab 不直接或间接引用项目美术资源及 YooAsset 管理资源。
- BootstrapScene 和 RootScope 到 YooAsset 内容只保存 package/location 等间接标识，不建立 Unity Object 强引用。

### YooAsset DefaultPackage

- 仅使用离线模式，不实现远端版本、联网下载或下载重试。
- MainScene、运行期 Popup、主菜单 UI、美术资源及与玩法场景共享的资源进入 DefaultPackage。
- “可更新”指资源包化后随 Steam / 客户端内容更新替换，不表示运行时联网热更新。
- 主菜单与玩法内容共享资源只由 YooAsset 资源域持有，避免 Player 内置副本与 AssetBundle 副本并存。
- 主菜单和运行期返回主菜单均通过 `YooAssets.GetPackage("DefaultPackage")` 获取包，再调用 `ResourcePackage.LoadSceneAsync`；`DefaultPackageManager` 不提供 Package 访问门面。

### DefaultPackageManager

- 继续负责创建、初始化、持有和释放离线 DefaultPackage。
- 继续先调用 `AssetsUtility.LoadDataFromPackage(package)`，保留现有 NGO Prefab 与数据注册行为。
- 紧接该调用之后，遍历 `network_prefab` 资源：
  - 只处理带包级 `NetworkObject` 的 Prefab；
  - 调用 `INetworkObjectManager.RegisterPrefab` 注册 P2P Prefab；
  - 保存注册对象及必要的 YooAsset 句柄；
  - Dispose 时先 `UnregisterPrefab`，再释放对应句柄和默认数据。
- 不增加独立的 P2P Prefab 异步启动模块。

## 三、场景切换与 Scene Scope

### 默认切换方式

- 使用 `LoadSceneMode.Additive`：先加载新场景，再卸载旧场景，不引入中间场景。
- 加载新场景时可传入 `allowSceneActivation: false`，旧场景继续运行并显示自身 Loading。
- 新场景准备完成后允许激活，等待新场景 Awake 和 Scene Scope 构建完成。
- 将新场景设为 Active Scene，再关闭旧场景输入、Camera、UI，最后通过旧 `SceneHandle` 卸载旧场景。
- 场景重叠期间避免同时启用两套 Camera、AudioListener、EventSystem 和输入控制器。
- 接受切换瞬间同时持有新旧场景资源的内存峰值；只有未来大型场景切换产生实际压力时再改为先卸载后加载。

### 场景切换所有权

- 场景句柄和切换编排由 RootScope 中无美术依赖的场景切换对象持有，确保其生命周期跨越旧 Scene Scope 销毁。
- Scene Scope 只负责本场景 UI 和表现，不持有跨场景切换的最终所有权。
- 联机玩法场景的多客户端同步仍应走对应网络场景流程；本方案不以本地 YooAsset 切换绕过 NGO / P2P 会话协调。

## 四、Popup 管理

### 生命周期和注册

- 不建立统一 `IPopupService`。
- Loading、Confirm、Toast 按类型去中心化管理，调用方只依赖自己实际使用的 Popup。
- 运行期 Popup 由 MainSceneScope / GameplaySceneScope 注册，不进入 RootScope。
- 每个 Scene Scope 只注册该场景实际需要的 Popup 类型。
- Popup Prefab、字体、图片、材质和动画随对应场景由 YooAsset 加载。
- RootScope 服务不得依赖 Scene Scope Popup；Root 服务返回结果或异常，由场景控制器决定 UI 呈现。

### 三类 Popup 行为

- Loading：场景内引用计数；每次显示返回独立且只能释放一次的句柄。
- Confirm：同时只显示一个请求；并发请求进入队列；Scene Scope 销毁时取消未完成请求。
- Toast：由场景内控制对象持有模板和对象池；随 Scene Scope 销毁。
- 初始化阶段原生 UI 与运行期 Loading Popup 是不同资源域和生命周期，不复用 Prefab。
- Additive 切换期间旧场景 Loading 可持续显示，直到新场景激活；不因此将普通 Loading 提升到 RootScope。

## 五、容器结构和注册方式

- 最大容器层级保持 `Root → Scene → Object`。
- 不为网络功能增加 `NetworkScope`。
- Lobby Start / Stop 是可重复的逻辑会话生命周期，不创建 DI Scope。
- 只有未来出现必须随每次 Join / Leave 整体构造和释放的服务集合时，才重新评估 SessionScope。
- Scope 与对象的结构性依赖默认使用显式代码注册。
- 需要 Unity 身份、引用或生命周期的对象行为使用 MonoBehaviour，并由所在 Scope 显式注册。
- `[SerializeReference] List<IComponent>` 只用于真正需要 Prefab 选择类型和参数的纯 C# 可选能力，不作为通用 DI 注册入口。

## 六、P2P Root Runtime

### 生命周期归属

- `SteamNetworkLobby`、`SteamNetworkTransport`、`NetworkSession`、消息管理、对象管理、ID 分配器、PeerConnector、RPC 和 NetworkVariable 模块全部归 RootScope。
- MainScene 只负责 Lobby UI 和 Create / Join / Leave 命令。
- GameplayScene 只消费已经启动的 P2P 服务。
- 不让 BootstrapScene、MainSceneScope 或 GameplaySceneScope 持有 P2P 全局生命周期。

### P2PNetworkRuntime

- 在 RootScope 注册专用同步 EntryPoint `P2PNetworkRuntime`（名称可在实现时按邻近风格调整）。
- 该类型通过构造或注入确保消息管理、对象管理、ID 分配器、PeerConnector、RPC/NV 模块被实际构造。
- 初始化时调用 `SteamNetworkTransport.Start()`；`Initialize()` 本身只注册 PlayerLoop Poll，不等价于启动 Steam。
- RootScope 销毁时停止 Transport、注销回调并释放 P2P 组件。
- Steam / P2P 启动异常由 Runtime 捕获并标记 P2P 不可用，不能阻断 NGO 主路径。

### 第一阶段对象创建边界

- 最小验证用 `NetworkObject` 只允许依赖根容器服务；NV/RPC 由同 GameObject 上的可选能力组件提供。
- `INetworkObjectFactory` 第一阶段只需完成 Prefab 实例化以及 RootScope 注入。
- 暂不解决网络动态对象从当前 GameplaySceneScope 获取场景服务的问题。
- 完成双端闭环后，再单独设计玩法对象、场景对象和跨场景对象的注入父级。

## 七、网络对象离开策略

- 成员离开时，其拥有的全部网络对象统一转移给当前会话 Owner。
- 玩家操控对象转移 Owner 后，由玩家脚本实现延迟销毁。
- 世界道具等持久对象转移给会话 Owner 后继续存在。
- 不增加 `PersistOnOwnerLeave` 分支；以当前源码统一 TransferOwnership 的方向为准。

## 八、明确暂缓事项

### 双向连接去重

- 当前不新增确定性发起方或额外逻辑连接去重。
- 暂按 Steam 传输层实际行为运行，在双端测试中记录是否出现重复 `PeerConnected` / `MemberJoined`。
- 若验证出现重复逻辑加入，再基于证据设计 Transport 或 PeerConnector 层收敛策略。

### RPC / NetworkVariable sender 权限

- 本轮不增加“sender 必须是对象当前 Owner”的接收端校验。
- 技术验证对象不得依赖尚未实现的权威安全保证。
- 后续若玩法依赖权威写入，再单独确定协议权限策略。

## 九、实施阶段

### 阶段 1：启动与资源边界

- RootScope 接替 BootstrapManager。
- 实现原生初始化 UI 和集中异常观察。
- 保留 RootLifetimeScope 自动创建。
- MainScene 加入 DefaultPackage 并从 Player 场景列表移除。
- 使用 YooAsset Additive 加载 MainScene，卸载 BootstrapScene。

### 阶段 2：Scene Scope Popup 与切换

- 删除 PopupManager 的独立 `DontDestroyOnLoad` 所有权和 RootScope 查找注册。
- 将 Loading、Confirm、Toast 拆为场景内注册和独立管理。
- 迁移调用方，移除 Root 服务对 Popup 的依赖。
- 增加跨场景 SceneHandle 编排，更新所有返回主菜单路径。

### 阶段 3：P2P Root 组合

- 注册缺失的 P2P 服务与 `P2PNetworkRuntime`。
- 启动并释放 SteamNetworkTransport。
- 完成 RootScope 注入的对象工厂。
- 在 DefaultPackageManager 中注册 / 注销 P2P Prefab。

### 阶段 4：最小双端验证对象

- 增加最小 `NetworkObject` 派生验证 Prefab，并按验证范围挂载 `NetworkVariableComponent` / `NetworkRpcComponent` 派生组件。
- 覆盖 Spawn、初始快照、NetworkVariable、RPC、Despawn。
- 覆盖成员离开、Owner 转移、玩家对象延迟销毁和持久对象保留。

## 十、验证清单

### 静态与 Unity 验证

- Unity 完成编译，无 VContainer 注册或源生成错误。
- RootScope 只创建一次，BootstrapScene 不存在第二个根容器。
- Player Build Settings 只内置 BootstrapScene。
- DefaultPackage 能在 Editor Simulation 和离线 Player 模式加载 MainScene。
- MainScene / GameplayScene 切换时新旧 Scene Scope 构建和释放各一次。
- 场景切换期间无重复 Camera、AudioListener、EventSystem 持续存在。
- Popup 的 Prefab 和美术依赖不进入 BootstrapScene / RootScope 依赖链。

### P2P 双端验证

- 两个 Steam 客户端创建 / 加入同一 Lobby。
- RangeId Ready 后建立逻辑连接；记录是否发生重复 MemberJoined，本轮不预先修复。
- 双端完成 Spawn、初始快照、NetworkVariable、RPC、Despawn。
- 成员离开后完成 Owner 转移。
- 玩家对象由玩法逻辑延迟销毁，持久对象继续存在。
- 离开或切换 Lobby 后停止连接并清理会话对象。
- Steam / P2P 初始化失败时，NGO 主路径仍能进入主菜单。

## 十一、主要入口文件

- `Assets/Scripts/BootstrapManager.cs`
- `Assets/Scripts/Expriverse/RootScope.cs`
- `Assets/Scripts/Expriverse/IAsyncBootstrapModule.cs`
- `Assets/Scripts/Expriverse/DefaultPackageManager.cs`
- `Assets/Scripts/Expriverse/Utilities/AssetsUtility.cs`
- `Assets/Scripts/Expriverse/LifetimeScopes/MainSceneScope.cs`
- `Assets/Scripts/Expriverse/LifetimeScopes/GameplaySceneScope.cs`
- `Assets/Scripts/Expriverse/UI/Popup/PopupManager.cs`
- `Assets/Scripts/Expriverse/UI/Popup/LoaderPopup.cs`
- `Assets/Scripts/Expriverse/UI/Popup/ConfirmPopup.cs`
- `Assets/Scripts/Expriverse/UI/Popup/ToastPopup.cs`
- `Assets/Settings/AssetBundleCollectorSetting.asset`
- `Assets/Settings/VContainerSettings.asset`
- `ProjectSettings/EditorBuildSettings.asset`
- `Assets/Scripts/Networking/SteamNetworkTransport.cs`
- `Assets/Scripts/Networking/SteamNetworkLobby.cs`
- `Assets/Scripts/Networking/NetworkSession.cs`
- `Assets/Scripts/Networking/SteamNetworkObjectIdAllocator.cs`
- `Assets/Scripts/Networking/SteamNetworkPeerConnector.cs`
- `Assets/Scripts/Networking/P2PValidationNetworkObject.cs`
- `Packages/io.github.xoderony.networking/Runtime/INetworkObjectManager.cs`
- `Packages/io.github.xoderony.networking/Runtime/NetworkObjectManager.cs`
- `Packages/io.github.xoderony.networking/Runtime/InstantiateNetworkObjectFactory.cs`

## 十二、实施边界

- 不在第一阶段替换 NGO、删除 Unity Services 或改写既有网络协议。
- 不提前实现双向连接去重。
- 不增加 RPC/NV sender 权限系统。
- 不建立 NetworkScope、SessionScope 或统一 Popup 门面。
- 不顺带重构无关模块、外部插件或生成目录。
- 未经用户明确要求，不执行 Git 提交或推送。
