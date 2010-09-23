using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows;
using System.IO;

namespace Astral.Plane
{
    public class TileFactory : IComparable<TileFactory>
    {
        private string _imagePath;
        private string _tags;
        private Rect _borders;
        private int _tilesHoriz;
        private int _tilesVert;
        private Lazy<string> _tileID;
        private Lazy<BitmapSource> _bitmapSource;

        public TileFactory(string imagePath, string tags, System.Windows.Rect borders, int tilesHoriz, int tilesVert)
        {
            this._tileID = new Lazy<string>(ComputeTileHash);
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
                return _tileID.Value;
            }
        }

        public string[] Tags { get; set; }

        public BitmapSource Image
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

        public int CompareTo(TileFactory other)
        {
            return TileID.CompareTo(other.TileID);
        }


        #region Internal

        internal Stream GetImageStream()
        {
            return this.GetImageStream(new PngBitmapEncoder());
        }
        internal Stream GetImageStream(BitmapEncoder encoder)
        {
            encoder.Frames.Add(BitmapFrame.Create(Image));
            MemoryStream memStream = new MemoryStream();
            encoder.Save(memStream);
            return memStream;
        }

        #endregion


        #region Private

        private string ComputeTileHash()
        {
            if (this.Image == null) throw new InvalidOperationException("Cannot compute hash for uninitialized TileFactory");


            throw new NotImplementedException();
        }

        #endregion
    }
}
