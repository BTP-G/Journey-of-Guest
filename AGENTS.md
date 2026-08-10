# Journey of Guest - Codex 项目规则

## 工作方式

- 默认使用简体中文；代码标识符、命令、路径、协议字段和第三方名称保持原文。
- 表达简洁并以决策和结果为导向；需求明确时直接实施，不止步于方案。
- 开始项目任务时先读根目录 `PROJECT_CONTEXT.md`，按任务导航定位入口；优先读取直接相关文件，信息不足时再用 `rg` 逐步扩大范围。
- `AGENTS.md` 只记录长期协作规则和稳定设计原则；`PROJECT_CONTEXT.md` 只记录项目事实、模块职责、关键入口和当前风险。
- 形成持久规则或架构事实时同步更新对应文档；一次性要求、未确认设想和临时调试结论不写入。发现内容过时则以代码和配置为准并修正文档；重大冲突无法判断时先询问用户。
- 遇到新问题时先提炼是否形成稳定规则：持久规则追加到 `AGENTS.md` 对应章节并在回复中说明"已新增规则"；一次性结论只进 `PROJECT_CONTEXT.md` 或代码注释。
- 规则冲突时以用户最新指示和代码/配置为准；发现 `AGENTS.md` 或 `PROJECT_CONTEXT.md` 过时，主动修正并提示用户。

## 项目与架构

- 这是 Unity 6 多人合作项目。具体 Unity 和包版本以 `ProjectSettings/ProjectVersion.txt`、`Packages/manifest.json`、`Packages/packages-lock.json` 为准。
- 项目业务代码使用 `JoG.*` 并保留在 `Assets/Scripts/JoG`，统一编入 `Assembly-CSharp`；供 Mod 独立引用的稳定 JoG 契约放入 `Packages/io.github.xoderony.jog` 的 `JoG` 程序集，且不得反向依赖 `Assembly-CSharp`。跨项目复用的基础设施优先进入 `io.github.xoderony.*` UPM 包并使用 `Xoderony.*`。`Xoderony` 是作者的 GitHub 用户名。
- 优先使用职责单一的组件扩展能力，保持调用链短、所有权明确；不要持续膨胀基础类或建立庞大统一上下文。
- 约定大于配置：能用清晰约定覆盖的边界不引入配置参数；可读性和性能优先，安全性其次。程序员可手动避免的异常不写防御性检查（如 null 参数守卫），以调用约定表达。
- 只靠约定出问题不好定位时加断言，不抛异常：用断言代替防御性抛异常，失败直接暴露位置；发布构建自动剥离，无运行时开销。可引用 Unity 的代码用 `UnityEngine.Debug.Assert`（`UNITY_ASSERTIONS`，编辑器与 Development Build 生效）；无 Unity 依赖的代码用 `System.Diagnostics.Debug.Assert`（`DEBUG`，默认仅编辑器生效）。
- 不为不存在或未证实的需求设计通用抽象：完全相同的代码可以抽基类复用，不通用的情况单独实现；允许少量直接重复，不为强行统一引入额外基类、回调参数或配置项。
- 角色由轻量状态机统一协调 Animancer、CharacterMotor、输入、玩法组件和网络同步，不另设并行的统一物理或动画控制器。
- 多个简单状态机可以并行或借助 GameObject 组合成层级。状态只处理自身行为并报告完成等事实；具体状态机决定状态转移。
- 网络同步位于状态机边界。状态可序列化自身额外数据，但不感知 RPC 或其他网络实现；本地所有者和远端实例尽量复用玩法逻辑。
- 不要依赖禁用 `NetworkBehaviour` 所在 GameObject 来切换状态或玩法内容；网络同步组件应持续可用。
- 实体能力不因需要挂载或注入而继承 `MonoBehaviour`；无 Unity 生命周期或场景引用需求时，优先使用可由 `Entity.Components` 序列化的普通 `IComponent`。只有需要 RPC、`NetworkVariable` 或 NGO 生命周期的组件才使用 `NetworkBehaviour`。
- 阵营关系由实体的 `Faction` 组件表达；Unity Tag 只用于 Unity 对象分类，不参与伤害、治疗、AI 选敌或目标统计等玩法敌我判定。
- `UnityEvent` 和 `UnityEvent2` 原则上只用于 UI 等 Inspector 表现层；核心玩法组件使用实体局部委托或消息通信，不序列化 UnityEvent。
- 属性等数据对象保持被动，不反向依赖玩法组件；需要把数值变化写入有状态系统时，使用职责单一的连接组件。
- 数值表示按语义选择：离散、协议或确定性数值优先整数或定点数，连续 Unity 数值使用浮点数。转换放在明确边界；泛型基础实现只共享类型确定的不变量操作，不用虚转换隐藏热路径。少量直接重复可用于换取更短调用链和清晰性能特征。
- 高频玩法与网络路径避免托管分配。
- 遍历集合时优先使用 `foreach`，并优先让遍历目标保持为数组、`Span<T>` 或具体集合类型；避免在高频路径通过 `IEnumerable<T>`、`IReadOnlyList<T>` 等接口枚举。只有确实需要索引时才使用 `for`。依赖注入边界必须使用集合接口时，尽早取得具体存储再遍历。
- 消除程序集循环时优先重新划分具体类型的模块归属，并把强耦合类型放在同一领域模块；不要仅为打破循环增加独立接口层。
- 模块采用职责明确且有自然扩展空间的领域命名，避免过大的聚合模块或过度细碎的程序集。
- 输入通道以 string key + 泛型 `InputChannel<T>` 表达（跨项目基础设施，核心在 `Xoderony.InputChannels`（Foundation 程序集），Unity 载荷如 `AimInput` 同命名空间放 `Xoderony.Unity` 程序集；key 常量 `InputKeys` 是玩法约定，放游戏项目 `JoG.Character`）；同一 key 的读写方必须约定相同的泛型类型（如 `InputKeys.Jump` 固定为 `InputChannel<bool>`），类型不一致由 Hub 的断言直接暴露。

