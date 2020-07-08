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

_create_config() {
GAMESERVER_FOLDER_NAME="${PROCON_GAMESERVER_IP}_${PROCON_GAMESERVER_PORT}"
GAMESERVER_FILE_NAME="${GAMESERVER_FOLDER_NAME}.cfg"

    if [[ ! -d "/opt/procon/Configs/${GAMESERVER_FOLDER_NAME}" ]] ; then
        mkdir -p "/opt/procon/Configs/${GAMESERVER_FOLDER_NAME}"

        cat << EOF > /opt/procon/Configs/procon.cfg
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

        cat << EOF > /opt/procon/Configs/accounts.cfg
/////////////////////////////////////////////
// This config will be overwritten by procon.
////////////////////////////////////////////
procon.public.accounts.create "DockerAdmin" "admin"

EOF
    fi
}

if [[ ! -f /opt/procon/Configs/.initialized ]]; then
    echo "Initializing container"
    _create_config
    # run initializing commands
    touch /opt/procon/Configs/.initialized
fi

mono ./PRoCon.Console.exe
