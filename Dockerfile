FROM mono:5.10
RUN mkdir -p release && \
	apt-get update && \
	apt-get install unzip git wget -y && \
	wget -O procon.zip https://api.myrcon.net/procon/download  && \
	unzip -x procon.zip -d release/

CMD [ "mono",  "./release/PRoCon.Console.exe" ]
