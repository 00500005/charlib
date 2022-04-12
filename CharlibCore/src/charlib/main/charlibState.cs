
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
  // Use an interface for mocking (in tests)
  public interface ICharLibState : IModState<ICharLibState, CharLib> {
    public Harmony Harmony {get;}
    public List<IStatOverride> Overrides {get;}
    public List<IStatOverrideKind<int>> IntOverrides {get;}
    public List<IStatOverrideKind<float>> FloatOverrides {get;}
    public ChainRegistry ChainRegistry {get;}
    public SaveData? SaveData {get;set;}
  }
  public class CharLibState : 
    ModState<ICharLibState,CharLib>, 
    ICharLibState
  {
    public CharLibState(CharLib charlib) : base(charlib) {
      Harmony = new Harmony(charlib.Mod.Info.ModID);
      Overrides.AddRange(IntOverrides);
      Overrides.AddRange(FloatOverrides);
    }
    public Harmony Harmony {get;}
    public List<IStatOverride> Overrides {get;} = new List<IStatOverride>();
    public List<IStatOverrideKind<int>> IntOverrides {get;}
    = new List<IStatOverrideKind<int>>{
      new ip.debug.cooking.FirepitCookingPotStackSizeOverride(),
    };
    public List<IStatOverrideKind<float>> FloatOverrides {get;}
    = new List<IStatOverrideKind<float>>{
      new ip.debug.cooking.FirepitBonusFoodChanceOverride(),
      new ip.debug.cooking.OvenBonusFoodChanceOverride(),
      new ip.debug.cooking.FirepitBurnChanceOverride(),
      new ip.debug.cooking.OvenBurnChanceOverride(),
      new ip.debug.cooking.FirepitCookingPotCapacityLitersOverride(),
      new ip.debug.cooking.FirepitCookingTimeOverride(),
      new ip.debug.cooking.OvenCookingTimeOverride(),
    };
    public ChainRegistry ChainRegistry {get; private set;} 
      = new ChainRegistry();
    public SaveData? SaveData {get;set;}
  }
}