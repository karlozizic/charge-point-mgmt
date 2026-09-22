// End-to-end: the real simulator script against the real CPMS.Proxy and CPMS.API.
const { loadSimulator, sleep } = require('./harness');

const PROXY = 'ws://127.0.0.1:5000/OCPP';
const API = 'http://localhost:5023/api';
const CP = process.env.CP_ID || 'CP-SMOKE-1';
let CP_GUID = null;                                      // looked up at startup; the DB holds older data too
const mine = (rows) => rows.filter(r => r.chargePointId === CP_GUID);

let pass = 0, fail = 0;
const check = (name, ok, detail) => {
  if (ok) { pass++; console.log(`  PASS  ${name}`); }
  else { fail++; console.log(`  FAIL  ${name}${detail ? '  -> ' + detail : ''}`); }
};
const section = (t) => console.log(`\n== ${t}`);
const api = async (p) => (await fetch(API + p)).json();

function boot(tag) {
  const sb = loadSimulator(PROXY);
  sb.$('#CP').val(CP);
  sb.$('#TAG').val(tag);
  sb.$('#environment').val(PROXY);
  return sb;
}

(async () => {
  CP_GUID = (await api('/ChargePoints')).filter(c => c.ocppChargerId === CP).map(c => c.id)[0];
  if (!CP_GUID) {
    console.error(`No charge point "${CP}" in the API. Seed it, plus tags TAG-OK and a blocked TAG-BLOCKED.`);
    process.exit(2);
  }

  section('L1  BootNotification against the real gateway');
  let sb = boot('TAG-OK');
  sb.__handlers.connect();
  await sleep(2500);

  check('boot response recognised', sb.__storage.get('Configuration') !== null,
        `Configuration=${sb.__storage.get('Configuration')}`);
  check('heartbeat timer started from that interval', !!sb.hbInterval);
  check('socket is open', sb._websocket && sb._websocket.readyState === 1);
  const cp = (await api('/ChargePoints')).find(c => c.ocppChargerId === CP);
  check('charge point is registered', !!cp);
  check('connectors reported by StatusNotification', cp && cp.totalConnectors === 2,
        cp ? `totalConnectors=${cp.totalConnectors}` : 'no charge point');

  // The gateway keeps an authorization cache: StartTransaction is refused with Invalid unless the
  // charger sent Authorize for that tag first. The HTML simulator's Authorize button does this.
  sb.__handlers.send();
  await sleep(1200);
  check('Authorize was accepted for TAG-OK', sb.__log.some(l => l.includes('"status":"Accepted"')),
        sb.__log.slice(-2).join(' | '));

  section('L2  a blocked tag must not start charging');
  sb.$('#TAG').val('TAG-BLOCKED');
  sb.__handlers.send();          // Authorize first, so the refusal comes from the tag, not the cache
  await sleep(1200);
  sb.__handlers.start1();
  await sleep(2000);
  check('meter timer not running', !sb.meterValInterval[1], `=${sb.meterValInterval[1]}`);
  check('TransactionInProgress1 false', sb.__storage.get('TransactionInProgress1') === 'false');
  check('no transaction id stored', sb.__storage.get('TransactionId1') === undefined);
  check('rejection was logged', sb.__log.some(l => l.includes('Transaction rejected')),
        sb.__log.slice(-3).join(' | '));

  section('L3  both connectors start at once and each gets its own transaction');
  sb.$('#TAG').val('TAG-OK');
  sb.__handlers.send();
  await sleep(1200);
  sb.__handlers.start1();
  sb.__handlers.start2();
  await sleep(2500);

  const tx1 = sb.__storage.get('TransactionId1');
  const tx2 = sb.__storage.get('TransactionId2');
  check('connector 1 got a transaction', !!tx1, `tx1=${tx1}`);
  check('connector 2 got a transaction', !!tx2, `tx2=${tx2}`);
  check('the two transactions differ', tx1 && tx2 && tx1 !== tx2, `${tx1} vs ${tx2}`);
  check('both meter timers running', !!sb.meterValInterval[1] && !!sb.meterValInterval[2]);
  check('nothing left pending', Object.keys(sb.pendingStarts).length === 0);

  const active = mine(await api('/ChargeSessions/active'));
  check('API shows two active sessions for this charge point', active.length === 2, `${active.length} active`);
  check('the API agrees on the transaction ids',
        active.map(s => String(s.transactionId)).sort().join(',') === [tx1, tx2].sort().join(','),
        `api=${active.map(s => s.transactionId)} sim=${[tx1, tx2]}`);

  section('L4  MeterValues reach the API');
  console.log('  (waiting 17 s for the 15 s meter interval)');
  await sleep(17000);
  const sessions = mine(await api('/ChargeSessions/active'));
  const withMeter = sessions.filter(s => (s.meterValues || []).length > 0);
  check('at least one session accumulated meter values', withMeter.length >= 1,
        sessions.map(s => `${s.id && s.id.slice(0, 8)}:${(s.meterValues || []).length}`).join(' '));
  check('no CALLERROR came back', !sb.__log.some(l => l.includes('JSON is not accepted')),
        sb.__log.filter(l => l.includes('not accepted')).join(' | '));

  section('L5  stop both, and the sessions close');
  sb.__handlers.stop1();
  sb.__handlers.stop2();
  await sleep(2500);
  check('meter timers cleared', !sb.meterValInterval[1] && !sb.meterValInterval[2]);
  const stillActive = mine(await api('/ChargeSessions/active'));
  check('no active sessions left for this charge point', stillActive.length === 0, `${stillActive.length} still active`);
  const all = await api('/ChargeSessions/by-chargepoint/' + CP_GUID);
  const closed = all.filter(s => s.status === 'Stopped' && [tx1, tx2].includes(String(s.transactionId)));
  check('both sessions are Stopped', closed.length === 2, `${closed.length} stopped of ${all.length}`);
  check('energy was recorded', closed.every(s => s.energyDeliveredKWh > 0),
        closed.map(s => s.energyDeliveredKWh).join(', '));

  section('L6  disconnect leaves nothing running');
  sb.wsConnect();
  await sleep(1500);
  check('heartbeat timer cleared', sb.hbInterval === null);
  check('ping timer cleared', sb.pingInterval === null);
  check('socket cleared', sb._websocket === null);
  const before = sb.__log.length;
  await sleep(3000);
  check('no errors after the close', !sb.__log.slice(before).some(l => /error|Cannot read/i.test(l)),
        sb.__log.slice(before).join(' | '));

  console.log(`\n${pass} passed, ${fail} failed`);
  process.exit(fail ? 1 : 0);
})().catch(e => { console.error('HARNESS ERROR', e); process.exit(2); });
