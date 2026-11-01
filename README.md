# CtYun 云电脑保活

CtYun 用于登录天翼云电脑并维持 WebSocket 保活连接。当前版本支持：

- 多账号同时登录和保活。
- Windows/本机可执行文件：支持本地配置文件模式和交互输入模式。
- Docker：支持挂载本地配置文件模式和 `-it` 交互输入模式。
- 首次绑定设备需要短信验证码时，通过终端交互输入。

## 与上游 [leleji/CtYun](https://github.com/leleji/CtYun) 的差异

本仓库基于上游 v1.1.5 fork，**保活逻辑与上游完全一致**，仅新增以下功能：

1. **日志文件化**：控制台输出的同时写入数据目录 `logs/ctyun-YYYY-MM-DD.log`，按天滚动，UTF-8 带 BOM（Windows 记事本打开不乱码）；Windows/Linux/Docker 全平台生效。
2. **控制台可隐藏（`hideConsole`）**：`accounts.json` 新增配置项，设为 `true` 后 Windows 下运行不再弹出黑色控制台窗口，日志改由文件日志承载；仅 Windows 生效，Linux/Docker 自动跳过，不影响原有功能。
3. **发行版编译形态**：Release 中的 `CtYun.exe` 为 **NativeAOT 原生编译**单文件（win-x64，约 6.4 MB），与上游发行版体积一致，免安装 .NET 运行时直接运行。CI 已支持推送 `v*` 标签时自动完成 AOT 编译并发布 Release。

`accounts.json` 配置示例：

```json
{
  "keepAliveSeconds": 60,
  "hideConsole": true,
  "accounts": [
    {
      "name": "account-a",
      "user": "你的账号",
      "password": "你的密码"
    }
  ]
}
```

> 注意：首次设备绑定需要短信验证码输入，请保持 `hideConsole` 为 `false`，用窗口模式完成绑定后再开启隐藏。

## 配置文件模式

程序默认在数据目录读取 `accounts.json`：

- 本机可执行文件：默认数据目录为程序所在目录。
- Docker：默认数据目录为 `/app/data`，建议挂载到宿主机。
- 也可以通过环境变量 `CTYUN_CONFIG` 指定配置文件路径。
- 也可以通过环境变量 `CTYUN_DATA_DIR` 指定数据目录。

`accounts.json` 示例：

```json
{
  "keepAliveSeconds": 60,
  "accounts": [
    {
      "name": "account-a",
      "user": "你的账号1",
      "password": "你的密码1",
      "deviceCode": "web_自行生成的32位随机字符"
    },
    {
      "name": "account-b",
      "user": "你的账号2",
      "password": "你的密码2"
    }
  ]
}
```

`deviceCode` 可不填。程序会为每个账号自动生成设备码，并保存到 `devices/{账号名}.txt`。为了避免每次 Docker 重建镜像后重新绑定设备，务必持久化数据目录。

> 提示：多账号时可为每个账号单独指定 `deviceCode`；日志文件位于数据目录 `logs/` 下，按账号名和云电脑编号区分。

Linux 生成设备码示例：

```bash
echo "web_$(cat /dev/urandom | tr -dc 'a-zA-Z0-9' | fold -w 32 | head -n 1)"
```

## 本机运行

把 `accounts.json` 放到程序目录后直接运行：

```bash
CtYun.exe
```

如果没有配置文件，也没有环境变量，程序会进入交互输入模式：

```text
账号:
密码:
继续添加账号? (y/N):

```

首次设备绑定时，程序会提示输入短信验证码。

## Docker 首次运行

准备宿主机配置目录：

```bash
mkdir -p ./ctyun-data
```

把 `accounts.json` 放到 `./ctyun-data/accounts.json`。首次运行建议使用 `-it`，方便输入短信验证码：

```bash
docker run -it --rm \
  --name ctyun-init \
  -v "$(pwd)/ctyun-data:/app/data" \
  su3817807/ctyun:latest
```

看到保活任务启动后，说明设备码已经绑定成功。之后可以按 `Ctrl+C` 停止初始化容器，再改为后台运行。

## Docker 后台运行

设备绑定完成后使用：

```bash
docker run -d \
  --name ctyun \
  -v "$(pwd)/ctyun-data:/app/data" \
  su3817807/ctyun:latest
```

查看日志：

```bash
docker logs -f ctyun
```

## 兼容旧环境变量模式

单账号仍支持旧环境变量。首次绑定设备时同样使用 `-it` 输入短信验证码：

```bash
docker run -it --rm \
  --name ctyun-init \
  -v "$(pwd)/ctyun-data:/app/data" \
  -e APP_USER="你的账号" \
  -e APP_PASSWORD="你的密码" \
  -e DEVICECODE="web_你的设备码" \
  su3817807/ctyun:latest
```

绑定完成后改为后台运行：

```bash
docker run -d \
  --name ctyun \
  -v "$(pwd)/ctyun-data:/app/data" \
  -e APP_USER="你的账号" \
  -e APP_PASSWORD="你的密码" \
  -e DEVICECODE="web_你的设备码" \
  su3817807/ctyun:latest
```

建议新部署优先使用 `accounts.json`，多账号管理更清晰，也更适合 Docker 持久化。

## 日志与保活

程序会为每个账号、每台云电脑启动独立保活任务。日志格式会带上账号名和云电脑编号，便于区分：

```text
[account-a][desktop-code] -> 收到保活校验
[account-a][desktop-code] -> 发送保活响应成功

```

本 fork 额外将上述日志写入数据目录 `logs/ctyun-YYYY-MM-DD.log`（控制台与文件双写）。

## 从源码构建

- **Windows NativeAOT**（产出与 Release 相同的 6.4 MB 单文件，需 VS 2022 C++ 工具链）：

```bash
dotnet publish CtYun/CtYun.csproj -c Release -r win-x64 -o publish
```

- **Docker / Linux**（框架依赖，多架构镜像由 CI 自动构建）：

```bash
dotnet publish CtYun/CtYun.csproj -c Release -p:PublishAot=false -o publish
```

- **CI**：推送到 `master` 自动构建并推送 Docker 多架构镜像；推送 `v*` 标签自动编译 Windows NativeAOT 并发布 GitHub Release。

## 说明

登录图形验证码识别接口方案来自 [sml2h3/ddddocr](https://github.com/sml2h3/ddddocr)。
