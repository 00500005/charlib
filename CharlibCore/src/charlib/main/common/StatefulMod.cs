
using System;
using Vintagestory.API.Common;

namespace Charlib
{
  public interface IStatefulMod<S> 
    where S : IModState<S> {}
  public interface IStatefulMod<S,M>
    : IStatefulMod<S>
    where S : IModState<S,M> 
    where M : IStatefulMod<S,M> {}
  public interface IStatefulMod<S,M,FP>
    : IVsLog<FP>, IStatefulMod<S,M>
    where S : IModState<S,M,FP> 
    where M : IStatefulMod<S,M,FP>
    where FP : VsLogParams, IVsLogParams<FP>, new() {}
  public abstract class StatefulMod<S,M,FP> 
    : ModSystem, IVsLog<FP>, IStatefulMod<S,M,FP>
    where S : IModState<S,M,FP>
    where M : StatefulMod<S,M,FP>
    where FP : VsLogParams, IVsLogParams<FP>, new()
  {
    public abstract S CreateState(ICoreAPI api, ILogger? logger=null);
    public void AssignState(ICoreAPI api) {
      State = CreateState(api, SourceLogger);
      AssignStatic()(State);

      Action<S> AssignStatic() => api.Side switch {
        EnumAppSide.Universal => SetGlobalState,
        EnumAppSide.Server => SetGlobalState,
        EnumAppSide.Client => SetGlobalState,
        _ => _ => throw new InvalidOperationException()
      };
    }
    public StatefulMod(ILogger? optionalLogger) {
      SourceLogger = optionalLogger;
    }
    private ILogger? SourceLogger;
    public S? State = (S?)(object?)null;
    private static S? _GlobalState = (S?)(object?)null;
    public static T WithGlobalState<T>(System.Func<S, T> ctor, T defaultT) {
      return _GlobalState != null ? ctor(_GlobalState) : defaultT;
    }
    public static void SetGlobalState(S? instance) {
      if (
        _GlobalState != null 
        && !_GlobalState.IsDisposed()
        && !Object.ReferenceEquals(instance, _GlobalState)
      ) {
        throw new Exception(
          $"Cannot have 2 global states for ${typeof(M).Name}." +
          "A prior global state was not properly disposed, or there are conflicting charlib mobs"
        );
      }
      _GlobalState = instance;
    }
    public static IVsLog Logger => ((IVsLog?)_GlobalState) ?? new VsLog();
    public ExtensibleFormat<FP> Format 
      => ExtensibleFormatImpl.VsExtensibleFormat;
    public ExtensibleLogOutput<EnumLogType>? _LogOutput 
      => ((IVsLog<FP>?)State)?._LogOutput ?? new VsLog(SourceLogger)._LogOutput;

    public override void StartPre(ICoreAPI api)
    {
      AssignState(api);
      base.StartPre(api);
    }

    public override void Dispose()
    {
      State?.Dispose();
      base.Dispose();
    }
  }
}