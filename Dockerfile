FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

RUN set -uex \
        && apt-get update \
        && apt-get install -y ca-certificates curl gnupg \
        && mkdir -p /etc/apt/keyrings \
        && curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key \
         | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg \
        && NODE_MAJOR=20 \
        && echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_$NODE_MAJOR.x nodistro main" \
         | tee /etc/apt/sources.list.d/nodesource.list \
        && apt-get update \
        && apt-get install nodejs -y;

WORKDIR /src
COPY ["contester/contester.csproj", "contester/"]
RUN dotnet restore "contester/contester.csproj"
COPY . .
WORKDIR "/src/contester"
RUN dotnet build "contester.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "contester.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS runtime

WORKDIR /app

RUN apt-get update \
        && apt-get install -y wget unzip libaio1 \
        && wget -O oracle-instantclient-basic.zip https://download.oracle.com/otn_software/linux/instantclient/2350000/instantclient-basic-linux.x64-23.5.0.24.07.zip \
        && wget -O oracle-instantclient-sqlplus.zip https://download.oracle.com/otn_software/linux/instantclient/2350000/instantclient-sqlplus-linux.x64-23.5.0.24.07.zip \
        && DESTINATION_FOLDER=/opt/oracle \
        && mkdir -p $DESTINATION_FOLDER \
        && unzip -qo ./oracle-instantclient-basic.zip -d $DESTINATION_FOLDER \
        && unzip -qo ./oracle-instantclient-sqlplus.zip -d $DESTINATION_FOLDER  \
        && rm oracle-instantclient-basic.zip oracle-instantclient-sqlplus.zip \
        && echo "export PATH=$DESTINATION_FOLDER/instantclient_23_5:\$PATH" >> /etc/profile \
        && echo "export LD_LIBRARY_PATH=$DESTINATION_FOLDER/instantclient_23_5:\$LD_LIBRARY_PATH" >> /etc/profile;

COPY ["contester/Assets/requirements.txt", "Assets/"]

RUN set -uex \
        && apt-get update \
        && apt-get install -y python3.11 python3-pip python3.11-venv \
        && python3.11 -m venv /opt/venv \
        && /opt/venv/bin/pip install -r Assets/requirements.txt \
        && rm -rf /var/lib/apt/lists/*;

COPY --from=publish /app/publish .
COPY ["certs/localhost.pfx", "/storage/localhost.pfx"]
ENTRYPOINT ["dotnet", "contester.dll"]
