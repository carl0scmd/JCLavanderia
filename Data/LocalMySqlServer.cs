using System.Diagnostics;
using System.IO.Compression;
using System.Net.Sockets;
using MySql.Data.MySqlClient;

namespace JCLavanderia.Pedidos.Data;

internal static class LocalMySqlServer
{
    private const string Version = "8.4.8";
    private const string ArchiveName = $"mysql-{Version}-winx64.zip";
    private const string FolderName = $"mysql-{Version}-winx64";
    private const string DownloadUrl = $"https://cdn.mysql.com/Downloads/MySQL-8.4/{ArchiveName}";

    public static void EnsureStarted(string connectionString, string contentRootPath, IConfiguration configuration)
    {
        if (!configuration.GetValue("LocalMySql:AutoStart", true))
        {
            return;
        }

        var connection = new MySqlConnectionStringBuilder(connectionString);
        var host = string.IsNullOrWhiteSpace(connection.Server) ? "127.0.0.1" : connection.Server;
        var port = (int)connection.Port;

        if (!IsLocalHost(host) || IsPortOpen(host, port, TimeSpan.FromMilliseconds(500)))
        {
            return;
        }

        var mysqlRoot = Path.Combine(contentRootPath, ".mysql");
        var mysqlHome = Path.Combine(mysqlRoot, FolderName);
        var dataDir = Path.Combine(mysqlRoot, "data");
        var mysqldPath = Path.Combine(mysqlHome, "bin", "mysqld.exe");

        EnsureDistribution(mysqlRoot, mysqlHome, mysqldPath);
        EnsureDataDirectory(mysqldPath, mysqlHome, dataDir);
        StartServer(mysqldPath, mysqlHome, dataDir, port, mysqlRoot);
        WaitForServer(host, port);
    }

    private static bool IsLocalHost(string host) =>
        string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(host, "::1", StringComparison.OrdinalIgnoreCase);

    private static bool IsPortOpen(string host, int port, TimeSpan timeout)
    {
        try
        {
            using var client = new TcpClient();
            var connect = client.ConnectAsync(host, port);
            return connect.Wait(timeout) && client.Connected;
        }
        catch
        {
            return false;
        }
    }

    private static void EnsureDistribution(string mysqlRoot, string mysqlHome, string mysqldPath)
    {
        if (File.Exists(mysqldPath))
        {
            return;
        }

        Directory.CreateDirectory(mysqlRoot);
        var archivePath = Path.Combine(mysqlRoot, ArchiveName);

        if (!File.Exists(archivePath))
        {
            Console.WriteLine("Baixando MySQL local de desenvolvimento. Isso acontece apenas na primeira execucao.");
            using var httpClient = new HttpClient();
            using var response = httpClient.GetAsync(DownloadUrl).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            using var source = response.Content.ReadAsStream();
            using var destination = File.Create(archivePath);
            source.CopyTo(destination);
        }

        ZipFile.ExtractToDirectory(archivePath, mysqlRoot, overwriteFiles: true);

        if (!File.Exists(mysqldPath))
        {
            throw new InvalidOperationException($"MySQL local nao encontrado em {mysqldPath}.");
        }
    }

    private static void EnsureDataDirectory(string mysqldPath, string mysqlHome, string dataDir)
    {
        if (Directory.Exists(dataDir) && Directory.EnumerateFileSystemEntries(dataDir).Any())
        {
            return;
        }

        Directory.CreateDirectory(dataDir);
        RunProcess(mysqldPath, $"--initialize-insecure --basedir=\"{mysqlHome}\" --datadir=\"{dataDir}\"", waitForExit: true);
    }

    private static void StartServer(string mysqldPath, string mysqlHome, string dataDir, int port, string mysqlRoot)
    {
        var errorLog = Path.Combine(mysqlRoot, "mysql-error.log");
        var arguments =
            $"--no-defaults --basedir=\"{mysqlHome}\" --datadir=\"{dataDir}\" --port={port} " +
            $"--bind-address=127.0.0.1 --mysqlx=0 --skip-name-resolve --log-error=\"{errorLog}\"";

        RunProcess(mysqldPath, arguments, waitForExit: false);
    }

    private static void WaitForServer(string host, int port)
    {
        var timeoutAt = DateTime.UtcNow.AddSeconds(120);
        while (DateTime.UtcNow < timeoutAt)
        {
            if (IsPortOpen(host, port, TimeSpan.FromMilliseconds(500)))
            {
                return;
            }

            Thread.Sleep(1000);
        }

        throw new TimeoutException($"MySQL local nao abriu a porta {port} dentro do tempo esperado.");
    }

    private static void RunProcess(string fileName, string arguments, bool waitForExit)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            CreateNoWindow = true,
            UseShellExecute = false
        };

        var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"Nao foi possivel iniciar {fileName}.");

        if (!waitForExit)
        {
            return;
        }

        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"{Path.GetFileName(fileName)} terminou com codigo {process.ExitCode}.");
        }
    }
}
