using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;
using System;

namespace charlib {

  public class PlayerRef {
    IPlayer? Player;
    string? Id;
    public PlayerRef(string? Id) {
      this.Id = Id;
      this.Player = Id != null ? CharLib.State.Api.World.PlayerByUid(Id) : null;
    }
  }
  public delegate void OnPlayerChange(
    ref object source, 
    PlayerRef before, 
    PlayerRef after
  );
  public interface ILastPlayer {
    public event OnPlayerChange? PlayerChange;
    public string? PlayerId {get;}
    public void SetPlayerId(ref object instance, string owner);
  }

  public static class LastPlayerDelegates
  {
    public static Delegates.FromTreeAttributes FromTreeAttributesFactory<T>(
      System.Func<T> extFactory
    ) where T : class, ILastPlayer {
      return (ref object instance, ITreeAttribute attr) => {
        string key = typeof(T).Name;
        string? owner = attr.GetString(key, null);
        if (owner != null) {
          T extension = InstanceExt.EnsureExtension<T>(
            ref instance, extFactory);
          extension.SetPlayerId(ref instance, owner);
        }
      };
    }
    public static Delegates.ToTreeAttributes ToTreeAttributesFactory<T>() 
      where T : class, ILastPlayer {
      return (ref object instance, ITreeAttribute attr) => {
        T? extension = InstanceExt.GetExtension<T>(ref instance);
        if (extension?.PlayerId != null) {
          attr.SetString(typeof(T).Name, extension.PlayerId);
        }
      };
    }
    public static void RegisterTreeAttributeDelegates<T>(
      Type target,
      System.Func<T> extFactory
    ) where T : class,ILastPlayer {
      CharLib.Trace(
        "Adding {0}.FromTreeAttributes delegate extension", 
        target.Name
      );
      DelegateExt.AddMethod<Delegates.FromTreeAttributes>(
        target,
        MethodId.FromTreeAttributes,
        FromTreeAttributesFactory<T>(extFactory)
      );
      CharLib.Trace(
        "Adding {0}.ToTreeAttributes delegate extension", 
        target.Name
      );
      DelegateExt.AddMethod<Delegates.ToTreeAttributes>(
        target,
        MethodId.ToTreeAttributes,
        ToTreeAttributesFactory<T>()
      );
    }
  }

  public class LastPlayer : ILastPlayer {
    public string? _playerId;
    public string? PlayerId { get => _playerId; }
    public void SetPlayerId(ref object instance, string? value) {
      if (_playerId != value) {
        CharLib.Trace("Updating {0} = {1}", this.GetType().Name, value);
        string? oldPlayer = _playerId;
        PlayerChange?.Invoke(
          ref instance, 
          new PlayerRef(_playerId), 
          new PlayerRef(value));
      }
      _playerId = value;
    }
    public event OnPlayerChange? PlayerChange;
    public IPlayer? GetPlayer() {
      if (PlayerId != null) {
        return CharLib.State.Api.World.PlayerByUid(PlayerId);
      }
      return null;
    }
  }

  public sealed class LastAccessingPlayer : LastPlayer {
    public static void Open(
        ref object instance,
        IPlayer player,
        ref object __result 
    ) {
      LastAccessingPlayer ext = InstanceExt
        .EnsureExtension<LastAccessingPlayer>(
          ref instance, () => new LastAccessingPlayer()
        );
      ext.SetPlayerId(ref instance, player?.PlayerUID);
    }
    public static void Initialize() {
      LastPlayerDelegates.RegisterTreeAttributeDelegates<LastAccessingPlayer>(
        typeof(InventoryBase),
        () => new LastAccessingPlayer()
      );
      CharLib.Trace(
        "Adding LastInteractingPlayer.Open delegate extension"
      );
      DelegateExt.AddMethod<Delegates.Open>(
        typeof(InventoryBase),
        MethodId.Open,
        LastAccessingPlayer.Open
      );
    }
  }
  public sealed class LastModifyingPlayer : LastPlayer {
    public static void OnItemSlotModified(
        ref object instance,
        ItemSlot slot
    ) {
      LastAccessingPlayer? lastAccessingPlayer = InstanceExt
        .GetExtension<LastAccessingPlayer>(ref instance);
      if (lastAccessingPlayer?.PlayerId != null) {
        LastModifyingPlayer lastModifyingPlayer = InstanceExt
          .EnsureExtension<LastModifyingPlayer>(ref instance, 
          () => new LastModifyingPlayer());
        lastModifyingPlayer.SetPlayerId(ref instance, 
          lastAccessingPlayer.PlayerId);
      }
    }
    public static void ActivateSlot(
      ref object __instance,
      int slotId, 
      ItemSlot sourceSlot, 
      ref ItemStackMoveOperation op,
      ref object __result
    ) {
      if (op.ActingPlayer != null) {
        LastModifyingPlayer lastModifyingPlayer = InstanceExt
          .EnsureExtension<LastModifyingPlayer>(ref __instance, 
          () => new LastModifyingPlayer());
        lastModifyingPlayer.SetPlayerId(
          ref __instance, op.ActingPlayer.PlayerUID);
      }

    }
    public static void Initialize() {
      LastPlayerDelegates.RegisterTreeAttributeDelegates<LastModifyingPlayer>(
        typeof(BlockEntityFirepit),
        () => new LastModifyingPlayer()
      );
      CharLib.Trace(
        "Adding BlockEntityFirepit.OnItemSlotModified += LastModifyingPlayer"
      );
      DelegateExt.AddMethod<Delegates.OnItemSlotModified>(
        typeof(BlockEntityFirepit),
        MethodId.OnItemSlotModified,
        LastModifyingPlayer.OnItemSlotModified
      );
      CharLib.Trace(
        "Adding BlockEntityFirepit.ActiveSlot += LastModifyingPlayer"
      );
      DelegateExt.AddMethod<Delegates.ActivateSlot>(
        typeof(BlockEntityFirepit),
        MethodId.ActivateSlot,
        LastModifyingPlayer.ActivateSlot
      );
    }
  }
  public sealed class LastFirepitCookingSlotModifyingPlayer : LastPlayer {
    public static void Initialize() {
      LastPlayerDelegates.RegisterTreeAttributeDelegates<LastFirepitCookingSlotModifyingPlayer>(
        typeof(InventoryBase),
        () => new LastFirepitCookingSlotModifyingPlayer()
      );
    }
  }
  public static class LastPlayerExtensions {
    public static LastAccessingPlayer GetLastInteractingPlayer(
      this InventoryBase inv
    ) {
      object objInv = (object)inv;
      return InstanceExt.EnsureExtension<LastAccessingPlayer>(
        ref objInv, () => new LastAccessingPlayer()
      );
    }
    public static LastModifyingPlayer GetLastModifyingPlayer(
      this InventoryBase inv
    ) {
      object objInv = (object)inv;
      return InstanceExt.EnsureExtension<LastModifyingPlayer>(
        ref objInv, () => new LastModifyingPlayer()
      );
    }

    public static LastFirepitCookingSlotModifyingPlayer GetLastCookingSlotModifyingPlayer(
      this InventoryBase inv
    ) {
      object objInv = (object)inv;
      return InstanceExt.EnsureExtension<LastFirepitCookingSlotModifyingPlayer>(
        ref objInv, () => new LastFirepitCookingSlotModifyingPlayer()
      );
    }
  }
}