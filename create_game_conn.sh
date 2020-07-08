#!/usr/bin/env bash

PROCON_GAMESERVER_IP=""
PROCON_GAMESERVER_PORT=""
PROCON_GAMESERVER_PASS=""

while [[ -n "$1" ]]; do

    case "$1" in
    -gip)
        PROCON_GAMESERVER_IP="$2"
     ;;

    -gp)
        PROCON_GAMESERVER_PORT="$2"
    ;;
    -grp)
        PROCON_GAMESERVER_PASS="$2"
    ;;

    esac
    shift
done

GAMESERVER_FOLDER_NAME="${PROCON_GAMESERVER_IP}_${PROCON_GAMESERVER_PORT}"
GAMESERVER_FILE_NAME="${GAMESERVER_FOLDER_NAME}.cfg"

cat << EOF >> /opt/procon/Configs/procon.cfg
procon.private.servers.add "${PROCON_GAMESERVER_IP}" ${PROCON_GAMESERVER_PORT} "${PROCON_GAMESERVER_PASS}"
procon.private.servers.name "${PROCON_GAMESERVER_IP}" ${PROCON_GAMESERVER_PORT}
procon.private.servers.autoconnect "${PROCON_GAMESERVER_IP}" ${PROCON_GAMESERVER_PORT}
EOF

cat << EOF > /opt/procon/Configs/${GAMESERVER_FOLDER_NAME}/${GAMESERVER_FILE_NAME}
/////////////////////////////////////////////
// This config will be overwritten by procon.
/////////////////////////////////////////////
procon.protected.layer.setPrivileges "DockerAdmin" 4185975
procon.protected.layer.enable True 27260 "0.0.0.0" "PRoCon[%servername%]"
procon.protected.playerlist.settings true 1 0.5 0.5
procon.protected.chat.settings False False True 0 0 False False
procon.protected.events.captures False 200 False False
procon.protected.lists.settings False
procon.protected.console.settings True False False True True True
procon.protected.timezone_UTCoffset 0
EOF
