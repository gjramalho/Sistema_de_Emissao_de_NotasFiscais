FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/EstoqueService/EstoqueService.csproj backend/EstoqueService/
RUN dotnet restore backend/EstoqueService/EstoqueService.csproj

COPY backend/EstoqueService/ backend/EstoqueService/
RUN dotnet publish backend/EstoqueService/EstoqueService.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080

RUN mkdir -p /app/data
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "EstoqueService.dll"]
