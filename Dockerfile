FROM mono:latest

ARG UID=10000
ARG GID=10000
ARG PROCONPATH="/opt/procon"
ARG FILE="procon.zip"
ARG DLURL="https://api.myrcon.net/procon/download"
ENV PROCON_GAMESERVER_IP=""
ENV PROCON_GAMESERVER_PORT=""
ENV PROCON_GAMESERVER_PASS=""

RUN mkdir -p $PROCONPATH
RUN apt-get update && apt-get install unzip wget inetutils-ping -yqq
RUN wget -q -O /tmp/$FILE $DLURL && unzip -x /tmp/$FILE -d $PROCONPATH
RUN rm -f /tmp/$FILE

RUN groupadd -r -g $GID procon && useradd -r -g procon -u $UID procon
RUN chown procon:procon -R $PROCONPATH
RUN chmod -R 0777 $PROCONPATH

WORKDIR $PROCONPATH

COPY ./entrypoint.sh $PROCONPATH
RUN chmod +x ./entrypoint.sh

USER procon:procon

VOLUME ["$PROCONPATH/Configs", "$PROCONPATH/Plugins", "$PROCONPATH/Logs"]
