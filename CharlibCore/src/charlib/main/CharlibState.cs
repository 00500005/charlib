
using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using System.Linq;
using Vintagestory.API.Server;
using Charlib.PatchChain;
using Charlib.PlayerDict;
using Charlib.PatchChain.Override;

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
    public static IEnumerable<IPatchOverrideTypeKey> 
      GetAllSupportedPatchOverrideKeys(
        this ICharlibState state
      ) 
    {
      return state.StringConstructorRegistry.SupportedTypes().SelectMany(
        t => state.GetPatchOverrideKeys(t)
      );
    }
    public static int RegisterPatchOverrides<V>(
      this ICharlibState state
    ) {
      int count = 0;
      foreach(var key in state.GetPatchOverrideKeys<V>()) {
        PatchOverrideFacade.RegisterOverride(
          key,
          state.PlayerDictManager,
          state.PatchChainRegistry
        );
        count++;
      }
      return count;
    }
    public static IEnumerable<IPatchOverrideTypeKey> 
      GetPatchOverrideKeys(
        this ICharlibState state,
        Type valueType
      ) 
    {
      return state.PatchChainRegistry
        .GetPatchChains()
        .Where(reg => reg.Key.DoesAllowValue(valueType))
        .Select(reg => PatchOverrideFacade.OverrideTypeKeyNonGeneric(reg.Key));
    }
    public static IEnumerable<IPatchOverrideTypeKey<V>> 
      GetPatchOverrideKeys<V>(
        this ICharlibState state
      ) 
    {
      return state.PatchChainRegistry
        .GetPatchChains()
        .Where(reg => reg.Key.DoesAllowValue(typeof(V)))
        .Select(reg => PatchOverrideFacade.OverrideTypeKey<V>(
          (IPatchTypeKey<V>)reg.Key
        ));
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