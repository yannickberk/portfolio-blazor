Installing Prometheus (production guidance)
=========================================

This chart intentionally does not bundle the full Prometheus stack. Installing the monitoring stack separately is recommended for production so you can configure storage, retention, resource limits and upgrades independently.

Recommended steps
-----------------

1) Add the Prometheus Helm repo and update:
   ```bash
   helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
   helm repo update
   ```

2) Install the Prometheus stack using the provided wrapper chart (recommended):
   ```bash
   # Add repo and update
   helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
   helm repo update

   # Update chart dependencies for the wrapper and install into 'monitoring'
   helm dependency update chart/prometheus-stack
   helm upgrade --install prometheus chart/prometheus-stack \
     --namespace monitoring --create-namespace \
     --values k8s/values.prometheus.yaml
   ```

3) Install the Prometheus Adapter using the wrapper chart and the provided adapter rules:
   ```bash
   helm dependency update chart/prometheus-adapter
   helm upgrade --install prometheus-adapter chart/prometheus-adapter \
     --namespace monitoring \
     --values k8s/values.adapter.yaml
   ```

4) Deploy your application chart (it will create a ServiceMonitor for Prometheus to discover):
   ```bash
   helm upgrade --install my-blazor chart/BlazorApp --set metrics.enabled=true
   ```

Files provided in this repo:
- `k8s/values.prometheus.yaml` — example values to set persistent storage and retention.
- `k8s/values.adapter.yaml` — example adapter config mapping nginx requests to a metric.

After installation
------------------
- Verify Prometheus UI (port-forward `svc/prometheus-kube-prometheus-prometheus 9090:9090 -n monitoring`) and check for your ServiceMonitor target.
- Ensure the adapter exposes the custom metric (`kubectl get --raw "/apis/custom.metrics.k8s.io/v1beta1"` should not error).
