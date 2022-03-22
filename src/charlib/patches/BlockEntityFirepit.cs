
using System.Collections.Generic;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace charlib {
	[HarmonyPatch(typeof(BlockEntityFirepit))]
	public static class BlockEntityFirepitBasePatch
	{
		[HarmonyPatch(nameof(BlockEntityFirepit.FromTreeAttributes))]
		[HarmonyPostfix]
		public static void FromTreeAttributes(
      ref object __instance, 
      ITreeAttribute tree
    ) {
      List<Delegates.FromTreeAttributes> delegates = DelegateExt.GetAllInheriting<Delegates.FromTreeAttributes>(
        typeof(BlockEntityFirepit), MethodId.FromTreeAttributes
      );
      foreach(Delegates.FromTreeAttributes fn in delegates) {
        fn.Invoke(ref __instance, tree);
      }
    }

		[HarmonyPatch(nameof(BlockEntityFirepit.ToTreeAttributes))]
		[HarmonyPostfix]
		public static void ToTreeAttributes(
      ref object __instance, 
      ITreeAttribute tree
    ) {
      List<Delegates.ToTreeAttributes> delegates = DelegateExt.GetAllInheriting<Delegates.ToTreeAttributes>(
        typeof(BlockEntityFirepit), MethodId.ToTreeAttributes
      );
      foreach(Delegates.ToTreeAttributes fn in delegates) {
        fn.Invoke(ref __instance, tree);
      }
    }
  }

	[HarmonyPatch(typeof(InventoryBase))]
  public static class InventoryBasePatch {
		// [HarmonyPatch(nameof(InventoryBase.OnItemSlotModified))]
		// [HarmonyPostfix]
		// public static void OnItemSlotModified(
    //   ref object __instance,
    //   ItemSlot slot
    // ) {
    //   List<Delegates.OnItemSlotModified> delegates = DelegateExt.GetAllInheriting<Delegates.OnItemSlotModified>(
    //     typeof(InventoryBase), MethodId.OnItemSlotModified
    //   );
    //   foreach(Delegates.OnItemSlotModified fn in delegates) {
    //     fn.Invoke(ref __instance, slot);
    //   }
    //   CharLib.Trace(
    //     "InventoryBase.OnItemSlotModified({0}, {1}) [{2}]",
    //     __instance, slot,
    //     delegates.Count
    //   );
    // }

		[HarmonyPatch(nameof(InventoryBase.Open))]
		[HarmonyPrefix]
    public static void Open(
      ref object __instance,
      IPlayer player,
      ref object __result
    ) {
      List<Delegates.Open> delegates = DelegateExt.GetAllInheriting<
        Delegates.Open
      >(
        typeof(InventoryBase), MethodId.Open
      );
      CharLib.Trace(
        "InventoryBase.Open({0}, {1}) = {2} [{3}]",
        __instance, player, __result,
        delegates.Count
      );
      foreach(Delegates.Open fn in delegates) {
        fn.Invoke(ref __instance, player, ref __result);
      }
    }

		[HarmonyPatch(nameof(InventoryBase.ActivateSlot))]
		[HarmonyPrefix]
		public static void ActivateSlot(
      ref object __instance,
      int slotId, 
      ItemSlot sourceSlot, 
      ref ItemStackMoveOperation op,
      ref object __result
    ) {
      List<Delegates.ActivateSlot> delegates = DelegateExt.GetAllInheriting<
        Delegates.ActivateSlot
      >(
        typeof(InventoryBase), MethodId.ActivateSlot
      );
      CharLib.Trace(
        "InventoryBase.ActivateSlot({0}, {1}, {2}, {3}) = {4} [{5}]",
        __instance, slotId, sourceSlot, op, __result,
        delegates.Count
      );
      foreach(Delegates.ActivateSlot fn in delegates) {
        fn.Invoke(ref __instance, slotId, sourceSlot, ref op, ref __result);
      }
    }
  }
}