# 交互、AI 与场景道具

## 交互

- 契约位于 `Packages/io.github.xoderony.jog/Runtime/Interaction`：`IInteractable` 与 `InteractionHandler`。
- `Character/CharacterInteractor.cs` 根据 Aim 目标和 Interact 输入调用 `CanInteract`/`OnInteracted`，并联动 WorldTooltip 与 Outline。
- `Props/Teleporter.cs`、`GameEndRock.cs` 分别通过 ObjectiveController 切场景和离开会话。

## AI

- `TargetFinder.cs` 依据 Faction 敌我关系和 HurtBox 查找最近目标；无目标时回到巡逻路线。
- `Patrol` 下的 PatrolService、PatrolRoute、IPatrolBehavior 负责巡逻。
- `AITarget.cs`、`NavMeshAgentController.cs`、`PathFinder.cs` 分别承载目标 Transform、NavMesh 驱动和路径查询。
- `EnemySpawner.cs` 使用 `_respawnCount`/`_respawnAt` 重生敌人，并根据 DifficultyManager 差值施加 MaxHealth/AttackPower 效果。

## 已知风险

- `Props/DemonAltarInteraction.cs` 的生命代价和效果施加仍被注释，目前只广播交互事件，但 `CanInteract` 仍检查生命比例。
- `HolyAltarInteraction.cs` 与祭坛行为修改时应连同交互契约和角色效果链一起核对。
- AI 敌我判定必须使用 Faction，不使用 Unity Tag。
