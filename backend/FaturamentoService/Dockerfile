FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/FaturamentoService/FaturamentoService.csproj backend/FaturamentoService/
RUN dotnet restore backend/FaturamentoService/FaturamentoService.csproj

COPY backend/FaturamentoService/ backend/FaturamentoService/
RUN dotnet publish backend/FaturamentoService/FaturamentoService.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080

RUN mkdir -p /app/data
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "FaturamentoService.dll"]
