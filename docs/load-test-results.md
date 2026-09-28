# Load test results

Single API instance, single gateway, Postgres 16 in Docker, all on one machine.

```
dotnet run --project CPMS.Simulator -c Release -- --chargers 100 --arrival 20 --session 300 --meter-interval 2 --idle 10 --duration 420 --id-prefix CP-A100- --seed
```

100 chargers, 7 minutes, a meter reading every 2 s (so session documents grow as fast as in a multi-hour session).

## Read-side changes

| | Before | After |
|---|---|---|
| Messages | 21096 | 21191 |
| MeterValues failed | 148 | 0 |
| MeterValues p50 / p95 / p99 | 36 / 97 / 142 ms | 34 / 92 / 119 ms |
| StopTransaction p50 / p95 / p99 | 314 / 1036 / 1079 ms | 258 / 1337 / 1441 ms |
| Stats endpoint, idle | 135 ms (2711 sessions) | 53-91 ms (3662 sessions) |
| Stats endpoint, during load | up to 346 ms | not measured |
| Session document | 1.1-1.4 kB, growing | about 435 bytes |

- Meter readings are separate documents. The session document no longer grows.
- Stats use count, sum and average queries instead of loading every session.
- Other actions (StartTransaction, Authorize, BootNotification) were also faster in the second run. The first run started on a cold API and a fresh seed, so that gap is not all from the changes.
