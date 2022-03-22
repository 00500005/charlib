using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Vintagestory.API.Datastructures;

namespace charlib {

  public partial class MethodId {
    public string Id { get; private set; }
    public Type DelegateType { get; private set; }
    private MethodId(string id, Type delegateType) {
      this.Id = id;
      this.DelegateType = delegateType;
    }
  }
  public static class DelegateExt {
    public static Dictionary<Type, ClassExtensionsRegistry> ClassRegistry {get; private set;} = new Dictionary<Type, ClassExtensionsRegistry>();

    private static List<T> AppendMethods<T>(
      ref List<T> acc,
      Type sourceClass,
      MethodId extMethod
    ) where T : class {
      if (!ClassRegistry.ContainsKey(sourceClass)) {
        return acc;
      }
      IEnumerable<T> newDelegates = 
        ClassRegistry[sourceClass].DelegatesAsType<T>(
          extMethod
        );
      foreach (T t in newDelegates) {
        if (!acc.Contains(t)) {
          acc.Add(t);
        }
      }
      return acc;
    }
    private static bool IsTypeMismatch<T>(
      MethodId extMethod
    ) where T : class {
      if (typeof(T) != extMethod.DelegateType) {
        CharLib.Trace(
          "Invalid type {0} for extension method {1}."
          + " The class must match exactly.",
          typeof(T).Name, extMethod.Id
        );
        return true;
      }
      return false;
    }
    
    public static List<T> GetDirect<T>(
      Type sourceClass,
      MethodId extMethod
    ) where T : class {
      if (IsTypeMismatch<T>(extMethod)) {
        return new List<T>();
      }
      if (!ClassRegistry.ContainsKey(sourceClass)) {
        return new List<T>();
      }
      return ClassRegistry[sourceClass].DelegatesAsType<T>(
        extMethod).ToList();
    }
    public static List<T> GetAllInheriting<T>(
      Type sourceClass,
      MethodId extMethod
    ) where T : class {
      List<T> exts = new List<T>();
      if (IsTypeMismatch<T>(extMethod)) {
        return exts;
      }
      Type nextCls = sourceClass;
      while (nextCls != null && nextCls != typeof(object)) {
        AppendMethods(ref exts, nextCls, extMethod);
        foreach(Type t in nextCls.GetInterfaces()) {
          AppendMethods(ref exts, t, extMethod);
        }
        nextCls = nextCls.BaseType;
      }
      return exts;
    }
    public static void AddMethod<T>(
      Type targetClass,
      MethodId extMethodId,
      T delegateMethodInstance
    ) where T : class {
      if (IsTypeMismatch<T>(extMethodId)) {
        throw new InvalidCastException(String.Format(
          "{0} is not supported by method {1}",
          typeof(T).Name,
          extMethodId.Id
        ));
      }
      if (!ClassRegistry.ContainsKey(targetClass)) {
        ClassRegistry[targetClass] = new ClassExtensionsRegistry();
      }
      // TODO: execution order of extensions
      ClassRegistry[targetClass].AppendExtension(
        extMethodId,
        delegateMethodInstance
      );
    }
  }
  public class ClassExtensionsRegistry {
    public Dictionary<MethodId, List<object>> Extensions 
      = new Dictionary<MethodId, List<object>>();

    public override string ToString()
    {
      return String.Join("\n", Extensions.Select(i =>
        $"{i.Key.Id} = {i.Value.Count}"
      ));
    }

    public void AppendExtension(
      MethodId id,
      object methodInstance
    ) {
      if (!Extensions.ContainsKey(id)) {
        Extensions[id] = new List<object>();
      }
      Extensions[id].Add(methodInstance);
    }
    public List<object> GetExtensions(
      MethodId id
    ) {
      if (!Extensions.ContainsKey(id)) {
        return new List<object>();
      }
      return Extensions[id];
    }

    public IEnumerable<T> DelegatesAsType<T>(
      MethodId id
    ) where T : class {
      return GetExtensions(id)
        .Select(o => o as T)
        .Where(t => t != null)
        .Cast<T>();
    }
  }
}