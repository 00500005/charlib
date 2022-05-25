using System;
using System.Linq;
using System.Collections.Generic;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Common;

namespace Charlib.PlayerDict {
  public interface IPlayerDict {
    public IPlayerDictTypeKeyRegistry Registry {get;}
    public IPlayer OwningPlayer {get;}
    public IEnumerable<IPlayerDictTypeKey> Keys();
    public object? Get(string key);
    public bool Has(string key);
    public void Set(string key, object? value);
    public IPlayerDict RegisterBehavior();
  }
  public abstract class PlayerDictEntity : EntityBehavior, IPlayerDict {
    public static string VSKey = "PlayerDict";
    protected PlayerDictEntity(Entity entity) : base(entity) { }
    public abstract IPlayerDictTypeKeyRegistry Registry {get;}
    public abstract IPlayer OwningPlayer {get;}
    public abstract object? Get(string key);
    public abstract bool Has(string key);
    public abstract IEnumerable<IPlayerDictTypeKey> Keys();
    public abstract IPlayerDict RegisterBehavior();
    public abstract void Set(string key, object? value);
  }
  public static class IPlayerDictImpl {
    public static PlayerDictEntity CreateAndRegister(
      IPlayer player,
      IPlayerDictTypeKeyRegistry registry
    ) {
      var impl = new Impl(player, registry);
      impl.RegisterBehavior();
      return impl;
    }
    private class Impl : PlayerDictEntity {
      public override IPlayer OwningPlayer {get;}
      public override IPlayerDictTypeKeyRegistry Registry {get;}
      public Dictionary<string, object?> Data {get;} 
        = new Dictionary<string, object?>();
      public Impl(
        IPlayer owningPlayer,
        IPlayerDictTypeKeyRegistry reg
      ) : base(owningPlayer.Entity) {
        this.Registry = reg;
        this.OwningPlayer = owningPlayer;
      }
      public override IPlayerDict RegisterBehavior() {
        entity.AddBehavior(this);
        return this;
      }

      public override string ToString()
      {
        return String.Join("\n", Data.Select(kv =>
          $"{kv.Key} = {kv.Value}"
        ));
      }
      public override object? Get(string key)
      {
        return Data.ContainsKey(key) ? Data[key] : null;
      }
      public override bool Has(string key)
      {
        return Data.ContainsKey(key);
      }
      public override void Set(string key, object? value)
      {
        CharlibMod.Logger.Debug("Setting Value {0} = {1}", key, value);
        Data[key] = value;
      }
      public override IEnumerable<IPlayerDictTypeKey> Keys()
      {
        return Data.Keys.Select(Registry.Get);
      }
      public override string PropertyName()
      {
        return VSKey;
      }
    }
  }
  public static class IPlayerDictExt {
    public static T? Get<T>(
      this IPlayerDict dict,
      IPlayerDictTypeKey<T> key
    ) {
      return (T?)dict.Get(key.Id);
    }
    public static bool Has(
      this IPlayerDict dict,
      IPlayerDictTypeKey key
    ) {
      return dict.Has(key.Id);
    }

    public static void Set<T>(
      this IPlayerDict dict,
      IPlayerDictTypeKey key,
      T? value
    ) {
      dict.Set(key.Id, value);
    }
    public static void SetWith(
      this IPlayerDict self,
      IPlayerDict other
    ) {
      foreach (IPlayerDictTypeKey key in other.Keys()) {
        self.Set(key, other.Get(key.Id));
      }
    }
    public static V? Ensure<V>(
      this IPlayerDict dict,
      IPlayerDictTypeKey<V> key,
      System.Func<V> constructor
    ) {
      if (!dict.Has(key)) {
        dict.Set(key, constructor());
      }
      return dict.Get<V>(key);
    }
  }
}