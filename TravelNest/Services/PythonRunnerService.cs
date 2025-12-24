using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TravelNest.Services
{
    public class PythonRunnerService : IHostedService, IDisposable
    {
        private Process? _pythonProcess;
        private readonly ILogger<PythonRunnerService> _logger;

        public PythonRunnerService(ILogger<PythonRunnerService> logger)
        {
            _logger = logger;
        }
        //porneste scriptul python la startul aplicatiei
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(">>> Se porneste automat scriptul Python (faceEmb.py)...");
            string scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "FaceRecognition", "faceEmb.py");

            if (!File.Exists(scriptPath))
            {
                _logger.LogError($"[EROARE] Nu am gasit scriptul Python la: {scriptPath}");
                return Task.CompletedTask;
            }
            var startInfo = new ProcessStartInfo
            {
                FileName = "python", 
                Arguments = $"\"{scriptPath}\"",
                RedirectStandardOutput = false, 
                UseShellExecute = true,         
                CreateNoWindow = false,     
                WorkingDirectory = Directory.GetCurrentDirectory()
            };

            try
            {
                _pythonProcess = Process.Start(startInfo);
                _logger.LogInformation($"Python pornit cu succes! PID: {_pythonProcess?.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Nu s-a putut porni Python automat: {ex.Message}");
            }

            return Task.CompletedTask;
        }
        //inchide script cand opresc aplicatia
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(">>> Se opreste procesul Python...");
            if (_pythonProcess != null && !_pythonProcess.HasExited)
            {
                try
                {
                    _pythonProcess.Kill();
                    _pythonProcess.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Eroare la oprirea Python: {ex.Message}");
                }
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _pythonProcess?.Dispose();
        }
    }
}