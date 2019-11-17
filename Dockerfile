FROM mono:latest

ARG UID=10000
ARG GID=10000
ARG PATH=/opt/procon
ARG FILE="procon.zip"
ARG DLURL="https://api.myrcon.net/procon/download?p=docker"

RUN mkdir -p $PATH
RUN apt-get update && apt-get install unzip wget -yqq
RUN wget -O /tmp/$FILE $DLURL && unzip -x /tmp/$FILE -d $PATH
RUN rm -f /tmp/$FILE

RUN groupadd -r -g $GID procon && useradd -r -g procon -u $UID procon
RUN chown procon:procon -R $PATH
	
WORKDIR $PATH

USER procon:procon

VOLUME ["$PATH/Configs", "$PATH/Plugins"]

EXPOSE 27260

CMD [ "mono",  "./PRoCon.Console.exe" ]