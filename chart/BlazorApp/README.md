Prometheus and Adapter (production-ready)
--------------------------------------

This chart can optionally deploy a production-grade Prometheus stack and Prometheus Adapter by enabling chart dependencies:

To enable bundled Prometheus (kube-prometheus-stack) and the adapter, set values when installing/ upgrading:

```
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo update

helm upgrade --install my-blazor . \
  --set prometheus.enabled=true \
  --set prometheusAdapter.enabled=true \
  --set metrics.enabled=true
```

Notes:
- The chart declares `kube-prometheus-stack` and `prometheus-adapter` as dependencies. Helm will download and install them when enabled.
- This approach is convenient for dev/test clusters. For production you may prefer to manage the Prometheus stack and adapter separately with their own values and storage backends.
- The included `ServiceMonitor` allows kube-prometheus-stack to discover and scrape the `blazorapp` exporter metrics when `prometheus.enabled=true`.
- The HPA template `hpa-prometheus.yaml` shows how to use the adapter-exposed metric `nginx_requests_per_second`.
