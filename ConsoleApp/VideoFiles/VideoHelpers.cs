using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
//using Size = System.Windows.Size;

namespace ConsoleApp.VideoFiles
{
	public static class VideoHelpers
	{
		public static Size CalcVideoSize(IEnumerable<Size> sizes, AutoSizeMode autoSizeMode)
		{
			var size = new Size();
			foreach (var sz in sizes)
			{
				if (sz.Height < 1 || sz.Width < 1)
				{
					continue;
				}

				switch (autoSizeMode)
				{
					case AutoSizeMode.FirstImageSize:
					{
						size = sz;
						return size;
					}
					case AutoSizeMode.LargestSize:
					{
						size.Width = (int) Math.Max(size.Width, sz.Width);
						size.Height = (int) Math.Max(size.Height, sz.Height);
						break;
					}
					case AutoSizeMode.MaxWidth:
					{
						if (sz.Width > size.Width)
						{
							size = sz;
						}

						break;
					}
					case AutoSizeMode.MaxHeight:
					{
						if (sz.Height > size.Height)
						{
							size = sz;
						}

						break;
					}
					default:
					{
						throw new ArgumentOutOfRangeException(nameof(autoSizeMode), autoSizeMode, null);
					}
				}
			}

			return size;
		}

		public static Bitmap PrepareBitmap(Size size, string srcPath)
		{
			var width = (int)size.Width;
			var height = (int)size.Height;

			// Врисовываем изображение в кадр. Если изображение меньше кадра, то оно помещается в центр. Образуются черные поля по краям
			Image frame;
			using (var image = Image.FromFile(srcPath))
			{
				frame = new Bitmap(width, height);

				using (var graphics =
					Graphics.FromImage(frame))
				{
					graphics.FillRectangle(Brushes.Black, 0, 0, width, height);

					var x = (width - image.Width) / 2;
					var y = (height - image.Height) / 2;

					graphics.DrawImage(image, x, y);
				}
			}

			return new Bitmap(frame);
		}

        public static Bitmap PrepareBitmap(Size size, byte[] imageBytes)
        {
            var width = (int)size.Width;
            var height = (int)size.Height;

            // Врисовываем изображение в кадр. Если изображение меньше кадра, то оно помещается в центр. Образуются черные поля по краям
            Image frame;

            using (var ms = new MemoryStream(imageBytes))
            {
				using (var image = Image.FromStream(ms))
                {
                    frame = new Bitmap(width, height);

                    using (var graphics =
                           Graphics.FromImage(frame))
                    {
                        graphics.FillRectangle(Brushes.Black, 0, 0, width, height);

                        var x = (width - image.Width) / 2;
                        var y = (height - image.Height) / 2;

                        graphics.DrawImage(image, x, y);
                    }
                }
			}

            return new Bitmap(frame);
        }
	}

	public enum AutoSizeMode
	{
		FirstImageSize,
		LargestSize,
		MaxWidth,
		MaxHeight
	}
}