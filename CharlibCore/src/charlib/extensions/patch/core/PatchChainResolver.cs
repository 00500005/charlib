
using System;

namespace Charlib.PatchChain {
  public delegate V? ChainFn<V, in C>(C c, V? t);
  public interface IHasChainFn<V, in C> {
    public ChainFn<V, C> ChainFn {get;}
  }
  public static class ChainFnExt {
    static object? val = default(Nullable<int>);
    public static ChainFn<V, C> AsChainFn<V, C>(
      this IHasChainFn<V, C> self
    ) {
      return self.ChainFn;
    }
    public static IHasChainFn<V, C> AsChainObject<V, C>(
      this ChainFn<V, C> self
    ) {
      return new IHasChainFnImpl.HasChainFnStruct<V,C>(self);
    }
  }
  public static class IHasChainFnImpl {
    public struct HasChainFnStruct<V, C> : IHasChainFn<V,C> {
      public HasChainFnStruct(ChainFn<V,C> fn) {
        ChainFn = fn;
      }
      public ChainFn<V, C> ChainFn {get;}
    }
  }
}