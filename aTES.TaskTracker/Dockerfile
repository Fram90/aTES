FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["aTES.TaskTracker/aTES.TaskTracker.csproj", "aTES.TaskTracker/"]
RUN dotnet restore "aTES.TaskTracker/aTES.TaskTracker.csproj"
COPY . .
WORKDIR "/src/aTES.TaskTracker"
RUN dotnet build "aTES.TaskTracker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "aTES.TaskTracker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "aTES.TaskTracker.dll"]
