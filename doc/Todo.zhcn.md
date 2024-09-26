# TODOs

记录项目中未完成的任务。
- 里程碑开始时，尽量将所有任务列出来（并清理上一个里程碑的遗留任务）
- 任务完成时，将对应任务标记为完成（适度拆解，使其更易于追踪和管理）
- 里程碑结束时，将所有任务标记为完成（未完成任务未来将交接给下一里程碑）

## 工程项目基础

项目基础的构建，包括里程碑、功能分区、源码结构、构建工具、测试覆盖率和代码规范检查等。

- 项目配置
	- [ ] 里程碑定义：定义项目里程碑和迭代计划
	- [ ] 功能分区定义：定义项目功能分区
	- [ ] 权限管理：定义项目访问权限

- 项目构建
	- [x] 源码结构：定义项目结构
	- [ ] 构建工具：集成构建工具并定义可共享的Build配置
	- [ ] 包发布：发布项目包

- 工程优化
	- [ ] 持续集成（CI）：配置CI流水线
	- [ ] 持续发布（CD）：配置CD流水线
	- [ ] 代码规范检查：集成代码规范检查工具
	- [ ] 测试覆盖率：集成测试覆盖率工具

## 架构设计、技术栈与基础设施

项目架构设计，包括系统架构、模块设计、接口设计、数据模型设计等。

- 系统架构设计
  - [ ] 层次结构图
  - [ ] 时序图

- 核心模块和接口设计
  - [x] Eligible
  - [ ] Balance
  - [ ] Expiry
  - [ ] Entitlement
  - [ ] Chain
  - [ ] Taxonomy

- 技术栈与工具
  - 文档创作
	- [x] 文本：Markdown
	- [x] 图形：DrawIO
  - 程序开发
    - [x] 编程语言：C#，.NET 8.0+
	- [x] 开发环境：Visual Studio 2022, Visual Studio Code
	- [ ] 代码库管理：Github
  - 外部库
	- [x] 单元测试：xUnit
	- [x] 时间日期处理：NodaTime
	- [x] 命令行解析：CommandLineParser
	- [x] 命令行交互：Spectre.Console

## 运行时（内核）

- Tier 0
  - [x] IEligible接口
  - [x] IEligible扩展方法
  - [x] Delegation类
  - [x] Eligible单元测试

- Tier 1
  - Balance
    - [x] IBalance接口
    - [x] BalanceType枚举
    - [x] IBalance扩展方法
    - [x] Balance类
    - [x] Balance单元测试
  - Expiry
    - [x] IExpiry接口
    - [x] IExpiry扩展方法
    - [x] Expiry类
    - [x] Expiry单元测试

- Tier 2
  - [ ] Entitlement对象
  - [ ] Chain对象

## 文档和演示代码

- 项目文档
  - [x] 自述文件
  - [x] 版权说明
  - [x] 待办事项 
  - [ ] 里程碑
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
