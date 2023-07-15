FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["aTES.Web/aTES.Web.csproj", "aTES.Web/"]
RUN dotnet restore "aTES.Web/aTES.Web.csproj"
COPY . .
WORKDIR "/src/aTES.Web"
RUN dotnet build "aTES.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "aTES.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "aTES.Web.dll"]
