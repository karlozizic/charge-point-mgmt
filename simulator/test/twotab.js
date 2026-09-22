// Two "tabs" on one charger id against the real gateway: the evicted one must not fight back.
const { loadSimulator, sleep } = require('./harness');
const PROXY='ws://127.0.0.1:5000/OCPP', CP='CP-SMOKE-1';
let pass=0,fail=0;
const check=(n,ok,d)=>{ok?(pass++,console.log('  PASS  '+n)):(fail++,console.log('  FAIL  '+n+(d?'  -> '+d:'')));};
function tab(){const sb=loadSimulator(PROXY);sb.$('#CP').val(CP);sb.$('#TAG').val('TAG-OK');sb.$('#environment').val(PROXY);return sb;}
(async()=>{
  const a=tab(); a.__handlers.connect(); await sleep(1500);
  check('tab A connected', a._websocket && a._websocket.readyState===1);

  const b=tab(); b.__handlers.connect(); await sleep(2500);
  check('tab B connected', b._websocket && b._websocket.readyState===1);
  check('tab A was closed by the gateway', a._websocket===null, `readyState=${a._websocket&&a._websocket.readyState}`);
  check('tab A did not schedule a reconnect', a.reconnectTimer===null, `timer=${a.reconnectTimer}`);
  check('tab A logged the eviction', a.__log.some(l=>l.includes('1000')), a.__log.slice(-2).join(' | '));
  check('tab A stopped every timer', a.hbInterval===null && a.pingInterval===null);

  console.log('  (holding 6 s to prove there is no ping-pong)');
  const bLogBefore=b.__log.length;
  await sleep(6000);
  check('tab B is still connected', b._websocket && b._websocket.readyState===1);
  check('tab B was never evicted', !b.__log.slice(bLogBefore).some(l=>l.includes('1000')));
  check('tab A stayed down', a._websocket===null);

  b.wsConnect(); await sleep(500);
  console.log(`\n${pass} passed, ${fail} failed`);
  process.exit(fail?1:0);
})().catch(e=>{console.error('ERR',e);process.exit(2);});
