using DotNetEnv;

namespace CadPlus.Extensions;

public static class EnvironmentExtensions
{
    public static void LoadEnvironmentFile(string fileName = ".env")
    {
        // Salvar variáveis de ambiente existentes do sistema
        var existingApiPort = Environment.GetEnvironmentVariable("API_PORT");
        var existingApiUrl = Environment.GetEnvironmentVariable("API_URL");
        
        // Tentar carregar arquivo .env na raiz do projeto
        var envPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
        
        // Fallback para diretório do executável
        if (!File.Exists(envPath))
        {
            envPath = Path.Combine(AppContext.BaseDirectory, fileName);
        }
        
        if (File.Exists(envPath))
        {
            // Carregar arquivo .env apenas se não há variáveis de ambiente definidas
            if (string.IsNullOrEmpty(existingApiPort) && string.IsNullOrEmpty(existingApiUrl))
            {
                Env.Load(envPath);
                Console.WriteLine($"✓ Arquivo {fileName} carregado com sucesso de: {envPath}");
            }
            else
            {
                Console.WriteLine($"✓ Usando variáveis de ambiente do sistema (sobrescrevendo .env)");
            }
            
            // Debug: mostrar algumas variáveis carregadas
            var apiPort = Environment.GetEnvironmentVariable("API_PORT");
            var apiUrl = Environment.GetEnvironmentVariable("API_URL");
            Console.WriteLine($"  📍 API_PORT: {apiPort}");
            Console.WriteLine($"  🌐 API_URL: {apiUrl}");
        }
        else
        {
            Console.WriteLine($"⚠ Arquivo {fileName} não encontrado. Tentou:");
            Console.WriteLine($"  - {Path.Combine(Directory.GetCurrentDirectory(), fileName)}");
            Console.WriteLine($"  - {Path.Combine(AppContext.BaseDirectory, fileName)}");
        }
    }

    public static string GetRequiredEnvironmentVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrEmpty(value))
        {
            throw new InvalidOperationException(
                $"A variável de ambiente '{name}' é obrigatória, mas não foi definida. " +
                $"Verifique se está definida no arquivo .env ou nas variáveis do sistema.");
        }
        return value;
    }

    public static string GetEnvironmentVariableOrDefault(string name, string defaultValue = null!)
    {
        return Environment.GetEnvironmentVariable(name) ?? defaultValue ?? string.Empty;
    }
}
