
using HarmonyLib;
using Vintagestory.API.Common;

namespace Charlib {
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
      __instance.GetBlockEntity()
        ?.EnsureLastPlayerBEB()
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
      __instance.GetBlockEntity()
        ?.EnsureLastPlayerBEB()
        ?.OnModify(op.ActingPlayer);
    }
  }
}