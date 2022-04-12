using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using System.Linq;
using Vintagestory.API.Server;
using charlib.ip.debug;
using Vintagestory.API.Config;
using System.IO;

namespace charlib
{
  public partial class CharLib : StatefulMod<ICharLibState, CharLib>
  {
    public CharLib() : base(s => new CharLibState(s)) { }
    public static string ChannelKey { get; private set; } = "charlib";
    public override void Start(ICoreAPI api)
    {
      base.Start(api);
      state.OnStart();
      RegisterClasses();
      DoHarmonyPatches();
      ConfigureNetworkCommands();
      LoadSaveData();
      state.OnStartComplete();
      this.Debug($"CharLib finished loading {api.Side}");
    }
    public override bool ShouldLoad(EnumAppSide forSide)
    {
      return true;
    }
    public override double ExecuteOrder()
    {
      return 0.1;
    }
    private void RegisterClasses() {
      var api = state.Api!;
      api.RegisterBlockEntityBehaviorClass(
        nameof(LastPlayerBEB), typeof(LastPlayerBEB)
      );
      api.RegisterEntityBehaviorClass(
        nameof(PlayerDict), typeof(PlayerDict)
      );
    }
    private void DoHarmonyPatches() {
      var harmony = state.Harmony;
      harmony.PatchAll(Assembly.GetExecutingAssembly());
      SmeltingInventoryPatch.Patch(harmony);
      this.Debug(
        "Actually patched methods:\n\t{0}",
        String.Join("\n\t", harmony.GetPatchedMethods()
          .Select(m => String.Format(
            "{0}.{1}(...{2}...)",
            m?.DeclaringType?.Name,
            m?.Name,
            m?.GetParameters()?.Count()
          ))
        )
      );
    }
    public static string SaveFileLocation = Path.Combine(
      GamePaths.Saves,
      nameof(charlib),
      "PlayerData.json"
    );


    private void ConfigureNetworkCommands() {
      var api = state.Api!;
      api.Network.RegisterChannel(CharLib.ChannelKey);
      PlayerDict.RegisterGeneralHandlers(api);
      CharLib.WithServerApi(serverApi => {
        ChatCommands.RegisterCommands(serverApi);
      });
      this.Trace("Registering {0} overrides", state.Overrides.Count);
      state.Overrides.ForEach(o => o.Register(api));
    }
    private void LoadSaveData() {
      CharLib.WithServerApi(serverApi => {
        state.SaveData = new SaveData(serverApi, CharLib.SaveFileLocation);
        state.SaveData.Register();
      });
    }
  }
}