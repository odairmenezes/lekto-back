# Use a imagem oficial do .NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# Criar diretórios necessários
RUN mkdir -p /app/data /app/logs

# Use a imagem oficial do .NET SDK para build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar arquivos de projeto e restaurar dependências
COPY lelklto.csproj .
RUN dotnet restore "lelklto.csproj"

# Copiar o resto dos arquivos e fazer build
COPY . .
RUN dotnet build "lelklto.csproj" -c Release -o /app/build

# Publicar a aplicação
FROM build AS publish
RUN dotnet publish "lelklto.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Criar imagem final
FROM base AS final
WORKDIR /app

# Copiar arquivos publicados
COPY --from=publish /app/publish .

# Configurar porta (será sobrescrita pelo docker-compose se necessário)
EXPOSE 7001

# Ponto de entrada
ENTRYPOINT ["dotnet", "lelklto.dll"]

# Labels para metadados
LABEL maintainer="CadPlus Team"
LABEL version="1.0.0"
LABEL description="CadPlus ERP - Sistema Hospitalar"

