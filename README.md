


> ASP.NET Core + Selenium 实现 Web 自动化平台

[![repo size](https://img.shields.io/github/repo-size/yiyungent/Dragonfly.svg?style=flat)]()
[![LICENSE](https://img.shields.io/github/license/yiyungent/Dragonfly.svg?style=flat)](https://github.com/yiyungent/Dragonfly/blob/master/LICENSE)
[![QQ Group](https://img.shields.io/badge/QQ%20Group-894031109-deepgreen)](https://jq.qq.com/?_wv=1027&k=q5R82fYN)

## Introduce


ASP.NET Core + Selenium 实现 Web 自动化平台

- **开箱即用** - 完全打包好的 `Selenium` 环境
- **易扩展** - 集成 `PluginCore`, 插件化架构




## Quick Start


### 方式1: 使用 Railway 免费部署 

[![Deploy on Railway](https://railway.app/button.svg)](https://railway.app/new/template?code=JQuUBW&referralCode=8eKBDA)


#### Railway 环境变量

| 环境变量名称                | 必填 | 备注                    |
| --------------------------- | ---- | ----------------------- |
| `PLUGINCORE_ADMIN_USERNAME` | √    | PluginCore Admin 用户名 |
| `PLUGINCORE_ADMIN_PASSWORD` | √    | PluginCore Admin 密码   |


> 注意: Railway 重新 Deploy 后会删除数据, 你安装的所有插件及数据都将清空。

### 方式2: 使用 Heroku 免费部署

[![Deploy on Heroku](https://www.herokucdn.com/deploy/button.svg)](https://heroku.com/deploy?template=https://github.com/yiyungent/Dragonfly)

#### Heroku 环境变量

| 环境变量名称                | 必填 | 备注                    |
| --------------------------- | ---- | ----------------------- |
| `PLUGINCORE_ADMIN_USERNAME` | √    | PluginCore Admin 用户名 |
| `PLUGINCORE_ADMIN_PASSWORD` | √    | PluginCore Admin 密码   |


### 方式3: 使用 Docker

```bash
# 获取源代码: 方式1: ssh 
git clone git@github.com:yiyungent/Dragonfly.git
# 获取源代码: 方式2: https 
git clone https://github.com/yiyungent/Dragonfly.git

cd Dragonfly

docker build -t yiyungent/dragonfly -f src/WebApi/Dockerfile .

docker run -d -p 5004:80 -e ASPNETCORE_URLS="http://*:80" --shm-size="500m" --name dragonfly yiyungent/dragonfly
```


## 插件开发

> 插件开发 可参考:   
> - [插件开发 | PluginCore](https://moeci.com/PluginCore/zh/PluginDev/Guide/)

> Dragonfly 插件开发包  
> 插件开发包中已包含:   
> - `Selenium.WebDriver`
> - `PluginCore.IPlugins`

```powershell
dotnet add package Dragonfly.Sdk
```








## Donate

Dragonfly is an MIT licensed open source project and completely free to use. However, the amount of effort needed to maintain and develop new features for the project is not sustainable without proper financial backing.

We accept donations through these channels:

- <a href="https://afdian.net/@yiyun" target="_blank">爱发电</a> (￥5.00 起)
- <a href="https://dun.mianbaoduo.com/@yiyun" target="_blank">面包多</a> (￥1.00 起)

## Author

**Dragonfly** © [yiyun](https://github.com/yiyungent), Released under the [MIT](./LICENSE) License.<br>
Authored and maintained by yiyun with help from contributors ([list](https://github.com/yiyungent/Dragonfly/contributors)).

> GitHub [@yiyungent](https://github.com/yiyungent) Gitee [@yiyungent](https://gitee.com/yiyungent)

