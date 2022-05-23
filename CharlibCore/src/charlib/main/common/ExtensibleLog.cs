using System;
using Vintagestory.API.Common;

namespace Charlib {
  public delegate void ExtensibleLogOutput<in P>(P? param, string msg);
  public interface IExtensibleLogOutput<in P> { 
    public ExtensibleLogOutput<P>? _LogOutput {get;}
  }
  public static class IExtensibleLogOutputExt {
    // Fallback to console for whatever reason
    public static void LogOutput<P>(
      this IExtensibleLogOutput<P> log,
      P param,
      string message
    ) {
      ExtensibleLogOutput<P> useLog = (
        log._LogOutput ?? ExtensibleLogOutputAdapters.UseConsole<P>()
      );
      useLog(param, message);
    }
  }
  public static partial class ExtensibleLogOutputAdapters {
    public static ExtensibleLogOutput<P> UseConsole<P>() {
      return (p,m) => System.Console.WriteLine(m);
    }
  }
  public delegate string ExtensibleFormat(
    IExtensibleParams opts, 
    string msg, 
    params object[] args
  );
  public delegate string ExtensibleFormat<in P>(
    P opts, 
    string msg, 
    params object?[] args
  ) where P : IExtensibleParams, new();
  public static partial class ExtensibleFormatImpl {
    public static string ExtensibleFormat<FP>(
      FP ops, 
      string msg, 
      params object?[] args
    ) where FP : IExtensibleParams, new() {
      var m = $"[{ops.Prefix}]: {msg}";
      return args.Length > 0 ? String.Format(m, args) : m;
    }
  }
  public static class ExtensibleFormatExts {
    public static string InvokeNull<FP>(
      this ExtensibleFormat<FP> format,
      string msg,
      params object[] args
    ) where FP : IExtensibleParams, new() {
      return format(new FP(), msg, args);
    }
    public static string InvokeCast<FP>(
      this ExtensibleFormat<FP> format,
      IExtensibleParams? opts,
      string msg,
      params object[] args
    ) where FP : IExtensibleParams, new() {
      var castedOpts = opts == null ? new FP() : opts.Cast<FP>();
      return format(castedOpts, msg, args);
    }
  }
  public interface IExtensibleFormat<in P>
    where P : IExtensibleParams, new()
  { 
    public ExtensibleFormat<P> Format {get;}
  }
  public interface IExtensibleParams {
    public string? Prefix {get;set;}
  }
  public interface IExtensibleParams<out S> 
    : IExtensibleParams
    where S : IExtensibleParams<S>,new() { }
  public interface IExtensibleParams<out FP, out LP> 
    : IExtensibleParams<FP>
    where FP : IExtensibleParams<FP, LP>, new() { 
    LP asLogParams();
  }
  public class LogParams : IExtensibleParams<LogParams> {
    public static IExtensibleParams<LogParams> New => new LogParams();
    public string? Prefix {get;set;} = "Debug";
    public static LogParams WithPrefix(string s) => LogParams.New.WithPrefix(s);
    public static LogParams WithPrefix(Enum s) => LogParams.New.WithPrefix(s);
  }
  public static class IExtensibleParamsExt {
    public static V Cast<V>(
      this IExtensibleParams p,
      IDiscriminator<V>? _ = null
    ) where V : IExtensibleParams,new() {
      if (p.GetType().IsAssignableTo<V>()) {
        return (V)p;
      }
      V v = new V();
      v.Prefix = p.Prefix;
      return v;
    }
    public static S WithPrefix<S>(
      this IExtensibleParams<S> self,
      string prefix
    ) where S : IExtensibleParams<S>,new() {
      self.Prefix = prefix;
      return (S)self;
    }
    public static S WithPrefix<S>(
      this IExtensibleParams<S> self,
      Enum prefix
    ) where S : IExtensibleParams<S>,new() {
      self.Prefix = Enum.GetName(prefix.GetType(), prefix);
      return (S)self;
    }
  }
  public interface IExtensibleLog<in FP, in LP> 
    : IExtensibleFormat<FP>, IExtensibleLogOutput<LP>
    where FP : IExtensibleParams<FP, LP>, new()  {}
  public interface IExtensibleLog<in P> 
    : IExtensibleLog<P, P> where P : IExtensibleParams<P, P>, new() {}
  public static class IExtensibleLogBaseExt {
    public static string Format<P>(
      this IExtensibleFormat<P> formatter,
      IExtensibleParams? opts,
      string msg,
      params object[] args
    ) where P : IExtensibleParams, new() {
      return formatter.Format.InvokeCast(opts, msg, args);
    }
    public static string Format<P>(
      this IExtensibleFormat<P> formatter,
      string msg,
      params object[] args
    ) where P : IExtensibleParams, new() {
      return formatter.Format.InvokeNull(msg, args);
    }
  }
  
  public class SimpleExtensibleLog<FP,LP> : IExtensibleLog<FP, LP>
    where FP : IExtensibleParams<FP,LP>, new()
  {
    public SimpleExtensibleLog(
      ExtensibleFormat<FP>? formatFn = null,
      ExtensibleLogOutput<LP>? logFn = null
    ) {
      Format = formatFn ?? ExtensibleFormatImpl.ExtensibleFormat;
      _LogOutput = logFn;
    }
    public ExtensibleFormat<FP> Format {get;}
    public ExtensibleLogOutput<LP>? _LogOutput {get;}
  }
  public static class IExtensibleLogExt {
    public static void Log<FP, LP>(
      this IExtensibleLog<FP, LP> log,
      IExtensibleParams? pars,
      string msg, params object?[] args
    ) 
      where FP : IExtensibleParams<FP,LP>, new() 
      where LP : new()
    {
      var p = pars == null ? new FP() : pars.Cast<FP>();
      log.LogOutput(p.asLogParams(), log.Format(p, msg, args));
    }
    public static void Log<FP, LP>(
      this IExtensibleLog<FP, LP> log,
      string msg, params object?[] args
    ) 
      where FP : IExtensibleParams<FP,LP>, new() 
      where LP : new()
    {
      var p = new FP();
      log.LogOutput(p.asLogParams(), log.Format(p, msg, args));
    }
  }
}