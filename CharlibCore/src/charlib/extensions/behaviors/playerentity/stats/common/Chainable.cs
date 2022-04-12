
using System;
using System.Linq;
using System.Collections.Generic;
using Vintagestory.API.Common;

namespace charlib {
  namespace ip {
    public delegate T? ChainableFn<C,T>(C c, T? t);
    public interface IChain<C,T> {
      public T? Resolve(C c, T? t);
    }
    /** This just serves as a marker */
    public interface IKey<K,C,T> where K : IKey<K,C,T> { 
    }
    public class ChainRegistry {
      
      /** 
        Actually Dictionary<IKey<T,C>, List<IChain<T,C>>> but C#
        doesn't have rank-n type variance
      */
      public Dictionary<Type, List<object>> Registry 
        = new Dictionary<Type, List<object>>();
      public void Register<K,C,T>(
        ChainableFn<C,T> fn,
        K? key
      ) where K : IKey<K,C,T> {
        Register<K,C,T>(fn);
      }
      public void Register<K,C,T>(
        IChain<C,T> chain,
        K? key
      ) where K : IKey<K,C,T> {
        Register<K,C,T>(chain);
      }
      public Chain<C,T> GetChain<K,C,T>(
        IKey<K,C,T>? key
      ) where K : IKey<K,C,T> {
        return GetChain<K,C,T>();
      }
      public void Register<K,C,T>(
        ChainableFn<C,T> fn
      ) where K : IKey<K,C,T> {
        Register<K,C,T>(new Chainable<C,T>(fn));
      }
      public void Register<K,C,T>(
        IChain<C,T> chain
      ) where K : IKey<K,C,T> {
        EnsureList<K,C,T>().Add(chain);
      }
      public List<object> EnsureList<K,C,T>() where K : IKey<K,C,T> {
        Type key = typeof(K);
        if (!Registry.ContainsKey(key)) {
          Registry[key] = new List<object>();
        }
        return Registry[key];
      }
      public Chain<C,T> GetChain<K,C,T>() where K : IKey<K,C,T> {
        Type key = typeof(K);
        if (!Registry.ContainsKey(key)) {
          return Chain<C,T>.Empty();
        }
        IEnumerable<IChain<C,T>?> l = 
          Registry[key].Cast<IChain<C,T>>();
        Chain<C,T> chain = Chain<C,T>.Empty();
        foreach (IChain<C,T>? c in l) {
          if (c != null) {
            chain.Add(c);
          }
        }
        return chain;
      }
    }
    public static class Chain {
      public static int PriorityLast = int.MaxValue;
      public static int PriorityFirst = int.MinValue;
    }
    public class Chainable<C,T> : IComparer<Chainable<C,T>>, IChain<C,T> {
      public ChainableFn<C,T> Fn;
      public int Priority;
      public Chainable(ChainableFn<C,T> fn, int priority = 0) {
        Fn = fn;
        Priority = priority;
      }

      public int Compare(Chainable<C,T>? x, Chainable<C,T>? y)
      {
        return (x?.Priority ?? 0) - (y?.Priority ?? 0);
      }

      public T? Resolve(C c, T? original) {
        return Fn(c, original);
      }
    }
    public class Chain<C,T> : IChain<C,T> {
      public static Chain<C,T> Empty() {
        return new Chain<C,T>();
      }
      List<Chainable<C,T>> Fns = new List<Chainable<C,T>>();
      public void Add(Chainable<C,T> chain) {
        Fns.Add(chain);
        Fns.Sort();
      }
      public void Add(IChain<C,T> fn) {
        Chainable<C,T>? actualChainable = fn as Chainable<C,T>;
        if (actualChainable != null) {
          Add(actualChainable);
        } else {
          Add(fn.Resolve);
        }
      }
      public void Add(ChainableFn<C,T> fn, int priority = 0) {
        Add(new Chainable<C,T>(fn, priority));
      }
      public T? Resolve(C c, T? original) {
        T? next = original;
        foreach (Chainable<C,T> fn in Fns) {
          next = fn.Resolve(c, next);
        }
        return next;
      }
    }
  }
}