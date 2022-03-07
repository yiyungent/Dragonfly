# Railway Dockerfile

FROM yiyungent/dragonfly:latest

ADD railway-entrypoint.sh ./railway-entrypoint.sh
RUN chmod +x ./railway-entrypoint.sh
ADD railway-PluginCore.Config.json ./railway-PluginCore.Config.json

ENTRYPOINT ["/bin/sh", "./railway-entrypoint.sh"]