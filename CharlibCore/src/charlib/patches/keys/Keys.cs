using Charlib.PatchChain;
/**
  * TODO: figure out a better way to group like keys
  * *OR* enforce sorting in this file
  */

namespace Charlib {
  using FirepitContext = global::Charlib.PatchChain.PlayerAndBlockEntity;
  using OvenContext = global::Charlib.PatchChain.PlayerAndBlockEntity;
  public sealed class FirepitCookingTime 
    : PatchFacade.PatchTypeKey<
      float, FirepitContext, FirepitCookingTime
    > { }
  public sealed class OvenCookingTime
    : PatchFacade.PatchTypeKey<
      float, OvenContext, OvenCookingTime
    > {}
  public sealed class FirepitCookingPotCapacityLiters
    : PatchFacade.PatchTypeKey<
      float, FirepitContext, FirepitCookingPotCapacityLiters
    > {}
  public sealed class FirepitCookingPotStackSize
    : PatchFacade.PatchTypeKey<
      int, FirepitContext, FirepitCookingPotStackSize
    > {}
  public sealed class FirepitBurnChance
    : PatchFacade.PatchTypeKey<
      float, FirepitContext, FirepitBurnChance
    > {}
  public sealed class OvenBurnChance
    : PatchFacade.PatchTypeKey<
      float, OvenContext, OvenBurnChance
    > {}
  public sealed class FirepitBonusFoodChance
    : PatchFacade.PatchTypeKey<
      float, FirepitContext, FirepitBonusFoodChance
    > {}
  public sealed class OvenBonusFoodChance
    : PatchFacade.PatchTypeKey<
      float, OvenContext, OvenBonusFoodChance
    > {}
}