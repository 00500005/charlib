
using System;
using System.Linq;
using System.Collections.Generic;

namespace Charlib.PlayerDict {
  public interface IPlayerDictTypeKeyRegistry {
    public IEnumerable<IPlayerDictTypeKey> Keys();
    public bool Has(string id);
    public IPlayerDictTypeKey Get(string key);
    public void Register(IPlayerDictTypeKey key);
  }
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
      public void Register(IPlayerDictTypeKey keyToAdd)
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
    public static bool Has(
      this IPlayerDictTypeKeyRegistry reg,
      IPlayerDictTypeKey key
    ) {
      return reg.Has(key.Id);
    }
    public static IEnumerable<IPlayerDictTypeKey> KeysWithType(
      this IPlayerDictTypeKeyRegistry reg,
      Type type
    ) {
      return reg.Keys().Where(key => type.IsAssignableFrom(key.ValueType));
    }
    public static IPlayerDictTypeKey? KeyWithType(
      this IPlayerDictTypeKeyRegistry reg,
      Type type
    ) {
      return reg.KeysWithType(type).First();
    }
    public static IPlayerDictTypeKey Get(
      this IPlayerDictTypeKeyRegistry reg,
      IPlayerDictKeyId key
    ) {
      return reg.Get(key.Id);
    }
    public static IPlayerDictTypeKey<V> Get<V>(
      this IPlayerDictTypeKeyRegistry reg,
      IPlayerDictKeyId key,
      IDiscriminator<V>? _ = null
    ) {
      return reg.Get(key.Id).Cast<V>();
    }
    public static IPlayerDictTypeKey<V> Get<V>(
      this IPlayerDictTypeKeyRegistry reg,
      string key,
      IDiscriminator<V>? _ = null
    ) {
      return reg.Get(key).Cast<V>();
    }
    public static IPlayerDictTypeKey<V> Register<V>(
      this IPlayerDictTypeKeyRegistry reg,
      string? id = null,
      IDiscriminator<V>? _ = null
    ) {
      var key = IPlayerDictTypeKeyImpl.Create(id, _);
      reg.Register(key);
      return key;
    }
  }
}