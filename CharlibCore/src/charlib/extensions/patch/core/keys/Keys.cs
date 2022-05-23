
namespace Charlib.PatchChain.Key
{
  using FirepitContext = global::Charlib.PatchChain.PlayerAndBlockEntity;
  using OvenContext = global::Charlib.PatchChain.PlayerAndBlockEntity;
  public sealed class FirepitCookingTime 
    : PatchTypeKeyImpl.PatchTypeKeyWithValueAndContext<
      float, FirepitContext, FirepitCookingTime
    > { }
  public sealed class OvenCookingTime
    : PatchTypeKeyImpl.PatchTypeKeyWithValueAndContext<
      float, OvenContext, OvenCookingTime
    > {}
  public sealed class FirepitCookingPotCapacityLiters
    : PatchTypeKeyImpl.PatchTypeKeyWithValueAndContext<
      float, FirepitContext, FirepitCookingPotCapacityLiters
    > {}
  public sealed class FirepitCookingPotStackSize
    : PatchTypeKeyImpl.PatchTypeKeyWithValueAndContext<
      int, FirepitContext, FirepitCookingPotStackSize
    > {}
  public sealed class FirepitBurnChance
    : PatchTypeKeyImpl.PatchTypeKeyWithValueAndContext<
      float, FirepitContext, FirepitBurnChance
    > {}
  public sealed class OvenBurnChance
    : PatchTypeKeyImpl.PatchTypeKeyWithValueAndContext<
      float, OvenContext, OvenBurnChance
    > {}
  public sealed class FirepitBonusFoodChance
    : PatchTypeKeyImpl.PatchTypeKeyWithValueAndContext<
      float, FirepitContext, FirepitBonusFoodChance
    > {}
  public sealed class OvenBonusFoodChance
    : PatchTypeKeyImpl.PatchTypeKeyWithValueAndContext<
      float, OvenContext, OvenBonusFoodChance
    > {}
}