using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using System;
using System.IO;
using Image = SixLabors.ImageSharp.Image;
using Size = SixLabors.ImageSharp.Size;

namespace Game_Library.Extensions
{
    public static class ImageOptimizer 
    {
        public static void OptimizeAndSave(string sourceFilePath, string targetFilePath)
        {
            if (!File.Exists(sourceFilePath)) return;
            string extension = Path.GetExtension(sourceFilePath).ToLower();

            try
            {
                using (Image image = Image.Load(sourceFilePath))
                {
                    if (extension == ".gif")
                    {
                        var gifEncoder = new GifEncoder();
                        image.Save(targetFilePath, gifEncoder);
                    }
                    else
                    {
                        int targetWidth = 550;
                        if (image.Width > targetWidth)
                        {
                            image.Mutate(x => x.Resize(new ResizeOptions
                            {
                                Size = new Size(targetWidth, 0),
                                Mode = ResizeMode.Max
                            }));
                        }

                        if (extension == ".png")
                        {
                            image.Save(targetFilePath, new PngEncoder { CompressionLevel = PngCompressionLevel.BestCompression });
                        }
                        else
                        {
                            image.Save(targetFilePath, new JpegEncoder { Quality = 78 });
                        }
                    }

                }
            }
            catch (Exception)
            {
                File.Copy(sourceFilePath, targetFilePath, true);
            }
        }
        
    }
}
