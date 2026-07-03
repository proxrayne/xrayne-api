namespace Infrastructure.Services;

public interface IEventStreamManager
{
    EventStreamSubscription<object> Subscribe(string streamKey);
    EventStreamSubscription<T> Subscribe<T>(string streamKey);
    bool Unsubscribe(Guid subscriptionId);
    void Dispatch(string streamKey, object? data);
    void Dispatch<T>(string streamKey, T data);
}
