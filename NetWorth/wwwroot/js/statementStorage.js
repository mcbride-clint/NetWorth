// statementStorage.js
// Provides functions to save and load statement data from browser localStorage

window.statementStorage = {
    saveStatement: function(key, statementJson) {
        localStorage.setItem(key, statementJson);
    },
    loadStatement: function(key) {
        return localStorage.getItem(key);
    },
    clearStatement: function(key) {
        localStorage.removeItem(key);
    }
};
