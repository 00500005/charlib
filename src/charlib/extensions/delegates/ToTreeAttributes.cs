using Vintagestory.API.Datastructures;

namespace charlib {
  namespace Delegates {
    public delegate void ToTreeAttributes(
      ref object instance,
      ITreeAttribute attr
    );
  }
  public partial class MethodId {
    public static MethodId ToTreeAttributes { get; private set; } 
      = new MethodId("ToTreeAttributes", 
        typeof(Delegates.ToTreeAttributes));
  }
}