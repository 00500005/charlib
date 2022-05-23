
using System;
using System.Linq;
using System.Linq.Expressions;
using Charlib.PlayerDict;
using Charlib.PatchChain;
using Moq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Charlib.PlayerDict.Reducer;
using System.IO;
using NUnit.Framework;

namespace Charlib.Mocks {
  public static class IMaybeMock {
    public static IMaybeMock<M>.Deferred Defer<M>() 
      where M : class {
      return new IMaybeMock<M>.Deferred();
    }
    public static IMaybeMock<M>.MockImpl Mock<M>() 
      where M : class {
      return new IMaybeMock<M>.MockImpl();
    }
    public static IMaybeMock<M>.StaticImpl Of<M>(M instance)
      where M : class {
      return new IMaybeMock<M>.StaticImpl(instance);
    }
    public static IMaybeMock<M>.Impl Of<M>(System.Func<M> instance)
      where M : class {
      return new IMaybeMock<M>.Impl(instance);
    }
  }
  public interface IMaybeMock<M> 
    where M : class {
    public Mock<M>? AsMock {get;}
    public M AsValue {get;}
    public interface IValue : IMaybeMock<M> {
      public System.Func<M> Constructor {get;}
      public void SetValue(M value);
    }
    public class Deferred : IMaybeMock<M>, IValue
    {
      public IMaybeMock<M> Resolve(IMaybeMock<M> inner) {
        Impl = inner;
        return inner;
      }
      public IMaybeMock<M> ResolveWithValue(System.Func<M> value) {
        Impl = new IMaybeMock<M>.Impl(value);
        return Impl;
      }
      public IMaybeMock<M> ResolveWithMock(Mock<M>? useMock=null) {
        Impl = new IMaybeMock<M>.MockImpl(useMock);
        return Impl;
      }
      public void SetValue(M value) => (Impl as IValue)!.SetValue(value);
      public IMaybeMock<M>? Impl {get;private set;}
      public Mock<M>? AsMock => Impl!.AsMock;
      public M AsValue => Impl!.AsValue;
      public System.Func<M> Constructor => (Impl as Impl)!.Constructor;
    }
    public class MockImpl : IMaybeMock<M>
    {
      public MockImpl(Mock<M>? useMock=null) {
        AsMock = useMock ?? new Mock<M>();
      }
      public Mock<M> AsMock {get;}
      public M AsValue => AsMock!.Object;
    }
    
    public class StaticImpl : IMaybeMock<M> {
      public Mock<M>? AsMock => null;
      public M AsValue {get;}
      public StaticImpl(M value) {
        AsValue = value;
      }
    }
    public class Impl : IValue {
      public Mock<M>? AsMock => null;
      public M AsValue {get;internal set;}
      public System.Func<M> Constructor {get;}
      public Impl(System.Func<M> constructor) {
        Constructor = constructor;
        AsValue = Constructor();
      }
      void IMaybeMock<M>.IValue.SetValue(M value) {
        AsValue = value;
      }
    }
  }
  public static class IMaybeMockExt {
    public static bool HasResolved<M>(
      this IMaybeMock<M> maybeMock
    ) where M : class {
      var asDeferred = maybeMock as IMaybeMock<M>.Deferred;
      if (asDeferred != null && asDeferred.Impl == null) {
        return false;
      }
      return true;
    }
    public static IMaybeMock<M> Reset<M>(
      this IMaybeMock<M> maybeMock
    ) where M : class {
      if (maybeMock.AsMock != null) {
        maybeMock.AsMock.Reset();
      } else {
        var val = maybeMock as IMaybeMock<M>.IValue;
        if (val != null) {
          throw new NotSupportedException();
        }
        val!.SetValue(val.Constructor());
      }
      return maybeMock;
    }
    public static IMaybeMock<M> IfNotMock<M>(
      this IMaybeMock<M> maybeMock,
      System.Action<M> fn
    ) where M : class {
      if (maybeMock.HasResolved() && maybeMock.AsMock == null) {
        fn(maybeMock.AsValue);
      }
      return maybeMock;
    }
    public static X? IfNotMock<M,X>(
      this IMaybeMock<M> maybeMock,
      System.Func<M,X> fn
    ) where M : class
      where X : class {
      if (maybeMock.HasResolved() && maybeMock.AsMock == null) {
        return fn(maybeMock.AsValue);
      }
      return null;
    }
    public static IMaybeMock<M> IfMock<M>(
      this IMaybeMock<M> maybeMock,
      System.Action<Mock<M>> fn
    ) where M : class {
      if (maybeMock.HasResolved() && maybeMock.AsMock != null) {
        fn(maybeMock.AsMock);
      }
      return maybeMock;
    }
    public static Moq.Language.Flow.ISetup<M>? SetupMock<M>(
      this IMaybeMock<M> maybeMock,
      Expression<System.Action<M>> fn
    ) where M : class {
      if (maybeMock.HasResolved() && maybeMock.AsMock != null) {
        return maybeMock.AsMock.Setup(fn);
      }
      return null;
    }
    public static Moq.Language.Flow.ISetup<M, T>? SetupMock<M, T>(
      this IMaybeMock<M> maybeMock,
      Expression<System.Func<M, T>> fn
    ) where M : class {
      if (maybeMock.HasResolved() && maybeMock.AsMock != null) {
        return maybeMock.AsMock.Setup(fn);
      }
      return null;
    }
    public static X? IfMock<M,X>(
      this IMaybeMock<M> maybeMock,
      System.Func<Mock<M>,X> fn
    ) where M : class
      where X : class {
      if (maybeMock.HasResolved() && maybeMock.AsMock != null) {
        return fn(maybeMock.AsMock);
      }
      return null;
    }
  }
  public interface ISideSpecific<SELF,G,C,S>
    where SELF : ISideSpecific<SELF,G,C,S>
    where G : class
    where C : class
    where S : class {
    public Mock<G> General {get;}
    public Mock<C> Client {get;}
    public Mock<S> Server {get;}
    public class Impl : Mock<G>, ISideSpecific<SELF, G, C, S>
    {
      public Mock<G> General => this;
      public Mock<C> Client {get;}
      public Mock<S> Server {get;}
      public virtual void Initialize() {}
      public virtual SELF Reset() {
        Client.Reset();
        Server.Reset();
        General.Reset();
        return (SELF)(ISideSpecific<SELF,G,C,S>)(this);
      }
      public Impl() {
        Client = General.As<C>();
        Server = General.As<S>();
        Initialize();
      }
    }
  }
  public static class SideSpecificExt {
    public static SELF Reset<
      SELF,G,C,S
    >(
      this ISideSpecific<SELF,G,C,S> self
    ) 
      where SELF : ISideSpecific<SELF,G,C,S> 
      where G : class
      where C : class
      where S : class 
    {
      self.Client.Reset();
      self.Server.Reset();
      self.General.Reset();
      return (SELF)self;
    }
    public static ISideSpecific<SELF,G,C,S> Cast<
      SELF,G,C,S
    >(
      this ISideSpecific<SELF,G,C,S> self
    ) 
      where SELF : ISideSpecific<SELF,G,C,S> 
      where G : class
      where C : class
      where S : class 
    {
        return self;
    }
    public static SELF Attach<
      SELF,G,C,S,
      O,OG,OC,OS
    >(
      this ISideSpecific<SELF,G,C,S> self,
      ISideSpecific<O,OG,OC,OS> other,
      Expression<System.Func<G,OG>>? getGeneral=null,
      Expression<System.Func<C,OC>>? getClient=null,
      Expression<System.Func<S,OS>>? getServer=null
    ) 
      where SELF : ISideSpecific<SELF,G,C,S>
      where G : class
      where C : class
      where S : class 
      where O : ISideSpecific<O,OG,OC,OS>
      where OG : class 
      where OC : class
      where OS : class 
    {
      if (getGeneral != null) {
        self.General.Setup(getGeneral).Returns(other.General.Object);
      }
      if (getServer != null) {
        self.Server.Setup(getServer).Returns(other.Server.Object);
      }
      if (getClient != null) {
        self.Client.Setup(getClient).Returns(other.Client.Object);
      }
      return (SELF)self;
    }
  }
  public class MockPlayer : ISideSpecific<
    MockPlayer, IPlayer, IClientPlayer, IServerPlayer
  >.Impl {
    public Mock<EntityPlayer> Entity {get;}
      = new Mock<EntityPlayer>();
    public IMaybeMock<IPlayerDict>.Deferred Dict {get;} 
      = IMaybeMock.Defer<IPlayerDict>();
    public MockPlayer(
      string? uuid = null
    ) : base() {
      Initialize(uuid);
    }
    public void Initialize(string? uuid = null) {
      Setup(p => p.Entity).Returns(Entity.Object);
      Setup(p => p.PlayerUID).Returns(uuid ?? Guid.NewGuid().ToString());
    }
    public MockPlayer WithRealDict(
      IPlayerDictTypeKeyRegistry registry
    ) {
      Dict.ResolveWithValue(() => {
        var implDict = PlayerDictFacade.CreateAndRegister(
          this.Object, registry);
        Entity.Setup(p => p.GetBehavior<PlayerDictEntity>())
          .Returns(implDict);
        return implDict;
      });
      return this;
    }
    public MockPlayer WithMockDict(
      Mock<PlayerDictEntity>? dictMock = null
    ) {
      var implDict = dictMock ?? new Mock<PlayerDictEntity>();
      Dict.ResolveWithMock(implDict.As<IPlayerDict>());
      Entity.Setup(p => p.GetBehavior<PlayerDictEntity>())
        .Returns(implDict.Object);
      return this;
    }
  }
  public class MockApi : ISideSpecific<
    MockApi, ICoreAPI, ICoreClientAPI, ICoreServerAPI
  >.Impl {
    public MockApi Attach(
      MockNetworkApi network
    ) {
      this.Attach(
        network, 
        api => api.Network,
        api => api.Network,
        api => api.Network
      );
      return this;
    }
  }
  public class MockNetworkApi : ISideSpecific<
    MockNetworkApi, INetworkAPI, IClientNetworkAPI, IServerNetworkAPI
  >.Impl {
    public MockNetworkApi Attach(
      MockChannel channel
    ) {
      this.Attach(
        channel, 
        api => api.GetChannel(It.IsAny<string>()),
        api => api.GetChannel(It.IsAny<string>()),
        api => api.GetChannel(It.IsAny<string>())
      );
      return this;
    }
  }
  public class MockChannel : ISideSpecific<
    MockChannel, INetworkChannel, IClientNetworkChannel, IServerNetworkChannel
  >.Impl {
  }
  public class MockWorld : ISideSpecific<
    MockWorld, IWorldAccessor, IClientWorldAccessor, IServerWorldAccessor
  >.Impl {
    public MockWorld Attach(
      MockPlayer player,
      bool isClientPlayer = true
    ) {
      this.Cast().Attach(
        player,
        w => w.PlayerByUid(player.Object.PlayerUID),
        isClientPlayer ? w => w.Player : null,
        null
      );
      return this;
    }
  }

