FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["Railway.Api/Railway.Api.csproj", "Railway.Api/"]
COPY ["Railway.Application/Railway.Application.csproj", "Railway.Application/"]
COPY ["Railway.Domain/Railway.Domain.csproj", "Railway.Domain/"]
COPY ["Railway.Infrastructure/Railway.Infrastructure.csproj", "Railway.Infrastructure/"]

RUN dotnet restore "Railway.Api/Railway.Api.csproj"

COPY . .

WORKDIR /src/Railway.Api

RUN dotnet publish "Railway.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

USER $APP_UID

ENTRYPOINT ["dotnet", "Railway.Api.dll"]