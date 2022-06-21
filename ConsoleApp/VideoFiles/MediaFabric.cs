using System;
using System.Collections.Generic;
using ConsoleApp.VideoFiles.MediaFoundationVideo;
using ConsoleApp.VideoFiles.VideoForWindows;

namespace ConsoleApp.VideoFiles
{
	/// <summary>
	/// Класс-фабрика видеогенераторов
	/// </summary>
	public static class MediaFabric
	{
		/// <summary>
		/// Возвращает поддерживаемый данной опреационкой генератор
		/// </summary>
		public static VideoApi SupportedApi => DetermineApi();

		public static VideoContainerType DefaultVideoContainerType
		{
			get
			{
				switch (SupportedApi)
				{
					case VideoApi.VideoForWindows:
						return VideoContainerType.Avi;
					case VideoApi.MediaFoundation:
						return VideoContainerType.Wmv;
					default:
						return VideoContainerType.None;
				}
			}
		}

		private static VideoApi DetermineApi()
		{
			var major = Environment.OSVersion.Version.Major;
			if (major == 5)
				return VideoApi.VideoForWindows;
			if (major > 5)
				return VideoApi.MediaFoundation;
			return VideoApi.NotSupported;
		}

		public static VideoGeneratorBase CreateVideoGenerator()
		{
			switch (SupportedApi)
			{
				case VideoApi.VideoForWindows:
					return new VfwVideoCreator();
				case VideoApi.MediaFoundation:
					return new MfVideoCreator();
				default:
					return null;
			}
		}
	}

	public static class VideoContainers
	{
	    private static Dictionary<VideoContainerType, string> _extensions = new Dictionary<VideoContainerType, string>
	                                                                            {
	                                                                                {VideoContainerType.Avi, ".avi"},
	                                                                                {VideoContainerType.Mp4, ".mp4"},
	                                                                                {VideoContainerType.Wmv, ".wmv"}
	                                                                            };
		public static Dictionary<VideoContainerType, string> Extensions => _extensions;
	}

	public enum VideoContainerType
	{
		None,
		Avi,
		Mp4,
		Wmv
	}

	public enum VideoApi
	{
		NotSupported,
		VideoForWindows,
		MediaFoundation
	}
}
