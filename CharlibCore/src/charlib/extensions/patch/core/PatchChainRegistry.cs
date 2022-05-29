using System;
using System.Linq;
using System.Collections.Generic;
using Charlib.PatchChain.Override;

namespace Charlib.PatchChain {
  public interface IPatchChainRegistry {
    public bool HasDeclared(string rawKey);
    public IPatchChainRegistrationCollection? GetPatchChainNonGeneric(
      string rawKey);
    public IEnumerable<IPatchChainRegistrationCollection> GetPatchChains();
    public IEnumerable<IPatchOverrideTypeKey> GetPatchOverrideTypeKeys();
    public void Register<V,C>(
      IFullyQualifiedChainFn<V,C> fn,
      IPatchTypeKey<V,C> key
    );
    public IPatchChainRegistrationCollectionInput<V,C> Declare<V, C>(
      IPatchTypeKey<V,C> key
    );
  }
  public static class IPatchChainRegistryExt {
    public static void Declare<V,C,S>(
      this IPatchChainRegistry reg
    ) where S : IPatchTypeKey<V,C>,new() {
      reg.Declare<V,C>(new S());
    }
    public static bool HasDeclared(
      this IPatchChainRegistry registry,
      IPatchTypeKey key
    ) {
      return registry.HasDeclared(key.Id);
    }
    public static IEnumerable<IPatchTypeKey> GetPatchKeys(
      this IPatchChainRegistry registry
    ) {
      return registry.GetPatchChains().Select(k => k.Key);
    }
    public static IPatchTypeKey? GetPatchTypeKey(
      this IPatchChainRegistry registry,
      string id
    ) {
      return registry.GetPatchChainNonGeneric(id)?.Key;
    }
    public static IPatchChainRegistrationCollectionInput<V,C>? 
      GetPatchChainDeclaration<V,C>(
        this IPatchChainRegistry registry,
        IPatchTypeKey<V,C> key
      )
    {
      return registry.GetPatchChainNonGeneric(key.Id)
        as IPatchChainRegistrationCollectionInput<V,C>;
    }
    public static V InvokeChain<V,C>(
      this IPatchChainRegistry registry,
      IPatchTypeKey<V,C> key,
      C context,
      V initialValue
    ) {
      var reg = registry.GetPatchChainNonGeneric(key.Id);
      if (reg == null) {
        CharlibMod.Logger.Warning("Patch Chain '{0}' not found", key.Id);
        return initialValue;
      }
      return reg.AsInvocable<V,C>()
        .ChainFn(context, initialValue) ?? initialValue;
    }
  }
  public static class IPatchChainRegistryImpl {
    public class Impl : IPatchChainRegistry {
      public Dictionary<string, IPatchChainRegistrationCollection> Registry =
        new Dictionary<string, IPatchChainRegistrationCollection>();
      public void Register<V,C>(
        IFullyQualifiedChainFn<V,C> fn,
        IPatchTypeKey<V,C> key
      ) {
        Declare(key);
        CharlibMod.Logger.Debug(
          "Registering PatchChain {0} for {1}",
          key.Id, key.ValueType.FullName
        );
        var reg = Registry[key.Id].AsDeclarable<V,C>();
        reg.Add(fn);
      }
      public bool HasDeclared(string rawKey) {
        return Registry.ContainsKey(rawKey);
      }
      public IPatchChainRegistrationCollection? GetPatchChainNonGeneric(string rawKey) {
        return Registry.ContainsKey(rawKey) ? Registry[rawKey] : null;
      }
      public IEnumerable<IPatchChainRegistrationCollection> GetPatchChains() {
        return Registry.Values;
      }
      public IPatchChainRegistrationCollectionInput<V,C> 
        Declare<V, C>(IPatchTypeKey<V, C> key)
      {
        if (!Registry.ContainsKey(key.Id)) {
          Registry[key.Id] = PatchRegistrationFacade.Empty(key);
          CharlibMod.Logger.Debug(
            "Declaring PatchChain {0} as {1}",
            key.Id, key.ValueType.FullName
          );
        }
        var chainReg = this.GetPatchChainDeclaration(key);
        if (chainReg == null) {
          var currReg = Registry[key.Id];
          if (currReg == null) {
            throw new InvalidProgramException(
              $@"Critical program error: we could not find a key {key.Id} we just declared"
            );
          }
          if (!key.ContextType.IsAssignableTo(currReg.Key.ContextType)) {
            throw new InvalidCastException(
              $@"Patch key {key.Id} was already declared with context type {
                currReg.Key.ContextType.FullName
              }. As the requested context type [{
                currReg.Key.ValueType.FullName
              }] is not cannot be assigned to it, this is an invalid cast."
            );
          }
          if (currReg.Key.ValueType != key.ValueType) {
            throw new InvalidCastException(
              $@"Unable to declare {key.Id} with value type {
                key.ValueType.FullName
              }. Key already declared with value type {
                currReg.Key.ValueType.FullName
              }."
            );
          }
          throw new InvalidProgramException(
            $@"We thought key your declared patch key [{
              key
            }] was valid when compared to the existing key [{
              currReg.Key
            }], but C# failed to cast it."
          );
        }
        return chainReg;
      }

      public IEnumerable<IPatchOverrideTypeKey> GetPatchOverrideTypeKeys()
      {
        return Registry.Values.Select(k => k.Key.AsPatchOverrideTypeKey());
      }
    }
  }
}