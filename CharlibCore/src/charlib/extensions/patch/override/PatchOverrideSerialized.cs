using ProtoBuf;
using Charlib.PlayerDict.Reducer;

namespace Charlib.PatchChain.Override {
  [ProtoContract]
  public class PatchOverrideSerializedValue<V>
    : Serializable<
      V, 
      PatchOverrideSerializedValue<V>,
      IPlayerDictReducerMessageStandardDependencies
    >
  {
    [ProtoMember(1)]
    V? Value;
    public override PatchOverrideSerializedValue<V> AsSerialized()
    {
      return this;
    }

    public override V? AsValue(IPlayerDictReducerMessageStandardDependencies? _)
    {
      return Value;
    }

    public override PatchOverrideSerializedValue<V> WithValue(V? value)
    {
      Value = value;
      return this;
    }
  }
}