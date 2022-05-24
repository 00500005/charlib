using System;
using Charlib.PatchChain.Override;

namespace Charlib.PatchChain {
    public interface IPatchTypeKeyRaw {
      public string Id {get;}
    }
    public interface IPatchTypeKeyKind {
      public Type ContextType {get;}
      public Type ValueType {get;}
      public IPatchOverrideTypeKey AsPatchOverrideTypeKey();
    }
    public interface IPatchTypeKey 
      : IPatchTypeKeyRaw
      , IPatchTypeKeyKind 
    {
      public interface IContextAspect<in C> 
        : IPatchTypeKey
      { }
      public interface IValueAspect<V> 
        : IPatchTypeKey
      { 
        public new IPatchOverrideTypeKey<V> AsPatchOverrideTypeKey();
      }
    }
    public interface IPatchTypeKey<V> 
      : IPatchTypeKey 
      , IPatchTypeKey.IValueAspect<V>
      , IDiscriminator<V> { }
    public interface IPatchTypeKey<V, in C> 
      : IPatchTypeKey<V>
      , IPatchTypeKey.IContextAspect<C>
      , IDiscriminator<V> { 
    }
    public static class PatchTypeKeyImpl {
      public class PatchTypeKeyWithValueAndContext<V,C,S> 
        : IPatchTypeKey<V, C> 
        where S : PatchTypeKeyWithValueAndContext<V,C,S>, new()
      {
        public static IDiscriminator<V,C,S> TypeId 
          = Discriminator.Identify<V,C,S>();
        public PatchTypeKeyWithValueAndContext(string? id = null) {
          this.Id = id == null ? this.GetType().Name : id;
          this.ValueType = typeof(V);
          this.ContextType = typeof(C);
        }
        public Type ContextType {get;}
        public Type ValueType {get;}
        public string Id {get;}
        public static void Declare(
          IPatchChainRegistry registry
        ) {
          registry.Declare(new S());
        }
        public IPatchOverrideTypeKey<V> AsPatchOverrideTypeKey() {
          return PatchOverrideFacade.OverrideTypeKey(this);
        }
        IPatchOverrideTypeKey IPatchTypeKeyKind.AsPatchOverrideTypeKey()
        {
          return AsPatchOverrideTypeKey();
        }
      }
    }
    public static class IPatchTypeKeyExt {
      public static bool DoesAllowValue(
        this IPatchTypeKey self,
        Type ValueType
      ) {
        return self.ValueType.IsAssignableFrom(ValueType)
          && self.ValueType.IsAssignableTo(ValueType);
      }
      public static bool DoesAllowValue<V>(
        this IPatchTypeKey self,
        IDiscriminator<V>? _ = null
      ) {
        return self.DoesAllowValue(typeof(V));
      }
      public static bool DoesAllowContext<C>(
        this IPatchTypeKey self,
        IDiscriminator<C>? _ = null
      )  {
        return self.ContextType.IsAssignableFrom<C>();
      }
      public static bool DoesAllow<V,C>(
        this IPatchTypeKey self,
        IDiscriminator<V,C>? _ = null
      )  {
        // Chainable function must both:
        // accept V (isAssignableFrom V)
        //  (as a parameter) *and*
        // return a viable V (isAssignableTo V)
        return self.ValueType.IsAssignableFrom<V>()
          && self.ValueType.IsAssignableTo<V>()
          && self.ContextType.IsAssignableFrom<C>();
      }
      public static T ApplyPatchChain<T,C>(
        this IPatchTypeKey<T,C> key,
        C context,
        T initialValue
      ) {
        return CharlibMod.WithGlobalState<T>(
          state => state.PatchChainRegistry.InvokeChain(
            key, context, initialValue
          ) ?? initialValue
          , initialValue
        );
      }
    }
}