## 状态机与网络协议

- 除非有意修改协议，同步状态 ID 使用 `sbyte`，状态自定义数据最大序列化大小为 `1024`。
- NGO RPC 数据优先使用 `FastBufferWriter`、`FastBufferReader` 和适当的原生容器实现零 GC 传输。
- 项目网络序列化不使用 `BytePacker` / `ByteUnpacker` 压缩；字段按其实际类型通过 `WriteValueSafe` / `ReadValueSafe` 读写。
- 状态序列化和反序列化回调必须由 `try/catch` 隔离。
- 自定义数据序列化失败时，状态标识仍须同步。
- RPC 与 `NetworkVariable` 只传协议类型（`int`、Q16、原生容器、Entity ID），不传引用类型与 ScriptableObject；跨端计算使用定点数保证结果一致。
- 只有权威端写 `NetworkVariable` 或发起玩法 RPC，远端只读并应用；`NetworkVariable` 变更回调内不重入写入。
- `HealthChangeMessage.Value` 负值为伤害、正值为治疗；攻击检测与伤害施加统一经 `JoG.Combat`（`HitQuery` 查询去重 + `CombatDamage` 施加，参数为正伤害量并内部取负）。Effects 内部已同步的效果用本地 `Route`，权威端生效的射弹与近战用 `Broadcast`。

## C# 与 Unity 编辑器

