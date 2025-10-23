<#
Installs Prometheus (kube-prometheus-stack) and Prometheus Adapter using the wrapper charts
and then deploys the BlazorApp chart with metrics enabled.

Usage:
  ./scripts/install-monitoring.ps1

This script expects Helm and kubectl on PATH and a working kubeconfig context.
#>

param(
    [string]$Namespace = 'monitoring',
    [string]$PromValues = 'k8s/values.prometheus.yaml',
    [string]$AdapterValues = 'k8s/values.adapter.yaml',
    [string]$AppChart = 'chart/BlazorApp',
    [string]$PromWrapper = 'chart/prometheus-stack',
    [string]$AdapterWrapper = 'chart/prometheus-adapter',
    [int]$TimeoutSeconds = 600
)

Set-StrictMode -Version Latest

function Exec($cmd) {
    Write-Host "> $cmd"
    $proc = Start-Process -FilePath pwsh -ArgumentList "-NoProfile","-Command",$cmd -NoNewWindow -PassThru -Wait
    if ($proc.ExitCode -ne 0) { throw "Command failed: $cmd" }
}

function Wait-For-PodsReady {
    param(
        [string]$Namespace,
        [int]$TimeoutSec = 300
    )
    $end = (Get-Date).AddSeconds($TimeoutSec)
    while ((Get-Date) -lt $end) {
        $pods = kubectl get pods -n $Namespace -o json | ConvertFrom-Json
        if ($null -eq $pods.items) { Start-Sleep -Seconds 5; continue }
        $allReady = $true
        foreach ($p in $pods.items) {
            $cs = $p.status.containerStatuses
            if ($cs -eq $null) { $allReady = $false; break }
            foreach ($c in $cs) {
                if (-not $c.ready) { $allReady = $false; break }
            }
            if (-not $allReady) { break }
        }
        if ($allReady) { Write-Host "All pods in namespace '$Namespace' are ready."; return $true }
        Write-Host "Waiting for pods in namespace '$Namespace' to become ready..." -NoNewline
        Write-Host " ($(Get-Date))"
        Start-Sleep -Seconds 5
    }
    throw "Timeout waiting for pods in namespace $Namespace to become ready after $TimeoutSec seconds"
}

try {
    Write-Host "Adding Helm repo and updating..."
    Exec "helm repo add prometheus-community https://prometheus-community.github.io/helm-charts || true; helm repo update"

    Write-Host "Preparing wrapper chart dependencies..."
    Exec "helm dependency update $PromWrapper"
    Exec "helm dependency update $AdapterWrapper"

    Write-Host "Installing Prometheus stack into namespace '$Namespace'..."
    Exec "helm upgrade --install prometheus $PromWrapper --namespace $Namespace --create-namespace --values $PromValues"

    Write-Host "Waiting for Prometheus pods to become ready (this can take a few minutes)..."
    Wait-For-PodsReady -Namespace $Namespace -TimeoutSec $TimeoutSeconds

    Write-Host "Installing Prometheus Adapter into namespace '$Namespace'..."
    Exec "helm upgrade --install prometheus-adapter $AdapterWrapper --namespace $Namespace --values $AdapterValues"

    Write-Host "Waiting for Prometheus Adapter pods to become ready..."
    Wait-For-PodsReady -Namespace $Namespace -TimeoutSec $TimeoutSeconds

    Write-Host "Installing BlazorApp chart with metrics enabled..."
    Exec "helm upgrade --install my-blazor $AppChart --set metrics.enabled=true"

    Write-Host "Waiting for BlazorApp pods to become ready in the 'default' namespace..."
    Wait-For-PodsReady -Namespace 'default' -TimeoutSec $TimeoutSeconds

    Write-Host "Installation complete. Quick verifications:"
    Write-Host "  - Port-forward Prometheus UI: kubectl port-forward -n $Namespace svc/prometheus-kube-prometheus-prometheus 9090:9090"
    Write-Host "  - Open http://localhost:9090 and check 'Targets' for the blazorapp ServiceMonitor"
    Write-Host "  - Check custom metrics API: kubectl get --raw \"/apis/custom.metrics.k8s.io/v1beta1\" | jq ."
}
catch {
    Write-Error "Error: $_"
    exit 1
}
