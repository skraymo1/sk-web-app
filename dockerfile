# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build-env /app/out .
COPY Plugins /app/Plugins
ENTRYPOINT ["dotnet", "sk-web-api.dll"]
EXPOSE 80
EXPOSE 443
#docker build -t sk-web-api .
#docker run -d -p 5132:80 --name sk-web-api sk-web-api:latest
#az acr build --registry acaalbumsskraymo1 --image sk-web-app:v1 .
#az acr task create --registry acaalbumsskraymo1 --name taskskwebappcodepush --image sk-web-app:latest --context https://github.com/skraymo1/sk-web-app.git#aca --file dockerfile --git-access-token