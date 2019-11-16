FROM mono:latest

RUN mkdir -p release

CMD [ "mono",  "./PRoCon.Console.exe" ]
