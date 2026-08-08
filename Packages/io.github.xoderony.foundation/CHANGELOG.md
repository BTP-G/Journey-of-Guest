# Changelog

## [Unreleased]

- 在根命名空间 `Xoderony` 中新增按委托类型区分的订阅与派发通道。
- 将原有基础程序集统一合并为 `Xoderony.Foundation`，命名空间继续按职责组织。
- 对象池：新增 `CollectionPool<TCollection, TElement>` 作为 List/HashSet/Dictionary 共用基类，Stack/Queue 单独实现；所有集合池归还时自动清空元素、容量只读、不实现 IDisposable，统一 nullable 注解，移除构造时的 null 参数异常并改用调用方约定。
- `PooledObjectScope<T>` 改为 ref struct：只能存在于栈上，编译器禁止存入字段、容器或被捕获，避免作用域被复制后重复归还。
- 新增基础数值类型扩展：`SByteExtensions`、`ByteExtensions`、`Int16Extensions`、`UInt16Extensions`、`UInt32Extensions`、`Int64Extensions`、`UInt64Extensions`、`DoubleExtensions`、`DecimalExtensions`（有符号类型提供 Abs/Clamp，无符号类型提供 Clamp，Double 另含 Clamp01）。
- 集合池 `Return` 对 null 归还改用 `System.Diagnostics.Debug.Assert` 断言，便于开发期定位；运行时守卫保留。
- 移除未使用的 `IsExternalInit` 定义（包内无 init/record 用法，按程序集按需保留的原则不再携带）。

## [0.1.0] - 2026-07-25

- 从项目 `Assets/Xoderony` 提取基础集合、通用扩展和非 Unity 对象池代码。
- 保留原有程序集名称和资源 GUID。
- 将依赖 Unity 序列化的 `ArrayList<T>` 移至 `Xoderony.Collections.Unity`。
