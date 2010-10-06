using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace Astral.Projector
{
    class AstralScreen : Control
    {
        protected override void OnRender(DrawingContext dc)
        {
            dc.PushOpacity(0.5);
            
            base.OnRender(dc);
        }
    }


    static class DrawingContextHelper
    {
        public static void DrawHex(this DrawingContext dc)
        {

        }
    }
}
