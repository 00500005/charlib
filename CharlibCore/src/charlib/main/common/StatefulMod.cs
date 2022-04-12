
using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using System.Linq;
using Vintagestory.API.Server;
using charlib.ip.debug;
using charlib.ip;

namespace charlib
{
  public enum InitializationStage {
    // Sorted in expected order
    Construct,
    PreStart,
    Starting,
    Started,
    Disposed,
  }
  public static class InitializationStageExt {
    public static string Name(this InitializationStage s) {
      return Enum.GetName(typeof(InitializationStage), s);
    }
  }
  public class LoadOrderException : Exception {
    public LoadOrderException(string message) : base(message) { }
  }
  public class UnexpectedUsage : Exception {
    public UnexpectedUsage(string message) : base(message) { }
  }
  public interface IModState<S,M>
    where S : IModState<S,M>
    where M : StatefulMod<S,M>
  {
    M Instance {get;}
    ICoreAPI? Api {get;set;}
    InitializationStage Stage {get;set;}
  }
  public abstract class ModState<S, M> : IModState<S, M>
    where S : IModState<S, M>
    where M : StatefulMod<S, M>
  {
    public virtual M Instance {get;}
    protected ModState(M mod) {
     this.Instance = mod;
    }
    public virtual ICoreAPI? Api {get;set;} = null;
    public virtual InitializationStage Stage {get;set;}
      = InitializationStage.Construct;
  }

  public interface IModServerState<S,M> 
    : IModState<S,M>
    where S : IModState<S,M>
    where M : StatefulMod<S,M> {
  }
  public interface IModClientState<S,M>
    : IModState<S,M>
    where S : IModState<S,M>
    where M : StatefulMod<S,M> {
  }
  public interface IModUniversalState<S,M>
    : IModState<S,M>, IModClientState<S,M>, IModServerState<S,M>
    where S : IModState<S,M>
    where M : StatefulMod<S,M> {
  }

