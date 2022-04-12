
using HarmonyLib;
using Vintagestory.API.Common;

namespace charlib {
	[HarmonyPatch(typeof(InventoryBase))]
	public static class InventoryBasePatch
	{
		[HarmonyPatch(nameof(InventoryBase.Open))]
		[HarmonyPrefix]
    public static void Open(
      ref InventoryBase __instance,
      IPlayer player,
      ref object __result
    ) {
      BlockEntity? be = __instance?.Pos != null
        ? __instance?.Api?.World
          ?.BlockAccessor?.GetBlockEntity(__instance?.Pos)
        : null;
      LastPlayerBEB? beb = be
        ?.GetBehavior<LastPlayerBEB>();
      // if (be != null) {
      //   if (beb != null) {
      //     CharLib.Trace("Open LastPlayer enabled inventory");
      //   } else {
      //     CharLib.Trace("Open LastPlayer missing");
      //   }
      // } else {
      //   CharLib.Trace("no BlockEntity found for inventory: {0} @{1}",
      //     __instance?.GetType(),
      //     __instance?.Pos
      //   );
      // }
      beb
        ?.OnAccess(player);
    }
		[HarmonyPatch(nameof(InventoryBase.ActivateSlot))]
		[HarmonyPrefix]
		public static void ActivateSlot(
      ref InventoryBase __instance,
      int slotId, 
      ItemSlot sourceSlot, 
      ref ItemStackMoveOperation op,
      ref object __result
    ) {
      BlockEntity? be = __instance?.Pos != null
        ? __instance?.Api?.World
          ?.BlockAccessor?.GetBlockEntity(__instance?.Pos)
        : null;
      LastPlayerBEB? beb = be
        ?.GetBehavior<LastPlayerBEB>();
      // if (be != null) {
      //   if (beb != null) {
      //     CharLib.Trace("Activate LastPlayer enabled inventory");
      //   } else {
      //     CharLib.Trace("Activate LastPlayer missing");
      //   }
      // } else {
      //   CharLib.Trace("no BlockEntity found for inventory: {0} @{1}",
      //     __instance?.GetType(),
      //     __instance?.Pos
      //   );
      // }
      beb
        ?.OnModify(op.ActingPlayer);
    }
  }
}