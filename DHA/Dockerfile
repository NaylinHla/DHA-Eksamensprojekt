FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
COPY ["server/Startup/Startup.csproj", "server/Startup/"]
RUN dotnet restore "server/Startup/Startup.csproj" --runtime linux-musl-x64
COPY ["server/", "server/"]
RUN dotnet publish "server/Startup/Startup.csproj" \
    -c Release \
    -o /app/publish \
    --runtime linux-musl-x64 \
    --self-contained true \
    /p:PublishSingleFile=true

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["./Startup"]