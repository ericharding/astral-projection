using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using Astral.Plane.Container;
using System.Xml;
using Astral.Plane.Utility;
using System.Diagnostics;
using System.Threading;

namespace Astral.Plane
{
    /*
     * Responsible for loading, saving and holding map tile data
     */
    public class Map
    {
        #region Strings
        private const int MAP_VERSION = 2;

        private const string MANIFEST_NAME = "AstralManifest.xml";
        private const string TILEFACTORY_COLLECTION = "TileTypes";
        internal const string TILEFACTORY_NODE = "TileType";
        private const string REFERENCE_COLLECTION = "References";
        private const string REFERENCE_NODE = "Reference";
        private const string TILE_COLLECTION = "Tiles";
        internal const string TILE_NODE = "Tile";
        internal const string MAP_NOTES = "Notes";
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an empty Map
        /// </summary>
        public Map()
        {
            // empty map!
        }

        // Initializies an empty map
        public Map(int tileSizeX, int tileSizeY)
        {
            this.TileSizeX = tileSizeX;
            this.TileSizeY = tileSizeY;
        }

        /// <summary>
        /// Loads map data from the specified file
        /// </summary>
        /// <param name="fileName">A file conforming to the AstralMap spec</param>
        /// 
        public static Map LoadFromFile(string filename)
        {
            Map ret = LoadFromFile(filename, true);
            ret.RevertToLastSave();
            return ret;
        }

        public static Map LoadFromFile(string filename, bool allowCache)
        {
            if (Path.IsPathRooted(filename))
                Environment.CurrentDirectory = Path.GetDirectoryName(filename);

            string fullpath = Path.GetFullPath(filename);
            WeakReference<Map> mapRef = null;

            // if the old map is known and the reference is still alive return that
            if (allowCache && Map.TheMapCache.TryGetValue(fullpath, out mapRef) && mapRef.IsAlive)
            {
                return mapRef.Target;
            }

            Map newMap = new Map(fullpath);
            Map.TheMapCache[fullpath] = new WeakReference<Map>(newMap);

            return newMap;
        }

        private Map(string fileName)
        {
            _fileName = fileName;
            lock (_fileLock)
            {
                using (IContainer container = new ZipFileContainer(fileName))
                {
                    Load(container);
                }
            }
            _isDirty = false;
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
            if (!_references.Contains(includedMap))
            {
                _references.Add(includedMap);
            }
        }

        public void AddTileFactory(TileFactory tf)
        {
            if (tf.Map == null && tf.Image == null) throw new InvalidOperationException("Invalid tile factory.");
            // NOTE: this may *cause* image load.  (Can't delay load an image if we're switching maps b/c we lose the source location)
            if (tf.Map != null && tf.Image == null) throw new InvalidOperationException("Image must be loaded to add to a map");

            tf.Map = this;
            _tileFactories.Add(tf);
            _isDirty = true;
        }

        /// <summary>
        /// This is VERY dangerous.  Removing a tile factory invalidates all tiles which reference it.
        /// This means that a saved map referencing this map could become invalid and throw at load time when it can't find a removed tile.
        /// </summary>
        public void RemoveTileFactory(TileFactory tf)
        {
            _isDirty = true;
            if (tf.RefCount > 0) throw new InvalidOperationException("Tile factory is referenced by a loaded map and cannot be removed at this time.");
            this._tileFactories.Remove(tf);
        }

        // todo: Include a way for the application to resolve the unresolved factory
        public event Action<string> UnresolvedTileFactory;

        public void AddTile(Tile tile)
        {
            if (this.FindTileFactory(tile.Factory) == null)
            {
                throw new InvalidOperationException("This map does not know about the specified type of tile.  Did you forget to add a map reference?");
            }

            // Ok, you may pass
            _tiles.Add(tile);
            tile.Map = this;
            tile.Factory.RefCount++;
            _isDirty = true;
        }

        public void RemoveTile(Tile tile)
        {
            _tiles.Remove(tile);
            tile.Factory.RefCount--;
            _isDirty = true;
        }

        public ReadOnlyObservableCollection<TileFactory> TileFactories
        {
            get
            {
                return new ReadOnlyObservableCollection<TileFactory>(_tileFactories);
            }
        }

        public IEnumerable<Tile> Tiles
        {
            get
            {
                return _tiles.AsReadOnly();
            }
        }

        public IList<Map> References
        {
            get
            {
                return _references.AsReadOnly();
            }
        }

        private int _tileSizeX;
        public int TileSizeX
        {
            get { return _tileSizeX; }
            set { _tileSizeX = value; _isDirty = true; }
        }

        private int _tileSizeY;
        public int TileSizeY
        {
            get { return _tileSizeY; }
            set { _tileSizeY = value; _isDirty = true; }
        }

        public string FileName
        {
            get
            {
                return _fileName;
            }
        }

