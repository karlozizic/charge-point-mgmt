# OCPP 1.6 reference

`schemas/` holds the official OCPP 1.6 JSON Schema files (draft-04) for the 16 messages this system
speaks — the eight the charger sends and the eight the CSMS may send back. They are the machine-readable
definition of every payload, so they are the thing to check against when a message shape is in doubt.

The simulator test suite validates every frame against them; see `simulator/test/`.

## Where they came from

The Open Charge Alliance publishes OCPP 1.6 at <https://openchargealliance.org/protocols/open-charge-point-protocol/>
(free registration). These files are the same schemas, taken from the `mobilityhouse/ocpp` distribution.

The full specification text is not in this repo. Download it from the OCA if you need the prose: the
schemas give field names, types and enums, but not the state machine rules — for example that a charge
point must stay silent until BootNotification is Accepted (OCPP 1.6 section 4.2.1).

## Licence

OCPP 1.6 and its schema files are © Open Charge Alliance, released under the
[Creative Commons Attribution-NoDerivatives 4.0 International licence](https://creativecommons.org/licenses/by-nd/4.0/).
They are redistributed here unchanged, as that licence allows. Do not edit them; if one looks wrong,
check it against the OCA download instead.
