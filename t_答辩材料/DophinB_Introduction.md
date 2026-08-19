# DophinB 后台更新机制介绍

> 适用范围：AOE3D 客户端 Lua 层后台资源更新（BackgroundUpdate）模块
> 关键词：DophinB / Dolphin SDK 第二通道 / 后台静默更新 / 配表热更 / Lua 资源覆盖层

---

## 一、DophinB 是什么

DophinB 是 Dolphin（海豚）更新 SDK 的**第二更新通道**（B 通道），用于在游戏运行时**后台静默下载**热更资源（主要是配置表 protobuf、Lua 的 .tba 归档等）。

它与常规 Dolphin 通道（启动时的强制更新）相互独立：

| 维度 | 常规 Dolphin（启动更新） | DophinB（后台更新） |
|------|------------------------|---------------------|
| 触发时机 | 游戏启动时 | 运行时后台静默 |
| 渠道标识 channelId | 主渠道 ID | `DolphinBId`（从 appVersion 读取） |
| 下载临时目录 | `doc/ifs/dolphinTmp/` | `doc/ifs/dolphinBTmp/ifs/` |
| 正式存放目录 | `doc/ifs/res/main/` 等 | `doc/ifs/res/dolphinb/` |
| 是否阻塞游戏 | 是（更新完才进游戏） | 否（玩家无感知） |
| 资源加载优先级 | main(6) | dolphinb(2)，高于 main |

---

## 二、核心文件清单

| 文件 | 角色 |
|------|------|
| `BGUpdateMgr.lua` | 后台更新总控：串联 DophinB + 常规校验，管理回调与文件复制 |
| `Task_BGSourceDophinBUpdate.lua` | DophinB SDK 驱动任务：下载 + CRC32 校验 + 完成事件 |
| `Task_BGUpdateBase.lua` | 任务基类：状态机 + SDK 回调接口实现 |
| `BGUpdateDophinBData.lua` | 实现 `DolphinDataInterface`，向 C# SDK 提供版本号/存储路径 |
| `BGUpdateDophinBPath.lua` | DophinB 目录路径管理（`dolphinBTmp/`） |
| `BGUpdatePath.lua` | 旧 DophinB 目录管理（`dolphinB/`，每次 Init 清理） |
| `BGUpdateModule/CfgUdpate.lua` | 配表子模块：注册 protobuf 源/目标路径复制映射 |
| `CS/Updater/Tasks/Task_SetDolphinB.cs` | 启动更新阶段的 C# 端 DophinB 文件落地逻辑 |

---

## 三、状态机

`Task_BGUpdateBase` 定义了 DophinB 任务的四个状态：

| 状态 | 值 | 含义 |
|------|----|----|
| Processing | 1 | 更新进行中 |
| Finished | 2 | 更新完成 |
| FinishAndExit | 3 | 完成并需要退出 |
| Failed | 4 | 更新失败 |

`Run_A2S` 中的驱动循环会持续调用 SDK，直到 state 变为 Finished / Failed / FinishAndExit 之一才退出。

---

## 四、完整更新流程

### 4.1 入口：BGUpdateMgr:Start()

```
BGUpdateMgr:Start()
  |
  +-- [updateEnableDolphinb = true] DophinB 阶段
  |     1. 创建 BGUpdateDophinBData（提供版本/路径元数据给 SDK）
  |     2. 创建 Task_BGSourceDophinBUpdate
  |     3. Run_A2S(updateData)  <-- 协程式阻塞，直到下载完成
  |     4. 检查 cleanDir，必要时 Recreate 临时目录
  |
  +-- [updateEnableSrcCheck = true] 常规资源校验阶段
        1. Task_BGSourceUpdate:Run_A2S(DolphinData)
        2. OnUpdateFinished(isCurrentNewest, updatedSucc)
             +-- autoCopyDst 文件复制
             +-- resUpdateCallbacks 回调
             +-- Post "ResUpdateFinished" 事件
```

### 4.2 Task_BGSourceDophinBUpdate:Run_A2S 内部

