using Mantis.Core.Logging;

namespace Mantis.Platform;

public static class PlatformLogCategories
{
	private static readonly LogCategory platform = LogCategory.Register("Platform");
	private static readonly LogCategory window = LogCategory.Register("Window");
	private static readonly LogCategory keyboard = LogCategory.Register("Keyboard");
	private static readonly LogCategory mouse = LogCategory.Register("Mouse");
	private static readonly LogCategory controller = LogCategory.Register("Controller");
	private static readonly LogCategory screen = LogCategory.Register("Screen");

	extension(LogCategory)
	{
		public static LogCategory Platform => platform;
		public static LogCategory Window => window;
		public static LogCategory Keyboard => keyboard;
		public static LogCategory Mouse => mouse;
		public static LogCategory Controller => controller;
		public static LogCategory Screen => screen;
	}
}
