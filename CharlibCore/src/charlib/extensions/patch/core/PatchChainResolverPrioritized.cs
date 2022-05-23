using System.Collections.Generic;

namespace Charlib.PatchChain {
  public class HasPriorityFacade {
    public static IComparer<IHasPriority> Comparer() {
      return new IHasPriorityImpl.PrioritizedComparer();
    }
  }
  public interface IHasPriority {
    double Priority {get;}
  }
  public interface IHasPrioritizedChainFn<V,in C> 
    : IHasPriority
    , IHasChainFn<V,C> { }
  public static class IHasPrioritizedChainFnExt {
    public static IHasPrioritizedChainFn<V,C> WithPriority<V,C>(
      this IHasChainFn<V,C> self,
      double priority
    ) {
      return new IHasPrioritizedChainFnImpl.PrioritizedChainFnStruct<V,C>(
        self.ChainFn,
        priority
      );
    }
    public static IHasPrioritizedChainFn<V,C> WithPriority<V,C>(
      this ChainFn<V,C> self,
      double priority
    ) {
      return new IHasPrioritizedChainFnImpl.PrioritizedChainFnStruct<V,C>(
        self,
        priority
      );
    }
  }
  public static class IHasPriorityImpl {
    public class PrioritizedComparer : IComparer<IHasPriority>
    {
      public int Compare(IHasPriority? x, IHasPriority? y)
      {
        return (int)((x?.Priority ?? 0) - (y?.Priority ?? 0));
      }
    }
  }
  public static class IHasPrioritizedChainFnImpl {
    public struct PrioritizedChainFnStruct<V, C>
      : IHasPrioritizedChainFn<V, C>
    {
      internal PrioritizedChainFnStruct(
        ChainFn<V,C> fn,
        double priority
      ) {
        ChainFn = fn;
        Priority = priority;
      }
      public double Priority {get;}
      public ChainFn<V, C> ChainFn {get;}
    }
  }
}