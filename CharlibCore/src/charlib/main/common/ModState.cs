
using System;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Charlib
{
  public interface IModState : IDisposable { 
    ICoreAPI Api {get;}
    InitializationStage Stage {get;set;}
  }
  public interface IModState<S> 
    : IDisposable, IModState
    where S : IModState<S> { 
  }
  public interface IModState<S, M> 
    : IDisposable, IModState<S>
    where S : IModState<S, M> 
    where M : IStatefulMod<S, M> { 
    M Instance {get;}
  }
  public interface IModState<S,M,FP> 
    : IVsLog<FP>, IDisposable, IModState<S,M>
    where S : IModState<S,M,FP>
    where M : IStatefulMod<S,M,FP>
    where FP : VsLogParams, IVsLogParams<FP>, new() { }
  public abstract class ModState<S, M, FP> : IModState<S, M, FP>
    where S : IModState<S, M, FP>
    where M : StatefulMod<S, M, FP>
    where FP : VsLogParams, IVsLogParams<FP>, new()
  {
    public virtual M Instance {get;}
    protected ModState(
      M mod,
      ICoreAPI api,
      ILogger? loggerOutput = null,
      ExtensibleFormat<FP>? loggerFormat = null
    ) {
      this.Instance = mod;
      this.Api = api;
      this.Format = loggerFormat ?? ExtensibleFormatImpl.VsExtensibleFormat;
      this._LogOutput = ExtensibleLogOutputAdapters.ILoggerAdapter(
        loggerOutput ?? mod.Mod.Logger
      );
    }
    public virtual ICoreAPI Api {get;}
    public virtual InitializationStage Stage {get;set;}
      = InitializationStage.PreStart;
    public ExtensibleFormat<FP> Format {get;set;}
    public ExtensibleLogOutput<EnumLogType>? _LogOutput {get;set;}

    public virtual void Dispose()
    {
      this.Stage = InitializationStage.Disposed;
    }
  }
  public static class ModStateExt {
    public static bool IsDisposed(
      this IModState state
    ) {
      return state.Stage == InitializationStage.Construct 
        || state.Stage == InitializationStage.Disposed;
    }
    public static EnumAppSide Side(
      this IModState state
    ) {
      // we use 0 as an unknown enum side
      return state?.Api?.Side ?? (EnumAppSide)0;
    }
    public static T? WithClientApi<T>(
      this IModState state,
      System.Func<ICoreClientAPI, T> doFn
    ) {
      ICoreClientAPI? maybeApi = state.Api as ICoreClientAPI;
      return maybeApi == null ? (T?)(object?)null : doFn(maybeApi);
    }
    public static void WithClientApi(
      this IModState state,
      Action<ICoreClientAPI> doFn
    ) {
      ICoreClientAPI? maybeApi = state.Api as ICoreClientAPI;
      if (maybeApi != null) {
        doFn(maybeApi);
      }
    }
    public static void WithServerApi(
      this IModState state,
      Action<ICoreServerAPI> doFn
    ) {
      ICoreServerAPI? maybeApi = state.Api as ICoreServerAPI;
      if (maybeApi != null) {
        doFn(maybeApi);
      }
    }
    public static T? WithServerApi<T>(
      this IModState state,
      System.Func<ICoreServerAPI, T> doFn
    ) {
      ICoreServerAPI? maybeApi = state.Api as ICoreServerAPI;
      return maybeApi == null ? (T?)(object?)null : doFn(maybeApi);
    }
    public static ICoreClientAPI? ClientApi(
      this IModState state
    ) {
      return (state.Api as ICoreClientAPI);
    }
    public static ICoreServerAPI? ServerApi(
      this IModState state
    ) {
      return (state.Api as ICoreServerAPI);
    }
    private static void Expect<S>(
      this IModState<S> state,
      bool condition,
      System.Func<string> message
    ) where S : IModState<S> {
      if(!condition) {
        state.Expect(false, message());
      }
    }
    private static void Expect<S>(
      this IModState<S> state,
      bool condition,
      string message
    ) where S : IModState<S> {
      if(!condition) {
        throw new LoadOrderException(
          $"[{typeof(S).Name}]@[{state.Stage.Name()}] {message}"
        );
      }
    }
    public static void Expect<S>(
      this IModState<S> state,
      params InitializationStage[] expectedAnyOfStage
    ) where S : IModState<S> {
      state.Expect(
        expectedAnyOfStage.Contains(state.Stage), 
        $@"Expected {
          String.Join(", ", expectedAnyOfStage.Select(s => $"'{s.Name()}'"))
        }. Got '{state.Stage}'");
    }
    public static void OnStart<S>(
      this IModState<S> state
    ) where S : IModState<S> {
      state.Expect(InitializationStage.PreStart);
      state.Stage = InitializationStage.Starting;
    }
    public static void OnStartComplete<S>(
      this IModState<S> state
    ) where S : IModState<S> {
      state.Expect(InitializationStage.Starting);
      state.Stage = InitializationStage.Started;
    }
  }
}