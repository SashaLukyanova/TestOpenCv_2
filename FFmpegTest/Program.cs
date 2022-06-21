using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using FFMediaToolkit;
using FFMediaToolkit.Encoding;
using FFMediaToolkit.Graphics;
//using FFMpegCore;
using FFmpegTest.Helpers;

namespace FFmpegTest
{
    internal class Program
    {
        //const string OutVideoFile = @"C:\\Users\\user3\\Desktop\\Video\\video.wmv";
        //const string LogoFile = @"C:\Users\user3\Desktop\Icon.png";

        //const string SearchFolder = @"C:\Users\user3\Desktop\Images";
        const string SearchFolder = @"C:\Users\a.lukyanava\Desktop\Images";

        //const string OutVideoFile = @"C:\\Users\\a.lukyanava\\Desktop\\Video\\video.avi";
        const string LogoFile = @"C:\Users\a.lukyanava\Desktop\Icon.png";

        //вспомогательная
        public static string[] GetImageFiles(string path)
        {
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

            var imageFiles = new List<string>();
            foreach (string filename in files)
            {
                if (Regex.IsMatch(filename, @".jpeg|.png$"))
                {
                    imageFiles.Add(filename);
                }
            }

            return imageFiles.ToArray();
        }
        //вспомогательная
        public static Bitmap GetBitmap(string imegePath)
        {
            var bmpArr = new Bitmap(imegePath);
            

            return bmpArr;
        }
        public static Size GetWindowMaxSizes(string[] bmpArr)
        {
            int maxWidth = 0;
            int maxHeight = 0;
            foreach (var btm in bmpArr)
            {
                var img = new Bitmap(btm);
                if (img.Height > maxHeight)
                {
                    maxHeight = img.Height;
                }

                if (img.Width > maxWidth)
                {
                    maxWidth = img.Width;
                }

                img.Dispose();
            }
            return new Size(maxWidth, maxHeight);
        }
        public static Bitmap AddBackground(Bitmap image, Bitmap background)
        {
            if (image.Width == background.Width && image.Height == background.Height)
            {
                return image;
            }
            var pointX = Convert.ToInt32(background.Width / 2 - image.Width / 2);
            var pointY = Convert.ToInt32(background.Height / 2 - image.Height / 2);
            using (var graphics = Graphics.FromImage(background))
            {
                graphics.DrawImage(image, pointX, pointY);
            }

            return background;
        }
        public static Bitmap AddContent(Bitmap image, Bitmap logo, int index, int logoPoint, Text textSet)
        {
            var standardLength = LogoPointAndLengthStr(image, logo, logoPoint, textSet.Position).StandardLengthStr;
            string text = $"{index} В первой форме метода Substring() подстрока извлекается, начиная с места, " +
                          $"обозначаемого параметром startIndex, и до конца вызывающей строки. " +
                          $"А во второй форме данного метода извлекается подстрока, состоящая из количества символов, " +
                          $"определяемых параметром length, начиная с места, обозначаемого параметром startIndex.";
            var font = new Font(textSet.FontName, textSet.Size);

            using (var g = Graphics.FromImage(image))
            {
                var fontMeasure = g.MeasureString(text, font);
                var fontHeight = Convert.ToInt32(Math.Ceiling(fontMeasure.Height));
                var countSections = Convert.ToInt32(Math.Ceiling(fontMeasure.Width / standardLength));
                var brush = new SolidBrush(Color.FromArgb(textSet.Opacity, textSet.Color));

                g.DrawString(
                    fontMeasure.Width > standardLength
                        ? RebuildMultilineText(text, fontMeasure.Width, standardLength, countSections)
                        : text, font, brush, TextPoint(image, logo, fontHeight, logoPoint, textSet.Position, countSections));

                g.DrawImage(logo, LogoPointAndLengthStr(image, logo, logoPoint, textSet.Position).LogoXY);
            }

            return image;
        }
        public static PointF TextPoint(Bitmap image, Bitmap logo, int fontHeight, int logoPoint, int textPoint, int countSections)
        {
            //textPoint=1 - top, textPoint=2 - bottom
            const int Mergin = 7;
            int pointXText = 0;
            int pointYText = 0;

            if (logoPoint == 1 && textPoint == 1)
            {
                pointXText = logo.Width + Mergin;
                pointYText = Mergin;
                return new PointF(pointXText, pointYText);
            }
            if (logoPoint == 2 && textPoint == 2)
            {
                pointXText = logo.Width + Mergin;
                pointYText = image.Height - fontHeight * countSections - Mergin;
                return new PointF(pointXText, pointYText);
            }
            if (logoPoint == 4 && textPoint == 2)
            {
                pointXText = Mergin;
                pointYText = image.Height - fontHeight * countSections - Mergin;
                return new PointF(pointXText, pointYText);
            }
            if (logoPoint == 3 || logoPoint == 1 && textPoint == 2)
            {
                pointXText = Mergin;
                pointYText = image.Height - fontHeight * countSections - Mergin;
                return new PointF(pointXText, pointYText);
            }
            else
            {
                pointXText = Mergin;
                pointYText = Mergin;
            }

            return new PointF(pointXText, pointYText);
        }
        public static string RebuildMultilineText(string text, float measure, int standard, int countSection)
        {
            const string Enter = "\n";
            const string Space = " ";
            var interval = Convert.ToInt32(Math.Round(text.Length * standard / (measure + 1), MidpointRounding.ToZero));
            var editInterval = 0;

            for (var i = 1; i < countSection; i++)
            {
                editInterval += interval;
                while (String.Compare(Space, 0, text, editInterval, 1) != 0)
                {
                    editInterval--;
                }
                text = text.Insert(editInterval + 1, Enter);
            }
            return text;
        }
        public static (Point LogoXY, int StandardLengthStr) LogoPointAndLengthStr(Bitmap image, Bitmap logo, int logoPoint, int textPoint)
        {
            const int Mergin = 3;
            int pointXLogo = Mergin;//TopLeft
            int pointYLogo = Mergin;

            int standardLength = image.Width - 4 * Mergin;

            if (logoPoint == 2) //BottomLeft
            {
                pointXLogo = Mergin;
                pointYLogo = image.Height - logo.Height - Mergin;
            }
            if (logoPoint == 3) //TopRight
            {
                pointXLogo = image.Width - logo.Width - Mergin;
                pointYLogo = Mergin;
            }
            if (logoPoint == 4) //BottomRight
            {
                pointXLogo = image.Width - logo.Width - Mergin;
                pointYLogo = image.Height - logo.Height - Mergin;
            }
            if ((logoPoint == 2 && textPoint == 2) || (logoPoint == 4 && textPoint == 2) ||
                (logoPoint == 3 && textPoint == 1) || (logoPoint == 1 && textPoint == 1))
            {
                standardLength = image.Width - logo.Width - 2 * Mergin;
            }
            return (new Point(pointXLogo, pointYLogo), standardLength);
        }
        public static Bitmap LogoOpacity(Bitmap logo, float opacity)
        {
            var attributes = new ImageAttributes();
            var matrix = new ColorMatrix { Matrix33 = opacity };
            attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            var rect = new Rectangle(0, 0, logo.Width, logo.Height);

            var newLogo = new Bitmap(logo.Width, logo.Height);

            using (Graphics gr = Graphics.FromImage(newLogo))
            {
                gr.DrawImage(logo, rect, 0, 0, logo.Width, logo.Height,
                    GraphicsUnit.Pixel, attributes);
            }

            return newLogo;
        }
        public static Bitmap LogoSettings(Bitmap logo, Size maxSize, float opacity)
        {
            //Берем 1/8 части длины и высоты главного окна
            const float transform = 0.125f;
            int newWidth = 0;
            int newHeight = 0;

            var standardWidth = Convert.ToInt32(Math.Truncate(maxSize.Width * transform));
            var standardHeight = Convert.ToInt32(Math.Truncate(maxSize.Height * transform));

            if (logo.Width <= standardWidth && logo.Height <= standardHeight)
            {
                return LogoOpacity(logo, opacity);
            }

            if (logo.Width > logo.Height)
            {
                newWidth = standardWidth;
                newHeight = standardWidth * logo.Height / logo.Width;
            }
            else
            {
                newWidth = standardHeight * logo.Width / logo.Height;
                newHeight = standardHeight;
            }

            var result = new Bitmap(newWidth, newHeight);
            using (var graphics = Graphics.FromImage(result))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                graphics.DrawImage(logo, 0, 0, newWidth, newHeight);
            }
            return LogoOpacity(result, opacity);

        }
        public static string OutVideoFile(string videoFormat)
        {
            string outVideoFile = @"C:\\Users\\a.lukyanava\\Desktop\\Video\\video.wmv";
            bool format = false;

            if (videoFormat.EndsWith(".avi") || videoFormat.EndsWith(".Avi") || videoFormat.EndsWith(".AVI"))
            {
                outVideoFile = @"C:\\Users\\a.lukyanava\\Desktop\\Video\\" + videoFormat;
                format = true;
            }
            if (videoFormat.Equals("avi") || videoFormat.Equals("Avi") || videoFormat.Equals("AVI"))
            {
                outVideoFile = @"C:\\Users\\a.lukyanava\\Desktop\\Video\\" + videoFormat;
                format = true;
            }
            if (videoFormat.Equals("wmv") || videoFormat.Equals("Wmv") || videoFormat.Equals("WMV"))
            {
                outVideoFile = @"C:\\Users\\a.lukyanava\\Desktop\\Video\\" + videoFormat;
                format = true;
            }
            if (videoFormat.Equals("mpeg") || videoFormat.Equals("Mpeg") || videoFormat.Equals("MPEG"))
            {
                outVideoFile = @"C:\\Users\\a.lukyanava\\Desktop\\Video\\" + videoFormat;
                format = true;
            }
            if (videoFormat.Equals("mp4") || videoFormat.Equals("Mp4") || videoFormat.Equals("MP4"))
            {
                outVideoFile = @"C:\\Users\\a.lukyanava\\Desktop\\Video\\" + videoFormat;
                format = true;
            }
            if (!format)
            {
                outVideoFile = @"C:\\Users\\a.lukyanava\\Desktop\\Video\\" + videoFormat + ".wmv";
            }

            return outVideoFile;
        }
        public static void FFMpegVideoGenerator(string[] imagesPath, Bitmap logo, Bitmap background, Size maxSize, Text textObject, Watermark watermark, Video video)
        {
            var counter = 0;
            FFmpegLoader.FFmpegPath = @"C:\FFmpegLgp\bin\Win32";

            var settings = new VideoEncoderSettings(maxSize.Width, maxSize.Height, 1, codec: VideoCodec.Default);
            settings.EncoderPreset = EncoderPreset.VeryFast;
            settings.CRF = 18;
            var file = MediaBuilder.CreateContainer(OutVideoFile(video.Format)).WithVideo(settings).Create();
            var rect = new System.Drawing.Rectangle(System.Drawing.Point.Empty, maxSize);

            foreach (var imagePath in imagesPath)
            {
                var image = new Bitmap(imagePath);
                var cloneBackground = (Bitmap)background.Clone(new Rectangle(0, 0, background.Width, background.Height), image.PixelFormat);
                var imageWithBackground = AddBackground(image, cloneBackground);

                var bmpWithTextAndLogo = AddContent(imageWithBackground, logo, counter, watermark.Position, textObject);

                

                var bitLock = bmpWithTextAndLogo.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                var bitmapData = ImageData.FromPointer(bitLock.Scan0, ImagePixelFormat.Bgr24, bmpWithTextAndLogo.Size);

                int i = 0;
                while (i < video.Fps)
                {
                    file.Video.AddFrame(bitmapData);
                    i++;
                }
                bmpWithTextAndLogo.UnlockBits(bitLock);
                bmpWithTextAndLogo.Dispose();
                imageWithBackground.Dispose();
                cloneBackground.Dispose();
                image.Dispose();
                counter++;
            }
            file.Dispose();
        }

