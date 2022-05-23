
using System.Collections.Generic;
using System.IO;
using Charlib.PlayerDict;
using Newtonsoft.Json;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Charlib {
  using PlayerLookup = Dictionary<string, PlayerDictSerialized>;
  public interface IPlayerSaveData {
    public void Register();
    public void StorePlayer(IServerPlayer player);
    public void OnPlayerJoinsServer(IServerPlayer player);
    public void Save();
    public void Restore();
  }
  public class PlayerSaveData : IHasCharlibState.Impl, IPlayerSaveData {
    public ICoreServerAPI Api {get; private set;}
    public string SaveFileLocation {get; private set;}
    private PlayerLookup CachedPlayerData = new PlayerLookup();
    public PlayerSaveData(
      ICharlibState state,
      ICoreServerAPI api, 
      string SaveFileLocation
    ) : base(state) {
      this.Api = api;
      this.SaveFileLocation = SaveFileLocation;
    }
    public void Register() {
			Api.Event.PlayerJoin += OnPlayerJoin;
			Api.Event.PlayerDisconnect += OnPlayerDisconnect;
			Api.Event.GameWorldSave += OnWorldSave;
    }
    public void OnPlayerJoin(IServerPlayer player) {
      OnPlayerJoinsServer(player);
    }
    public void OnPlayerDisconnect(IServerPlayer player) {
      StorePlayer(player);
    }
    public void OnWorldSave() {
      Save();
    }
    public void StorePlayer(IServerPlayer player) {
      CachedPlayerData[player.PlayerUID] = player
        .EnsurePlayerDict(
          State.PlayerDictManager.DictKeyRegistry
        )
        .Serialize();
    }
    public void OnPlayerJoinsServer(IServerPlayer player) {
      IPlayerDict pdCurrent = player.EnsurePlayerDict(
        State.PlayerDictManager.DictKeyRegistry
      );
      IPlayerDict? pdLastKnown = RestoreFromCache(player);
      if (pdLastKnown != null) {
        pdCurrent.SetWith(pdLastKnown);
        State.PlayerDictManager.RequiresServer().SendSync(player);
      }
    }
    public void Save() {
      string Dir = Path.GetDirectoryName(this.SaveFileLocation)!;
      if (!File.Exists(Dir)) {
        Directory.CreateDirectory(Dir);
      }
      string playerDataStr = JsonConvert.SerializeObject(CachedPlayerData);
      File.WriteAllText(SaveFileLocation, playerDataStr);
    }
    public void Restore() {
      if (File.Exists(SaveFileLocation)) {
        CachedPlayerData = JsonConvert.DeserializeObject<PlayerLookup>(
          File.ReadAllText(SaveFileLocation)) ?? new PlayerLookup();
      }
    }
    private IPlayerDict? RestoreFromCache(IPlayer player) {
      if (CachedPlayerData.ContainsKey(player.PlayerUID)) {
        return CachedPlayerData[player.PlayerUID].DeserializeInto(
          State.PlayerDictManager.DictKeyRegistry,
          player
        );
      }
      return null;
    }
  }
}