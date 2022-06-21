using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using ConsoleApp.VideoFiles;
using FFMediaToolkit;
using FFMediaToolkit.Encoding;
using FFMediaToolkit.Graphics;
using Point = System.Drawing.Point;

namespace ConsoleApp
{
    internal class Program
    {
        //const string OutVideoFile = @"C:\\Users\\user3\\Desktop\\Video\\video.wmv";
        //const string LogoFile = @"C:\Users\user3\Desktop\Icon.png";

        //const string SearchFolder = @"C:\Users\user3\Desktop\Images";
        //const string SearchFolder = @"C:\Users\a.lukyanava\Desktop\Images";

        const string OutVideoFile = @"C:\\Users\\a.lukyanava\\Desktop\\Video\\video.avi";
        const string LogoFile = @"C:\Users\a.lukyanava\Desktop\Icon.png";
        
        public static string[] GetFiles(string path)
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
        public static Bitmap[] CreateBitmap(string path)
        {
            var images = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            var bmpArr = new Bitmap[images.Length];
            for (int i = 0; i < bmpArr.Length; i++)
            {
                bmpArr[i] = new Bitmap(images[i]);
            }

            return bmpArr;
        }
        public static (int Width, int Height) GetMaxSizes(Bitmap[] bmpArr)
        {
            int maxWidth = 0;
            int maxHeight = 0;
            foreach (var btm in bmpArr)
            {
                if (btm.Height > maxHeight)
                {
                    maxHeight = btm.Height;
                }

                if (btm.Width > maxWidth)
                {
                    maxWidth = btm.Width;
                }
            }

            return (maxWidth, maxHeight);
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
        public static Bitmap AddContent(Bitmap image, Bitmap logo, int index, int logoPoint, string fontName, float fontSize, Color fontColor)
        {
            const int mergin = 3;
            int pointXLogo = mergin;
            int pointYLogo = mergin;

            int standartLength = image.Width - 4 * mergin;
            int pointXText = image.Width / 15;
            int pointYText = image.Height - image.Height / 10;

            if (logoPoint == 2) //bottomLeft
            {
                pointXLogo = mergin;
                pointYLogo = image.Height - logo.Height - mergin;
                standartLength = image.Width - logo.Width - 2 * mergin;
            }

            if (logoPoint == 3)
            {
                pointXLogo = image.Width - logo.Width - mergin;
                pointYLogo = mergin;
            }

            if (logoPoint == 4) //bottomRight
            {
                pointXLogo = image.Width - logo.Width - mergin;
                pointYLogo = image.Height - logo.Height - mergin;
                standartLength = image.Width - logo.Width - 2 * mergin;
            }

            string text = $"Test text и еще для испытания длины и языка.{index} fffffffffffffff";
            var font = new Font(fontName, fontSize);


            var pointText = new PointF(pointXText, pointYText);

            using (var g = Graphics.FromImage(image))
            {
                var fontMeasure = g.MeasureString(text, font);
                var aa = text.Length;
                if (fontMeasure.Width > standartLength)
                {
                    //[] resText = text.Substring(0, standartLength);
                    //g.DrawString(resText, font, new SolidBrush(fontColor), pointText);
                }

                g.DrawString(text, font, new SolidBrush(fontColor), pointText);
                g.DrawImage(logo, pointXLogo, pointYLogo);
            }

            return image;
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
        public static Bitmap BuildingLogo(Bitmap logo, int maxWidth, int maxHeight, float opacity)
        {
            //Берем 1/8 части длины и высоты главного окна
            const float transform = 0.125f;
            int newWidth = 0;
            int newHeight = 0;

            var standardWidth = Convert.ToInt32(Math.Truncate(maxWidth * transform));
            var standardHeight = Convert.ToInt32(Math.Truncate(maxHeight * transform));

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
        public static void MediaFabricVideoGenerator(Bitmap[] images, Bitmap background, Bitmap logo, int maxWidth, int maxHeight)
        {
            using (var videoGenerator = MediaFabric.CreateVideoGenerator())
            {
                var counter = 0;
                videoGenerator.Fps = 1;
                videoGenerator.Height = maxHeight;
                videoGenerator.Width = maxWidth;

                videoGenerator.TargetFile = OutVideoFile;
                try
                {
                    videoGenerator.Init();

                    if (!videoGenerator.IsInitialized)
                    {
                        throw new ApplicationException("Video generator creation failed");
                    }

                    foreach (var image in images)
                    {
                        var cloneBackground = (Bitmap)background.Clone();
                        var imageWithBackground = AddBackground(image, cloneBackground);

                        var bmpWithTextAndLogo = AddContent(imageWithBackground, logo, counter, 2, "Verdana", 20f, Color.Red);
                        //var bmpWithTextAndLogo = AddContent(imageWithBackground, (Bitmap)logo.Clone(), counter);

                        videoGenerator.WriteFrame(bmpWithTextAndLogo);

                        bmpWithTextAndLogo.Dispose();
                        imageWithBackground.Dispose();

                        cloneBackground.Dispose();
                        image.Dispose();
                        counter++;
                        Console.WriteLine(counter);
                    }

                }

                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    videoGenerator.Close();
                }
            }
        }
        public static void OpenCvVideoGenerator(Bitmap[] images, Bitmap background, Bitmap logo, int maxWidth, int maxHeight)
        {
            var counter = 0;
            var videoWriter = new OpenCvSharp.VideoWriter();
            var fourcc =0;
            fourcc = FourCC.XVID;//AVI: XVID, DIVX
            //fourcc = FourCC.WMV1; //WMV

            videoWriter.Open(OutVideoFile, fourcc, 0.33, new OpenCvSharp.Size(maxWidth, maxHeight), true);

            foreach (var image in images)
            {
                var cloneBackground = (Bitmap)background.Clone(new Rectangle(0, 0, background.Width, background.Height), image.PixelFormat);
                var imageWithBackground = AddBackground(image, cloneBackground);

                var bmpWithTextAndLogo = AddContent(imageWithBackground, logo, counter, 2, "Verdana", 20f, Color.Red);

                videoWriter.Write(bmpWithTextAndLogo.ToMat());

                bmpWithTextAndLogo.Dispose();
                imageWithBackground.Dispose();

                cloneBackground.Dispose();
                image.Dispose();
                counter++;
            }
            videoWriter.Dispose();
        }
        public static void FFMpegVideoGenerator(Bitmap[] images, Bitmap background, Bitmap logo, int maxWidth, int maxHeight)
        {

            var counter = 0;
            //FFmpegLoader.FFmpegPath = @"C:\FFmpeg\bin\Win32";
            FFmpegLoader.FFmpegPath = @"C:\_FFmpeg\bin";
            var settings = new VideoEncoderSettings(maxWidth, maxHeight, 1, codec: VideoCodec.Default);
            settings.EncoderPreset = EncoderPreset.VeryFast;
            settings.CRF = 18;
            var file = MediaBuilder.CreateContainer(OutVideoFile).WithVideo(settings).Create();
            
            foreach (var image in images)
            {
                var cloneBackground = (Bitmap)background.Clone(new Rectangle(0, 0, background.Width, background.Height), image.PixelFormat);
                var imageWithBackground = AddBackground(image, cloneBackground);

                var bmpWithTextAndLogo = AddContent(imageWithBackground, logo, counter, 2, "Verdana", 20f, Color.Red);
                //var bmpWithTextAndLogo = AddContent(imageWithBackground, logo, counter);

                var rect = new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmpWithTextAndLogo.Size);

                var bitLock = bmpWithTextAndLogo.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                var bitmapData = ImageData.FromPointer(bitLock.Scan0, ImagePixelFormat.Bgr24, bmpWithTextAndLogo.Size);

                int i = 0;
                while (i < 3)
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
            //videoWriter.Dispose();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Please, choose video generator:\n1 - OpenCv\n2 - MediaFabric\n3 - FFMpeg");
            var inputValue= Console.ReadLine() ?? "1";
            
            string SearchFolder = inputValue.Equals("2") ? @"C:\Users\a.lukyanava\Desktop\Images" : @"C:\Users\a.lukyanava\Desktop\Images";
            //string SearchFolder = inputValue.Equals("2") ? @"C:\Users\user3\Desktop\Images" : @"C:\Users\user3\Desktop\Images1";

            TimeSpan ts;

            //Счетчик
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            //Рабочий массив Bitmap изображений
            var images = CreateBitmap(SearchFolder);
            
            //Максимальные размеры рабочего окна
            var maxWidth = GetMaxSizes(images).Width;
            var maxHeight = GetMaxSizes(images).Height;

            var logo = BuildingLogo(new Bitmap(LogoFile), maxWidth, maxHeight, 0.5f);
            var background = new Bitmap(maxWidth, maxHeight);
            if (inputValue.Equals("1"))
            {
                Console.WriteLine("Start OpenCv video genereator:");
                OpenCvVideoGenerator(images, background, logo, maxWidth, maxHeight);
            }
            if (inputValue.Equals("2"))
            {
                Console.WriteLine("Start MediaFabric video genereator:");
                MediaFabricVideoGenerator(images, background, logo, maxWidth, maxHeight);
            }
            if (inputValue.Equals("3"))
            {
                Console.WriteLine("Start FFMpeg video genereator:");
                FFMpegVideoGenerator(images, background, logo, maxWidth, maxHeight);
            }

            stopWatch.Stop();
            
            background.Dispose();
            logo.Dispose();

            ts = stopWatch.Elapsed;
            var elapsedTime = $"{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:00}";
            Console.WriteLine($"RunTime:{elapsedTime}");
            GC.Collect();
            Console.ReadKey();
        }
    }
}