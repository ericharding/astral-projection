using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace Astral.Plane
{
    public class TileFactory
    {

        public string TileID
        {
            get
            {
                // todo: This should be a hash of this tile's properties + the image's bits
                
                throw new NotImplementedException();
            }
        }

        public string Name { get; set; }

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
