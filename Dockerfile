FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props global.json UserManagement.slnx ./
COPY UserManagement.Data/UserManagement.Data.csproj UserManagement.Data/
COPY UserManagement.Services/UserManagement.Services.csproj UserManagement.Services/
COPY UserManagement.Web/UserManagement.Web.csproj UserManagement.Web/
RUN dotnet restore UserManagement.Web/UserManagement.Web.csproj

COPY UserManagement.Data/ UserManagement.Data/
COPY UserManagement.Services/ UserManagement.Services/
COPY UserManagement.Web/ UserManagement.Web/
RUN dotnet publish UserManagement.Web/UserManagement.Web.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    --property:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

USER $APP_UID
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "UserManagement.Web.dll"]
