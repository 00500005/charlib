
using System;

namespace Charlib.PatchChain {
  public delegate V? ChainFn<V, in C>(C c, V? t);
  public interface IHasChainFn<V, in C> {
    public ChainFn<V, C> ChainFn {get;}
  }
  public interface IHasChainId {
    public string Id {get;}
  }
  public static class ChainFnExt {
    public static ChainFn<V, C> AsChainFn<V, C>(
      this IHasChainFn<V,C> self
    ) {
      return self.ChainFn;
    }
  }
}