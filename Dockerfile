# Start from the sdk image
FROM mcr.microsoft.com/dotnet/sdk:6.0.401-cbl-mariner2.0@sha256:d77f43f6584085f89c8d964e8818369af4d14f6ab440b05c31e5117191f29f1d AS build
ARG workingDir="/app"

WORKDIR "$workingDir"
COPY . $workingDir

RUN set -x && dotnet build metrics.sln -c "Release" -o "/build"


# Use the SDK image to create the nonroot user and group
FROM mcr.microsoft.com/dotnet/aspnet:6.0.9-cbl-mariner2.0-amd64@sha256:c32a2611428ef23455e7551d7d654f3d657505fe46c7d41d8541e2b4a10f65eb AS users
RUN tdnf install shadow-utils -y && \
  tdnf clean all

RUN groupadd nonroot -g 1000 && useradd -r -M -s /sbin/nologin -g nonroot -c nonroot nonroot -u 1000

FROM scratch AS nonroot
COPY --from=users /etc/group /etc/group
COPY --from=users /etc/passwd /etc/passwd

# Copy the published application
FROM mcr.microsoft.com/dotnet/aspnet:6.0.9-cbl-mariner2.0-amd64@sha256:c32a2611428ef23455e7551d7d654f3d657505fe46c7d41d8541e2b4a10f65eb AS runtime

COPY --from=nonroot / /

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
USER nonroot