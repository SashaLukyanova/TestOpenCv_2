using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ConsoleApp.VideoFiles.VideoForWindows
{
	class VfwVideoCreator : VideoGeneratorBase
	{
		AviManager _aviManager;
		VideoStream _aviStream;

		public override void Init()
		{
			try
			{
				_aviManager = new AviManager(TargetFile, false);
				_aviStream = _aviManager.AddVideoStream(true, Fps, 1, Width, Height, PixelFormat.Format32bppArgb);
			}
			catch (Exception ex)
			{
				throw new Exception($"Video generator initialization failed. {ex}");
			}
			IsInitialized = true;
		}

		public override void WriteFrame(Bitmap bmp)
		{
			if (IsInitialized)
			{
				_aviStream.AddFrame(bmp);
			}
			else
			{
				throw new Exception("Video generator must be initialized before use with Init() method");
			}
		}

		public override void FinalizeVideo()
		{
			if (_aviManager != null) _aviManager.Close();
			IsInitialized = false;
		}

		public override void Close()
		{
			if (_aviManager != null)
			{
				_aviManager.Close();
			}
			IsInitialized = false;
		}
	}
}
