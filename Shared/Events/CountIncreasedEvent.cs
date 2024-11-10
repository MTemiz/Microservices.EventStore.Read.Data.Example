namespace Shared.Events;

public class CountIncreasedEvent
{
    public string ProductId { get; set; }
    public int IncrementCount { get; set; }
}