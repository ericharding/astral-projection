using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows;

namespace Astral.Plane
{
    public class TileFactory
    {
        private string _imagePath;
        private string _tags;
        private Rect _borders;
        private int _tilesHoriz;
        private int _tilesVert;

        public TileFactory(string imagePath, string tags, System.Windows.Rect borders, int tilesHoriz, int tilesVert)
        {
            this._imagePath = imagePath;
            this._tags = tags;
            this._borders = borders;
            this._tilesHoriz = tilesHoriz;
            this._tilesVert = tilesVert;
        }

        public string TileID
        {
            get
            {
                // todo: This should be a hash of this tile's properties + the image's bits
                
                throw new NotImplementedException();
            }
        }

        public string[] Tags { get; set; }

        public BitmapImage Image
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Map Map
        {
            get
            {
                throw new NotImplementedException();
            }
            internal set
            {
                // todo: when a tile is added to a map set this
            }
        }

    }
}
