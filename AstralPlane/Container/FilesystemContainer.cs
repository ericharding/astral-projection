using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Astral.Plane.Container
{
    class FilesystemContainer : IContainer
    {
        private string _basePath;


        public FilesystemContainer(string basePath)
        {
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
            _basePath = basePath;
        }

        public bool ContainsFile(string filename)
        {
            return File.Exists(_basePath + filename);
        }

        public Stream GetFileStream(string filename, bool create = true)
        {
            string fullName = _basePath + filename;

            if (File.Exists(fullName) || create)
            {
                return File.Open(fullName, FileMode.OpenOrCreate);
            }
            else
            {
                throw new FileNotFoundException(fullName);
            }
        }

        public void Dispose()
        {
            // noop
        }
    }
}
