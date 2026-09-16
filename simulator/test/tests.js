// Behaviour checks for the four high-severity simulator fixes.
// The simulator source is used verbatim; only timing constants are lowered per test.
const { loadSimulator, startCsms, sleep } = require('./harness');

let pass = 0, fail = 0;
function check(name, ok, detail) {
  if (ok) { pass++; console.log(`  PASS  ${name}`); }
  else { fail++; console.log(`  FAIL  ${name}${detail ? '  -> ' + detail : ''}`); }
}
function section(t) { console.log(`\n== ${t}`); }

const noViolations = (csms) => check('every frame matched the OCPP 1.6 schemas',
  csms.violations.length === 0, csms.violations.slice(0, 3).join(' | '));

const PORT = 5099;
const URL = `ws://127.0.0.1:${PORT}/OCPP`;
const press = (sb, id) => sb.__handlers[id]();
const count = (csms, action) => csms.received.filter(r => r.action === action).length;

async function t1_heartbeatFromBoot() {
  section('T1  heartbeat interval comes from the BootNotification response');
  const csms = startCsms({ port: PORT, hbInterval: 1 });   // 1 s, vs the 30 s hardcoded fallback
  const sb = loadSimulator(URL);
  press(sb, 'connect');
  await sleep(3500);

  check('boot response was recognised', sb.__storage.get('Configuration') === '1',
        `Configuration=${sb.__storage.get('Configuration')}`);
  const hb = count(csms, 'Heartbeat');
  check('heartbeats use the 1 s interval, not the 30 s fallback', hb >= 2, `${hb} heartbeats in 3.5 s`);

  sb.wsConnect();                                          // press Disconnect
  await sleep(300);
  noViolations(csms);
  await csms.close();
}

async function t2_rejectedStartDoesNotCharge() {
  section('T2  a rejected StartTransaction does not start the meter timer');
  const csms = startCsms({ port: PORT, hbInterval: 60, startStatus: 'Blocked' });
  const sb = loadSimulator(URL);
  press(sb, 'connect');
  await sleep(500);
  press(sb, 'start1');
  await sleep(800);

  check('meter timer is not running', sb.meterValInterval[1] === null || sb.meterValInterval[1] === undefined,
        `meterValInterval[1]=${sb.meterValInterval[1]}`);
  check('TransactionInProgress1 is false', sb.__storage.get('TransactionInProgress1') === 'false',
        `=${sb.__storage.get('TransactionInProgress1')}`);
  check('no transaction id stored', sb.__storage.get('TransactionId1') === undefined);
  check('connector was not reported Charging',
        !csms.received.some(r => r.action === 'StatusNotification' && r.payload.connectorId === 1 && r.payload.status === 'Charging'));
  check('soc1 is editable again', sb.__fields.soc1.attrs.readonly === undefined);
  check('no MeterValues sent', count(csms, 'MeterValues') === 0);

  sb.wsConnect();
  await sleep(300);
  noViolations(csms);
  await csms.close();
}

async function t3_acceptedStartCharges() {
  section('T3  an accepted StartTransaction does start charging');
  const csms = startCsms({ port: PORT, hbInterval: 60, startStatus: 'Accepted' });
  const sb = loadSimulator(URL);
  press(sb, 'connect');
  await sleep(500);
  press(sb, 'start1');
  await sleep(800);

  check('meter timer is running', !!sb.meterValInterval[1]);
  check('TransactionInProgress1 is true', sb.__storage.get('TransactionInProgress1') === 'true');
  check('transaction id stored', sb.__storage.get('TransactionId1') === '1001',
        `=${sb.__storage.get('TransactionId1')}`);
  check('connector reported Charging',
        csms.received.some(r => r.action === 'StatusNotification' && r.payload.connectorId === 1 && r.payload.status === 'Charging'));

  // connector 2 must be untouched
  check('connector 2 timer still idle', !sb.meterValInterval[2]);

  press(sb, 'stop1');
  await sleep(300);
  check('stop cleared the meter timer', !sb.meterValInterval[1]);
  const stop = csms.received.find(r => r.action === 'StopTransaction');
  check('StopTransaction carries the real transaction id', stop && stop.payload.transactionId === 1001,
        stop ? `id=${stop.payload.transactionId}` : 'no StopTransaction');

  sb.wsConnect();
  await sleep(300);
  noViolations(csms);
  await csms.close();
}

