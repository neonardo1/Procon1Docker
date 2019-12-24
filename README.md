# Introduction

More to be written later.

# Using the setup script

```
git clone https://github.com/AdKats/Procon1Docker.git && \
cd Procon1Docker && \
./setup.sh -d /path/to/where/you/want/configs -i InstanceName \
-gip "GameServerIP" -gp "GameServerRconPort" -grp "RconPassword" \
-p 27260
```

The `-p` flag is what the port docker will expose. All dockers will
reference port `27260` internally. So if you have multiple servers then
make sure change the `-p` flag to any open port on your system.

So if you want to use port `27356` then set it to `-p 27356`. It will be
mapped to the internal port `27260`

`27356 => 27260`

# Getting Started

#### Running the docker
```
docker-compose -f gsp.yml up -d
```

#### Restarting the docker
```
docker-compose -f gsp.yml restart
```

#### Stopping the docker
```
docker-compose -f gsp.yml down
```

#### Updating the docker
```
chmod -R 0777 /path/to/where/you/want/configs && \
docker-compose -f gsp.yml up -d --build
```