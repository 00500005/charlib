using Charlib.PatchChain;
using Charlib.PatchChain.Override;
using Charlib.PlayerDict;
using Charlib.Mocks;
using NUnit.Framework;

namespace Charlib.testing {
  [TestFixture]
  public class PatchOverrideCanRegisterTest {
    [Test]
    public void patchOverride_should_registerWithNoGenericParameters() {
      using(var game = new MockGame()) {
        var patchKey = new FirepitCookingTime();
        var overrideKey = patchKey.AsPatchOverrideTypeKey();
        game.PatchChainRegistry.Declare(FirepitCookingTime.TypeId);
        // assert preconditions
        Assert.That(
          game.PatchChainRegistry.GetPatchChainNonGeneric(patchKey.Id),
          Is.Not.Null);
        Assert.That(
          game.PatchChainRegistry.GetPatchChainNonGeneric(patchKey.Id)!.Length,
          Is.EqualTo(0));
        Assert.False(game.DictKeyRegistry.Has(patchKey.Id));

        var numberRegistered = game.State.RegisterPatchOverrides();
        Assert.That(numberRegistered, Is.EqualTo(1));

        var patchRegistration 
          = game.PatchChainRegistry.GetPatchChainNonGeneric(patchKey.Id);
        Assert.That(patchRegistration, Is.Not.Null);
        Assert.That(patchRegistration!.Length, Is.EqualTo(1));
        Assert.That(patchRegistration.Key.ValueType,
          Is.EqualTo(patchKey.ValueType));
        Assert.That(patchRegistration.Key.ContextType,
          Is.EqualTo(patchKey.ContextType));
        var playerDictTypeKey = game.DictKeyRegistry.MaybeGet(
          overrideKey.InferDictKey());
        Assert.That(playerDictTypeKey, Is.Not.Null);
        Assert.That(playerDictTypeKey!.ValueType, Is.EqualTo(patchKey.ValueType));
      }
    }
  }
}