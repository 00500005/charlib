using System;
using System.Collections.Generic;

namespace Charlib.PlayerDict.Reducer {
  public interface IPlayerDictReducerRegistry {
    public IPlayerDictReducerDefinition Get(string reducerId);
    public bool Has(string key);
    public void Register(IPlayerDictReducerDefinition def);
  }
  public static class IReducerRegistrarExt {
    public static bool Has(
      this IPlayerDictReducerRegistry registrar,
      IPlayerDictReducerIdAspect key
    ) {
      return registrar.Has(key.ReducerId);
    }
    public static IPlayerDictReducerDefinition Get(
      this IPlayerDictReducerRegistry registrar,
      IPlayerDictReducerIdAspect key
    ) {
      return registrar.Get(key.ReducerId);
    }
  }

  public static class IPlayerDictReducerRegistryImpl {
    public class Impl : IPlayerDictReducerRegistry {
      public Dictionary<string, IPlayerDictReducerDefinition> Definitions 
        = new Dictionary<string, IPlayerDictReducerDefinition>();
      public bool Has(string key) {
        return Definitions.ContainsKey(key);
      }
      public IPlayerDictReducerDefinition Get(string key) {
        return Definitions[key];
      }
      public void Register(IPlayerDictReducerDefinition def) {
        string id = def.ReducerKey.ReducerId;
        if (Definitions.ContainsKey(id)) {
          if (!Get(id).Equals(def)) {
            throw new InvalidOperationException(
              $"Reducer {id} already registered. IDs cannot be reused"
            );
          }
        } else {
          CharlibMod.Logger.Debug(
            "Registering Reducer id {0} as {1}",
            id, def.ReducerKey.ResultValueType.FullName
          );
          Definitions[id] = def;
        }
      }
    }
  }
}