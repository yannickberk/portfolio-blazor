prometheus-adapter wrapper
==========================

Install the adapter using the provided example values:

```
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo update
helm dependency update chart/prometheus-adapter
helm upgrade --install prometheus-adapter chart/prometheus-adapter --namespace monitoring --values values.yaml
```
