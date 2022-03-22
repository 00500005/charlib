
using Vintagestory.API.Common;

namespace charlib {
  namespace Delegates {
    public delegate void OnItemSlotModified(
      ref object __instance,
      ItemSlot slot
    );
  }
  public partial class MethodId {
    public static MethodId OnItemSlotModified { get; private set; } 
      = new MethodId("OnItemSlotModified", 
        typeof(Delegates.OnItemSlotModified));
  }
}