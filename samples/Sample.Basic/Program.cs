using Mantis.Core;
using Mantis.Core.Logging;
using Mantis.Core.Logging.Sinks;

Thread.CurrentThread.Name = "Main Thread";

var init = new LogInitializationDescriptor()
{
	MaximumLevel = LogLevel.Trace,
	Sinks =
	[
		new ConsoleLogSink()
		{
			MaximumLevel = LogLevel.Info,
		},
		new FileLogSink()
		{
			MaximumLevel = LogLevel.Trace,
		},
	],
};
Log.Initialize(in init);

Log.Info(LogCategory.Core, $"Hello, {MantisEngine.NAME} {MantisEngine.Version}!");

Log.Dispose();
