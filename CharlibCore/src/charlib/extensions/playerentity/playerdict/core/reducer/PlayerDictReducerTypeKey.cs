using System;

namespace Charlib.PlayerDict.Reducer {
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
      M, S
    > : IPlayerDictReducerIdAspect 
    , IPlayerDictReducerKindAspect 
    , IPlayerDictReducerMessageSerializationAspect 
    where S : class, ISerializableSerializeAspect<M, S>, new() { }
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
    , IPlayerDictReducerMessageSerializerTypeKey<M,S>
    where S : class, ISerializable<M, S
      , IPlayerDictReducerMessageStandardDependencies
    >, new() {}
  public static class IPlayerDictReducerTypeKeyExt {
    public static bool DoesAllowMessage<M>(
      this IPlayerDictReducerTypeKey key,
      IDiscriminator<M>? _ = null
    ) {
      return key.MessageType.IsAssignableFrom<M>();
    }
    public static bool DoesAllowMessage(
      this IPlayerDictReducerTypeKey key,
      Type messageType
    ) {
      return key.MessageType.IsAssignableFrom(messageType);
    }
    public static bool DoesAllowValue<V>(
      this IPlayerDictReducerTypeKey key,
      IDiscriminator<V>? _ = null
    ) {
      return key.ResultValueType.IsAssignableTo<V>();
    }
  }
  public static class IPlayerDictReducerTypeKeyImpl {
    public class GenericFullyQualified<V,M,S> 
      : IPlayerDictReducerTypeKey<V, M, S> 
      where S : class, ISerializable<M,S
        , IPlayerDictReducerMessageStandardDependencies
      >, new()
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
    }
  }
}