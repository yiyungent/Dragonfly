# Heroku Dockerfile

FROM yiyungent/dragonfly:v0.1.3

ADD heroku-entrypoint.sh ./heroku-entrypoint.sh
RUN chmod +x ./heroku-entrypoint.sh
ADD heroku-PluginCore.Config.json ./heroku-PluginCore.Config.json

ENTRYPOINT ["/bin/sh", "./heroku-entrypoint.sh"]