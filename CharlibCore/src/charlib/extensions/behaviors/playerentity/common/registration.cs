using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;

namespace charlib {
  public partial class PlayerDict : PlayerData<PlayerDict> {
    public static void RegisterHandler<T>(
      ICoreAPI api
    ) where T : class {
      switch(api.Side) {
        case EnumAppSide.Client: 
          RegisterHandler<T>(
            (api as ICoreClientAPI)!
          ); return;
        case EnumAppSide.Server: 
          RegisterHandler<T>(
            (api as ICoreServerAPI)!
          ); return;
        default:
          return;
      }
    }
    internal static void RegisterGeneralHandlers(
      ICoreAPI api
    ) {
      ICoreClientAPI? clientAPI = api as ICoreClientAPI;
      ICoreServerAPI? serverAPI = api as ICoreServerAPI;
      if (clientAPI != null) {
        RegisterGeneralHandlers(clientAPI);
      }
      if (serverAPI != null) {
        RegisterGeneralHandlers(serverAPI);
      }
    }
    public static void OnPlayerJoinRegisterBehavior(
      IPlayer player
    ) {
      if (player.Entity.GetBehavior<PlayerDict>() == null) {
        player.Entity.AddBehavior(new PlayerDict(player, shouldRegister: true));
      }
    }
    internal static void RegisterGeneralHandlers(
      ICoreClientAPI clientAPI
    ) {
      clientAPI.Event.PlayerJoin += OnPlayerJoinRegisterBehavior;
    }
    internal static void RegisterGeneralHandlers(
      ICoreServerAPI api
    ) {
      api.Event.PlayerJoin += OnPlayerJoinRegisterBehavior;
    }
    private static void RegisterHandler<T>(
      ICoreServerAPI serverAPI
    ) where T : class {
      serverAPI.Network.GetChannel(CharLib.ChannelKey).RegisterMessageType<T>();
      serverAPI.Network.GetChannel(CharLib.ChannelKey).SetMessageHandler<T>(
        (player, t) => {
          player.Entity.GetBehavior<PlayerDict>()?.Set<T>(t);
        }
      );
    }
    private static void RegisterHandler<T>(
      ICoreClientAPI clientAPI
    ) where T : class {
      clientAPI.Network.GetChannel(CharLib.ChannelKey).RegisterMessageType<T>();
      clientAPI.Network.GetChannel(CharLib.ChannelKey).SetMessageHandler<T>(
        t => {
          if (clientAPI.World.Player != null) {
            clientAPI.World.Player.Entity.GetBehavior<PlayerDict>()?.Set<T>(t);
          } else {
            CharLib.Instance.Warning(
              "No client player registered, ignoring server player update"
            );
          }
        });
    }
    public void SendUpdate<T>() where T : class {
      switch(entity.Api.Side) {
        case EnumAppSide.Client: 
          SendUpdate<T>(
            (entity.Api as ICoreClientAPI)!
          ); return;
        case EnumAppSide.Server: 
          SendUpdate<T>(
            (entity.Api as ICoreServerAPI)!, 
            (Player as IServerPlayer)!
          ); return;
        default:
          return;
      }
    }
    public void SendUpdate<T>(
      ICoreClientAPI clientAPI
    ) where T : class {
      clientAPI.Network.GetChannel(CharLib.ChannelKey).SendPacket<T>(
        Get<T>()!
      );
    }
    public void SendUpdate<T>(
      ICoreServerAPI serverAPI, 
      IServerPlayer player
    ) where T : class {
      serverAPI.Network.GetChannel(CharLib.ChannelKey).SendPacket<T>(
        Get<T>()!,
        player
      );
    }
  }
}