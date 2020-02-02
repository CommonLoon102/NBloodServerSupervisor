FROM buildpack-deps:bionic-scm

ENV \
    # Enable detection of running in a container
    DOTNET_RUNNING_IN_CONTAINER=true \
    # Enable correct mode for dotnet watch (only mode supported in a container)
    DOTNET_USE_POLLING_FILE_WATCHER=true \
    # Skip extraction of XML docs - generally not useful within an image/container - helps performance
    NUGET_XMLDOC_MODE=skip \
    # Stop M$ snooping
    DOTNET_CLI_TELEMETRY_OPTOUT=true

# Install .NET CLI dependencies
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        libc6 \
        libgcc1 \
        libgssapi-krb5-2 \
        libicu60 \
        libssl1.1 \
        libstdc++6 \
        zlib1g \
    && rm -rf /var/lib/apt/lists/*

# Install .NET Core SDK
RUN dotnet_sdk_version=3.1.101 \
    && curl -SL --output dotnet.tar.gz https://dotnetcli.azureedge.net/dotnet/Sdk/$dotnet_sdk_version/dotnet-sdk-$dotnet_sdk_version-linux-x64.tar.gz \
    && dotnet_sha512='eeee75323be762c329176d5856ec2ecfd16f06607965614df006730ed648a5b5d12ac7fd1942fe37cfc97e3013e796ef278e7c7bc4f32b8680585c4884a8a6a1' \
    && echo "$dotnet_sha512 dotnet.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -ozxf dotnet.tar.gz -C /usr/share/dotnet \
    && rm dotnet.tar.gz \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    # Trigger first run experience by running arbitrary cmd
    && dotnet help

# Install toolchain to build NBlood and the supervisor
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        build-essential \
        git \
        libgl1-mesa-dev \
        libglu1-mesa-dev \
        libsdl-mixer1.2-dev \
        libsdl1.2-dev \
        libsdl2-dev \
        libsdl2-mixer-dev \
        nano \
        nasm \
        nginx \
        nginx-extras \
    && rm -rf /var/lib/apt/lists/*

# Installing the NBlood supervisor related things
WORKDIR /supervisor
RUN mkdir -p publish/blood

# Disable cache from this point
ARG CACHEBUST

# Clone NBloodServerSupervisor
RUN git clone https://github.com/CommonLoon102/NBloodServerSupervisor.git

# Build NBloodServerSupervisor
RUN dotnet publish NBloodServerSupervisor/NBloodServerSupervisor.sln --configuration Release --output publish --self-contained false --runtime linux-x64 \
    && sed -i -e "s/CHANGEME/$(< /dev/urandom tr -dc A-Za-z0-9 | head -c${1:-32};echo;)/g" publish/appsettings.json

# Clone NBlood
RUN git clone https://github.com/CommonLoon102/NBlood.git

# Build NBlood
RUN cd NBlood \
    && git checkout norender \
    && make blood NORENDER=1

# Configure nginx
RUN printf '\
server { \n\
    listen        23580; \n\
    location / { \n\
        proxy_pass         http://localhost:5000; \n\
        proxy_http_version 1.1; \n\
        proxy_set_header   Upgrade $http_upgrade; \n\
        proxy_set_header   Connection keep-alive; \n\
        proxy_set_header   Host $host; \n\
        proxy_cache_bypass $http_upgrade; \n\
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for; \n\
        proxy_set_header   X-Forwarded-Proto $scheme; \n\
    } \n\
    location ~/cp.mp4 { \n\
        root               /supervisor/publish/wwwroot; \n\
        mp4; \n\
    } \n\
    location ~/favicon.ico { \n\
        root               /supervisor/publish/wwwroot; \n\
    } \n\
}' > /etc/nginx/sites-available/default && service nginx start && nginx -t && nginx -s reload

CMD service nginx start \
    && cp /supervisor/NBlood/nblood /supervisor/publish/blood/nblood_server \
    && dotnet /supervisor/publish/WebInterface.dll