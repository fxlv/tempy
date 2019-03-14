FROM microsoft/dotnet
MAINTAINER Kaspars Mickevics (kaspars@fx.lv)
RUN git clone https://github.com/fxlv/tempy.git
WORKDIR tempy
RUN dotnet restore
RUN dotnet build
RUN dotnet test