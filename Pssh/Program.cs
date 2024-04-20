using System.Diagnostics;
using System.Text.Json;

namespace Pssh;

public class Program
{
    private static List<ConsoleColor> _defaultColors = new() {
        ConsoleColor.Red,
        ConsoleColor.Green,
        ConsoleColor.Yellow,
        ConsoleColor.Blue,
        ConsoleColor.Magenta,
        ConsoleColor.Cyan,
        ConsoleColor.Gray,
        ConsoleColor.DarkRed,
        ConsoleColor.DarkGreen,
        ConsoleColor.DarkYellow,
        ConsoleColor.DarkBlue,
        ConsoleColor.DarkMagenta,
        ConsoleColor.DarkCyan,
    };

    public static async Task Main(string[] args)
    {
        var command = string.Join(' ', args);

        if (string.IsNullOrEmpty(command))
        {
            await Console.Error.WriteLineAsync("No command provided.");
            Environment.Exit(1);
        }

        if (Environment.GetEnvironmentVariable("NO_COLOR") != null)
        {
            _defaultColors = new List<ConsoleColor> { ConsoleColor.White };
        }

        var configFilePath = Environment.GetEnvironmentVariable("PSSH_CONFIG");

        if (string.IsNullOrEmpty(configFilePath))
        {
            configFilePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "pssh", "config.json");
        }
        
        if (!File.Exists(configFilePath))
        {
            await Console.Error.WriteLineAsync($"Config file not found at {configFilePath}");
            await Console.Error.WriteLineAsync("Please create a config file at the above path or set the PSSH_CONFIG environment variable to the path of your config file.");
            Environment.Exit(1);
        }

        var hosts = JsonSerializer.Deserialize(await File.ReadAllTextAsync(configFilePath), typeof(List<Host>), SourceGenerationContext.Default) as List<Host>;
        
        if (hosts is null || hosts.Count == 0)
        {
            await Console.Error.WriteLineAsync("No hosts found in config file.");
            Environment.Exit(1);
        }

        var maxLength = hosts.Max(h => h.Address.Length);

        var processes = new List<Process>();
        var colorIndex = 0;

        foreach (var host in hosts)
        {
            ConsoleColor color;

            if (string.IsNullOrEmpty(host.Color))
            {
                color = _defaultColors[colorIndex % _defaultColors.Count];
                colorIndex++;
            }
            else
            {
                color = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), host.Color, true);
            }

            var process = ExecuteCommand(host, color, maxLength, command); 
            processes.Add(process);
        }

        foreach (var process in processes)
        {
            await process.WaitForExitAsync();
        }
    }
    
    private static Process ExecuteCommand(Host host, ConsoleColor color, int maxLength, string command)
    {
        var sshCommand = new List<string>
        {
            !string.IsNullOrEmpty(host.Username)
            ? $"{host.Username}@{host.Address}"
            : $"{Environment.UserName}@{host.Address}"
        };

        if (host.Port != 0)
        {
            sshCommand.Add("-p");
            sshCommand.Add(host.Port.ToString());
        }

        if (!string.IsNullOrEmpty(host.KeyFile))
        {
            sshCommand.Add("-i");
            sshCommand.Add(host.KeyFile);
        }

        sshCommand.Add(command);

        var cmdProcess = new Process();
        cmdProcess.StartInfo.FileName = "ssh";
        cmdProcess.StartInfo.Arguments = string.Join(' ', sshCommand);
        cmdProcess.StartInfo.RedirectStandardOutput = true;
        cmdProcess.StartInfo.RedirectStandardError = true;
        cmdProcess.StartInfo.CreateNoWindow = true;
        cmdProcess.StartInfo.UseShellExecute = false;

        cmdProcess.OutputDataReceived += (sender, e) =>
        {
            if (string.IsNullOrEmpty(e.Data)) return;
            
            lock(Console.Out)
            {
                Console.ForegroundColor = color;
                Console.Write($"{host.Address.PadRight(maxLength)} ");
                Console.ResetColor();
                Console.WriteLine(e.Data);
            }
        };

        cmdProcess.ErrorDataReceived += (sender, e) =>
        {
            if (string.IsNullOrEmpty(e.Data)) return;
            
            lock(Console.Error)
            {
                Console.ForegroundColor = color;
                Console.Write($"{host.Address.PadRight(maxLength)} Error: ");
                Console.ResetColor();
                Console.WriteLine(e.Data);
            }
        };

        cmdProcess.Start();
        cmdProcess.BeginOutputReadLine();
        cmdProcess.BeginErrorReadLine();
        
        return cmdProcess;
    }
}
