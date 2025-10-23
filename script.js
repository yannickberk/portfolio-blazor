import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '15s', target: 1000 },
    { duration: '1m', target: 500 },
    { duration: '20s', target: 0 },
  ],
};

export default function () {
  const res = http.get('http://localhost:80/');
  check(res, { 'status was 200': (r) => r.status == 200 });
  sleep(1);
}