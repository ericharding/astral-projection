using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Xml.Linq;

namespace Astral.Plane
{
    public class TileFactory : IComparable<TileFactory>, IComparable
    {
        private string _imagePath;
        private string _tags;
        private Thickness _borders;
        private int _tilesHoriz;
        private int _tilesVert;
        private Lazy<string> _tileID;
        private int _hashCode;
        private BitmapSource _bitmapSource;
        private Map _map;

        public TileFactory(BitmapSource image, string tags, Thickness borders, int tilesHoriz, int tilesVert)
        {
            if (tilesHoriz <= 0 || tilesVert <= 0)
            {
                throw new ArgumentException("tiles must be greater than 0 in both directions.");
            }

            this._tileID = new Lazy<string>(ComputeTileHash);
            this._bitmapSource = image;
            this._tags = tags;
            this._borders = borders;
            this._tilesHoriz = tilesHoriz;
            this._tilesVert = tilesVert;
        }

        internal TileFactory(Map map, string id ,string imagePath, string tags, Thickness borders, int tilesHoriz, int tilesVert)
            : this(null, tags, borders, tilesHoriz, tilesVert)
        {
            _tileID = new Lazy<string>(() => id);
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

        public string[] Tags
        {
            get
            {
                return _tags.Split(';');
            }
        }

        public BitmapSource Image
        {
            get
            {
                if (_bitmapSource == null)
                {
                    LoadBitmapSource();
                }
                return _bitmapSource;
            }
        }

        /// <summary>
        /// The map that contains this TileFactory
        /// </summary>
        internal Map Map
        {
            get
            {
                return _map;
            }
            set
            {
                _map = value;
            }
        }

        #region Comparison

        public int CompareTo(TileFactory other)
        {
            return TileID.CompareTo(other.TileID);
        }

        public int CompareTo(object other)
        {
            return this.CompareTo(other as TileFactory);
        }

        public override bool Equals(object obj)
        {
            return obj is TileFactory &&
                this.GetHashCode() == obj.GetHashCode() &&
                this.TileID == ((TileFactory)obj).TileID;
        }

        public override int GetHashCode()
        {
            if (_hashCode == 0)
            {
                ComputeTileHash();
            }
            return _hashCode;
        }

        public static bool operator ==(TileFactory left, TileFactory right)
        {
			if ((object)left == null)
				return (object)right == null;

			return left.Equals(right);
        }

        public static bool operator !=(TileFactory left, TileFactory right)
        {
			if ((object)left == null)
				return (object)right != null;

			return !left.Equals(right);
        }

        #endregion

        /// <summary>
        /// Creates a new tile of this type
        /// </summary>
        /// <returns></returns>
        public Tile CreateTile()
        {
            // This is using a factory pattern b/c we now have the option of
            // automatically adding this tile to the Map.  Not doing that yet though...
            return new Tile(this);
        }

        public Thickness Borders
        {
            get
            {
                return _borders;
            }
        }

        public int TilesHorizontal
        {
            get { return _tilesHoriz; }
        }

        public int TilesVertical
        {
            get { return _tilesVert; }
        }

        public override string ToString()
        {
			return string.Format("{0} ({1}x{2})", _tags, _tilesHoriz, _tilesVert);
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
            memStream.Seek(0, SeekOrigin.Begin);
            return memStream;
        }
        internal bool HasBitmapSource { get { return _bitmapSource != null; } }

        #endregion

        #region Private

        private string ComputeTileHash()
        {
            if (this.Image == null) throw new InvalidOperationException("Cannot compute hash for uninitialized TileFactory");

            double[] borders = { _borders.Top, _borders.Left, _borders.Bottom, _borders.Right };
            int[] tileCount = { _tilesHoriz, _tilesVert };

            int srcSize = (borders.Length * sizeof(double)) +
                            (tileCount.Length * sizeof(int)) +
                            (this.Image.PixelWidth * this.Image.PixelHeight * 4);
            byte[] imageBits = new byte[srcSize];

            int offset = 0;
            foreach (double d in borders)
            {
                byte[] bytes = BitConverter.GetBytes(d);
                Array.Copy(bytes, 0, imageBits, offset, sizeof(double));
                offset += sizeof(double);
            }
            foreach (int i in tileCount)
            {
                Array.Copy(BitConverter.GetBytes(i), 0, imageBits, offset, sizeof(int));
                offset += sizeof(int);
            }

            Debug.Assert(srcSize - offset == (this.Image.PixelWidth * this.Image.PixelHeight * 4));

            this.Image.CopyPixels(imageBits, this.Image.PixelWidth * 4, offset);

            SHA1 sha = SHA1.Create();
            byte[] hash = sha.ComputeHash(imageBits);

            int[] hashcodes = { BitConverter.ToInt32(hash, 0),
                                BitConverter.ToInt32(hash, 4),
                                BitConverter.ToInt32(hash, 8),
                                BitConverter.ToInt32(hash, 12),
                                BitConverter.ToInt32(hash, 16)};
            _hashCode = hashcodes[0] ^ hashcodes[1] ^ hashcodes[2] ^ hashcodes[3] ^ hashcodes[4];

            StringBuilder str = new StringBuilder(40);
            for (int x = 0; x < 20; x++)
            {
                str.AppendFormat("{0:X2}", hash[x]);
            }
            return str.ToString() + ".png";
        }

        internal void LoadBitmapSource()
        {
            if (Map == null) throw new InvalidOperationException("Cannot load bitmap from path when not part of a Map");

            Stream s = Map.LoadStream(_imagePath);
            PngBitmapDecoder decoder = new PngBitmapDecoder(s, BitmapCreateOptions.None, BitmapCacheOption.None);
            _bitmapSource = decoder.Frames[0];
            _bitmapSource.Freeze();
        }

        internal XNode ToXML()
        {
            return new XElement("Tiletype",
                new XAttribute("TileID", this.TileID),
                new XAttribute("tags", this._tags),
                new XAttribute("Borders", _borders),
                new XAttribute("Tiles", string.Format("{0},{1}", _tilesHoriz, _tilesVert)));

        }

        internal void FromXML(XElement node)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
