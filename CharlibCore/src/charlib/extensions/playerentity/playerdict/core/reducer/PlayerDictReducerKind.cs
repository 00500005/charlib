namespace Charlib.PlayerDict.Reducer {
  public enum ReducerKind {
    SERVER_ONLY,
    CLIENT_SUBSCRIBES,
    DUAL_MODELING,
    SERVER_SUBSCRIBES,
  }
  public static class ReducerKindExt {
    public static bool AllowsClientToServer(this ReducerKind kind) {
      return kind == ReducerKind.SERVER_SUBSCRIBES;
    }
    public static bool AllowsServerToClient(this ReducerKind kind) {
      return kind == ReducerKind.CLIENT_SUBSCRIBES;
    }
    public static bool IsSyncClientToServer(this ReducerKind kind) {
      return kind == ReducerKind.SERVER_SUBSCRIBES;
    }
    public static bool IsSyncServerToClient(this ReducerKind kind) {
      return kind == ReducerKind.CLIENT_SUBSCRIBES;
    }
  }
}