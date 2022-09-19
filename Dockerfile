# Start from the sdk image
FROM mcr.microsoft.com/dotnet/sdk:6.0.400-cbl-mariner2.0@sha256:b537aba85bb5f051c736bf4fe05840e6e125ea7b3e346754c289d9addff80dbc AS build
ARG workingDir="/app"

WORKDIR "$workingDir"
COPY . $workingDir

RUN set -x && dotnet build metrics.sln -c "Release" -o "/build"


# Copy the published application
FROM mcr.microsoft.com/dotnet/aspnet:6.0.8-cbl-mariner2.0-amd64@sha256:0144a026361143348421f4fea488146d53d8f397550a1bff26e71d6f62b1959c AS runtime

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

EXPOSE 9184
