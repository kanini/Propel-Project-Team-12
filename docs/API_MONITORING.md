# API Monitoring (AC3 - US_057)

## Overview

All API requests are monitored for response times, error rates, and request volumes using Prometheus.NET. Metrics are available via `/health/metrics` endpoint (JSON format) and `/metrics` endpoint (Prometheus format).

## Metrics Endpoints

### GET /health/metrics (JSON Format)

Returns metrics summary with <1s query time (AC3).

**Response Example**:

```json
{
  "timestamp": "2026-03-27T00:00:00Z",
  "responseTimes": {
    "p50": 0.025,
    "p95": 0.15,
    "p99": 0.5,
    "mean": 0.05
  },
  "requestVolume": {
    "total": 10500,
    "perMinute": 2100
  },
  "errorRate": {
    "percentage": 0.08,
    "totalErrors": 84,
    "totalRequests": 10500,
    "targetPercentage": 0.1
  },
  "statusCodeDistribution": {
    "200": 9800,
    "404": 50,
    "500": 34,
    "429": 616
  },
  "topEndpoints": [
    {
      "method": "GET",
      "endpoint": "/api/patients/{id}",
      "requestCount": 3500
    },
    {
      "method": "GET",
      "endpoint": "/api/appointments",
      "requestCount": 2100
    }
  ]
}
```

**Response Headers**:

```
X-Query-Time-Ms: 125
```

### GET /metrics (Prometheus Format)

Returns Prometheus metrics in text format for integration with Grafana, Application Insights, etc. (AC3).

**Response Example**:

```
# HELP http_request_duration_seconds HTTP request duration in seconds
# TYPE http_request_duration_seconds histogram
http_request_duration_seconds_bucket{method="GET",endpoint="/api/patients/{id}",status_code="200",le="0.01"} 250
http_request_duration_seconds_bucket{method="GET",endpoint="/api/patients/{id}",status_code="200",le="0.05"} 2800
http_request_duration_seconds_bucket{method="GET",endpoint="/api/patients/{id}",status_code="200",le="0.1"} 3400
http_request_duration_seconds_sum{method="GET",endpoint="/api/patients/{id}",status_code="200"} 175.5
http_request_duration_seconds_count{method="GET",endpoint="/api/patients/{id}",status_code="200"} 3500

# HELP http_requests_total Total HTTP requests
# TYPE http_requests_total counter
http_requests_total{method="GET",endpoint="/api/patients/{id}",status_code="200"} 3500
http_requests_total{method="GET",endpoint="/api/appointments",status_code="200"} 2100

# HELP http_errors_total Total HTTP errors (4xx + 5xx)
# TYPE http_errors_total counter
http_errors_total{method="GET",endpoint="/api/patients/{id}",status_code="404"} 50
http_errors_total{method="POST",endpoint="/api/appointments",status_code="500"} 34
```

## Monitored Metrics (AC3)

### Response Times

- **P50 (Median)**: 50th percentile response time
- **P95**: 95th percentile response time (most requests complete faster)
- **P99**: 99th percentile response time (tail latency)
- **Mean**: Average response time

### Request Volume

- **Total**: Total requests processed
- **Per Minute**: Requests per minute (5-minute sliding window)

### Error Rate

- **Percentage**: (4xx + 5xx errors) / total requests * 100
- **Target**: <0.1% (NFR-011)
- **Total Errors**: Count of 4xx + 5xx responses
- **Total Requests**: Count of all requests

### Status Code Distribution

- Count of requests per HTTP status code (200, 404, 500, 429, etc.)

### Top Endpoints

- Top 10 most-requested endpoints by volume

## Performance Requirements

- **Query Time**: Metrics endpoint MUST return within 1 second (AC3)
- **Storage**: In-memory Prometheus metrics (fast queries)
- **Window**: 5-minute sliding window for real-time metrics

## Grafana Integration

### Configure Prometheus Data Source

```yaml
datasources:
  - name: PatientAccessAPI
    type: prometheus
    url: https://api.patient-access.com/metrics
    access: proxy
```

