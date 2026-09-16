# Simulator tests

Runs the `<script>` block of `../OCPP_1.6._Charger_SImulator.html` **unmodified** in Node, with small
DOM and jQuery stubs, over a real WebSocket. So the code under test is the code that ships.

```bash
npm install
npm test          # tests.js  - reconnect, timers, transaction state
                  # spec.js   - OCPP 1.6 conformance
npm run live      # against a running CPMS.API + CPMS.Proxy
npm run twotab    # two tabs on one charger id against the real gateway
```

`npm test` needs nothing but Node. The fake CSMS validates every frame the simulator sends against the
schemas in `docs/ocpp/schemas`, so a payload that drifts from OCPP 1.6 fails the run.

`npm run live` needs the stack up and a charge point plus two tags seeded:

| Seed | Value |
|------|-------|
| Charge point | `CP-SMOKE-1` (or set `CP_ID`) |
| Tag | `TAG-OK`, not blocked |
| Tag | `TAG-BLOCKED`, blocked |
