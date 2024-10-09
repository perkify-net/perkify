# 测试

TODO

## Traits

| Category | Description | 
| :-- | :-- | 
| `Unit` | 单元测试 |
| `Functional` | 特定模块的功能测试 |
| `Integration` | 整个后端服务的集成测试 |
| `Benchmark` | 性能测试 |
| `E2E` | 端到端测试（针对用户界面）|

| Environment, Functional/Integration Tests only | Description | 
| :-- | :-- | 
| `Prod` | 生产环境，正式运行在生产环境里 |
| `Test` | 测试环境，运行在与生产环境高度相似的沙箱环境 |
| `Dev` | 开发环境，运行在项目构建阶段，一般会Mock复杂的上下游依赖并支持单机调试 |

| Module | Description |
| :-- | :-- |
| `Balance` | 余额 |
| `Expiry` | 过期 |
| `Taxonomy` | 分类 |
| `Entitlement` | 权益 |
| `Chain` | 权益链 |

| Class, Unit Tests only | Description | 
| :-- | :-- | :-- | 
| `Entity` | 实体类，一般用于对象间交换数据的具体数据格式（包括枚举、记录等），无较为复杂的业务逻辑 |
| `Interface` | 接口类，统一数据操作方式，包括基类、接口方法的默认实现及针对接口的扩展方法等 |
| `Contract` | 协议类，定义并实现存储系统或服务通讯中的具体格式，需额外关注兼容性 |
| `Engine` | 业务类，核心业务逻辑的封装与实现 |
| `Utility` | 工具类，为使用某些底层类库提供特殊便利 |

| Priority | Description | 
| :-- | :-- | 
| `High` | 优先级高， |
| `Middle` | 优先级中 |
| `Low` | 优先级低 |
