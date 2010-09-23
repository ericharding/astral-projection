using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

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
        internal Map(string fileName)
        {
            _fileName = fileName;
            LoadFromFile(fileName);
        }

        public static Map Load(string filename)
        {
            Map m = new Map(filename);
            //todo: record map's id in static table and re
            return m;
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
            tf.Map = this;
            _tileFactories.Add(tf);
        }

        public void AddTile(Tile tile)
        {
            // todo: validate that tile.Factory is in this map's reference chain

            throw new NotImplementedException();
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
        
        /// <summary>
        /// Save the map to a file.
        /// </summary>
        /// <param name="full">If true the resulting file will include all necessary tiles and be completely standalone</param>
        public void Save(bool standalone, string filename)
        {
            XDocument doc = new XDocument(new XElement("AstralMap"));

            // Serialize the references
            // If this is a standalone map it will contain all of the TileFactories for it's references
            if (!standalone)
            {
                // todo:  Load will have to load the referenced maps too
                XElement references = new XElement("References");
                doc.Root.Add(references);

                foreach (Map m in _references)
                {
                    references.Add(new XElement("Reference", new XAttribute("Source", m._fileName)));
                }
            }

            

            // todo: does tileFactory need an id except during save/load?  maybe not

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

            
            // Serialize includes if !standalone
            // Serialize either the local TileFactories or all referenced TileFactories
            // Serialize Tiles to xml
            
        }

        

        #endregion


        #region private

        private void LoadFromFile(string fileName)
        {
            throw new NotImplementedException();
        }

        public IList<TileFactory> TileFactories
        {
            get
            {
                return _tileFactories;
            }
        }

        public IEnumerable<Tile> Tiles
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion


        #region private members
        string _fileName;
        List<TileFactory> _tileFactories = new List<TileFactory>();
        List<Map> _references = new List<Map>();
        List<Tile> _tiles = new List<Tile>();
        
        
        #endregion
    }
}
