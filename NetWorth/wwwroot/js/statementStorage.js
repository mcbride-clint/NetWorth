// statementStorage.js
// Provides functions to save and load statement data from browser IndexedDB

window.statementStorage = (() => {
    const DB_NAME = 'NetWorthDB';
    const STORE_NAME = 'statements';
    const DB_VERSION = 1;

    function openDb() {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open(DB_NAME, DB_VERSION);
            request.onupgradeneeded = e => {
                const db = e.target.result;
                if (!db.objectStoreNames.contains(STORE_NAME))
                    db.createObjectStore(STORE_NAME);
            };
            request.onsuccess = e => resolve(e.target.result);
            request.onerror = e => reject(e.target.error);
        });
    }

    return {
        saveStatement: async function (key, statementJson) {
            const db = await openDb();
            return new Promise((resolve, reject) => {
                const tx = db.transaction(STORE_NAME, 'readwrite');
                tx.objectStore(STORE_NAME).put(statementJson, key);
                tx.oncomplete = () => resolve();
                tx.onerror = e => reject(e.target.error);
            });
        },
        loadStatement: async function (key) {
            const db = await openDb();
            return new Promise(async (resolve, reject) => {
                const tx = db.transaction(STORE_NAME, 'readonly');
                const req = tx.objectStore(STORE_NAME).get(key);
                req.onsuccess = async e => {
                    let value = e.target.result ?? null;
                    // One-time migration from localStorage
                    if (value === null) {
                        const legacy = localStorage.getItem(key);
                        if (legacy !== null) {
                            await window.statementStorage.saveStatement(key, legacy);
                            localStorage.removeItem(key);
                            value = legacy;
                        }
                    }
                    resolve(value);
                };
                req.onerror = e => reject(e.target.error);
            });
        },
        clearStatement: async function (key) {
            const db = await openDb();
            return new Promise((resolve, reject) => {
                const tx = db.transaction(STORE_NAME, 'readwrite');
                tx.objectStore(STORE_NAME).delete(key);
                tx.oncomplete = () => resolve();
                tx.onerror = e => reject(e.target.error);
            });
        }
    };
})();
