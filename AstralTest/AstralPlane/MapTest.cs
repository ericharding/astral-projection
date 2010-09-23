using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Astral.Plane;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;

namespace AstralTest.AstralPlane
{
    [TestClass]
    public class MapTest
    {
        // [TestMethod]
        public void NewMap1()
        {
            Map m = new Map();

            Rect borders = new Rect(0, 0, 0, 0);
            int tilesHoriz = 1;
            int tilesVert = 1;

            BitmapImage image = new BitmapImage(new Uri("images/red.png", UriKind.Relative));
            TileFactory redtiles = new TileFactory(image, "red" ,borders, tilesHoriz, tilesVert);
            m.AddTileFactory(redtiles);

            image = new BitmapImage(new Uri("images/teal.png", UriKind.Relative));
            TileFactory tealtiles = new TileFactory(image, "teal", borders, 2, 2);
            m.AddTileFactory(tealtiles);

            Assert.IsTrue(m.TileFactories.Contains(redtiles));
            Assert.IsTrue(m.TileFactories.Contains(tealtiles));

            string filename = Path.GetTempFileName();
            try
            {
                m.Save(true, filename);

                // Load a new copy of the map we just saved
                Map m2 = new Map(filename);

                Assert.IsTrue(m2.TileFactories.Contains(redtiles));

            }
            finally
            {
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
            }
        }

    }


}
