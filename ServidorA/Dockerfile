FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["../ServidorA/ServidorA.csproj", "../ServidorA/"]
RUN dotnet restore "../ServidorA/ServidorA.csproj"
COPY . .
WORKDIR "/src/../ServidorA"
RUN dotnet build "ServidorA.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ServidorA.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServidorA.dll"]
