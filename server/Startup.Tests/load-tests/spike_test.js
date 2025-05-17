import http from 'k6/http';
import { b64decode } from 'k6/encoding';
import { check, sleep, fail } from 'k6';

const BASE = 'http://localhost:5000';
if (!__ENV.LOGIN_EMAIL || !__ENV.LOGIN_PASSWORD) {
    console.error('→ please set LOGIN_EMAIL and LOGIN_PASSWORD');
    fail('missing env‑vars');
}
// Setup

export function setup() {
    const loginRes = http.post(
        `${BASE}/api/Auth/Login`,
        JSON.stringify({
            email:    __ENV.LOGIN_EMAIL,
            password: __ENV.LOGIN_PASSWORD,
        }),
        { headers: { 'Content-Type': 'application/json' } }
    );
    check(loginRes, {
        'login 200': (r) => r.status === 200,
        'got jwt':   (r) => !!r.json('jwt'),
    }) || fail('login failed');
    
    const jwt = loginRes.json('jwt');
    const parts   = jwt.split('.');
    if (parts.length !== 3) {fail('invalid JWT format');}
    
    let b64 = parts[1]
        .replace(/-/g, '+')
        .replace(/_/g, '/');
    while (b64.length % 4 !== 0) { b64 += '='; }

    const jsonPayload = b64decode(b64, 'std', 's');
    const claims = JSON.parse(jsonPayload);
    const userId = claims.Id;

    return { 
        token: jwt,
        userId: userId,
    };
}

// k6 Test Configuration
export let options = {
    scenarios: {
        spike_alerts: {
            executor: 'ramping-arrival-rate',
            startRate: 1,
            timeUnit: '1s',
            preAllocatedVUs: 20,
            maxVUs: 100,
            stages: [
                { target: 10, duration: '5s' },
                { target: 20, duration: '10s' },
                { target: 0,   duration: '5s' },
            ],
            exec: 'testGetAlerts',
        },
        spike_plants: {
            executor: 'ramping-arrival-rate',
            startRate: 1,
            timeUnit: '1s',
            preAllocatedVUs: 20,
            maxVUs: 100,
            stages: [
                { target: 10,  duration: '5s'   },
                { target: 20, duration: '10s'  },
                { target: 0,   duration: '5s'   },
            ],
            exec: 'testGetAllPlants',
        },
        spike_history: {
            executor: 'ramping-arrival-rate',
            startRate: 1,
            timeUnit: '1s',
            preAllocatedVUs: 20,
            maxVUs: 100,
            stages: [
                { target: 10,  duration: '5s'  },
                { target: 20, duration: '10s' },
                { target: 0,   duration: '5s'  },
            ],
            exec: 'testGetRecentSensorData',
        },
    },
    thresholds: {
        'http_req_duration{scenario:spike_plants}':  ['p(95)<500','p(99)<1000'],
        'http_req_duration{scenario:spike_history}': ['p(95)<500','p(99)<1000'],
        'http_req_failed':                          ['rate<0.01'],
    },
};


function authParams(token) {
    return {
        headers: {
            Authorization: token,
            'Content-Type': 'application/json',
        },
    };
}


// Test functions

export function testGetAllPlants(data) {
    console.log(`Getting all plants for user ${data.userId}`);
    const res = http.get(
        `${BASE}/api/Plant/GetAllPlants?userId=${encodeURIComponent(data.userId)}`,
        authParams(data.token),
    );
    console.log(`GetAllPlants → ${res.status}\n${res.body}`);
    check(res, {
        'status was 200':  (r) => r.status === 200,
        'got some plants': (r) => Array.isArray(r.json()),
    });
    sleep(1);
}

export function testGetAlerts(data) {
    const res = http.get(
        `${BASE}/api/Alert/GetAlerts?year=2025`,
        authParams(data.token),
    );
    console.log(`GetAllAlerts → ${res.status}\n${res.body}`);
    check(res, {
        'status was 200': (r) => r.status === 200,
        'returned an array': (r) => Array.isArray(r.json()),
    });
    sleep(1);
}


export function testGetRecentSensorData(data) {
    const res = http.get(
        `${BASE}/api/GreenhouseDevice/GetRecentSensorDataForAllUserDevice`,
        authParams(data.token),
    );
    console.log(`GetAllDeviceSensorData → ${res.status}\n${res.body}`);
    check(res, {
        'status was 200':        (r) => r.status === 200,
    });
    sleep(1);
}