#!/bin/bash

BASE_DOCKER_REPO="$(pwd)"
TARGET_DIR=""
INSTANCE_NAME=""
DOCKER_GSP_CONFIG_PATH=""
DOCKER_COMPOSE_FILE="${BASE_DOCKER_REPO}/docker-compose.yml"
PROCON_LAYER_PORT=27260
PROCON_GAMESERVER_IP=""
PROCON_GAMESERVER_PORT=""
PROCON_GAMESERVER_PASS=""
PROCON_DEFAULT_LAYER_USER="DockerAdmin"
PROCON_DEFAULT_LAYER_PASS="admin"

while getopts lr:dt arg
do 
    case $arg in
        d) TARGET_DIR="${OPTARG}";;
        i) INSTANCE_NAME="${OPTARG}";;
        c) DOCKER_GSP_CONFIG_PATH="${OPTARG}";;
        p) PROCON_LAYER_PORT="${OPTARG}";;
        gip) PROCON_GAMESERVER_IP="${OPTARG}";;
        gp) PROCON_GAMESERVER_PORT="${OPTARG}";;
        grp) PROCON_GAMESERVER_PASS="${OPTARG}";;
        z) BASE_DOCKER_REPO="${OPTARG}";;
    esac
done

# Remove a trailing slash if it was provided
TARGET_DIR=${TARGET_DIR%/}

GAMESERVER_FOLDER_NAME="${PROCON_GAMESERVER_IP}_${PROCON_GAMESERVER_PORT}"
GAMESERVER_FILE_NAME="${GAMESERVER_FOLDER_NAME}.cfg"

ENV_INSTANCE_NAME=${INSTANCE_NAME}
ENV_LAYER_PORT=${PROCON_LAYER_PORT}
ENV_CONFIG_PATH=${TARGET_DIR}/Configs
ENV_PLUGINS_PATH=${TARGET_DIR}/Plugins
ENV_LOGS_PATH=${TARGET_DIR}/Logs

# Check if the supplied directory path is an empty string
if [[ ${TARGET_DIR} == "" ]]; then
    echo "Target directory path can not be blank."
    exit
else
    # Check if the directory doesn't exist. If it does then skip creating it.
    if [[ ! -d "$TARGET_DIR" ]] ; then
        echo "Target directory does not exist. Creating the directory."
        mkdir -p "$TARGET_DIR"

        # Check to make sure directory was created
        if [[ -d "$TARGET_DIR" ]] ; then
            echo "Directory created at '$TARGET_DIR'"
            echo "Now creating server folder at \"${ENV_CONFIG_PATH}/${GAMESERVER_FOLDER_NAME}\""
            mkdir -p ${ENV_CONFIG_PATH}/${GAMESERVER_FOLDER_NAME}
            mkdir -p ${ENV_LOGS_PATH}

            if [[ -d "${ENV_CONFIG_PATH}/${GAMESERVER_FOLDER_NAME}" ]] ; then
                echo "'${ENV_CONFIG_PATH}/${GAMESERVER_FOLDER_NAME}' created."
            fi
        else
            echo "Failed to create directory"
            exit
        fi
    fi
fi

cat << EOF > ${TARGET_DIR}/.env
INSTANCE_NAME=${ENV_INSTANCE_NAME}
# This can be any available port
LAYER_PORT=${ENV_LAYER_PORT}
CONFIG_PATH=${ENV_CONFIG_PATH}
PLUGINS_PATH=${ENV_PLUGINS_PATH}
LOGS_PATH=${ENV_LOGS_PATH}
EOF

cp -r ${BASE_DOCKER_REPO}/Configs/* ${ENV_CONFIG_PATH}
cp -r ${BASE_DOCKER_REPO}/Plugins/ ${ENV_PLUGINS_PATH}

cat << EOF > ${TARGET_DIR}/gsp.yml
version: '2.4'
services:
  gsp:
    extends:
      file: ${DOCKER_COMPOSE_FILE}
      service: procon
    env_file: .env
EOF

cat << EOF > ${ENV_CONFIG_PATH}/accounts.cfg
/////////////////////////////////////////////
// This config will be overwritten by procon.
/////////////////////////////////////////////
procon.public.accounts.create "${PROCON_DEFAULT_LAYER_USER}" "${PROCON_DEFAULT_LAYER_PASS}"

EOF

cat << EOF > ${ENV_CONFIG_PATH}/procon.cfg
/////////////////////////////////////////////
// This config will be overwritten by procon.
/////////////////////////////////////////////
procon.private.window.position Normal 193 42 1536 858
procon.private.options.setLanguage "au.loc"
procon.private.options.chatLogging False
procon.private.options.consoleLogging False
procon.private.options.eventsLogging False
procon.private.options.pluginLogging False
procon.private.options.autoCheckDownloadUpdates True
procon.private.options.autoApplyUpdates False
procon.private.options.autoCheckGameConfigsForUpdates True
procon.private.options.showtrayicon True
procon.private.options.minimizetotray False
procon.private.options.closetotray False
procon.private.options.allowanonymoususagedata True
procon.private.options.runPluginsInSandbox False
procon.private.options.allowAllODBCConnections False
procon.private.options.allowAllSmtpConnections False
procon.private.options.adminMoveMessage True
procon.private.options.chatDisplayAdminName True
procon.private.options.EnableAdminReason False
procon.private.options.layerHideLocalPlugins True
procon.private.options.layerHideLocalAccounts True
procon.private.options.ShowRoundTimerConstantly False
procon.private.options.ShowCfmMsgRoundRestartNext True
procon.private.options.ShowDICESpecialOptions False
procon.private.httpWebServer.enable False 27360 "0.0.0.0"
procon.private.options.trustedHostDomainsPorts
procon.private.options.statsLinkNameUrl Metabans http://metabans.com/search/%player_name%
procon.private.options.pluginMaxRuntime 0 59
procon.private.options.UsePluginOldStyleLoad False
procon.private.options.enablePluginDebugging False
procon.private.servers.add "${PROCON_GAMESERVER_IP}" ${PROCON_GAMESERVER_PORT} "${PROCON_GAMESERVER_PASS}"
procon.private.servers.name "${PROCON_GAMESERVER_IP}" ${PROCON_GAMESERVER_PORT} ""
procon.private.servers.autoconnect "${PROCON_GAMESERVER_IP}" ${PROCON_GAMESERVER_PORT}
EOF

cat << EOF > ${ENV_CONFIG_PATH}/${GAMESERVER_FOLDER_NAME}/${GAMESERVER_FILE_NAME}
/////////////////////////////////////////////
// This config will be overwritten by procon.
/////////////////////////////////////////////
procon.protected.layer.setPrivileges "${PROCON_DEFAULT_LAYER_USER}" 4185975
procon.protected.layer.enable True 27260 "0.0.0.0" "PRoCon[%servername%]"
procon.protected.playerlist.settings true 1 0.5 0.5
procon.protected.chat.settings False False True 0 0 False False
procon.protected.events.captures False 200 False False
procon.protected.lists.settings False
procon.protected.console.settings True False False True True True
procon.protected.timezone_UTCoffset 0
EOF
