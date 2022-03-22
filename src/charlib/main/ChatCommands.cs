using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace charlib {
  public static class ChatCommands {
    public static void RegisterCommands(
      ICoreServerAPI api
    ) {
			IServerNetworkChannel channel = api.Network.RegisterChannel("charlib");
      api.RegisterCommand(
        "char",
        "commands to debug charlib",
        OnDebugSetUsage,
        OnDebugSet
        // Privilege.commandplayer
      );

    }
    public static string OnDebugSetUsage = "[set] [maxcookingtime] <float>";
    public static Dictionary<string, ServerChatCommandDelegate> OnDebugSubcommands = new Dictionary<string, ServerChatCommandDelegate>{
      { "set", OnDebugSetSet }
    };
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
      }
      CharLib.Trace("Running OnDebugSet command {0}", cmd);
      OnDebugSubcommands[cmd](player, groupId, args);
    }
    public static void OnDebugSetSet(
      IServerPlayer player, 
      int groupId, 
      CmdArgs args
    ) {
      string targetAttr = args.PopWord().ToLower();
      object playerObj = (object)player;
      switch(targetAttr) {
        case "maxcookingtime":
          string literalAttrValue = args[0];
          CharLib.Trace("Setting {0} to {1}", targetAttr, literalAttrValue);
          PlayerCookingSpeed cookingSpeed = InstanceExt.EnsureExtension(
            ref playerObj,
            () => new PlayerCookingSpeed()
          );
          float oldAttrValue = cookingSpeed.cookingSpeedMulti;
          CharLib.Trace("Got existing cooking speed = {0}", oldAttrValue);
          float? attrValue = args.PopFloat();
          CharLib.Trace("attrValue = {0}", attrValue);
          if (attrValue == null) {
            player.SendMessage(
              0, $"Unable to update {targetAttr} to {literalAttrValue} (please enter a valid float)",
              EnumChatType.CommandError
            );
            return;
          }
          cookingSpeed.cookingSpeedMulti = (float)attrValue;
          string successMsg = 
            $"Successfully updated {targetAttr} = {attrValue} (was {oldAttrValue})";
          player.SendMessage(
            0, successMsg,
            EnumChatType.CommandSuccess
          );
          CharLib.Trace(successMsg);
          return;
        default:
          CharLib.Trace("Unknown attribute {0}", targetAttr);
          player.SendMessage(
            0, $"Unknown attribute {targetAttr}", 
            EnumChatType.CommandError
          );
          return;
      }
    }
  }

}