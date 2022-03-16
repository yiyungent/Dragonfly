# Railway Dockerfile

# 注意: yiyungent/dragonfly:latest railway 会一直使用缓存，不会更新镜像
FROM yiyungent/dragonfly:v0.1.3

ADD railway-entrypoint.sh ./railway-entrypoint.sh
RUN chmod +x ./railway-entrypoint.sh
ADD railway-PluginCore.Config.json ./railway-PluginCore.Config.json

ENTRYPOINT ["/bin/sh", "./railway-entrypoint.sh"]