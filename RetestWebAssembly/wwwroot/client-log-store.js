window.clientLogStore = (() => {
    const dbName = "RetestLogsDb";
    const storeName = "logs";
    const version = 1;

    function openDb() {
        return new Promise((resolve, reject) => {
            const req = indexedDB.open(dbName, version);
            req.onupgradeneeded = () => {
                const db = req.result;
                if (!db.objectStoreNames.contains(storeName)) {
                    db.createObjectStore(storeName, { keyPath: "localId", autoIncrement: true });
                }
            };
            req.onsuccess = () => resolve(req.result);
            req.onerror = () => reject(req.error);
        });
    }

    async function add(entry) {
        const db = await openDb();

        // IndexedDB: keyPath localId + autoIncrement => localId nesmí být null
        const item = { ...entry };
        if (item.localId == null) {
            delete item.localId;
        }
        if (item.LocalId == null) {
            delete item.LocalId;
        }

        await new Promise((resolve, reject) => {
            const tx = db.transaction(storeName, "readwrite");
            tx.objectStore(storeName).add(item);
            tx.oncomplete = () => resolve();
            tx.onerror = () => reject(tx.error);
        });

        db.close();
    }

    async function getBatch(maxCount) {
        const db = await openDb();
        const items = [];
        await new Promise((resolve, reject) => {
            const tx = db.transaction(storeName, "readonly");
            const req = tx.objectStore(storeName).openCursor();
            req.onsuccess = () => {
                const c = req.result;
                if (!c || items.length >= maxCount) return resolve();
                items.push(c.value);
                c.continue();
            };
            req.onerror = () => reject(req.error);
        });
        db.close();
        return items;
    }

    async function deleteBatch(ids) {
        const db = await openDb();
        await new Promise((resolve, reject) => {
            const tx = db.transaction(storeName, "readwrite");
            const store = tx.objectStore(storeName);
            for (const id of ids) store.delete(id);
            tx.oncomplete = () => resolve();
            tx.onerror = () => reject(tx.error);
        });
        db.close();
    }

    async function prune(maxAgeDays, maxCount) {
        const cutoff = Date.now() - (maxAgeDays * 24 * 60 * 60 * 1000);
        const db = await openDb();

        const all = await new Promise((resolve, reject) => {
            const tx = db.transaction(storeName, "readonly");
            const req = tx.objectStore(storeName).getAll();
            req.onsuccess = () => resolve(req.result || []);
            req.onerror = () => reject(req.error);
        });

        const stale = [];
        for (const item of all) {
            const t = new Date(item.utcTime ?? item.UtcTime ?? 0).getTime();
            if (!Number.isNaN(t) && t < cutoff) stale.push(item.localId);
        }

        const survivors = all
            .filter(x => !stale.includes(x.localId))
            .sort((a, b) => a.localId - b.localId);

        const overflow = Math.max(0, survivors.length - maxCount);
        for (let i = 0; i < overflow; i++) stale.push(survivors[i].localId);

        if (stale.length > 0) {
            await deleteBatch(stale);
        }

        db.close();
        return stale.length;
    }

    return { add, getBatch, deleteBatch, prune };
})();