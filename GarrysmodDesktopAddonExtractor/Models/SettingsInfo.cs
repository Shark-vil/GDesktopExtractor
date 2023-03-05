namespace GarrysmodDesktopAddonExtractor.Models
{
    public struct SettingsInfo
    {
        public string GarrysModAddonsFolderPath;
        public string GarrysModWorkshopFolderPath;

        public SettingsInfo()
        {
            GarrysModAddonsFolderPath = string.Empty;
            GarrysModWorkshopFolderPath = string.Empty;
        }
    }
}
