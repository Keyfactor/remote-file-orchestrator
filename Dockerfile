FROM mcr.microsoft.com/dotnet/sdk:7.0-jammy

# Set ARGs
ARG COMMAND_USERNAME=""
ARG COMMAND_PASSWORD=""
ARG COMMAND_HOSTNAME=""
ARG COMMAND_DOMAIN=""
ARG UO_VER=10.1.0
ARG UO_PREFIX="container_"
ARG UO_SUFFIX=""
ARG UO_SVC_USERNAME="keyfactor-orchestrator"
ARG UO_SVC_NAME="keyfactor-orchestrator-default"
ARG UO_SVC_DIR="/opt/keyfactor/orchestrator"
ARG UOEXT_DIR="/opt/keyfactor/orchestrator/extensions"
ARG UOEXT_NAME="remote-file-orchestrator"
ARG UOEXT_VERSION="1.1.0"
ARG UOEXT_FOLDER_NAME="RemoteFileOrchestrator"
ARG KFUTIL_VER=0.3.0
ARG DEBUG=false

# Set ENVs
#ENV KEYFACTOR_USERNAME=$COMMAND_USERNAME
#ENV KEYFACTOR_PASSWORD=$COMMAND_PASSWORD
ENV KEYFACTOR_HOSTNAME=$COMMAND_HOSTNAME
ENV KEYFACTOR_DOMAIN=$COMMAND_DOMAIN
ENV UO_VERSION=$UO_VER
ENV UO_NAME_PREFIX=$UO_PREFIX
ENV UO_NAME_SUFFIX=$UO_SUFFIX
ENV UO_SERVICE_ACC_USERNAME=$UO_SVC_USERNAME
ENV UO_SERVICE_NAME=$UO_SVC_NAME
ENV UO_SERVICE_DIR=$UO_SVC_DIR
ENV UO_EXT_DIR=$UOEXT_DIR
ENV UO_EXT_NAME=$UOEXT_NAME
ENV UO_EXT_VERSION=$UOEXT_VERSION
ENV UO_EXT_FOLDER_NAME=$UOEXT_FOLDER_NAME
ENV KFUTIL_VERSION=$KFUTIL_VER
ENV DEBUG=$DEBUG

# Install OS dependencies
#RUN apk add --no-cache --update \
#    bash \
#    curl \
#    jq \
#    unzip \
#    git \
#    github-cli \
#    rsync \
#    ca-certificates \
#    openssh-server \
#    mono \
#    && update-ca-certificates
RUN echo 'Package: *\nPin: origin "packages.microsoft.com"\nPin-Priority: 1001\n' >> /etc/apt/preferences.d/99microsoft-dotnet.pref
RUN apt remove --yes dotnet* aspnetcore* netstandard* && \
    set -e; \
    apt update && apt upgrade --yes && \
    DEBIAN_FRONTEND=noninteractive apt install --no-install-recommends --yes \
        apt-transport-https \
        bash \
        ca-certificates \
        curl \
        dotnet-runtime-6.0 \
        dotnet-sdk-6.0 \
        git \
        gh \
        jq \
        mono-complete \
        rsync \
        unzip

# Download Keyfactor Universal Orchestrator Service
RUN curl -O -J -L \
      https://gitlab.com/spbsoluble/keyfactor-uo-releases/-/raw/main/KeyfactorUniversalOrchestrator-${UO_VERSION}.zip?inline=false && \
    mkdir -p /opt/KeyfactorUniversalOrchestrator-${UO_VERSION} && \
    unzip -u KeyfactorUniversalOrchestrator-${UO_VERSION}.zip -d /opt/KeyfactorUniversalOrchestrator-${UO_VERSION}/ && \
    chmod oug+x /opt/KeyfactorUniversalOrchestrator-${UO_VERSION}/*.sh

# Install Keyfactor Universal Orchestrator Service
WORKDIR /opt/KeyfactorUniversalOrchestrator-${UO_VERSION}
COPY install.sh /opt/KeyfactorUniversalOrchestrator-${UO_VERSION}/install.sh
RUN echo $HOSTNAME && \
    echo $KEYFACTOR_HOSTNAME
RUN bash install.sh \
  --force \
  --username "${KEYFACTOR_USERNAME}" \
  --password "${KEYFACTOR_PASSWORD}" \
  --url "https://${KEYFACTOR_HOSTNAME}/KeyfactorAgents" \
  --orchestrator-name "${UO_NAME_PREFIX}${HOSTNAME}${UO_NAME_SUFFIX}" \
  --capabilities all

# Ensure service account home and service directories exist and are owned by the service account
RUN mkdir -p /home/${UO_SERVICE_ACC_USERNAME} && \
    chown -R ${UO_SERVICE_ACC_USERNAME}:${UO_SERVICE_ACC_USERNAME} /home/${UO_SERVICE_ACC_USERNAME} && \
    chmod -R 700 /home/${UO_SERVICE_ACC_USERNAME} && \
    chown -R ${UO_SERVICE_ACC_USERNAME}:${UO_SERVICE_ACC_USERNAME} ${UO_SERVICE_DIR}

## Download Keyfactor Universal Orchestrator Extension \
#RUN curl -O -J -L\
#      https://github.com/Keyfactor/${UO_EXT_NAME}/releases/download/${UO_EXT_VERSION}/${UO_EXT_NAME}_${UO_EXT_VERSION}.zip && \
#    mkdir -p ${UO_EXT_NAME}_${UO_EXT_VERSION} && ls -la && \
#    unzip -u ${UO_EXT_NAME}_${UO_EXT_VERSION}.zip -d ${UO_EXT_DIR}/${UO_EXT_FOLDER_NAME}/ && \
#    mv ${UO_EXT_DIR}/${UO_EXT_FOLDER_NAME}/Release/* ${UO_EXT_DIR}/${UO_EXT_FOLDER_NAME}/ || true && \
#    rm -rf ${UO_EXT_DIR}/${UO_EXT_FOLDER_NAME}/Release || true && \
#    mv ${UO_EXT_DIR}/${UO_EXT_FOLDER_NAME}/netcoreapp3.1/* ${UO_EXT_DIR}/${UO_EXT_FOLDER_NAME}/ || true && \
#    rm -rf ${UO_EXT_DIR}/${UO_EXT_FOLDER_NAME}/netcoreapp3.1 || true && \
#    chown -R ${UO_SERVICE_ACC_USERNAME}:${UO_SERVICE_ACC_USERNAME} ${UO_EXT_DIR}/${UO_EXT_FOLDER_NAME}/

# Set logging to trace if DEBUG is set to true
RUN if [ "${DEBUG}" = "true" ]; then sed -i 's/"Info"/"Trace"/g' ${UO_SERVICE_DIR}/configuration/nlog.json; fi

# Runtime Image Stage
#FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS runtime
# Become service account
USER ${UO_SERVICE_ACC_USERNAME}
# set workdir
WORKDIR ${UO_SERVICE_DIR}

# Start Keyfactor Universal Orchestrator Service in the foreground.
#ENTRYPOINT ["dotnet", "./Orchestrator.dll"]
# The above is commented out as the orchestrator will fail to start having not provided any secrets to connect to
# Keyfactor Command. It is NOT recommended to bake secrets into the image. Instead, secrets should be provided at runtime
# via environment variables or a secrets manager.

## Use this to debug the container environmentals and verify KEYFACTOR_HOSTNAME is set
#ENTRYPOINT ["/bin/bash", "-c", "printenv && dotnet ./Orchestrator.dll"]

EXPOSE 50000
EXPOSE 50022