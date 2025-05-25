# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project and restore
COPY ./ViolationEditorApi/*.csproj ./ViolationEditorApi/
RUN dotnet restore ./ViolationEditorApi/ViolationEditorApi.csproj

# Copy all files and build
COPY ./ViolationEditorApi/ ./ViolationEditorApi/
WORKDIR /app/ViolationEditorApi
RUN dotnet publish -c Release -o /out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out ./

ENTRYPOINT ["dotnet", "ViolationEditorApi.dll"]

ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
