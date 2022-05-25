
using System.Collections.Generic;
using ProtoBuf;
using Vintagestory.API.Common;

namespace Charlib.PlayerDict {
  [ProtoContract]
  public class PlayerDictSyncRequest {}
  [ProtoContract]
  public class PlayerDictSerialized : Serializable<
    IPlayerDict, PlayerDictSerialized, PlayerDictSerializedDependencies
  > {
    [ProtoMember(1)]
    public Dictionary<string, byte[]> Data {get;}
      = new Dictionary<string, byte[]>();
    public override PlayerDictSerialized AsSerialized() {
      return this;
    }
    public override IPlayerDict AsValue(
      PlayerDictSerializedDependencies dependencies
    ) {
      return this.DeserializeInto(
        dependencies.Player.EnsurePlayerDict(
          dependencies.Registry
        )
      );
    }

    public override PlayerDictSerialized WithValue(IPlayerDict? dict) {
      return dict?.Serialize() ?? new PlayerDictSerialized();
    }
  }
  public struct PlayerDictSerializedDependencies {
    public IPlayer Player;
    public IPlayerDictTypeKeyRegistry Registry;
  }
  public static class IPlayerDictSyncExt {
    public static PlayerDictSerialized Serialize(
      this IPlayerDict dict
    ) {
      PlayerDictSerialized instance = new PlayerDictSerialized();
      foreach (var k in dict.Keys()) {
        instance.Data[k.Id] = dict.Get(k.Id)?.SerializeProto() ?? new byte[]{};
      }
      return instance;
    }
    public static IPlayerDict DeserializeInto(
      this PlayerDictSerialized dict,
      IPlayerDictTypeKeyRegistry registry,
      IPlayer player
    ) {
      return dict.DeserializeInto(player.EnsurePlayerDict(registry));
    }
    public static IPlayerDict DeserializeInto(
      this PlayerDictSerialized dict,
      IPlayerDict instance
    ) {
      foreach(var kv in dict.Data) {
        IPlayerDictTypeKey key = instance.Registry.Get(kv.Key);
        instance.Set(key, kv.Value.DeserializeProto(key.ValueType));
      }
      return instance;
    }
  }
}