# TODOs

记录项目中未完成的任务。
- 里程碑开始时，尽量将所有任务列出来（并清理上一个里程碑的遗留任务）
- 任务完成时，将对应任务标记为完成（适度拆解，使其更易于追踪和管理）
- 里程碑结束时，将所有任务标记为完成（未完成任务未来将交接给下一里程碑）

## 工程项目基础

项目基础的构建，包括里程碑、功能分区、源码结构、构建工具、测试覆盖率和代码规范检查等。

- 项目配置
	- [x] 创建Github组织
	- [x] 创建Github团队
	- [x] 创建Github项目看板
	- [x] 创建Github仓库
	- [x] 创建Github标签
	- [x] 创建Github里程碑
	- [x] 创建Github问题模板：Bug, Feature
	- [x] 创建Github讨论区
	- [ ] 创建Github知识库

- 项目构建
	- [x] 源码结构：定义项目结构
	- [x] 构建工具：共享构建配置
	- [x] 构建工具：统一包版本管理
	- [ ] 构建工具：升级到.NET 9.0
	- [ ] 包发布：发布项目包

- 质量管理
	- [x] 代码规范检查（CI）
	- [ ] 安全检查（CI）
	- [x] 代码风格检查（CI）
	- [x] 文档风格检查（CI）
	- [x] 单元测试（CI）
	- [x] 测试覆盖率（CI）
	- [ ] 代码复杂度（CI）

- 项目发布
	- [ ] 发布Nuget包（CD）
	- [ ] 发布Github Page文档（CD）

## 架构设计、技术栈与基础设施

项目架构设计，包括系统架构、模块设计、接口设计、数据模型设计等。

- 系统架构设计
  - [x] 层次结构图
  - [ ] 时序图

- 核心模块和接口设计
  - [x] Eligible
  - [x] Balance, Expiry, Enablement & Delegation
  - [x] Entitlement
  - [x] Chain
  - [ ] Taxonomy

- 基础设施、技术栈与工具链
  - 文档创作
	- [x] 文本：Markdown
	- [x] 图形：DrawIO
  - 程序开发
    - [x] 编程语言：C#，.NET 8.0+
	- [x] 开发环境：Visual Studio 2022, Visual Studio Code
	- [x] 代码库管理：Github
  - 外部库
	- [x] 单元测试：xUnit
	- [x] 时间日期处理：NodaTime
	- [x] 命令行解析：CommandLineParser
	- [x] 命令行交互：Spectre.Console

## 工程项目

- Perkify.Core（运行时，内核）
  - [x] Eligible
  - [x] Balance, Expiry, Enablement & Delegation
  - [x] Entitlement
  - [ ] Chain
    - [x] 代码实现
    - [ ] 单元测试

## 文档和演示代码

- 项目文档
  - [x] 自述文件
  - [x] 版权说明
  - [x] 待办事项 
  - [x] 里程碑
  - [ ] 构建说明
  - [ ] 使用说明
  - [ ] SDK文档

- Playground CLI
  - 代码框架
	- [x] REPL
	- [x] Welcome
	- [x] Print Usage
	- [x] Switch Mode
  - 演示场景和功能
	- [x] 订阅：Subscription
	- [ ] 点卡：Pay As You Go
	- [ ] 代金券：Coupon
	- [x] 时钟（模拟）：Faked Clock
