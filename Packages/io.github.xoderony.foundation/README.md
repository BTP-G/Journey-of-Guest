# Xoderony Foundation

Xoderony Foundation 提供可复用的基础集合、委托通道、通用扩展和对象池能力。

## 程序集

- `Xoderony.Foundation`

程序集使用 `Xoderony` 根命名空间，并按职责保留 `Xoderony.Collections`、`Xoderony.Extensions`、`Xoderony.ObjectPool` 等子命名空间。

## 安装

项目内开发时，将本包作为 embedded package 放在 `Packages/io.github.xoderony.foundation`。

从 Git 仓库安装时，可以使用带 `path` 的 Git URL 指向本包目录。

## 兼容性

- Unity 6000.0 或更高版本

## 设计边界

本包不包含依赖特定第三方包或 Unity 子系统的集成代码。Unity 对象池、Netcode、ZString、Hjson 等扩展由其他 Xoderony 包提供。
