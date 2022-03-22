
using System.Collections.Generic;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace charlib {
	[HarmonyPatch(typeof(BlockEntityFirepit))]
	public static class FirepitMaxCookingTime
	{
		[HarmonyPatch(nameof(BlockEntityFirepit.maxCookingTime))]
		[HarmonyPostfix]
		public static void patchedMaxCookingTime(
      ref object __instance, 
      ref float __result
    ) {
      if (__instance == null) {
        CharLib.Trace("Invalid patched maxCookingTime. We should not be null");
        return;
      }
      List<Delegates.MaxCookingTime> delegates = DelegateExt.GetAllInheriting<Delegates.MaxCookingTime>(
        typeof(BlockEntityFirepit),
        MethodId.MaxCookingTime);
      BlockEntityFirepit? firepit = __instance as BlockEntityFirepit;
      // CharLib.Trace("Updated cooking time with {0} delegates @{1}",
      //   delegates.Count,
      //   firepit?.Pos
      // );
      float oldResult = __result;
      foreach(Delegates.MaxCookingTime fn in delegates) {
        fn(ref __instance, ref __result);
      }
      // CharLib.Trace("Updated cooking time from {0} to {1} (@{2})",
      //   oldResult,
      //   __result,
      //   firepit?.Pos
      // );
    }
  }
}
