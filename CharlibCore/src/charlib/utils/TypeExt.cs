using System;

namespace Charlib {
  public static class TypeExt {
    public static bool IsAssignableTo<T>(
      this Type self
    ) {
      return typeof(T).IsAssignableFrom(self);
    }
    public static bool IsAssignableTo(
      this Type self,
      Type other
    ) {
      return other.IsAssignableFrom(self);
    }
    public static bool IsAssignableFrom<T>(
      this Type self
    ) {
      return self.IsAssignableFrom(typeof(T));
    }
  }
}