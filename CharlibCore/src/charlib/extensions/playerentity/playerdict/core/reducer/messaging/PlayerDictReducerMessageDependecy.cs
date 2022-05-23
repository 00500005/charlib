
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Charlib.PlayerDict.Reducer {
  public interface IPlayerDictReducerMessageStandardDependencies {
    public IPlayer ForPlayer {get;}
    public IPlayerDictManager Manager {get;}
  }
  public interface IPlayerDictReducerMessageServerDependencies 
    : IPlayerDictReducerMessageStandardDependencies {
    public new IServerPlayer ForPlayer {get;}
    public new IPlayerDictServerManager Manager {get;}
  }
  public interface IPlayerDictReducerMessageClientDependencies 
    : IPlayerDictReducerMessageStandardDependencies {
    public new IClientPlayer ForPlayer {get;}
    public new IPlayerDictClientManager Manager {get;}
  }
  public static class IPlayerDictReducerMessageStandardDependenciesExt {
    public static IPlayerDictReducerMessageServerDependencies AsServer(
      this IPlayerDictReducerMessageStandardDependencies self
    ) {
      return (IPlayerDictReducerMessageServerDependencies)self;
    }
    public static IPlayerDictReducerMessageClientDependencies AsClient(
      this IPlayerDictReducerMessageStandardDependencies self
    ) {
      return (IPlayerDictReducerMessageClientDependencies)self;
    }
  }
  public static class IPlayerDictReducerMessageStandardDependenciesImpl  {
    internal class ServerImpl : IPlayerDictReducerMessageServerDependencies
    {
      internal ServerImpl(
        IServerPlayer player, IPlayerDictServerManager manager
      ) {
        ForPlayer = player;
        Manager = manager;
      }
      public IServerPlayer ForPlayer {get;}
      public IPlayerDictServerManager Manager {get;}
      IPlayer IPlayerDictReducerMessageStandardDependencies.ForPlayer 
        => ForPlayer;
      IPlayerDictManager IPlayerDictReducerMessageStandardDependencies.Manager  
        => Manager;
    }
    internal class ClientImpl : IPlayerDictReducerMessageClientDependencies
    {
      internal ClientImpl(
        IClientPlayer player, IPlayerDictClientManager manager
      ) {
        ForPlayer = player;
        Manager = manager;
      }
      public IClientPlayer ForPlayer {get;}
      public IPlayerDictClientManager Manager {get;}
      IPlayer IPlayerDictReducerMessageStandardDependencies.ForPlayer 
        => ForPlayer;
      IPlayerDictManager 
        IPlayerDictReducerMessageStandardDependencies.Manager 
        => Manager;
    }
    public class UniversalImpl 
      : IPlayerDictReducerMessageServerDependencies
      , IPlayerDictReducerMessageClientDependencies {
      UniversalImpl(IPlayer player, IPlayerDictUniversalManager manager) {
        ForPlayer = player;
        Manager = manager;
      }
      public IPlayer ForPlayer {get;}
      public IPlayerDictUniversalManager Manager {get;}
      IClientPlayer IPlayerDictReducerMessageClientDependencies.ForPlayer 
        => (IClientPlayer)ForPlayer;
      IServerPlayer IPlayerDictReducerMessageServerDependencies.ForPlayer 
        => (IServerPlayer)ForPlayer;

      IPlayerDictClientManager 
        IPlayerDictReducerMessageClientDependencies.Manager 
        => Manager;

      IPlayerDictManager 
        IPlayerDictReducerMessageStandardDependencies.Manager 
        => Manager;

      IPlayerDictServerManager 
        IPlayerDictReducerMessageServerDependencies.Manager 
        => Manager;
    }
  }

}