  public static class ModStateExt {
    public static M Unwrap<S,M>(
      this StatefulMod<S,M> mod
    ) where S : IModState<S,M>
      where M : StatefulMod<S,M> {
      return (M)mod;
    }
    public static S Unwrap<S,M>(
      this IModState<S,M> state
    ) where S : IModState<S,M>
      where M : StatefulMod<S,M> {
      return (S)state;
    }
    public static EnumAppSide Side<S,M>(
      this IModState<S,M> state
    ) where S : IModState<S,M>
      where M : StatefulMod<S,M> {
      return state?.Api?.Side ?? (EnumAppSide)0;
    }
    public static ICoreClientAPI ClientApi<S,M>(
      this IModClientState<S,M> state
    ) where S : IModState<S,M>
      where M : StatefulMod<S,M> {
      return (state.Api as ICoreClientAPI)!;
    }
    public static ICoreServerAPI ServerApi<S,M>(
      this IModServerState<S,M> state
    ) where S : IModState<S,M>
      where M : StatefulMod<S,M> {
      return (state.Api as ICoreServerAPI)!;
    }
    private static void Expect<S,M>(
      this IModState<S,M> state,
      bool condition,
      System.Func<string> message
    ) where S : IModState<S,M>
      where M : StatefulMod<S,M> 
    {
      if(!condition) {
        state.Expect(false, message());
      }
    }
    private static void Expect<S,M>(
      this IModState<S,M> state,
      bool condition,
      string message
    ) where S : IModState<S,M>
      where M : StatefulMod<S,M> 
    {
      if(!condition) {
        throw new LoadOrderException(
          $"[{typeof(S).Name}]@[{state.Stage.Name()}] {message}"
        );
      }
    }
    public static void Expect<S,M>(
      this IModState<S,M> state, 
      InitializationStage stage
    ) where S : IModState<S,M>
      where M : StatefulMod<S,M> 
    {
      state.Expect(state.Stage == stage, $"Expected {stage.Name()}");
    }
    public static void OnPreStart<S,M>(
      this IModState<S,M> state, 
      ICoreAPI api
    ) where S : IModState<S,M>
      where M : StatefulMod<S,M> {
      state.Expect(InitializationStage.Construct);
      state.Expect(state.Api == null, "already had an api assigned");
      state.Api = api;
      state.Stage = InitializationStage.PreStart;
    }
    public static void OnStart<S,M>(
      this IModState<S,M> state
    ) where S : IModState<S,M>
      where M : StatefulMod<S,M> {
      state.Expect(InitializationStage.PreStart);
      state.Stage = InitializationStage.Starting;
    }
    public static void OnStartComplete<S,M>(
      this IModState<S,M> state
    ) where S : IModState<S,M>
      where M : StatefulMod<S,M> {
      state.Expect(InitializationStage.Starting);
      state.Stage = InitializationStage.Started;
    }
    public static void OnDispose<S,M>(
      this IModState<S,M> state 
    ) where S : IModState<S,M>
      where M : StatefulMod<S,M> {
      state.Expect(InitializationStage.Started);
      state.Stage = InitializationStage.Disposed;
    }
  }
  public abstract class StatefulMod<S,M> 
    : ModSystem, ILoggable
    where S : IModState<S,M>
    where M : StatefulMod<S,M>
  {
    public StatefulMod(System.Func<M,S> stateCtor) {
      this.state = stateCtor((M)this);
    }
    public StatefulMod(S state) {
      this.state = state;
    }
    protected S state;
    public static IModUniversalState<S, M>? universalInstance;
    public static IModClientState<S, M>? clientInstance;
    public static IModServerState<S, M>? serverInstance;
    public static void WithServerApi(Action<ICoreServerAPI> fn) {
      if (ServerState?.ServerApi() != null) {
        fn(ServerState.ServerApi());
      }
    }
    public static void WithClientApi(Action<ICoreClientAPI> fn) {
      if (ClientState?.ClientApi() != null) {
        fn(ClientState.ClientApi());
      }
    }
    public static void setUniversalInstance(IModState<S,M> instance) {
      if (universalInstance != null && universalInstance != instance) {
        universalInstance.Expect(
          InitializationStage.Disposed
        );
      }
      universalInstance = (IModUniversalState<S, M>)instance;
    }
    public static void setClientInstance(IModState<S,M> instance) {
      if (clientInstance != null && clientInstance != instance) {
        clientInstance.Expect(
          InitializationStage.Disposed
        );
      }
      clientInstance = (IModClientState<S, M>)instance;
    }
    public static void setServerInstance(IModState<S,M> instance) {
      if (serverInstance != null && serverInstance != instance) {
        serverInstance.Expect(
          InitializationStage.Disposed
        );
      }
      serverInstance = (IModServerState<S, M>)instance;
    }
    public static IModServerState<S,M>? ServerState { get {
      return new IModServerState<S, M>?[]{
        universalInstance,
        serverInstance
      }.Where(s => s != null).First();
    }}
    public static IModClientState<S,M>? ClientState {get {
      return new IModClientState<S, M>?[]{
        universalInstance,
        clientInstance
      }.Where(s => s != null).First();
    } }
    public static S State {get {
      S? instance = new S?[]{
        (S?)universalInstance,
        (S?)serverInstance,
        (S?)clientInstance,
      }.Where(s => s != null).First();
      if (instance == null) {
        throw new LoadOrderException(
          $"Attempting to access mod {typeof(S).Name} before it was ever constructed"
        );
      }
      return instance;
    }}
    public static M Instance => State.Instance;
    public ILogger logger => Mod.Logger;
    public Tracer TraceF => 
      this.WithPrefix($"[{typeof(S).Name}]@[{state.Side()}]");

    public override void StartPre(ICoreAPI api)
    {
      base.StartPre(api);
      state.OnPreStart(api);
      AssignStatic()(state);

      Action<IModState<S,M>> AssignStatic() => api.Side switch {
        EnumAppSide.Universal => setUniversalInstance,
        EnumAppSide.Server => setServerInstance,
        EnumAppSide.Client => setClientInstance,
        _ => _ => throw new InvalidOperationException()
      };
    }

    public override void Dispose()
    {
      base.Dispose();
      state.OnDispose();
    }
  }
}