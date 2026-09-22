// OCPP 1.6 conformance checks. Every frame is validated against the official schemas by the CSMS.
const { loadSimulator, startCsms, sleep } = require('./harness');

let pass = 0, fail = 0;
function check(name, ok, detail) {
  if (ok) { pass++; console.log(`  PASS  ${name}`); }
  else { fail++; console.log(`  FAIL  ${name}${detail ? '  -> ' + detail : ''}`); }
}
const section = (t) => console.log(`\n== ${t}`);
const noViolations = (csms) => check('every frame matched the OCPP 1.6 schemas', csms.violations.length === 0,
  csms.violations.slice(0, 3).join(' | '));

const PORT = 5098;
const URL = `ws://127.0.0.1:${PORT}/OCPP`;
const press = (sb, id) => sb.__handlers[id]();
const count = (csms, action) => csms.received.filter(r => r.action === action).length;
const last = (csms, action) => csms.answers.filter(x => x.action === action).pop();

async function bootRejected() {
  section('S1  a Rejected BootNotification silences the charger and retries');
  const csms = startCsms({ port: PORT, hbInterval: 1, bootStatus: 'Rejected' });
  const sb = loadSimulator(URL);
  press(sb, 'connect');
  await sleep(500);

  check('no StatusNotification before Accepted', count(csms, 'StatusNotification') === 0,
        `${count(csms, 'StatusNotification')} sent`);
  check('no heartbeat timer', sb.hbInterval === null);

  press(sb, 'start1');
  await sleep(300);
  check('StartTransaction is suppressed', count(csms, 'StartTransaction') === 0);
  check('suppression was logged', sb.__log.some(l => l.includes('not Accepted')));

  await sleep(1400);
  check('BootNotification was retried after the interval', count(csms, 'BootNotification') >= 2,
        `${count(csms, 'BootNotification')} boots`);

  sb.wsConnect();
  await sleep(300);
  noViolations(csms);
  await csms.close();
}

async function bootPending() {
  section('S2  a Pending BootNotification holds back traffic but still answers');
  const csms = startCsms({ port: PORT, hbInterval: 1, bootStatus: 'Pending' });
  const sb = loadSimulator(URL);
  press(sb, 'connect');
  await sleep(600);

  check('no StatusNotification', count(csms, 'StatusNotification') === 0);
  check('no heartbeat timer', sb.hbInterval === null);

  csms.client().call('GetConfiguration', {});
  await sleep(400);
  check('GetConfiguration is answered while Pending', !!last(csms, 'GetConfiguration'));

  sb.wsConnect();
  await sleep(300);
  noViolations(csms);
  await csms.close();
}

async function remoteStop() {
  section('S3  RemoteStopTransaction rejects an unknown transaction');
  const csms = startCsms({ port: PORT, hbInterval: 60 });
  const sb = loadSimulator(URL);
  press(sb, 'connect');
  await sleep(400);
  press(sb, 'start1');
  await sleep(500);

  csms.client().call('RemoteStopTransaction', { transactionId: 999999 });
  await sleep(400);
  let a = last(csms, 'RemoteStopTransaction');
  check('unknown transaction is Rejected', a && a.payload.status === 'Rejected', JSON.stringify(a));
  check('connector 1 is still charging', !!sb.meterValInterval[1]);

  csms.client().call('RemoteStopTransaction', { transactionId: 1001 });
  await sleep(500);
  a = last(csms, 'RemoteStopTransaction');
  check('the real transaction is Accepted', a && a.payload.status === 'Accepted', JSON.stringify(a));
  check('connector 1 stopped', !sb.meterValInterval[1]);
  const stop = csms.received.filter(r => r.action === 'StopTransaction').pop();
  check('StopTransaction reason is Remote', stop && stop.payload.reason === 'Remote',
        stop ? stop.payload.reason : 'none');

  sb.wsConnect();
  await sleep(300);
  noViolations(csms);
  await csms.close();
}

