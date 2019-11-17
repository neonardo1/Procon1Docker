FROM mono:latest

RUN mkdir -p /procon && \
	apt-get update && \
	apt-get install unzip wget -y && \
	wget -O /tmp/procon.zip https://api.myrcon.net/procon/download && \
	unzip -x /tmp/procon.zip -d /procon/ && \
    rm -f /tmp/procon.zip
	
WORKDIR /procon

ADD Configs/ /procon
ADD Plugins/ /procon

EXPOSE 27260

CMD [ "mono",  "./PRoCon.Console.exe" ]
