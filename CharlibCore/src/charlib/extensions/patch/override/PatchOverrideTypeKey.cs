using System;
using Charlib.PlayerDict;
using Charlib.PlayerDict.Reducer;

namespace Charlib.PatchChain.Override {
  public interface IPatchOverrideTypeKey : IPatchTypeKey { 
    public object? ValueFromString(
      IStringConstructorRegistry reg,
      string valueAsString
    );
  }
  public interface IPatchOverrideTypeKey<V> 
    : IPatchOverrideTypeKey
    , IPatchTypeKey<V, IHasPlayer> { 
    public new V? ValueFromString(
      IStringConstructorRegistry reg,
      string valueAsString
    );
  }
  public static class IPatchOverrideTypeKeyExt {
    public static IPatchOverrideTypeKey<V> Cast<V>(
      this IPatchOverrideTypeKey self
    ) {
      var simpleCast = self as IPatchOverrideTypeKey<V>;
      if (typeof(V) != self.ValueType) {
        throw new InvalidCastException();
      }
      return simpleCast ?? new IPatchOverrideTypeKeyImpl
        .GenericFullyQualified<V>(
          self.Id, self.ValueType, self.ContextType
        );
    }
    public static IPlayerDictTypeKey<V> InferDictKey<V>(
      this IPatchOverrideTypeKey<V> key
    ) {
      return IPlayerDictTypeKeyImpl.Create<V>(key.Id);
    }
    public static IPlayerDictTypeKey InferDictKey(
      this IPatchOverrideTypeKey key
    ) {
      return IPlayerDictTypeKeyImpl.Create(key.ValueType, key.Id);
    }
    
    public static IPlayerDictReducerTypeKey<
      V,V,PatchOverrideSerializedValue<V>
    > InferReducerKey<V>(
      this IPatchOverrideTypeKey<V> key
    ) {
      return new IPlayerDictReducerTypeKeyImpl.GenericFullyQualified<
        V,
        V,
        PatchOverrideSerializedValue<V>
      >(
        key.Id,
        ReducerKind.CLIENT_SUBSCRIBES,
        InferDictKey(key).Id
      );
    }
    public static V? ApplyOverrideToPlayer<V>(
      this IPatchOverrideTypeKey<V> self,
      V? value,
      IPlayerDict pd,
      IPlayerReducerStore store
    ) {
      CharlibMod.Logger.Debug("Applying Override {0} to {1}", self.Id, value);
      return value;
    }

    public static ChainFn<
      V,
      IHasPlayer
    > ApplyOverrideUsingPatchContext<V>(
      this IPatchOverrideTypeKey<V> key,
      IPlayerDictTypeKey<V> pdKey,
      IPlayerDictTypeKeyRegistry registry
    ) {
      ChainFn<V,IHasPlayer> fn = (
        context,
        original 
      ) => {
        if (context.Player == null) { return original; }
        var pd = context.Player.EnsurePlayerDict(registry);
        if (!pd.Has(pdKey)) { return original; }
        return pd.Get(pdKey);
      };
      return fn;
    }
  }
  public static class IPatchOverrideTypeKeyImpl {
    public class NonGeneric : IPatchOverrideTypeKey
    {
      public NonGeneric(
        string id, 
        Type valueType,
        Type contextType
      ) {
        Id = id;
        ValueType = valueType;
        ContextType = contextType;
      }
      public string Id {get;}
      public Type ContextType {get;}
      public Type ValueType {get;}
      public object? ValueFromString(
        IStringConstructorRegistry reg, 
        string valueAsString
      ) {
        return reg.ForType(ValueType)(valueAsString).AsNullable();
      }
    }
    public class GenericFullyQualified<V>
      : NonGeneric, IPatchOverrideTypeKey<V> {
      public GenericFullyQualified(
        string id, 
        Type? valueType = null, 
        Type? contextType = null
      ) : base(id, valueType ?? typeof(V), contextType ?? typeof(IHasPlayer)) 
      { }
      public new V? ValueFromString(
        IStringConstructorRegistry reg
        , string valueAsString
      ) {
        return reg.ForType<V>()(valueAsString).AsNullable();
      }
    }
  }
}