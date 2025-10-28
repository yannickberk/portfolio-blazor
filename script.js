import http from 'k6/http';
import { check, sleep } from 'k6';

// Allow overriding the target URL via environment variable so tests can
// target a minikube service URL, a kubectl port-forward, or an in-cluster
// service DNS name. Default remains http://localhost:80/ for backward
// compatibility.
const TARGET_URL = typeof __ENV.TARGET_URL !== 'undefined' ? __ENV.TARGET_URL : 'http://localhost:80/';

export const options = {
  stages: [
    { duration: '2m', target: 400 },
    { duration: '2m', target: 1000 },
    { duration: '1m', target: 0 },
  ],
};

export default function () {
  const res = http.get(TARGET_URL);
  check(res, { 'status was 200': (r) => r.status == 200 });
  sleep(1);
}