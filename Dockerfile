# Start from the sdk image
FROM mcr.microsoft.com/dotnet/sdk:6.0.401-cbl-mariner2.0@sha256:d77f43f6584085f89c8d964e8818369af4d14f6ab440b05c31e5117191f29f1d AS build
ARG workingDir="/app"

WORKDIR "$workingDir"
COPY . $workingDir

RUN set -x && dotnet build metrics.sln -c "Release" -o "/build"


# Copy the published application
FROM mcr.microsoft.com/dotnet/aspnet:6.0.9-cbl-mariner2.0-amd64@sha256:3f7009d557beb5c42ae18ac485ff390fd3e47266cb30f4d8cf229b7e9f2f9a83 AS runtime

# docs suggest prefer updates over reproducibility in builds
# https://eng.ms/docs/more/containers-secure-supply-chain/updating
# if someone doesnt clean first installs may fail, so for a clean for people incase they forgot
RUN tdnf clean all && tdnf repolist --refresh && tdnf update -y && tdnf clean all

# curl for testing, tar for support of kubectl
RUN tdnf install curl tar -y && \
  tdnf clean all


# region switch to using ubuntu

# FROM mcr.microsoft.com/dotnet/aspnet AS runtime
# RUN apt-get update && apt-get install curl -y 
# RUN curl -sL https://aka.ms/InstallAzureCLIDeb |  bash

# endregion

# Set the working directory and copy the build
WORKDIR /app
COPY --from=build /build .

ENTRYPOINT ["dotnet", "/app/metrics.dll"]

EXPOSE 8080
