namespace AsylumLauncher.Models
{
    public class AsylumLauncherVersion
    {
        public float LatestVersion { get; set; }

        public float WarningVersion { get; set; }

        public float ForceUpdateVersion { get; set; }

        public string? WarningMessage { get; set; }

        public string? ForceUpdateMessage { get; set; }
    }
}