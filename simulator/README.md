# Charger simulator

`OCPP_1.6._Charger_SImulator.html` is a single-charger OCPP 1.6-J simulator that runs in the browser.
Open the file directly — there is no build step. It loads jQuery from a CDN, so it needs network
access.

## Use

1. Start the stack: `docker compose up -d`, then `CPMS.API` and `CPMS.Proxy`.
2. Create the charge point in the API first (Swagger or the operator console). BootNotification is
   rejected for an unknown charger id, and the simulator ignores the rejection, so make sure the id
   exists before you connect.
3. Open the HTML file, set the charger id, press **Connect**.
4. Press **Authorize** before **Start**. The gateway keeps an authorization cache per charger, and
   answers StartTransaction with `Invalid` for a tag it has not seen an Authorize for.

The Local target is `ws://127.0.0.1:5000/OCPP` — the port `CPMS.Proxy` pins in its
`launchSettings.json`.

## What it does

- Sends BootNotification and a StatusNotification per connector on connect, then heartbeats at the
  interval from the Boot response.
- Start/Stop per connector drive StartTransaction/StopTransaction. A connector only enters the
  charging state after the CSMS answers `Accepted`; a rejected tag rolls the connector back. While
  charging it sends MeterValues every 15 s (`Energy.Active.Import.Register` in Wh,
  `Power.Active.Import`, `SoC`) from a small battery model.
- Answers server-initiated calls: Reset, RemoteStartTransaction, RemoteStopTransaction,
  UnlockConnector, ChangeAvailability, ChangeConfiguration, GetConfiguration, SetChargingProfile.

## Reconnect

An unexpected close reconnects with exponential backoff, 1 s to 30 s, and gives up after 10 attempts;
press **Connect** to start over. A close with code 1000 does not reconnect at all, because that is how
the gateway drops the older socket when the same charger id connects twice — two tabs on one id would
otherwise evict each other forever.

**Use a different charger id per tab.** One charger per tab; open more tabs for more chargers.

Timers stop when the socket closes. A transaction that was running is not resumed after a reconnect —
stop it and start a new one.
