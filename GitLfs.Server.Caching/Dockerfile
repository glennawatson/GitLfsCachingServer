FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["GitLfs.Server.Caching/GitLfs.Server.Caching.csproj", "GitLfs.Server.Caching/"]
RUN dotnet restore "GitLfs.Server.Caching/GitLfs.Server.Caching.csproj"
COPY . .
WORKDIR "/src/GitLfs.Server.Caching"
RUN dotnet build "GitLfs.Server.Caching.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "GitLfs.Server.Caching.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "GitLfs.Server.Caching.dll"]