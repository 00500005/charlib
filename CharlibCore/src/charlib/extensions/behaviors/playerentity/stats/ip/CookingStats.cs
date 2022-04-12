

using System;
using System.Collections.Generic;
using ProtoBuf;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

namespace charlib {
  using FirepitContext = charlib.context.PlayerAndBlockEntity;
  using OvenContext = charlib.context.PlayerAndBlockEntity;
  namespace ip {
    namespace cooking {
      public sealed class FirepitCookingTime : IKey<
        FirepitCookingTime, FirepitContext, float
      > { }
      public sealed class OvenCookingTime : IKey<
        OvenCookingTime, OvenContext, float
      > {}
      public sealed class FirepitCookingPotCapcityLiters : IKey<
        FirepitCookingPotCapcityLiters, FirepitContext, float
      > {}
      public sealed class FirepitCookingPotStackSize : IKey<
        FirepitCookingPotStackSize, FirepitContext, int
      > {}
      public sealed class FirepitBurnChance : IKey<
        FirepitBurnChance, FirepitContext, float
      > {}
      public sealed class OvenBurnChance : IKey<
        OvenBurnChance, OvenContext, float
      > {}
      public sealed class FirepitBonusFoodChance : IKey<
        FirepitBonusFoodChance, FirepitContext, float
      > {}
      public sealed class OvenBonusFoodChance : IKey<
        OvenBonusFoodChance, OvenContext, float
      > {}
    }
    namespace debug {
      using charlib.ip.cooking;
      namespace cooking {
        [ProtoContract]
        public sealed class FirepitCookingTimeOverride 
        : StatOverride<
          FirepitCookingTimeOverride,
          FirepitCookingTime, FirepitContext, float
        > { 
          public override string ShortName() {
            return "firepit-speed";
          }
        }
        [ProtoContract]
        public sealed class OvenCookingTimeOverride 
        : StatOverride<
          OvenCookingTimeOverride,
          OvenCookingTime, OvenContext, float
        > { 
          public override string ShortName() {
            return "oven-speed";
          }
        }
        [ProtoContract]
        public sealed class FirepitCookingPotStackSizeOverride 
        : StatOverride<
          FirepitCookingPotStackSizeOverride,
          FirepitCookingPotStackSize, FirepitContext, int
        > { 
          public override string ShortName() {
            return "firepit-stacksize";
          }
        }
        [ProtoContract]
        public sealed class FirepitCookingPotCapacityLitersOverride 
        : StatOverride<
          FirepitCookingPotCapacityLitersOverride,
          FirepitCookingPotCapcityLiters, FirepitContext, float
        > { 
          public override string ShortName() {
            return "firepit-capacity";
          }
        }
        [ProtoContract]
        public sealed class FirepitBurnChanceOverride 
        : StatOverride<
          FirepitBurnChanceOverride,
          FirepitBurnChance, FirepitContext, float
        > { 
          public override string ShortName() {
            return "firepit-burnchance";
          }
        }
        [ProtoContract]
        public sealed class OvenBurnChanceOverride 
        : StatOverride<
          OvenBurnChanceOverride,
          OvenBurnChance, OvenContext, float
        > { 
          public override string ShortName() {
            return "oven-burnchance";
          }
        }
        [ProtoContract]
        public sealed class FirepitBonusFoodChanceOverride 
        : StatOverride<
          FirepitBonusFoodChanceOverride,
          FirepitBonusFoodChance, FirepitContext, float
        > { 
          public override string ShortName() {
            return "firepit-bonusfood";
          }
        }
        [ProtoContract]
        public sealed class OvenBonusFoodChanceOverride 
        : StatOverride<
          OvenBonusFoodChanceOverride,
          OvenBonusFoodChance, OvenContext, float
        > { 
          public override string ShortName() {
            return "oven-bonusfood";
          }
        }
      }
    }
  }
}