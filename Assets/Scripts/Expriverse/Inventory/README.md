# 物品与库存

## 稳定链路

拾取 RPC → `CharacterInventory` → `CharacterInventoryEffectController` 差量投影到 `CharacterEffects`。玩家丢出、死亡掉落表和 JSON 存档均已有实现，但仍需随角色 Prefab 迁移完成挂载验证。

## 关键入口

- `Item/ItemData.cs`：继承 `GameplayEffectDefinition` 并实现 Tooltip；包含 pickupPrefab/icon，以 `item_data` 标签注册。
- `CharacterInventory.cs`：`ItemData → Count` 的唯一持有者，发布 `ItemCountChanged`。
- `CharacterInventoryEffectController.cs`：按数量差调用 `CharacterEffects.AddEffectRpc`/`RemoveEffectRpc`。
- `CharacterInventoryNetwork.cs`：AddItemRpc/RemoveItemRpc，发送目标为 Owner。
- `Item/ItemPickupBehaviour.cs`：拾取后由 Authority 执行 GivePickupRpc，再 AddItemRpc 并销毁拾取物。
- `CharacterItemDropController.cs`、`Item/ItemDropController.cs`：玩家主动丢出和敌人死亡掉落。
- `InventorySaveController.cs`：保存到 `persistentDataPath/InventorySaves/{Session.Code}.json`。

旧 `PlayerCharacterInventory` 已删除；角色 Prefab 上的 Missing Script 和新 `CharacterInventory` 挂载状态见 [角色上下文](../Character/README.md)。
