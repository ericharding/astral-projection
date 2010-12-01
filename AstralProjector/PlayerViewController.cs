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
        long _x, _y;

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
                _pv.MapView.SetMapPosition(_x, _y);
            }
        }

        public void SetLayerVisibility(int layer, bool visibility)
        {
            _pv.MapView.LayerMap[layer] = visibility;
            _pv.MapView.InvalidateVisual();
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
            _x = x;
            _y = y;
        }

        //
        // Fog of War methods
        //

        // todo: handle different zoom levels
        public void UpdateFogAt(double x, double y, int size, bool clear)
        {
            _pv.Fog.ChangeFog(x, y, clear);
        }

        //
        // 
        //
    }
}
