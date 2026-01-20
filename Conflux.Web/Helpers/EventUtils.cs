using Microsoft.AspNetCore.Components;

namespace Conflux.Web.Helpers;

public static class EventUtils {
    public static Action AsNonRenderingEventHandler(Action callback) {
        return new ActionReceiver(callback).Invoke;
    }
    
    public static Action<TValue> AsNonRenderingEventHandler<TValue>(Action<TValue> callback) {
        return new ActionReceiver<TValue>(callback).Invoke;
    }
    
    public static Func<Task> AsNonRenderingEventHandler(Func<Task> callback) {
        return new FuncReceiver(callback).Invoke;
    }
    
    public static Func<TValue, Task> AsNonRenderingEventHandler<TValue>(Func<TValue, Task> callback) {
        return new FuncReceiver<TValue>(callback).Invoke;
    }
    
    public static Func<Task> AsNonRenderingEventHandler(EventCallback callback) {
        return new EventCallbackReceiver(callback).Invoke;
    }
    
    public static Func<TValue, Task> AsNonRenderingEventHandler<TValue>(EventCallback<TValue> callback) {
        return new EventCallbackReceiver<TValue>(callback).Invoke;
    }

    private record ActionReceiver(Action callback) : ReceiverBase {
        public void Invoke() {
            callback();
        }
    }

    private record ActionReceiver<T>(Action<T> callback) : ReceiverBase {
        public void Invoke(T arg) {
            callback(arg);
        }
    }

    private record FuncReceiver(Func<Task> callback) : ReceiverBase {
        public Task Invoke() {
            return callback();
        }
    }

    private record FuncReceiver<T>(Func<T, Task> callback) : ReceiverBase {
        public Task Invoke(T arg) {
            return callback(arg);
        }
    }
    
    private record EventCallbackReceiver(EventCallback callback) : ReceiverBase {
        public Task Invoke() {
            return callback.InvokeAsync();
        }
    }
    
    private record EventCallbackReceiver<T>(EventCallback<T> callback) : ReceiverBase {
        public Task Invoke(T arg) {
            return callback.InvokeAsync(arg);
        }
    }

    private record ReceiverBase : IHandleEvent {
        public Task HandleEventAsync(EventCallbackWorkItem item, object? arg) => item.InvokeAsync(arg);
    }
}