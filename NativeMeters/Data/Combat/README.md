# Combat definitions

Data comes from [ACT's public definitions](https://github.com/ravahn/FFXIV_ACT_Plugin/tree/master/Definitions). Update with PowerShell 7:

```powershell
./scripts/Update-CombatDefinitions.ps1
```

Local corrections go in `Overrides/<Job>.json`. Updates leave these alone. Rebuild after editing.
