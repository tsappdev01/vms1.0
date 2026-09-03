/* Small interop surface. Kept deliberately thin: anything that can be done in C# is. */
window.vms = {
    /**
     * Hands the browser a CSV to save. Blazor Server cannot write a file directly, so the
     * bytes cross the circuit and a blob URL is created here.
     */
    downloadCsv: function (fileName, content) {
        const blob = new Blob([content], { type: 'text/csv;charset=utf-8' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    }
};
