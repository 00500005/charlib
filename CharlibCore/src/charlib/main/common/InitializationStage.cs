
using System;

namespace Charlib {
  public enum InitializationStage {
    // Sorted in expected order
    Construct,
    PreStart,
    Starting,
    Started,
    Disposed,
  }
  public static class InitializationStageExt {
    public static string Name(this InitializationStage s) {
      return (Enum.GetName(typeof(InitializationStage), s))!;
    }
  }
  public class LoadOrderException : Exception {
    public LoadOrderException(string message) : base(message) { }
  }
  public class UnexpectedUsage : Exception {
    public UnexpectedUsage(string message) : base(message) { }
  }
}