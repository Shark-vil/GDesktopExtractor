using Avalonia.Controls;
using GarrysmodDesktopAddonExtractor.Models;
using GarrysmodDesktopAddonExtractor.Services;
using System;
using System.Threading.Tasks;

namespace GarrysmodDesktopAddonExtractor
{
    public partial class SettingsWindow : Window
    {
        private SwrringsContext _context = new SwrringsContext();

        public SettingsWindow()
        {
            InitializeComponent();

            DataContext = _context;

            Button_SelectAddonsFolder.Click += OnClickSelectAddonsFolder;
            Button_SelectWorkshopFolder.Click += OnClickSelectWorkshopFolder;

            Task.Factory.StartNew(async () => await StartAsync());
        }

        ~SettingsWindow()
        {
            Button_SelectAddonsFolder.Click -= OnClickSelectAddonsFolder;
            Button_SelectWorkshopFolder.Click -= OnClickSelectWorkshopFolder;
        }

        private void OnClickSelectAddonsFolder(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            OpenSelectFolderDialog(async (string folderPath, SettingsInfo settingsInfo) =>
            {
                settingsInfo.GarrysModAddonsFolderPath = folderPath;
                await SettingsService.WriteSettingsAsync(settingsInfo);
            });
        }

        private void OnClickSelectWorkshopFolder(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            OpenSelectFolderDialog(async (string folderPath, SettingsInfo settingsInfo) =>
            {
                settingsInfo.GarrysModWorkshopFolderPath = folderPath;
                await SettingsService.WriteSettingsAsync(settingsInfo);
            });
        }

        private void OpenSelectFolderDialog(Func<string, SettingsInfo, Task> invokeSelectedFolder)
        {
            Task.Factory.StartNew(async () =>
            {
                var folderDialog = new OpenFolderDialog();
                string? folderPath = await folderDialog.ShowAsync(this);
                if (!string.IsNullOrEmpty(folderPath))
                {
                    SettingsInfo settingsInfo = await SettingsService.GetSettingsAsync();
                    await invokeSelectedFolder.Invoke(folderPath, settingsInfo);
                    await UpdateContextInfo();
                }
            });
        }

        private async Task StartAsync()
        {
            await UpdateContextInfo();
        }

        private async Task UpdateContextInfo()
        {
            SettingsInfo settingsInfo = await SettingsService.GetSettingsAsync();

            _context.GarrysModAddonsFolderPath = settingsInfo.GarrysModAddonsFolderPath;
            _context.GarrysModWorkshopFolderPath = settingsInfo.GarrysModWorkshopFolderPath;
        }
    }
}
