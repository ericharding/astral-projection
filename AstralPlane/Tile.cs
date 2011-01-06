using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using System.Diagnostics;
using Astral.Plane.Utility;
using System.Windows.Media.Imaging;

namespace Astral.Plane
{
    public enum TileRotation
    {
        Zero = 0,
        CW90 = 90,
        CW180 = 180,
        CW270 = 270
    }

    [Flags]
    public enum TileMirror
    {
        None = 0,
        Horizontal = 1,
        Vertical = 2,
    }

    public class Tile
    {
        internal Tile(TileFactory source)
        {
            if (source == null) throw new ArgumentNullException("source");
            this.Factory = source;
            this.Note = string.Empty;
        }

        public TileFactory Factory { get; private set; }
        internal Map Map { get; set; }

        private Point _location;
        public Point Location { get { return _location; } set { _location = value; Dirty(); } }
        private int _layer;
        public int Layer
        {
            get { return _layer; }
            set
            {
                if (_layer < 0) throw new ArgumentException("Layer must be positive");
                _layer = value;
                Dirty();
            }
        }
        private TileRotation _rotation;
        public TileRotation Rotation { get { return _rotation; } set { _rotation = value; Dirty(); } }
        private TileMirror _mirror;
        public TileMirror Mirror { get { return _mirror; } set { _mirror = value; Dirty(); } }
        private string _note;
        public string Note { get { return _note; } set { _note = value; Dirty(); } }

        public BitmapSource Image { get { return this.Factory.Image; } }
        public Borders Borders { get { return this.Factory.Borders; } }
        public int TilesHorizontal { get { return this.Factory.TilesHorizontal; } }
        public int TilesVertical { get { return this.Factory.TilesVertical; } }

        internal XNode ToXML()
        {
            return new XElement(Map.TILE_NODE,
                new XAttribute("Type", this.Factory.TileID),
                new XAttribute("Location", this.Location.ToString()),
                new XAttribute("Layer", this.Layer),
                new XAttribute("Rotation", (int)this.Rotation),
                new XAttribute("Mirror", this.Mirror),
                new XAttribute("Note", this.Note));
        }

        internal void LoadFromXML(XElement element)
        {
            Debug.Assert(this.Factory != null);

            this.Location = element.Attribute("Location").Parse(Point.Parse);
            this.Layer = (int)element.Attribute("Layer").Parse(UInt32.Parse);
            this.Rotation = element.Attribute("Rotation").Parse(s =>
                {
                    int rotation;
                    if (Int32.TryParse(s, out rotation))
                    {
                        return (TileRotation)rotation;
                    }
                    // Old map compatability
                    return (TileRotation)Enum.Parse(typeof(TileRotation), s);
                });
            this.Mirror = element.Attribute("Mirror").Parse(s => (TileMirror)Enum.Parse(typeof(TileMirror), s));
            this.Note = element.Attribute("Note").Parse(s => s);
        }

        private void Dirty()
        {
            if (this.Map != null)
            {
                this.Map.IsDirty = true;
            }
        }

    }
}
