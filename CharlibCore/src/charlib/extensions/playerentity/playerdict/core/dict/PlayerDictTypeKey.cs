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
        string? id = null
      ) {
        return new Impl<V>(id);
      }
      private class Impl<V> : IPlayerDictTypeKey<V> {
        public Impl(string? id = null) { 
          Id = id ?? typeof(V).Name;
        }
        public Type ValueType => typeof(V);
        public string Id {get;}
      }
    }
    public static class IPlayerDictKeyExts {
      public static IPlayerDictTypeKey<V> Cast<V>(
        this IPlayerDictTypeKey self
      ) {
        var asDerived = self as IPlayerDictTypeKey<V>;
        if (asDerived == null) {
          throw new InvalidCastException(
            $@"Cannot cast TypeKey {
              self.Id
            } with [ValueType = {
              self.ValueType.FullName
            }] to a key with [ValueType = {typeof(V).FullName}]"
          );
        }
        return asDerived;
      }
    }
  }
}