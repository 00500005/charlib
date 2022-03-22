using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace charlib {
  public static class InstanceExt {
    public static ConditionalWeakTable<object, InstanceExtensionsRegistry> ObjectTable = 
      new ConditionalWeakTable<object, InstanceExtensionsRegistry>();
    
    private static InstanceExtensionsRegistry Get(ref object key) {
      return ObjectTable.GetValue(key, (_) => new InstanceExtensionsRegistry());
    }
    public static object? GetExtension(ref object key, Type type) {
      InstanceExtensionsRegistry lib = Get(ref key);
      if (!lib.Extensions.ContainsKey(type)) {
        return null;
      } else {
        return lib.Extensions[type];
      }
    }
    public static T? GetExtension<T>(ref object key) 
      where T : class {
      return (T?)GetExtension(ref key, typeof(T));
    }
    public static T EnsureExtension<T>(ref object key, Func<T> defaultValue) 
      where T : class {
      InstanceExtensionsRegistry lib = Get(ref key);
      Type type = typeof(T);
      if (!lib.Extensions.ContainsKey(type)) {
        lib.Extensions[type] = defaultValue();
      }
      return (T)lib.Extensions[type];
    }

    public static T SetExtension<T>(ref object key, T value) 
      where T : class {
      InstanceExtensionsRegistry lib = Get(ref key);
      lib.Extensions[typeof(T)] = value;
      return value;
    }

  }
  public class InstanceExtensionsRegistry {
    public Dictionary<Type, object> Extensions = new Dictionary<Type, object>();
  }
}