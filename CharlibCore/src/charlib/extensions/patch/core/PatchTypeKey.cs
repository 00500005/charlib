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
      public interface IContextAspect<out C> 
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
      {}
    public interface IPatchTypeKey<V, out C> 
      : IPatchTypeKey<V>
      , IPatchTypeKey.IContextAspect<C> { 
    }
    public static class PatchTypeKeyImpl {
      public class PatchTypeKeyWithValueAndContext<V,C,SELF> 
        : IPatchTypeKey<V, C> 
        where SELF : PatchTypeKeyWithValueAndContext<V,C,SELF>, new()
      {
        public static PatchTypeKeyWithValueAndContext<V,C,SELF> TypeId 
          = new SELF();
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
          registry.Declare(new SELF());
        }
        public override string ToString() {
          return $@"PatchKey {Id} [V = {ValueType.FullName}] [C = {
            ContextType.FullName
          }]";
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