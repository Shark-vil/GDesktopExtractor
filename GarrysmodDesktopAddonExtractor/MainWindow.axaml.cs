using Avalonia.Controls;
using Avalonia.Threading;
using CSharpGmaReaderLibrary;
using CSharpGmaReaderLibrary.Models;
using GarrysmodDesktopAddonExtractor.Models;
using GarrysmodDesktopAddonExtractor.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GarrysmodDesktopAddonExtractor
{
    public partial class MainWindow : Window
    {
        private MainContext _context = new MainContext();
        private SettingsWindow? _settingsWindow = null;
        private List<Func<Task>>? _readAddonsTasks = null;
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private bool _canExtractAtThisMoment = false;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = _context;

            Menu_Application_Settings.Click += OnClickOpenSettingsMenu;
            Menu_Scan.Click += OnClickScan;
            Menu_ExtractSelected.Click += OnClickExtractSelected;
            Menu_ExtractAll.Click += OnClickExtractAll;
            Menu_Exit.Click += OnClickCloseWindow;

#if !DEBUG
            OnScanAddons();
#endif
        }

        private void OnClickExtractAll(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            DataGrid_Addons.SelectAll();
            OnStartExtractSelected();
        }

        private void OnClickCloseWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => this.Close();

        private void OnClickExtractSelected(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => OnStartExtractSelected();

        private void OnStartExtractSelected()
        {
            if (_canExtractAtThisMoment)
                return;

            Task.Factory.StartNew(async () =>
            {
                System.Collections.IList selectedItems = DataGrid_Addons.SelectedItems;
                var extractTasks = new List<Func<Task>>();

                var folderDialog = new OpenFolderDialog();
                string? extractFolderPath = await folderDialog.ShowAsync(this);
                if (string.IsNullOrEmpty(extractFolderPath) || !Directory.Exists(extractFolderPath))
                    return;

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ProgressBar_Bottom.Minimum = 0;
                    ProgressBar_Bottom.Maximum = selectedItems.Count;
                    _canExtractAtThisMoment = true;
                });

                foreach (object entryObject in selectedItems)
                {
                    if (entryObject is not AddonDataRowModel)
                        continue;

                    AddonDataRowModel? addonDataRowNullable = (AddonDataRowModel)entryObject;
                    if (addonDataRowNullable == null)
                        continue;

                    var addonDataRow = (AddonDataRowModel)addonDataRowNullable;
                    if (string.IsNullOrEmpty(addonDataRow.AddonInfo.SourcePath))
                        continue;

                    extractTasks.Add(async () =>
                    {
                        try
                        {
                            string folderName = addonDataRow.AddonInfo.Name ?? Guid.NewGuid().ToString("N");
                            string specificExtractPath = Path.Combine(extractFolderPath, GetValidFileName(folderName));

                            var gmaExtractor = new GmaExtractor(specificExtractPath);
                            var gmaReader = new GmaReader();
                            var options = new ReadFileContentOptions { AddonInfo = addonDataRow.AddonInfo };

                            await gmaReader.ReadFileContentAsync(addonDataRow.AddonInfo.SourcePath, async (FileContentModel content) => await gmaExtractor.ExtractFileAsync(content), options);
                            await gmaExtractor.MakeDescriptionFile(addonDataRow.AddonInfo);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex);
                        }
                        finally
                        {
                            await CalculateProgressAsync();
                        }
                    });
                }

                await Parallel.ForEachAsync(extractTasks, async (Func<Task> f, CancellationToken c) =>
                {
                    if (!c.IsCancellationRequested)
                        await f.Invoke();
                    else
                        await CalculateProgressAsync();
                });
            });
        }

        private string GetValidFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(c, '_');
            return fileName;
        }

        private void OnClickScan(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => OnScanAddons();

        private void OnScanAddons()
        {
            _context.Data = new ObservableCollection<AddonDataRowModel>();

            Task.Factory.StartNew(async () =>
            {
                SettingsInfo settingsInfo = await SettingsService.GetSettingsAsync();
                List<string> addonsFilePaths = new List<string>();

                if (Directory.Exists(settingsInfo.GarrysModAddonsFolderPath))
                    addonsFilePaths = Directory
                        .GetFiles(settingsInfo.GarrysModAddonsFolderPath, "*.*", SearchOption.TopDirectoryOnly)
                        .Where(s => s.EndsWith(".gma") || s.EndsWith(".bin"))
                        .ToList();

                if (Directory.Exists(settingsInfo.GarrysModWorkshopFolderPath))
                {
                    string[] workshopSubDirectoryPaths = Directory
                        .GetDirectories(settingsInfo.GarrysModWorkshopFolderPath, "*.*", SearchOption.TopDirectoryOnly);

                    if (workshopSubDirectoryPaths.Length > 0)
                    {
                        foreach (string subDirectoryPath in workshopSubDirectoryPaths)
                        {
                            List<string> subDirectoryFilesPaths = Directory
                                .GetFiles(subDirectoryPath, "*.*", SearchOption.TopDirectoryOnly)
                                .Where(s => s.EndsWith(".gma") || s.EndsWith(".bin"))
                                .ToList();

                            if (subDirectoryFilesPaths.Count > 0)
                                addonsFilePaths.AddRange(subDirectoryFilesPaths);
                        }
                    }
                }

                if (addonsFilePaths.Count == 0) return;

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ProgressBar_Bottom.Minimum = 0;
                    ProgressBar_Bottom.Maximum = addonsFilePaths.Count;
                });

                _readAddonsTasks = new List<Func<Task>>();
                int index = 0;

                foreach (string filePath in addonsFilePaths)
                    _readAddonsTasks.Add(async () =>
                    {
                        AddonInfoModel? addonInfo = null;

                        try
                        {
                            var gmaReader = new GmaReader();
                            addonInfo = await gmaReader.ReadHeaderAsync(filePath);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex);
                        }

                        if (addonInfo != null)
                        {
                            index++;
                            addonInfo.Id = index;
                            await Dispatcher.UIThread.InvokeAsync(() => _context.Data.Add(new AddonDataRowModel(addonInfo)));
                        }

                        await CalculateProgressAsync();
                    });

                await Parallel.ForEachAsync(_readAddonsTasks, async (Func<Task> f, CancellationToken c) =>
                {
                    if (!c.IsCancellationRequested)
                        await f.Invoke();
                    else
                        await CalculateProgressAsync();
                });
            });
        }

        private async Task CalculateProgressAsync()
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (ProgressBar_Bottom.Value + 1 == ProgressBar_Bottom.Maximum)
                {
                    ProgressBar_Bottom.Value = 0;
                    ProgressBar_Text.Text = string.Empty;
                    _canExtractAtThisMoment = false;
                }
                else
                {
                    ProgressBar_Bottom.Value += 1;

                    double value = ProgressBar_Bottom.Value / ProgressBar_Bottom.Maximum;
                    int percent = (int)(value * 100);
                    ProgressBar_Text.Text = $"{percent} % / 100 %";
                }
            });
        }

        private void OnClickOpenSettingsMenu(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_settingsWindow != null)
                return;

            _settingsWindow = new SettingsWindow();
            _settingsWindow.Closing += (s, e) => _settingsWindow = null;
            _settingsWindow.Show();
        }
    }
}