        private string _notes;
        public string Notes
        {
            get { return _notes; }
            set { _notes = value; _isDirty = true; }
        }

        /// <summary>
        /// Count of layers used in this map. (Computed on demand)
        /// </summary>
        public int Layers
        {
            get
            {
                return (int)_tiles.Max(t => t.Layer);
            }
        }

        internal bool IsDirty { get { return _isDirty; } set { _isDirty = value; } }

        /// <summary>
        /// Save the Map
        /// </summary>
        /// <param name="standalone"></param>
        public void Save()
        {
            if (!_isDirty && File.Exists(_fileName)) /*noop*/ return;
            if (string.IsNullOrEmpty(_fileName)) throw new FileNotFoundException("No filename");
            this.Save(_fileName);
            _isDirty = false;
        }

        /// <summary>
        /// Save the map to a file.
        /// </summary>
        public void Save(string filename)
        {
            SafeSave(false, false, filename);

            // If we used to have a different name clear the reference by that name in the cache
            if (!string.IsNullOrEmpty(_fileName))
            {
                Map.TheMapCache.Remove(_fileName);
            }
             
            _fileName = filename;
            _isDirty = false;
            // Add to the cache
            Map.TheMapCache[Path.GetFullPath(filename)] = new WeakReference<Map>(this);
        }

        /// <summary>
        /// Saves a file ready for sharing.  The resulting file will not need your local tile library to be available when loaded.
        /// If this map has references the saved map will not. (the in memory map is unchanged)
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="prune">If true any tileFactory that was explicitly added to the map will be deleted if no tiles reference it</param>
        public void ExportStandalone(string filename, bool prune = true)
        {
            SafeSave(true, prune, filename);
            // Not saving this in the Map cache because it is actually different on disk (Differnet # factories) than it is in memory.
        }

        public void RevertToLastSave()
        {
            if (string.IsNullOrEmpty(_fileName)) throw new InvalidOperationException("Cannot revert to file.  This map has not been saved or loaded since it was created.");

            // no-op
            if (!_isDirty) return;

            lock (_fileLock)
            {
                using (IContainer container = new ZipFileContainer(_fileName))
                {
                    Load(container);
                }
                _isDirty = false;
            }
        }

        private void SafeSave(bool standalone, bool prune, string filename)
        {
            string tempFile = Path.GetTempFileName();
            ActuallySave(standalone, prune, tempFile, filename);
            TryDelete(filename);
            File.Move(tempFile, filename);
        }

