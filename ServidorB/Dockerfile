FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["./ServidorB.csproj", "."]
RUN dotnet restore "./ServidorB.csproj"
COPY . .
#WORKDIR "/src/../ServidorB"
RUN dotnet build "ServidorB.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ServidorB.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServidorB.dll"]
