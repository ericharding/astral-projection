using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Astral.Projector.Initiative;
using TileMap;
using System.Windows.Media;


namespace Astral.Projector
{
    /// <summary>
    /// Interaction logic for PlayerView.xaml
    /// </summary>
    public partial class PlayerView : Window
    {
        private Countdown _counter = new Countdown();
        public PlayerView()
        {
            InitializeComponent();

            _counterView.DataContext = _counter;
            _counter.CountdownCompleted += CountdownCompleted;
        }

        void CountdownCompleted()
        {
           _counterView.Visibility = Visibility.Collapsed;
        }

        internal Countdown Countdown
        {
           get { return _counter; }
        }

        public void ShowImage(string image)
        {
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.StreamSource = File.OpenRead(image);
            src.EndInit();
            _imageViewer.Source = src;

            Storyboard sb = this.Resources["ImageIn"] as Storyboard;
            this.BeginStoryboard(sb);
        }

        public void HideImage()
        {
            Storyboard sb = this.Resources["ImageOut"] as Storyboard;
            this.BeginStoryboard(sb);
        }

        public MapPane MapView { get { return _mapView; } }
        public FogOfWar Fog { get { return _fog; } }
        public UIElementCollection Effects { get { return _effectContainer.Children; } }

        public void UpdateInitiative(IEnumerable<Event> events)
        {
            _initiativeView.UpdateEvents(events);
        }


        //internal void SetRotation(int rotation)
        //{
        //   RotateTransform rt = (RotateTransform)_rotationFrame.LayoutTransform;
        //   rt.Angle = rotation;
        //}
    }
}
