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
        private string _version;
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

        public string Version
        {
            get { return _version; }
            set
            {
                _version = value;
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
