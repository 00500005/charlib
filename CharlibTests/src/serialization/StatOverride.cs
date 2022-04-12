using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using charlib.ip.debug.cooking;
using charlib.ip;
using charlib.ip.debug;

namespace charlib.testing {
  [TestClass]
  public class StateOverrideTest {
    Mock<IPlayer>? MockPlayer;
    Mock<EntityPlayer>? MockEntity;
    Mock<ICoreAPI> MockApi = new Mock<ICoreAPI>();
    Mock<ICharLibState> MockCharlibState = new Mock<ICharLibState>();
    Mock<IWorldAccessor> MockWorld = new Mock<IWorldAccessor>();
    Dictionary<string, Mock<IPlayer>> Players = 
      new Dictionary<string, Mock<IPlayer>>();
    
    [TestInitialize]
    public void setup() {
      MockCharlibState = new Mock<ICharLibState>();
      var universalInstance = MockCharlibState.As<IModUniversalState<ICharLibState, CharLib>>();
      MockApi = new Mock<ICoreAPI>(){CallBase = true};
      MockWorld = new Mock<IWorldAccessor>();
      Players = new Dictionary<string, Mock<IPlayer>>();
      MockPlayer = null;
      MockEntity = null;
      MockApi.Setup(o => o.World).Returns(MockWorld.Object);
      MockWorld.Setup(o => o.PlayerByUid(It.IsAny<string>()))
        .Returns<string>(s => Players[s].Object);
      MockCharlibState.Setup(o => o.Api).Returns((ICoreAPI?)MockApi.Object);
      CharLib.universalInstance = universalInstance.Object;
    }
    public void GeneratePlayer(string id) {
      MockPlayer = new Mock<IPlayer>();
      MockEntity = new Mock<EntityPlayer>();
      Players[id] = MockPlayer;
      MockPlayer.Setup(p => p.Entity).Returns(MockEntity.Object);
      MockPlayer.Setup(p => p.PlayerUID).Returns(id);
      // MockEntity.Setup(e => e.PlayerUID).Returns(id);
    }
    [TestMethod]
    public void JsonRoundTrip() {
      string id = "1234";
      GeneratePlayer(id);
      PlayerDict pd = new PlayerDict(MockPlayer!.Object, false);
      var firepitBonusFood = PdWith(1.0f, FirepitBonusFoodChanceOverride.Type);
      System.Console.WriteLine("pd: '{0}'", pd);
      string serialized = JsonConvert.SerializeObject(pd);
      System.Console.WriteLine("Got serialized pd: '{0}'", serialized);
      var actual = JsonConvert.DeserializeObject<PlayerDict>(serialized);
      System.Console.WriteLine("Got deserialized pd: '{0}'", actual);
      ExpectedPdValue(firepitBonusFood);
      O PdWith<O,K,C,T>(T val, StatOverride<O,K,C,T>? instance) 
        where O : StatOverride<O,K,C,T>, new()
        where K : IKey<K,C,T> 
        where C : context.IHasPlayer {
        O @override = new O();
        @override.overrideValue = val;
        pd.Set<O>(@override);
        return @override;
      }
      void ExpectedPdValue<T>(
        T expected
      ) where T : class,IStatOverride {
        Assert.AreEqual(actual!.Get<T>(), expected);
      }
    }
  }
}