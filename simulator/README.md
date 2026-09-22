# Charger simulator

`OCPP_1.6._Charger_SImulator.html` is a single-charger OCPP 1.6-J simulator that runs in the browser.
Open the file directly — there is no build step. It loads jQuery from a CDN, so it needs network
access.

## Use

1. Start the stack: `docker compose up -d`, then `CPMS.API` and `CPMS.Proxy`.
2. Create the charge point and the tag. The easiest way is to let the load driver do it:
   `dotnet run --project CPMS.Simulator -- --chargers 1 --duration 1 --seed`. That always creates
   `CP-MANUAL-1` and `TAG-MANUAL`, reserved for this page, which the defaults below already use.
   BootNotification is rejected for a charger id the API does not know.
3. Open the HTML file and press **Connect**.
4. Press **Authorize** before **Start**. The gateway keeps an authorization cache per charger, and
   answers StartTransaction with `Invalid` for a tag it has not seen an Authorize for.

| Field | Default | Note |
|-------|---------|------|
| Charger ID | `CP-MANUAL-1` | reserved for this page, so it never fights a fleet member for the id |
| Tag | `TAG-MANUAL` | never expires |
| Password | *(disabled)* | the gateway has no authentication; the field did nothing |

Do not point this page at a `CP-SIM-####` id while the load driver is running. The gateway closes the
older socket when one id connects twice, so the two would evict each other.

The Local target is `ws://127.0.0.1:5000/OCPP` — the port `CPMS.Proxy` pins in its
`launchSettings.json`.

## What it does

- Sends BootNotification on connect. Until the answer is `Accepted` it sends nothing but replies to the
  CSMS, and retries the boot after the `interval` it was given (OCPP 1.6 section 4.2.1).
- Once accepted: a StatusNotification per connector, then heartbeats at the interval from the boot
  response.
- Start/Stop per connector drive StartTransaction/StopTransaction. A connector enters the charging state
  only when the CSMS answers `Accepted`; a rejected tag rolls it back. While charging it sends
  MeterValues every 15 s (`Energy.Active.Import.Register` Wh, `Power.Active.Import` W, `SoC` Percent)
  from a small battery model.
- Answers server-initiated calls, and answers them honestly: Reset, RemoteStartTransaction,
  RemoteStopTransaction, UnlockConnector, ChangeAvailability, ChangeConfiguration, GetConfiguration,
  SetChargingProfile. An unknown transaction id or connector is `Rejected`, not `Accepted`.
  ChangeConfiguration on `HeartbeatInterval` or `MeterValueSampleInterval` really does change them.
- Anything else gets a CALLERROR of `NotImplemented`.

Every payload is checked against the OCPP 1.6 JSON schemas in `../docs/ocpp/schemas` by the test suite
in `test/`.

## Reconnect

An unexpected close reconnects with exponential backoff, 1 s to 30 s, and gives up after 10 attempts;
press **Connect** to start over. A close with code 1000 does not reconnect at all, because that is how
the gateway drops the older socket when the same charger id connects twice — two tabs on one id would
otherwise evict each other forever.

**Use a different charger id per tab.** One charger per tab; open more tabs for more chargers.

Timers stop when the socket closes. A transaction that was running is not resumed after a reconnect —
stop it and start a new one.
