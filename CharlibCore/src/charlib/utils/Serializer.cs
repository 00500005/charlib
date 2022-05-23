using System;
using ProtoBuf.Meta;

namespace Charlib {
  public sealed class NoDeps { }
  public interface ISerializableAnonymousSerializer {
    public object? WithValue(object? value);
    public object? AsSerialized();
  }
  public interface ISerializableSerializeAspect<in USER_TYPE,out SELF_TYPE> 
    : ISerializableAnonymousSerializer {
    public SELF_TYPE WithValue(USER_TYPE value);
    public new SELF_TYPE AsSerialized();
  }
  public interface ISerializableDeserializeAspect<
    out USER_TYPE, in DEPS
  > {
    public USER_TYPE? AsValue(DEPS? dependencies);
  }
  public interface ISerializable<USER_TYPE,out SELF_TYPE,in DEPS> 
    : ISerializableSerializeAspect<USER_TYPE, SELF_TYPE>
    , ISerializableDeserializeAspect<USER_TYPE, DEPS>
    where SELF_TYPE : ISerializable<USER_TYPE,SELF_TYPE,DEPS>, new() { }
  public interface ISerializable<USER_TYPE,out SELF_TYPE> 
    : ISerializable<USER_TYPE, SELF_TYPE, NoDeps>
    where SELF_TYPE : ISerializable<USER_TYPE,SELF_TYPE, NoDeps>, new() { }
  public static class ISerializableExt {
    public static USER_TYPE? AsValue<USER_TYPE>(
      this ISerializableDeserializeAspect<USER_TYPE, NoDeps> self
    ) {
      return self.AsValue(null);
    }
  }
  public abstract class Serializable<
    USER_TYPE,
    SELF_TYPE,
    DEPS
  > : ISerializable<USER_TYPE, SELF_TYPE,DEPS>
    where SELF_TYPE : Serializable<USER_TYPE,SELF_TYPE,DEPS>, new()
  {
    public abstract SELF_TYPE WithValue(USER_TYPE? value);
    public abstract USER_TYPE? AsValue(DEPS? dependencies);
    public abstract SELF_TYPE AsSerialized();
    public object? WithValue(object? value)
    {
      return WithValue((USER_TYPE?)value);
    }
    object? ISerializableAnonymousSerializer.AsSerialized()
    {
      return AsSerialized();
    }

    public static explicit operator SELF_TYPE(
      Serializable<USER_TYPE,SELF_TYPE,DEPS> self 
    ) {
      return self.AsSerialized();
    }
  }
  public static class SerializerExt {
    public static void Register<V,S,DEPS>(
      this ISerializable<V,S,DEPS> self
    ) where S : ISerializable<V,S,DEPS>, new() {
      RuntimeTypeModel.Default[typeof(V)].SetSurrogate(typeof(S));
    }
    public static void Register<V,S>(
      this ISerializable<V,S> self
    ) where S : ISerializable<V,S>, new() {
      RuntimeTypeModel.Default[typeof(V)].SetSurrogate(typeof(S));
    }
  }
}