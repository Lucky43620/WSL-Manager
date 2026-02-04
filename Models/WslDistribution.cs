namespace WSL_Manager.Models
{
    /// <summary>
    /// Représente une distribution WSL
    /// </summary>
    public class WslDistribution
    {
        /// <summary>
        /// Nom de la distribution (ex: Ubuntu, Debian)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// État de la distribution (Running, Stopped)
        /// </summary>
        public string State { get; set; } = string.Empty;

        /// <summary>
        /// Version de WSL (1 ou 2)
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Est-ce la distribution par défaut?
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Est-ce que la distribution est en cours d'exécution?
        /// </summary>
        public bool IsRunning => State.Equals("Running", System.StringComparison.OrdinalIgnoreCase);
    }
}
