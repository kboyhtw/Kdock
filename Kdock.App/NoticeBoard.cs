namespace Kdock.App;

public static class NoticeBoard
{
    public static int Count { get; private set; }
    public static event Action? Changed;

    public static void Push()
    {
        Count++;
        Changed?.Invoke();
    }

    public static void Clear()
    {
        Count = 0;
        Changed?.Invoke();
    }
}
