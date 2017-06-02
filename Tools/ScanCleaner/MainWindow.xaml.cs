using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Path = System.IO.Path;

namespace ScanCleaner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private BitmapImage _baseImage;
        private SimpleBitmap _wrapper;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog {Filter = "Jpeg|*.jpg"};
            if (ofd.ShowDialog() != true) return;
            var path = ofd.FileName;
            var fn = Path.GetFileNameWithoutExtension(path);
            var idx = int.Parse(fn.Split('_')[1]);
            OffsetBox.Text = idx%2 == 0 ? "257" : "270";
            _baseImage = new BitmapImage(new Uri(path, UriKind.Absolute));
            Reload(sender, e);
            Title = path;
            ApplySpecific(false);
        }

        private void Reload(object sender, RoutedEventArgs e)
        {
            _wrapper = new SimpleBitmap(_baseImage);
            Viewport.Source = _wrapper.BaseBitmap;
        }

        private void Apply(object sender, RoutedEventArgs e)
        {
            ApplySpecific(false);
        }

        private void ApplySpecific(bool alwaysWhite)
        {
            var offset = int.Parse(OffsetBox.Text);
            var skip = int.Parse(SkipBox.Text);
            var white = int.Parse(CleanBox.Text);
            var color = (alwaysWhite || (IsBlack.IsChecked == false)) ? Colors.White : Colors.Black;
            _wrapper.BaseBitmap.Lock();
            for (int i = offset; i < _baseImage.PixelWidth; i += (skip + white))
            {
                if (i + skip > _baseImage.PixelWidth) continue;
                for (int j = 0; j < white; j++)
                {
                    for (int k = offset + skip; k < _baseImage.PixelHeight; k++)
                    {
                        var x = i + skip + j;
                        if (x >= _baseImage.PixelWidth) continue;
                        _wrapper.SetColor(i + skip + j, k, color, false);
                    }
                }
                _wrapper.BaseBitmap.AddDirtyRect(new Int32Rect(i + skip,
                                                               0,
                                                               Math.Min(white, _baseImage.PixelWidth - (i + skip)),
                                                               _baseImage.PixelHeight));
            }
            _wrapper.BaseBitmap.Unlock();
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            ApplySpecific(true);
            var path = _baseImage.UriSource.OriginalString.Replace("sao18", "sao18Cleanup");
            var encoder = new JpegBitmapEncoder() {QualityLevel = 100};
            encoder.Frames.Add(BitmapFrame.Create(_wrapper.BaseBitmap));
            using (var writer = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite,FileShare.ReadWrite))
                encoder.Save(writer);
            ApplySpecific(false);
        }

        private void OffsetPlus(object sender, RoutedEventArgs e)
        {
            OffsetMove(sender, e, 10);
        }

        private void OffsetMinus(object sender, RoutedEventArgs e)
        {
            OffsetMove(sender, e, -10);
        }

        private void OffsetPlusSmall(object sender, RoutedEventArgs e)
        {
            OffsetMove(sender, e, 2);
        }

        private void OffsetMinusSmall(object sender, RoutedEventArgs e)
        {
            OffsetMove(sender, e, -2);
        }

        private void OffsetMove(object sender, RoutedEventArgs e, int value)
        {
            OffsetBox.Text = (int.Parse(OffsetBox.Text) + value).ToString();
            Reload(sender, e);
            ApplySpecific(false);
        }
    }
}
