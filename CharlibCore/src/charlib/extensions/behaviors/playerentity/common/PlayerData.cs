using System;
using Newtonsoft.Json;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace charlib {
  [JsonObject(MemberSerialization.OptIn)]
  public abstract class PlayerData<SELF> : EntityBehavior
    where SELF : PlayerData<SELF>
  {
    [JsonProperty]
    public string PlayerId {get => Player.PlayerUID;}
    protected IPlayer Player;
    [JsonConstructor]
    public PlayerData(string PlayerId) 
      : this(CharLib.State.Api!.World.PlayerByUid(PlayerId)) {}
    public PlayerData(
      IPlayer player, 
      bool shouldRegister=true
    ) : base(player.Entity) { 
      this.Player = player;
      if (shouldRegister) {
        this.RegisterBehavior();
      }
    }
    public virtual void RegisterBehavior() {
      bool isRegistered = entity.GetBehavior<SELF>() != null;
      if (!isRegistered) {
        entity.AddBehavior(this);
      }
    }
    public override string PropertyName()
    {
      return typeof(SELF).Name;
    }
  }
}