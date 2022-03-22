
using Vintagestory.API.Common;

namespace charlib {
  namespace Delegates {
    public delegate void Open(
      ref object instance,
      IPlayer player,
      ref object __result
    );
  }
  public partial class MethodId {
    public static MethodId Open { get; private set; } 
      = new MethodId("Open", 
        typeof(Delegates.Open));
  }
}