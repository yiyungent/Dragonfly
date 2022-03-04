#!/bin/sh

# 参考: https://github.com/Squidex/squidex/blob/master/packer/heroku/squidex/squidex.sh

# 注意: $PORT 非常重要, Railway 必须监听此端口
export ASPNETCORE_URLS="http://+:$PORT"
export ASPNETCORE_ENVIRONMENT="Production"
export TZ="Asia/Shanghai"

dotnet WebScreenshot.dll