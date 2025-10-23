<#
.SYNOPSIS
  Build the Blazor app container with Podman, push to a local registry, and load into Minikube.

.DESCRIPTION
  This script builds the container image from `src/BlazorApp` (by default), tags it
  as `localhost:5000/blazorapp:<version>` (default `latest`), pushes it to the local
  registry at `localhost:5000` and then runs `minikube image load` so Minikube can use it.

.NOTES
  - Requires: podman, minikube in PATH, and a registry listening on localhost:5000.
  - Run in PowerShell (pwsh.exe) on Windows or PowerShell Core on Linux/macOS.

.EXAMPLE
  # Build latest, push and load
  .\build-and-load-podman.ps1

.EXAMPLE
  # Build a specific tag and skip minikube load
  .\build-and-load-podman.ps1 -Version 1.0.0 -NoLoad
#>

param(
    [string]$Version = 'latest',
    [string]$ProjectPath = 'src/BlazorApp',
    [string]$ImageName = 'localhost:5000/blazorapp',
    [switch]$NoLoad
)

$ErrorActionPreference = 'Stop'


function Test-CommandExists {
  param([string]$cmd)
  if (-not (Get-Command $cmd -ErrorAction SilentlyContinue)) {
    Write-Error "Required command '$cmd' was not found in PATH. Please install it and try again."
    exit 2
  }
}

Test-CommandExists podman
Test-CommandExists minikube


$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
# Build the project path relative to the script directory in two steps to avoid parser
# issues when the path contains a drive letter (e.g. C:\...). Then resolve to an absolute path.
$parentDir = Join-Path -Path $scriptDir -ChildPath '..'
$combinedPath = Join-Path -Path $parentDir -ChildPath $ProjectPath
$projectFullPath = Resolve-Path -Path $combinedPath -ErrorAction Stop

$image = "${ImageName}:${Version}"

Write-Host "Building image: $image" -ForegroundColor Cyan
Write-Host "Project path: $projectFullPath" -ForegroundColor DarkCyan

try {
    # Build with podman. Use the project directory as the build context so the Dockerfile in it is used.
    Write-Host "Running: podman build -t $image $projectFullPath" -ForegroundColor Yellow
    podman build -t $image $projectFullPath

    Write-Host "Pushing image to registry: $ImageName" -ForegroundColor Yellow
    podman push $image --tls-verify=false

    if (-not $NoLoad) {
        Write-Host "Loading image into minikube: minikube image load $image" -ForegroundColor Yellow
        minikube image load $image
    }

    Write-Host "Done. Image available as: $image" -ForegroundColor Green
}
catch {
    Write-Error "An error occurred: $_"
    exit 1
}
