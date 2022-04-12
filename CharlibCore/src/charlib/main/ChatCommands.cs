using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using charlib.ip.debug.cooking;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace charlib {
  using CmdDispatch = Dictionary<string, ServerChatCommandDelegate>;
  using DebugSetDef = List<KeyValuePair<string, ChatCommands.DebugSet>>;
  public static class ChatCommands {
    public delegate void DebugSet(
      string attribute,
      IServerPlayer player, int groupId, string newValue
    );
    public static DebugSetDef DebugDispatchList = new DebugSetDef {
      SetEntry(new FirepitCookingTimeOverride()),
      SetEntry(new OvenCookingTimeOverride()),
      SetEntry(new FirepitBonusFoodChanceOverride()),
      SetEntry(new OvenBonusFoodChanceOverride()),
      SetEntry(new FirepitBurnChanceOverride()),
      SetEntry(new OvenBurnChanceOverride()),
      SetEntry(new FirepitCookingPotCapacityLitersOverride()),
      SetEntry(new FirepitCookingPotStackSizeOverride()),
    };
    public static CmdDispatch OnDebugSubcommands = new CmdDispatch{
      { "set", OnDebugSetSet }
    };
    public static void RegisterCommands(
      ICoreServerAPI api
    ) {
      api.RegisterCommand(
        "char",
        "commands to debug charlib",
        OnDebugSetUsage,
        OnDebugSet
        // Privilege.commandplayer
      );

    }
    public static void OnDebugSet(
      IServerPlayer player, 
      int groupId, 
      CmdArgs args
    ) {
      if (args.Length <= 0) {
        player.SendMessage(0, OnDebugSetUsage, EnumChatType.CommandError);
        return;
      }
      string cmd = args.PopWord().ToLower();
      if (!OnDebugSubcommands.ContainsKey(cmd)) {
        player.SendMessage(0, $"Unknown command '{cmd}'", 
          EnumChatType.CommandError);
        return;
      }
      CharLib.Instance.Trace("Running OnDebugSet command {0}", cmd);
      OnDebugSubcommands[cmd](player, groupId, args);
    }
    public static DebugSet DoSet<O,K,C,T>(
      System.Func<string, T> valParser,
      ip.debug.StatOverride<O,K,C,T>? discriminator
    ) where O : ip.debug.StatOverride<O,K,C,T>, new()
      where K : ip.IKey<K,C,T>
      where C : context.IHasPlayer { return DoSet<O,K,C,T>(valParser); }
    public static DebugSet DoSet<O,K,C,T>(
      System.Func<string, T> valParser
    ) where O : ip.debug.StatOverride<O,K,C,T>, new()
      where K : ip.IKey<K,C,T>
      where C : context.IHasPlayer
    {
      return (string attr, IServerPlayer player, int groupId, string val) => {
        PlayerDict? pd = player.Entity.GetBehavior<PlayerDict>();
        O? o = pd?.Ensure<O>(() => new O());
        if (o != null) {
          T? oldVal = o.overrideValue;
          try {
            o.overrideValue = valParser(val);
          } catch {
            player.SendMessage(
              groupId, $"Invalid value: {val}",
              EnumChatType.CommandError
            );
            return;
          }
          pd!.SendUpdate<O>();
          string successMsg = 
            $"Successfully updated {attr} = {val} for player {player.PlayerName} (was {oldVal})";
          player.SendMessage(
            groupId, successMsg,
            EnumChatType.CommandSuccess
          );
          CharLib.Instance.Trace(successMsg);
        }
      };
    }
    public static KeyValuePair<string, DebugSet> SetEntry<O,K,C>(
      ip.debug.StatOverride<O,K,C,float>? discriminator
    ) where O : ip.debug.StatOverride<O,K,C,float>, new()
      where K : ip.IKey<K,C,float>
      where C : context.IHasPlayer
    {
      return new KeyValuePair<string, DebugSet>( 
        new O().ShortName().ToLower(), DoSet<O,K,C,float>(Single.Parse)
      );
    }
    public static KeyValuePair<string, DebugSet> SetEntry<O,K,C>(
      ip.debug.StatOverride<O,K,C,int>? discriminator
    ) where O : ip.debug.StatOverride<O,K,C,int>, new()
      where K : ip.IKey<K,C,int>
      where C : context.IHasPlayer
    {
      return new KeyValuePair<string, DebugSet>( 
        new O().ShortName().ToLower(), DoSet<O,K,C,int>(Int32.Parse)
      );
    }


    public static string OnDebugSetUsage = 
      $"[set] [{String.Join("|", DebugDispatchList.Select(p => p.Key))}] <val>";
    public static Dictionary<string, DebugSet> DebugSetDispatch = 
      DebugDispatchList.ToDictionary(d => d.Key, d => d.Value);
    public static void OnDebugSetSet(
      IServerPlayer player, 
      int groupId, 
      CmdArgs args
    ) {
      string attr = args.PopWord().ToLower();
      string val = args.PopWord();
      if (!DebugSetDispatch.ContainsKey(attr)) {
        player.SendMessage(
          groupId, $"Unknown override stat {attr}", 
          EnumChatType.CommandError
        );
      } else {
        DebugSetDispatch[attr](attr, player, groupId, val);
      }
    }
  }

}