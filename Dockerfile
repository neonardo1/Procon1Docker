FROM mono:latest

ARG UID=10000
ARG GID=10000
ARG PROCONPATH="/opt/procon"
ARG FILE="procon.zip"
ARG DLURL="https://api.myrcon.net/procon/download"

ARG GAMESERVER_FOLDER_NAME="${PROCON_GAMESERVER_IP}_${PROCON_GAMESERVER_PORT}"
ARG GAMESERVER_FILE_NAME="${GAMESERVER_FOLDER_NAME}.cfg"

RUN mkdir -p $PROCONPATH
RUN apt-get update && apt-get install unzip wget -yqq
RUN wget -q -O /tmp/$FILE $DLURL && unzip -x /tmp/$FILE -d $PROCONPATH
RUN rm -f /tmp/$FILE

RUN groupadd -r -g $GID procon && useradd -r -g procon -u $UID procon
RUN chown procon:procon -R $PROCONPATH
RUN chmod -R 0777 $PROCONPATH

WORKDIR $PROCONPATH

RUN echo "procon.public.accounts.create \"DockerAdmin\" \"admin\"" >> "$PROCONPATH/Configs/accounts.cfg"

RUN echo $'procon.private.servers.add "${PROCON_GAMESERVER_IP}" ${PROCON_GAMESERVER_PORT} "${PROCON_GAMESERVER_PASS}"\n\
procon.private.servers.name "${PROCON_GAMESERVER_IP}" ${PROCON_GAMESERVER_PORT}\n\
procon.private.servers.autoconnect "${PROCON_GAMESERVER_IP}" ${PROCON_GAMESERVER_PORT}\n'\
>> "$PROCONPATH/Configs/procon.cfg"

RUN echo $'procon.protected.layer.setPrivileges "${PROCON_DEFAULT_LAYER_USER}" 4185975\n\
procon.protected.layer.enable True 27260 "0.0.0.0" "PRoCon[%servername%]"\n'\
>> "$PROCONPATH/Configs/${GAMESERVER_FOLDER_NAME}/${GAMESERVER_FILE_NAME}"

USER procon:procon

VOLUME ["$PROCONPATH/Configs", "$PROCONPATH/Plugins", "$PROCONPATH/Logs"]

CMD [ "mono",  "./PRoCon.Console.exe" ]
