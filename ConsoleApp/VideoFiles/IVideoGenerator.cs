using System.Drawing;

namespace ConsoleApp.VideoFiles
{
	public interface IVideoGenerator
	{
		/// <summary>
		/// Initialized video generator, target file
		/// </summary>
		/// <returns></returns>
		void Init();

		/// <summary>
		/// Adds single frame to video
		/// </summary>
		/// <param name="bmp"></param>
		/// <returns></returns>
		void WriteFrame(Bitmap bmp);

		/// <summary>
		/// Finalizes target videofile and stops
		/// </summary>
		/// <returns></returns>
		void FinalizeVideo();

		/// <summary>
		/// Closes video generator
		/// </summary>
		void Close();
	}
}