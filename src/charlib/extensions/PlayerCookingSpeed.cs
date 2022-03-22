using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace charlib {
  public class PlayerCookingSpeed {
    public float cookingSpeedMulti = 1.0f;
    public static void Initialize() {
      CharLib.Trace("Adding BlockEntityFirepit.MaxCookingTime delegate extension");
      DelegateExt.AddMethod<Delegates.MaxCookingTime>(
        typeof(BlockEntityFirepit),
        MethodId.MaxCookingTime,
        PlayerCookingSpeedDelegates.FirepitMaxCookingTime
      );
    }
  }
  public static class PlayerCookingSpeedExtension {
    public static PlayerCookingSpeed? GetCookingSpeed(this IPlayer player) {
      object objPlayer = (object)player;
      return InstanceExt.GetExtension<PlayerCookingSpeed>(ref objPlayer);
    }
  }
  public static class PlayerCookingSpeedDelegates {

    public static void FirepitMaxCookingTime(
      ref object __instance,
      ref float __result
    ) {
      BlockEntityFirepit? firepit = __instance as BlockEntityFirepit;
      LastModifyingPlayer? player = InstanceExt.
        GetExtension<LastModifyingPlayer>(ref __instance);
      if (player == null) {
        // CharLib.Trace("FirepitMaxCookingTime#LastModifyingPlayer = null. Skipping");
        return;
      }
      object? playerObj = player?.GetPlayer();
      if (playerObj == null) {
        CharLib.Trace(
          "FirepitMaxCookingTime#LastModifyingPlayer = {0} but unable to find player object. Skipping",
          player!.PlayerId
        );
        return;
      }
      PlayerCookingSpeed? speed = InstanceExt
        .GetExtension<PlayerCookingSpeed>(ref playerObj);
      if (speed == null) {
        CharLib.Trace("FirepitMaxCookingTime#speed = null. {0}", () =>
          new object?[] { 
            InstanceExt.ObjectTable
          });
        return;
      }
      float originalSpeed = __result;
      __result = __result * speed.cookingSpeedMulti;
      CharLib.Trace(
        "Updated cooking speed from {0} to {1} @{2}", 
        originalSpeed, 
        __result,
        firepit?.Pos
      );
    }
    // public static void OnItemSlotModified(
    //   ref object __instance,
    //   ItemSlot slot
    // ) {
    // }
    // public static void OnPlayerChange(
    //   ref object source, 
    //   PlayerRef before, 
    //   PlayerRef after
    // ) {

    // }

  }

}