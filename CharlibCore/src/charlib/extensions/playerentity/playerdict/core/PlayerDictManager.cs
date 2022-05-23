
using System;
// using System.Linq;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Client;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Charlib.PlayerDict.Reducer;
using System.Linq;
using System.Diagnostics;

namespace Charlib.PlayerDict {
  public delegate void PlayerDictUpdateSettledCallbackFn(
    IPlayerDict player, IPlayerReducerStore store
  );
  public interface IPlayerReducerStore {
    /**
      * Schedules an update to run
      * Does not immediately run
      * Will send sync to client/server when applying
      */
    public void Apply<V,M,S>(
      IPlayer player, 
      IPlayerDictReducerTypeKey<V, M, S> key,
      M message,
      PlayerDictUpdateSettledCallbackFn? onSettled=null
    ) where S : class, ISerializable<M, S,
      IPlayerDictReducerMessageStandardDependencies
    >, new();
    public void ApplyNonGeneric(
      IPlayer player, 
      IPlayerDictReducerTypeKey key,
      object? message,
      PlayerDictUpdateSettledCallbackFn? onSettled=null
    );
    // In case we need to look at the registries for something
    public IPlayerDictTypeKeyRegistry DictKeyRegistry {get;}
    public IPlayerDictReducerRegistry ReducerRegistry {get;}
  }
  public interface IPlayerDictManager 
    : IPlayerReducerStore, IHasCharlibState, IDisposable {
    public void Register();
    public IPlayerDictClientManager RequiresClient();
    public IPlayerDictServerManager RequiresServer();
  }
  public interface IPlayerDictServerManager 
    : IPlayerDictManager, IHasServerApi {
    public void SendToClient(
      PlayerDictReducerMessage packagedMessage, 
      params IServerPlayer[] player
    );
    public void ApplyClientMessageImmediately(
      IPlayerDictReducerTypeKey key,
      object? message,
      IServerPlayer forPlayer
    );
  }
  public interface IPlayerDictClientManager 
    : IPlayerDictManager, IHasClientApi {
    public void SendToServer(PlayerDictReducerMessage packagedMessage);
    public void ApplyServerMessageImmediately(
      IPlayerDictReducerTypeKey key,
      object? message
    );
  }
  public interface IPlayerDictUniversalManager : 
    IPlayerDictManager, IPlayerDictServerManager, IPlayerDictClientManager {
  }
  public static class IPlayerDictServerManagerExt {
    public static void RegisterServer(
      this IPlayerDictServerManager self
    ) {
      var channel = self.ServerAPI.Network.GetChannel(CharlibMod.ChannelKey);
      channel.SetMessageHandler<PlayerDictSyncRequest>(self.SendSync);
      channel.SetMessageHandler<PlayerDictReducerMessage>(
        self.ApplyClientSerializedMessage);
    }
    public static void SendToClient<M>(
      this IPlayerDictServerManager self,
      M msg,
      params IServerPlayer[] targetPlayer
    ) where M : class { 
      var channel = self.ServerAPI.Network.GetChannel(CharlibMod.ChannelKey);
      channel.SendPacket(msg, targetPlayer);
    }
    public static void SendSync(
      this IPlayerDictServerManager self,
      IServerPlayer forPlayer,
      PlayerDictSyncRequest? _ = null
    ) {
      IPlayerDict pd = forPlayer.EnsurePlayerDict(
        self.DictKeyRegistry
      );
      self.SendToClient(
        pd.Serialize(),
        forPlayer
      );
    }
    internal static void ApplyClientSerializedMessage(
      this IPlayerDictServerManager self,
      IServerPlayer player, 
      PlayerDictReducerMessage packagedMessage
    ) {
      var reducerDef = self.ReducerRegistry.Get(packagedMessage.ReducerId);
      if (reducerDef == null) {
        self.State.Warning(
          $"Player {player.PlayerName} tried to send an unrecognized reducer message ({packagedMessage.ReducerId}). Message was ignored."
        );
        return;
      }
      if (!reducerDef.ReducerKey.Kind.AllowsClientToServer()) {
        self.State.Warning(
          $"Player {player.PlayerName} tried to send a disallowed message to the server ({reducerDef.ReducerKey.ReducerId}). Message was ignored."
        );
        return;
      }
      var message = reducerDef.DeserializeMessage(
        packagedMessage,
        Reducer.ReducerFacade.DependenciesForServer(
          self,
          player
        )
      );
      self.ApplyClientMessageImmediately(
        reducerDef.ReducerKey, 
        message, 
        player
      );
    }
  }
  public static class IPlayerDictClientManagerExt {
    public static void SendToServer<M>(
      this IPlayerDictClientManager self,
      M message
    ) where M : class {
      var channel = self.ClientAPI.Network.GetChannel(CharlibMod.ChannelKey);
      channel.SendPacket<M>(message);
    }
    public static void RequestSync(
      this IPlayerDictClientManager self
    ) {
      self.SendToServer<PlayerDictSyncRequest>(
        new PlayerDictSyncRequest()
      );
    }
    public static void RegisterClient(
      this IPlayerDictClientManager self
    ) {
        var channel = self.ClientAPI.Network.GetChannel(CharlibMod.ChannelKey);
        channel.SetMessageHandler<PlayerDictReducerMessage>
          (self.ApplyServerSerializedMessage);
        channel.SetMessageHandler<PlayerDictSerialized>(
          self.ApplyServerSyncToClient
        );
    }
    internal static void ApplyServerSyncToClient(
      this IPlayerDictClientManager self,
      PlayerDictSerialized syncMessage
    ) {
      IPlayer player = self.GetPlayer();
      IPlayerDict playerDict = player.EnsurePlayerDict(
        self.State.PlayerDictManager.DictKeyRegistry
      );
      lock(playerDict) {
        syncMessage.DeserializeInto(playerDict);
      }
    }
    internal static void ApplyServerSerializedMessage(
      this IPlayerDictClientManager self,
      PlayerDictReducerMessage serializedMessage
    ) {
      IClientPlayer player = self.GetPlayer();
      var reducerDef = self.ReducerRegistry.Get(serializedMessage.ReducerId);

      if (!reducerDef.ReducerKey.Kind.AllowsServerToClient()) {
        self.State.Warning(
          $"Server sent update message to player client {player.PlayerName} but that message type shouldn't be sent ({reducerDef.ReducerKey.ReducerId})"
        );
        return;
      }
      if (reducerDef == null) {
        self.State.Warning(
          $"Server tried to send an unrecognized reducer message ({serializedMessage.ReducerId}). Message was ignored."
        );
        return;
      }
      var message = reducerDef.DeserializeMessage(
        serializedMessage,
        Reducer.ReducerFacade.DependenciesForClient(
          self,
          player
        )
      );
      self.ApplyServerMessageImmediately(
        reducerDef.ReducerKey,
        message
      );
    }
  }
  public static class IPlayerDictManagerImpl {
    public abstract class BaseImpl 
      : IHasCharlibState.Impl, IPlayerDictManager {
      private BlockingCollection<Action> TaskQueue;
      private BlockingCollection<Action> SettledQueue;
      private CancellationTokenSource QueueCancelToken;
      private Thread QueueThread;
      protected BaseImpl(ICharlibState state) 
        : base(state) { 
        TaskQueue = new BlockingCollection<Action>(
          new ConcurrentQueue<Action>());
        SettledQueue = new BlockingCollection<Action>(
          new ConcurrentQueue<Action>());
        QueueCancelToken = new CancellationTokenSource();
        QueueThread = new Thread(() => {
          while (
            !QueueCancelToken.IsCancellationRequested
            && !TaskQueue.IsAddingCompleted
          ) {
            Action? next = null;
            try {
              while(TaskQueue.TryTake(out next, 100, QueueCancelToken.Token)) {
                CharlibMod.Logger.Debug(
                  $@"""{nameof(IPlayerDictManager)}.TaskQueue 
                  Got a task: {next.ToString()}. Running..."""
                );
                next();
                CharlibMod.Logger.Debug(
                  $@"""{nameof(IPlayerDictManager)}.TaskQueue 
                  Finished task {next.ToString()}"""
                );
              };
              while (
                TaskQueue.Count == 0 
                && SettledQueue.Count > 0
                && SettledQueue.TryTake(out next)
              ) {
                CharlibMod.Logger.Debug(
                  $@"""{nameof(IPlayerDictManager)}.TaskQueue 
                  Running settling task {next.ToString()}"""
                );
                next();
                CharlibMod.Logger.Debug(
                  $@"""{nameof(IPlayerDictManager)}.TaskQueue 
                  Finished settling task {next.ToString()}"""
                );
              }
            } catch (Exception e) {
              CharlibMod.Logger.Error(@"""
              Failed while running task {0}
              {1}: {2}
              """, next?.ToString(), e.Message, e.StackTrace);
            }
          }
          CharlibMod.Logger.Warning(
            $"{nameof(IPlayerDictManager)}.TaskQueue Exiting"
          );
        });
        QueueThread.Name = "PlayerDictManager.ReducerThread";
        QueueThread.Start();
      }

      public IPlayerDictTypeKeyRegistry DictKeyRegistry {get;} 
        = new IPlayerDictTypeKeyRegistryImpl.Impl();
      public IPlayerDictReducerRegistry ReducerRegistry {get;} 
        = new IPlayerDictReducerRegistryImpl.Impl();
      public abstract EnumAppSide Side {get;}
      public abstract void Register();
      public abstract IPlayerDictClientManager RequiresClient();
      public abstract IPlayerDictServerManager RequiresServer();
      protected void WithPlayerDict(
        IPlayer player, 
        Action<IPlayerDict> fn
      ) {
        IPlayerDict dict = player.EnsurePlayerDict(
          DictKeyRegistry);
        lock(dict) {
          fn(dict);
        }
      }
      protected void WithReducerDef<V,M>(
        string reducerId,
        Action<IPlayerDictReducerDefinition<V,M>> fn
      ) {
        IPlayerDictReducerDefinition<V,M> def
          = ReducerRegistry.Get(reducerId).Cast<V,M>();
        fn(def);
      }
      public void SetLocalPlayerDictValue(
        IPlayer player, 
        IPlayerDictReducerTypeKey key, 
        object? message
      ) {
        CharlibMod.Logger.Debug("Setting generic local pd {0} = {1}",
          key.ReducerId,
          message?.ToString() ?? "<NULL>"
        );
        var makeOptional = typeof(Optional).GetMethods()
          .Where(m => m.Name == nameof(Optional.FromNullable))
          .Where(m => m.GetGenericArguments().Length == 1)
          .Single()
          .MakeGenericMethod(key.MessageType);
        var messageAsOptional = makeOptional.Invoke(
          null, new object?[] { message }
        );
        CharlibMod.Logger.Debug(
          "Setting generic local pd (asOptional) {0} = {1}",
          key.ReducerId,
          messageAsOptional
        );
        typeof(BaseImpl).GetMethods()
          .Where(m => m.Name == nameof(SetLocalPlayerDictValue))
          .Where(m => m.GetGenericArguments().Length == 2)
          .Single()
          .MakeGenericMethod(key.ResultValueType, key.MessageType)
          .Invoke(
            this, new object?[] { player, key, messageAsOptional }
          );
      }
      public void SetLocalPlayerDictValue<V,M>(
        IPlayer player, 
        IPlayerDictReducerIdAspect key, 
        IOptional<M> message
      ) {
        if (player == null) {
          State.Warning(
            $"Attempted to apply a state update to non-existent player");
          return;
        }
        CharlibMod.Logger.Debug("Setting local pd {0} = {1}",
          key.ReducerId,
          message.ToString()
        );
        WithPlayerDict(player, dict => {
          WithReducerDef<V,M>(key.ReducerId, def => {
            dict.Set(
              def.ReducerKey.ResultId,
              def.Reducer(message.AsNullable<M>(), dict, this)
            );
          });
        });
      }
      public static IPlayerDictManager CreateManager(
        ICharlibState state
      ) {
        var api = state.Api;
        ICoreServerAPI? serverApi = api as ICoreServerAPI;
        ICoreClientAPI? clientAPI = api as ICoreClientAPI;
        if (serverApi != null && clientAPI != null) {
          return new IPlayerDictManagerImpl.UniversalImpl(state, 
            serverApi, clientAPI);
        } else if (serverApi != null) {
          return new IPlayerDictManagerImpl.ServerImpl(state, serverApi);
        } else if (clientAPI != null) {
          return new IPlayerDictManagerImpl.ClientImpl(state, clientAPI);
        }
        throw new NotSupportedException(
          "api must implement ICoreServerAPI or ICoreClientAPI");
      }

      public void Dispose()
      {
        try {
          TaskQueue.CompleteAdding();
          SettledQueue.CompleteAdding();
        } catch(ObjectDisposedException) {}
        QueueCancelToken.Cancel();
        try {
          QueueThread.Interrupt();
          QueueThread.Join(TimeSpan.FromSeconds(1));
        } catch(Exception e) {
          CharlibMod.Logger.Warning(
            @"Exception when cleaning up QueueThread: {0}
            {1}", e.Message, e.StackTrace
          );
        }
        TaskQueue.Dispose();
        SettledQueue.Dispose();
      }

      void IPlayerReducerStore.Apply<V, M, S>(
        IPlayer player, 
        IPlayerDictReducerTypeKey<V, M, S> key, 
        M message, 
        PlayerDictUpdateSettledCallbackFn? onSettled
      ) {
        TaskQueue.Add(() => {
          SetLocalPlayerDictValue<V,M>(player, key, 
            Optional.FromNullable<M>(message)
          );
          if (
            Side == EnumAppSide.Client && key.Kind.IsSyncClientToServer()
          ) {
            RequiresClient().SendToServer(Reducer.ReducerFacade.CreateReducerMessage<M,S>(
              key, message
            ));
          } else if (
            Side == EnumAppSide.Server && key.Kind.IsSyncServerToClient()
          ) {
            RequiresServer().SendToClient(Reducer.ReducerFacade.CreateReducerMessage<M,S>(
              key, message
            ), (IServerPlayer)player);
          }
        });
        if (onSettled != null) {
          SettledQueue.Add(() => WithPlayerDict(player, 
            dict => onSettled(dict, this)
          ));
        }
      }

      public void ApplyNonGeneric(
        IPlayer player, 
        IPlayerDictReducerTypeKey key, 
        object? message, 
        PlayerDictUpdateSettledCallbackFn? onSettled = null
      ) {
        TaskQueue.Add(() => {
          CharlibMod.Logger.Debug(
            "Starting queue task: {0}[{1}] = {2}", 
            key.ReducerId, key.MessageType, message ?? "<NULL>"
          );
          SetLocalPlayerDictValue(player, key, message);
          if (
            Side == EnumAppSide.Client && key.Kind.IsSyncClientToServer()
          ) {
            RequiresClient().SendToServer(Reducer.ReducerFacade.CreateReducerMessageFromNonGeneric(
              key, message
            ));
          } else if (
            Side == EnumAppSide.Server && key.Kind.IsSyncServerToClient()
          ) {
            RequiresServer().SendToClient(Reducer.ReducerFacade.CreateReducerMessageFromNonGeneric(
              key, message
            ), (IServerPlayer)player);
          }
          CharlibMod.Logger.Debug(
            "Finished queue task: {0}[{1}] = {2}", 
            key.ReducerId, key.MessageType, message ?? "<NULL>"
          );
        });
        CharlibMod.Logger.Debug(
          "Added to task queue: {0}[{1}] = {2}", 
          key.ReducerId, key.MessageType, message ?? "<NULL>"
        );
        if (onSettled != null) {
          SettledQueue.Add(() => WithPlayerDict(player, 
            dict => onSettled(dict, this)
          ));
        }
      }
    }
    public class ServerImpl : 
      IPlayerDictManagerImpl.BaseImpl, 
      IPlayerDictServerManager {
      public ICoreServerAPI ServerAPI {get;}
      public override EnumAppSide Side => EnumAppSide.Server;
      public override void Register()
      {
        this.RegisterServer();
      }
      public override IPlayerDictClientManager RequiresClient() {
        throw new NotSupportedException();
      }
      public override IPlayerDictServerManager RequiresServer() {
        return this;
      }
      public void SendToClient(
        PlayerDictReducerMessage message,
        params IServerPlayer[] players
      ) {
        this.SendToClient<PlayerDictReducerMessage>(message, players);
      }

      public void ApplyClientMessageImmediately(
          IPlayerDictReducerTypeKey key, 
          object? message, 
          IServerPlayer forPlayer
      ) {
        IPlayerDictReducerMessageServerDependencies deps 
          = Reducer.ReducerFacade.DependenciesForServer(
            this,
            forPlayer
          );

        throw new NotImplementedException();
      }

      public ServerImpl(
        ICharlibState state,
        ICoreServerAPI serverAPI
      ) : base(state) {
        ServerAPI = serverAPI;
      }
    }
    public class ClientImpl 
      : IPlayerDictManagerImpl.BaseImpl
      , IPlayerDictClientManager {
      public ClientImpl(
        ICharlibState state,
        ICoreClientAPI clientAPI
      ) : base(state) {
        ClientAPI = clientAPI;
      }
      public override EnumAppSide Side => EnumAppSide.Client;
      public override void Register() {
        this.RegisterClient();
      }
      public override IPlayerDictClientManager RequiresClient() {
        return this;
      }
      public override IPlayerDictServerManager RequiresServer() {
        throw new NotSupportedException();
      }

      public void SendToServer(PlayerDictReducerMessage packagedMessage)
      {
        throw new NotImplementedException();
      }

      public void ApplyServerMessageImmediately(
        IPlayerDictReducerTypeKey key, 
        object? message
      ) {
        throw new NotImplementedException();
      }
      public ICoreClientAPI ClientAPI {get;}
    }
    public class UniversalImpl : 
      IPlayerDictManagerImpl.BaseImpl, 
      IPlayerDictUniversalManager 
    {
      public UniversalImpl(
        ICharlibState state,
        ICoreServerAPI serverAPI,
        ICoreClientAPI clientAPI
      ) : base(state) {
        ClientAPI = clientAPI;
        ServerAPI = serverAPI;
      }
      public override EnumAppSide Side => EnumAppSide.Universal;
      public ICoreClientAPI ClientAPI {get;}
      public ICoreServerAPI ServerAPI {get;}
      public override IPlayerDictClientManager RequiresClient() {
        return this;
      }
      public override IPlayerDictServerManager RequiresServer() {
        return this;
      }
      public void SendToClient(
        PlayerDictReducerMessage message,
        params IServerPlayer[] player
      ) {
        // Nothing to do, we're the same instance 
        // and should have identical state
      }
      public void SendToServer(
        PlayerDictReducerMessage message)
      {
        // Nothing to do, we're the same instance 
        // and should have identical state
      }
      public override void Register()
      {
        this.RegisterServer();
        this.RegisterClient();
      }

      public void ApplyClientMessageImmediately(
        IPlayerDictReducerTypeKey key, 
        object? message, 
        IServerPlayer forPlayer
      ) {
        throw new InvalidProgramException("Should never be invoked");
      }

      public void ApplyServerMessageImmediately(
        IPlayerDictReducerTypeKey key, 
        object? message
      ) {
        throw new InvalidProgramException("Should never be invoked");
      }
    }
  }
}