$VS_VERSION="1.16.5"
$workspaceFolder="$PSScriptRoot/.."
mono `
  --debugger-agent=address=127.0.0.1:12345,server=y,suspend=n,transport=dt_socket `
  --debug "$workspaceFolder/vendor-vs/$VS_VERSION/Vintagestory.exe" `
  --addModPath "$workspaceFolder/bin/Debug/net461" `
  --addOrigin "$workspaceFolder/resources/assets"