

using Vintagestory.API.Common;

namespace charlib {
  namespace Delegates {
    public delegate void MaxCookingTime(
      ref object __instance,
      ref float __result
    );
  }
  public partial class MethodId {
    public static MethodId MaxCookingTime { get; private set; } 
      = new MethodId("MaxCookingTime", 
        typeof(Delegates.MaxCookingTime));
  }
}