  public class MockGame : IDisposable {
    public delegate IMaybeMock<T> WithValue<T>(MockGame game) where T : class;
    public struct Options {
      // public WithValue<IPlayerDictTypeKeyRegistry>? DictKeyRegistry;
      // public WithValue<IPlayerDictReducerRegistry>? ReducerRegistry;
      // public WithValue<IPatchChainRegistry>? PatchChainRegistry;
      // public WithValue<IPlayerDictManager>? Manager;
      // public WithValue<IStringConstructorRegistry>? StringConstructorRegistry;
    }
    /**
    TODO: SaveData + ServerCommands
    */
    public CharlibMod Mod {get;}
    public ICharlibState State {get;}
    public MockApi Api {get;} = new MockApi();
    public MockNetworkApi Network {get;} = new MockNetworkApi();
    public MockChannel Channel {get;} = new MockChannel();
    public MockWorld World {get;} = new MockWorld();
    public IPlayerDictTypeKeyRegistry DictKeyRegistry 
      => State.PlayerDictManager.DictKeyRegistry;
    public IPlayerDictManager Manager 
      => State.PlayerDictManager;
    public IPlayerDictReducerRegistry ReducerRegistry 
      => State.PlayerDictManager.ReducerRegistry;
    public IPatchChainRegistry PatchChainRegistry
      => State.PatchChainRegistry;
    public IStringConstructorRegistry StringConstructorRegistry 
      => State.StringConstructorRegistry;
    public static ILogger CreateLogger(TextWriter output) {
      Mock<ILogger> logger = new Mock<ILogger>();
      SetupLog(nameof(ILogger.Audit));
      SetupLog(nameof(ILogger.Build));
      SetupLog(nameof(ILogger.Chat));
      SetupLog(nameof(ILogger.Debug));
      SetupLog(nameof(ILogger.Error));
      SetupLog(nameof(ILogger.Event));
      SetupLog(nameof(ILogger.Fatal));
      SetupLog(nameof(ILogger.Notification));
      SetupLog(nameof(ILogger.StoryEvent));
      SetupLog(nameof(ILogger.VerboseDebug));
      SetupLog(nameof(ILogger.Warning));
      SetupBaseLog();
      return logger.Object;
      void SetupLog(string name) {
        var loggerParam = Expression.Parameter(typeof(ILogger), "l");
        var fn1 = typeof(ILogger).GetMethods()
          .Where(m => m.Name.Equals(name))
          .Where(m => m.GetParameters().Length == 1)
          .Single();
        var fn2 = typeof(ILogger).GetMethods()
          .Where(m => m.Name.Equals(name))
          .Where(m => m.GetParameters().Length == 2)
          .Single();
        var itIsAny = typeof(It).GetMethods()
          .Where(m => m.Name == "IsAny")
          .Where(m => m.GetParameters().Length == 0)
          .Where(m => m.GetGenericArguments().Length == 1)
          .Single();
        var itIsAnyString = itIsAny.MakeGenericMethod(typeof(string));
        var itIsAnyObjects = itIsAny.MakeGenericMethod(typeof(object[]));
        var matchFn = Expression.Lambda<Action<ILogger>>(
          Expression.Call(loggerParam, fn1, 
            Expression.Call(null, itIsAnyString)
          ), loggerParam
        );
        var matchFFn = Expression.Lambda<Action<ILogger>>(
          Expression.Call(loggerParam, fn2, 
            Expression.Call(null, itIsAnyString),
            Expression.Call(null, itIsAnyObjects)
          ), loggerParam
        );
        logger.Setup(matchFn).Callback(DoLog(name));
        logger.Setup(matchFFn).Callback(DoLogF(name));
      }
      void SetupBaseLog() {
        logger.Setup(l => l.Log(
            It.IsAny<EnumLogType>(), 
            It.IsAny<string>()
          ))
          .Callback((EnumLogType t, string m) => {
            DoLogLog(t, m);
          });
        logger.Setup(l => l.Log(
            It.IsAny<EnumLogType>(),
            It.IsAny<string>(), 
            It.IsAny<object[]>()
          ))
          .Callback((EnumLogType t, string m, object[] a) => {
            DoLogLogF(t, m, a);
          });
      }
      void Write(string prefix, string msg) {
        output.WriteLine($"[{prefix}]: {msg}");
      }
      Action<string> DoLog(string prefix) {
        return m => Write(prefix, m);
      }
      Action<string, object[]> DoLogF(string prefix) {
        return (string f, object[] args) => {
          Write(prefix, String.Format(f, args));
        };
      }
      void DoLogLog(EnumLogType logType, string m) {
        Write(logType.ToString(), m);
      }
      void DoLogLogF(EnumLogType logType, string f, object[] args) {
        Write(logType.ToString(), String.Format(f, args));
      }
    }
    public MockGame(Options? options=null) {
      var logger = CreateLogger(TestContext.Out);
      Mod = new CharlibMod(logger);
      Api.Attach(Network);
      Api.Attach(World);
      Network.Attach(Channel);
      Api.Setup(api => api.Side).Returns(EnumAppSide.Universal);
      Mod.StartPre(Api.Object);
      State = Mod.State!;
    }
    private MockGame DoResolve<M>(
      IMaybeMock<M>.Deferred toResolve, 
      WithValue<M>? val
    ) where M : class {
      if (val != null) { toResolve.Resolve(val(this)); }
      else { toResolve.Resolve(IMaybeMock.Mock<M>()); }
      return this;
    }
    public MockPlayer AddPlayer(
      string? uuid = null,
      bool isClientPlayer = true
    ) {
      MockPlayer player = new MockPlayer(uuid);
      World.Attach(player, isClientPlayer);
      return player;
    }

    public void Dispose()
    {
      State.Dispose();
    }
  }
}