```
Run_A2S(updateData)
  |
  1. CoWrap_DolphinCallBackInterface(self)  将 Lua 对象包成 C# 回调接口
  2. SDKUpdate.CreateUpdateMgr(callback, updateData)  创建 SDK 实例
  3. 读 appVersion 获取 DolphinBId 作为 channelId
  4. GetDolphinBUpdateUrl() 决定正式/预发布 URL
  5. StartUpdateService(initInfo)  启动服务，失败重试最多 3 次
  6. 驱动循环:
       while state 未完成 do
           dolphinMgr:DriveUpdateService()   每帧驱动 SDK
           WaitForUpdate()                   等一帧
       end
  7. CleanUp()  停止并卸载 SDK
  8. Post "BGUpdateDolphinBFinished" 事件（成功/失败都发）
```

### 4.3 SDK 回调驱动（关键节点）

SDK 通过回调接口反向驱动状态机：

| 回调 | 作用 |
|------|------|
| `OnNoticeNewVersionInfo` | 发现新版本 -> `dolphinMgr:Continue()` 继续流程 |
| `OnNoticeChangeSourceVersion` | 下载完成 -> 逐文件 CRC32 校验 -> 写加密版本文件；校验失败设 `cleanDir=true` |
| `OnNoticeUpdateSuccess` | 更新成功 -> `state = Finished`，退出驱动循环 |
| `OnUpdateMessageBoxInfo` | 出错 -> 最多自动重试 3 次（每次间隔 1 秒），仍失败则设 Finished |

---

## 五、下载资源如何生效（覆盖层设计）

DophinB 下载的资源**不会覆盖原始文件**，而是采用**独立目录覆盖层**：

```
原始资源（永不改动）
  doc/ifs/res/main/...            主包资源
  doc/.Lua/...                    Lua 源文件（仅 Editor）

DophinB 覆盖层（独立目录）
  doc/ifs/res/dolphinb/...        DophinB 下载资源
```

### 5.1 加载优先级

xLua / TBUResManager 按资源组（group）优先级查找文件：

```
cos(0) > dev(1) > dolphinb(2) > cfg(3) > dynamic(4) > patchMain(5) > main(6)
```

DophinB 目录（dolphinb group）优先级**高于** main，所以只要 `res/dolphinb/` 下有同名文件，就会优先加载，原始文件保持不动。

### 5.2 xLua 加载链

```
require("Module.Xxx")
  |
  1. LoadFromSrcDirs      -> doc/.Lua/Module/Xxx.lua  （源文件，仅 Editor）
  2. LoadFromDolphinB     -> res/dolphinb/bundle/lua/Module/Xxx.tba
  3. LoadFromTBUBinary    -> res/main/bundle/lua/Module/Xxx.tba
```

### 5.3 文件落地时机（启动更新 vs 后台更新）

| 步骤 | 启动更新（Task_SetDolphinB.cs） | 后台更新（BGUpdateMgr.lua） |
|------|------------------------------|---------------------------|
| 下载目的目录 | `dolphinBTmp/ifs/` | `dolphinBTmp/ifs/` |
| 拷贝到正式目录 | SafeCopyTo -> `res/dolphinb/` | 仅 CfgUdpate 拷贝 protobuf |
| 触发重新索引 | WriteNeedIndex() | 无 |
| 清理旧资源 | ClearDolphinBRes() | 无 |

**注意**：后台更新路径目前只有 `CfgUdpate` 子模块会把 protobuf 从临时目录拷贝到 `res/cfg/protobuf/`，尚未实现 Lua .tba 的自动落地与重索引。

---

## 六、配表更新子模块（CfgUdpate）

`BGUpdateModule/CfgUdpate.lua` 是目前唯一注册了自动复制的子模块：

