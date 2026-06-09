using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace tagApiKonfigurasi.Services
{
    public static class PhotoCompressionHelper
    {
        private const int MaxDimension = 400;
        private const int JpegQuality = 75;

        public static string? ToMobileBase64(byte[] photoBytes)
        {
            if (photoBytes.Length == 0)
                return null;

            try
            {
                using var input = new MemoryStream(photoBytes);
                using var image = Image.Load(input);

                if (image.Width > MaxDimension || image.Height > MaxDimension)
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(MaxDimension, MaxDimension)
                    }));
                }

                using var output = new MemoryStream();
                image.SaveAsJpeg(output, new JpegEncoder { Quality = JpegQuality });
                return $"data:image/jpeg;base64,{Convert.ToBase64String(output.ToArray())}";
            }
            catch
            {
                return $"data:image/jpeg;base64,{Convert.ToBase64String(photoBytes)}";
            }
        }
    }
}
