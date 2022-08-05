using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using YoutubeExplode.Common;


namespace Youtube_Music_Downloader {
    internal class Utils {

		private static readonly string tempFolder = Path.GetTempPath();

		public static string DownloadThumbnail(Thumbnail thumbnail) {
			var extension = GetFileExtensionFromUrl(thumbnail.Url);
			var imagePath = $"{tempFolder}{Guid.NewGuid()}{extension}";

			using(var client = new WebClient())
				client.DownloadFile(thumbnail.Url, imagePath);

			if(extension.Equals(".webp")) {
				var newImagePath = imagePath.Replace("webp", "jpg");
				var startInfo = new ProcessStartInfo() {
					FileName = "./dwebp.exe",
					Arguments = String.Format("{0} -o {1}", imagePath, newImagePath),
					CreateNoWindow = true,
					UseShellExecute = false
				};

				var process = Process.Start(startInfo);
				process.WaitForExit();

				while(!process.HasExited && process.Responding)
					Thread.Sleep(100);

				return newImagePath;
			}
			return imagePath;
		}

		public static string ResizeAndCropImage(Thumbnail thumbnail, string image) {
			var size = new Size(thumbnail.Resolution.Width, thumbnail.Resolution.Height);
			if(size.Width > size.Height)
				size.Width = size.Height;
			else if(size.Width < size.Height)
				size.Height = size.Width;

			var x = (thumbnail.Resolution.Width - size.Width) / 2;
			var y = 0;

			var croppedImage = $"{tempFolder}{Guid.NewGuid()}.jpg";
			using(var sourceBitmap = new Bitmap(Image.FromFile(image), thumbnail.Resolution.Width, thumbnail.Resolution.Height)) {
				var cropRect = new Rectangle(x, y, size.Width, size.Height);

				using(var newBitmap = new Bitmap(size.Width, size.Height)) {
					using(var g = Graphics.FromImage(newBitmap)) {
						g.CompositingQuality = CompositingQuality.HighQuality;
						g.InterpolationMode = InterpolationMode.HighQualityBicubic;
						g.PixelOffsetMode = PixelOffsetMode.HighQuality;
						g.SmoothingMode = SmoothingMode.HighQuality;

						g.DrawImage(sourceBitmap, new Rectangle(0, 0, newBitmap.Width, newBitmap.Height), cropRect, GraphicsUnit.Pixel);
						File.WriteAllBytes(croppedImage, GetBitmapBytes(newBitmap));
					}
				}
			}
			return croppedImage;
		}


		private static byte[] GetBitmapBytes(Bitmap source) {
			var codec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
			var parameters = new EncoderParameters(1);
			parameters.Param[0] = new EncoderParameter(Encoder.Quality, 100L);

			using(MemoryStream ms = new MemoryStream()) {
				source.Save(ms, codec, parameters);

				byte[] result = new byte[ms.Length];
				ms.Seek(0, SeekOrigin.Begin);
				ms.Read(result, 0, (int) ms.Length);

				return result;
			}
		}

		private static string GetFileExtensionFromUrl(string url) {
			url = url.Split('?')[0];
			url = url.Split('/').Last();
			return url.Contains('.') ? url.Substring(url.LastIndexOf('.')) : "";
		}

	}
}
