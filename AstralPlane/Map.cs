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

        #region public Interface

        /// <summary>
        /// Add a reference to another map.  
        /// This map will use the other map to resolve Tile references
        /// </summary>
        /// <param name="includedMap"></param>
        public void AddReference(Map includedMap)
        {
        }

        /// <summary>
        /// Save the map to a file.
        /// </summary>
        /// <param name="full">If true the resulting file will include all necessary tiles and be completely standalone</param>
        public void Save(bool standalone, string filename)
        {

        }

        #endregion


        #region private

        private void Load(string fileName)
        {
            throw new NotImplementedException();
        }


        #endregion

    }
}
