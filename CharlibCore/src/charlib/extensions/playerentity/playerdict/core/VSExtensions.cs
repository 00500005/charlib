using Charlib.PlayerDict;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Charlib {
  public static partial class VSExtensions {
    public static IPlayerDict EnsurePlayerDict(
      this IPlayer player,
      IPlayerDictTypeKeyRegistry registry
    ) {
      IPlayerDict? pd = player.Entity.GetBehavior<PlayerDictEntity>();
      if (pd == null) {
        pd = PlayerDictFacade.CreateAndRegister(player, registry);
      }
      return pd;
    }
    public static IPlayerDict? GetPlayerDict(
      this IPlayer player
    ) {
      return player.Entity.GetBehavior<PlayerDictEntity>();
    }
    public static LastPlayerBEB? GetLastPlayerBEB(
      this BlockEntity blockEntity
    ) {
      return blockEntity.GetBehavior<LastPlayerBEB>();
    }
    public static LastPlayerBEB EnsureLastPlayerBEB(
      this BlockEntity blockEntity
    ) {
      LastPlayerBEB? beb = blockEntity.GetBehavior<LastPlayerBEB>();
      if (beb == null) {
        beb = new LastPlayerBEB(blockEntity);
        blockEntity.Behaviors.Add(beb);
      }
      return beb;
    }
  }
}