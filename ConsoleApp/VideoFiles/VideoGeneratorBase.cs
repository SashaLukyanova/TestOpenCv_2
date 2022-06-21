using System.Drawing;

namespace ConsoleApp.VideoFiles
{
	public abstract class VideoGeneratorBase : Disposable, IVideoGenerator
	{
		/// <summary>
		/// Gets or sets width of the target video
		/// </summary>
		public int Width { get; set; }

		/// <summary>
		/// Gets or sets height of the target video
		/// </summary>
		public int Height { get; set; }

		/// <summary>
		/// Gets or sets frame rate (frames per second) of the target video
		/// </summary>
		public uint Fps { get; set; }

		/// <summary>
		/// Gets or sets target video file name. Must have .mp4 or .wmv extension
		/// </summary>
		public string TargetFile { get; set; }

		public bool IsInitialized { get; protected set; }

		public abstract void Init();

		public abstract void WriteFrame(Bitmap bmp);

		public abstract void FinalizeVideo();

		public abstract void Close();
	}
}