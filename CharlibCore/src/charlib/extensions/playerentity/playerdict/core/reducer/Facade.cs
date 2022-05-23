using System;
using Vintagestory.API.Client;
using Vintagestory.API.Server;

namespace Charlib.PlayerDict.Reducer {
  /** 
    The reducer pattern is useful as there are several types of state setting
    that we need to support. By making a single reducer Apply, we abstract away the need for the modifying code to be aware of:
      1. The current execution context is server or client (we'll check for it as part of the reduction and apply if appropriate)
      2. The update should additional be send to the server/or client (we'll send the update if appropriate)
    We also get the normal benefits of the reducer pattern (though notably or state is mutable rather immutable for performance reasons). Several levels of auditing can be easily added in a transparent matter, making debugging issues many times easier.

    The types of reductions are as follows:

      1. Server tracked only
        This is the simplest type, where the client never needs to be updated about the information and the server silently updates when appropriate methods are invoked.
        This state reduction may invoke other reductions on certain conditions
        - For instance, XP might be tracked via server, but we want to notify
          the client of lvl ups
      2. Server tracked, client subscribed
        - This type is likely to be chatty (negative server performance). 
        - Every time the server a PlayerDict, it sends the client the update as well.
        - Only appropriate if the reducer is updated infrequently
      3. Dual modeling
        - This type is the most complex and error prone (but one of the more efficient if made correctly). 
        - Code modifications will not check if executing on server or client and will always update state. Ideally, if both server + client execute methods in the same order, we'll have identical states on both client + server. 
        - However, it is likely that some methods will only ever be executed on the client or the server, making de-sync likely.
      4. Client tracked, server subscribed
        - This type should only ever be done if there is code that exclusively executes on the client, and there is not a better place to put where this type of state change should happen.
        - This type allows the client to modify server state, and is likely to be somewhat chatty.
      5. Client only *DISALLOWED*
        - Both the reducer and the associated PlayerDictKeys are never needed by server code, and is exclusively used by the client.
        - This type of state/reducer will *not* work with most of tooling as it's always assumed that the server will know the state
        - It's likely an abuse of the PlayerDict system as well, as at that point it's unlikely to be a "player" setting, but rather a client setting.
    Note, in most of these types of reductions, the client is unable to affect server state directly, making it secure. 

    However, we do not want to mix different types of reducers on a given piece of state (ie, for a given PlayerDictKey). Having multiple reducers of different types modify the same information is extremely likely to result in de-sync and confused developers.

  */
  public static class ReducerFacade {
    public static IPlayerDictReducerDefinition<V,M,S>
      DefineReducer<V,M,S>(
        IPlayerDictReducerRegistry registry,
        IPlayerDictReducerTypeKey<V,M,S> key,
        Reducer<V,M> reducer
      ) where S : class,ISerializable<M,S,
        IPlayerDictReducerMessageStandardDependencies
      >, new()
    {
      return IPlayerDictReducerDefinitionImpl.Register(
        registry, key, reducer
      );
    }
    public static PlayerDictReducerMessage CreateReducerMessage<M,S>(
      IPlayerDictReducerMessageSerializerTypeKey<M,S> key,
      M msg
    ) where S : class, ISerializableSerializeAspect<M,S>, new()
    {
      S package = new S().WithValue(msg);
      return new PlayerDictReducerMessage(
        key.ReducerId, 
        package.SerializeProto()
      );
    }
    public static PlayerDictReducerMessage CreateReducerMessageFromNonGeneric(
      IPlayerDictReducerTypeKey key,
      object? msg
    ) {
      CharlibMod.Logger.Debug(
        "Creating message of type {0} = {1}", 
        key.MessageSerializedType.FullName,
        msg
      );
      var s = (ISerializableAnonymousSerializer?)
        Activator.CreateInstance(key.MessageSerializedType);
      CharlibMod.Logger.Debug(
        "Creating message packager = {0}", 
        s
      );
      object? package = s?.WithValue(msg);
      CharlibMod.Logger.Debug(
        "Added message packaged value = {0}", 
        s
      );
      var proto = package?.SerializeProto() ?? new byte[]{};
      CharlibMod.Logger.Debug(
        "Got message proto value: {0}", 
        BitConverter.ToString(proto)
      );
      return new PlayerDictReducerMessage(
        key.ReducerId, 
        proto
      );
    }
  
    public static IPlayerDictReducerMessageServerDependencies 
      DependenciesForServer(
        IPlayerDictServerManager manager,
        IServerPlayer player
      ) 
    {
      return new IPlayerDictReducerMessageStandardDependenciesImpl.ServerImpl(
        player, manager
      );
    }
    public static IPlayerDictReducerMessageClientDependencies 
      DependenciesForClient(
        IPlayerDictClientManager manager,
        IClientPlayer? player = null
      ) 
    {
      return new IPlayerDictReducerMessageStandardDependenciesImpl.ClientImpl(
        player ?? manager.GetPlayer(),
        manager
      );
    }
  }
}