```lua
cls.protobufSrcDir = mgr.bgUpdateMgr.bgUpdatePath.protobufDir
    -- 源：dolphinB/ifs/res/main/protobuf/
cls.protobufDesDir = cls.cfgDir.."protobuf/"
    -- 目标：doc/ifs/res/cfg/protobuf/

mgr.bgUpdateMgr:AddCopyResOnNewVersion("cfg", cls.protobufSrcDir, cls.protobufDesDir, function(fName)
    if mgr.cfg then
        mgr.cfg:CleanUpStreamCfgByName(fName)  -- 复制前释放旧配置文件句柄
    end
end)
```

复制链路：

```
dolphinBTmp/ifs/ (SDK 下载目的地)
    |
    | CopyFolder（逐文件 SafeCopyTo，每文件前 CleanUpStreamCfgByName）
    v
doc/ifs/res/cfg/protobuf/ (游戏实际读取目录)
```

版本比对：`GetBaseResVersion()` 读主版本，`GetSubResVersion(key)` 读子模块版本，版本不一致才触发复制（避免重复拷贝）。

---

## 七、完成事件

DophinB 任务完成后（无论成功失败）会发出事件：

```lua
mgr.evt:Post("BGUpdateDolphinBFinished", {
    isCurrentNewest = ...,  -- 是否已是最新版本（无新版本为 true）
    updatedSucc = ...,      -- 资源是否成功下载更新
    state = ...,            -- 任务最终状态（Finished / Failed / Processing）
})
```

监听方式：

```lua
mgr.evt:Regist("BGUpdateDolphinBFinished", self, self.OnDolphinBUpdateFinished)

function cls:OnDolphinBUpdateFinished(data)
    if data.updatedSucc then
        -- 有新资源下载成功，可提示玩家或触发后续处理
    end
end
```

常规资源校验阶段另有独立事件 `ResUpdateFinished`，由 `DynamicDownloadMgr` 监听，检测到新资源时弹窗询问玩家是否返回登录界面重新加载。

---

## 八、Lua 代码热更的限制

DophinB 只负责**下载和落地文件**，不负责运行时重新加载 Lua 模块。

- 已 `require` 过的 Lua 模块缓存在 `package.loaded` 中，DophinB 更新全程不触碰它
- `HotReloadMgr` 的运行时热重载机制**仅在 Editor 生效**（依赖 FileSystemWatcher 监控源码目录 + 两个 PlayerPref 开关），APK 环境不走这条路径
- APK 下 Lua 代码更新目前依赖玩家重新登录（重新走 Launcher 流程 -> 重新 require 全部模块）

若要实现 APK 下"下载完立即生效"，需组合三步：
1. 参考 `Task_SetDolphinB` 把 `dolphinBTmp/ifs/` 的 .tba 拷贝到 `res/dolphinb/`
2. 触发 TBUResManager 重新索引
3. 按模块清单逐个清 `package.loaded` 并复用 `HotReloadMgr` 的 table patching（原地覆盖字段，保留旧引用）逻辑重载

---

## 九、开关说明

| 开关 | 位置 | 作用 |
|------|------|------|
| `updateEnableDolphinb` | BGUpdateMgr 类变量 | 是否启用 DophinB 后台下载 |
| `updateEnableSrcCheck` | BGUpdateMgr 类变量 | 是否启用常规资源校验 |
| `DolphinBEnv` | AoECore 配置 | 决定正式（Dist）/预发布 URL |
| `DolphinBTempDelete` | Dolphin 自定义配置 | 预留开关：删除 DophinB 下载临时目录 |
| `DolphinBDelete` | Dolphin 自定义配置 | 预留开关：删除 DophinB 加载目录 |
| `DolphinBEnable` | Dolphin 自定义配置 | 预留开关：关闭 DophinB 文件拷贝 |

---

## 十、术语速查

| 术语 | 说明 |
|------|------|
| A2S | Async to Sync，协程式同步调用（在协程内以同步写法执行异步流程） |
| .tba | TBUBinaryArchive，Lua 资源的二进制归档格式 |
| ifs | 资源文件系统目录（in-app file system） |
| CRC32 | 文件完整性校验算法，DophinB 下载后逐文件校验 |
| DolphinBId | DophinB 通道的渠道标识，从加密的 appVersion 文件读取 |
