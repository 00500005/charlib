using System;
using Vintagestory.API.Common;

namespace Charlib {
  public static partial class ExtensibleLogOutputAdapters {
    public static ExtensibleLogOutput<EnumLogType> ILoggerAdapter(
      ILogger? logger
    ) {
      return logger == null 
        ? ExtensibleLogOutputAdapters.UseConsole<EnumLogType>()
        : logger.Log;
    }
  }
  public static partial class ExtensibleFormatImpl {
    public static string VsExtensibleFormat<FP>(
      FP param,
      string msg,
      params object?[] args
    ) where FP : VsLogParams,IVsLogParams<FP>,new() {
      return args.Length > 0 ? String.Format(msg, args) : msg;
    }
  }
  public class VsLogParams : IVsLogParams<VsLogParams> {
    public static VsLogParams New => new VsLogParams();
    public VsLogParams() {}
    public EnumLogType LogType = EnumLogType.Debug;
    public string? Prefix {
      get => Enum.GetName(typeof(EnumLogType), LogType);
      set => Enum.TryParse<EnumLogType>(value, out LogType);
    }
    EnumLogType IVsLogParams<VsLogParams>.LogType { 
      get => LogType; 
      set => LogType = value; 
    }

    public EnumLogType asLogParams() {
      return LogType;
    }
  }
  public interface IVsLogParams<out S> 
    : IExtensibleParams<S, EnumLogType> 
    where S : VsLogParams, IExtensibleParams<S, EnumLogType>, new() {
    public EnumLogType LogType {get;set;}
  }
  public interface IVsLog<in P> 
    : IExtensibleLog<P, EnumLogType> 
    where P : VsLogParams, IVsLogParams<P>, new() {}
  public interface IVsLog : IVsLog<VsLogParams> {}
  public class VsLog : SimpleExtensibleLog<VsLogParams, EnumLogType>, IVsLog {
    public VsLog(
      ExtensibleFormat<VsLogParams>? formatFn = null, 
      ExtensibleLogOutput<EnumLogType>? logFn = null
    ) : base(formatFn ?? ExtensibleFormatImpl.VsExtensibleFormat, logFn) { }
    public VsLog(
      ILogger? logger,
      ExtensibleFormat<VsLogParams>? formatFn = null 
    ) : base(
      formatFn ?? ExtensibleFormatImpl.VsExtensibleFormat, ExtensibleLogOutputAdapters.ILoggerAdapter(logger)
    ) { }
  }
  public static class IExtensibleLogVsExt {
    public static void Audit(
      this IExtensibleLog<VsLogParams, EnumLogType> log, 
      string m, params object?[] a
    ) => log.Log(VsLogParams.New.WithPrefix(EnumLogType.Audit), m, a);
    public static void Build(
      this IExtensibleLog<VsLogParams, EnumLogType> log, 
      string m, params object?[] a
    ) => log.Log(VsLogParams.New.WithPrefix(EnumLogType.Build), m, a);
    public static void Chat(
      this IExtensibleLog<VsLogParams, EnumLogType> log, 
      string m, params object?[] a
    ) => log.Log(VsLogParams.New.WithPrefix(EnumLogType.Chat), m, a);
    public static void Debug(
      this IExtensibleLog<VsLogParams, EnumLogType> log, 
      string m, params object?[] a
    ) => log.Log(VsLogParams.New.WithPrefix(EnumLogType.Debug), m, a);
    public static void Error(
      this IExtensibleLog<VsLogParams, EnumLogType> log, 
      string m, params object?[] a
    ) => log.Log(VsLogParams.New.WithPrefix(EnumLogType.Error), m, a);
    public static void Event(
      this IExtensibleLog<VsLogParams, EnumLogType> log, 
      string m, params object?[] a
    ) => log.Log(VsLogParams.New.WithPrefix(EnumLogType.Event), m, a);
    public static void Fatal(
      this IExtensibleLog<VsLogParams, EnumLogType> log, 
      string m, params object?[] a
    ) => log.Log(VsLogParams.New.WithPrefix(EnumLogType.Fatal), m, a);
    public static void Log(
      this IExtensibleLog<VsLogParams, EnumLogType> log, 
      EnumLogType t, string m, params object?[] a
    ) => log.Log(VsLogParams.New.WithPrefix(t), m, a);
    public static void Notification(
      this IExtensibleLog<VsLogParams, EnumLogType> log, 
      string m, params object?[] a
    ) => log.Log(VsLogParams.New.WithPrefix(EnumLogType.Notification), m, a);
    public static void StoryEvent(
      this IExtensibleLog<VsLogParams, EnumLogType> log, 
      string m, params object?[] a
    ) => log.Log(VsLogParams.New.WithPrefix(EnumLogType.StoryEvent), m, a);
    public static void VerboseDebug(
      this IExtensibleLog<VsLogParams, EnumLogType> log, 
      string m, params object?[] a
    ) => log.Log(VsLogParams.New.WithPrefix(EnumLogType.VerboseDebug), m, a);
    public static void Warning(
      this IExtensibleLog<VsLogParams, EnumLogType> log, 
      string m, params object?[] a
    ) => log.Log(VsLogParams.New.WithPrefix(EnumLogType.Warning), m, a);
  }
}