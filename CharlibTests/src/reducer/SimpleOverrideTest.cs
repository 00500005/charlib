using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Vintagestory.API.Common;
using Charlib.PatchChain;
using Charlib.PatchChain.Key;
using Charlib.PlayerDict;
using Charlib.Mocks;
using Charlib.PlayerDict.Reducer;
using Charlib.PatchChain.Override;
using NUnit.Framework;
using System;

namespace Charlib.testing {
  [TestFixture]
  public class SingleStatOverrideReducerTest {
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
        PatchOverrideFacade.RegisterOverride(
          OverrideTypeKey,
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
    public void Should_Create_ReducerMessageNonGeneric() {
      using(SingleOverrideTestParam<float, PlayerAndBlockEntity> par 
        = Of(new FirepitCookingTime())
      ) {
        string id = "1234";
        float val = 3.0f;
        var player = par.GeneratePlayer(id);
        var pd = par.Player!.Dict.AsValue;
        var key = par.OverrideTypeKey.InferReducerKey();
        
        var reducerMsg = ReducerFacade.CreateReducerMessageFromNonGeneric(key, val);
        Assert.True(reducerMsg != null);
        

      }
    }

    [Test]
    public async Task Should_Apply_ValidReducerMessageNonGeneric() {
      using(SingleOverrideTestParam<float, PlayerAndBlockEntity> par 
        = Of(new FirepitCookingTime())
      ) {
        string id = "1234";
        float val = 3.0f;
        var player = par.GeneratePlayer(id);
        var pd = par.Player!.Dict.AsValue;
        var key = par.OverrideTypeKey.InferReducerKey();
        IPlayerDict? actualPd = null;
        var promise = new SemaphoreSlim(1);
        promise.Wait();
        par.Game.State.PlayerDictManager.ApplyNonGeneric(
          player.Object, key, val, (pd, _) => {
            actualPd = pd;
            promise.Release();
          });
        await promise.WaitAsync();
        // Test claims on actualPd
        CharlibMod.Logger.Debug("{0}",actualPd);
        
        Assert.That(actualPd, Is.Not.Null);
        Assert.That(actualPd.Get(key.ReducerId), Is.EqualTo(val));

      }
    }

    [Test]
    public async Task Should_Apply_NullReducerMessageNonGeneric() {
      using(SingleOverrideTestParam<float, PlayerAndBlockEntity> par 
        = Of(new FirepitCookingTime())
      ) {
        string id = "1234";
        float? val = default(float?);
        var player = par.GeneratePlayer(id);
        var pd = par.Player!.Dict.AsValue;
        var key = par.OverrideTypeKey.InferReducerKey();
        IPlayerDict? actualPd = null;
        var promise = new SemaphoreSlim(1);
        promise.Wait();
        par.Game.State.PlayerDictManager.ApplyNonGeneric(
          player.Object, key, val, (pd, _) => {
            actualPd = pd;
            promise.Release();
          });
        await promise.WaitAsync();
        // Test claims on actualPd
        CharlibMod.Logger.Debug("{0}",actualPd);
        
        Assert.That(actualPd, Is.Not.Null);
        Assert.That(actualPd.Get(key.ReducerId), Is.EqualTo(val));

      }
    }


  }
}