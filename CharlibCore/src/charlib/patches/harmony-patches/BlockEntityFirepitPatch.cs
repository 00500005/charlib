
using HarmonyLib;
using Vintagestory.GameContent;
using Charlib.PatchChain;

namespace Charlib {
	[HarmonyPatch(typeof(BlockEntityFirepit))]
	public static class BlockEntityFirepitPatch
	{
		[HarmonyPatch(nameof(BlockEntityFirepit.maxCookingTime))]
		[HarmonyPostfix]
		public static void patchedMaxCookingTime(
      ref BlockEntityFirepit __instance, 
      ref float __result
    ) {
      __result = new FirepitCookingTime()
        .ApplyPatchChain(PlayerAndBlockEntity.FromBlockEntity(
          __instance, LastPlayerBEB.GetLastModifyingPlayer
        ), __result);
    }
  }
}