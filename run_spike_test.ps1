<#
  run_spike_test.ps1

  Usage:
    .\run_spike_test.ps1 -JwtSecret 'your‑k6‑secret' -Email you@domain
#>

param(
  [Parameter(Mandatory=$true)] [string] $Email,
  [Parameter(Mandatory=$true)] [string] $Password
)

# 1) Change into the directory that holds k6 script
#    (adjust path if you move load‑tests folder)
Push-Location $PSScriptRoot\server\Startup.Tests\load-tests

# 2) Invoke k6 directly, injecting env‑vars with -e
#    Make sure k6 is on your PATH (or point to full C:\Program Files\k6\k6.exe)
k6 run `
  -e LOGIN_EMAIL=$Email `
  -e LOGIN_PASSWORD=$Password `
  spike_test.js

Pop-Location
