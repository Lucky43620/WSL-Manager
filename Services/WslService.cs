using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WSL_Manager.Models;

namespace WSL_Manager.Services
{
    /// <summary>
    /// Service pour interagir avec WSL via des commandes système
    /// </summary>
    public class WslService
    {
        /// <summary>
        /// Récupère la liste de toutes les distributions WSL installées
        /// </summary>
        public async Task<List<WslDistribution>> GetDistributionsAsync()
        {
            var distributions = new List<WslDistribution>();

            try
            {
                // Exécute la commande: wsl --list --verbose
                var output = await ExecuteWslCommandAsync("--list --verbose");

                Debug.WriteLine("=== Sortie brute WSL ===");
                Debug.WriteLine(output);
                Debug.WriteLine("=== Fin sortie ===");

                // Parse la sortie pour extraire les distributions
                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                Debug.WriteLine($"Nombre de lignes: {lines.Length}");

                // Parse chaque ligne
                foreach (var line in lines)
                {
                    Debug.WriteLine($"Parsing ligne: '{line}'");
                    var distribution = ParseDistributionLine(line);
                    if (distribution != null)
                    {
                        Debug.WriteLine($"  -> Distribution trouvée: {distribution.Name}");
                        distributions.Add(distribution);
                    }
                    else
                    {
                        Debug.WriteLine($"  -> Ligne ignorée");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log l'erreur (pour l'instant on affiche juste dans le debug)
                Debug.WriteLine($"Erreur lors de la récupération des distributions: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            return distributions;
        }

        /// <summary>
        /// Démarre une distribution WSL
        /// </summary>
        public async Task<bool> StartDistributionAsync(string name)
        {
            try
            {
                await ExecuteWslCommandAsync($"-d {name} --exec echo 'Started'");
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Arrête une distribution WSL
        /// </summary>
        public async Task<bool> StopDistributionAsync(string name)
        {
            try
            {
                await ExecuteWslCommandAsync($"--terminate {name}");
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Redémarre une distribution WSL (arrêt puis démarrage)
        /// </summary>
        public async Task<bool> RestartDistributionAsync(string name)
        {
            try
            {
                // Arrête la distribution
                await StopDistributionAsync(name);

                // Attend un peu avant de redémarrer
                await Task.Delay(500);

                // Redémarre la distribution
                await StartDistributionAsync(name);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Définit une distribution comme distribution par défaut
        /// </summary>
        public async Task<bool> SetDefaultDistributionAsync(string name)
        {
            try
            {
                await ExecuteWslCommandAsync($"--set-default {name}");
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Ouvre un terminal Windows Terminal pour une distribution
        /// Essaie Windows Terminal puis cmd en fallback
        /// </summary>
        public void OpenTerminal(string name)
        {
            try
            {
                // Essaie d'abord Windows Terminal avec wsl -d
                var processInfo = new ProcessStartInfo
                {
                    FileName = "wt.exe",
                    Arguments = $"wsl -d {name}",
                    UseShellExecute = true
                };
                Process.Start(processInfo);
            }
            catch
            {
                // Fallback : ouvre avec cmd + wsl
                try
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/K wsl -d \"{name}\"",
                        UseShellExecute = true
                    };
                    Process.Start(processInfo);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Erreur lors de l'ouverture du terminal: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Ouvre l'explorateur de fichiers Windows pour le système de fichiers WSL
        /// Essaie d'abord \\wsl.localhost\ puis \\wsl$\ en fallback
        /// </summary>
        public void OpenFileExplorer(string name)
        {
            try
            {
                // Windows 11/10 récents utilisent \\wsl.localhost\
                var path = $"\\\\wsl.localhost\\{name}";

                var processInfo = new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = path,
                    UseShellExecute = true
                };
                Process.Start(processInfo);
            }
            catch
            {
                // Fallback pour anciennes versions : \\wsl$\
                try
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"\\\\wsl$\\{name}",
                        UseShellExecute = true
                    };
                    Process.Start(processInfo);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Erreur lors de l'ouverture de l'explorateur: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Obtient des informations détaillées sur une distribution
        /// </summary>
        public async Task<string> GetDistributionInfoAsync(string name)
        {
            try
            {
                // Exécute une commande pour obtenir des infos système
                var output = await ExecuteWslCommandAsync($"-d {name} uname -a");
                Debug.WriteLine($"Info pour {name}: {output}");
                return output.Trim();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur GetDistributionInfo: {ex.Message}");
                return "Informations non disponibles";
            }
        }

        /// <summary>
        /// Met à jour WSL vers la dernière version
        /// </summary>
        public async Task<bool> UpdateWslAsync()
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = "--update",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processInfo };
                process.Start();
                await process.WaitForExitAsync();

                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur UpdateWsl: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Convertit une distribution de WSL 1 vers WSL 2 ou vice-versa
        /// </summary>
        public async Task<bool> ConvertWslVersionAsync(string name, int targetVersion)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = $"--set-version {name} {targetVersion}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processInfo };
                process.Start();
                await process.WaitForExitAsync();

                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur ConvertWslVersion: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Exporte une distribution vers un fichier tar
        /// </summary>
        public async Task<bool> ExportDistributionAsync(string name, string filePath)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = $"--export {name} \"{filePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processInfo };
                process.Start();
                await process.WaitForExitAsync();

                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur ExportDistribution: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Importe une distribution depuis un fichier tar
        /// </summary>
        public async Task<bool> ImportDistributionAsync(string name, string installLocation, string filePath)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = $"--import {name} \"{installLocation}\" \"{filePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processInfo };
                process.Start();
                await process.WaitForExitAsync();

                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur ImportDistribution: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Supprime une distribution (désinstallation complète)
        /// </summary>
        public async Task<bool> UnregisterDistributionAsync(string name)
        {
            try
            {
                Debug.WriteLine($"=== Début UnregisterDistributionAsync pour '{name}' ===");

                var processInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = $"--unregister \"{name}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processInfo };
                process.Start();

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                Debug.WriteLine($"UnregisterDistribution - ExitCode: {process.ExitCode}");
                if (!string.IsNullOrEmpty(output))
                    Debug.WriteLine($"UnregisterDistribution - Output: {output}");
                if (!string.IsNullOrEmpty(error))
                    Debug.WriteLine($"UnregisterDistribution - Error: {error}");

                Debug.WriteLine($"=== Fin UnregisterDistributionAsync ===");

                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur UnregisterDistribution: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Arrête toutes les distributions WSL
        /// </summary>
        public async Task<bool> ShutdownAllAsync()
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = "--shutdown",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processInfo };
                process.Start();
                await process.WaitForExitAsync();

                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur ShutdownAll: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Installe une nouvelle distribution depuis le store Microsoft
        /// </summary>
        /// <param name="distroName">Nom de la distribution (Ubuntu, Debian, etc.)</param>
        public async Task<bool> InstallDistributionAsync(string distroName)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = $"--install -d {distroName}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processInfo };
                process.Start();
                await process.WaitForExitAsync();

                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur InstallDistribution: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Met à jour les packages d'une distribution Linux
        /// </summary>
        /// <param name="name">Nom de la distribution</param>
        /// <returns>True si la mise à jour a réussi</returns>
        public async Task<bool> UpdateDistributionPackagesAsync(string name)
        {
            try
            {
                Debug.WriteLine($"=== Début UpdateDistributionPackagesAsync pour '{name}' ===");

                // Détecte le type de distribution pour utiliser la bonne commande
                var distroInfo = await ExecuteWslCommandAsync($"-d {name} cat /etc/os-release");
                Debug.WriteLine($"Distribution info: {distroInfo}");

                string updateCommand;
                if (distroInfo.Contains("Ubuntu", StringComparison.OrdinalIgnoreCase) ||
                    distroInfo.Contains("Debian", StringComparison.OrdinalIgnoreCase))
                {
                    // Pour Ubuntu/Debian : apt update && apt upgrade -y
                    updateCommand = "sudo apt update && sudo apt upgrade -y";
                }
                else if (distroInfo.Contains("Alpine", StringComparison.OrdinalIgnoreCase))
                {
                    // Pour Alpine : apk update && apk upgrade
                    updateCommand = "sudo apk update && sudo apk upgrade";
                }
                else if (distroInfo.Contains("Arch", StringComparison.OrdinalIgnoreCase))
                {
                    // Pour Arch : pacman -Syu --noconfirm
                    updateCommand = "sudo pacman -Syu --noconfirm";
                }
                else if (distroInfo.Contains("openSUSE", StringComparison.OrdinalIgnoreCase) ||
                         distroInfo.Contains("SUSE", StringComparison.OrdinalIgnoreCase))
                {
                    // Pour openSUSE : zypper update -y
                    updateCommand = "sudo zypper update -y";
                }
                else if (distroInfo.Contains("Fedora", StringComparison.OrdinalIgnoreCase) ||
                         distroInfo.Contains("Red Hat", StringComparison.OrdinalIgnoreCase) ||
                         distroInfo.Contains("CentOS", StringComparison.OrdinalIgnoreCase))
                {
                    // Pour Fedora/RHEL/CentOS : dnf update -y ou yum update -y
                    updateCommand = "sudo dnf update -y || sudo yum update -y";
                }
                else
                {
                    Debug.WriteLine("Type de distribution non reconnu, utilisation de apt par défaut");
                    updateCommand = "sudo apt update && sudo apt upgrade -y";
                }

                Debug.WriteLine($"Commande de mise à jour: {updateCommand}");

                var processInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = $"-d \"{name}\" bash -c \"{updateCommand}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processInfo };
                process.Start();

                // Lecture asynchrone de la sortie pour éviter les blocages
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                Debug.WriteLine($"UpdateDistributionPackages - ExitCode: {process.ExitCode}");
                if (!string.IsNullOrEmpty(output))
                    Debug.WriteLine($"UpdateDistributionPackages - Output: {output.Substring(0, Math.Min(500, output.Length))}...");
                if (!string.IsNullOrEmpty(error))
                    Debug.WriteLine($"UpdateDistributionPackages - Error: {error}");

                Debug.WriteLine($"=== Fin UpdateDistributionPackagesAsync ===");

                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur UpdateDistributionPackages: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Liste les distributions disponibles à l'installation
        /// </summary>
        public async Task<List<string>> GetAvailableDistributionsAsync()
        {
            var distributions = new List<string>();

            try
            {
                var output = await ExecuteWslCommandAsync("--list --online");

                Debug.WriteLine("=== Distributions disponibles ===");
                Debug.WriteLine(output);
                Debug.WriteLine("=== Fin ===");

                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                // Parse chaque ligne pour extraire les noms de distributions
                foreach (var line in lines.Skip(1)) // Skip header
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0 && !line.Contains("NAME"))
                    {
                        distributions.Add(parts[0]);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur GetAvailableDistributions: {ex.Message}");
            }

            return distributions;
        }

        /// <summary>
        /// Exécute une commande WSL et retourne la sortie
        /// Gère correctement l'encodage pour éviter les problèmes de parsing
        /// </summary>
        private async Task<string> ExecuteWslCommandAsync(string arguments)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "wsl.exe",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8
            };

            using var process = new Process { StartInfo = processInfo };
            process.Start();

            // Lit la sortie brute
            var outputBytes = new List<byte>();
            var buffer = new byte[4096];
            int bytesRead;

            while ((bytesRead = await process.StandardOutput.BaseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < bytesRead; i++)
                {
                    // Filtre les bytes null (0x00) qui causent des espaces
                    if (buffer[i] != 0)
                    {
                        outputBytes.Add(buffer[i]);
                    }
                }
            }

            await process.WaitForExitAsync();

            // Convertit en string UTF-8
            return System.Text.Encoding.UTF8.GetString(outputBytes.ToArray());
        }

        /// <summary>
        /// Parse une ligne de la sortie "wsl --list --verbose"
        /// Format: * Ubuntu          Running         2
        /// Gère les caractères Unicode et les espaces multiples
        /// </summary>
        private WslDistribution? ParseDistributionLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            // Nettoie la ligne de tous les caractères problématiques
            line = line.Replace('\u0000', ' ')
                      .Replace('\t', ' ')
                      .Trim();

            // Ignore les lignes d'en-tête
            if (line.Contains("NAME") || line.Contains("STATE") || line.Contains("VERSION"))
                return null;

            // Vérifie si c'est la distribution par défaut (commence par *)
            bool isDefault = line.TrimStart().StartsWith("*");
            if (isDefault)
                line = line.TrimStart().Substring(1).Trim();

            // Sépare par les espaces multiples et filtre les vides
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 3)
                return null;

            // Nettoie chaque partie
            var name = parts[0].Trim();
            var state = parts[1].Trim();
            var versionStr = parts[2].Trim();

            if (string.IsNullOrEmpty(name))
                return null;

            return new WslDistribution
            {
                Name = name,
                State = state,
                Version = int.TryParse(versionStr, out var version) ? version : 2,
                IsDefault = isDefault
            };
        }
    }
}
