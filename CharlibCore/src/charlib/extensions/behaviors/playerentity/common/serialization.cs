using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Vintagestory.API.Common;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace charlib {
  using PlayerStats = Dictionary<string, object>;

  public partial class PlayerDict {
    [OnDeserializing]
    public void RestorePlayerStatsOnDeserialize(StreamingContext context) {
      Console.WriteLine("Re-serializing {0}", this);
      PlayerStats jsonStats = Stats;
      Stats = new PlayerStats();
      foreach(var kv in jsonStats) {
        Type type = Type.GetType(kv.Key);
        JToken? val = kv.Value as JToken;
        if (val == null) {
          Console.WriteLine("Val for {0} is null", type.Name);
        } else {
          Stats[kv.Key] = val.ToObject(type);
        }
      }
      Console.WriteLine("Re-serialized {0}", this);
    }
  }
}