async function remoteStart() {
  section('S4  RemoteStartTransaction validates the connector');
  const csms = startCsms({ port: PORT, hbInterval: 60 });
  const sb = loadSimulator(URL);
  press(sb, 'connect');
  await sleep(400);

  csms.client().call('RemoteStartTransaction', { connectorId: 3, idTag: 'TAG-1' });
  await sleep(400);
  let a = last(csms, 'RemoteStartTransaction');
  check('connector 3 is Rejected', a && a.payload.status === 'Rejected', JSON.stringify(a));
  check('no StartTransaction was sent', count(csms, 'StartTransaction') === 0);

  csms.client().call('RemoteStartTransaction', { connectorId: 0, idTag: 'TAG-1' });
  await sleep(400);
  a = last(csms, 'RemoteStartTransaction');
  check('connector 0 is Rejected', a && a.payload.status === 'Rejected');

  csms.client().call('RemoteStartTransaction', { idTag: 'TAG-1' });
  await sleep(600);
  a = last(csms, 'RemoteStartTransaction');
  check('an omitted connector picks a free one', a && a.payload.status === 'Accepted');
  check('that connector is charging', !!sb.meterValInterval[1]);

  sb.wsConnect();
  await sleep(300);
  noViolations(csms);
  await csms.close();
}

async function unlockConnector() {
  section('S5  UnlockConnector acts on the connector it was given');
  const csms = startCsms({ port: PORT, hbInterval: 60 });
  const sb = loadSimulator(URL);
  press(sb, 'connect');
  await sleep(400);
  press(sb, 'start1');
  press(sb, 'start2');
  await sleep(700);
  check('both connectors charging', !!sb.meterValInterval[1] && !!sb.meterValInterval[2]);

  csms.client().call('UnlockConnector', { connectorId: 7 });
  await sleep(400);
  let a = last(csms, 'UnlockConnector');
  check('an unknown connector gives UnlockFailed', a && a.payload.status === 'UnlockFailed', JSON.stringify(a));
  check('nothing was stopped', !!sb.meterValInterval[1] && !!sb.meterValInterval[2]);

  csms.client().call('UnlockConnector', { connectorId: 2 });
  await sleep(500);
  a = last(csms, 'UnlockConnector');
  check('connector 2 is Unlocked', a && a.payload.status === 'Unlocked');
  check('connector 2 stopped', !sb.meterValInterval[2]);
  check('connector 1 is untouched', !!sb.meterValInterval[1]);
  const stop = csms.received.filter(r => r.action === 'StopTransaction').pop();
  check('stop reason is UnlockCommand', stop && stop.payload.reason === 'UnlockCommand',
        stop ? stop.payload.reason : 'none');

  sb.wsConnect();
  await sleep(300);
  noViolations(csms);
  await csms.close();
}

async function configuration() {
  section('S6  GetConfiguration and ChangeConfiguration follow the spec');
  const csms = startCsms({ port: PORT, hbInterval: 60 });
  const sb = loadSimulator(URL);
  press(sb, 'connect');
  await sleep(400);

  csms.client().call('GetConfiguration', { key: ['HeartbeatInterval', 'NoSuchKey'] });
  await sleep(400);
  let a = last(csms, 'GetConfiguration');
  check('only the asked-for key comes back', a && a.payload.configurationKey.length === 1,
        JSON.stringify(a && a.payload));
  check('the unknown key is reported', a && a.payload.unknownKey && a.payload.unknownKey[0] === 'NoSuchKey');

  csms.client().call('ChangeConfiguration', { key: 'NumberOfConnectors', value: '4' });
  await sleep(400);
  a = last(csms, 'ChangeConfiguration');
  check('a readonly key is Rejected', a && a.payload.status === 'Rejected', JSON.stringify(a));

  csms.client().call('ChangeConfiguration', { key: 'NoSuchKey', value: '1' });
  await sleep(400);
  a = last(csms, 'ChangeConfiguration');
  check('an unknown key is NotSupported', a && a.payload.status === 'NotSupported');

  const before = count(csms, 'Heartbeat');
  csms.client().call('ChangeConfiguration', { key: 'HeartbeatInterval', value: '1' });
  await sleep(2600);
  a = last(csms, 'ChangeConfiguration');
  check('HeartbeatInterval is Accepted', a && a.payload.status === 'Accepted');
  check('and actually applied', count(csms, 'Heartbeat') - before >= 2,
        `${count(csms, 'Heartbeat') - before} heartbeats in 2.6 s`);

  sb.wsConnect();
  await sleep(300);
  noViolations(csms);
  await csms.close();
}

