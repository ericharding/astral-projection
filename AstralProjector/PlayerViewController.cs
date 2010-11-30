using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Astral.Plane;

namespace Astral.Projector
{
    // todo: extract interface and make this remoteable
    public class PlayerViewController
    {
        PlayerView _pv = new PlayerView();

        public PlayerViewController()
        {
            // Position the player view
        }

        public bool IsMapVisible
        {
            get
            {
                return _pv.IsVisible;
            }
            set
            {
                if (value)
                {
                    _pv.Show();

                    // TODO: don't assume there are 2 screens and the projector is on the right
                    _pv.Left = SystemParameters.PrimaryScreenWidth;
                    _pv.WindowState = WindowState.Maximized;
                }
                else
                {
                    _pv.Hide();
                }
            }
        }

        public int ZoomLevel
        {
            get
            {
                return _pv.MapView.TileSize;
            }
            set
            {
                _pv.MapView.TileSize = value;
            }
        }

        public void LoadMap(string mapPath)
        {
            Map map = Map.LoadFromFile(mapPath, false);
            _pv.MapView.SetMap(map);
            _pv.MapView.LayerMap.SetAll(false);
            _pv.MapView.LayerMap[0] = true;
        }


        public void DisplayEffect(string effect)
        {
            // yea right
        }

        public void UpdateMapPosition(long x, long y)
        {
            _pv.MapView.SetMapPosition(x, y);
        }

        //
        // Fog of War methods
        //

        // todo: handle different zoom levels
        public void UpdateFogAt(double x, double y, int size, bool clear)
        {
            //if (x < 0 || x > 1) throw new ArgumentException("x must be between 0 and 1");
            //if (y < 0 || y > 1) throw new ArgumentException("y must be between 0 and 1");

            _pv.Fog.ChangeFog(x, y, size, clear);
        }

        //
        // 
        //
    }
}
