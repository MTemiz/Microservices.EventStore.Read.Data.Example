FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Product.Event.Handler.Service/Product.Event.Handler.Service.csproj", "Product.Event.Handler.Service/"]
RUN dotnet restore "Product.Event.Handler.Service/Product.Event.Handler.Service.csproj"
COPY . .
WORKDIR "/src/Product.Event.Handler.Service"
RUN dotnet build "Product.Event.Handler.Service.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Product.Event.Handler.Service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Product.Event.Handler.Service.dll"]
