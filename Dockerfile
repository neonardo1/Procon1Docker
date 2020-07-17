FROM mono:latest
LABEL maintainer="Prophet731"

ARG UID=10000
ARG GID=10000
ARG PROCONPATH="/opt/procon"
ARG FILE="procon.zip"
ARG DLURL="https://api.myrcon.net/procon/download"
ARG PLUGINPACKURL="https://api.myrcon.net/plugins_pack1.zip"
ARG PLUGINPACKNAME="plugins_pack1.zip"

ENV PROCON_GAMESERVER_IP=""
ENV PROCON_GAMESERVER_PORT=""
ENV PROCON_GAMESERVER_PASS=""

# Install required software
RUN apt-get update && apt-get install unzip wget inetutils-ping -yqq

# Create the directory where procon will live
RUN mkdir -p $PROCONPATH

# Download latest procon version and unzip it
RUN wget -q -O /tmp/$FILE $DLURL && unzip -x /tmp/$FILE -d $PROCONPATH

# Delete the temp file
RUN rm -f /tmp/$FILE

# Download a plugins pack of the most common used
RUN wget -q -O /tmp/$PLUGINPACKNAME $PLUGINPACKURL && unzip -o -x /tmp/$PLUGINPACKNAME -d "$PROCONPATH/Plugins"

# Delete the temp file
RUN rm -f /tmp/$PLUGINPACKNAME

RUN groupadd -r -g $GID procon && useradd -r -g procon -u $UID procon
RUN chown procon:procon -R $PROCONPATH
RUN chmod -R 0777 $PROCONPATH

WORKDIR $PROCONPATH

COPY ./entrypoint.sh $PROCONPATH
RUN chmod +x ./entrypoint.sh

USER procon:procon

VOLUME ["$PROCONPATH/Configs", "$PROCONPATH/Plugins", "$PROCONPATH/Logs"]

ENTRYPOINT ./entrypoint.sh
