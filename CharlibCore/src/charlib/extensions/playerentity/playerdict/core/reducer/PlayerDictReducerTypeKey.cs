using System;
using System.Collections.Generic;

namespace Charlib.PlayerDict.Reducer {
  using SerializeDeps = IPlayerDictReducerMessageStandardDependencies;
  public interface IPlayerDictReducerIdAspect {
    public string ReducerId {get;}
  }
  public interface IPlayerDictReducerKindAspect {
    public ReducerKind Kind {get;}
  }
  public interface IPlayerDictReducerResultAspect {
    public string ResultId {get;}
    public Type ResultValueType {get;}
  }
  public interface IPlayerDictReducerMessageAspect {
    public Type MessageType {get;}
  }
  public interface IPlayerDictReducerMessageSerializationAspect {
    public Type MessageSerializedType {get;}
  }
  public interface IPlayerDictReducerMessageSerializerTypeKey<
      in M, S
    > : IPlayerDictReducerIdAspect 
    , IPlayerDictReducerKindAspect 
    , IPlayerDictReducerMessageSerializationAspect 
    where S : ISerializableSerializeAspect<M, S> { }
  public interface IPlayerDictReducerMessageDeserializerTypeKey<
      out M,S
    > 
    : IPlayerDictReducerIdAspect 
    , IPlayerDictReducerKindAspect 
    , IPlayerDictReducerMessageSerializationAspect 
    where S : ISerializableDeserializeAspect<M, SerializeDeps> { }
  public interface IPlayerDictReducerTypeKey
    : IPlayerDictReducerIdAspect
    , IPlayerDictReducerKindAspect
    , IPlayerDictReducerResultAspect
    , IPlayerDictReducerMessageAspect
    , IPlayerDictReducerMessageSerializationAspect {}
  public interface IPlayerDictReducerTypeKey<out V>
    : IPlayerDictReducerTypeKey
    , IPlayerDictReducerResultAspect {}
  public interface IPlayerDictReducerTypeKey<out V, M>
    : IPlayerDictReducerTypeKey<V>
    , IPlayerDictReducerMessageAspect {}
  public interface IPlayerDictReducerTypeKey<out V, M, S>
    : IPlayerDictReducerTypeKey<V, M>
    , IPlayerDictReducerMessageSerializerTypeKey<M, S>
    , IPlayerDictReducerMessageDeserializerTypeKey<M, S>
    where S : ISerializable<M, S, SerializeDeps> {}
  public static class IPlayerDictReducerTypeKeyExt {
  }
  public static class IPlayerDictReducerTypeKeyImpl {
    public class GenericFullyQualified<V,M,S> 
      : IPlayerDictReducerTypeKey<V, M, S> 
      where S : ISerializable<M, S, SerializeDeps>
    {
      public GenericFullyQualified(
        string reducerId,
        ReducerKind kind, 
        string resultId
      ) {
        ResultId = resultId;
        ReducerId = reducerId;
        Kind = kind;
      }

      public string ResultId {get;}
      public string ReducerId {get;}
      public ReducerKind Kind {get;}
      public Type ResultValueType => typeof(V);
      public Type MessageType => typeof(M);
      public Type MessageSerializedType => typeof(S);
      public override bool Equals(object obj) {
        var casted = obj as IPlayerDictReducerTypeKey<V, M, S>;
        if (casted == null) {
          return this == null;
        }
        return ReducerId == casted.ReducerId
          && ResultId == casted.ResultId
          && Kind == casted.Kind
          && ResultValueType == casted.ResultValueType
          && MessageType == casted.MessageType
          && MessageSerializedType == casted.MessageSerializedType;

      }

      public override int GetHashCode()
      {
        int hashCode = -399217223;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ResultId);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ReducerId);
        hashCode = hashCode * -1521134295 + Kind.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(ResultValueType);
        hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(MessageType);
        hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(MessageSerializedType);
        return hashCode;
      }
    }
  }
}