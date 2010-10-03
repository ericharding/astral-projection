using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Astral.Plane;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;
using AstralTest.Utility;
using Astral.Plane.Container;
using System.Xml.Linq;

/*
 * General Idea:
 * 
 * The Map can be both a "map" and a library of tiles.
 * The map editor program (AstralMap) will keep all the tiles that are imported in one or more logically grouped Maps.
 * 
 * Example:
 * User loads Astralmap
 *     AstralMap loads the tile library consisting of 2 .astral files DungeonTiles.astral and CaveTiles.astral
 *     Map dungeonTiles = new Map("DungeonTiles.astral");  Map caveTiles = new Map("CaveTiles.astral");
 *     AstralMap enumerates the TileFactories in those maps and displays them in the "library" pane.
 * User selects File->New map.  
 * AstralMap calls:  
 *      Map newMap = new Map();  // Create a new empty map
 *      newMap.AddReference(dungeonTiles) // instance of first library map
 *      newMap.AddReference(caveTiles) // instance of second library Map
 * The user picks a tile from the library and places it on the map
 *      Astralmap gets the selected TileFactory and calls CreateTile() to make a tile instance
 *      Astralmap sets any relevant properties (like position) and calls
 *      newMap.Addtile(newtile);
 * When the user is done with the map he clicks "save"
 *      AstralMap can now call newMap.Save(false); to create a local version of the map. This map cannot be loaded without the library that the tiles came from
 *      Astralmap could also call newmap.Save(true); this saves a completely self contained map.  Any TileFactory that is referenced by a tile is saved as part of the map.
 * 
 */ 


namespace AstralTest.AstralPlane
{
    [TestClass]
    public class MapTest
    {
        [TestMethod]
        public void MapBasic()
        {
            Map map = new Map();

            TileFactory redtiles = new TileFactory(TestUtility.RedImage, "red", Borders.Empty, 1, 1);
            map.AddTileFactory(redtiles);

            TileFactory tealtiles = new TileFactory(TestUtility.TealImage, "teal", Borders.Empty, 2, 2);
            map.AddTileFactory(tealtiles);

            TileFactory mustardTiles = new TileFactory(TestUtility.MustardImage, "mustard", Borders.Empty, 1, 1);
            // not adding to map

            Assert.IsTrue(map.TileFactories.Contains(redtiles));
            Assert.IsTrue(map.TileFactories.Contains(tealtiles));

            Tile tred = redtiles.CreateTile();
            Tile tred2 = redtiles.CreateTile();

            map.AddTile(tred);
            map.AddTile(tred2);

            Assert.IsTrue(map.Tiles.Contains(tred));

            Tile tmust = mustardTiles.CreateTile();

            // Tiles that aren't in the reference chain of Map's factories cannot be added to the map
            bool success = false;
            try
            {
                map.AddTile(tmust);
            }
            catch
            {
                success = true;
            }

            Assert.IsTrue(success);
        }

        [TestMethod]
        public void MapReference()
        {
            Map map = new Map();
            Map map2 = new Map();
            map.AddReference(map2);

            Borders borders = Borders.Empty;

            TileFactory redtiles = new TileFactory(TestUtility.RedImage, "red", borders, 1, 1);
            map.AddTileFactory(redtiles);

            TileFactory tealtiles = new TileFactory(TestUtility.TealImage, "teal", borders, 2, 2);
            map.AddTileFactory(tealtiles);

            TileFactory mustardTiles = new TileFactory(TestUtility.MustardImage, "mustard", borders, 1, 1);
            map2.AddTileFactory(mustardTiles);

            Assert.IsTrue(map.TileFactories.Contains(redtiles));
            Assert.IsTrue(map.TileFactories.Contains(tealtiles));

            Tile tred = redtiles.CreateTile();
            Tile tred2 = redtiles.CreateTile();

            map.AddTile(tred);
            map.AddTile(tred2);

            Assert.IsTrue(map.Tiles.Contains(tred));

            Tile tmust = mustardTiles.CreateTile();

            // OK this time b/c it is handled by the reference
            map.AddTile(tmust);
        }

        [TestMethod]
        public void MapReferenceEquivalent()
        {
            Map map = new Map();
            Map map2 = new Map();
            Map map2Equivalent = new Map();
            map.AddReference(map2Equivalent);

            Borders borders = Borders.Empty;

            TileFactory redtiles = new TileFactory(TestUtility.RedImage, "red", borders, 1, 1);
            map.AddTileFactory(redtiles);

            TileFactory tealtiles = new TileFactory(TestUtility.TealImage, "teal", borders, 2, 2);
            map.AddTileFactory(tealtiles);

            TileFactory mustardTiles = new TileFactory(TestUtility.MustardImage, "mustard", borders, 1, 1);
            map2.AddTileFactory(mustardTiles);

            TileFactory otherMustard = new TileFactory(TestUtility.MustardImage, "mustard2", borders, 1, 1);
            map2Equivalent.AddTileFactory(otherMustard);

            Assert.IsTrue(map.TileFactories.Contains(redtiles));
            Assert.IsTrue(map.TileFactories.Contains(tealtiles));

            Tile tred = redtiles.CreateTile();
            Tile tred2 = redtiles.CreateTile();

            map.AddTile(tred);
            map.AddTile(tred2);

            Assert.IsTrue(map.Tiles.Contains(tred));

            Tile tmust = mustardTiles.CreateTile();

            // tmust comes from map2 but there is another tile otherMustard in map2Equivalent which satisfies the requirement so this is ok
            map.AddTile(tmust);
        }

