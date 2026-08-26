# 战斗与弹体

## 当前设计

- `Packages/io.github.xoderony.expriverse/Runtime/Combat/HitQuery.cs` 提供 Sphere/Box 一次性查询，按实体去重并保留最近命中点；`HitResult` 保存 Entity、Collider 和命中点。
- `CombatDamage.cs` 统一阵营准入、falloff 和伤害施加。调用方传正伤害量，内部转换成负的 `HealthChangeMessage.Value`；Effects 内部用 `Route`，权威端射弹和近战用 `Broadcast`。
- `ProjectileEntity.cs` 是同步 Owner 和 lifetime 的抽象射弹基类；`LinearProjectile`、`PlacedProjectile` 按初始化参数集区分具体弹种。
- `ProjectileExplosion` 与 `Components/ProjectileDamage`、`ProjectileDot`、`ProjectilePenetration`、`ProjectileDespawn` 是普通 `IComponent` 能力，由射弹类型被动调用并配置在 `Entity.Components`。
- Mage 和 Spitter 的 SkillController 创建弹体后通过强类型 `Initialize` 初始化。

## 迁移状态与风险

- 2026-08-09 已删除旧 Motor/DamageOnCollision/ExplosionOn*/EffectOn*/ApplyDot/Properties 等脚本；旧 `.meta` 交给 Unity 刷新清理。
- 4 个射弹 Prefab 需要重新组装主脚本、`Entity.Components` 能力和网络骨架，由用户在 Unity 中处理。
- `HitQuery`/`CombatDamage` 已接入 Golem、Ghost、Bite 和 ConditionalArea；Fighter、Skeleton 的触发式 `HitBox` 以及其余 Projectile 使用点需在对应模块重构时迁移。
- 未迁移的 Fighter/Skeleton 仍可能以正 `Value` 调 `TakeDamage`，按当前生命协议会进入治疗分支；HitBox 重构时必须统一到 `CombatDamage`。
- `Character/States/*/SkillController.cs` 旧链路还会经 `IHittable.TakeHit` 和 `IDamageable.TakeDamage`；修改时需同时核对 HitRouter 与 HealthChangeRouter。

当前结论为静态核对，射弹 Prefab 和运行时行为尚未在 Unity 验证。
