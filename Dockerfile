FROM mcr.microsoft.com/dotnet/core/aspnet:5.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:5.0-buster AS build
WORKDIR /src
COPY ["TheBugTracker.csproj", ""]
RUN dotnet restore "./TheBugTracker.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "TheBugTracker.csproj" -c Release -o /app/build


FROM build AS publish
RUN dotnet build "TheBugTracker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
CMD ASPNETCORE_URLS=http://*:$PORT dotnet TheBugTracker.dll