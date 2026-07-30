# Changelog

## [Unreleased]

- 在根命名空间 `Xoderony` 中新增按委托类型区分的订阅与派发通道。
- 将原有基础程序集统一合并为 `Xoderony.Foundation`，命名空间继续按职责组织。

## [0.1.0] - 2026-07-25

- 从项目 `Assets/Xoderony` 提取基础集合、通用扩展和非 Unity 对象池代码。
- 保留原有程序集名称和资源 GUID。
- 将依赖 Unity 序列化的 `ArrayList<T>` 移至 `Xoderony.Collections.Unity`。
