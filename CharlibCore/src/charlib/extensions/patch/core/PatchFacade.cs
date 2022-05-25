
namespace Charlib.PatchChain {
  public static class PatchFacade {
    public static double PriorityLast = double.MaxValue;
    public static double PriorityFirst = double.MinValue;
    public class PatchTypeKey<V,C,SELF> 
      : PatchTypeKeyImpl.PatchTypeKeyWithValueAndContext<V,C,SELF>
      where SELF : PatchTypeKey<V,C,SELF>, new() { }
  }
}