async function errorsAndAvailability() {
  section('S7  CALLERROR shape, SetChargingProfile, ChangeAvailability');
  const csms = startCsms({ port: PORT, hbInterval: 60 });
  const sb = loadSimulator(URL);
  press(sb, 'connect');
  await sleep(400);

  csms.client().call('TriggerMessage', { requestedMessage: 'Heartbeat' });
  await sleep(400);
  const err = last(csms, 'TriggerMessage');
  check('an unknown action gets a 5-element CALLERROR', err && err.error === 'NotImplemented',
        JSON.stringify(err));

  csms.client().call('SetChargingProfile', { connectorId: 1 });
  await sleep(300);
  let a = last(csms, 'SetChargingProfile');
  check('SetChargingProfile without a profile is Rejected', a && a.payload.status === 'Rejected');

  csms.client().call('ChangeAvailability', { connectorId: 1, type: 'Inoperative' });
  await sleep(300);
  a = last(csms, 'ChangeAvailability');
  check('ChangeAvailability while idle is Accepted', a && a.payload.status === 'Accepted');

  press(sb, 'start1');
  await sleep(500);
  csms.client().call('ChangeAvailability', { connectorId: 1, type: 'Inoperative' });
  await sleep(300);
  a = last(csms, 'ChangeAvailability');
  check('ChangeAvailability while charging is Scheduled', a && a.payload.status === 'Scheduled');

  sb.wsConnect();
  await sleep(300);
  noViolations(csms);
  await csms.close();
}

async function payloads() {
  section('S8  MeterValues and StopTransaction carry honest numbers');
  const csms = startCsms({ port: PORT, hbInterval: 60 });
  const sb = loadSimulator(URL);
  sb.meterValuesInterval = 300;
  press(sb, 'connect');
  await sleep(400);

  press(sb, 'mv1');
  await sleep(300);
  let mv = csms.received.filter(r => r.action === 'MeterValues').pop();
  check('no transactionId outside a transaction', mv && mv.payload.transactionId === undefined,
        JSON.stringify(mv && mv.payload));
  const sampled = mv.payload.meterValue[0].sampledValue;
  check('energy is reported in Wh', sampled.some(v => v.measurand === 'Energy.Active.Import.Register' && v.unit === 'Wh'));
  check('power is reported in W', sampled.some(v => v.measurand === 'Power.Active.Import' && v.unit === 'W'));
  check('SoC is reported in Percent', sampled.some(v => v.measurand === 'SoC' && v.unit === 'Percent'));

  press(sb, 'start1');
  await sleep(1200);
  mv = csms.received.filter(r => r.action === 'MeterValues').pop();
  check('transactionId is present while charging', mv && mv.payload.transactionId === 1001,
        String(mv && mv.payload.transactionId));

  press(sb, 'stop1');
  await sleep(400);
  const stop = csms.received.filter(r => r.action === 'StopTransaction').pop();
  check('meterStop is the measured total, not a fabricated 2000', stop && stop.payload.meterStop !== 2000,
        `meterStop=${stop && stop.payload.meterStop}`);
  check('stop reason defaults to Local', stop && stop.payload.reason === 'Local');

  sb.wsConnect();
  await sleep(300);
  noViolations(csms);
  await csms.close();
}

async function uniqueIds() {
  section('S9  every CALL uses a fresh uniqueId');
  const csms = startCsms({ port: PORT, hbInterval: 1 });
  const sb = loadSimulator(URL);
  press(sb, 'connect');
  await sleep(400);
  press(sb, 'start1');
  await sleep(400);
  press(sb, 'heartbeat');
  press(sb, 'status1');
  press(sb, 'data_transfer');
  await sleep(2500);

  const ids = csms.received.filter(r => r.uid).map(r => r.uid);
  check('more than one CALL was sent', ids.length >= 6, `${ids.length} calls`);
  check('all uniqueIds differ', new Set(ids).size === ids.length,
        `${ids.length} calls, ${new Set(ids).size} distinct`);

  sb.wsConnect();
  await sleep(300);
  noViolations(csms);
  await csms.close();
}

(async () => {
  await bootRejected();
  await bootPending();
  await remoteStop();
  await remoteStart();
  await unlockConnector();
  await configuration();
  await errorsAndAvailability();
  await payloads();
  await uniqueIds();
  console.log(`\n${pass} passed, ${fail} failed`);
  process.exit(fail ? 1 : 0);
})().catch((e) => { console.error('HARNESS ERROR', e); process.exit(2); });
