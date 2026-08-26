# 角色、状态、属性与生命

## 入口与边界

- 实体契约位于 `Packages/io.github.xoderony.expriverse/Runtime/Entities`：`Entity` 为每个实体创建 VContainer 子作用域，注册子 GameObject 上的 `IComponent` 以及 `Entity.Components` 中的 SerializeReference 组件，并转发生命周期、权威和同步事件。
- `CharacterEntity.cs` 缓存 Animator、Animancer、CharacterMotor、Rigidbody，并解析 Health、Effects、模型和消息 Router 等角色能力。
- `CharacterSpawner.cs` 以 Owner 可写的 `NetworkVariable<NetworkObjectReference>` 管理 Body；公开入口为 `TrySpawnBody`/`TryRecycleBody`，只允许权威端调用。
- `PlayerSpawner.cs` 创建玩家角色并在 `OnBodyAssigned` 时补满生命；`AI/EnemySpawner.cs` 负责敌人重生和随难度施加 MaxHealth/AttackPower 效果。

## 输入、状态与移动

- 输入使用 `Xoderony.InputChannels` 的 string key + `InputChannel<T>`；key 常量在 `InputKeys.cs`，包括 Move、Aim、Jump、Sprint、PrimarySkill、SecondarySkill、SpecialSkill、Interact。
- `AimInput` 位于 `Xoderony.Unity`；同一 key 的读写方必须约定相同泛型类型。
- 包内 `StateMachine`/`MonoStateMachine`/`NetworkStateMachine` 提供状态边界；`CharacterRootStateMachine.cs` 根据 LifeStart/LifeStop 切换生死状态。
- `Character/States/CharacterLocomotionStateMachine.cs` 根据 Motor 稳定性和 Move 输入切换 Air/Move/Idle，目前未挂到 Prefab。
- 角色状态统一协调 Animancer、CharacterMotor、输入、玩法组件与网络同步，不另设并行的统一物理或动画控制器。

## 属性与生命链路

- 包内 `Stat` 使用 int 基础值/边界/当前值和 Q16 倍率槽；写入立即触发 `ValueChanged`，`Value` 按需重算，在消费边界转 float。
- `Stat.AddModifier` 返回 `StatModifier` 实例句柄，通过 `SetValue`/`Remove` 操作；槽位 API 仅包内可见。
- `HealthComponent` 保存 Current/Max/Ratio/IsAlive，Owner 写 NetworkVariable；Max 改变时按比例缩放 Current。
- 稳定链路：`Damageable`/`Healable` → `HealthChangeRouter` → modifier → `HealthComponentChangeResolver` → report 委托与 `IPublisher<HealthChangeReport>`。
- `HealthChangeMessage.Value` 负值表示伤害、正值表示治疗。`HealthChangeReport.value` 是修改后的请求值，`deltaValue` 是实际生命变化，消费方必须按语义选择。
- `Faction` 的整数 Id 决定友军/敌军；伤害、治疗和 AI 选敌不得使用 Unity Tag 判断阵营。
- `CharacterLifeController` 处理零点跨越；`CharacterMaxHealthController`、`CharacterHealthRegenerationController`、`CharacterHitImpulseController` 分别连接最大生命、回复和权威端击退。
- `CharacterHurtBoxLifeController` 与 `CharacterMotorNetworkController` 承接旧 `CharacterBody` 的 HurtBox 和 Motor 网络生命周期。

## 当前 Prefab 迁移风险

- 8 个角色 Prefab（Fighter、Ghost、Golem、GiantDummy、MegaspikanLarvae、Mage、Skeleton、Spitter）仍有已删除脚本留下的 Missing Script；8 个 Prefab 合计存在 12 个不同 GUID，其中 5 个为共有项。`PlayerObject.prefab` 另有 1 个，`PlayerCharacterOverlay.prefab` 有 7 个，需在 Unity 刷新后确认。
- 新的 Health、Faction、Life、Motor、Hit、Effects、Inventory 等拆分组件尚未完整挂载；`Entity.Components` 仍有空引用，Stat 尚未完成配置。
- 旧 `CharacterBody` 仍挂载，迁移完成前不得与相同职责的新拆分组件同时启用。
- Animator 与 Animancer 暂时并存。
- 当前 Prefab 仍由 `CharacterMoveInputHandler` 写 CharacterMotor；新 Locomotion 状态机未挂载，两套逻辑不得同时驱动 Motor。
- 输入 Driver 仍保留在角色 Prefab 以兼容旧序列化；`CharacterInputBinding` 优先使用 Spawner Driver，无则回退 Body 旧 Driver，后续再迁到 Spawner。
- `CharacterHitImpulseController` 未挂 Prefab 前，命中链路不会产生击退。

这些结论仅做过静态核对，尚未完成 Unity 编译和运行验证。
