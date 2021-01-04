using Jeffaries.CoverArt.Properties;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Jeffaries.CoverArt
{
    public class Plugin : MediaBrowser.Common.Plugins.BasePlugin<PluginConfiguration>, IImageEnhancer
    {
        ILogger logger;
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, ILogger logger) : base(applicationPaths, xmlSerializer)
        {
            this.logger = logger;
            //logger.Info("Jeff's CoverArt : C'est parti!");
        }
        public override Guid Id => Guid.Parse("89729EB4-4110-4FB8-BD78-9FD20C05BB51");
        public override string Name { get { return "Jeff's CoverArt"; } }

        public MetadataProviderPriority Priority => MetadataProviderPriority.First;

        public Task EnhanceImageAsync(BaseItem item, string inputFile, string outputFile, ImageType imageType, int imageIndex)
        {
            return Task.Run(() =>
            {
                //logger.Info("EnhanceImageAsync : Size of {0} : {1}x{2}", inputFile, item.Width, item.Height);
                if (item.MediaType == MediaType.Video && imageType == ImageType.Primary && 5500 > item.Width && item.Width > 2500)
                {
                    var audios = item.GetMediaStreams().FindAll(o => o.Type == MediaStreamType.Audio);
                    logger.Info("Audios for {0} : {1}", String.Join(", ", item.Name, audios.Select(o => o.Language)));

                    //logger.Info("EnhanceImageAsync : Converting {0} to {1}", inputFile, outputFile);
                    SKBitmap bitmap = SKBitmap.Decode(inputFile);
                    //ImageSize newSize = GetEnhancedImageSize(item, imageType, imageIndex, new ImageSize(bitmap.Width, bitmap.Height));

                    var margin = (int)(bitmap.Height / 50f);

                    //var toBitmap = bitmap.Resize(new SKSizeI((int)Math.Round(newSize.Width), (int)Math.Round(newSize.Height)), SKFilterQuality.High);
                    var canvas = new SKCanvas(bitmap);
                    var bmp = SKBitmap.Decode(Resources._4KHDR);
                    SKSizeI size = new SKSizeI();
                    size.Height = (int)((double)bitmap.Height / 8f);
                    size.Width = (int)((double)bmp.Width * ((double)size.Height / (double)bmp.Height));
                    var smallIcon = bmp.Resize(size, SKFilterQuality.High);
                    //logger.Info("EnhanceImageAsync : Icon -> {0} x {1}", smallIcon.Width, smallIcon.Height);
                    canvas.DrawImage(SKImage.FromBitmap(smallIcon), bitmap.Width - smallIcon.Width - margin, bitmap.Height - smallIcon.Height - margin);
                    canvas.Flush();

                    var image = SKImage.FromBitmap(bitmap);
                    var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);

                    using (var stream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                        data.SaveTo(stream);

                    data.Dispose();
                    image.Dispose();
                    canvas.Dispose();
                    bitmap.Dispose();
                    //logger.Info("EnhanceImageAsync : Convertion {0} to {1} done!", inputFile, outputFile);
                }
                else
                {
                    System.IO.File.Copy(inputFile, outputFile, true);
                }

            });
        }

        public string GetConfigurationCacheKey(BaseItem item, ImageType imageType)
        {
            return item.Id.ToString();
        }

        public EnhancedImageInfo GetEnhancedImageInfo(BaseItem item, string inputFile, ImageType imageType, int imageIndex)
        {
            //logger.Info("GetEnhancedImageInfo: {0}", inputFile);
            return new EnhancedImageInfo() { RequiresTransparency = false };
        }

        public ImageSize GetEnhancedImageSize(BaseItem item, ImageType imageType, int imageIndex, ImageSize originalImageSize)
        {
            //logger.Info("GetEnhancedImageSize");
            double resizeFactor = 1;
            //if (item.MediaType== MediaType.Video && imageType == ImageType.Primary)
            //{
            //    if(originalImageSize.Height> originalImageSize.Width) 
            //        resizeFactor = (double)(1080f / (double)originalImageSize.Height);
            //    else
            //        resizeFactor = (double)(1240 / (double)originalImageSize.Width);
            //}
            return new ImageSize(originalImageSize.Width * resizeFactor, originalImageSize.Height * resizeFactor);
        }

        public bool Supports(BaseItem item, ImageType imageType)
        {
            return true;
        }
    }
}
