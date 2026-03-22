using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TravelNest.Services
{
    public class PythonRunnerService : IHostedService, IDisposable
    {
        private readonly List<Process> _proceseActive = new List<Process>();
        private readonly ILogger<PythonRunnerService> _logger;

        public PythonRunnerService(ILogger<PythonRunnerService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            string caleFaceEmb = Path.Combine(Directory.GetCurrentDirectory(), "FaceRecognition", "faceEmb.py");
            string caleTagging = Path.Combine(Directory.GetCurrentDirectory(), "RecomandariForYou", "main.py");
            PornesteProcesPython(caleFaceEmb, "FaceRecognition");
            PornesteProcesPython(caleTagging, "RecomandariForYou");
            return Task.CompletedTask;
        }

        private void PornesteProcesPython(string caleScript, string folderLucru)
        {
            if (!File.Exists(caleScript))
            {
                _logger.LogError(caleScript);
                return;
            }

            string directorRadacina = Path.Combine(Directory.GetCurrentDirectory(), folderLucru);
            string pythonExe = Path.Combine(directorRadacina, "venv", "Scripts", "python.exe");
            if (!File.Exists(pythonExe)) 
                pythonExe = "python";

            var startInfo = new ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments = $"\"{caleScript}\"",
                WorkingDirectory = directorRadacina,
                RedirectStandardOutput = false,
                UseShellExecute = true,
                CreateNoWindow = false
            };

            try
            {
                var proces = Process.Start(startInfo);
                if (proces != null)
                {
                    _proceseActive.Add(proces);
                    _logger.LogInformation($"S-a pornit {Path.GetFileName(caleScript)} (PID: {proces.Id})");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {

            foreach (var proces in _proceseActive)
            {
                try
                {
                    if (!proces.HasExited)
                    {
                        proces.Kill();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            foreach (var proces in _proceseActive)
            {
                proces.Dispose();
            }
        }
    }
}