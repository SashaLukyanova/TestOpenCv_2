using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using SharpDX.MediaFoundation;

namespace ConsoleApp.VideoFiles.MediaFoundationVideo
{
	/// <summary>
	/// Create video from series of images
	/// </summary>
	public class MfVideoCreator : VideoGeneratorBase
	{
		public delegate void ProgressChangedEventHandler(object sender, ProgressChangedEventArgs e);

		private VideoFormat _encodingFormat;
		private VideoFormat _inputFormat;
		private int _mainStreamIndex;
		private SinkWriter _sinkWriter;
		private long _endOfCurrentFilm = 0;
		private long _rtDuration;
		public ImagesErrorsPanicLevel ImagesErrorsPanicLevel { get; set; }

		public MfVideoCreator()
		{
			Fps = 1;
			Delay = 2;
		}

		/// <summary>
		/// Gets or sets the Debuger messages On/Off
		/// </summary>
		public bool DebugMode { get; set; }

		/// <summary>
		/// Gets the containertype of target
		/// </summary>
		public MediaContainer MediaContainer { get; private set; }

		/// <summary>
		/// Gets or sets slides delay
		/// </summary>
		public uint Delay { get; set; }

		/// <summary>
		/// Gets status of video creator
		/// </summary>
		public Status Status { get; private set; }

		/// <summary>
		/// Starts video renderer
		/// </summary>
		/// <remarks>Method chooses video format in accordance with target file extension, so extension must be "mp4" or "wmv"</remarks>
		public override void Init()
		{

			ImagesErrorsPanicLevel = ImagesErrorsPanicLevel.PassBraked;

			try
			{
				SetUpTargetAndFormats();

				MfHelpers.MfStartup();
				Status = Status.Started;

				try
				{
					_inputFormat.FrameSize = new PackedSize((uint)Width, (uint)Height);
					_inputFormat.FrameRate = new PackedInt32(Fps, 1);

					_encodingFormat.FrameSize = new PackedSize((uint)Width, (uint)Height);
				}
				catch (Exception)
				{
					throw new Exception("Frame size set up failed");
				}

				// Creating and start sink writer
				InitializeSinkWriter();

				if (_sinkWriter == null)
				{
					throw new Exception("Video writer initialization failed");
				}

				MediaFactory.FrameRateToAverageTimePerFrame((int)Fps, (int)(Fps * Delay), out _rtDuration);
			}
			catch (Exception ex)
			{
				throw new Exception($"Video generator initialization failed. {ex}");
			}
			IsInitialized = true;
		}

		public override void WriteFrame(Bitmap bmp)
		{
			try
			{
				WriteFrame(bmp, ref _endOfCurrentFilm, _rtDuration);
			}
			catch (Exception ex)
			{
				Status = Status.Error;
				throw new Exception($"Add frame failed. {ex.Message}");
			}
		}

		public override void FinalizeVideo()
		{
			try
			{
				if (_sinkWriter != null)
					_sinkWriter.Finalize();
				Status = Status.Finished;
			}
			catch (Exception ex)
			{
				Status = Status.Error;
				throw new Exception($"Video render throws exception: {ex.Message}");
			}
		}

		public override void Close()
		{
			FinalizeVideo();
		}

		/// <summary>
		/// Sets video formats of input and output media in according to target file extension
		/// </summary>
		private void SetUpTargetAndFormats()
		{
			if (string.IsNullOrEmpty(TargetFile))
			{
				throw new Exception("Target file name must be set");
			}

			var extension = Path.GetExtension(TargetFile);
			if (string.IsNullOrEmpty(extension))
			{
				throw new Exception("Target file name must have .mp4 or .wmv extension");
			}
			var s = extension.ToLower();
			switch (s)
			{
				case ".mp4":
					_encodingFormat = new VideoFormat(Consts.MFVideoFormat_H264, Fps) { AvgBitRate = (uint)(Width * Height * 3) };
					MediaContainer = MediaContainer.Mp4;
					break;
				case ".wmv":
					_encodingFormat = new VideoFormat(Consts.MFVideoFormat_WMV3, Fps) { AvgBitRate = (uint)(Width * Height * 3) };
					MediaContainer = MediaContainer.Wmv;
					break;
				default:
					throw new Exception("Mpeg4 (.mp4) and Asf (.wmv) containers are only supported");
			}

			_inputFormat = new VideoFormat(Consts.MFVideoFormat_AYUV);
		}

