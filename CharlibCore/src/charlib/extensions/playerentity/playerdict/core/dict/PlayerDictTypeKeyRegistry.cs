
using System;
using System.Linq;
using System.Collections.Generic;

namespace Charlib {
  using Charlib.PlayerDict;
  public interface IPlayerDictTypeKeyRegistry {
    public IEnumerable<IPlayerDictTypeKey> Keys();
    public bool Has(string id);
    /** 
      * Whenever possible, type'd version of get and declare should be used
      * (see extension methods below)
      */
    internal IPlayerDictTypeKey Get(string key);
    internal void Declare(IPlayerDictTypeKey key);
  }
}

namespace Charlib.PlayerDict {
  public static class IPlayerDictTypeKeyRegistryImpl {
    public class Impl : IPlayerDictTypeKeyRegistry
    {
      private Dictionary<string, IPlayerDictTypeKey> _keys {get;} 
        = new Dictionary<string, IPlayerDictTypeKey>();
      public IEnumerable<IPlayerDictTypeKey> Keys()
      {
        return _keys.Values;
      }
      public bool Has(string id) {
        return _keys.ContainsKey(id);
      }
      public void Declare(IPlayerDictTypeKey keyToAdd)
      {
        string id = keyToAdd.Id;
        if (_keys.ContainsKey(id)) {
          var existingKey = _keys[id];
          if (existingKey.ValueType != keyToAdd.ValueType) {
            throw new InvalidOperationException(
              $@"
              Already registered player data {id}.
              Requested = {keyToAdd.ValueType.Name}
              Existing = {existingKey.ValueType.Name}
              "
            );
          }
        } else {
          CharlibMod.Logger.Debug(
            "Registering PlayerDict id {0} as {1}",
            id, keyToAdd.ValueType.FullName
          );
          _keys[id] = keyToAdd;
        }
      }
      public IPlayerDictTypeKey Get(string key)
      {
        return _keys[key];
      }
    }
  }
  public static class IPlayerDictTypeKeyRegistryExts {
    public static IPlayerDictTypeKey<V>? MaybeGet<V>(
      this IPlayerDictTypeKeyRegistry reg,
      IPlayerDictTypeKey<V> key
    ) {
      return reg.Has(key.Id) ? reg.Get(key.Id).Cast<V>() : null;
    }
    public static IPlayerDictTypeKey<V> Declare<V>(
      this IPlayerDictTypeKeyRegistry reg,
      IPlayerDictTypeKey<V> key
    ) {
      reg.Declare(key);
      return key;
    }
  }
}