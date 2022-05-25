using ProtoBuf;

namespace Charlib.PlayerDict.Reducer {
  [ProtoContract]
  public class PlayerDictReducerMessage {
    public PlayerDictReducerMessage(string id, byte[] msgData) {
      ReducerId = id;
      MsgData = msgData;
    }
    [ProtoMember(1)]
    public string ReducerId {get;}
    [ProtoMember(2)]
    public byte[] MsgData {get;}
  }
}