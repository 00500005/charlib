using System;
using System.Reflection;
using Vintagestory.API.Common;
using System.Linq;
using Vintagestory.API.Config;
using System.IO;

namespace Charlib
{
  using Charlib.PatchChain;
  using Charlib.PatchChain.Key;
  using Charlib.PatchChain.Override;
  using Charlib.PlayerDict.Reducer;
  using PlayerDict;
  public class CharlibMod 
    : StatefulMod<ICharlibState, CharlibMod, VsLogParams>
    , IVsLog
  {
    public CharlibMod() : base(null) {}
    public CharlibMod(ILogger useLogger) : base(useLogger) {}
    public const string ModID = "charlibcore";
    public static string ChannelKey { get; } = ModID;
    public override void Start(ICoreAPI api)
    {
      base.Start(api);
      State!.OnStart();
      RegisterClasses();
      DoHarmonyPatches();
      ConfigureNetworkCommands();
      LoadSaveData();
      State!.OnStartComplete();
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
      var api = State!.Api;
      api.RegisterBlockEntityBehaviorClass(
        nameof(LastPlayerBEB), typeof(LastPlayerBEB)
      );
      api.RegisterEntityBehaviorClass(
        PlayerDictEntity.VSKey, typeof(PlayerDictEntity)
      );
      var channel = api.Network.RegisterChannel(ChannelKey);
      channel.RegisterMessageType<PlayerDictSyncRequest>();
      channel.RegisterMessageType<PlayerDictSerialized>();
      channel.RegisterMessageType<PlayerDictReducerMessage>();
      State.PatchChainRegistry.Declare(FirepitBonusFoodChance.TypeId);
      State.PatchChainRegistry.Declare(FirepitBurnChance.TypeId);
      State.PatchChainRegistry.Declare(FirepitCookingPotCapacityLiters.TypeId);
      State.PatchChainRegistry.Declare(FirepitCookingPotStackSize.TypeId);
      State.PatchChainRegistry.Declare(FirepitCookingTime.TypeId);

      State.PatchChainRegistry.Declare(OvenBonusFoodChance.TypeId);
      State.PatchChainRegistry.Declare(OvenBurnChance.TypeId);
      State.PatchChainRegistry.Declare(OvenCookingTime.TypeId);
      var declareCount = State.PatchChainRegistry.GetPatchChains().Count();
      Logger.Debug("Declared {0} patch keys", declareCount);
      Logger.Debug("Registered {0} float patch keys"
        , State.RegisterPatchOverrides<float>());
      Logger.Debug("Registered {0} int patch keys"
        , State.RegisterPatchOverrides<int>());
    }
    private void DoHarmonyPatches() {
      var harmony = State!.Harmony;
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
    private static string SaveFileLocation = Path.Combine(
      GamePaths.Saves,
      nameof(CharlibMod),
      "PlayerData.json"
    );
    private void ConfigureNetworkCommands() {
      var api = State!.Api;
      api.Network.RegisterChannel(CharlibMod.ChannelKey);
      State.PlayerDictManager.Register();
      State.WithServerApi(serverApi => {
        State.ServerCommands = new ServerCommands(State, serverApi);
        State.ServerCommands.Register();
      });
    }
    private void LoadSaveData() {
      State!.WithServerApi(serverApi => {
        State!.SaveData = new PlayerSaveData(
          State,
          serverApi, 
          CharlibMod.SaveFileLocation
        );
        State.SaveData.Register();
      });
    }

    public override ICharlibState CreateState(
      ICoreAPI api, ILogger? logger = null
    ) {
      return new CharlibState(this, api, logger);
    }
  }
}