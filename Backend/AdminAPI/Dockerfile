FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore AdminAPI/AdminAPI.csproj
RUN dotnet build AdminAPI/AdminAPI.csproj -c Release -o /app/build
RUN dotnet publish AdminAPI/AdminAPI.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AdminAPI.dll"]
