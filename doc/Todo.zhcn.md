# TODOs

记录项目中未完成的任务。
- 里程碑开始时，尽量将所有任务列出来（并清理上一个里程碑的遗留任务）
- 任务完成时，将对应任务标记为完成（适度拆解，使其更易于追踪和管理）
- 里程碑结束时，将所有任务标记为完成（未完成任务未来将交接给下一里程碑）

## 工程项目基础

项目基础的构建，包括里程碑、功能分区、源码结构、构建工具、测试覆盖率和代码规范检查等。

- 项目配置
	- [ ] 创建Github知识库
- 项目构建
	- [ ] 构建工具：升级到.NET 9.0
	- [ ] 包发布：发布项目包
- 质量管理
	- [ ] 代码复杂度（CI）
- 项目发布
	- [ ] 发布Nuget包（CD）
	- [ ] 发布Github Page文档（CD）

## 架构设计、技术栈与基础设施

项目架构设计，包括系统架构、模块设计、接口设计、数据模型设计等。

- 系统架构设计
  - [ ] 时序图
- 核心模块和接口设计
  - [ ] Entitlement Builder
  - [ ] Taxonomy

## 工程项目

- Perkify.Core（运行时，内核）
  - [x] Eligible, Delegation
  - [x] Balance, Expiry, Enablement
  - [x] Entitlement, Entitlement Chain
  - [ ] Entitlement Builder

## 文档和演示代码

- 项目文档
  - [ ] 构建说明
  - [ ] 使用说明
  - [ ] SDK文档

- Perkify.Playground
  - [x] 时钟：A Faked Clock
  - [x] 订阅：Subscription
  - [ ] 点卡：Pay As You Go
  - [ ] 代金券：Coupon
