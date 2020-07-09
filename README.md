# Introduction

More to be written later.

# Using the setup script

```
git clone https://github.com/AdKats/Procon1Docker.git && cd Procon1Docker && touch .env
```

Edit the `.env` file and put this in. Fill out the information for your game server.

```
LAYER_PORT=
PROCON_GAMESERVER_IP=
PROCON_GAMESERVER_PORT=
PROCON_GAMESERVER_PASS=
```

# Getting Started

#### Running the docker
```
docker-compose -p "NameLayerInstance" up -d
```

#### Restarting the docker
```
docker-compose restart
```

#### Stopping the docker
```
docker-compose stop
```
