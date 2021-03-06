using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Charlib.PatchChain {
  public interface IHasPlayer {
    public IPlayer? Player {get;}
  }
  public interface IHasBlockEntity {
    public BlockEntity? Entity {get;}
  }
  public sealed class PlayerAndBlockEntity : IHasPlayer, IHasBlockEntity {
    public IPlayer? Player {get;set;} = null;
    public BlockEntity? Entity {get;set;} = null;
    public static PlayerAndBlockEntity FromInventory(
      InventoryBase? instance,
      System.Func<BlockEntity, IPlayer?> getPlayer
    ) {
      BlockPos? pos = instance?.Pos;
      if (pos != null) {
        return FromBlockEntity(
          instance?.Api?.World?.BlockAccessor?.GetBlockEntity(pos),
          getPlayer
        );
      }
      return new PlayerAndBlockEntity();
    }
    public static PlayerAndBlockEntity FromBlockEntity(
      BlockEntity? instance,
      System.Func<BlockEntity, IPlayer?> getPlayer
    ) {
      LastPlayerBEB? beb = instance?.GetBehavior<LastPlayerBEB>();
      IPlayer? player = instance == null ? null : getPlayer(instance);
      return new PlayerAndBlockEntity {
        Player = player,
        Entity = instance
      };
    }
  } 
}