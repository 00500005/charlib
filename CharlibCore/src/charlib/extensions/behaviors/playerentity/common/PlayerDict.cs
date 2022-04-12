using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Vintagestory.API.Common;

namespace charlib {
  using PlayerStats = Dictionary<string, object>;
  [JsonObject(MemberSerialization.OptIn)]
  public partial class PlayerDict : PlayerData<PlayerDict> {
    [JsonProperty]
    public PlayerStats Stats { get; private set; } = new PlayerStats();
    public V Set<V>(V value) where V : notnull {
      Stats[typeof(V)!.AssemblyQualifiedName!] = value;
      return value;
    }
    public V? Get<V>(V? defaultValue = null) where V : class {
      string name = typeof(V)!.AssemblyQualifiedName!;
      if (Stats.ContainsKey(name)) {
        return (V?)Stats[name];
      }
      return defaultValue;
    }

    public void SetWith(PlayerDict pdLastKnown)
    {
      foreach(var stat in pdLastKnown.Stats) {
        Stats[stat.Key] = stat.Value;
      }
    }

    public V Ensure<V>(System.Func<V> constructor) where V : class {
      string name = typeof(V)!.AssemblyQualifiedName!;
      if (!Stats.ContainsKey(name)) {
        Stats[name] = constructor();
      }
      return (V)Stats[name];
    }
    [JsonConstructor]
    public PlayerDict(
      string playerId, 
      PlayerStats stats
    ) : base(CharLib.State.Api!.World.PlayerByUid(playerId)) { 
      Stats = stats;
    }
    public PlayerDict(
      IPlayer player, 
      bool shouldRegister = true
    ) : base(player, shouldRegister) { 
    }
    public override string ToString()
    {
      return String.Join("\n", Stats.Select(kv => 
        $"{kv.Key} = {kv.Value}"
      ));
    }

  }
}