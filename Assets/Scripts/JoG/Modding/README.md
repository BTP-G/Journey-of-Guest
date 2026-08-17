# 数据注册与 Modding

## 数据注册

- `Utilities/AssetsUtility.cs` 按 YooAsset 标签加载 Definition、Data 和网络 Prefab，并写入各 Shared 注册表或 NGO PrefabHandler；同时提供 `LoadLanguageFromHjson`。
- `DefaultPackageManager.cs` 创建 DefaultPackage 后调用数据加载。
- `Character/CharacterDataDictionary.cs`、`Item/ItemDataDictionary.cs`、`GameplayEffects/PeriodicHealthChangeDefinitionDictionary.cs` 是项目侧 Shared 注册表，并包含调试命令入口。
- 当前标签包括 `item_data`、`character_data`、`gameplay_effect_def`、`periodic_health_change_def` 和 `network_prefab`。
- `Xoderony.Numerics.Q16` 位于 `io.github.xoderony.unity`，使用 16 位小数；JoG 包中的 `Q16Serializer.cs` 提供 NGO 读写。

## Mod 边界

- `Modding/ModManager.cs` 扫描 `Assets/Mods` 下的 `mod.json`，拓扑排序依赖，通过 `Assembly.LoadFrom` 加载程序集，并用 `enabled.txt` 保存启用状态。
- `Packages/io.github.xoderony.jog/Runtime/Modding` 中的 `Mod` 和 `IModManager` 是独立 Mod 可引用的稳定契约。
- 需要供 Mod 使用的稳定 JoG 类型应提升到 `Packages/io.github.xoderony.jog`；具体玩法实现保留在 `Assembly-CSharp`，包不得反向依赖项目程序集。

## 本地包开发

创建新包时从根目录 `PackageTemplates/io.github.xoderony.feature-template` 开始，并同步检查目标包的 asmdef、package.json、README 和必要 `.meta`。包的长期公共职责写在包自身 README；Journey of Guest 的使用方式与迁移风险写在项目侧对应模块 README。
