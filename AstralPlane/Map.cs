using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            Load(fileName);
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
        }

        public void AddTileFactory(TileFactory tf)
        {
            tf.Map = this;
            _tileFactories.Add(tf);
        }

        /// <summary>
        /// Save the map to a file.
        /// </summary>
        /// <param name="full">If true the resulting file will include all necessary tiles and be completely standalone</param>
        public void Save(bool standalone, string filename)
        {
            // Serialize includes if !standalone
            // Serialize either the local TileFactories or all referenced TileFactories
            // Serialize Tiles to xml
            
        }

        #endregion


        #region private

        private void Load(string fileName)
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
        List<TileFactory> _tileFactories = new List<TileFactory>();
        List<Map> _references = new List<Map>();
        List<Tile> _tiles = new List<Tile>();
        
        
        #endregion
    }
}