### Sample Grafana Queries

```promql
# Request rate per endpoint
rate(http_requests_total[5m])

# Error rate percentage
(sum(rate(http_errors_total[5m])) / sum(rate(http_requests_total[5m]))) * 100

# P95 response time
histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))

# Requests per status code
sum by (status_code) (rate(http_requests_total[5m]))
```

## Monitoring Alerts

### Error Rate Alert (NFR-011)

- **Condition**: Error rate > 0.1% for 5 minutes
- **Action**: Trigger Application Insights alert, notify ops team

### High Response Time Alert

- **Condition**: P95 response time > 1 second for 5 minutes
- **Action**: Trigger Application Insights alert

### High Request Volume Alert

- **Condition**: Request rate > 10,000 requests/minute
- **Action**: Trigger capacity planning review

## Testing Metrics Collection

### Generate Test Traffic

```bash
# Generate 100 requests
for i in {1..100}; do
  curl -X GET https://api.patient-access.com/api/patients/1 \
    -H "Authorization: Bearer $JWT_TOKEN"
done
```

### Verify Metrics

```bash
# Check JSON metrics
curl https://api.patient-access.com/health/metrics

# Check Prometheus metrics
curl https://api.patient-access.com/metrics
```

## Metrics Retention

- **In-Memory**: Metrics are stored in-memory using Prometheus.NET
- **Persistence**: For long-term storage, integrate with Prometheus server or Application Insights
- **Reset**: Metrics reset on application restart

## Security Considerations

1. **Public Access**: `/health/metrics` and `/metrics` are publicly accessible for monitoring tools
2. **No Sensitive Data**: Metrics do not expose PII or credentials
3. **Rate Limiting**: Metrics endpoints are not rate-limited to support monitoring tools
4. **Authentication**: For production, consider requiring authentication for metrics endpoints

## Troubleshooting

### Metrics Query Takes >1 Second (AC3 Violation)

**Symptom**: `X-Query-Time-Ms` header shows >1000ms

**Possible Causes**:
- High request volume (millions of requests)
- Memory pressure on server
- Complex aggregation logic

**Solutions**:
- Reduce metrics retention window
- Optimize MetricsService aggregation logic
- Use sampling for high-volume endpoints

### Metrics Not Updating

**Symptom**: `/health/metrics` returns zero or stale data

**Possible Causes**:
- MetricsMiddleware not registered
- Middleware order incorrect
- Metrics reset on restart

**Solutions**:
- Verify `app.UseApiMetrics()` in Program.cs
- Check middleware order (metrics before controllers)
- Confirm application hasn't restarted

### Prometheus Format Errors

**Symptom**: `/metrics` endpoint returns 500 or malformed data

**Possible Causes**:
- Prometheus.NET package version mismatch
- Label values contain invalid characters
- Metric naming conflicts

**Solutions**:
- Update Prometheus.NET to latest version
- Sanitize endpoint paths (remove special characters)
- Check Prometheus logs for metric validation errors

## Metric Naming Conventions

All metrics follow Prometheus naming conventions:

- **Lowercase**: `http_request_duration_seconds`
- **Underscores**: Words separated by underscores
- **Units**: Append units to metric names (`_seconds`, `_bytes`, `_total`)
- **Labels**: Use labels for dimensions (method, endpoint, status_code)

## Related Documentation

- [RATE_LIMITING.md](./RATE_LIMITING.md) - Rate limiting and security alerts
- [AUTHENTICATION.md](./AUTHENTICATION.md) - Authentication and authorization
- [DEPLOYMENT_CHECKLIST.md](./DEPLOYMENT_CHECKLIST.md) - Production deployment

## OWASP Compliance

This implementation aligns with:

- **OWASP API Security Top 10 (2023)**:
  - API9:2023 - Improper Inventory Management
  - API10:2023 - Unsafe Consumption of APIs
- **OWASP ASVS v4.0**:
  - V7.4 - Error Handling and Logging
