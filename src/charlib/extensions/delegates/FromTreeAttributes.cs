
using System;
using System.Collections.Generic;
using HarmonyLib;
using Vintagestory.API.Datastructures;

namespace charlib {
  namespace Delegates {
    public delegate void FromTreeAttributes(
      ref object instance,
      ITreeAttribute attr
    );
  }
  public partial class MethodId {
    public static MethodId FromTreeAttributes { get; private set; } 
      = new MethodId("FromTreeAttributes", 
        typeof(Delegates.FromTreeAttributes));
  }
}