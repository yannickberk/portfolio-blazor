prometheus-stack wrapper
========================

This small wrapper helps install the upstream `kube-prometheus-stack` using a single chart directory.

Usage:

```
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo update
helm dependency update chart/prometheus-stack
helm upgrade --install prometheus chart/prometheus-stack --namespace monitoring --create-namespace --values values.yaml
```
