using Vintagestory.API.Common;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace Charlib {
  public static partial class VSExtensions {
    public static IClientPlayer GetPlayer(
      this ICoreClientAPI api
    ) {
      return api.World.Player;
    }
    public static BlockEntity? GetBlockEntity(
      this BlockPos? pos,
      ICoreAPI api
    ) {
      return pos == null ? null : api?.World?.BlockAccessor?.GetBlockEntity(pos);
    }
    public static BlockEntity? GetBlockEntity(
      this InventoryBase inv
    ) {
      return inv.Pos?.GetBlockEntity(inv.Api);
    }
  }
}