﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.IO;
using System.IO.Packaging;
using System.Windows;

namespace AstralTest.Utility
{
    class TestUtility
    {
        [ThreadStatic]
        private static BitmapSource _red = null;
        [ThreadStatic]
        private static BitmapSource _mustard = null;
        [ThreadStatic]
        private static BitmapSource _teal = null;

        public static BitmapSource RedImage
        {
            get
            {
                if (_red == null)
                {
                    _red = LoadImage("red.png");
                    _red.Freeze();
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
                    _teal.Freeze();
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
                    _mustard.Freeze();
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


        public static void TryDelete(string path)
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


        public static bool TestForThrow(Action action)
        {
            try
            {
                action();
            }
            catch
            {
                return true;
            }
            return false;
        }


        // Doesn't work - hmm... who has the reference
        public static Func<bool> WaitForGC(params object[] objs)
        {
            return () =>
                {
                    List<WeakReference> refs = new List<WeakReference>(objs.Select(o => new WeakReference(o)));
                    for (int x = 0; x < objs.Length; x++) objs[x] = null;
                    int failSafe = 100;
                    while (refs.Any(w => w.IsAlive) && failSafe > 0)
                    {
                        GC.Collect();
                        failSafe--;
                        System.Threading.Thread.Sleep(100);
                    }

                    return failSafe > 0;
                };
        }
    }

}
