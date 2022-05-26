
using System;
using System.Collections.Generic;

namespace Charlib.PatchChain {
  public static class PatchChainRegistrationFacade {
    public static IPatchChainRegistrationCollectionInput<V,C> Empty<V,C>(
      IPatchTypeKey<V,C> key
    ) {
      return new IPatchChainRegistrationImpl.PrioritizedListImpl<V,C>(key);
    }
  }
  public interface IPatchChainRegistration {
    public IPatchTypeKey Key {get;}
  }
  public interface IPatchChainRegistrationCollection 
    : IPatchChainRegistration 
  { 
    public int Length { get; }
  }
  public interface IPatchChainRegistration<V,in C_IN> 
    : IPatchChainRegistration 
    , IHasChainFn<V, C_IN> { }
  public interface IPatchChainRegistration<V,in C_IN,out C_OUT> 
    : IPatchChainRegistration<V, C_IN>
    where C_IN : C_OUT
  {
    public new IPatchTypeKey<V,C_OUT> Key {get;}
  }
  public interface IPatchChainRegistrationCollection<
    V, in C_IN
  > : IPatchChainRegistrationCollection
    , IPatchChainRegistration<V,C_IN> 
  { }
  public interface IPatchChainRegistrationCollectionInput<
    V, out C_OUT
  > : IPatchChainRegistrationCollection {
    public void Add(IHasPrioritizedChainFn<V,C_OUT> fn);
    public bool HasFn(IHasPrioritizedChainFn<V,C_OUT> fn);
  }
  public static class IPatchChainRegistrationImpl {
    public class PrioritizedListImpl<V,C>
      : IPatchChainRegistrationCollection<V,C>
      , IPatchChainRegistrationCollectionInput<V,C>
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
    public static IPatchChainRegistrationCollectionInput<V,C> AsDeclarable<
      V,C
    >(
      this IPatchChainRegistrationCollection self
    ) {
      return (IPatchChainRegistrationCollectionInput<V,C>)(self);
    }
    public static IPatchChainRegistrationCollection<V,C> AsInvocable<
      V,C
    >(
      this IPatchChainRegistrationCollection self
    ) {
      return (IPatchChainRegistrationCollection<V,C>)(self);
    }
  }
}