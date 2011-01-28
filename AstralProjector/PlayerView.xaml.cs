using System.IO;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using TileMap;
using System.Windows.Controls;


namespace Astral.Projector
{
    /// <summary>
    /// Interaction logic for PlayerView.xaml
    /// </summary>
    public partial class PlayerView : Window
    {
        public PlayerView()
        {
            InitializeComponent();
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

    }
}
