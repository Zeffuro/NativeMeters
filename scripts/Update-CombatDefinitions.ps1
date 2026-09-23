param([string]$Ref = 'master')

$ErrorActionPreference = 'Stop'
$Repository = 'ravahn/FFXIV_ACT_Plugin'
$OutputPath = Join-Path $PSScriptRoot '../NativeMeters/Data/Combat/Definitions'
$Headers = @{ 'User-Agent' = 'NativeMeters' }
if ($env:GH_TOKEN) { $Headers.Authorization = "Bearer $env:GH_TOKEN" }

$ApiUrl = "https://api.github.com/repos/$Repository"
$Commits = Invoke-RestMethod "$ApiUrl/commits?path=Definitions&sha=$([Uri]::EscapeDataString($Ref))&per_page=1" -Headers $Headers
$Commit = $Commits[0].sha
if ($Commit -notmatch '^[0-9a-f]{40}$') { throw 'Could not resolve the definitions commit.' }

$Listing = Invoke-RestMethod "$ApiUrl/contents/Definitions?ref=$Commit" -Headers $Headers
$Files = @($Listing | Where-Object name -Like '*.json' | Sort-Object name)
if (!$Files) { throw 'No definitions found.' }

$Downloads = [ordered]@{}
foreach ($File in $Files) {
    if ($File.name -notmatch '^[A-Za-z]+\.json$') { throw "Unexpected filename: $($File.name)" }
    $Content = (Invoke-WebRequest "https://raw.githubusercontent.com/$Repository/$Commit/Definitions/$($File.name)").Content
    $Definition = $Content | ConvertFrom-Json
    if ($null -eq $Definition.actions -or $null -eq $Definition.statuseffects) {
        throw "Missing actions or statuses in $($File.name)"
    }
    $Downloads[$File.name] = $Content
}

$Manifest = [ordered]@{ Repository = $Repository; SourceCommit = $Commit; Files = @($Files.name) }
$Downloads['manifest.json'] = ($Manifest | ConvertTo-Json -Depth 5) + "`n"
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
foreach ($File in Get-ChildItem -LiteralPath $OutputPath -Filter '*.json') {
    if (!$Downloads.Contains($File.Name)) { throw "Upstream removed $($File.Name); review its local copy." }
}

$Changed = 0
foreach ($Name in $Downloads.Keys) {
    $Path = Join-Path $OutputPath $Name
    $Content = $Downloads[$Name].Replace("`r`n", "`n")
    $Previous = if (Test-Path -LiteralPath $Path) { [IO.File]::ReadAllText($Path).Replace("`r`n", "`n") } else { '' }
    if ($Content -ceq $Previous) { continue }
    [IO.File]::WriteAllText($Path, $Content, [Text.UTF8Encoding]::new($false))
    $Changed++
    Write-Output "Updated $Name"
}
Write-Output "$Changed files changed; source commit $Commit. Overrides untouched."
