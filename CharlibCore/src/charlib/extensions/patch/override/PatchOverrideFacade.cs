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
    public static void RegisterOverride<V>(
      IPatchOverrideTypeKey<V> overrideKey,
      IPlayerDictManager manager,
      IPatchChainRegistry patchRegistry
    ) {
      var pdRegistry = manager.DictKeyRegistry;
      var reducerRegistry = manager.ReducerRegistry;
      var pdKey = overrideKey.InferDictKey();
      var reducerKey = overrideKey.InferReducerKey();
      if (!pdRegistry.Has(pdKey)) {
        pdRegistry.Register(pdKey);
      }
      if (!reducerRegistry.Has(reducerKey.ReducerId)) {
        ReducerFacade.DefineReducer(
          reducerRegistry, reducerKey, overrideKey.ApplyOverrideToPlayer<V>
        );
      }
      if (!patchRegistry.HasDeclared(overrideKey)) {
        patchRegistry.Register(
          overrideKey
            .ApplyOverrideUsingPatchContext(pdKey, pdRegistry)
            .WithPriority(PatchFacade.PriorityLast),
          overrideKey
        );
      }
    }
    public static void RegisterOverrideNonGeneric(
      IPatchOverrideTypeKey overrideKey,
      IPlayerDictManager manager,
      IPatchChainRegistry patchRegistry
    ) {
      overrideKey.Register(manager, patchRegistry);
    }
  }
}