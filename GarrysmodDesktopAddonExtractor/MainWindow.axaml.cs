using Avalonia.Controls;
using Avalonia.Threading;
using CSharpGmaReaderLibrary;
using CSharpGmaReaderLibrary.Models;
using GarrysmodDesktopAddonExtractor.Models;
using GarrysmodDesktopAddonExtractor.Services;
using NLog;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GarrysmodDesktopAddonExtractor
{
    public partial class MainWindow : Window
    {
        private MainContext _context = new MainContext("Version - 1.3.0");
        private SettingsWindow? _settingsWindow = null;
        private List<Func<Task>>? _readAddonsTasks = null;
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private bool _canExtractAtThisMoment = false;
        private bool _canProgress = false;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = _context;

            Menu_Application_Settings.Click += OnClickOpenSettingsMenu;
            Menu_Scan.Click += OnClickScan;
            Menu_ExtractSelected.Click += OnClickExtractSelected;
            Menu_ExtractAll.Click += OnClickExtractAll;
            Menu_Exit.Click += OnClickCloseWindow;
			//Menu_TextBox_Search.TextInput += OnSearchTextInput;
			Menu_TextBox_Search.KeyUp += OnSearchKeyUp;
			Menu_TextBox_Search.KeyDown += OnSearchKeyDown;

#if !DEBUG
            OnScanAddons();
#endif
		}

		private void OnSearchKeyDown(object? sender, Avalonia.Input.KeyEventArgs e) => OnSearchByText();

		private void OnSearchKeyUp(object? sender, Avalonia.Input.KeyEventArgs e) => OnSearchByText();

		private void OnSearchTextInput(object? sender, Avalonia.Input.TextInputEventArgs e) => OnSearchByText();


		private void OnSearchByText()
        {
            if (_context.DataAll == null) return;

			string? searchText = _context.SearchText;

			if (string.IsNullOrWhiteSpace(searchText))
			{
				_context.Data = _context.DataAll;
				return;
			}

			searchText = searchText.Trim().ToLower();
			searchText = Regex.Replace(searchText, @"\s+", string.Empty);

			if (string.IsNullOrWhiteSpace(searchText))
			{
				_context.Data = _context.DataAll;
				return;
			}
			else
			{
				_context.DataSearch = new ObservableCollection<AddonDataRowModel>();
				for (int i = _context.DataAll.Count - 1; i > 0; i--)
				{
					AddonDataRowModel rowEntry = _context.DataAll[i];
                    if (rowEntry.AddonName != null)
                    {
                        string addonName = rowEntry.AddonName.Trim().ToLower();
						addonName = Regex.Replace(addonName, @"\s+", string.Empty);

                        if (addonName.Contains(searchText) || addonName.IndexOf(searchText) > 0)
						    _context.DataSearch.Add(rowEntry);
                    }
				}
				_context.Data = _context.DataSearch;
			}
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

                    var gmaReader = new GmaReader();
                    var gmaHeaderReaderOptions = new ReadHeaderOptions
                    {
                        ReadCacheSingleTime = true,
                        ReadFilesInfo = true,
                    };
                    var gmaExtractFileOptions = new ExtractFileOptions
                    {
                        RewriteExistsFiles = true,
                    };

                    DateTime extractInfoPrintDelay = DateTime.Now;

                    extractTasks.Add(async () =>
                    {
                        try
                        {
                            string folderName = addonDataRow.AddonInfo.Name ?? Guid.NewGuid().ToString("N");
                            string specificExtractPath = Path.Combine(extractFolderPath, GetValidFileName(folderName));
                            string? directoryPath = Path.GetDirectoryName(specificExtractPath);
                            if (directoryPath != null)
                            {
                                string directoryName = Path.GetFileName(specificExtractPath);
								directoryName = directoryName.Trim();
								directoryName = directoryName.ToLower();
								directoryName = Regex.Replace(directoryName, @"\s+", " ");
                                directoryName = directoryName.Replace(" ", "_");

								if (addonDataRow.AddonId != null)
									directoryName = directoryName + "_" + addonDataRow.AddonId.ToString();

                                specificExtractPath = Path.Combine(directoryPath, directoryName);
							}

							await Dispatcher.UIThread.InvokeAsync(() => ProgressBar_Text.Text = "Extract: " + addonDataRow.AddonInfo.Name);

							var gmaExtractor = new GmaExtractor(specificExtractPath);
                            var options = new ReadFileContentOptions { AddonInfo = addonDataRow.AddonInfo, HeaderOptions = gmaHeaderReaderOptions };

                            await gmaReader.ReadFileContentAsync(addonDataRow.AddonInfo.SourcePath, async (FileContentModel content) =>
                            {
                                await gmaExtractor.ExtractFileAsync(content, gmaExtractFileOptions);
                                if (extractInfoPrintDelay < DateTime.Now)
                                {
                                    extractInfoPrintDelay = DateTime.Now.AddSeconds(.5);
									await Dispatcher.UIThread.InvokeAsync(() => ProgressBar_Text.Text = "Extract: " + addonDataRow.AddonInfo.Name + " - " + content.FilePath);
                                }
                            }, options);
                            //await gmaExtractor.MakeDescriptionFile(addonDataRow.AddonInfo);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex);
                        }
                        finally
                        {
                            await CalculateProgressAsync();

                            if (_canProgress == false)
                                gmaReader.Dispose();
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
            _context.DataAll = new ObservableCollection<AddonDataRowModel>();
            _context.Data.Clear();

			Task.Factory.StartNew(async () =>
            {
                SettingsInfo settingsInfo = await SettingsService.GetSettingsAsync();
				var addonsFilePathsAll = new List<AddonPathModel>();
				var addonsFilePathsAddons = new List<AddonPathModel>();
				var addonsFilePathsWorkshopEntries = new List<AddonPathModel>();

				if (Directory.Exists(settingsInfo.GarrysModWorkshopFolderPath))
                {
					string[] workshopSubDirectoryPaths = Directory
                        .GetDirectories(settingsInfo.GarrysModWorkshopFolderPath, "*.*", SearchOption.TopDirectoryOnly);

                    if (workshopSubDirectoryPaths.Length > 0)
                    {
						foreach (string subDirectoryPath in workshopSubDirectoryPaths)
                        {
                            List<string> addonsFilePaths = Directory
                                .GetFiles(subDirectoryPath, "*.*", SearchOption.TopDirectoryOnly)
                                .Where(s => s.EndsWith(".gma") || s.EndsWith(".bin"))
                                .ToList();

							foreach (string filePath in addonsFilePaths)
								addonsFilePathsWorkshopEntries.Add(new AddonPathModel
								{
									AddonId = Path.GetFileName(Path.GetDirectoryName(filePath)),
									FilePath = filePath,
									IsWorkshop = true,
								});
						}
                    }
                }

				if (Directory.Exists(settingsInfo.GarrysModAddonsFolderPath))
                {
					List<string> addonsFilePaths = Directory
						.GetFiles(settingsInfo.GarrysModAddonsFolderPath, "*.*", SearchOption.TopDirectoryOnly)
						.Where(s => s.EndsWith(".gma"))
						.ToList();

					foreach (string filePath in addonsFilePaths)
                    {
                        string[] filePathParts = Path.GetFileNameWithoutExtension(filePath).Split("_");
                        string addonId = filePathParts.Length > 0 ? filePathParts[filePathParts.Length - 1] : string.Empty;
                        DateTime lastEditTime = File.GetLastWriteTime(filePath);

                        for (int i = addonsFilePathsWorkshopEntries.Count - 1; i >= 0; i--)
                        {
							AddonPathModel workshopFilePathEntry = addonsFilePathsWorkshopEntries[i];
                            if (workshopFilePathEntry.AddonId != addonId)
                                continue;

                            string workshopFilePath = workshopFilePathEntry.FilePath;
							//if (File.GetLastWriteTime(workshopFilePath) <= lastEditTime && Path.GetExtension(workshopFilePath) == ".bin")
							if (File.GetLastWriteTime(workshopFilePath) <= lastEditTime)
							{
								addonsFilePathsAddons.Add(new AddonPathModel
                                {
                                    AddonId = addonId,
                                    FilePath = filePath,
                                    IsWorkshop = false,
                                });

								addonsFilePathsWorkshopEntries.RemoveAt(i);
							}
						}
					}
				}

                addonsFilePathsAll.AddRange(addonsFilePathsWorkshopEntries);
				addonsFilePathsAll.AddRange(addonsFilePathsAddons);

				await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ProgressBar_Bottom.Minimum = 0;
                    ProgressBar_Bottom.Maximum = addonsFilePathsAll.Count;
                });

                await CalculateProgressTextAsync();

                _readAddonsTasks = new List<Func<Task>>();

                var skipHashes = new List<string>();
                var gmaReader = new GmaReader();
                var gmaHeaderReaderOptions = new ReadHeaderOptions
                {
                    UseCache = true,
                    ReadCacheSingleTime = true,
                    ReadFilesInfo = true,
                };

                /*
				foreach (AddonPathModel addonPath in addonsFilePathsAll)
                {
					AddonInfoModel? addonInfo = null;

					try
					{
						addonInfo = await gmaReader.ReadHeaderAsync(addonPath.FilePath, gmaHeaderReaderOptions);
                        if (addonInfo != null)
                            _logger.Info("Reading addon: {0} - {1}", addonInfo.Name ?? "None", addonInfo.SourcePath ?? "None");
					}
					catch (Exception ex)
					{
						_logger.Error(ex);
#if DEBUG
                        Console.WriteLine(string.Format("ERROR:\n{0}", ex));
#endif
                    }
					finally
					{
						if (addonInfo != null)
						{
							if (addonInfo.AddonFileHash != null)
								await Dispatcher.UIThread.InvokeAsync(() => _context.Data.Add(new AddonDataRowModel(addonInfo)));
							else
								await Dispatcher.UIThread.InvokeAsync(() => ProgressBar_Bottom.Maximum--);
						
							//if (addonInfo.AddonFileHash != null && !skipHashes.Exists(x => x == addonInfo.AddonFileHash))
							//{
							//	skipHashes.Add(addonInfo.AddonFileHash);
							//	await Dispatcher.UIThread.InvokeAsync(() => _context.Data.Add(new AddonDataRowModel(addonInfo)));
							//}
							//else
							//{
							//	await Dispatcher.UIThread.InvokeAsync(() => ProgressBar_Bottom.Maximum--);
							//}
						}

						await CalculateProgressAsync();
					}
				}

				gmaReader.Dispose();
				_canProgress = false;
                */

                foreach (AddonPathModel addonPath in addonsFilePathsAll)
                    _readAddonsTasks.Add(async () =>
                    {
                        AddonInfoModel? addonInfo = null;

                        try
                        {
                            addonInfo = await gmaReader.ReadHeaderAsync(addonPath.FilePath, gmaHeaderReaderOptions);
                            if (addonInfo != null && long.TryParse(addonPath.AddonId, out long addonId))
                                addonInfo.AddonId = addonId;
						}
                        catch (Exception ex)
                        {
                            _logger.Error(ex);
#if DEBUG
							Console.WriteLine(string.Format("ERROR:\n{0}", ex));
#endif
						}
						finally
                        {
                            if (addonInfo != null)
                            {
								//if (addonInfo.AddonFileHash != null && !skipHashes.Exists(x => x == addonInfo.AddonFileHash))
								//{
								//    skipHashes.Add(addonInfo.AddonFileHash);
								//    await Dispatcher.UIThread.InvokeAsync(() => _context.Data.Add(new AddonDataRowModel(addonInfo)));
								//}
								//else
								//{
								//    await Dispatcher.UIThread.InvokeAsync(() => ProgressBar_Bottom.Maximum--);
								//}

								await Dispatcher.UIThread.InvokeAsync(() => _context.DataAll.Add(new AddonDataRowModel(addonInfo)));
							}

                            await CalculateProgressAsync();

                            if (_canProgress == false)
                                gmaReader.Dispose();
                        }
                    });

				//var parallelOptions = new ParallelOptions()
				//{
				//	MaxDegreeOfParallelism = 6
				//};

				await Parallel.ForEachAsync(_readAddonsTasks, async (Func<Task> f, CancellationToken c) =>
                {
                    if (!c.IsCancellationRequested)
                        await f.Invoke();
                    else
                        await CalculateProgressAsync();
                });

                _context.Data = _context.DataAll;
			});
        }

        private async Task CalculateProgressAsync()
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (ProgressBar_Bottom.Value + 1 == ProgressBar_Bottom.Maximum)
                {
                    ProgressBar_Bottom.Value = 0;
                    _canExtractAtThisMoment = false;
                    _canProgress = false;
                }
                else
                {
                    ProgressBar_Bottom.Value += 1;
                    _canProgress = true;
                }
            });

            await CalculateProgressTextAsync();
        }

        private async Task CalculateProgressTextAsync()
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!_canProgress)
                    ProgressBar_Text.Text = string.Empty;
                else
                {
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
