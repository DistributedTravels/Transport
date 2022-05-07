#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_ENVIRONMENT=Development

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Transport.csproj", "."]
RUN dotnet restore "./Transport.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Transport.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Transport.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ADD Init /Init
ENTRYPOINT ["dotnet", "Transport.dll", "--server.urls", "http://+:80"]