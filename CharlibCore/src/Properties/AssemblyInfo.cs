using Vintagestory.API.Common;

[assembly: ModInfo( 
  "Character Library Core",
  "charlib_core",
	Description = "Library that adds various player stats for world interactions",
  Website     = "",
	Authors     = new []{  "00500005" },
  Version = "0.0.0-dev.0",
  RequiredOnClient = true,
  RequiredOnServer = true,
  Side = nameof(EnumAppSide.Universal)
)]