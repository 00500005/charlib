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
  public interface IFullyQualifiedChainFn<V,in C> 
    : IHasPriority
    , IHasChainId
    , IHasChainFn<V,C> {}
  public static class IHasPrioritizedChainFnExt {
    public static IFullyQualifiedChainFn<V,C> WithPriority<V,C>(
      this IFullyQualifiedChainFn<V,C> self,
      double priority
    ) {
      return new IHasPrioritizedChainFnImpl.PrioritizedChainFnStruct<V,C>(
        self.ChainFn,
        priority,
        self.Id
      );
    }
    public static IFullyQualifiedChainFn<V,C> WithId<V,C>(
      this IFullyQualifiedChainFn<V,C> self,
      string id
    ) {
      return new IHasPrioritizedChainFnImpl.PrioritizedChainFnStruct<V,C>(
        self.ChainFn,
        self.Priority,
        id
      );
    }
    public static IFullyQualifiedChainFn<V,C> WithPriorityAndId<V,C>(
      this ChainFn<V,C> self,
      double priority,
      string id
    ) {
      return new IHasPrioritizedChainFnImpl.PrioritizedChainFnStruct<V,C>(
        self,
        priority,
        id
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
      : IFullyQualifiedChainFn<V,C>
    {
      internal PrioritizedChainFnStruct(
        ChainFn<V,C> fn,
        double priority,
        string id
      ) {
        ChainFn = fn;
        Priority = priority;
        Id = id;
      }
      public double Priority {get;}
      public ChainFn<V, C> ChainFn {get;}
      public string Id {get;}
    }
  }
}