        [TestMethod]
        public void MapTileSharing()
        {
            Map map = new Map();
            Map map2 = new Map();

            TileFactory redtiles = new TileFactory(TestUtility.RedImage, "red", Borders.Empty, 1, 1);

            // This is OK since the tile has an image
            map.AddTileFactory(redtiles);
            map2.AddTileFactory(redtiles);

            // todo: If the tile did not have an image loaded then it would be forced to load it's image when added to another map
            // If it were unable to load the image it would throw

        }

        [TestMethod]
        public void MapSaveBasic()
        {
            Map map = new Map();
            Borders borders = Borders.Empty;

            TileFactory redtiles = new TileFactory(TestUtility.RedImage, "red", borders, 1, 1);
            map.AddTileFactory(redtiles);

            Tile tile1 = redtiles.CreateTile();
            tile1.Location = new Point(100, 100);
            map.AddTile(tile1);

            string tempFile = Path.GetTempFileName();
            map.Save(tempFile);

            using (ZipFileContainer file = new ZipFileContainer(tempFile))
            {
                Assert.IsTrue(file.ContainsFile("AstralManifest.xml"));
                Assert.IsTrue(file.ContainsFile(string.Format("images/{0}", redtiles.TileID)));
            }

            TestUtility.TryDelete(tempFile);
        }

        [TestMethod]
        public void MapSaveReferences()
        {
            Map library = new Map();
            Map map1 = new Map();

            map1.AddReference(library);

            TileFactory redtiles = new TileFactory(TestUtility.RedImage, "red", Borders.Empty, 1, 1);
            library.AddTileFactory(redtiles);

            TileFactory mustiles = new TileFactory(TestUtility.MustardImage, "mustard", Borders.Empty, 1, 1);
            library.AddTileFactory(mustiles);

            Tile tred = redtiles.CreateTile();
            Tile tmus = mustiles.CreateTile();

            map1.AddTile(tred);
            map1.AddTile(tmus);

            string tempLibrary = Path.GetTempFileName();
            string tempFile = Path.GetTempFileName();

            library.Save(tempLibrary);
            map1.Save(tempFile); // Should just have a reference to library and include no images

            using (ZipFileContainer file = new ZipFileContainer(tempFile))
            {
                Assert.IsTrue(file.ContainsFile("AstralManifest.xml"));
                Assert.IsFalse(file.ContainsFile("images/" + redtiles.TileID));
                Assert.IsFalse(file.ContainsFile("images/" + mustiles.TileID));
            }

            using (ZipFileContainer file = new ZipFileContainer(tempLibrary))
            {
                Assert.IsTrue(file.ContainsFile("AstralManifest.xml"));
                Assert.IsTrue(file.ContainsFile("images/" + redtiles.TileID));
                Assert.IsTrue(file.ContainsFile("images/" + mustiles.TileID));
            }

            TestUtility.TryDelete(tempFile);
            TestUtility.TryDelete(tempLibrary);
        }

        [TestMethod]
        public void MapSaveStandalone()
        {
            Map library = new Map();
            Map map1 = new Map();

            map1.AddReference(library);

            TileFactory redtiles = new TileFactory(TestUtility.RedImage, "red", Borders.Empty, 1, 1);
            library.AddTileFactory(redtiles);

            TileFactory mustiles = new TileFactory(TestUtility.MustardImage, "mustard", Borders.Empty, 1, 1);
            library.AddTileFactory(mustiles);

            Tile tred = redtiles.CreateTile();
            Tile tmus = mustiles.CreateTile();

            map1.AddTile(tred);
            map1.AddTile(tmus);

            string tempLibrary = Path.GetTempFileName();
            string tempFile = Path.GetTempFileName();

            library.Save(tempLibrary);
            map1.SaveStandalone(tempFile); // Should just have a reference to library and include no images

            using (ZipFileContainer file = new ZipFileContainer(tempFile))
            {
                Assert.IsTrue(file.ContainsFile("AstralManifest.xml"));
                // This time the map DOES contain the images necessary for display
                Assert.IsTrue(file.ContainsFile("images/" + redtiles.TileID));
                Assert.IsTrue(file.ContainsFile("images/" + mustiles.TileID));
            }

            using (ZipFileContainer file = new ZipFileContainer(tempLibrary))
            {
                Assert.IsTrue(file.ContainsFile("AstralManifest.xml"));
                Assert.IsTrue(file.ContainsFile("images/" + redtiles.TileID));
                Assert.IsTrue(file.ContainsFile("images/" + mustiles.TileID));
            }

            TestUtility.TryDelete(tempFile);
            TestUtility.TryDelete(tempLibrary);
        }


