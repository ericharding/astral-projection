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
        # region Common

        // Define a few temporary files to use for saving/loading and avoid duplicating initialization/cleanup code
        private Lazy<string> _tempFilename1 = new Lazy<string>(() => Path.GetTempFileName());
        private Lazy<string> _tempFilename2 = new Lazy<string>(() => Path.GetTempFileName());
        private Lazy<string> _tempFilename3 = new Lazy<string>(() => Path.GetTempFileName());
        public string TempFile1 { get { return _tempFilename1.Value; } }
        public string TempFile2 { get { return _tempFilename2.Value; } }
        public string TempFile3 { get { return _tempFilename3.Value; } }

        [TestCleanup]
        public void Cleanup()
        {
            if (_tempFilename1.IsValueCreated)
                TestUtility.TryDelete(_tempFilename1.Value);
            if (_tempFilename2.IsValueCreated)
                TestUtility.TryDelete(_tempFilename2.Value);
            if (_tempFilename3.IsValueCreated)
                TestUtility.TryDelete(_tempFilename3.Value);
        }

        #endregion


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

            map.Save(TempFile1);

            using (ZipFileContainer file = new ZipFileContainer(TempFile1))
            {
                Assert.IsTrue(file.ContainsFile("AstralManifest.xml"));
                Assert.IsTrue(file.ContainsFile(string.Format("images/{0}", redtiles.TileID)));
            }
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

            string tempLibrary = TempFile1;
            string tempFile = TempFile2;

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
        }

        [TestMethod]
        public void MapSaveBadReference()
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


            string tempFile = TempFile1;
            bool threw = false;
            try
            {
                // Can't save map1 b/c it references library which was never saved
                map1.Save(tempFile);
            }
            catch { threw = true; }

            Assert.IsTrue(threw);

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

            string tempLibrary = TempFile1;
            string tempFile = TempFile2;

            library.Save(tempLibrary);
            map1.ExportStandalone(tempFile); // Should just have a reference to library and include no images

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

        }

        [TestMethod]
        public void MapLoadFromCache()
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

            map.Save(TempFile1);

            // load a new copy of the map we just saved
            Map map2 = Map.LoadFromFile(TempFile1);

            // Since "filename" was already loaded LoadFromFile returns that reference
            Assert.IsTrue(object.ReferenceEquals(map, map2));
        }

        [TestMethod]
        public void MapInvalidateCacheUponSaveWithDifferentName()
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

            map.Save(TempFile1);
            map.Save(TempFile2);

            // load a new copy of the map we just saved
            Map map2 = Map.LoadFromFile(TempFile1);

            // Since "filename" was already loaded LoadFromFile returns that reference
            Assert.IsFalse(object.ReferenceEquals(map, map2));
        }

        [TestMethod]
        public void MapLoadBasic()
        {
            Map map = new Map();
            TileFactory tf1 = new TileFactory(TestUtility.TealImage, "don't serialize me!", Borders.Empty, 1, 1);
            TileFactory tf2 = new TileFactory(TestUtility.TealImage, "lalala I'm not listening", new Borders(5), 1, 2);
            var tile = tf1.CreateTile();
            tile.Mirror = TileMirror.Horizontal | TileMirror.Vertical;
            var tile2 = tf2.CreateTile();

            map.AddTileFactory(tf1);
            map.AddTileFactory(tf2);
            map.AddTile(tile);
            string note = "Something cool happens here.";
            tile.Note = note;
            map.AddTile(tile2);

            map.Save(TempFile1);

            File.Copy(TempFile1, TempFile2, true);

            // load a new copy of the map we just saved
            Map map2 = Map.LoadFromFile(TempFile2); // real load
            Map map3 = Map.LoadFromFile(TempFile2); // load from cache
            Assert.IsTrue(object.ReferenceEquals(map2, map3)); // Map cache should handle the second load

            Assert.IsTrue(map2.TileFactories.Contains(tf1)); // because TileFactory overrides equivalence this is not reference equals
            Assert.IsTrue(map3.TileFactories.Contains(tf2));
            Assert.IsTrue(map2.Tiles.Count() == 2);
            Assert.IsTrue(map2.Tiles.First().Note == note || map2.Tiles.Last().Note == note);           
        }

        [TestMethod]
        public void TestMapRefCache()
        {
            // Create a new library and save it
            Map lib = new Map();
            TileFactory tf1 = new TileFactory(TestUtility.RedImage, "red", Borders.Empty, 1, 2);
            TileFactory tf2 = new TileFactory(TestUtility.MustardImage, "notred", new Borders(-2), 3, 4);
            lib.AddTileFactory(tf1);
            lib.AddTileFactory(tf2);

            // save library - adds to cache by filename
            lib.Save(TempFile1);

            // make new map - reference library
            Map map1 = new Map();
            map1.AddReference(lib);
            map1.AddTile(tf1.CreateTile());
            map1.AddTile(tf2.CreateTile());

            // save map
            map1.Save(TempFile2);
            
            // load map - assert references contain library by reference
            File.Copy(TempFile2, TempFile3, true);
            Map map2 = Map.LoadFromFile(TempFile3);
            Assert.IsFalse(object.ReferenceEquals(map1, map2)); // assert map is not psychic

            Assert.IsTrue(map1.Tiles.Count() == map2.Tiles.Count());
            Assert.IsTrue(map1.References.Count == 2);
            Assert.IsTrue(map2.References.Count == 2);
            Assert.IsTrue(object.ReferenceEquals(map1.References[1], map2.References[1]));
        }

        [TestMethod]
        public void TestMapRecursiveRefLoader()
        {
            Map lib = new Map();
            TileFactory tf1 = new TileFactory(TestUtility.RedImage, "red", Borders.Empty, 1, 2);
            TileFactory tf2 = new TileFactory(TestUtility.MustardImage, "notred", new Borders(-2), 3, 4);
            lib.AddTileFactory(tf1);
            lib.AddTileFactory(tf2);
            lib.Save(TempFile1);

            Map map = new Map();
            map.AddReference(lib);
            map.AddTile(tf1.CreateTile());
            map.AddTile(tf2.CreateTile());
            map.Save(TempFile2);

            WeakReference wLib = new WeakReference(lib);
            WeakReference wmap = new WeakReference(map);
            lib = null;
            map = null;
            int failsafe = 100;
            while (wLib.IsAlive && wmap.IsAlive && failsafe > 0)
            {
                GC.Collect();
                System.Threading.Thread.Sleep(100);
                failsafe--;
            }

            Assert.IsTrue(failsafe > 0, "We're leaking memory, captain, and the GC just can't take any more!");

            map = Map.LoadFromFile(TempFile2); // Loads both map and lib
            Assert.IsTrue(map.References.Count > 1);
            lib = Map.LoadFromFile(TempFile1);
            Assert.IsTrue(map.References.Contains(lib));
        }

        [TestMethod]
        public void TestImageRoundTrip()
        {
            Map map = new Map();
            TileFactory tf1 = new TileFactory(TestUtility.RedImage, "red", Borders.Empty, 1, 2);
            long originalLength = tf1.GetImageStream().Length;
            map.AddTileFactory(tf1);

            map.Save(TempFile1);

            // fool the cache
            File.Copy(TempFile1, TempFile2, true);
            
            Map map2 = Map.LoadFromFile(TempFile2);
            
            Assert.IsTrue(map2.TileFactories.Count == 1);

            Assert.AreEqual(originalLength, map2.TileFactories[0].GetImageStream().Length);
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
            Assert.IsTrue(TestUtility.TestForThrow(() => map1.AddTile(tile)));

            map1.AddTileFactory(tf1);
            map1.AddTileFactory(tf2);
            map1.AddTile(tile);
            map1.AddTile(tile2);

            map1.RemoveTile(tile);

            string tempFile = TempFile1;
            map1.ExportStandalone(tempFile);

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
            bool threw = TestUtility.TestForThrow(() => map1.AddTile(tile));
            Assert.IsTrue(threw);

            map1.AddTileFactory(tf1);
            map1.AddTileFactory(tf2);

            string tempFile = TempFile1;
            map1.ExportStandalone(tempFile, false);

            using (ZipFileContainer file = new ZipFileContainer(tempFile))
            {
                Assert.IsTrue(file.ContainsFile("AstralManifest.xml"));
                Assert.IsTrue(file.ContainsFile(string.Format("images/{0}", tf2.TileID)));
                Assert.IsTrue(file.ContainsFile(string.Format("images/{0}", tf1.TileID)));
            }

            TestUtility.TryDelete(tempFile);
        }

        [TestMethod]
        public void RevertToSaved()
        {
            Map map1 = new Map();
            TileFactory tf1 = new TileFactory(TestUtility.TealImage, "Teal is for real!", Borders.Empty, 1, 1);
            TileFactory tf2 = new TileFactory(TestUtility.TealImage, "lalala I'm not listening", new Borders(5), 1, 2);
            var tile = tf1.CreateTile();
            var tile2 = tf2.CreateTile();

            map1.AddTileFactory(tf1);
            map1.AddTileFactory(tf2);
            map1.AddTile(tile);
            map1.AddTile(tile2);

            map1.Save(this.TempFile1);

            Assert.IsTrue(map1.TileFactories.Count == 2);
            Assert.IsTrue(map1.Tiles.Count() == 2);

            TileFactory tf3 = new TileFactory(TestUtility.RedImage, "Red Image!", Borders.Empty, 1, 2);
            var tile3 = tf3.CreateTile();
            var tile4 = tf1.CreateTile();
            var tile5 = tf2.CreateTile();

            map1.AddTileFactory(tf3);
            map1.AddTile(tile3);
            map1.AddTile(tile4);
            map1.AddTile(tile5);

            Assert.IsTrue(map1.TileFactories.Count == 3);
            Assert.IsTrue(map1.Tiles.Count() == 5);

            map1.RevertToLastSave();
            Assert.IsTrue(map1.TileFactories.Count == 2);
            Assert.IsTrue(map1.Tiles.Count() == 2);
        }

        [TestMethod]
        public void TileDirtyTest()
        {
            Map map1 = new Map();
            TileFactory tf1 = new TileFactory(TestUtility.TealImage, "Teal is for real!", Borders.Empty, 1, 1);
            TileFactory tf2 = new TileFactory(TestUtility.TealImage, "lalala I'm not listening", new Borders(5), 1, 2);
            var tile = tf1.CreateTile();
            var tile2 = tf2.CreateTile();

            map1.AddTileFactory(tf1);
            map1.AddTileFactory(tf2);
            map1.AddTile(tile);
            map1.AddTile(tile2);

            Assert.IsTrue(map1.IsDirty);
            map1.Save(this.TempFile1);

            Assert.IsFalse(map1.IsDirty);

            tile.Layer = 3;
            Assert.IsTrue(map1.IsDirty);
        }

        [TestMethod]
        public void TestTileProperties()
        {
            Map map1 = new Map();
            TileFactory tf1 = new TileFactory(TestUtility.TealImage, "Teal is for real!", Borders.Empty, 1, 1);
            TileFactory tf2 = new TileFactory(TestUtility.TealImage, "lalala I'm not listening", new Borders(5), 1, 2);
            var tile = tf1.CreateTile();
            var tile2 = tf2.CreateTile();

            map1.AddTileFactory(tf1);
            map1.AddTileFactory(tf2);
            map1.AddTile(tile);
            map1.AddTile(tile2);

            tile.Properties["iscool"] = "sure is";
            tile2.Properties["Marker"] = "3";

            map1.Save(this.TempFile1);

            File.Copy(TempFile1, TempFile2, true);
            
            Map map2 = Map.LoadFromFile(TempFile2);

            Assert.IsTrue(map2.Tiles.Any(t => t.Properties.ContainsKey("iscool") && t.Properties["iscool"] == "sure is"));
            Assert.IsTrue(map2.Tiles.Any(t => t.Properties.ContainsKey("Marker") && t.Properties["Marker"] == "3"));
        }

        [TestMethod]
        public void TestCounter()
        {
            Map map1 = new Map();
            map1.MarkerCounter++;

            map1.Save(TempFile1);
            File.Copy(TempFile1, TempFile2, true);

            Map map2 = Map.LoadFromFile(TempFile2);

            Assert.IsTrue(map2.MarkerCounter == 1);
        }

        [TestMethod]
        public void TestLayerInterop()
        {
            Map map1 = new Map();
            TileFactory tf1 = new TileFactory(TestUtility.TealImage, "Teal is for real!", Borders.Empty, 1, 1);
            map1.AddTileFactory(tf1);

            Tile tile1 = tf1.CreateTile();
            tile1.Layer = 3;
            map1.AddTile(tile1);

            Tile tile2 = tf1.CreateTile();
            tile2.Layer = 4;
            tile2.Tag = "Pit Trap";
            map1.AddTile(tile2);

            map1.Save(TempFile1);
            File.Copy(TempFile1, TempFile2, true);

            Map map2 = Map.LoadFromFile(TempFile2);

            Tile tile3 = map2.Tiles.Where(t => t.Layer == 3).First();
            Tile tile4 = map2.Tiles.Where(t => t.Layer == 4).First();
            Assert.IsTrue(tile3.Tag == "Layer3");
            Assert.IsTrue(tile4.Tag == "Pit Trap");
        }

        [TestMethod]
        public void TestMarkerFactory()
        {
            var mtf = Map.RootMap.TileFactories.First();

            Map m1 = new Map();

            Tile newTile = mtf.CreateTile();
            newTile.Location = new Point(100, 100);
            m1.AddTile(newTile);
            m1.Save(TempFile1);

            File.Copy(TempFile1, TempFile2, true);
            Map m2 = Map.LoadFromFile(TempFile2);

            Assert.IsTrue(m2.Tiles.Count() == 1);
            Assert.IsTrue(m2.TileFactories.Count == 0);

        }

        [TestMethod]
        public void TestLoadTwoMaps()
        {
            // todo: this test doesn't actually load the stream - too lazy.
            Map map1 = new Map();
            TileFactory tf1 = new TileFactory(TestUtility.TealImage, "Teal is for real!", Borders.Empty, 1, 1);
            var tile = tf1.CreateTile();
            map1.AddTileFactory(tf1);
            map1.AddTile(tile);

            map1.Save(this.TempFile1);
            map1.Dispose();
            File.Copy(TempFile1, TempFile2, true);

            using (Map map2 = Map.LoadFromFile(this.TempFile2, false))
            {
                var width = map2.Tiles.First().Image.Width;
                using (Map map3 = Map.LoadFromFile(this.TempFile2, false))
                {
                    Assert.IsTrue(map3.Tiles.Count() == 1);
                    Assert.IsTrue(width == map3.Tiles.First().Image.Width);
                }
            }
        }

    }


}
