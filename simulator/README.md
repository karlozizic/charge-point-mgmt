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

The Local target is `ws://127.0.0.1:5000/OCPP` — the port `CPMS.Proxy` pins in its
`launchSettings.json`.

## What it does

- Sends BootNotification and a StatusNotification per connector on connect, then heartbeats at the
  interval from the Boot response.
- Start/Stop per connector drive StartTransaction/StopTransaction; while charging it sends
  MeterValues every 15 s (`Energy.Active.Import.Register` in Wh, `Power.Active.Import`, `SoC`) from a
  small battery model.
- Answers server-initiated calls: Reset, RemoteStartTransaction, RemoteStopTransaction,
  UnlockConnector, ChangeAvailability, ChangeConfiguration, GetConfiguration, SetChargingProfile.

One charger per browser tab; open more tabs for more chargers.
