# Xoderony Unity

本包提供 Unity 序列化集合、Unity API 扩展、定点数值类型、PlayerLoop、Unity 对象池、通用组件、PropertyAttribute 和编辑器 UI 控件。

## 程序集

- `Xoderony.Unity`：全部运行时 API。
- `Xoderony.Unity.Editor`：全部仅编辑器 API。

命名空间继续按 API 职责组织；程序集只用于区分运行时与编辑器依赖边界。

## 依赖

- `io.github.xoderony.foundation`

运行时程序集开启自动引用，供 `Assembly-CSharp` 中的项目代码使用；拥有 asmdef 的使用方仍需显式引用。