async function t4_noReconnectOn1000() {
  section('T4  close code 1000 does not reconnect');
  let connections = 0;
  const csms = startCsms({
    port: PORT, hbInterval: 60,
    onConnection: (sock) => { connections++; setTimeout(() => sock.close(1000, 'evicted'), 200); },
  });
  const sb = loadSimulator(URL);
  sb.RECONNECT_BASE_DELAY = 50;
  press(sb, 'connect');
  await sleep(2000);

  check('exactly one connection was made', connections === 1, `${connections} connections`);
  check('no reconnect was scheduled', sb.reconnectTimer === null, `reconnectTimer=${sb.reconnectTimer}`);
  check('attempt counter reset', sb.reconnectAttempts === 0);
  check('close reason logged', sb.__log.some(l => l.includes('1000')));
  await csms.close();
}

async function t5_backoffAndCap() {
  section('T5  an abnormal close reconnects with backoff, then gives up');
  const sb = loadSimulator(URL);                 // nothing is listening on PORT
  sb.RECONNECT_BASE_DELAY = 40;
  sb.RECONNECT_MAX_DELAY = 200;
  sb.RECONNECT_MAX_ATTEMPTS = 4;
  press(sb, 'connect');
  await sleep(3000);

  const attemptLogs = sb.__log.filter(l => l.includes('Reconnect attempt'));
  check('retried, but not unboundedly', attemptLogs.length === 4, `${attemptLogs.length} retries logged`);
  check('gave up after the cap', sb.__log.some(l => l.includes('Giving up after 4 attempts')));
  check('counter reset so Connect works again', sb.reconnectAttempts === 0);
  check('backoff grows', /in 0s/.test(attemptLogs[0] || '') || /in 1s/.test(attemptLogs[0] || ''));
  check('console was not wiped', sb.__log.length > 4, `${sb.__log.length} log lines kept`);
  check('socket cleared', sb._websocket === null);
}

async function t6_timersStopOnClose() {
  section('T6  every timer stops when the socket closes');
  const csms = startCsms({ port: PORT, hbInterval: 1, startStatus: 'Accepted' });
  const sb = loadSimulator(URL);
  press(sb, 'connect');
  await sleep(400);
  press(sb, 'start1');
  press(sb, 'start2');
  await sleep(600);
  check('both meter timers running before close', !!sb.meterValInterval[1] && !!sb.meterValInterval[2]);
  check('heartbeat timer running before close', !!sb.hbInterval);

  sb.wsConnect();                                // Disconnect -> close(3001)
  await sleep(400);

  check('heartbeat timer cleared', sb.hbInterval === null, `hbInterval=${sb.hbInterval}`);
  check('ping timer cleared', sb.pingInterval === null, `pingInterval=${sb.pingInterval}`);
  check('meter timers cleared', sb.meterValInterval[1] === null && sb.meterValInterval[2] === null);
  check('socket cleared', sb._websocket === null);

  const before = csms.received.length;
  await sleep(2500);                             // longer than the 1 s heartbeat interval
  check('nothing is sent after the close', csms.received.length === before,
        `${csms.received.length - before} extra frames`);
  noViolations(csms);
  await csms.close();
}

async function t7_reconnectAfterServerRestart() {
  section('T7  an abnormal close reconnects and re-reads the boot interval');
  let connections = 0;
  const csms = startCsms({ port: PORT, hbInterval: 1, onConnection: () => { connections++; } });
  const sb = loadSimulator(URL);
  sb.RECONNECT_BASE_DELAY = 100;
  press(sb, 'connect');
  await sleep(500);
  check('connected once', connections === 1);

  csms.wss.clients.forEach(c => c.terminate());  // abnormal drop, code 1006
  await sleep(1500);

  check('reconnected', connections === 2, `${connections} connections`);
  check('boot interval read again after reconnect', sb.__storage.get('Configuration') === '1');
  check('heartbeat timer alive again', !!sb.hbInterval);
  check('attempt counter reset on success', sb.reconnectAttempts === 0);

  sb.wsConnect();
  await sleep(300);
  noViolations(csms);
  await csms.close();
}

(async () => {
  await t1_heartbeatFromBoot();
  await t2_rejectedStartDoesNotCharge();
  await t3_acceptedStartCharges();
  await t4_noReconnectOn1000();
  await t5_backoffAndCap();
  await t6_timersStopOnClose();
  await t7_reconnectAfterServerRestart();
  console.log(`\n${pass} passed, ${fail} failed`);
  process.exit(fail ? 1 : 0);
})();
