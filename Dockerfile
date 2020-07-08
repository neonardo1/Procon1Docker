FROM mono:latest

ARG UID=10000
ARG GID=10000
ARG PROCONPATH="/opt/procon"
ARG FILE="procon.zip"
ARG DLURL="https://api.myrcon.net/procon/download"

RUN mkdir -p $PROCONPATH
RUN apt-get update && apt-get install unzip wget -yqq
RUN wget -q -O /tmp/$FILE $DLURL && unzip -x /tmp/$FILE -d $PROCONPATH
RUN rm -f /tmp/$FILE

RUN groupadd -r -g $GID procon && useradd -r -g procon -u $UID procon
RUN chown procon:procon -R $PROCONPATH
RUN chmod -R 0777 $PROCONPATH

WORKDIR $PROCONPATH

USER procon:procon

VOLUME ["$PROCONPATH/Configs", "$PROCONPATH/Plugins", "$PROCONPATH/Logs"]

CMD [ "mono",  "./PRoCon.Console.exe" ]
