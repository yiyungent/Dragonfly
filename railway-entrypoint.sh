#!/bin/sh

# 参考: https://github.com/Squidex/squidex/blob/master/packer/heroku/squidex/squidex.sh

# 注意: $PORT 非常重要, Railway 必须监听此端口
export ASPNETCORE_URLS="http://+:$PORT"
export ASPNETCORE_ENVIRONMENT="Production"
export TZ="Asia/Shanghai"

echo ${PLUGINCORE_ADMIN_USERNAME}
echo ${PLUGINCORE_ADMIN_PASSWORD}

mkdir App_Data

touch /app/App_Data/PluginCore.Config.json

cat '/app/railway-PluginCore.Config.json' | sed "s/PLUGINCORE_ADMIN_USERNAME/${PLUGINCORE_ADMIN_USERNAME}/g" | tee '/app/App_Data/PluginCore.Config.json'
cat '/app/App_Data/PluginCore.Config.json' | sed "s/PLUGINCORE_ADMIN_PASSWORD/${PLUGINCORE_ADMIN_PASSWORD}/g" | tee '/app/App_Data/PluginCore.Config.json'

dotnet WebApi.dll