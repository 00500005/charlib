using System;

namespace Charlib {
  namespace PlayerDict {
    public interface IPlayerDictKeyId {
      public string Id {get;}
    }
    public interface IPlayerDictTypeKey : IPlayerDictKeyId {
      public Type ValueType {get;}
    }
    public interface IPlayerDictTypeKey<out V>
      : IPlayerDictTypeKey 
    { }
    public static class IPlayerDictTypeKeyImpl {
      public static IPlayerDictTypeKey<V> Create<V>(
        string? id = null,
        IDiscriminator<V>? _ = null
      ) {
        return new Impl<V>(id);
      }
      public static IPlayerDictTypeKey Create(
        Type type,
        string? id = null
      ) {
        return new Impl(type, id);
      }
      private class Impl : IPlayerDictTypeKey {
        public Impl(
          Type valueType,
          string? id = null
        ) {
          this.ValueType = valueType;
          this.Id = id == null ? this.ValueType.Name : id;
        }
        public Type ValueType {get;}
        public string Id {get;}
      }
      private class Impl<V> 
        : Impl, IPlayerDictTypeKey<V> 
      {
        public Impl(string? id = null, Type? valueType = null) 
          : base(valueType ?? typeof(V), id)
        { }
      }
    }
    public static class IPlayerDictKeyExts {
      public static IPlayerDictTypeKey<V> Cast<V>(
        this IPlayerDictTypeKey self,
        IDiscriminator<V>? _ = null
      ) {
        var asDerived = self as IPlayerDictTypeKey<V>;
        if (asDerived != null) {
          return asDerived;
        }
        if (!self.ValueType.IsAssignableTo<V>()) {
          throw new InvalidCastException(
            $"Cannot cast IDictKey of {self.ValueType.Name} to {typeof(V).Name}"
          );
        }
        return IPlayerDictTypeKeyImpl.Create(
          self.Id,
          _
        );
      }
    }
  }
}