        // All the save overloads end up here.
        private void ActuallySave(bool standalone, bool prune, string filename, string realfilename)
        {
            XDocument doc = new XDocument(new XElement("AstralMap",
                new XAttribute("MapVersion", MAP_VERSION),
                new XAttribute("TileSizeX", this.TileSizeX),
                new XAttribute("TileSizeY", this.TileSizeY)));

            if (!string.IsNullOrEmpty(this.Notes))
            {
                XElement xNotes = new XElement(Map.MAP_NOTES, this.Notes);
                doc.Root.Add(xNotes);
            }

            // Serialize the references
            // If this is a standalone map it will contain all of the TileFactories for it's references
            if (!standalone)
            {
                XElement xRreferences = new XElement(Map.REFERENCE_COLLECTION);
                doc.Root.Add(xRreferences);

                foreach (Map m in _references)
                {
                    if (string.IsNullOrEmpty(m._fileName)) throw new InvalidOperationException("Cannot save a map with unsaved references");
                    m.Save(); // For consistency
                    xRreferences.Add(new XElement(Map.REFERENCE_NODE, new XAttribute("Source", MakeRelative(m._fileName, realfilename))));
                }
            }


            // Since it is "impossible" to add a tile that doesn't have a locateable TileFactory... 
            // we can "safely" write the tiles now and collect the tilefactories that we use
            List<TileFactory> usedTiles = new List<TileFactory>(); // Linear search b/c this should be pretty short.
            XElement xTiles = new XElement(Map.TILE_COLLECTION);
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

            // If we're standalone but the user doesn't want us to prune then
            // add all of the unreferenced tilefactories that were already here
            if (standalone && !prune)
            {
                foreach (TileFactory tf in _tileFactories)
                {
                    if (!usedTiles.Contains(tf))
                        usedTiles.Add(tf);
                }
            }

            // Serialize the TileFactories and the associated image bits
            // todo: this logic is a little funky... see how this plays out and maybe change it
            XElement xTypes = new XElement(TILEFACTORY_COLLECTION);
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
            lock (_fileLock)
            {
                TryDelete(filename);
                using (IContainer saveContainer = new ZipFileContainer(filename))
                {
                    XmlWriter writer = XmlWriter.Create(saveContainer.GetFileStream(MANIFEST_NAME), new XmlWriterSettings() { Indent = true });
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
            }
        }

        private string MakeRelative(string path, string basePath)
        {
            Uri baseUri = new Uri(basePath);
            Uri pathUri = new Uri(path);
            Uri relativePath = baseUri.MakeRelativeUri(pathUri);

            return relativePath.ToString();
        }

        private void Load(IContainer file)
        {
            XDocument doc = XDocument.Load(file.GetFileStream(MANIFEST_NAME, false));

            var versionAttribute = doc.Root.Attribute("MapVersion");
            // Intentionally allowing no map version to pass by and "try" to load
            if (versionAttribute != null)
            {
                int mapVersion = versionAttribute.Parse(Int32.Parse);
                if (mapVersion < Map.MAP_VERSION)
                {
                    throw new Exception("Map file format too old.");
                }
            }

            this.TileSizeX = doc.Root.Attribute("TileSizeX").Parse(Int32.Parse);
            this.TileSizeY = doc.Root.Attribute("TileSizeY").Parse(Int32.Parse);

            XElement xNotes = doc.Root.Element(Map.MAP_NOTES);
            if (xNotes != null)
            {
                this.Notes = xNotes.Value;
            }

            // Load references (recursive)
            XElement references = doc.Root.Element(Map.REFERENCE_COLLECTION);
            _references.Clear();
            if (references != null)
            {
                foreach (XElement r in references.Elements(Map.REFERENCE_NODE))
                {
                    string filename = r.Attribute("Source").Value;
                    if (File.Exists(filename))
                    {
                        // Should lazy load maps already in memory
                        Map refMap = Map.LoadFromFile(filename, true);
                        _references.Add(refMap);
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format("Unresolved reference {0}", filename));
                    }
                }
            }

            // Load TileFactories w/ lazy image load
            XElement factories = doc.Root.Element(Map.TILEFACTORY_COLLECTION);
            _tileFactories.Clear();
            if (factories != null)
            {
                foreach (XElement xfactory in factories.Elements(Map.TILEFACTORY_NODE))
                {
                    TileFactory newFactory = TileFactory.FromXML(this, xfactory);
                    // Not going through AddTileFactory because it forces the image bits to load up.
                    _tileFactories.Add(newFactory);
                }
            }

            // Load Tiles
            XElement xtiles = doc.Root.Element(Map.TILE_COLLECTION);
            _tiles.Clear();
            if (xtiles != null)
            {
                foreach (XElement xtile in xtiles.Elements(Map.TILE_NODE))
                {
                    string factoryID = xtile.Attribute("Type").Value;
                    var tileFactory = this.FindTileFactory(factoryID);
                    if (tileFactory == null)
                    {
                        // todo: The Unresolved tile factory event should be able to return a TileFactory so the load can continue
                        if (this.UnresolvedTileFactory != null)
                        {
                            this.UnresolvedTileFactory(factoryID);
                        }
                    }
                    else
                    {
                        Tile newTile = tileFactory.CreateTile();
                        newTile.LoadFromXML(xtile);
                        this.AddTile(newTile); // ensures tilefactory is valid and refcounted
                    }
                }
            }
        }

        private static void CopyStream(Stream input, Stream output)
        {
            input.Seek(0, SeekOrigin.Begin);
            byte[] buffer = new byte[4096];
            int read = 0;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
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


        private TileFactory FindTileFactory(TileFactory searchFactory)
        {
            return this.FindTileFactory(searchFactory.TileID);
        }

        private TileFactory FindTileFactory(string tileID)
        {
            // todo: Possible performance optimization would be to cache the results of this test ala "dynamic programming"
            // Note: once we implement RemoveTileFactory it is not safe to cache the results

            foreach (TileFactory tf in TileFactories)
            {
                if (tf.TileID == tileID)
                    return tf;
            }

            foreach (Map refmap in _references)
            {
                TileFactory match = refmap.FindTileFactory(tileID);
                if (match != null) return match;
            }

            return null;
        }


        private string _fileName;
        private ObservableCollection<TileFactory> _tileFactories = new ObservableCollection<TileFactory>();
        private List<Map> _references = new List<Map>();
        private List<Tile> _tiles = new List<Tile>();
        private bool _isDirty = true;
        private object _fileLock = new object();

        private static Dictionary<string, WeakReference<Map>> TheMapCache = new Dictionary<string, WeakReference<Map>>();

        #endregion


        // !! Caution !!
        // This function takes a lock on the file and does not release
        // it until the returned object is disposed!
        internal IDisposable LoadStream(string imagePath, out Stream imageStream)
        {
            if (string.IsNullOrEmpty(this._fileName)) /*Impossible!*/ throw new InvalidOperationException("State invalid.  You cannot delay load an image from a Map which has never been saved.");

            Monitor.Enter(_fileLock);
            IContainer zipContainer = new ZipFileContainer(_fileName);
            imageStream = zipContainer.GetFileStream(imagePath, false);

            return new AnonymousDisposable(() =>
                {
                    zipContainer.Dispose();
                    Monitor.Exit(_fileLock);
                });
        }
    }
}
