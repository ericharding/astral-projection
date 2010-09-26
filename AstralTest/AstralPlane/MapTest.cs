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
            Rect borders = new Rect(0, 0, 0, 0);

            TileFactory redtiles = new TileFactory(TestUtility.RedImage, "red", borders, 1, 1);
            map.AddTileFactory(redtiles);

            TileFactory tealtiles = new TileFactory(TestUtility.TealImage, "teal", borders, 2, 2);
            map.AddTileFactory(tealtiles);

            TileFactory mustardTiles = new TileFactory(TestUtility.MustardImage, "mustard", borders, 1, 1);
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

            Rect borders = new Rect(0, 0, 0, 0);

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

            Rect borders = new Rect(0, 0, 0, 0);

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
        public void MapSaveBasic()
        {
            Map map = new Map();
            Rect borders = new Rect(0, 0, 0, 0);

            TileFactory redtiles = new TileFactory(TestUtility.RedImage, "red", borders, 1, 1);
            map.AddTileFactory(redtiles);

            string tempFile = Path.GetTempFileName();
            map.Save(false, tempFile);

            using (ZipFileContainer file = new ZipFileContainer(tempFile))
            {
                Assert.IsTrue(file.ContainsFile("AstralManifest.xaml"));
                Assert.IsTrue(file.ContainsFile(string.Format("images/{0}", redtiles.TileID)));
            }
        }

        [TestMethod]
        public void MapSaveReferences()
        {
            throw new NotImplementedException();
            // Check the references table
        }

        [TestMethod]
        public void MapSaveStandalone()
        {
            throw new NotImplementedException();
            // Save a tile included in a reference
        }


        // [TestMethod]
        public void MapLoadBasic()
        {

            //string filename = Path.GetTempFileName();
            //try
            //{
            //    m.Save(true, filename);

            //    // Load a new copy of the map we just saved
            //    Map m2 = new Map(filename);

            //    Assert.IsTrue(m2.TileFactories.Contains(redtiles));

            //}
            //finally
            //{
            //    if (File.Exists(filename))
            //    {
            //        File.Delete(filename);
            //    }
            //}

        }



    }


}
