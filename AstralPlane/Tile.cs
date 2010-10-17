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

        internal TileFactory Factory { get; set; }

        public Point Location { get; set; }
        public int Layer { get; set; }
        public TileRotation Rotation { get; set; }
        public TileMirror Mirror { get; set; }
        public string Note { get; set; }
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
                new XAttribute("Rotation", this.Rotation),
                new XAttribute("Mirror", this.Mirror),
                new XAttribute("Note", this.Note));
        }

        internal void LoadFromXML(XElement element)
        {
            Debug.Assert(this.Factory != null);

            this.Location = element.Attribute("Location").Parse(Point.Parse);
            this.Layer = element.Attribute("Layer").Parse(Int32.Parse);
            this.Rotation = element.Attribute("Rotation").Parse(s => (TileRotation)Enum.Parse(typeof(TileRotation), s));
            this.Mirror = element.Attribute("Mirror").Parse(s => (TileMirror)Enum.Parse(typeof(TileMirror), s));
            this.Note = element.Attribute("Note").Parse(s => s);
        }
        
    }
}
