namespace Shared.Events;

public class CountDecreasedEvent
{
    public string ProductId { get; set; }
    public int DecreasedCount { get; set; }
}