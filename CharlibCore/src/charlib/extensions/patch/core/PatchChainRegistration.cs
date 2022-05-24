
using System;
using System.Linq;
using System.Collections.Generic;
using Charlib.PatchChain.Override;

namespace Charlib.PatchChain {
  public static class PatchChainRegistrationFacade {
    public static IPatchChainPrioritizedRegistrationCollection<V,C> Empty<V,C>(
      IPatchTypeKey<V,C> key
    ) {
      return new IPatchChainRegistrationImpl.PrioritizedListImpl<V,C>(key);
    }
  }
  public interface IPatchChainRegistration {
    public IPatchTypeKey Key {get;}
  }
  public interface IPatchChainRegistration<V,in C> 
    : IPatchChainRegistration 
    , IHasChainFn<V,C>
  {
    public new IPatchTypeKey<V,C> Key {get;}
  }
  public interface IPatchChainRegistrationCollection 
    : IPatchChainRegistration {
      public int Length { get; }
    }
  public interface IPatchChainPrioritizedRegistrationCollection<V,C> 
    : IPatchChainRegistrationCollection
    , IPatchChainRegistration<V,C> 
  {
    public void Add(IHasPrioritizedChainFn<V,C> fn);
    public bool HasFn(IHasPrioritizedChainFn<V,C> fn);
  }
  public static class IPatchChainRegistrationImpl {
    public class PrioritizedListImpl<V,C>
      : IPatchChainPrioritizedRegistrationCollection<V,C>
    {
      internal PrioritizedListImpl(
        IPatchTypeKey<V,C> key
      ) {
        this.Key = key;
      }
      public IPatchTypeKey<V,C> Key {get;}
      public Type ValueType => typeof(V);
      public Type ContextType => typeof(C);
      IPatchTypeKey IPatchChainRegistration.Key => Key;
      ChainFn<V, C> IHasChainFn<V, C>.ChainFn => ChainFn;
      public int Length => Fns.Count;
      private List<IHasPrioritizedChainFn<V,C>> Fns 
        = new List<IHasPrioritizedChainFn<V,C>>();
      public void Add(IHasPrioritizedChainFn<V,C> fn) {
        Fns.Add(fn);
        Fns.Sort(HasPriorityFacade.Comparer());
      }
      public V? ChainFn(C c, V? t)
      {
        var nextT = t;
        foreach(var resolver in Fns) {
          nextT = resolver.ChainFn(c, nextT);
        }
        return nextT;
      }
      public bool HasFn(IHasPrioritizedChainFn<V, C> fn)
      {
        foreach(var chain in Fns){
          if (Object.ReferenceEquals(chain.ChainFn, fn.ChainFn)){
            return true;
          };
        }
        return false;
      }
    }
  }
  public static class IPatchChainRegistrationExt {
    public static IPatchChainPrioritizedRegistrationCollection<V,C> HardCast<
      V,C
    >(
      this IPatchChainRegistrationCollection self,
      IDiscriminator<V, C>? _ = null
    ) {
      return (IPatchChainPrioritizedRegistrationCollection<V,C>)(self);
    }
  }
}