using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace SVGAnimations.Manager
{
    public class SVGAnimator
    {
        public Image Image { get; private set; }
        private SvgImageSource svgsource;

        public SVGAnimator()
        {
            Image = new Image();
            svgsource = new SvgImageSource();
            Image.Source = svgsource;
        }

        private string svgData;
        IRandomAccessStream memoryStream;
        public async Task Initialize(string urisource)
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(urisource));
            svgData = await FileIO.ReadTextAsync(file);

            memoryStream = new InMemoryRandomAccessStream();
            await Write(memoryStream,svgData);

            await Refresh();
        }

        private async Task Refresh()
        {
            memoryStream.Seek(0);
            await svgsource.SetSourceAsync(memoryStream);
        }

        private async Task Write(IRandomAccessStream stream, string content)
        {
            memoryStream.Seek(0);
            using (var dataWriter = new DataWriter(stream))
            {
                dataWriter.WriteString(content);
                await dataWriter.StoreAsync();
                await dataWriter.FlushAsync();
                dataWriter.DetachStream();
            }
        }

        public void StartAnimation()
        {
            if (svgData == null)
                return;

            AnimateSvg();
        }

        int strokewidth = 1;
        private async Task AnimateSvg()
        {
            svgData = svgData.Replace($"stroke-width:{strokewidth};", $"stroke-width:{strokewidth + 1};");
            strokewidth++;
            await Write(memoryStream, svgData);
            await Refresh();
            await Task.Delay(1000);
            AnimateSvg();
        }
    }
}
