using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.IO;
using System.IO.Packaging;

namespace AstralTest.Utility
{
    class TestUtility
    {

        private static BitmapSource _red = null;
        private static BitmapSource _mustard = null;
        private static BitmapSource _teal = null;

        public static BitmapSource RedImage
        {
            get
            {
                if (_red == null)
                {
                    _red = LoadImage("red.png");
                }
                return _red;
            }
        }

        public static BitmapSource TealImage
        {
            get
            {
                if (_teal == null)
                {
                    _teal = LoadImage("teal.png");
                }
                return _teal;
            }
        }

        public static BitmapSource MustardImage
        {
            get
            {
                if (_mustard == null)
                {
                    _mustard = LoadImage("mustard.png");
                }
                return _mustard;
            }
        }
        

        private static BitmapSource LoadImage(string filename)
        {
            string resourceName = "AstralTest.Images." + filename;
            Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);

            PngBitmapDecoder decoder = new PngBitmapDecoder(s, BitmapCreateOptions.None, BitmapCacheOption.None);

            return decoder.Frames[0];
        }
    }
}
