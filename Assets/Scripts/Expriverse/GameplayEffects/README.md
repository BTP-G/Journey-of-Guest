# Gameplay Effects

## 结构与入口

- 通用契约位于 `Packages/io.github.xoderony.gameplay-effects`，只定义 `GameplayEffectData`、Definition、Controller 和全局注册表，不依赖 Expriverse、NGO 或 VContainer。
- `Character/CharacterEffects.cs` 是 Definition → Count 的唯一所有者，以 Data 运行时类型分发到 `IGameplayEffectController`。
- `CharacterEffects` 提供本地/RPC Add/Remove 和 `OnSynchronize` 快照；VContainer 在边界注入 `IReadOnlyList<IGameplayEffectController>`，实现会取得具体数组后遍历。
- `Character/CharacterTimedEffects.cs` 使用 ServerTime 管理限时批次，在 PostUpdateLoop 清理。
- `Character/CharacterPeriodicHealthChanges.cs` 管理周期伤害 Source/Tick，并按 `MergeMode` 合并 TickCount/TickValue；展示 Count 为剩余 Tick。

## 控制器

`Controllers` 当前包含 Stat、DamageDealt、周期伤害、伤害反射、HealthChange 值修改和条件区域伤害等控制器。可选 Count 与运行时状态由实际消费它的 Controller 持有，不建立跨 Controller 的统一 Count 查询协议。

`Data/StatEffectData.cs` 的 MultiplierBonus 使用 Q16，范围为 -99.9% 到 +999%，层数上限 1000；负加成按 `(1 + bonus)^count` 复利。

## 注册与风险

- Definition 的 `_dataArray` 使用 SerializeReference，运行时通过 `DataSpan` 暴露。
- `GameplayEffectDefinitionRegistry.Shared` 使用 `Animator.StringToHash(name)` 作为 ID；0 保留，冲突抛异常。
- `CharacterPeriodicHealthChanges.ApplyTick` 当前直接调用 `HealthChangeRouter.Route`，不会经过 `CanDamage`/`CanHeal` 准入；这是已确认代码事实，是否符合最终设计尚未运行验证。
- 旧 `Assets/Scripts/Expriverse/Buff` 实现已迁移，本目录和 `CharacterEffects` 是当前入口。
