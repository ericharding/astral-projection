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
using System.Runtime.InteropServices;
using Astral.Plane.Utility;

namespace Astral.Plane
{
    public class TileFactory : IComparable<TileFactory>, IComparable
    {
        private string _imagePath;
        private string _tags;
        private Borders _borders;
        private bool _arbitraryScale;
        private int _tilesHoriz;
        private int _tilesVert;
        private Lazy<string> _tileID;
        private BitmapSource _bitmapSource;
        private Map _map;

        internal int RefCount { get; set; }

        public TileFactory(BitmapSource image, string tags, Borders borders, int tilesHoriz, int tilesVert, bool arbitraryScale=false)
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
            this._arbitraryScale = arbitraryScale;
        }

        internal TileFactory(Map map, string id, string imagePath, string tags, Borders borders, int tilesHoriz, int tilesVert, bool arbitraryScale)
            : this(null, tags, borders, tilesHoriz, tilesVert, arbitraryScale)
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

        public string DisplayName
        {
            get
            {
                string dim = _arbitraryScale ? "(*)" : string.Format("({0}x{1})", _tilesHoriz, _tilesVert);
                return string.Format("{0} {1}", dim, Tags[0]);
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
            return TileID.GetHashCode();
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

        public Borders Borders
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

        public bool ArbitraryScale
        {
            get { return _arbitraryScale; }
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

            Stream s;
            using (Map.LoadStream(_imagePath, out s))
            {
                PngBitmapDecoder decoder = new PngBitmapDecoder(s, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                _bitmapSource = decoder.Frames[0];
                _bitmapSource.Freeze();
            }
        }

        internal XNode ToXML()
        {
            return new XElement(Map.TILEFACTORY_NODE,
                new XAttribute("TileID", this.TileID),
                new XAttribute("Tags", this._tags),
                new XAttribute("Borders", _borders),
                new XAttribute("Tiles", string.Format("{0},{1}", _tilesHoriz, _tilesVert)),
                new XAttribute("ArbitraryScale", this._arbitraryScale));
        }

        internal static TileFactory FromXML(Map m, XElement node)
        {
            string id, tags;
            Borders borders;
            int tileshoriz, tilesvert;
            bool arbitraryScale;

            id = node.Attribute("TileID").Value;
            tags = node.Attribute("Tags").Value;
            borders = node.Attribute("Borders").Parse(Borders.Parse);
            var tileValues = node.Attribute("Tiles").Parse(s => s.Split(',').Select(t => Int32.Parse(t))).ToArray();
            tileshoriz = tileValues[0];
            tilesvert = tileValues[1];
            arbitraryScale = node.Attribute("ArbitraryScale").Parse(bool.Parse);

            return new TileFactory(m, id, "images/" + id, tags, borders, tileshoriz, tilesvert, arbitraryScale);
        }

        #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    public class Borders : IEquatable<Borders>
    {
        private Borders()
        {
        }

        public Borders(double uniformBorder)
            : this()
        {
            Left = Top = Right = Bottom = uniformBorder;
        }

        public Borders(double left, double top, double right, double bottom)
            : this()
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }

        public double Left { get; set; }
        public double Top { get; set; }
        public double Right { get; set; }
        public double Bottom { get; set; }

        public static Borders Empty
        {
            get
            {
                return new Borders(0);
            }
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}", Left, Top, Right, Bottom);
        }

        public static Borders Parse(string borders)
        {
            string[] values = borders.Split(',');
            var ints = values.Select(s => double.Parse(s));
            var enumerator = ints.GetEnumerator();
            Borders b = new Borders();
            enumerator.MoveNext();
            b.Left = enumerator.Current;
            enumerator.MoveNext();
            b.Top = enumerator.Current;
            enumerator.MoveNext();
            b.Right = enumerator.Current;
            enumerator.MoveNext();
            b.Bottom = enumerator.Current;

            return b;
        }

        #region equality

        public override bool Equals(object other)
        {
            return this.Equals(other as Borders);
        }

        public override int GetHashCode()
        {
            return Left.GetHashCode() ^ Right.GetHashCode() ^ Top.GetHashCode() ^ Bottom.GetHashCode();
        }

        public bool Equals(Borders other)
        {
            return other != null &&
                this.Left == other.Left &&
                this.Top == other.Top &&
                this.Right == other.Right &&
                this.Bottom == other.Bottom;
        }

        public static bool operator ==(Borders left, Borders right)
        {
            return (object.ReferenceEquals(left, null) && object.ReferenceEquals(right, null)) || (right != null && right.Equals(left));
        }

        public static bool operator !=(Borders left, Borders right)
        {
            return !(left == right);
        }
        #endregion
    }
}
