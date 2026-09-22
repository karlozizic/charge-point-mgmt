// Validates OCPP 1.6-J frames against the official OCA JSON schemas in docs/ocpp/schemas.
const fs = require('fs');
const path = require('path');
const Ajv = require('ajv');

const SCHEMA_DIR = process.env.OCPP_SCHEMAS || path.resolve(__dirname, '..', '..', 'docs', 'ocpp', 'schemas');

const ajv = new Ajv({ schemaId: 'id', allErrors: true });
ajv.addMetaSchema(require('ajv/lib/refs/json-schema-draft-04.json'));

const validators = {};
for (const file of fs.readdirSync(SCHEMA_DIR)) {
  if (!file.endsWith('.json')) continue;
  const name = file.slice(0, -5);
  validators[name] = ajv.compile(JSON.parse(fs.readFileSync(path.join(SCHEMA_DIR, file), 'utf8')));
}

// OCPP-J 1.6 framing: CALL [2,id,action,payload], CALLRESULT [3,id,payload],
// CALLERROR [4,id,errorCode,errorDescription,errorDetails].
function checkFrame(frame) {
  if (!Array.isArray(frame)) return 'frame is not an array';
  const [type, uniqueId] = frame;
  if (typeof uniqueId !== 'string' || uniqueId.length === 0) return 'uniqueId missing';
  if (uniqueId.length > 36) return `uniqueId longer than 36 (${uniqueId.length})`;
  if (type === 2) return frame.length === 4 ? null : `CALL must have 4 elements, has ${frame.length}`;
  if (type === 3) return frame.length === 3 ? null : `CALLRESULT must have 3 elements, has ${frame.length}`;
  if (type === 4) {
    if (frame.length !== 5) return `CALLERROR must have 5 elements, has ${frame.length}`;
    if (typeof frame[2] !== 'string') return 'CALLERROR errorCode must be a string';
    if (typeof frame[3] !== 'string') return 'CALLERROR errorDescription must be a string';
    return null;
  }
  return `unknown message type ${type}`;
}

// name is the schema file name: "StartTransaction" for a request, "ResetResponse" for an answer.
function checkPayload(name, payload) {
  const validate = validators[name];
  if (!validate) return `no schema for ${name}`;
  if (validate(payload)) return null;
  return validate.errors.map((e) => `${name}${e.dataPath} ${e.message}`).join('; ');
}

module.exports = { checkFrame, checkPayload, knownSchemas: Object.keys(validators) };
