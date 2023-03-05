using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GarrysmodDesktopAddonExtractor.Models
{
    public class SwrringsContext : INotifyPropertyChanged
    {
        /* Private */
        public string _garrysModAddonsFolderPath = string.Empty;
        public string _garrysModWorkshopFolderPath = string.Empty;

        /* Public */
        public string GarrysModAddonsFolderPath
        {
            get { return _garrysModAddonsFolderPath; }
            set
            {
                _garrysModAddonsFolderPath = value;
                NotifyPropertyChanged();
            }
        }

        public string GarrysModWorkshopFolderPath
        {
            get { return _garrysModWorkshopFolderPath; }
            set
            {
                _garrysModWorkshopFolderPath = value;
                NotifyPropertyChanged();
            }
        }

        /* Event */
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
