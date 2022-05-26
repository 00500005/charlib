using Charlib;
using Charlib.PlayerDict;
using Charlib.PlayerDict.Reducer;

namespace Charlib.PatchChain.Override {
  public static class PatchOverrideFacade {
    public static IPatchOverrideTypeKey<V> OverrideTypeKey<V>(
      IPatchTypeKey<V> patchKey
    ) {
      return new IPatchOverrideTypeKeyImpl.GenericFullyQualified<V>(
        patchKey.Id,
        patchKey.ValueType,
        patchKey.ContextType
      );
    }
  }
}