# CPMS.Simulator

Headless OCPP 1.6-J charger for load testing. One process, many chargers, one WebSocket each.
The browser simulator in `../simulator/` stays the tool for looking at a single charger by hand.

```bash
dotnet run --project CPMS.Simulator -- --chargers 100 --duration 60 --seed
```

`--seed` first creates the charge points, the tag and a pricing group through the API. Without it
the gateway rejects every BootNotification, refuses StartTransaction for an unknown tag, and billing
fails on every stop — the run would measure silence.

| Option | Default | Meaning |
|--------|---------|---------|
| `--url` | `ws://127.0.0.1:5000/OCPP` | gateway |
| `--api` | `http://localhost:5023` | API, used by `--seed` |
| `--chargers` | 100 | how many |
| `--arrival` | 20 | chargers per second; opening every socket at once measures a connection storm |
| `--session` | 30 | session length in seconds |
| `--meter-interval` | 15 | seconds between MeterValues; the last one comes at the session's end |
| `--idle` | 10 | gap between sessions |
| `--stop-grace` | 5 | time a running transaction gets to close at shutdown |
| `--duration` | 120 | how long to run |
| `--id-prefix` | `CP-SIM-` | charger ids, numbered from 0001 |
| `--tag` | `TAG-SIM` | id tag |
| `--tags` | 1 | spread the fleet over n tags; one tag puts every Authorize on one ChargeTag stream |
| `--power` | 11000 | charging power in W |
| `--capacity` | 50000 | battery size in Wh |

Ctrl+C stops early. Either way a running transaction gets `--stop-grace` to close before the socket
goes, so a stopped run does not leave rows sitting in `Started`. This is the one thing that can
carry a run past `--duration`.

## Layout

| File | Holds |
|------|-------|
| `ChargerClient.cs` | One charger end to end. The protocol flow is written in order, so a charger's state is where the code is; the only remembered state is the open transaction. |
| `BatteryModel.cs` | Energy and state of charge. Takes elapsed time, never reads a clock. |
| `Ocpp/OcppConnection.cs` | Frames, correlation by `uniqueId`, call timeouts. |
| `Ocpp/WebSocketTransport.cs` | The socket. The only piece with no test. |
| `Fleet.cs` | Many chargers, paced arrivals. |
| `Seeder.cs` | Makes the API ready for a run. |
| `Metrics.cs` | Counts per action and latency percentiles. |

```bash
dotnet test CPMS.Simulator.Tests
```
