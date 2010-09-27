using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using Astral.Plane.Container;
using System.Xml;

namespace Astral.Plane
{
    /*
     * Responsible for loading, saving and holding map tile data
     */
    public class Map
    {

        #region Constructors

        /// <summary>
        /// Initializes an empty Map
        /// </summary>
        public Map()
        {
            // empty map!
        }

        /// <summary>
        /// Loads map data from the specified file
        /// </summary>
        /// <param name="fileName">A file conforming to the AstralMap spec</param>
        public Map(string fileName)
        {
            _fileName = fileName;
            LoadFromFile(fileName);
        }

        #endregion

        #region Public

        /// <summary>
        /// Add a reference to another map.  
        /// This map will use the other map to resolve Tile references
        /// </summary>
        /// <param name="includedMap"></param>
        public void AddReference(Map includedMap)
        {
            _references.Add(includedMap);
        }

        public void AddTileFactory(TileFactory tf)
        {
            if (tf.Map == null && tf.Image == null) throw new InvalidOperationException("Invalid tile factory.");
            if (tf.Map != null && tf.Image == null) throw new InvalidOperationException("Image must be loaded to add to a map");

            tf.Map = this;
            _tileFactories.Add(tf);
        }

        public void AddTile(Tile tile)
        {
            if (this.FindTileFactory(tile.Factory) == null)
            {
                throw new InvalidOperationException("This map does not know about the specified type of tile.  Did you forget to add a map reference?");
            }

            // Ok, you may pass
            _tiles.Add(tile);
        }

        public IList<TileFactory> TileFactories
        {
            get
            {
                return _tileFactories.AsReadOnly();
            }
        }

        public IEnumerable<Tile> Tiles
        {
            get
            {
                return _tiles.AsReadOnly();
            }
        }

        /// <summary>
        /// Save the Map
        /// </summary>
        /// <param name="standalone"></param>
        public void Save(bool standalone)
        {
            if (string.IsNullOrEmpty(_fileName)) throw new FileNotFoundException("No filename");
            this.Save(standalone, _fileName);
        }

#if DEBUG
        public bool SaveToDirectory { get; set; }
#endif

        public void SafeSave(bool standalone, string filename)
        {
            string tempFile = Path.GetTempFileName();
            Save(standalone, tempFile);
            File.Move(tempFile, filename);
        }

        /// <summary>
        /// Save the map to a file.
        /// </summary>
        /// <param name="full">If true the resulting file will include all necessary tiles and be completely standalone</param>
        public void Save(bool standalone, string filename)
        {
            /*
             * <AstralMap ID="">
             *   <References>
             *      <Reference path="foo.astral" ID="" />
             *   </References>
             *   <TileFactories>
             *      <TileFactory ID="1234" ImageSource="images/foo.png" Border="0,1,2,3"
             *   </TileFactories>
             *   <Tiles>
             *       <Tile Factory="1234" Position="1,2" ... />
             *   </Tiles>
             * </AstralMap>
             */

            XDocument doc = new XDocument(new XElement("AstralMap"));

            // Serialize the references
            // If this is a standalone map it will contain all of the TileFactories for it's references
            if (!standalone)
            {
                XElement xRreferences = new XElement("References");
                doc.Root.Add(xRreferences);

                foreach (Map m in _references)
                {
                    xRreferences.Add(new XElement("Reference", new XAttribute("Source", m._fileName)));
                }
            }


            // Since it is "impossible" to add a tile that doesn't have a locateable TileFactory... 
            // we can "safely" write the tiles now and collect the tilefactories that we use
            List<TileFactory> usedTiles = new List<TileFactory>(); // Linear search b/c this should be pretty short.
            XElement xTiles = new XElement("Tiles");
            doc.Root.Add(xTiles);
            foreach (Tile tile in _tiles)
            {
                xTiles.Add(tile.ToXML());

                if (standalone &&
                    !usedTiles.Contains(tile.Factory))
                {
                    // For a standalone map we need to know about every referenced tile factory
                    usedTiles.Add(tile.Factory);
                }
            }

            // Serialize the TileFactories and the associated image bits
            // todo: this logic is a little funky... see how this plays out and maybe change it
            XElement xTypes = new XElement("TileTypes");
            doc.Root.Add(xTypes);

            if (standalone)
            {
                foreach (TileFactory t in usedTiles)
                {
                    xTypes.Add(t.ToXML());
                }
            }
            else
            {
                foreach (TileFactory t in _tileFactories)
                {
                    xTypes.Add(t.ToXML());
                    usedTiles.Add(t);
                }
            }

            
            // Save the xml as AstralManifest.xml
            TryDelete(filename);
            using (IContainer saveContainer = this.SaveToDirectory ? (IContainer)new FilesystemContainer(filename) : (IContainer)new ZipFileContainer(filename))
            {
                XmlWriter writer = XmlWriter.Create(saveContainer.GetFileStream("AstralManifest.xml"), new XmlWriterSettings() { Indent = true });
                doc.Save(writer);
                writer.Close();

                // Save images to a sub directory
                foreach (TileFactory t in usedTiles)
                {
                    Stream saveStream = saveContainer.GetFileStream("images/" + t.TileID);
                    Stream imageStream = t.GetImageStream();
                    CopyStream(imageStream, saveStream);
                    saveStream.Close();
                    imageStream.Close();
                }
            }

            _fileName = filename;
        }

        private static void CopyStream(Stream input, Stream output)
        {
            input.Seek(0, SeekOrigin.Begin);
            byte[] buffer = new byte[4096];
            int read = 0;
            while ( (read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                else if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch { }
        }
        

        #endregion


        #region private

        private void LoadFromFile(string fileName)
        {
            // Todo: When you get around to writing load
            // Make the Map constructor private and replace it with a public Load() which
            // keeps track of all Maps in memory.  Then we can check references against the 
            // table of loaded maps and save time/memory.
            throw new NotImplementedException();
        }



        private TileFactory FindTileFactory(TileFactory searchFactory)
        {
            // todo: Possible performance optimization would be to cache the results of this test ala "dynamic programming"
            // Once a reference is added it cannot be removed so it is safe to cache

            foreach (TileFactory tf in TileFactories)
            {
                if (tf == searchFactory)
                    return tf;
            }

            foreach(Map refmap in _references)
            {
                TileFactory match = refmap.FindTileFactory(searchFactory);
                if (match != null) return match;
            }

            return null;
        }


        string _fileName;
        List<TileFactory> _tileFactories = new List<TileFactory>();
        List<Map> _references = new List<Map>();
        List<Tile> _tiles = new List<Tile>();
        
        
        #endregion

        internal static Stream LoadStream(string _imagePath)
        {
            throw new NotImplementedException();
        }
    }
}
