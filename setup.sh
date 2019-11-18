#!/usr/bin/env bash

BASE_DOCKER_REPO="/opt/Procon1Docker"
TARGET_DIR=""
INSTANCE_NAME=""
DOCKER_GSP_CONFIG_PATH=""
DOCKER_COMPOSE_FILE="${BASE_DOCKER_REPO}/docker-compose.yml"
PROCON_LAYER_PORT=27260


while [[ -n "$1" ]]; do

    case "$1" in

    -d)
        TARGET_DIR="$2"
     ;;

    -i)
        INSTANCE_NAME="$2"
    ;;

    -c)
        DOCKER_GSP_CONFIG_PATH="$2"
     ;;

    -p)
        PROCON_LAYER_PORT="$2"
     ;;

    -z)
        BASE_DOCKER_REPO="$2"
     ;;

    *) echo "Option $1 not recognized" ;;

    esac
    shift
done

# Remove a trailing slash if it was provided
TARGET_DIR=${TARGET_DIR%/}

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
        else
            echo "Failed to create directory"
            exit
        fi
    fi
fi

ENV_INSTANCE_NAME=${INSTANCE_NAME}
ENV_LAYER_PORT=${PROCON_LAYER_PORT}
ENV_CONFIG_PATH=${TARGET_DIR}/Config
ENV_PLUGINS_PATH=${TARGET_DIR}/Plugins
ENV_LOGS_PATH=${TARGET_DIR}/Logs

cat << EOF > ${TARGET_DIR}/.env
INSTANCE_NAME=${ENV_INSTANCE_NAME}
# This can be any available port
LAYER_PORT=${ENV_LAYER_PORT}
CONFIG_PATH=${ENV_CONFIG_PATH}
PLUGINS_PATH=${ENV_PLUGINS_PATH}
LOGS_PATH=${ENV_LOGS_PATH}
EOF

cp -r ${BASE_DOCKER_REPO}/Configs/ ${ENV_CONFIG_PATH}
cp -r ${BASE_DOCKER_REPO}/Plugins/ ${ENV_PLUGINS_PATH}

cat << EOF > ${TARGET_DIR}/gsp.yml
version: '2.4'
services:
  gsp:
    extends:
      file: ${DOCKER_COMPOSE_FILE}
      service: procon
    env_file:
      - .env
EOF