# Xoderony Feature Package Template

这是一个项目自有嵌入式 Unity Package Manager 包的最小模板。模板位于 `PackageTemplates`，Unity 不会将其作为包导入。

## 创建包

1. 将整个目录复制到 `Packages`。
2. 将目录名改为正式包名，例如 `io.github.xoderony.object-pool`。
3. 在模板目录内统一替换以下内容：

| 模板值 | 示例替换值 |
| --- | --- |
| `io.github.xoderony.feature-template` | `io.github.xoderony.object-pool` |
| `Xoderony Feature Template` | `Xoderony Object Pool` |
| `Xoderony.FeatureTemplate` | `Xoderony.ObjectPool` |

4. 将两个 asmdef 文件名改为与各自的 `name` 一致。
5. 将运行时代码放入 `Runtime`，编辑器代码放入 `Editor`。
6. 删除不需要的空目录或程序集。

## 迁移现有代码

- 移动现有 Unity 资源时，应连同原有 `.meta` 文件一起移动。
- 本模板不包含 `.meta`，复制到 `Packages` 后由 Unity 为新文件生成 GUID。
- 不要在多个实际包之间复制同一组已生成的 `.meta` 文件。
- 初次迁移只调整目录和依赖，不同时修改命名空间、公共 API 或代码行为。

## 依赖

- C# 程序集依赖写入对应 asmdef 的 `references`。
- 包直接依赖的其他 UPM 包写入 `package.json` 的 `dependencies`。
- 项目私有的嵌入式包放入 `Packages` 后，不需要额外加入项目的 `Packages/manifest.json`。
- 运行时程序集不得依赖 Editor 程序集。
- 包内程序集不得依赖项目 `Assets` 下的业务程序集。

## 可选目录

按实际需要添加，不要预先创建空结构：

```text
Tests/
  Editor/
  Runtime/
Samples~/
Documentation~/
CHANGELOG.md
LICENSE.md
THIRD PARTY NOTICES.md
```
