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
        private string _nextStep;
        private Step _currentStep;
        private AlbumInfo _album;

        public const string VARIOUSARTISTS = "Various Artists";

        public ViewModel()
        {
            CurrentStep = Step.Initial;
        }

        public string MusicLibraryPath
        {
            get
            {
                return _musicLibraryPath;
            }
            set
            {
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

        public AlbumInfo Album
        {
            get { return _album; }
            set
            {
                if (value != _album)
                {
                    _album = value;
                    OnPropertyChanged("Album");
                }
            }
        }

        public Step CurrentStep
        {
            get { return _currentStep; }
            set
            {
                if (value != _currentStep)
                {
                    _currentStep = value;
                    OnPropertyChanged("CurrentStep");
                    OnPropertyChanged("NextStep");
                }
            }
        }

        public string NextStep
        {
            get {
                switch (CurrentStep)
                {
                    case Step.Initial:
                        return "Read In File";
                    case Step.Extract:
                        return "Import to Music Library";
                    default:
                        return "Clear";
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
                    MessageBox.Show(string.Format("Files extracted to {0}.", extractPath));
                    Album = GetAlbumInfo(extractPath);
                    //TODO: move files to target
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Error: {0}", e.Message), "Error");
            }
        }

        /// <summary>
        /// looks at mp3 files in folder in sourceDirectoryPath and determines the correct artist/album to use
        /// </summary>
        private AlbumInfo GetAlbumInfo(string sourceDirectoryPath)
        {
            var albumInfo = new AlbumInfo() { Title = string.Empty, Artist = string.Empty, Genre = string.Empty };

            foreach (var filename in Directory.EnumerateFiles(sourceDirectoryPath))
            {
                TagLib.File tagfile = TagLib.File.Create(Path.Combine(sourceDirectoryPath, filename));

                //n.b. we just take the first title we see; assuming zip has only one album.
                //TODO: support multiple albums per zip
                if (albumInfo.Title == string.Empty && tagfile.Tag.Album != null)
                {
                    albumInfo.Title = tagfile.Tag.Album;
                }

                // set artist; if more than one track has different artists, set artist to VARIOUSARTISTS constant
                //TODO: support multiple artists
                var tagArtist = tagfile.Tag.FirstAlbumArtist;
                if (albumInfo.Artist != VARIOUSARTISTS && tagArtist != null && tagArtist != albumInfo.Artist)
                {
                    albumInfo.Artist = (albumInfo.Artist == string.Empty) ? tagArtist : VARIOUSARTISTS;
                }

                //take the first genre we see and use it for the whole album.
                //TODO: support multiple genres
                var tagGenre = tagfile.Tag.FirstGenre;
                if (albumInfo.Genre == string.Empty && tagGenre != null)
                {
                    albumInfo.Genre = tagGenre;
                }
            }
            return albumInfo;
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

    public enum Step
    {
        Initial,
        Extract,
        Import
    }
}
