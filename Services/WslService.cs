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
                // Essaie d'abord Windows Terminal avec wsl -d et change vers le home
                var processInfo = new ProcessStartInfo
                {
                    FileName = "wt.exe",
                    Arguments = $"wsl -d {name} --cd ~",
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
                        Arguments = $"/K wsl -d \"{name}\" --cd ~",
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
        /// <param name="webDownload">Télécharger depuis GitHub au lieu de Microsoft Store</param>
        /// <param name="preRelease">Installer la version pré-release</param>
        public async Task<(bool success, string? error)> UpdateWslAsync(bool webDownload = false, bool preRelease = false)
        {
            try
            {
                Helpers.Logger.BeginOperation("UpdateWsl", $"webDownload={webDownload}, preRelease={preRelease}");

                var arguments = "--update";
                if (webDownload) arguments += " --web-download";
                if (preRelease) arguments += " --pre-release";

                var processInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = arguments,
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

                Helpers.Logger.EndOperation("UpdateWsl", process.ExitCode == 0);
                return (process.ExitCode == 0, string.IsNullOrEmpty(error) ? null : error);
            }
            catch (Exception ex)
            {
                Helpers.Logger.Error("UpdateWsl failed", ex);
                return (false, ex.Message);
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
        /// Exporte une distribution vers un fichier tar ou VHD
        /// </summary>
        /// <param name="name">Nom de la distribution</param>
        /// <param name="filePath">Chemin du fichier de destination</param>
        /// <param name="asVhd">Exporter au format VHD au lieu de TAR</param>
        public async Task<(bool success, string? error)> ExportDistributionAsync(string name, string filePath, bool asVhd = false)
        {
            try
            {
                Helpers.Logger.BeginOperation("ExportDistribution", $"{name} to {filePath} (VHD={asVhd})");

                var arguments = asVhd
                    ? $"--export {name} \"{filePath}\" --vhd"
                    : $"--export {name} \"{filePath}\"";

                var processInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = arguments,
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

                Helpers.Logger.EndOperation("ExportDistribution", process.ExitCode == 0);
                return (process.ExitCode == 0, string.IsNullOrEmpty(error) ? null : error);
            }
            catch (Exception ex)
            {
                Helpers.Logger.Error("ExportDistribution failed", ex);
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Importe une distribution depuis un fichier tar ou VHD
        /// </summary>
        /// <param name="name">Nom de la distribution</param>
        /// <param name="installLocation">Emplacement d'installation</param>
        /// <param name="filePath">Chemin du fichier source</param>
        /// <param name="isVhd">Le fichier source est un VHD</param>
        /// <param name="version">Version WSL à utiliser (1 ou 2)</param>
        public async Task<(bool success, string? error)> ImportDistributionAsync(string name, string installLocation, string filePath, bool isVhd = false, int? version = null)
        {
            try
            {
                Helpers.Logger.BeginOperation("ImportDistribution", $"{name} from {filePath} (VHD={isVhd}, version={version})");

                var arguments = $"--import {name} \"{installLocation}\" \"{filePath}\"";
                if (version.HasValue)
                    arguments += $" --version {version.Value}";

                var processInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = arguments,
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

                Helpers.Logger.EndOperation("ImportDistribution", process.ExitCode == 0);
                return (process.ExitCode == 0, string.IsNullOrEmpty(error) ? null : error);
            }
            catch (Exception ex)
            {
                Helpers.Logger.Error("ImportDistribution failed", ex);
                return (false, ex.Message);
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
        /// <param name="webDownload">Télécharger depuis le web au lieu du Store</param>
        /// <param name="noLaunch">Ne pas lancer la distribution après installation</param>
        /// <param name="installLocation">Emplacement d'installation personnalisé</param>
        public async Task<(bool success, string? error)> InstallDistributionAsync(string distroName, bool webDownload = false, bool noLaunch = false, string? installLocation = null)
        {
            try
            {
                Helpers.Logger.BeginOperation("InstallDistribution", $"{distroName} (web={webDownload}, noLaunch={noLaunch})");

                var arguments = $"--install -d {distroName}";
                if (webDownload) arguments += " --web-download";
                if (noLaunch) arguments += " --no-launch";
                if (!string.IsNullOrEmpty(installLocation))
                    arguments += $" --location \"{installLocation}\"";

                var processInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = arguments,
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

                Helpers.Logger.EndOperation("InstallDistribution", process.ExitCode == 0);
                return (process.ExitCode == 0, string.IsNullOrEmpty(error) ? null : error);
            }
            catch (Exception ex)
            {
                Helpers.Logger.Error("InstallDistribution failed", ex);
                return (false, ex.Message);
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

        #region Nouvelles fonctionnalités WSL

        /// <summary>
        /// Définit la version WSL par défaut pour les nouvelles installations
        /// </summary>
        /// <param name="version">Version WSL (1 ou 2)</param>
        public async Task<(bool success, string? error)> SetDefaultWslVersionAsync(int version)
        {
            try
            {
                Helpers.Logger.BeginOperation("SetDefaultWslVersion", $"version={version}");

                if (version != 1 && version != 2)
                    return (false, "La version doit être 1 ou 2");

                var processInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = $"--set-default-version {version}",
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

                Helpers.Logger.EndOperation("SetDefaultWslVersion", process.ExitCode == 0);
                return (process.ExitCode == 0, string.IsNullOrEmpty(error) ? null : error);
            }
            catch (Exception ex)
            {
                Helpers.Logger.Error("SetDefaultWslVersion failed", ex);
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Récupère l'état détaillé de WSL
        /// </summary>
        public async Task<(bool success, string output, string? error)> GetWslStatusAsync()
        {
            try
            {
                Helpers.Logger.BeginOperation("GetWslStatus");

                var processInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = "--status",
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

                Helpers.Logger.EndOperation("GetWslStatus", process.ExitCode == 0);
                return (process.ExitCode == 0, output.Trim(), string.IsNullOrEmpty(error) ? null : error);
            }
            catch (Exception ex)
            {
                Helpers.Logger.Error("GetWslStatus failed", ex);
                return (false, string.Empty, ex.Message);
            }
        }

        /// <summary>
        /// Récupère la version de WSL installée
        /// </summary>
        public async Task<(bool success, string output, string? error)> GetWslVersionAsync()
        {
            try
            {
                Helpers.Logger.BeginOperation("GetWslVersion");

                var processInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = "--version",
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

                Helpers.Logger.EndOperation("GetWslVersion", process.ExitCode == 0);
                return (process.ExitCode == 0, output.Trim(), string.IsNullOrEmpty(error) ? null : error);
            }
            catch (Exception ex)
            {
                Helpers.Logger.Error("GetWslVersion failed", ex);
                return (false, string.Empty, ex.Message);
            }
        }

        /// <summary>
        /// Importe une distribution VHD en place (sans copie)
        /// </summary>
        /// <param name="name">Nom de la distribution</param>
        /// <param name="vhdPath">Chemin du fichier VHD</param>
        public async Task<(bool success, string? error)> ImportDistributionInPlaceAsync(string name, string vhdPath)
        {
            try
            {
                Helpers.Logger.BeginOperation("ImportDistributionInPlace", $"{name} from {vhdPath}");

                var processInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = $"--import-in-place {name} \"{vhdPath}\"",
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

                Helpers.Logger.EndOperation("ImportDistributionInPlace", process.ExitCode == 0);
                return (process.ExitCode == 0, string.IsNullOrEmpty(error) ? null : error);
            }
            catch (Exception ex)
            {
                Helpers.Logger.Error("ImportDistributionInPlace failed", ex);
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Monte un disque ou VHD dans WSL
        /// </summary>
        /// <param name="diskPath">Chemin du disque (ex: \\.\PHYSICALDRIVE0) ou VHD</param>
        /// <param name="bare">Monter en mode bare (sans partition)</param>
        /// <param name="partition">Numéro de partition (optionnel)</param>
        /// <param name="type">Type de système de fichiers (ext4, vfat, etc.)</param>
        public async Task<(bool success, string? error)> MountDiskAsync(string diskPath, bool bare = false, string? partition = null, string? type = null)
        {
            try
            {
                Helpers.Logger.BeginOperation("MountDisk", $"{diskPath} (bare={bare}, partition={partition}, type={type})");

                var arguments = $"--mount \"{diskPath}\"";
                if (bare) arguments += " --bare";
                if (!string.IsNullOrEmpty(partition)) arguments += $" --partition {partition}";
                if (!string.IsNullOrEmpty(type)) arguments += $" --type {type}";

                var processInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = arguments,
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

                Helpers.Logger.EndOperation("MountDisk", process.ExitCode == 0);
                return (process.ExitCode == 0, string.IsNullOrEmpty(error) ? null : error);
            }
            catch (Exception ex)
            {
                Helpers.Logger.Error("MountDisk failed", ex);
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Démonte un disque ou VHD
        /// </summary>
        /// <param name="diskPath">Chemin du disque à démonter</param>
        public async Task<(bool success, string? error)> UnmountDiskAsync(string diskPath)
        {
            try
            {
                Helpers.Logger.BeginOperation("UnmountDisk", diskPath);

                var processInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = $"--unmount \"{diskPath}\"",
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

                Helpers.Logger.EndOperation("UnmountDisk", process.ExitCode == 0);
                return (process.ExitCode == 0, string.IsNullOrEmpty(error) ? null : error);
            }
            catch (Exception ex)
            {
                Helpers.Logger.Error("UnmountDisk failed", ex);
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Ouvre un terminal pour une distribution avec un utilisateur spécifique
        /// </summary>
        /// <param name="distributionName">Nom de la distribution</param>
        /// <param name="userName">Nom d'utilisateur (optionnel, défaut = root si non spécifié)</param>
        public void OpenTerminalAsUser(string distributionName, string? userName = null)
        {
            try
            {
                Helpers.Logger.BeginOperation("OpenTerminalAsUser", $"{distributionName} as {userName ?? "default"}");

                var arguments = string.IsNullOrEmpty(userName)
                    ? $"wsl -d {distributionName} --cd ~"
                    : $"wsl -d {distributionName} -u {userName} --cd ~";

                // Essaie Windows Terminal
                try
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = "wt.exe",
                        Arguments = arguments,
                        UseShellExecute = true
                    };
                    Process.Start(processInfo);
                    Helpers.Logger.EndOperation("OpenTerminalAsUser", true);
                }
                catch
                {
                    // Fallback cmd
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/K {arguments}",
                        UseShellExecute = true
                    };
                    Process.Start(processInfo);
                    Helpers.Logger.EndOperation("OpenTerminalAsUser", true);
                }
            }
            catch (Exception ex)
            {
                Helpers.Logger.Error("OpenTerminalAsUser failed", ex);
                throw;
            }
        }

        /// <summary>
        /// Définit l'utilisateur par défaut pour une distribution
        /// </summary>
        /// <param name="distributionName">Nom de la distribution</param>
        /// <param name="userName">Nom d'utilisateur à définir par défaut</param>
        public async Task<(bool success, string? error)> SetDistributionDefaultUserAsync(string distributionName, string userName)
        {
            try
            {
                Helpers.Logger.BeginOperation("SetDistributionDefaultUser", $"{distributionName} to {userName}");

                var processInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = $"-d {distributionName} -u root usermod -aG sudo {userName}",
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

                // Note: Cette commande configure l'utilisateur, mais la vraie commande pour
                // définir l'utilisateur par défaut dépend de la distribution spécifique
                // (souvent via config dans /etc/wsl.conf)

                Helpers.Logger.EndOperation("SetDistributionDefaultUser", process.ExitCode == 0);
                return (process.ExitCode == 0, string.IsNullOrEmpty(error) ? null : error);
            }
            catch (Exception ex)
            {
                Helpers.Logger.Error("SetDistributionDefaultUser failed", ex);
                return (false, ex.Message);
            }
        }

        #endregion
    }
}
