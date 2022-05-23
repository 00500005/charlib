using System;
using System.Collections.Generic;

namespace Charlib {
  public interface IStringConstructorRegistry {
    public IEnumerable<Type> SupportedTypes();
    public Func<string, IOptional<V>> ForType<V>();
    public Func<string, IOptional> ForType(Type t);
    public bool HasType(Type t);
    public IStringConstructorRegistry Register(
      Type type, Func<string, IOptional> fn
    );
  }
  public static class IStringConstructorRegistryImpl {
    public class Impl : IStringConstructorRegistry
    {
      private Dictionary<Type, Func<string, IOptional>> Lookup 
        = new Dictionary<Type, Func<string, IOptional>>();
      public IEnumerable<Type> SupportedTypes() {
        return Lookup.Keys;
      }
      public Func<string, IOptional<V>> ForType<V>() {
        return (Func<string, IOptional<V>>)ForType(typeof(V));
      }
      public Func<string, IOptional> ForType(Type t) {
        if (!Lookup.ContainsKey(t)) {
          throw new NotImplementedException(t.FullName);
        }
        return Lookup[t];
      }
      public IStringConstructorRegistry Register(
        Type type, Func<string, IOptional> fn
      ) {
        if (
          Lookup.ContainsKey(type)
          && Lookup[type] != fn
        ) {
          throw new InvalidOperationException(
            $"Already registered StringConstructor of type {type.Name}"
          );
        }
        CharlibMod.Logger.Debug(
          "Registering StringConstructor for: {0}", type.Name
        );
        Lookup[type] = fn;
        return this;
      }
      public bool HasType(Type t)
      {
        return Lookup.ContainsKey(t);
      }
    }
  }
  
  public static class StringConstructorExt {
    public static IStringConstructorRegistry Register<V>(
      this IStringConstructorRegistry reg,
      Func<string, IOptional<V>> fn
    ) {
      return reg.Register(typeof(V), fn);
    }
    public static IStringConstructorRegistry Register<V>(
      this IStringConstructorRegistry reg,
      Func<string, V?> fn
    ) {
      return reg.Register(typeof(V), s => Optional.FromNullable<V>(fn(s)));
    }
  }
  public static class StringConstructorMethods {
    public static IOptional<int> IntFromString(string value) {
      int result = 0;
      return int.TryParse(value, out result) 
        ? Optional.FromNullable<int>(result)
        : Optional.Empty<int>();
    }
    public static IOptional<float> FloatFromString(string value) {
      float result = 0;
      return float.TryParse(value, out result) 
        ? Optional.FromNullable<float>(result)
        : Optional.Empty<float>();
    }
  }
}