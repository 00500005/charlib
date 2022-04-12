
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace charlib {
  using PlayerLookup = Dictionary<string, PlayerDict>;
  public class SaveData {
    public ICoreServerAPI Api {get; private set;}
    public string SaveFileLocation {get; private set;}
    private PlayerLookup playerData = new PlayerLookup();
    public SaveData(ICoreServerAPI api, string SaveFileLocation) {
      this.Api = api;
      this.SaveFileLocation = SaveFileLocation;
    }
    public void Register() {
			Api.Event.PlayerJoin += OnPlayerJoin;
			Api.Event.PlayerDisconnect += OnPlayerDisconnect;
			Api.Event.GameWorldSave += OnWorldSave;
    }
    public void OnPlayerJoin(IPlayer player) {
      UpdatePlayer(ref player);
    }
    public void OnPlayerDisconnect(IPlayer player) {
      StorePlayer(player);
    }
    public void OnWorldSave() {
      Save();
    }
    public void StorePlayer(IPlayer player) {
      PlayerDict? pdCurrent = player.Entity.GetBehavior<PlayerDict>();
      if (pdCurrent != null) {
        playerData[player.PlayerUID] = pdCurrent;
      }
    }
    public void UpdatePlayer(ref IPlayer player) {
      PlayerDict? pdCurrent = player.Entity.GetBehavior<PlayerDict>();
      PlayerDict? pdLastKnown = GetLastKnownPlayerDict(player.PlayerUID);
      if (pdCurrent != null && pdLastKnown != null) {
        pdCurrent.SetWith(pdLastKnown);
      } else if (pdCurrent == null && pdLastKnown != null) {
        player.Entity.AddBehavior(pdLastKnown!);
      }
    }
    public void Save() {
      string Dir = Path.GetDirectoryName(this.SaveFileLocation);
      if (!File.Exists(Dir)) {
        Directory.CreateDirectory(Dir);
      }
      string playerDataStr = JsonConvert.SerializeObject(playerData);
      File.WriteAllText(SaveFileLocation, playerDataStr);
    }
    public void Restore() {
      if (File.Exists(SaveFileLocation)) {
        playerData = JsonConvert.DeserializeObject<PlayerLookup>(
          File.ReadAllText(SaveFileLocation)) ?? new PlayerLookup();
      }
    }
    private PlayerDict? GetLastKnownPlayerDict(string playerId) {
      if (playerData.ContainsKey(playerId)) {
        return playerData[playerId];
      }
      return null;
    }
  }
}