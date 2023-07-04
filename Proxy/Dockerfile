FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["./Proxy.csproj", "."]
RUN dotnet restore "./Proxy.csproj"
COPY . .
#WORKDIR "/src/../Proxy"
RUN dotnet build "Proxy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Proxy.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Proxy.dll"]
