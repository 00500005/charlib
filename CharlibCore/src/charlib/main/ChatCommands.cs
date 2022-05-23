using System;
using System.Linq;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Charlib.PatchChain.Override;

namespace Charlib {
  using PlayerDict;
  using CmdDispatch = Dictionary<string, ServerChatCommandDelegate>;
  // using DebugSetDef = List<KeyValuePair<string, ChatCommands.DebugSet>>;
  public static class OverrideKindExt {
    public static void AssignValue(
      this IPatchOverrideTypeKey stat,
      IStringConstructorRegistry reg,
      IPlayerDictManager manager,
      IServerPlayer player,
      string rawValue,
      Action<string?>? callback = null
    ) {
      var def = manager.ReducerRegistry.Get(stat.Id);
      CharlibMod.Logger.Debug(
        "Setting {0}[{1}] = {2}", stat.Id, stat.ValueType.FullName, rawValue
      );
      manager.ApplyNonGeneric(
        player,
        def.ReducerKey,
        stat.ValueFromString(reg, rawValue),
        callback != null 
          ?  (pd, _) => callback(pd.Get(stat.InferDictKey().Id)?.ToString())
          : null
      );
    }
  }
  public class ServerCommands 
    : IHasCharlibState.Impl, IHasServerApi, IDisposable {
    public ServerCommands(ICharlibState state, ICoreServerAPI serverAPI) 
    : base(state) {
      ServerAPI = serverAPI;
      PatchOverrides = state.GetAllSupportedPatchOverrideKeys()
        .ToDictionary(
          d => d.Id.ToLower(), d => d
        );
      OnDebugSetUsage = $@"""[set] [{
        String.Join("\n | ", PatchOverrides.Select(p => p.Key))
      }] <val>""";
      OnDebugSubcommands = new CmdDispatch{
        { "set", OnDebugSetSet }
      };
    }
    public Dictionary<string, IPatchOverrideTypeKey> PatchOverrides {get;}
    public ICoreServerAPI ServerAPI {get;}
    public CmdDispatch OnDebugSubcommands {get;}
    public string OnDebugSetUsage {get;}
    public void Register() {
      ServerAPI.RegisterCommand(
        "char",
        "commands to debug charlib",
        OnDebugSetUsage,
        OnDebugSet
        // Privilege.commandplayer
      );
    }
    public void Dispose() {
      // TODO
    }
    public void OnDebugSet(
      IServerPlayer player, 
      int groupId, 
      CmdArgs args
    ) {
      if (args.Length < 1) {
        player.SendMessage(groupId, OnDebugSetUsage, EnumChatType.CommandError);
        return;
      }
      string cmd = args.PopWord("").ToLower();
      if (!OnDebugSubcommands.ContainsKey(cmd)) {
        player.SendMessage(0, $"Unknown command '{cmd}'", 
          EnumChatType.CommandError);
        player.SendMessage(0, $@"Supported commands are: '{
          OnDebugSubcommands.Keys
        }'", EnumChatType.CommandError);
        return;
      }
      State.Debug("Running OnDebugSet command {0}", cmd);
      OnDebugSubcommands[cmd](player, groupId, args);
    }
    public void DoSet(
      IPatchOverrideTypeKey patchOverride,
      string attr, 
      IServerPlayer player, 
      int groupId, 
      string rawValue
    ) {
      try {
        var oldVal = player.EnsurePlayerDict(
          State.PlayerDictManager.DictKeyRegistry
        ).Get(patchOverride.Id)?.ToString();

        patchOverride.AssignValue(
          State.StringConstructorRegistry, 
          State.PlayerDictManager, 
          player, 
          rawValue,
          val => {
            player.SendMessage(groupId, $@"""
            Successfully set {attr} to {val} (was {oldVal})
            """, EnumChatType.CommandSuccess);
          }
        );
        return;
      } catch (FormatException) {
        player.SendMessage(
          groupId, $"Invalid stat value {rawValue}", 
          EnumChatType.CommandError
        );
        return;
      } catch (Exception e) {
        player.SendMessage(
          groupId, $@"""Failed to set {attr} to value {rawValue}.
          Reason: {e.Message}.
          See logs for more details
          """, 
          EnumChatType.CommandError
        );
        CharlibMod.Logger.Notification("{0}\n{1}", e.Message, e.StackTrace);
        return;
      }
    }
    public void OnDebugSetSet(
      IServerPlayer player, 
      int groupId, 
      CmdArgs args
    ) {
      if (args.Length < 1) {
        player.SendMessage(
          groupId, $"Set requires an attribute to set", 
          EnumChatType.CommandError
        );
        return;
      }
      string attr = args.PopWord("").ToLower();
      string val = args.PopWord("");
      if (!PatchOverrides.ContainsKey(attr)) {
        player.SendMessage(
          groupId, $"Unknown override stat {attr}", 
          EnumChatType.CommandError
        );
        return;
      } 
      DoSet(PatchOverrides[attr], attr, player, groupId, val);
    }
  }
}