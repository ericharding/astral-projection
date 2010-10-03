using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Astral.Plane;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO.Packaging;
using AstralTest.Utility;

namespace AstralTest.AstralPlane
{
    [TestClass]
    public class TileFactoryTests
    {
        [TestMethod]
        public void TestHash()
        {
            

            TileFactory redTile1 = new TileFactory(TestUtility.RedImage, "red", Borders.Empty, 2, 2);
            TileFactory redTile2 = new TileFactory(TestUtility.RedImage, "red", Borders.Empty, 2, 2);
            TileFactory redTile3 = new TileFactory(TestUtility.RedImage, "red", Borders.Empty, 1, 2);
            TileFactory tealTile1 = new TileFactory(TestUtility.TealImage, "teal", Borders.Empty, 2, 2);
            TileFactory tealtile2 = new TileFactory(TestUtility.TealImage, "teal", Borders.Empty, 2, 2);

            Assert.AreEqual(redTile1.TileID, redTile2.TileID);
            Assert.AreEqual(redTile1, redTile2);
            Assert.AreNotEqual(redTile1, redTile3);
            Assert.AreNotEqual(redTile1, tealTile1);
            Assert.AreEqual(tealtile2, tealTile1);
            Assert.IsTrue(redTile1 == redTile2);
            Assert.IsFalse(redTile1 == redTile3);
        }
    }
}
