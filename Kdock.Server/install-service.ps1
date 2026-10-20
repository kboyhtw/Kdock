$exe = Join-Path $PSScriptRoot "bin\Release\publish\Kdock.Server.exe"
if (-not (Test-Path $exe)) {
    Write-Error "Publish Kdock.Server first. Expected $exe"
    exit 1
}
sc.exe create Kdock.Meeting binPath= "`"$exe`"" start= auto
sc.exe description Kdock.Meeting "Kdock meeting API on port 12605"
sc.exe start Kdock.Meeting
