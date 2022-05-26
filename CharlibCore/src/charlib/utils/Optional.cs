using System;

namespace Charlib {
  /**
    * So after painfully finding out that `Nullable<struct>` is 
    * not contravariant, I decided to make an optional class
    * 
    * Optional is *only* needed with contravariant generics
    * If you don't know what that is, you probably don't need this
    * Use `?` instead (and try casting to `object?` first)
    */
  public interface IHasValue {
    public bool HasValue {get;}
  }
  public interface IOptional : IHasValue { 
    public object? _InternalValue {get;}
  }
  public interface IOptional<out V> 
    : IOptional, IHasValue
  { 
    public new V _InternalValue {get;}
  }
  public static class Optional {
    public static IOptional<V> FromNullable<V>(object? value) {
      CharlibMod.Logger.Debug(
        "Creating optional {0} = {1}", typeof(V).FullName, value
      );
      return new Optional<V>(value);
    }
    public static IOptional<V> Empty<V>() {
      return new Optional<V>(default(V));
    }
  }
  public struct Optional<V> : IOptional<V> {
    public Optional(object? nullable) {
      HasValue = nullable != null;
      _InternalValue = (V?)nullable;
    }
    public override string ToString()
    {
      return HasValue ? _InternalValue?.ToString() ?? "NULL!" : "<EMPTY>";
    }
    public bool HasValue {get;} = false;
    private V? _InternalValue {get;} = default(V);
    object? IOptional._InternalValue => _InternalValue;
    V IOptional<V>._InternalValue {
      get {
        if (_InternalValue == null) {
          throw new NullReferenceException();
        }
        return _InternalValue;
      }
    }
  }
  public static class IOptionalExt {
    public static V? AsNullable<V>(
      this IOptional<V> value
    ) {
      return value.HasValue ? value._InternalValue : default(V);
    }
    public static object? AsNullable(
      this IOptional value
    ) {
      return value.HasValue ? value._InternalValue : (object?)null;
    }
  }
}