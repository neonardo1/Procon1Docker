FROM mono:latest

RUN mkdir -p /procon && \
	apt-get update && \
	apt-get install unzip wget -y && \
	wget -O /tmp/procon.zip https://api.myrcon.net/procon/download?p=docker && \
	unzip -x /tmp/procon.zip -d /procon/ && \
    rm -f /tmp/procon.zip
	
WORKDIR /procon

VOLUME /procon/Configs ./Configs
VOLUME /procon/Plugins ./Plugins

EXPOSE 27260

CMD [ "mono",  "./PRoCon.Console.exe" ]
