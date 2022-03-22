
using Vintagestory.API.Common;

namespace charlib {
  namespace Delegates {
    public delegate void ActivateSlot(
      ref object __instance,
      int slotId, 
      ItemSlot sourceSlot, 
      ref ItemStackMoveOperation op,
      ref object __result
    );
  }
  public partial class MethodId {
    public static MethodId ActivateSlot { get; private set; } 
      = new MethodId("ActivateSlot", 
        typeof(Delegates.ActivateSlot));
  }
}