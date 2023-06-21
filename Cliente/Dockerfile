FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["../Cliente/Cliente.csproj", "../Cliente/"]
RUN dotnet restore "../Cliente/Cliente.csproj"
COPY . .
WORKDIR "/src/../Cliente"
RUN dotnet build "Cliente.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Cliente.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cliente.dll"]