		private void InitializeSinkWriter()
		{
			try
			{
				_sinkWriter = MediaFactory.CreateSinkWriterFromURL(TargetFile, null, null);
			}
			catch (Exception exception)
			{
				throw new Exception($"Creating target video writer failed. {exception.Message}");
			}

			if (_sinkWriter == null)
				return;

			// настройка цели
			using (var mediaTypeOut = MfHelpers.CreateVideoMediaType(_encodingFormat))
			{
				_sinkWriter.AddStream(mediaTypeOut, out _mainStreamIndex);
			}

			// Настройка источника
			var inputVideoFormat = new VideoFormat(_inputFormat.Subtype.ToString())
			{
				AvgBitRate = (uint)(Width * Height * 3),
				FrameRate = new PackedInt32(Fps, 1),
				FrameSize = new PackedSize((uint)Width, (uint)Height),
				InterlaceMode = 2,
				PixelAspectRatio = new PackedInt32(1, 1)
			};

			using (var mediaTypeIn = MfHelpers.CreateVideoMediaType(inputVideoFormat))
			{
				try
				{
					_sinkWriter.SetInputMediaType(_mainStreamIndex, mediaTypeIn, null);
				}
				catch (Exception ex)
				{
					throw new Exception($"Enable to set Input video type. {ex.Message}");
				}
			}

			// Старт записи
			_sinkWriter.BeginWriting();
		}

		public void WriteFrame(Bitmap bmp, ref long rtStart, long rtDuration)
		{
			var stride = bmp.Width * 4;
			var data = new byte[Math.Abs(stride) * bmp.Height];

			using (var sample = MediaFactory.CreateSample())
			{
				using (var buffer = MediaFactory.CreateMemoryBuffer(Math.Abs(stride) * bmp.Height))
				{
					int maxLenght;
					int currentLenght;
					var pBuf = buffer.Lock(out maxLenght, out currentLenght);
					MediaFactory.CopyImage(data, stride, ImageHelpers.Rgb32ImageToYuvBytes(bmp), stride, stride,
						bmp.Height);

					buffer.CurrentLength = data.Length;
					Marshal.Copy(data, 0, pBuf, data.Length);
					buffer.Unlock();

					sample.AddBuffer(buffer);
					sample.SampleTime = rtStart;
					sample.SampleDuration = rtDuration;

					// Send the sample to the Sink Writer.
					try
					{
						_sinkWriter.WriteSample(_mainStreamIndex, sample);
						rtStart += rtDuration;
					}
					catch
					{
						if (ImagesErrorsPanicLevel == ImagesErrorsPanicLevel.PassBraked)
						{
							return;
						}
						throw new Exception("Writing sample failed.");
					}
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && Status == Status.Finished || Status == Status.Error)
			{
				if (_sinkWriter != null)
				{
					if (Status == Status.Error)
						try
						{
							_sinkWriter.Finalize(); // mpe: Have't found any way to check _sinkWriter allready finalized the target video file
						}
						catch
						{
						}


					_sinkWriter.Dispose();
					_sinkWriter = null;
				}
				if (Status == Status.Error && File.Exists(TargetFile))
					File.Delete(TargetFile);


				try
				{
					if(IsInitialized || Status == Status.Started)
						MfHelpers.MfShutdown();
				}
				catch (Exception)
				{
				}
			}
		}
	}

	public class ProgressChangedEventArgs
	{
		public ProgressChangedEventArgs(int p, int t)
		{
			ImagesRendered = p;
			TotalImages = t;
		}

		public int ImagesRendered { get; private set; }
		public int TotalImages { get; private set; }
	}

	public enum MediaContainer
	{
		Mp4,
		Wmv
	}

	public enum Status
	{
		Started,
		Finished,
		Error
	}

	public enum ImagesErrorsPanicLevel
	{
		/// <summary>
		/// Proccess just pass 'not found' image
		/// </summary>
		PassMissed,

		/// <summary>
		/// Proccess pass image, that is present but can not be readed
		/// </summary>
		PassBraked,

		/// <summary>
		/// Throw exception on any problem with image file
		/// </summary>
		OnAnyErrorsPanic

	}
}

