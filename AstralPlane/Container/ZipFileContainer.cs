using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Packaging;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;

namespace Astral.Plane.Container
{
    class ZipFileContainer : IContainer
    {
        string _packageName;
        ZipPackage _package;

        public ZipFileContainer(string packageName)
        {
            Log.log("Open Zip {0}", packageName);
            _package = (ZipPackage)Package.Open(packageName);
            _packageName = packageName;
        }

        public bool ContainsFile(string filename)
        {
            return _package.PartExists(PackUriHelper.CreatePartUri(new Uri(filename, UriKind.Relative)));
        }

        public System.IO.Stream GetFileStream(string filename, bool create = true)
        {
            Uri partUri = PackUriHelper.CreatePartUri(new Uri(filename, UriKind.Relative));
            if (_package.PartExists(partUri))
            {
                var packagePart = _package.GetPart(partUri);
                return packagePart.GetStream();
            }
            else if (create)
            {
                var packagePart = _package.CreatePart(partUri, GuessMimeType(filename));
                return packagePart.GetStream();
            }
            else
            {
                throw new FileNotFoundException(partUri.ToString());
            }
        }

        public void Dispose()
        {
            _package.Close();
            Log.log("Close Zip: {0}", _packageName);
        }

        private string GuessMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = Path.GetExtension(fileName).ToLower();
            RegistryKey regKey = Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }

    }
}
