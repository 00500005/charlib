
using System.Linq;
using System.Collections.Generic;
using Charlib.PatchChain.Override;

namespace Charlib.PatchChain {
  public interface IPatchChainRegistry {
    public bool HasDeclared(string rawKey);
    public IPatchChainRegistrationCollection? GetPatchChain(string rawKey);
    public IEnumerable<IPatchChainRegistrationCollection> GetPatchChains();
    public IEnumerable<IPatchOverrideTypeKey> GetPatchOverrideTypeKeys();
    public void Register<V,C>(
      IHasPrioritizedChainFn<V,C> fn,
      IPatchTypeKey<V,C> key
    );
    public void Declare<V,C>(
      IPatchTypeKey<V,C> key
    );
  }
  public static class IPatchChainRegistryExt {
    public static void Declare<V,C,S>(
      this IPatchChainRegistry reg,
      IDiscriminator<V,C,S>? _ = null
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
      return registry.GetPatchChain(id)?.Key;
    }
    public static IPatchChainPrioritizedRegistrationCollection<V,C>?
      GetPatchChain<V,C>(
        this IPatchChainRegistry registry,
        IPatchTypeKey<V,C> key
      )
    {
      return registry.GetPatchChain(key.Id) 
        as IPatchChainPrioritizedRegistrationCollection<V,C>;
    }
    public static V InvokeChain<V,C>(
      this IPatchChainRegistry registry,
      IPatchTypeKey<V,C> key,
      C context,
      V initialValue
    ) {
      var reg = registry.GetPatchChain<V,C>(key);
      if (reg == null) {
        CharlibMod.Logger.Warning("Patch Chain '{0}' not found", key.Id);
        return initialValue;
      }
      return reg.ChainFn(context, initialValue) ?? initialValue;
    }
  }
  public static class IPatchChainRegistryImpl {
    public class Impl : IPatchChainRegistry {
      public Dictionary<string, IPatchChainRegistrationCollection> Registry =
        new Dictionary<string, IPatchChainRegistrationCollection>();
      public void Register<V,C>(
        IHasPrioritizedChainFn<V,C> fn,
        IPatchTypeKey<V,C> key
      ) {
        Declare(key);
        CharlibMod.Logger.Debug(
          "Registering PatchChain {0} for {1}",
          key.Id, key.ValueType.FullName
        );
        var reg = Registry[key.Id].HardCast<V,C>();
        reg.Add(fn);
      }
      public bool HasDeclared(string rawKey) {
        return Registry.ContainsKey(rawKey);
      }
      public IPatchChainRegistrationCollection? GetPatchChain(string rawKey) {
        return Registry.ContainsKey(rawKey) ? Registry[rawKey] : null;
      }
      public IEnumerable<IPatchChainRegistrationCollection> GetPatchChains() {
        return Registry.Values;
      }
      public void Declare<V, C>(IPatchTypeKey<V, C> key)
      {
        if (!Registry.ContainsKey(key.Id)) {
          Registry[key.Id] = PatchChainRegistrationFacade.Empty(key);
        }
        CharlibMod.Logger.Debug(
          "Declaring PatchChain {0} as {1}",
          key.Id, key.ValueType.FullName
        );
      }

      public IEnumerable<IPatchOverrideTypeKey> GetPatchOverrideTypeKeys()
      {
        return Registry.Values.Select(k => k.Key.AsPatchOverrideTypeKey());
      }
    }
  }
}