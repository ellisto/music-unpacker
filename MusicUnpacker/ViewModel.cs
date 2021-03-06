﻿using System;
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
        private bool _isBusy;
        private string _busyMessage;

        private List<string> _cleanUpPaths;

        public const string VARIOUSARTISTS = "Various Artists";

        public ViewModel()
        {
            _cleanUpPaths = new List<string>();
            MusicLibraryPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        }

        //TODO: persist MusicLibraryPath in an App.Settings or similar
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

                    //TODO: add a spinner or something, because this can take a while sometimes.
                    if (_zipPath != null && _zipPath != string.Empty)
                    {
                        ProcessNewZip();
                    }
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

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (value != _isBusy)
                {
                    _isBusy = value;
                    OnPropertyChanged("IsBusy");
                }
            }
        }

        public string BusyMessage
        {
            get
            {
                return _busyMessage;
            }
            set
            {
                if (value != _busyMessage)
                {
                    _busyMessage = value;
                    OnPropertyChanged("BusyMessage");
                }
            }
        }

        public void SetBusy(string message = null)
        {
            IsBusy = true;
            if (message != null)
            {
                BusyMessage = message;
            }
        }

        public void UnsetBusy()
        {
            IsBusy = false;
        }

        /// <summary>
        /// Takes zip specified in ZipPath, extracts it to a temp dir, reads ID3 tags to determine album title, artist, and genre
        /// </summary>
        public async void ProcessNewZip()
        {
            SetBusy("Processing...");
            try
            {
                //get a temp folder
                _tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(_tempPath);
                _cleanUpPaths.Add(_tempPath);

                using (ZipArchive archive = ZipFile.Open(ZipPath, ZipArchiveMode.Read))
                {
                    await Task.Factory.StartNew(() => archive.ExtractToDirectory(_tempPath));
                    Console.WriteLine("Files extracted to {0}.", _tempPath);
                    Album = GetAlbumInfo(_tempPath);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Error: {0}", e.Message), "Error");
            }
            UnsetBusy();
        }

        /// <summary>
        /// Takes currently selected album and imports it into specified music library.
        /// </summary>
        public void ImportAlbum()
        {
            SetBusy("Importing album...");
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
                    var destPath = Path.Combine(targetPath, Path.GetFileName(sourcePath));
                    System.IO.File.Copy(sourcePath, destPath);
                }
                Process.Start(targetPath);
            }
            catch (Exception e)
            {
                //TODO: improve exception handling
                MessageBox.Show("Error: " + e.Message);
            }
            UnsetBusy();
        }

        /// <summary>
        /// looks at mp3 files in folder in sourceDirectoryPath and determines the correct artist/album to use
        /// </summary>
        private AlbumInfo GetAlbumInfo(string sourceDirectoryPath)
        {
            var albumInfo = new AlbumInfo() { Title = string.Empty, Artist = string.Empty, Genre = string.Empty };

            //TODO: iterate down into file structure until we find mp3s, in case the source zip had folders inside.
            foreach (var filename in Directory.EnumerateFiles(sourceDirectoryPath))
            {
                try
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
                catch (TagLib.UnsupportedFormatException e)
                {
                    //there might be unsupported files in the zip, like pdf liner notes or cover art; skip these.
                    continue;
                }
            }
            return albumInfo;
        }

        public void CleanUp()
        {
            foreach (var tempDir in _cleanUpPaths.Select(p => new DirectoryInfo(p)))
            {
                try
                {
                    tempDir.Delete(true);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error cleaning up directory {0}: {1}", tempDir.FullName, e.Message);
                }
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
