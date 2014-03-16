using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicUnpacker
{
    public class MockAlbumInfo : AlbumInfo
    {
        public MockAlbumInfo()
            : base()
        {
            Title = "Disintegration";
            Artist = "The Cure";
            Genre = "Awesome";
        }
    }
}
