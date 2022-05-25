using System;
using Vintagestory.API.Common;

namespace Charlib.PlayerDict {
  public static class PlayerDictFacade {
    public static IPlayerDictManager CreateManager(
      ICharlibState state
    ) {
      switch(state.Side()) {
        case EnumAppSide.Client:
          return new IPlayerDictManagerImpl.ClientImpl(
            state, state.ClientApi()!);
        case EnumAppSide.Server:
          return new IPlayerDictManagerImpl.ServerImpl(
            state, state.ServerApi()!);
        case EnumAppSide.Universal:
          return new IPlayerDictManagerImpl.UniversalImpl(
            state, state.ServerApi()!, state.ClientApi()!);
        default:
          throw new NotImplementedException();
      }
    }
    public static IPlayerDictTypeKey<V> CreateTypeKey<V>(
      IPlayerDictTypeKeyRegistry reg,
      string? id = null,
      IDiscriminator<V>? _ = null
    ) {
      return IPlayerDictTypeKeyImpl.Create(id, _);
    }

    public static PlayerDictEntity CreateAndRegisterPlayerDict(
      IPlayer player,
      IPlayerDictTypeKeyRegistry registry
    ) {
      return IPlayerDictImpl.CreateAndRegister(player, registry);
    }
  }
}