using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Astral.Plane;
using System.IO;

namespace AstralMerge
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3) return;

            try
            {
                string output = args.Last();
                MergeMaps(output, args.TakeWhile(s => s != output));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void MergeMaps(string output, IEnumerable<string> input)
        {
            Map merged = new Map();

            foreach (string file in input)
            {
                Map temp = Map.LoadFromFile(file);
                merged.AddReference(temp);
                foreach (var tile in temp.TileFactories)
                {
                    merged.AddTile(tile.CreateTile());
                }
            }
            merged.ExportStandalone(output, false);
        }
    }
}
