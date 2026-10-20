using System.IO;
using System.Windows;
using Kdock.Wire;

namespace Kdock.App;

public partial class App : System.Windows.Application
{
    public static AppState State { get; private set; } = null!;
    public static SessionListener LanListener { get; private set; } = null!;
    public static MeetingClient Meeting { get; private set; } = null!;
    public static ConversationViewModel Conversation { get; private set; } = null!;
    public static bool IsPeopleOpen { get; set; }
    public static event EventHandler<SessionAcceptedEventArgs>? SessionAccepted;
    public static event Func<IncomingCall, Task>? CallReceived;
    public static event Func<IncomingPair, Task>? PairRequested;

    protected override void OnStartup(StartupEventArgs e)
    {
        State = new AppState();
        Conversation = new ConversationViewModel(State.DataDirectory);
        Directory.CreateDirectory(ConversationViewModel.InboxFolder);
        Conversation.ChatTouched += RememberChat;
        Meeting = new MeetingClient(State.DataDirectory);
        Meeting.StartCallEvents(
            State.DeskId,
            call => CallReceived?.Invoke(call) ?? Task.CompletedTask,
            pair => PairRequested?.Invoke(pair) ?? Task.CompletedTask);
        LanListener = new SessionListener(ReadListenerPort(e.Args));
        LanListener.SessionAccepted += LanSessionAccepted;
        LanListener.Start();
        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        LanListener.SessionAccepted -= LanSessionAccepted;
        Conversation.ChatTouched -= RememberChat;
        Conversation.DisposeAsync().AsTask().GetAwaiter().GetResult();
        Meeting.Dispose();
        LanListener.DisposeAsync().AsTask().GetAwaiter().GetResult();
        base.OnExit(e);
    }

    private static void RememberChat(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;
        try
        {
            new PeopleStore(State.DataDirectory).AddRecent(key);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static async void LanSessionAccepted(object? sender, SessionAcceptedEventArgs e)
    {
        SessionAccepted?.Invoke(sender, e);
        try
        {
            if (Conversation.TakeSelfPeer())
            {
                Conversation.AttachPeer(e.Session);
                return;
            }
            await Conversation.AcceptSessionAsync(e.Session, e.RemoteAddress);
            RememberChat(e.RemoteAddress);
        }
        catch
        {
            await e.Session.DisposeAsync();
        }
    }

    private static int ReadListenerPort(string[] args)
    {
        for (int index = 0; index < args.Length - 1; index++)
        {
            if (!args[index].Equals("--port", StringComparison.OrdinalIgnoreCase))
                continue;
            if (int.TryParse(args[index + 1], out int port) && port is >= 1024 and <= 65535)
                return port;
        }

        return Protocol.LanPort;
    }
}
