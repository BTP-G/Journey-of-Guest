# Expriverse API

本包提供 Expriverse 可独立引用的运行时基础类型和 Mod 公共入口。

## 程序集

- `Expriverse`

程序集包含：

- `Expriverse.Core` 属性容器。
- 实体组件、生命周期和序列化契约。
- 伤害、治疗、命中及对应委托通道。
- NGO 未命名消息分发。
- 通用状态机、角色输入槽和属性类型。
- 交互与动画事件扩展契约。
- `Mod` 与 `IModManager` 公共入口。

## 依赖方向

`Assembly-CSharp` 和外部 Mod 可以依赖本包；本包不得引用 `Assembly-CSharp` 中的项目实现。

只使用上述扩展契约的 Mod 只需把 `Expriverse` 包及其清单依赖作为 API 边界，无需引用 `Assembly-CSharp.dll`。需要访问角色、Buff 实现、UI 或场景服务等项目内部类型的外部 Mod，才需要额外引用目标版本游戏生成的 `Assembly-CSharp.dll`。

`Assets/Mods/HudMod` 是不引用 `Assembly-CSharp`、只使用本包及公开第三方依赖的示例。
