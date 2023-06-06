using CSharpGmaReaderLibrary.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GarrysmodDesktopAddonExtractor.Models
{
    public class MainContext : INotifyPropertyChanged
    {
        /* Private */
        private ObservableCollection<AddonDataRowModel> _data;
		private ObservableCollection<AddonDataRowModel> _dataAll;
		private ObservableCollection<AddonDataRowModel> _dataSearch;
		private string _version;
        private string? _searchText;
		private bool _showGridLines;

        /* Public */
        public MainContext(string applicationVersion)
        {
            _data = new ObservableCollection<AddonDataRowModel>();
            _data.CollectionChanged += AddonInfosCollectionChanged;
            _version = applicationVersion;

#if DEBUG
            _showGridLines = true;
#endif
        }

        public ObservableCollection<AddonDataRowModel> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                NotifyPropertyChanged();
            }
        }

		public ObservableCollection<AddonDataRowModel> DataSearch
		{
			get { return _dataSearch; }
			set
			{
				_dataSearch = value;
				NotifyPropertyChanged();
			}
		}

		public ObservableCollection<AddonDataRowModel> DataAll
		{
			get { return _dataAll; }
			set
			{
				_dataAll = value;
				NotifyPropertyChanged();
			}
		}

		public string Version
        {
            get { return _version; }
            set
            {
                _version = value;
                NotifyPropertyChanged();
            }
        }

		public string? SearchText
		{
            get { return _searchText; }
			set
			{
				_searchText = value;
				NotifyPropertyChanged();
			}
		}

		public bool ShowGridLines
        {
            get { return _showGridLines; }
            set
            {
                _showGridLines = value;
                NotifyPropertyChanged();
            }
        }

        /* Event */
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AddonInfosCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged("AddonInfos");
        }
    }
}
