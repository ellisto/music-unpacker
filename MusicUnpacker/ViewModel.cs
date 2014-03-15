using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MusicUnpacker
{
    public class ViewModel : INotifyPropertyChanged
    {
        private string _musicLibraryPath;
        private string _zipPath;

        public string MusicLibraryPath
        {
            get
            {
                return _musicLibraryPath;
            }
            set {
                if (value != _musicLibraryPath)
                {
                    _musicLibraryPath = value;
                    OnPropertyChanged("MusicLibraryPath");
                }
            }
        }

        public string ZipPath
        {
            get
            {
                return _zipPath;
            }
            set
            {
                if (value != _zipPath)
                {
                    _zipPath = value;
                    OnPropertyChanged("ZipPath");
                }
            }
        }

        /// <summary>
        /// Takes zip specified in ZipPath and unpacks it into the music library rooted at MusicLibraryPath
        /// </summary>
        public void UnpackZip()
        {
            try
            {
                //get a temp folder
                var extractPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(extractPath);

                using (ZipArchive archive = ZipFile.Open(ZipPath, ZipArchiveMode.Read))
                {
                    archive.ExtractToDirectory(extractPath);
                    MessageBox.Show(string.Format("Files extracted to {0}.",extractPath));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Error: {0}", e.Message),"Error");
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion
    }
}
