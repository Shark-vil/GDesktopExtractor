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

        /* Public */
        public MainContext()
        {
            _data = new ObservableCollection<AddonDataRowModel>();
            _data.CollectionChanged += AddonInfosCollectionChanged;
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
