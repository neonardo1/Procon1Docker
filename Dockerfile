FROM mono:latest

ARG UID=10000
ARG GID=10000
ARG PROCONPATH="/opt/procon"
ARG FILE="procon.zip"
ARG DLURL="https://api.myrcon.net/procon/download?p=docker"

RUN mkdir -p $PROCONPATH
RUN apt-get update && apt-get install unzip wget -yqq
RUN wget -q -O /tmp/$FILE $DLURL && unzip -x /tmp/$FILE -d $PROCONPATH
RUN rm -f /tmp/$FILE

RUN groupadd -r -g $GID procon && useradd -r -g procon -u $UID procon
RUN chown procon:procon -R $PROCONPATH

WORKDIR $PROCONPATH

USER procon:procon

VOLUME ["$PROCONPATH/Configs", "$PROCONPATH/Plugins"]

CMD [ "mono",  "./PRoCon.Console.exe" ]