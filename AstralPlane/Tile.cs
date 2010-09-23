using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Linq;

namespace Astral.Plane
{
    public enum Rotation
    {
        Zero = 0,
        CW90 = 90,
        CW180 = 180,
        CW270 = 270
    }

    public class Tile
    {
        internal Tile(TileFactory source)
        {
            this.Factory = source;
        }

        internal TileFactory Factory { get; set; }

        public Point Location { get; set; }
        public int Layer { get; set; }
        public int Rotation { get; set; }
   
        internal XNode ToXML()
        {
            return new XElement("Tile",
                new XAttribute("Location.X", this.Location.X),
                new XAttribute("Location.Y", this.Location.Y),
                new XAttribute("Layer", this.Layer),
                new XAttribute("Rotation", this.Rotation));
        }

        internal void FromXML(XElement element)
        {
            throw new NotImplementedException();
        }
        
    }
}
