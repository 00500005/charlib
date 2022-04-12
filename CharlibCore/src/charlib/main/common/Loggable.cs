using Vintagestory.API.Common;

namespace charlib {
  public delegate void Tracer(string format, System.Func<object?[]> getParams);
  public interface ILoggable {
    ILogger logger {get;}
    public Tracer TraceF {get;}
  }
  public static class LoggableExtensions {

    public static void Audit(this ILoggable log, string m) 
      => log.logger.Audit(m);
    public static void Audit(this ILoggable log, string f, params object[] a)
      => log.logger.Audit(f, a);
    public static void Build(this ILoggable log, string m)
      => log.logger.Build(m);
    public static void Build(this ILoggable log, string f, params object[] a)
      => log.logger.Build(f, a);
    public static void Chat(this ILoggable log, string f, params object[] a)
      => log.logger.Chat(f, a);
    public static void Chat(this ILoggable log, string m)
      => log.logger.Chat(m);
    public static void Debug(this ILoggable log, string f, params object[] a)
      => log.logger.Debug(f, a);
    public static void Debug(this ILoggable log, string m)
      => log.logger.Debug(m);
    public static void Error(this ILoggable log, string f, params object[] a)
      => log.logger.Error(f, a);
    public static void Error(this ILoggable log, string m)
      => log.logger.Error(m);
    public static void Event(this ILoggable log, string m)
      => log.logger.Event(m);
    public static void Event(this ILoggable log, string f, params object[] a)
      => log.logger.Event(f, a);
    public static void Fatal(this ILoggable log, string m)
      => log.logger.Fatal(m);
    public static void Fatal(this ILoggable log, string f, params object[] a)
      => log.logger.Fatal(f, a);
    public static void Log(this ILoggable log, EnumLogType t, string f, params object[] a)
      => log.logger.Log(t, f, a);
    public static void Log(this ILoggable log, EnumLogType t, string m)
      => log.logger.Log(t, m);
    public static void Notification(this ILoggable log, string f, params object[] a)
      => log.logger.Notification(f, a);
    public static void Notification(this ILoggable log, string m)
      => log.logger.Notification(m);
    public static void StoryEvent(this ILoggable log, string f, params object[] a)
      => log.logger.StoryEvent(f, a);
    public static void StoryEvent(this ILoggable log, string m)
      => log.logger.StoryEvent(m);
    public static void VerboseDebug(this ILoggable log, string m)
      => log.logger.VerboseDebug(m);
    public static void VerboseDebug(this ILoggable log, string f, params object[] a)
      => log.logger.VerboseDebug(f, a);
    public static void Warning(this ILoggable log, string m)
      => log.logger.Warning(m);
    public static void Warning(this ILoggable log, string f, params object[] a)
      => log.logger.Warning(f, a);
    public static void Trace(this ILoggable log, string format) {
      log.TraceF(format, () => new object[]{});
    }
    public static void Trace(this ILoggable log, 
      string format, params object?[] args
    ) {
      log.TraceF(format, () => args);
    }
    public static void Trace(this ILoggable log,
      string format, System.Func<object?[]> getParams) {
      log.TraceF(format, getParams);
    }

    public static Tracer WithPrefix(this ILoggable log, string prefix) {
      return (string format, System.Func<object?[]> getParams) => {
        log.Log(
          EnumLogType.Notification, 
          $"{prefix}{format}", 
          (object[])getParams());
      };
    }
    public static void DoTrace(
      this ILoggable log, string format, System.Func<object?[]> getParams
    ) {
      log.Log(EnumLogType.Notification, format, (object[])getParams());
    }
    public static void IgnoreTrace(
      this ILoggable log, string format, System.Func<object?[]> getParams
    ) {
      // Do nothing
    }
  }
}