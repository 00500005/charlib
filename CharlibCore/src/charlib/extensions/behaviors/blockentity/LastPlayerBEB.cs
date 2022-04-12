using System;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace charlib
{
  public class LastPlayerBEB : BlockEntityBehavior
  {
    public string? LastAccessingPlayerId;
    public string? LastModifyingPlayerId;
    public LastPlayerBEB(BlockEntity blockentity) : base(blockentity) { }
    public static string FieldName(string field) {
      return $"{nameof(LastPlayerBEB)}.{field}";
    }
    public static IPlayer? GetLastModifyingPlayer(BlockEntity? be) {
      string? id = be?.GetBehavior<LastPlayerBEB>()?.LastModifyingPlayerId;
      return id == null ? null : be?.Api?.World?.PlayerByUid(id);
    }
    public static IPlayer? GetLastAccessingPlayer(BlockEntity? be) {
      string? id = be?.GetBehavior<LastPlayerBEB>()?.LastAccessingPlayerId;
      return id == null ? null : be?.Api?.World?.PlayerByUid(id);
    }
    public override void FromTreeAttributes(
      ITreeAttribute tree,
      IWorldAccessor worldAccessForResolve
    ) {
      LastAccessingPlayerId = tree.GetString(
        FieldName(nameof(LastPlayerBEB.LastAccessingPlayerId)),
        LastAccessingPlayerId
      );
      LastModifyingPlayerId = tree.GetString(
        FieldName(nameof(LastPlayerBEB.LastModifyingPlayerId)),
        LastModifyingPlayerId
      );
    }
    public void OnAccess(IPlayer player) {
      this.LastAccessingPlayerId = player.PlayerUID;
    }
    public void OnModify() {
      this.LastModifyingPlayerId = LastAccessingPlayerId;
    }
    public void OnModify(IPlayer player) {
      this.LastAccessingPlayerId = player.PlayerUID;
      this.LastModifyingPlayerId = player.PlayerUID;
    }
    public override void ToTreeAttributes(
      ITreeAttribute tree
    ) {
      if (LastAccessingPlayerId != null) {
        tree.SetString(
          FieldName(nameof(LastPlayerBEB.LastAccessingPlayerId)),
          LastAccessingPlayerId
        );
      }
      if (LastModifyingPlayerId != null) {
        tree.SetString(
          FieldName(nameof(LastPlayerBEB.LastModifyingPlayerId)),
          LastModifyingPlayerId
        );
      }
    }
    public IPlayer? LastAccessingPlayer()
    {
      return this.LastAccessingPlayer == null 
        ? null 
        : this.Api.World.PlayerByUid(this.LastAccessingPlayerId);
    }
    public IPlayer? LastModifyingPlayer()
    {
      return this.LastModifyingPlayer == null 
        ? null 
        : this.Api.World.PlayerByUid(this.LastModifyingPlayerId);
    }
  }
}