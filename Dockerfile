### BUILD
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /source

# Prerequisites
COPY utils/* utils/
RUN utils/install-all-prereqs.sh

# Copy csproj and restore as distinct layers
# ... sources
COPY src/PockerTime.Application/*.csproj src/PockerTime.Application/
COPY src/PockerTime.Common/*.csproj src/PockerTime.Common/
COPY src/PockerTime.Domain/*.csproj src/PockerTime.Domain/
COPY src/PockerTime.Infrastructure/*.csproj src/PockerTime.Infrastructure/
COPY src/PockerTime.Persistence/*.csproj src/PockerTime.Persistence/
COPY src/PockerTime.Web/*.csproj src/PockerTime.Web/
COPY src/*.props src/

# ... tests
COPY tests/PockerTime.Application.Tests.Unit/*.csproj tests/PockerTime.Application.Tests.Unit/
COPY tests/PockerTime.Domain.Tests.Unit/*.csproj tests/PockerTime.Domain.Tests.Unit/
COPY tests/PockerTime.Web.Tests.Unit/*.csproj tests/PockerTime.Web.Tests.Unit/
COPY tests/PockerTime.Web.Tests.Integration/*.csproj tests/PockerTime.Web.Tests.Integration/
COPY tests/*.props tests/

COPY *.sln .
COPY *.props .
COPY dotnet-tools.json .
RUN dotnet restore
RUN dotnet tool restore

# Yarn (although it isn't as large, still worth caching)
#COPY package.json src/PockerTime.Web/
#COPY yarn.lock src/PockerTime.Web/
RUN yarn --cwd src/PockerTime.Web/

## Skip build script pre-warm
## This causes later invocations of the build script to fail with "Failed to uninstall tool package 'cake.tool': Invalid cross-device link"
#COPY build.* .
#RUN ./build.sh --target=restore-node-packages

### TEST
FROM build-env AS test

# ... run tests
COPY . .
ENV RETURN_TEST_WAIT_TIME 30
ENV SCREENSHOT_TEST_FAILURE_TOLERANCE True
RUN ./build.sh --target=test

### PUBLISHING
FROM build-env AS publish

# ... run publish
COPY . .
RUN ./build.sh --target=Publish-Ubuntu-22.04-x64 --publish-dir=publish --verbosity=verbose --skip-compression=true

### RUNTIME IMAGE
FROM mcr.microsoft.com/dotnet/runtime-deps:6.0
WORKDIR /app

# ... Run libgdi install
COPY utils/install-app-prereqs.sh utils/
RUN bash utils/install-app-prereqs.sh

# ... Copy published app
COPY --from=publish /source/publish/ubuntu.22.04-x64/ .

ENV ASPNETCORE_ENVIRONMENT Production

# Config directory
VOLUME ["/etc/PockerTime"]

# Set some defaults for a "direct run" experience
ENV DATABASE__DATABASE "/app/data.db"
ENV DATABASE__DATABASEPROVIDER Sqlite

# ... enable proxy mode
ENV SECURITY__ENABLEPROXYMODE True

# ... health check
HEALTHCHECK CMD curl --fail http://localhost/health || exit

ENTRYPOINT [ "./launch", "run" ]