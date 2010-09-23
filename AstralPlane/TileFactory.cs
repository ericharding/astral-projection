using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;

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
        private BitmapSource _bitmapSource;
        private Map _map;

        public TileFactory(BitmapSource image, string tags, Rect borders, int tilesHoriz, int tilesVert)
        {
            this._tileID = new Lazy<string>(ComputeTileHash);
            this._bitmapSource = image;
            this._tags = tags;
            this._borders = borders;
            this._tilesHoriz = tilesHoriz;
            this._tilesVert = tilesVert;
        }

        internal TileFactory(Map map, string imagePath, string tags, Rect borders, int tilesHoriz, int tilesVert)
            : this(null, tags, borders, tilesHoriz, tilesVert)
        {
            _map = map;
            _imagePath = imagePath;
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
                if (_bitmapSource == null)
                {
                    
                }
                return _bitmapSource;
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

            double[] borders = { _borders.Top, _borders.Left, _borders.Bottom, _borders.Right };
            int[] tileCount = { _tilesHoriz, _tilesVert };

            int srcSize = (borders.Length * sizeof(double)) +
                            (tileCount.Length * sizeof(int)) +
                            (this.Image.PixelWidth * this.Image.PixelHeight);
            byte[] imageBits = new byte[srcSize];

            int offset = 0;
            foreach (double d in borders)
            {
                Array.Copy(BitConverter.GetBytes(d), 0, imageBits, offset, sizeof(double));
                offset += sizeof(double);
            }
            foreach (int i in tileCount)
            {
                Array.Copy(BitConverter.GetBytes(i), 0, imageBits, offset, sizeof(int));
                offset += sizeof(int);
            }

            Debug.Assert(srcSize - offset == (this.Image.PixelWidth * this.Image.PixelHeight));
            
            this.Image.CopyPixels(imageBits, imageBits.Length, offset);
                        
            
            SHA1 sha = SHA1.Create();
            

            throw new NotImplementedException();
        }

        private void LoadBitmapSource()
        {
            if (_map == null)
            {
                // Assume filename is a location on disk

            }
            else
            {
                // assume filename is a parturi
            }
        }

        #endregion
    }
}