        [TestMethod]
        public void MapLoadBasic()
        {
            Map map = new Map();
            TileFactory tf1 = new TileFactory(TestUtility.TealImage, "don't serialize me!", Borders.Empty, 1, 1);
            TileFactory tf2 = new TileFactory(TestUtility.TealImage, "lalala I'm not listening", new Borders(5), 1, 2);
            var tile = tf1.CreateTile();
            var tile2 = tf2.CreateTile();

            map.AddTileFactory(tf1);
            map.AddTileFactory(tf2);
            map.AddTile(tile);
            map.AddTile(tile2);

            string filename = Path.GetTempFileName();
            try
            {
                map.Save(filename);

                // load a new copy of the map we just saved
                Map map2 = Map.LoadFromFile(filename);
                Map map3 = Map.LoadFromFile(filename);
                Assert.IsTrue(object.ReferenceEquals(map2, map3)); // Map cache should handle the second load

                Assert.IsTrue(map2.TileFactories.Contains(tf1));
                Assert.IsTrue(map3.TileFactories.Contains(tf2));
                Assert.IsTrue(map2.Tiles.Contains(tile));
                Assert.IsTrue(map2.Tiles.Contains(tile2));

            }
            finally
            {
                TestUtility.TryDelete(filename);
            }
        }


        [TestMethod]
        public void RemoveTile()
        {
            Map map1 = new Map();
            TileFactory tf1 = new TileFactory(TestUtility.TealImage, "don't serialize me!", Borders.Empty, 1, 1);
            TileFactory tf2 = new TileFactory(TestUtility.TealImage, "lalala I'm not listening", new Borders(5), 1, 2);
            var tile = tf1.CreateTile();
            var tile2 = tf2.CreateTile();

            // Just tossing in an extra check b/c I almost forgot to add the file factory
            bool threw = false;
            try
            {
                map1.AddTile(tile);
            }
            catch
            {
                threw = true;
            }
            Assert.IsTrue(threw);

            map1.AddTileFactory(tf1);
            map1.AddTileFactory(tf2);
            map1.AddTile(tile);
            map1.AddTile(tile2);

            map1.RemoveTile(tile);

            string tempFile = Path.GetTempFileName();
            map1.SaveStandalone(tempFile);

            using (ZipFileContainer file = new ZipFileContainer(tempFile))
            {
                Assert.IsTrue(file.ContainsFile("AstralManifest.xml"));
                Assert.IsTrue(file.ContainsFile(string.Format("images/{0}", tf2.TileID)));
                Assert.IsFalse(file.ContainsFile(string.Format("images/{0}", tf1.TileID)));
            }

            TestUtility.TryDelete(tempFile);
        }

        [TestMethod]
        public void SaveNoPrune()
        {
            Map map1 = new Map();
            TileFactory tf1 = new TileFactory(TestUtility.TealImage, "don't serialize me!", Borders.Empty, 1, 1);
            TileFactory tf2 = new TileFactory(TestUtility.TealImage, "lalala I'm not listening", new Borders(5), 1, 2);
            var tile = tf1.CreateTile();
            var tile2 = tf2.CreateTile();

            // Just tossing in an extra check b/c I almost forgot to add the file factory
            bool threw = false;
            try
            {
                map1.AddTile(tile);
            }
            catch
            {
                threw = true;
            }
            Assert.IsTrue(threw);

            map1.AddTileFactory(tf1);
            map1.AddTileFactory(tf2);

            string tempFile = Path.GetTempFileName();
            map1.SaveStandalone(tempFile, false);

            using (ZipFileContainer file = new ZipFileContainer(tempFile))
            {
                Assert.IsTrue(file.ContainsFile("AstralManifest.xml"));
                Assert.IsTrue(file.ContainsFile(string.Format("images/{0}", tf2.TileID)));
                Assert.IsTrue(file.ContainsFile(string.Format("images/{0}", tf1.TileID)));
            }

            TestUtility.TryDelete(tempFile);
        }

        // [TestMethod]
        public void RemoveTileFactorySimple()
        {
            Map map1 = new Map();
            throw new NotImplementedException();
        }

        // [TestMethod]
        public void RemoveTileFactoryFromReference()
        {
            Map library = new Map();
            Map map1 = new Map();
            throw new NotImplementedException();
        }


    }


}
