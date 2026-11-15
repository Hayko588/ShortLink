#Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

#restore dependencies
COPY ["src/ShortLink.Api/ShortLink.Api.csproj", "ShortLink.Api/"]
RUN dotnet restore "ShortLink.Api/ShortLink.Api.csproj"

#build
COPY ["src/ShortLink.Api/", "ShortLink.Api/"]
WORKDIR /src/ShortLink.Api
RUN dotnet build "ShortLink.Api.csproj" -c Release -o /app/build

#Stage 2: Publish the application
FROM build AS publish
RUN dotnet publish "ShortLink.Api.csproj" -c Release -o /app/publish

#Stage 3: Run the application
FROM mcr.microsoft.com/dotnet/aspnet:10.0
EXPOSE 8080
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShortLink.Api.dll"]