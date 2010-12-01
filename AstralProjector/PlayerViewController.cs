using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Astral.Plane;
using System.Windows.Threading;
using System.Windows.Media.Animation;

namespace Astral.Projector
{
    // todo: extract interface and make this remoteable
    public class PlayerViewController
    {
        PlayerView _pv = new PlayerView();
        long _x, _y;
        long _mx, _my;

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
                _pv.MapView.SetMapPosition(_x + _mx, _y + _my);
            }
        }

        public bool ShowFogOfWar
        {
            get
            {
                return _pv.Fog.Visibility == Visibility.Visible;
            }
            set
            {
                _pv.Fog.Visibility = value ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public void ResetFog()
        {
            _pv.Fog.Reset();
        }

        public void SetLayerVisibility(int layer, bool visibility)
        {
            _pv.MapView.LayerMap[layer] = visibility;
            _pv.MapView.InvalidateVisual();
        }

        public void LoadMap(string mapPath)
        {
            Map map = Map.LoadFromFile(mapPath, false);
            _mx = 0;
            _my = 0;
            _x = 0;
            _y = 0;
            
            _pv.MapView.SetMap(map);
            _pv.MapView.LayerMap.SetAll(false);
            _pv.MapView.LayerMap[0] = true;
            Dispatcher.CurrentDispatcher.In(TimeSpan.FromSeconds(1), () => _pv.MapView.TileSize = 34);
        }


        public void DisplayEffect(string effect)
        {
            Storyboard sb = _pv.Resources[effect] as Storyboard;
            _pv.BeginStoryboard(sb);
        }

        public void UpdateMapPosition(long x, long y)
        {
            _x = x;
            _y = y;
            _pv.MapView.SetMapPosition(_x + _mx, _y + _my);
        }

        public void ManualAdjust(bool horizontal, int offset)
        {
            if (horizontal)
            {
                _mx += offset * _pv.MapView.TileSize;
            }
            else
            {
                _my += offset * _pv.MapView.TileSize;
            }

            _pv.MapView.SetMapPosition(_x + _mx, _y + _my);
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

        internal void ShowImage(string filename)
        {
            _pv.ShowImage(filename);
        }

        internal void HideImage()
        {
            _pv.HideImage();
        }
    }
}
