using System.Diagnostics;
using System.Text;
using Blog.EFCoreMigration.Services.Interfaces;

namespace Blog.EFCoreMigration.Services;

public class ProcessDispatcher(ILogger<ProcessDispatcher> logger) : IProcessDispatcher
{
    public async Task DispatchProcess(string fileName, string? arguments = null)
    {
        using var process = new Process();
        var processStartInfo = new ProcessStartInfo(fileName)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        if (!string.IsNullOrEmpty(arguments))
            processStartInfo.Arguments = arguments;
        process.StartInfo = processStartInfo;

        process.Start();

        // 必须并发读取两个流，否则子进程写满其中一个缓冲区后会阻塞，导致死锁。
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await Task.WhenAll(outputTask, errorTask);
        await process.WaitForExitAsync();

        var msg = await outputTask;
        if (!string.IsNullOrEmpty(msg))
            logger.LogInformation("Process {FileName} {Arguments} stdout: {Output}", fileName, arguments, msg);

        var errorMsg = await errorTask;
        if (!string.IsNullOrEmpty(errorMsg))
            logger.LogError("Process {FileName} {Arguments} stderr: {Error}", fileName, arguments, errorMsg);

        if (process.ExitCode != 0)
            throw new InvalidOperationException(
                $"Process '{fileName} {arguments}' exited with code {process.ExitCode}.");
    }
}