        //public static void FFMpegVideoGenerator(Text textObject, Watermark watermark, Video video)
        //{
        //    var counter = 0;
        //    FFmpegLoader.FFmpegPath = @"C:\FFmpegLgp\bin\Win32";
        //    var imagesPath = Directory.GetFiles(SearchFolder, "*.*", SearchOption.AllDirectories);

        //    var images = GetBitmap(SearchFolder);

        //    //Максимальные размеры рабочего окна
        //    var maxWidth = GetWindowMaxSizes(images).Width;
        //    var maxHeight = GetWindowMaxSizes(images).Height;

        //    var logo = LogoSettings(new Bitmap(LogoFile), maxWidth, maxHeight, watermark.Opacity);
        //    var background = new Bitmap(maxWidth, maxHeight);


        //    //background.Dispose();
        //    //logo.Dispose();


        //    var settings = new VideoEncoderSettings(maxWidth, maxHeight, 1, codec: VideoCodec.Default);
        //    settings.EncoderPreset = EncoderPreset.VeryFast;
        //    settings.CRF = 18;
        //    var file = MediaBuilder.CreateContainer(OutVideoFile(video.Format)).WithVideo(settings).Create();

        //    foreach (var image in images)
        //    {
        //        var cloneBackground = (Bitmap)background.Clone(new Rectangle(0, 0, background.Width, background.Height), image.PixelFormat);
        //        var imageWithBackground = AddBackground(image, cloneBackground);

