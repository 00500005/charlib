using Vintagestory.API.Common;
using Charlib.PatchChain;
using Charlib.PlayerDict;
using Charlib.Mocks;
using Charlib.PlayerDict.Reducer;
using Charlib.PatchChain.Override;
using NUnit.Framework;
using System;

namespace Charlib.testing {
  [TestFixture]
  public class SingleStatOverrideTest {
    public class SingleOverrideTestParam<V,C> 
      : IDisposable
      where C : IHasPlayer 
    {
      public MockGame Game {get;}
      public IPatchOverrideTypeKey<V> OverrideTypeKey {get;} 
      public MockPlayer? Player;
      public IPlayerDictTypeKey<V> DictTypeKey 
        => OverrideTypeKey.InferDictKey();
      public MockPlayer GeneratePlayer(string id) {
        Player = Game.AddPlayer(id);
        Player.WithRealDict(Game.DictKeyRegistry);
        return Player;
      }

      public void Dispose()
      {
        Game.Dispose();
      }

      public SingleOverrideTestParam(
        IPatchTypeKey<V,C> Key
      ) {
        OverrideTypeKey = PatchOverrideFacade.OverrideTypeKey(Key);
        Game = new MockGame();
        OverrideTypeKey.Register(
          Game.Manager,
          Game.PatchChainRegistry
        );
        Player = null;
      }
    }
    public SingleOverrideTestParam<V,C> Of<V,C>(
      IPatchTypeKey<V,C> Key
    ) where C : IHasPlayer {
      return new SingleOverrideTestParam<V,C>(Key);
    }
    
    [Test]
    public void MockLoggerTest() {
      using(var par = Of(new OvenCookingTime())) {
        par.Game.State.Notification("This is a test notification");
        par.Game.State.Warning("This is a test warning");
        par.Game.State.Error("This is a test error");
        par.Game.State.Log(EnumLogType.Error, "This is a test (raw) error");
      }
    }
    [Test]
    public void ProtoRoundTrip() {
      using(SingleOverrideTestParam<float, PlayerAndBlockEntity> par 
        = Of(new FirepitBonusFoodChance())
      ) {
        string id = "1234";
        float testValue = 3.0f;
        var player = par.GeneratePlayer(id);
        var pd = par.Player!.Dict.AsValue;
        var val = par.OverrideTypeKey.ValueFromString(
          par.Game.StringConstructorRegistry,
          testValue.ToString()
        );
        pd.Set<float>(
          par.OverrideTypeKey.InferDictKey(),
          val
        );
        System.Console.WriteLine("pd: '{0}'", pd);
        byte[] serialized = pd.Serialize().SerializeProto();
        System.Console.WriteLine($@"Got serialized pd: '{
          BitConverter.ToString(serialized)
        }'");
        var actual = serialized.DeserializeProto<PlayerDictSerialized>()
          .AsValue(new PlayerDictSerializedDependencies() {
            Player = player.Object,
            Registry = par.Game.DictKeyRegistry
          });
        System.Console.WriteLine("Got deserialized pd: '{0}'", actual);
        Assert.That(
          actual!.Get(par.DictTypeKey), 
          Is.EqualTo(pd!.Get(par.DictTypeKey))
        );
      }
    }
  }
}