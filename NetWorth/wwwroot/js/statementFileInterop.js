window.statementFileInterop = {
    exportStatement: function (json, filename) {
        const blob = new Blob([json], { type: 'application/json' });
        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(link.href);
    },
    importStatement: async function () {
        return new Promise((resolve, reject) => {
            const input = document.createElement('input');
            input.type = 'file';
            input.accept = '.json,application/json';
            input.onchange = e => {
                const file = e.target.files[0];
                if (!file) {
                    resolve(null);
                    return;
                }
                const reader = new FileReader();
                reader.onload = event => {
                    resolve(event.target.result);
                };
                reader.onerror = err => reject(err);
                reader.readAsText(file);
            };
            input.click();
        });
    }
};
