using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using System.Linq;
using Vintagestory.API.Server;

namespace charlib
{
  public enum CharLibStage {
    ShouldLoad,
    PreStart,
    Starting,
    Started,
  }
  public class LoadOrderException : Exception
  {
    public LoadOrderException(string message) : base(message)
    {
    }
  }
  public class UnexpectedUsage : Exception {
    public UnexpectedUsage(string message) : base(message) {}
  }
  public class CharLibGlobalState {

    public Dictionary<EnumAppSide, CharLibStage> LoadStages = new Dictionary<EnumAppSide, CharLibStage>{};

    public bool IsLoadComplete() {
      return 
        LoadStages.Count > 0 &&
        LoadStages.All(kv => kv.Value == CharLibStage.Started);
    }
    public ICoreAPI Api {
      get {
        ICoreAPI? client = this.ClientApi;
        ICoreAPI? server = this.ServerAPI;
        ICoreAPI? clientOrServer = client ?? server;
        if (clientOrServer == null) {
          throw new LoadOrderException(
            @"Invalid usage of charlib. We're not yet initialized."
          );
        }
        return clientOrServer;
      } 
    }
    public ICoreClientAPI? ClientApi { get; private set; }
    public ICoreServerAPI? ServerAPI { get; private set; }
    private ILogger? _logger;
    public ILogger Logger { 
      get {
        if (this._logger == null) {
          throw new LoadOrderException(
            @"Invalid usage of charlib. We're not yet initialized."
          );
        }
        return this._logger;
      } 
      private set { this._logger = value; } 
    }
    private Harmony? _harmony;
    public Harmony Harmony { 
      get {
        if (this._harmony == null) {
          throw new LoadOrderException(
            @"Invalid usage of charlib. We're not yet initialized."
          );
        }
        return this._harmony;
      } 
      private set { this._harmony = value; } 
    }

    private ModSystem? _modSystem;
    public ModSystem ModSystem {
      get {
        if (this._modSystem == null) {
          throw new LoadOrderException(
            @"Invalid usage of charlib. We're not yet initialized."
          );
        }
        return this._modSystem;
      } 
      private set { this._modSystem = value; } 
    }
    private Mod? _mod;
    public Mod Mod {
      get {
        if (this._mod == null) {
          throw new LoadOrderException(
            @"Invalid usage of charlib. We're not yet initialized."
          );
        }
        return this._mod;
      } 
      private set { this._mod = value; } 
    }
    internal void OnShouldLoad(
      EnumAppSide forSide,
      CharLib charLib     
    ) {
      updateModInstance(charLib);
      verifyAndUpdatePriorStage(
        forSide,
        null,
        CharLibStage.ShouldLoad,
        true
      );
    }
    internal void OnPreStart(
      ICoreAPI api,
      CharLib charLib     
    ) {
      updateModInstance(charLib);
      verifyAndUpdatePriorStage(
        api.Side, 
        CharLibStage.ShouldLoad, 
        CharLibStage.PreStart, 
        true
      );
      updateApiInstance(api);
    }
    internal void OnStart(
      ICoreAPI api,
      CharLib charLib,
      Harmony harmony
    ) {
      updateModInstance(charLib);
      verifyAndUpdatePriorStage(
        api.Side, 
        CharLibStage.PreStart, 
        CharLibStage.Starting, 
        false
      );
      updateApiInstance(api);
      this.Harmony = harmony;
    }
    internal void OnStartComplete(
      EnumAppSide side
    ) {
      verifyAndUpdatePriorStage(
        side, 
        CharLibStage.Starting, 
        CharLibStage.Started, 
        false
      );
    }

    private void warnIfChanged(
      ILogger logger,
      object? source, 
      object val,
      string msg
    ) {
      if (source != null && source != val) {
        logger.Warning(msg);
      }
    }
    private void updateModInstance(
      CharLib instance
    ) {
      ILogger logger = instance.Mod.Logger;
      warnIfChanged(logger, this._modSystem, instance, 
        $"ModSystem instance has changed during loading. Moving to new instance."
      );
      this.ModSystem = instance;
      warnIfChanged(logger, this._mod, instance.Mod,
        $"Mod instance has changed during loading. Moving to new instance."
      );
      this.Mod = instance.Mod;
      warnIfChanged(logger, this._logger, instance.Mod.Logger,
        $"Logger instance has changed during loading. Moving to new instance."
      );
      this.Logger = logger;
    }
    private void verifyAndUpdatePriorStage(
      EnumAppSide forSide,
      CharLibStage? expectedStage,
      CharLibStage currentStage,
      bool allowMissing
    ) {
      if (this.LoadStages.ContainsKey(forSide)) {
        if (this.LoadStages[forSide] != expectedStage) {
          CharLibStage loadStage = this.LoadStages[forSide];
          CharLib.Log(
            EnumLogType.Warning,
            "{0} invoked while at stage {1}",
            currentStage,
            loadStage
          );
          return;
        }
      } else if (!allowMissing) {
          throw new UnexpectedUsage(
            $"{currentStage} invoked without prior stage"
          );
      }
      this.LoadStages[forSide] = currentStage;
    }
    private void updateApiInstance(ICoreAPI api) {
      switch(api.Side) {
        case EnumAppSide.Client:
          this.ClientApi = api as ICoreClientAPI;
          break;
        case EnumAppSide.Server:
          this.ServerAPI = api as ICoreServerAPI;
          break;
        case EnumAppSide.Universal:
          this.ClientApi = api as ICoreClientAPI;
          this.ServerAPI = api as ICoreServerAPI;
          break;
      }
      switch(api.Side) {
        case EnumAppSide.Client:
          if (this.ClientApi == null) {
            throw new UnexpectedUsage("Invalid client api");
          }
          if (this.ServerAPI != null 
            && this.ServerAPI != this.ClientApi
          ) {
            throw new UnexpectedUsage(
              @"Did not expect to get separate instances of 
              client and server apis"
            );
          }
          break;
        case EnumAppSide.Server:
          if (this.ServerAPI == null) {
            throw new UnexpectedUsage("Invalid client api");
          }
          if (this.ClientApi != null 
            && this.ClientApi != this.ServerAPI
          ) {
            throw new UnexpectedUsage(
              @"Did not expect to get separate instances of 
              client and server apis"
            );
          }
          break;
        case EnumAppSide.Universal:
          if (
            this.ServerAPI == null 
            || this.ClientApi == null
            || this.ClientApi == this.ServerAPI
          ) {
            throw new UnexpectedUsage("Invalid universal api");
          }
          break;
      }
    }
  }
  public class CharLib : ModSystem
  {
    private static CharLibGlobalState _state = new CharLibGlobalState();
    public static bool IsReady {
      get { return _state.IsLoadComplete(); }
    }
    public static CharLibGlobalState State {
      get {
        if (!IsReady) {
          throw new ApplicationException(
            @"CharLib not yet initialized. 
            Unable to access global state.
            Try a later ExecuteOrder, or use CharLib::IsReady to check if state exists."
          );
        }
        return _state;
      }
      private set { _state = value; }
    }

