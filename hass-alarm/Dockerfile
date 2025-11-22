ARG BUILD_FROM=ghcr.io/hassio-addons/base:15.0.1
# hadolint ignore=DL3006
FROM ${BUILD_FROM}

# Set shell
SHELL ["/bin/bash", "-o", "pipefail", "-c"]

# Install .NET Core Runtime
ARG BUILD_ARCH
RUN \
    apk add --no-cache \
        icu-libs=74.1-r0 \
        krb5-libs=1.21.2-r0 \
        libgcc=13.2.1_git20231014-r0 \
        libintl=0.22.3-r0 \
        libssl3=3.1.4-r5 \
        libstdc++=13.2.1_git20231014-r0 \
        zlib=1.3-r2 \
    && \
    DOTNET_FILE="dotnet.tar.gz" \
    && \
    case "${BUILD_ARCH}" in \
        aarch64) DOTNET_ARCH="arm64" ;; \
        armv7|armhf) DOTNET_ARCH="arm" ;; \
        amd64) DOTNET_ARCH="x64" ;; \
        i386) DOTNET_ARCH="x86" ;; \
        *) echo "Unsupported architecture: ${BUILD_ARCH}" && exit 1 ;; \
    esac \
    && \
    DOTNET_VERSION="3.1.32" \
    && \
    curl -sSL "https://dotnetcli.azureedge.net/dotnet/Runtime/${DOTNET_VERSION}/dotnet-runtime-${DOTNET_VERSION}-linux-musl-${DOTNET_ARCH}.tar.gz" \
        -o "${DOTNET_FILE}" \
    && \
    mkdir -p /usr/share/dotnet \
    && \
    tar -C /usr/share/dotnet -xzf "${DOTNET_FILE}" \
    && \
    ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && \
    rm "${DOTNET_FILE}"

# Set environment
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    DOTNET_RUNNING_IN_CONTAINER=true \
    ASPNETCORE_URLS=http://+:80

# Copy root filesystem
COPY rootfs /

# Build application
WORKDIR /app
COPY Hass-Alarm/Hass-Alarm.csproj ./
RUN dotnet restore || true

COPY Hass-Alarm/ ./
RUN dotnet publish -c Release -o out --no-restore || dotnet publish -c Release -o out

# Setup runtime
FROM ${BUILD_FROM}

# Install .NET Core Runtime
ARG BUILD_ARCH
RUN \
    apk add --no-cache \
        icu-libs=74.1-r0 \
        krb5-libs=1.21.2-r0 \
        libgcc=13.2.1_git20231014-r0 \
        libintl=0.22.3-r0 \
        libssl3=3.1.4-r5 \
        libstdc++=13.2.1_git20231014-r0 \
        zlib=1.3-r2 \
    && \
    DOTNET_FILE="dotnet.tar.gz" \
    && \
    case "${BUILD_ARCH}" in \
        aarch64) DOTNET_ARCH="arm64" ;; \
        armv7|armhf) DOTNET_ARCH="arm" ;; \
        amd64) DOTNET_ARCH="x64" ;; \
        i386) DOTNET_ARCH="x86" ;; \
        *) echo "Unsupported architecture: ${BUILD_ARCH}" && exit 1 ;; \
    esac \
    && \
    DOTNET_VERSION="3.1.32" \
    && \
    curl -sSL "https://dotnetcli.azureedge.net/dotnet/Runtime/${DOTNET_VERSION}/dotnet-runtime-${DOTNET_VERSION}-linux-musl-${DOTNET_ARCH}.tar.gz" \
        -o "${DOTNET_FILE}" \
    && \
    mkdir -p /usr/share/dotnet \
    && \
    tar -C /usr/share/dotnet -xzf "${DOTNET_FILE}" \
    && \
    ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && \
    rm "${DOTNET_FILE}"

# Set environment
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    DOTNET_RUNNING_IN_CONTAINER=true \
    ASPNETCORE_URLS=http://+:80

WORKDIR /app
COPY --from=0 /app/out .
COPY rootfs /

RUN chmod a+x /etc/services.d/hass-alarm/run

# Labels
LABEL \
    io.hass.name="Hass-Alarm Panel" \
    io.hass.description="Advanced alarm control panel with PIN authentication" \
    io.hass.arch="${BUILD_ARCH}" \
    io.hass.type="addon" \
    io.hass.version="${BUILD_VERSION}" \
    maintainer="Hass-Alarm Team" \
    org.opencontainers.image.title="Hass-Alarm Panel" \
    org.opencontainers.image.description="Advanced alarm control panel with PIN authentication" \
    org.opencontainers.image.vendor="Hass-Alarm" \
    org.opencontainers.image.authors="Hass-Alarm Team" \
    org.opencontainers.image.licenses="MIT" \
    org.opencontainers.image.url="https://github.com/werddomain/Hass-Alarm" \
    org.opencontainers.image.source="https://github.com/werddomain/Hass-Alarm" \
    org.opencontainers.image.documentation="https://github.com/werddomain/Hass-Alarm/blob/main/README.md"
