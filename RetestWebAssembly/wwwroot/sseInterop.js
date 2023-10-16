window.sseClient = {
    eventSource: null,
    connect: function (url, dotNetObject) {
        this.eventSource = new EventSource(url);
        this.eventSource.addEventListener('message', function (event) {
            dotNetObject.invokeMethodAsync('HandleEventReceived', event.data.toString());
        });
        this.eventSource.addEventListener('error', function (event) {
            dotNetObject.invokeMethod(event);
        });
    },
    disconnect: function () {
        if (this.eventSource) {
            this.eventSource.close();
            this.eventSource = null;
        }
    }
};


window.sseInterop = {
    connectSse: function (url, dotNetObject) {
        window.sseClient.connect(url, dotNetObject);
    },
    disconnectSse: function () {
        window.sseClient.disconnect();
    }
};