    public override void StartPre(ICoreAPI api)
    {
      base.StartPre(api);
      CharLib._state.OnPreStart(api, this);
    }
    public override void Start(ICoreAPI api)
    {
      base.Start(api);
      Harmony harmony = new Harmony("charlib");
      CharLib._state.OnStart(api, this, harmony);
      DoHarmonyPatches(api, harmony);
      ConfigureNetworkCommands(api);
      RegisterExtensions();
      CharLib._state.OnStartComplete(api.Side);
      _state.Logger.Debug($"CharLib finished loading {api.Side}");
    }
    public override bool ShouldLoad(EnumAppSide forSide)
    {
      CharLib._state.OnShouldLoad(forSide, this);
      return true;
    }
    public override double ExecuteOrder()
    {
      return 0.1;
    }
    private static void RegisterExtensions() {
      Trace("Initializing extensions.");
      PlayerCookingSpeed.Initialize();
      LastAccessingPlayer.Initialize();
      LastModifyingPlayer.Initialize();
      // LastFirepitCookingSlotModifyingPlayer.Initialize();
      Trace("Finished initializing extensions.");
      Trace("Extension lib state =\n\t{0}", () => {
        return new object[]{
          String.Join("\n\t", DelegateExt.ClassRegistry.Select(i => 
            $"{i.Key.Name} = {i.Value.ToString()}"
          ))
        };
      });
      Trace("Extension lib state complete.");
    }
    private static void DoHarmonyPatches(
      ICoreAPI api,
      Harmony harmony
    ) {
      harmony.PatchAll(Assembly.GetExecutingAssembly());
      _state.Logger.Debug(
        "Actually patched methods:\n\t{0}",
        String.Join("\n\t", harmony.GetPatchedMethods()
          .Select(m => String.Format(
            "{0}.{1}(...{2}...)",
            m.DeclaringType.Name,
            m.Name,
            m.GetParameters().Count()
          ))
        )
      );
    }
    private static void WithServerApi(ICoreAPI api, Action<ICoreServerAPI> fn) {
      ICoreServerAPI? serverAPI = api as ICoreServerAPI;
      if (serverAPI != null) {
        fn(serverAPI);
      }
    }
    private static void WithClientApi(ICoreAPI api, Action<ICoreClientAPI> fn) {
      ICoreClientAPI? serverAPI = api as ICoreClientAPI;
      if (serverAPI != null) {
        fn(serverAPI);
      }
    }
    private static void ConfigureNetworkCommands(
      ICoreAPI api
    ) {
      WithServerApi(api, serverApi => {
        ChatCommands.RegisterCommands(serverApi);
      });
    }
    public delegate void Tracer(string format, System.Func<object?[]> getParams);

    public static Tracer TraceF = DoTrace;
    public static void Trace(string format) {
      TraceF(format, () => new object[]{});
    }
    public static void Trace(string format, params object?[] args) {
      TraceF(format, () => args);
    }
    public static void Trace(string format, System.Func<object?[]> getParams) {
      TraceF(format, getParams);
    }

    public static void DoTrace(string format, System.Func<object?[]> getParams) {
      Log(EnumLogType.Notification, format, getParams());
    }
    public static void IgnoreTrace(
      string format, System.Func<object?[]> getParams) {
      // Do nothing
    }


    public static void Log(
      EnumLogType logType, 
      string format, 
      params object?[] args
    ) {
      string msg = String.Format(format, args);
      Log(logType, msg);
    }

    public static void Log(EnumLogType logType, string msg)
    {
      try {
        _state.Logger.Log(logType, msg);
      } catch(Exception e) {
        throw new Exception(
          String.Format("Failed while attempting to log: {0}", msg),
          e
        );
      }
    }
  }
}