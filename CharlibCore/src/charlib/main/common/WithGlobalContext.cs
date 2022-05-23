using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Charlib {
  public interface IHasGlobalContext<S>
    where S : IModState<S> {
    S State {get;}
  }
  public interface IHasServerApi {
    ICoreServerAPI ServerAPI {get;}
  }
  public interface IHasClientApi {
    ICoreClientAPI ClientAPI {get;}
  }
  public static partial class IHasExtensions {
    public static IServerNetworkChannel GetCharlibChannel(
      this IHasServerApi self
    ) {
      return self.ServerAPI.Network.GetChannel(CharlibMod.ChannelKey);
    }
    public static IClientNetworkChannel GetCharlibChannel(
      this IHasClientApi self
    ) {
      return self.ClientAPI.Network.GetChannel(CharlibMod.ChannelKey);
    }
    public static IClientPlayer GetPlayer(
      this IHasClientApi self
    ) {
      return self.ClientAPI.World.Player;
    }
  }
}