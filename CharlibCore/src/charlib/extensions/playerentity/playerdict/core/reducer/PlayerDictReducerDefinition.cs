using System;

namespace Charlib.PlayerDict.Reducer {
  public delegate V? Reducer<out V, in M>(
    M? msg, 
    IPlayerDict pd,
    IPlayerReducerStore dispatcher
  );
  public interface IPlayerDictReducerDefinition {
    public IPlayerDictReducerTypeKey ReducerKey {get;}
    object? DeserializeMessage(
      PlayerDictReducerMessage serializedMessage,
      IPlayerDictReducerMessageStandardDependencies standardDeps
    );
  }
  public interface IPlayerDictReducerDefinition<out V, M> 
    : IPlayerDictReducerDefinition {
    public Reducer<V,M> Reducer {get;}
    public new IPlayerDictReducerTypeKey<V,M> ReducerKey {get;}
  }
  public interface IPlayerDictReducerDefinition<out V, M, S> 
    : IPlayerDictReducerDefinition<V,M>
    where S : class, ISerializable<M,S
      , IPlayerDictReducerMessageStandardDependencies
    >, new()
  {
    public new IPlayerDictReducerTypeKey<V,M,S> ReducerKey {get;}
  }
  
  public static class IPlayerDictReducerDefinitionExt {
    public static IPlayerDictReducerDefinition<V, M> Cast<V, M>(
      this IPlayerDictReducerDefinition self
    ) {
      var preCast = self as IPlayerDictReducerDefinition<V,M>;
      if (preCast == null) {
        var key = self.ReducerKey;
        throw new InvalidCastException($@"""Cannot cast reducer {
          key.ReducerId
        } from [{
          key.ResultValueType.FullName
        }, {
          key.MessageType.FullName
        }] to [{typeof(V).FullName}, {typeof(M).FullName}]""");
      }
      return preCast;
    }
    public static IPlayerDictReducerDefinition<V, M, S> Cast<V, M, S>(
      this IPlayerDictReducerDefinition self
    ) where S : class, ISerializable<M,S
      , IPlayerDictReducerMessageStandardDependencies
      >, new() 
    {
      var preCast = self as IPlayerDictReducerDefinition<V,M,S>;
      if (preCast == null) {
        var key = self.ReducerKey;
        throw new InvalidCastException($@"""Cannot cast reducer {
          key.ReducerId
        } from [{
          key.ResultValueType.FullName
        }, {
          key.MessageType.FullName
        }, {
          key.MessageSerializedType.FullName
        }] to [{typeof(V).FullName}, {typeof(M).FullName}, {
          typeof(S).FullName
        }]""");
      }
      return preCast;
    }
  }
  public static class IPlayerDictReducerDefinitionImpl {
    public static IPlayerDictReducerDefinition<V,M,S> Register<V,M,S>(
      IPlayerDictReducerRegistry registry,
      IPlayerDictReducerTypeKey<V,M,S> reducerKey,
      Reducer<V,M> reducer
    ) where S : class, ISerializable<M,S
      , IPlayerDictReducerMessageStandardDependencies
    >, new() {
      var def = new Impl<V,M,S>(reducerKey, reducer);
      registry.Register(def);
      return def;
    }
    private class Impl<V,M,S> 
      : IPlayerDictReducerDefinition<V,M,S> 
      where S : class, ISerializable<M,S
        , IPlayerDictReducerMessageStandardDependencies
      >, new()
    {
      public Impl(
        IPlayerDictReducerTypeKey<V,M,S> reducerKey,
        Reducer<V,M> reducer
      ) {
        Reducer = reducer;
        ReducerKey = reducerKey;
      }
      public Reducer<V,M> Reducer {get;}
      public IPlayerDictReducerTypeKey<V,M,S> ReducerKey {get;}
      IPlayerDictReducerTypeKey IPlayerDictReducerDefinition.ReducerKey 
        => ReducerKey;

      IPlayerDictReducerTypeKey<V, M> 
        IPlayerDictReducerDefinition<V, M>.ReducerKey => ReducerKey;

      public object? DeserializeMessage(
        PlayerDictReducerMessage serializedMessage,
        IPlayerDictReducerMessageStandardDependencies standardDeps
      ) {
        var s = serializedMessage.MsgData.DeserializeProto<S>();
        return s.AsValue(standardDeps);
      }
    }
  }
}