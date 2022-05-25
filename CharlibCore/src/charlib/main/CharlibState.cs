
using HarmonyLib;
using Vintagestory.API.Common;
using Charlib.PatchChain;
using Charlib.PlayerDict;

namespace Charlib
{
  // Use an interface for mocking (in tests)
  public interface ICharlibState 
    : IModState<ICharlibState, CharlibMod, VsLogParams> 
  {
    public Harmony Harmony {get;}
    public IStringConstructorRegistry StringConstructorRegistry {get;}
    public IPatchChainRegistry PatchChainRegistry {get;}
    public IPlayerDictManager PlayerDictManager {get;}
    public ServerCommands? ServerCommands {get;set;}
    public PlayerSaveData? SaveData {get;set;}
  }
  public static class ICharlibStateExt {
    public static int RegisterPatchOverrides(
      this ICharlibState state
    ) {
      int count = 0;
      foreach(
        var key
        in state.PatchChainRegistry.GetPatchOverrideTypeKeys()
      ) {
        key.Register(
          state.PlayerDictManager,
          state.PatchChainRegistry
        );
        count++;
      }
      return count;
    }
  }
  public interface IHasCharlibState : IHasGlobalContext<
    ICharlibState
  > {
    public class Impl : IHasCharlibState {
      public ICharlibState State {get;} 
      public Impl(ICharlibState state) {
        this.State = state;
      }
    }
  }
  public class CharlibState : 
    ModState<ICharlibState,CharlibMod, VsLogParams>, 
    IVsLog,
    ICharlibState
  {
    public CharlibState(
      CharlibMod charlib
      , ICoreAPI api
      , ILogger? useLogger = null
    ) : base(charlib, api, useLogger) {
      Harmony = new Harmony(CharlibMod.ModID);
      PlayerDictManager = PlayerDictFacade.CreateManager(this);
      StringConstructorRegistry
        .Register<float>(StringConstructorMethods.FloatFromString)
        .Register<int>(StringConstructorMethods.IntFromString);
    }
    public Harmony Harmony {get;}
    public IStringConstructorRegistry StringConstructorRegistry {get;}
      = new IStringConstructorRegistryImpl.Impl();
    public IPatchChainRegistry PatchChainRegistry {get; private set;} 
      = new IPatchChainRegistryImpl.Impl();
    public IPlayerDictManager PlayerDictManager {get;}
    public ServerCommands? ServerCommands {get;set;}
    public PlayerSaveData? SaveData {get;set;}
    public override void Dispose() {
      base.Dispose();
      PlayerDictManager.Dispose();
      // ChainRegistry.Dispose();
      ServerCommands?.Dispose();
      // SaveData?.Dispose();
    }
  }
}