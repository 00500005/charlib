
using System;
using System.Reflection;
using Charlib.PatchChain;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Charlib {
	public static class SmeltingInventoryPatch
	{
    public static void Patch(Harmony harmony) {
      harmony.Patch(IndexerGetterInfo(), null, GetItemSlotInfo());
    }
    public static void GetItemSlot(
      ref InventorySmelting __instance,
      ref ItemSlot __result
    ) {
      var inv = __instance;
      ItemSlotWatertight? isw = __result as ItemSlotWatertight;
      if (isw?.capacityLitres != null) {
        isw.capacityLitres = new FirepitCookingPotCapacityLiters()
          .ApplyPatchChain(PlayerAndBlockEntity.FromInventory(
            __instance, LastPlayerBEB.GetLastModifyingPlayer
          ), isw.capacityLitres);
      }
      if (__result != null) {
        __result.MaxSlotStackSize = new FirepitCookingPotStackSize()
          .ApplyPatchChain(PlayerAndBlockEntity.FromInventory(
            __instance, LastPlayerBEB.GetLastModifyingPlayer
          ), __result.MaxSlotStackSize);
      }
    }
    private static PropertyInfo? GetIndexer(Type type) {
      foreach (PropertyInfo p in type.GetProperties(
        BindingFlags.Instance | BindingFlags.Public
      )) {
        if (p.GetIndexParameters().Length > 0) {
          return p;
        }
      }
      return null;
    }
    private static MethodInfo IndexerGetterInfo() {
      return GetIndexer(typeof(InventorySmelting))?.GetGetMethod()!;
    }
    private static HarmonyMethod GetItemSlotInfo() {
      return new HarmonyMethod(
        typeof(SmeltingInventoryPatch).GetMethod(nameof(GetItemSlot))
      );
    }
	// 	[HarmonyPatch(
  //     nameof(InventorySmelting.CookingSlotCapacityLitres),
  //     MethodType.Getter
  //   )]
	// 	[HarmonyPostfix]
  //   public static void CookingSlotCapacityLitres(
  //     ref InventorySmelting __instance,
  //     ref float __result
  //   ) {
  //     __result = charlib.ip.ChainRegistry.Instance.GetChain(
  //       new FirepitCookingPotCapcityLiters()
  //     ).Resolve(PlayerAndBlockEntity.FromInventory(
  //       __instance, LastPlayerBEB.GetLastModifyingPlayer
  //     ), __result);
  //   }
	// 	[HarmonyPatch(
  //     nameof(InventorySmelting.CookingContainerMaxSlotStackSize),
  //     MethodType.Getter
  //   )]
	// 	[HarmonyPostfix]
  //   public static void CookingContainerMaxSlotStackSize(
  //     ref InventorySmelting __instance,
  //     ref int __result
  //   ) {
  //     __result = charlib.ip.ChainRegistry.Instance.GetChain(
  //       new FirepitCookingPotStackSize()
  //     ).Resolve(PlayerAndBlockEntity.FromInventory(
  //       __instance, LastPlayerBEB.GetLastModifyingPlayer
  //     ), __result);
  //   }
  }
}