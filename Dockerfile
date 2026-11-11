FROM node:22-alpine AS web-build
WORKDIR /workspace
COPY src/Web/package*.json ./src/Web/
RUN npm ci --prefix src/Web
COPY src/Web ./src/Web
RUN npm run build --prefix src/Web

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS publish
WORKDIR /workspace
COPY src/Api/Api.csproj ./src/Api/
RUN dotnet restore src/Api/Api.csproj
COPY src/Api ./src/Api
RUN dotnet publish src/Api/Api.csproj --configuration Release --output /app/publish --no-restore
COPY --from=web-build /workspace/src/Web/dist /app/publish/wwwroot

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