- 遵循根目录 `.editorconfig` 和附近 `JoG` 风格；当前 C# 文件使用 UTF-8（无 BOM）、LF，并保留末尾换行。
- API 标识符使用 `PascalCase`；私有实例字段使用 `_camelCase`；参数和局部变量使用 `camelCase`。
- 依赖注入优先使用 `[Inject]` 字段；只有需要在注入时组合、转换或立即执行初始化逻辑时才使用方法注入。
- 在语义等价且生命周期有保证时，优先通过成对注册/注销和输入值边界直接表达行为，避免额外状态标记与重复保护分支。
- 对仅承载少量值且不需要引用身份的类型，优先使用 `readonly struct`；设计集合、接口和委托路径时同时检查装箱与大结构体复制，确保确实减少堆分配。
- 编译期可确定且类型允许的稳定实现常量优先使用 `const`；其余固定、不可变的预定义静态值优先使用 `static readonly` 字段；只有值需要动态计算、反映可变状态或确实需要属性封装时才使用静态属性，不为固定值增加只读自动属性。
- 提取有业务意义的魔法数字和重复字符串；`0`、`1` 等结构值在命名反而降低可读性时可直接保留。
- 缩短变量生命周期，一个变量只承担一种用途。方法按获取、创建、配置、初始化、注册、组合、返回等清晰阶段排列。
- 需要把另一方法的返回值作为参数时，先使用含义明确的局部变量。
- 只有确实降低复杂度、减少有效重复或符合既有模式时才添加抽象；注释说明意图和约束，不复述代码。
- 方法签名清晰度优先于注释：参数语义、单位、表示方式或重载选择可能产生歧义时，先用清晰签名表达，仍不足再写 XML 注释。公共 API 的 XML 注释不是必须的，签名足够清晰时不加；确需注释时按对应标签书写（`<summary>`、`<typeparam>`、`<param>`、`<returns>`、`<remarks>`），不只写 summary。
- PropertyDrawer 优先使用 UI Toolkit `CreatePropertyGUI`，可行时保留行为一致的 `OnGUI`。
- 编辑器 UI 使用 `SerializedObject` / `SerializedProperty`，正确支持 Undo、多对象编辑、Prefab Override 和 `showMixedValue`。
- 程序化同步 UI 可能产生反馈循环或底层无法精确表示输入时，使用 `SetValueWithoutNotify`。
- 小而高频、非虚/接口重写且无隐式分配（装箱、闭包等）的简单方法，显式标记 `[MethodImpl(MethodImplOptions.AggressiveInlining)]`；新增代码时先检查是否符合条件，避免遗漏。
- 多次使用同一索引访问集合元素（如多次 `_states[i]`）时，先引入含义明确的局部变量；需要写回元素时使用 `ref` 局部。
- 事件订阅、委托注册必须成对退订/注销（`OnEnable`↔`OnDisable`、`OnSpawn`↔`OnDespawn`、`OnDestroy` 释放），禁止累积泄漏。
- `Awake` 只初始化自身，不依赖其他组件已执行 `Awake`；需要跨组件就绪的逻辑放 `Start`/`OnSpawn`。
- `Update`/`FixedUpdate` 内不做 `GetComponent`、`Find*`、`Resources` 查找；频繁访问的组件引用在初始化时缓存。
- 异步统一使用 UniTask，取消走 `CancellationToken`；禁止 `async void`（事件入口除外）和同步阻塞（`.Wait()`/`.Result`）。
- 热路径禁止字符串拼接/插值、LINQ（`Where`/`Select`/`ToArray` 等）、匿名函数捕获和装箱；需要拼接时使用 ZString。
- 高频对象使用 `ArrayPool`/`ListPool` 或对象池复用，热路径不 `new` 容器。
- 高频委托缓存为 `static` 或字段，不在每帧创建闭包。
- 热循环内不引入跳过分支：当被跳过项代价接近无操作（如乘 `Q16.One`）且分布不规则时，无分支连乘优于 `if` 提前跳过，避免分支预测失败。
- YooAsset 引用成对 `Load`/`Unload`（`Awake` 加载、`OnDestroy` 卸载），同一引用缓存复用，不重复加载。
- 日志统一走 `Xoderony.Logging`（`this.LogX`），业务代码不直接使用 `Debug.Log`。
- 对外 API 使用业务语义命名（如 `AddModifier`/`SetValue`），不用实现细节命名（如 `Slot`/`Index`/`Table`）。
- 返回实例句柄的 API 让调用方持有句柄操作（如 `Modifier.Remove()`），不返回裸索引让调用方自行配对。

## 修改与验证边界

- 修改前阅读现有实现，只改当前任务所需内容；优先选择范围小、风险低的方案，不顺带重构无关代码。
- 重构方案评估不被现有代码设计限制：若重写整个模块能更干净地实现目标，将其与局部修补方案一并列出，说明各自的改动范围、风险与收益权衡，由用户选择；实施仍以任务目标为界，不顺带改动无关内容。
- 只有用户明确要求时才执行或考虑 Git 相关操作；其他任务不检查、分析或汇报未提交状态，也不因工作区存在修改而暂停实施。
- 代码重构以目标架构和代码完整性为优先，不为现有 Prefab 序列化配置保留过渡兼容代码；Prefab 迁移由用户处理，除非用户明确要求同时修改。
- 对 Unity 刷新即可自动生成或清理的 `.meta` 文件，默认不手动创建、删除或改名；只有必须保留既有 GUID 和资源引用时才操作。若在 Unity 中刷新、重绑或迁移更简单，先告知用户并交由用户处理。
- 不为没有需求或证据的场景增加泛化异常处理、回退路径或预防性分支。
- 未经确认不做超出请求范围的大规模架构调整，不修改 Asset Store 插件或 Package Cache。
- 不编辑 `Library`、`Temp`、`Logs`、`obj` 等生成目录；保留工作区中用户的其他修改，不撤销、覆盖或批量格式化无关内容。
- 默认只做差异、引用和格式等轻量静态验证，不启动 Unity、不编译、不重新生成解决方案；只有用户明确要求时才执行完整验证。
- 未执行编译或运行验证时，在结果中明确说明。
- 用户要求 Git 操作时，分支使用 `codex/` 前缀。
- 提交按逻辑原子切分，不含生成目录与无关文件；提交消息格式「类型: 简述」（`feat`/`fix`/`refactor`/`chore`），中文描述。
