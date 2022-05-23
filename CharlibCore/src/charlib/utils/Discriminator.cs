namespace Charlib {
  public static class Discriminator {
    public static IDiscriminator<T> Identify<T>() {
      return new IDiscriminator<T>.Impl();
    }
    public static IDiscriminator<T1,T2> Identify<T1, T2>() {
      return new IDiscriminator<T1,T2>.Impl();
    }
    public static IDiscriminator<T1,T2,T3> Identify<T1, T2, T3>() {
      return new IDiscriminator<T1,T2,T3>.Impl();
    }
  }
  public interface IDiscriminator<T> {
    public class Impl : IDiscriminator<T> {}
  }
  public interface IDiscriminator<T1,T2> : IDiscriminator<T1> { 
    public new class Impl : IDiscriminator<T1,T2> {}
  }
  public interface IDiscriminator<T1,T2,T3> : IDiscriminator<T1,T2> { 
    public new class Impl : IDiscriminator<T1,T2,T3> {}
  }
}