FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY simple-api.csproj .
COPY Domain/ Domain/
COPY Application/ Application/
COPY Infrastructure/ Infrastructure/
COPY Properties/ Properties/
COPY appsettings.json .
COPY Api/ Api/
COPY Program.cs .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV Database__ApplyMigrationsOnStartup=true
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "simple-api.dll"]
