#!/bin/bash

# ==================================================================
# Script para executar a API CadPlus ERP
# ==================================================================
# Este script permite configurar a porta da API via argumento
# 
# Uso: ./run-api.sh [porta]
# Exemplo: ./run-api.sh 8080
#
# Se não for fornecida uma porta, será usada a configuração do .env
# ==================================================================

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Função para mostrar banner
show_banner() {
    echo -e "${BLUE}"
    echo "=================================" >&2
    echo "  CadPlus ERP - Sistema Hospitalar" >&2
    echo "=================================" >&2
    echo -e "${NC}"
}

# Função de ajuda
show_help() {
    echo "Uso: $0 [porta] [opções]"
    echo ""
    echo "Argumentos:"
    echo "  porta     Porta para executar a API (opcional, padrão: 7001)"
    echo ""
    echo "Opções:"
    echo "  --help, -h     Mostrar esta ajuda"
    echo "  --env FILE      Usar arquivo .env específico"
    echo "  --check         Verificar configurações antes de executar"
    echo ""
    echo "Exemplos:"
    echo "  $0                 # Rodar na porta configurada no .env (7001)"
    echo "  $0 8080            # Rodar na porta 8080"
    echo "  $0 3000 --check   # Verificar e rodar na porta 3000"
    echo ""
    echo "Arquivos de configuração:"
    echo "  .env              # Configurações principais"
    echo "  .env.example      # Template de configurações"
}

# Função para verificar configurações
check_config() {
    echo -e "${YELLOW}🔍 Verificando configurações...${NC}" >&2
    
    # Verificar arquivo .env
    if [ -f ".env" ]; then
        echo -e "${GREEN}✓ Arquivo .env encontrado${NC}" >&2
        
        # Verificar variáveis principais
        if grep -q "API_PORT=" .env; then
            echo -e "${GREEN}✓ API_PORT configurado no .env${NC}" >&2
        else
            echo -e "${YELLOW}⚠ API_PORT não encontrado no .env${NC}" >&2
        fi
        
        if grep -q "DATABASE_CONNECTION_STRING=" .env; then
            echo -e "${GREEN}✓ DATABASE_CONNECTION_STRING configurado${NC}" >&2
        else
            echo -e "${YELLOW}⚠ DATABASE_CONNECTION_STRING não encontrado${NC}" >&2
        fi
        
        if grep -q "JWT_SECRET_KEY=" .env; then
            echo -e "${GREEN}✓ JWT_SECRET_KEY configurado${NC}" >&2
        else
            echo -e "${YELLOW}⚠ JWT_SECRET_KEY não encontrado${NC}" >&2
        fi
    else
        echo -e "${RED}✗ Arquivo .env não encontrado${NC}" >&2
        echo -e "${YELLOW}💡 Execute: cp .env.example .env${NC}" >&2
        return 1
    fi
    
    # Verificar .NET
    if command -v dotnet > /dev/null; then
        echo -e "${GREEN}✓ .NET SDK instalado${NC}" >&2
        echo "Versão: $(dotnet --version)" >&2
    else
        echo -e "${RED}✗ .NET SDK não encontrado${NC}" >&2
        return 1
    fi
    
    return 0
}

# Função para buscar porta no .env
get_port_from_env() {
    if [ -f ".env" ]; then
        grep "API_PORT=" .env | cut -d'=' -f2 | tr -d ' \t' | head -1
    else
        echo "7001"
    fi
}

# Parâmetros padrão
PORT=""
CHECK_CONFIG=false
ENV_FILE=".env"

# Processar argumentos
while [[ $# -gt 0 ]]; do
    case $1 in
        --help|-h)
            show_help
            exit 0
            ;;
        --check)
            CHECK_CONFIG=true
            shift
            ;;
        --env)
            ENV_FILE="$2"
            shift 2
            ;;
        *)
            if [[ "$1" =~ ^[0-9]+$ ]]; then
                PORT="$1"
            else
                echo -e "${RED}Erro: Porta inválida '$1'${NC}" >&2
                show_help
                exit 1
            fi
            shift
            ;;
    esac
done

# Executar verificação se solicitado
if [ "$CHECK_CONFIG" = true ]; then
    show_banner
    if ! check_config; then
        echo -e "${RED}❌ Configurações com problemas${NC}" >&2
        exit 1
    fi
    echo -e "${GREEN}✅ Todas as configurações estão corretas!${NC}" >&2
    exit 0
fi

# Determinar porta final
if [ -z "$PORT" ]; then
    PORT=$(get_port_from_env)
fi

echo -e "${BLUE}🚀 CadPlus ERP - Sistema Hospitalar${NC}" >&2
echo -e "${YELLOW}📍 Porta: $PORT${NC}" >&2
echo -e "${YELLOW}🌐 URL: http://localhost:$PORT${NC}" >&2
echo ""

# Verificar se a porta está sendo usada
if command -v lsof > /dev/null; then
    if lsof -Pi :$PORT -sTCP:LISTEN -t >/dev/null; then
        echo -e "${RED}❌ Porta $PORT já está em uso${NC}" >&2
        echo -e "${YELLOW}💡 Tente uma porta diferente ou pare o processo que está usando a porta $PORT${NC}" >&2
        exit 1
    fi
fi

# Configurar variáveis de ambiente para porta
export API_PORT=$PORT
export API_URL="http://localhost:$PORT"
export ASPNETCORE_URLS="http://localhost:$PORT"

echo -e "${GREEN}✅ Iniciando API CadPlus ERP...${NC}" >&2
echo -e "${BLUE}📊 Swagger disponível em: http://localhost:$PORT/swagger${NC}" >&2
echo -e "${BLUE}🏥 Health Check disponível em: http://localhost:$PORT/health${NC}" >&2
echo -e "${YELLOW}Pressione Ctrl+C para parar${NC}" >&2
echo ""

# Executar a aplicação
dotnet run --configuration Debug

