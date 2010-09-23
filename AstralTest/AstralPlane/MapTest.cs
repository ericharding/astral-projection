using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Astral.Plane;
using System.Windows;

namespace AstralTest.AstralPlane
{
    [TestClass]
    public class MapTest
    {
        [TestMethod]
        public void NewMap1()
        {
            Map m = new Map();

            Rect borders = new Rect(0, 0, 0, 0);
            int tilesHoriz = 1;
            int tilesVert = 1;
            
            TileFactory redtiles = new TileFactory("images/red.png", "red" ,borders, tilesHoriz, tilesVert);

        }
    }
}
