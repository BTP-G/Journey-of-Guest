# Changelog

## [Unreleased]

- 将原有运行时程序集统一合并为 `Xoderony.Unity`。
- 将原有编辑器程序集统一合并为 `Xoderony.Unity.Editor`。
- 保留原有命名空间，以命名空间组织 API，以程序集表达运行时与编辑器依赖边界。
- 为 `Assembly-CSharp` 项目代码开启 `Xoderony.Unity` 自动引用。

## [0.1.0] - 2026-07-26

- 从 Assets 提取 Unity 相关基础设施。
- 将 Unity 组件统一迁入 `Xoderony.Unity`。
- 将可序列化 ArrayList 隔离到 `Xoderony.Collections.Unity`。
- 将通用 Q16 定点数及其 PropertyDrawer 迁入 `Xoderony.Numerics`。
