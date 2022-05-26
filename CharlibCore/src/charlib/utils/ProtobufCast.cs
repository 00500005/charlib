using System;
using System.IO;
using ProtoBuf;

namespace Charlib {
  public static class ByteExt {
    public static byte[] SerializeProto(
      this object? self
    )  {
      if (self == null) {
        return new byte[]{};
      }
      using(var stream = new MemoryStream()) {
        Serializer.Serialize(stream, self);
        return stream.ToArray();
      }
    }
    public static object DeserializeProto(
      this byte[] self,
      Type type
    )  {
      using(var stream = new MemoryStream(self)) {
        return Serializer.Deserialize(type, stream);
      }
    }
    public static T DeserializeProto<T>(
      this byte[] self
    ) {
      using(var stream = new MemoryStream(self)) {
        return Serializer.Deserialize<T>(stream);
      }
    }
  }
}