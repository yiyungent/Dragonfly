#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
# ע��: ���� Ϊ��ʹ�� Selenium
# ���� tools/selenium/ �ļ����µ��ļ��� ./tools/selenium/ �� /app/tools/selenium/
COPY tools/selenium/ ./tools/selenium/
RUN apt update
RUN apt-get install unzip
RUN apt-get install gdebi-core -y
WORKDIR /app/tools/selenium
# ��װ Chrome
# ע��: Chrome �汾������ chromedriver �汾��Ӧ
# ��װ google-chrome*.deb ������
# ע��: gdebi û�� -y, ʹ�� -n ����
RUN gdebi -n google-chrome*.deb
# ��װ chromedriver
RUN unzip chromedriver_linux64.zip
# Ϊ�����û���ӿ�ִ��Ȩ�� (��chromedriver�ļ�)
RUN chmod a+x chromedriver
# �������а�װ��������
RUN apt install -y --force-yes --no-install-recommends fonts-wqy-microhei
RUN apt install -y --force-yes --no-install-recommends ttf-wqy-zenhei
# ���°�װ Selenium WebDriver ��Ҫ������
RUN apt-get install libglib2.0 -y
RUN apt-get install libnss3-dev -y
RUN apt-get install libxcb1 -y

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/WebScreenshot/WebScreenshot.csproj", "src/WebScreenshot/"]
RUN dotnet restore "src/WebScreenshot/WebScreenshot.csproj"
COPY . .
WORKDIR "/src/src/WebScreenshot"
RUN dotnet build "WebScreenshot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebScreenshot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebScreenshot.dll"]
