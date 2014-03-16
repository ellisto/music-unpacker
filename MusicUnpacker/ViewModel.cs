using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        private AlbumInfo _album;
        private string _tempPath;

        public const string VARIOUSARTISTS = "Various Artists";

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
                    if (_zipPath != null && _zipPath != string.Empty)
                        ProcessNewZip();
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

        /// <summary>
        /// Takes zip specified in ZipPath, extracts it to a temp dir, reads ID3 tags to determine album title, artist, and genre
        /// </summary>
        public void ProcessNewZip()
        {
            try
            {
                //get a temp folder
                _tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(_tempPath);

                using (ZipArchive archive = ZipFile.Open(ZipPath, ZipArchiveMode.Read))
                {
                    archive.ExtractToDirectory(_tempPath);
                    //MessageBox.Show(string.Format("Files extracted to {0}.", _tempPath));
                    Album = GetAlbumInfo(_tempPath);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Error: {0}", e.Message), "Error");
            }
        }

        /// <summary>
        /// Takes currently selected album and imports it into specified music library.
        /// </summary>
        public void ImportAlbum()
        {
            //TODO: get a desired path from user
            try
            {
                var targetPath = Path.Combine(MusicLibraryPath, Album.Artist, Album.Title);
                //for now, using LIBRARY/ARTIST/ALBUM/*.mp3
                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }

                foreach (var sourcePath in Directory.EnumerateFiles(_tempPath))
                {
                    var destPath = Path.Combine(targetPath,Path.GetFileName(sourcePath));
                    System.IO.File.Copy(sourcePath, destPath);
                }
                Process.Start(targetPath);
            }
            catch (Exception e)
            {
                //TODO: improve exception handling
                MessageBox.Show("Error: " + e.Message);
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
}
