# Stage 1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY ./ViolationEditorApi/ ./ViolationEditorApi/
WORKDIR /app/ViolationEditorApi

# 🔧 No restore needed here — done locally
# RUN dotnet restore ./ViolationEditorApi/ViolationEditorApi.csproj

RUN dotnet publish -c Release -o /out

# Stage 2
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out ./
ENTRYPOINT ["dotnet", "ViolationEditorApi.dll"]
