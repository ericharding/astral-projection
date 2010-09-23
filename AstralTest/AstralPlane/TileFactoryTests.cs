using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Astral.Plane;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AstralTest.AstralPlane
{
    [TestClass]
    public class TileFactoryTests
    {
        [TestMethod]
        public void TestHash()
        {
            
            BitmapImage image = new BitmapImage(new Uri("Images/red.png", UriKind.Relative));

            TileFactory tf = new TileFactory(image, "red", Rect.Empty, 0, 0);

            string id = tf.TileID;
        }
    }
}
