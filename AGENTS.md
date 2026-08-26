# Expriverse - 编码规范

## 规则适用与维护

- 默认使用简体中文；代码、命令、路径、协议字段和第三方名称保持原文。
- 用户最新指示和代码配置优先于本文档。
- 涉及陌生模块、架构或跨模块改动时，先读 `PROJECT_INDEX.md`、相关模块 README 与入口代码。
- `AGENTS.md` 只记录跨模块的稳定编码规范；具体协议、实现状态与模块事实以模块 README 和代码为准。
- 在对话中持续维护本文件：将已确认、可复用的跨模块约定及时新增或合并；发现规则过时、冲突或含糊时及时修订或删除，不记录一次性任务事实。

## C# 设计与实现

- 遵循 `.editorconfig` 和邻近代码风格；C# 使用 UTF-8 无 BOM、LF、末尾换行，方法签名不换行。
- 标识符遵循 .NET 命名；私有实例字段使用 `_camelCase`。
- 成员按常量、字段、属性/事件、构造、公开 API、私有实现排列；契约顺序一致，成对成员相邻。
- 依赖注入优先使用 `[Inject]` 字段；使用 VContainer 源生成器时，含 `[Inject]` 的目标类型和全部注入入口（构造函数、方法、字段及属性 setter）至少为 `internal`，不得为 `private`；仅需转换、组合或立即初始化时使用方法注入。
- 小型值类型优先 `readonly struct`；按读写语义使用 `in`/`ref`，避免装箱和不必要的大结构体复制。
- 编译期常量使用 `const`，其余固定值使用 `static readonly`；动态或需封装的值才使用静态属性。
- 类型内部读取或修改自身状态优先直接使用字段；属性用于对类型外暴露业务语义或必要封装，避免在类型内部经自动属性读写状态。
- 变量保持短生命周期和单一用途；嵌套调用结果先赋给语义明确的局部变量。
- 抽象只用于降低复杂度或有效重复；允许少量直接重复。
- 注释说明意图与约束，不复述实现；注释使用中文，日志使用英文并统一走 `Xoderony.Logging`。
- 无 Unity 依赖的程序集使用 `System.Diagnostics.Debug.Assert`，依赖 Unity 的程序集使用 `UnityEngine.Assertions.Assert`。
- 约定错误直接断言；预期或不可信输入使用分支，在分支内记录日志并返回。不要用 `if` 包裹断言。
- 使用对应的断言比较 API，例如 `AreEqual`；不使用 `IsTrue(a == b)`。断言参数不得包含后续仍需执行的赋值或副作用。
- 对外 API 使用业务语义并返回可操作句柄，不暴露要求调用方配对的实现索引。
- 生命周期有保证时通过输入边界与成对注册表达行为，不增加重复保护状态。注册、订阅、加载必须成对释放；频繁引用预先缓存。
- 异步使用 UniTask 与 `CancellationToken`；除事件入口外禁止 `async void`，禁止同步阻塞异步任务。
- 网络对象能力用同 GameObject 上的可选组件组合：`NetworkObject` 只保留身份，并在 `Awake` 中按本对象组件顺序收集 `INetworkSynchronize`；`NetworkVariableComponent` / `NetworkRpcComponent` 各最多一个，模块用 `TryGetComponent` 发现，不扫描子节点。`io.github.xoderony.networking` 的 RPC/NV 模块自行构造订阅与 Dispose，不依赖 VContainer；固定步 `Flush()` 由项目侧驱动。

## 性能与 Unity 编辑器

- 高频玩法与网络路径避免托管分配、LINQ 和动态字符串构造；复用容器与委托，字符串构建使用 ZString。
- 高频集合优先遍历具体存储；仅需索引时使用 `for`；重复索引先取局部变量，需要写回时使用 `ref` 局部。
- 热路径的小型非虚、非接口实现且无隐式分配的方法，标记 `[MethodImpl(MethodImplOptions.AggressiveInlining)]`。
- PropertyDrawer 优先 UI Toolkit，并保持 IMGUI 行为一致；编辑器 UI 使用序列化 API，支持 Undo、多对象编辑和 Prefab Override。

## 修改与验证

- 修改前阅读现有实现；只改任务所需内容，保留用户的其他修改，不顺带重构、格式化或修改外部插件与生成目录。
- `.meta` 默认交由 Unity 刷新；仅在必须保留 GUID 或资源引用时手动操作。
- 默认由用户、Unity 与 Visual Studio 的分析器负责验证；仅在用户明确要求时执行验证命令。
- Git 操作仅在用户明确要求时执行；提交遵循 Conventional Commits（英文 type/scope，中文标题/正文），正文逐项说明改动目的，具体内容由差异推导。
