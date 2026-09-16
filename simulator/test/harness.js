// Loads the real <script> out of simulator/OCPP_1.6._Charger_SImulator.html and runs it in Node
// against a fake CSMS over a real WebSocket. Nothing in the simulator source is modified.
const fs = require('fs');
const path = require('path');
const vm = require('vm');
const WS = require('ws');
const ocpp = require('./ocpp');

const HTML = process.env.SIM_HTML || path.resolve(__dirname, '..', 'OCPP_1.6._Charger_SImulator.html');

function extractScript(file) {
  const src = fs.readFileSync(file, 'utf8').replace(/\r\n/g, '\n');
  const open = src.match(/\n[ \t]*<script>[ \t]*\n/);
  if (!open) throw new Error('opening <script> not found');
  const start = open.index + open[0].length;
  const end = src.indexOf('</script>', start);
  if (end < 0) throw new Error('closing </script> not found');
  return src.slice(start, end);
}

// --- minimal DOM / jQuery stubs -------------------------------------------
function makeSandbox(log) {
  const fields = {};          // id -> { value, attrs }
  function el(id) {
    if (!fields[id]) fields[id] = { id, value: defaultValue(id), attrs: {} };
    return {
      get value() { return fields[id].value; },
      set value(v) { fields[id].value = v; },
      setAttribute(k, v) { fields[id].attrs[k] = v; },
      removeAttribute(k) { delete fields[id].attrs[k]; },
    };
  }
  function defaultValue(id) {
    if (id.startsWith('soc')) return '20';
    if (id.startsWith('pai')) return '11000';
    if (id.startsWith('statusValue')) return 'Available';
    if (id === 'CP') return 'CP-TEST-1';
    if (id === 'TAG') return 'TAG-1';
    if (id === 'PASSWORD') return '';
    if (id === 'environment') return sandbox.__wsurl;
    return '';
  }

  const $ = (sel) => {
    const id = typeof sel === 'string' && sel.startsWith('#') ? sel.slice(1) : String(sel);
    return {
      val(v) { if (v === undefined) return el(id).value; el(id).value = v; return this; },
      text() { return this; },
      css() { return this; },
      html() { return this; },
      append(s) { log.push(String(s).replace(/<\/?li>/g, '')); return this; },
      ready(fn) { fn(); return this; },
      click(fn) { handlers[id] = fn; return this; },
      change() { return this; },
      on() { return this; },
      prop() { return this; },
      attr() { return this; },
    };
  };
  const handlers = {};         // element id -> click handler, so tests can "press" buttons

  const storage = new Map();
  const sandbox = {
    console: { log: () => {}, error: () => {} },
    setInterval, clearInterval, setTimeout, clearTimeout,
    JSON, Math, Number, String, Date, parseInt, parseFloat, isNaN, Object, Array,
    btoa: (s) => Buffer.from(s, 'binary').toString('base64'),
    WebSocket: WS,
    window: { crypto: { getRandomValues: (a) => { for (let i = 0; i < a.length; i++) a[i] = (Math.random() * 256) | 0; return a; } } },
    location: { reload: () => log.push('[location.reload]') },
    document: { getElementById: el },
    sessionStorage: {
      getItem: (k) => (storage.has(k) ? storage.get(k) : null),
      setItem: (k, v) => storage.set(k, String(v)),
      removeItem: (k) => storage.delete(k),
    },
    $,
    __fields: fields,
    __storage: storage,
    __log: log,
    __handlers: handlers,
  };
  sandbox.globalThis = sandbox;
  return sandbox;
}

function loadSimulator(wsurl) {
  const log = [];
  const sandbox = makeSandbox(log);
  sandbox.__wsurl = wsurl;
  vm.createContext(sandbox);
  vm.runInContext(extractScript(HTML), sandbox, { filename: 'simulator.js' });
  return sandbox;
}

// --- fake CSMS -------------------------------------------------------------
// Validates every frame the simulator sends against the official OCPP 1.6 schemas.
function startCsms(opts = {}) {
  const port = opts.port || 5099;
  const received = [];
  const violations = [];
  const answers = [];          // CALLRESULTs the simulator sent back, with the action they answer
  const wss = new WS.Server({ port, handleProtocols: (protocols) => (protocols.has('ocpp1.6') ? 'ocpp1.6' : false) });
  let txId = 1000;

  wss.on('connection', (sock) => {
    const outstanding = new Map();   // uniqueId -> action we asked for
    const seenCallIds = new Set();

    sock.call = (action, payload) => {
      const uid = 'srv-' + Math.random().toString(36).slice(2, 10);
      outstanding.set(uid, action);
      sock.send(JSON.stringify([2, uid, action, payload]));
      return uid;
    };

    if (opts.onConnection) opts.onConnection(sock, wss);

    sock.on('message', (raw) => {
      const text = raw.toString();
      if (text === 'ping') { received.push({ action: 'ping' }); return; }

      let m;
      try { m = JSON.parse(text); } catch { violations.push(`not JSON: ${text.slice(0, 60)}`); return; }

      const framing = ocpp.checkFrame(m);
      if (framing) { violations.push(framing); return; }

      if (m[0] === 3) {
        const action = outstanding.get(m[1]);
        if (!action) { violations.push(`CALLRESULT for unknown id ${m[1]}`); return; }
        outstanding.delete(m[1]);
        const bad = ocpp.checkPayload(action + 'Response', m[2]);
        if (bad) violations.push(bad);
        answers.push({ action, payload: m[2] });
        return;
      }
      if (m[0] === 4) { answers.push({ action: outstanding.get(m[1]), error: m[2] }); outstanding.delete(m[1]); return; }

      const [, uid, action, payload] = m;
      if (seenCallIds.has(uid)) violations.push(`uniqueId reused across CALLs: ${uid}`);
      seenCallIds.add(uid);

      const bad = ocpp.checkPayload(action, payload);
      if (bad) violations.push(bad);

      received.push({ action, payload, uid, at: Date.now() });

      let conf = {};
      if (action === 'BootNotification') {
        conf = { status: opts.bootStatus || 'Accepted', currentTime: new Date().toISOString(), interval: opts.hbInterval || 2 };
      } else if (action === 'StartTransaction') {
        conf = { transactionId: ++txId, idTagInfo: { status: opts.startStatus || 'Accepted' } };
      } else if (action === 'StopTransaction' || action === 'Authorize') {
        conf = { idTagInfo: { status: 'Accepted' } };
      }
      sock.send(JSON.stringify([3, uid, conf]));
    });
  });

  return {
    wss, received, violations, answers, port,
    client: () => Array.from(wss.clients)[0],
    close: () => new Promise((r) => wss.close(r)),
  };
}

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

module.exports = { loadSimulator, startCsms, sleep, extractScript, HTML };