        //        var bmpWithTextAndLogo = AddContent(imageWithBackground, logo, counter, watermark.Position, textObject);

        //        var rect = new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmpWithTextAndLogo.Size);

        //        var bitLock = bmpWithTextAndLogo.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        //        var bitmapData = ImageData.FromPointer(bitLock.Scan0, ImagePixelFormat.Bgr24, bmpWithTextAndLogo.Size);

        //        int i = 0;
        //        while (i < video.Fps)
        //        {
        //            file.Video.AddFrame(bitmapData);
        //            i++;
        //        }

        //        bmpWithTextAndLogo.UnlockBits(bitLock);
        //        bmpWithTextAndLogo.Dispose();
        //        imageWithBackground.Dispose();
        //        cloneBackground.Dispose();
        //        image.Dispose();
        //        counter++;
        //    }
        //    file.Dispose();
        //}

        static void Main(string[] args)
        {
            #region Initialize
            //Console.WriteLine("- Settings watermark.");
            //Console.WriteLine("Choose a POSITION for watermark:\n1 - TopLeft\n2 - TopRight\n3 - BottomLeft\n4 - BottomRight");
            //var logoPoint = Console.ReadLine();
            //Console.WriteLine("Choose a OPACITY for watermark (interval 0 - 1):");
            //var logoOpacity = Console.ReadLine();
            //Console.WriteLine("- Settings text.");
            //Console.WriteLine("Choose a POSITION for text:\n1 - Top\n2 - Bottom");
            //var textPoint = Console.ReadLine();
            //Console.WriteLine("Choose a OPACITY for text (interval 0 - 100):");
            //var textOpacity = Console.ReadLine();
            //Console.WriteLine("Choose a COLOR for text:\n1 - Red\n2 - Black\n3 - White");
            //var textColor = Console.ReadLine();
            //Console.WriteLine("Choose a FONT for text:\n1 - Arial\n2 - Times New Roman\n3 - Verdana");
            //var textFont = Console.ReadLine();
            //Console.WriteLine("Choose a SIZE for text (8, ..., 30, ...):");
            //var textSize = Console.ReadLine();
            //Console.WriteLine("- Settings video.");
            //Console.WriteLine("Choose a WAITING between frames (sec): 1, 2, 3, ... ");
            //var videoFps = Console.ReadLine();
            //Console.WriteLine("Input a FORMAT for video (default wmv):");
            //var videoFormat = Console.ReadLine();

            //var textObject = new Text();
            //var video = new Video();
            //var watermark = new Watermark();
            //watermark.Position = int.Parse(logoPoint);
            //watermark.Opacity = float.Parse(logoOpacity);
            //video.Format = videoFormat;
            //video.Fps = int.Parse(videoFps);
            //textObject.Size = int.Parse(textSize);
            //textObject.Position = int.Parse(textPoint);
            //textObject.Opacity = int.Parse(textOpacity);
            //if (int.Parse(textColor) == 1) textObject.Color = Color.Red;
            //if (int.Parse(textColor) == 2) textObject.Color = Color.Black;
            //if (int.Parse(textColor) == 3) textObject.Color = Color.White;
            //if (int.Parse(textFont) == 1) textObject.FontName = "Arial";
            //if (int.Parse(textFont) == 2) textObject.FontName = "Times New Roman";
            //if (int.Parse(textFont) == 3) textObject.FontName = "Verdana";

            var textObject = new Text();
            var video = new Video();
            var watermark = new Watermark();
            watermark.Position = 3;
            watermark.Opacity = 0.6f;
            video.Format = "rert.xxx";
            video.Fps = 3;
            textObject.Size = 18;
            textObject.Position = 1;
            textObject.Opacity = 90;
            textObject.Color = Color.Red;
            textObject.FontName = "Arial";
            #endregion
            TimeSpan ts;

            //Счетчик
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            
            Console.WriteLine("Start FFMpeg video genereator:");
            var imagesPath = Directory.GetFiles(SearchFolder, "*.*", SearchOption.AllDirectories);
            var maxSize = GetWindowMaxSizes(imagesPath);
            //var maxHeight = GetWindowMaxSizes(imagesPath).Height;
            //ts = stopWatch.Elapsed;
            var logo = LogoSettings(new Bitmap(LogoFile), maxSize, watermark.Opacity);
            var background = new Bitmap(maxSize.Width, maxSize.Height);
            
            FFMpegVideoGenerator(imagesPath, logo, background, maxSize, textObject, watermark, video);
            

            stopWatch.Stop();

            background.Dispose();
            logo.Dispose();
            
            ts = stopWatch.Elapsed;
            var elapsedTime = $"{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:00}";
            Console.WriteLine($"RunTime:{elapsedTime}");
            GC.Collect();
            Console.WriteLine("Press any key to close window");
            Console.ReadKey();
        }
    }
}
