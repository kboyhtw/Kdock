using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Kdock.App;

public partial class RemoteWindow : Window
{
    private readonly ConversationViewModel _conversation;
    private readonly Action? _openFiles;
    private int _sourceWidth = 1;
    private int _sourceHeight = 1;
    private System.Windows.Point _lastSent;
    private DateTime _lastMove;
    private bool _hasSent;
    private bool _full;
    private bool _pinned = true;
    private readonly object _frameLock = new();
    private readonly Queue<FramePiece> _pieces = new();
    private int _frameBusy;
    private int _inKeys;
    private int _inRegions;
    private int _inOther;
    private long _inBytes;
    private int _failed;
    private int _applied;
    private int _presented;
    private string _lastFailure = string.Empty;
    private long _lastPresentTick;
    private readonly DispatcherTimer _statsTimer;
    private static readonly System.Reflection.PropertyInfo? OperationName =
        typeof(DispatcherOperation).GetProperty("Name", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
    private long _uiBeat = Environment.TickCount64;
    private int _beatPending;
    private long _opStarted;
    private long _slowOpMs;
    private string _slowOp = string.Empty;
    private long _lastPieceTick;
    private volatile bool _watching = true;
    private readonly object _presentLock = new();
    private byte[]? _presentBuffer;
    private byte[]? _spareBuffer;
    private int _presentStride;
    private int _presentW;
    private int _presentH;
    private int _presentScreenW = 1;
    private int _presentScreenH = 1;
    private int _presentGeneration;
    private int _presentQueued;
    private int _screenW = 1;
    private int _screenH = 1;
    private byte[]? _pixels;
    private int _pixelW;
    private int _pixelH;
    private readonly WriteableBitmap?[] _bitmaps = new WriteableBitmap?[2];
    private int _bitmapIndex;
    private H264Decoder? _video;
    private readonly DispatcherTimer _cursorTimer;
    private DateTime _steerUntil;
    private bool _pointerOver;
    private bool _fileDrag;
    private int _cursorX;
    private int _cursorY;
    private int _cursorHotX;
    private int _cursorHotY;
    private int _cursorPixelW = 32;
    private int _cursorPixelH = 32;
    private bool _cursorVisible;
    private bool _haveCursor;

    public RemoteWindow(ConversationViewModel conversation, Action? openFiles = null)
    {
        _conversation = conversation;
        _openFiles = openFiles;
        InitializeComponent();
        RenderOptions.SetBitmapScalingMode(ScreenImage, BitmapScalingMode.Linear);
        Focusable = true;
        QualityBox.ItemsSource = ScreenShareService.Presets;
        QualityBox.SelectedIndex = 2;
        ChatList.ItemsSource = conversation.Transcript;
        _conversation.BeginClipboardSync();
        conversation.Changed += () => Dispatcher.BeginInvoke(() =>
        {
            if (!ReferenceEquals(ChatList.ItemsSource, conversation.Transcript))
                ChatList.ItemsSource = conversation.Transcript;
        });
        _cursorTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(80) };
        _cursorTimer.Tick += (_, _) => PlaceCursor();
        _cursorTimer.Start();
        _statsTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _statsTimer.Tick += (_, _) => ShowStats();
        _statsTimer.Start();
        Dispatcher.Hooks.OperationStarted += OperationStarted;
        Dispatcher.Hooks.OperationCompleted += OperationCompleted;
        new Thread(WatchFreezes) { IsBackground = true, Name = "Kdock freeze watch" }.Start();
    }

    private void OperationStarted(object? sender, DispatcherHookEventArgs e) => _opStarted = Environment.TickCount64;

    private void OperationCompleted(object? sender, DispatcherHookEventArgs e)
    {
        long ms = Environment.TickCount64 - _opStarted;
        if (ms <= _slowOpMs)
            return;
        _slowOpMs = ms;
        _slowOp = OperationName?.GetValue(e.Operation) as string ?? "unknown";
    }

    private void WatchFreezes()
    {
        long frozenAt = 0;
        int gen0 = 0, gen1 = 0, gen2 = 0;
        TimeSpan pause = TimeSpan.Zero;
        while (_watching)
        {
            Thread.Sleep(100);
            long now = Environment.TickCount64;
            if (Interlocked.CompareExchange(ref _beatPending, 1, 0) == 0)
                Dispatcher.BeginInvoke(() =>
                {
                    Volatile.Write(ref _uiBeat, Environment.TickCount64);
                    Volatile.Write(ref _beatPending, 0);
                }, DispatcherPriority.Input);
            long silent = now - Volatile.Read(ref _uiBeat);
            if (frozenAt == 0 && silent > 700)
            {
                frozenAt = Volatile.Read(ref _uiBeat);
                gen0 = GC.CollectionCount(0);
                gen1 = GC.CollectionCount(1);
                gen2 = GC.CollectionCount(2);
                pause = GC.GetTotalPauseDuration();
                _slowOpMs = 0;
                _slowOp = string.Empty;
            }
            else if (frozenAt != 0 && silent < 300)
            {
                long lasted = Volatile.Read(ref _uiBeat) - frozenAt;
                long pieceGap = now - Volatile.Read(ref _lastPieceTick);
                string line = $"{DateTime.Now:HH:mm:ss} FREEZE window did not respond for {lasted} ms · slowest task {(_slowOp.Length > 0 ? _slowOp : "none (window message or input)")} {_slowOpMs} ms · " +
                    $"GC gen0 +{GC.CollectionCount(0) - gen0} gen1 +{GC.CollectionCount(1) - gen1} gen2 +{GC.CollectionCount(2) - gen2} pause {(GC.GetTotalPauseDuration() - pause).TotalMilliseconds:0} ms · " +
                    $"memory {GC.GetTotalMemory(false) / (1024 * 1024)} MB · pictures arrived during freeze {(pieceGap < lasted ? "yes" : "no")}";
                try
                {
                    File.AppendAllText(Path.Combine(App.State.DataDirectory, "view.log"), line + Environment.NewLine);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
                frozenAt = 0;
            }
        }
    }

    private void ShowStats()
    {
        int keys = Interlocked.Exchange(ref _inKeys, 0);
        int regions = Interlocked.Exchange(ref _inRegions, 0);
        int other = Interlocked.Exchange(ref _inOther, 0);
        long bytes = Interlocked.Exchange(ref _inBytes, 0);
        int failed = Interlocked.Exchange(ref _failed, 0);
        int applied = Interlocked.Exchange(ref _applied, 0);
        int presented = _presented;
        _presented = 0;
        int queued;
        lock (_frameLock)
            queued = _pieces.Count;
        long age = _lastPresentTick == 0 ? -1 : (Environment.TickCount64 - _lastPresentTick) / 1000;
        string text = $"in 2s: full {keys} · parts {regions} · other {other} · {bytes / 2048} KB/s · applied {applied} · failed {failed} · drawn {presented} · waiting {queued} · picture {_pixelW}x{_pixelH} · last drawn {(age < 0 ? "never" : age + "s ago")}";
        if (failed > 0 && _lastFailure.Length > 0)
            text += " · " + _lastFailure;
        if (keys + regions + other + failed > 0 || age > 2)
        {
            try
            {
                File.AppendAllText(Path.Combine(App.State.DataDirectory, "view.log"), $"{DateTime.Now:HH:mm:ss} {text}{Environment.NewLine}");
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }

    public void ShowFrame(byte[] data, int width, int height)
    {
        if (width > 0 && height > 0)
        {
            _screenW = width;
            _screenH = height;
        }
        Enqueue(new FramePiece(data, keyframe: true));
    }

    public void ShowDelta(byte[] data) => Enqueue(new FramePiece(data, keyframe: false));

    public void ShowRegion(byte[] jpeg, int x, int y, int width, int height) => Enqueue(new FramePiece(jpeg, x, y, width, height));

    public void ShowCursor(int x, int y, bool visible, int hotX, int hotY, byte[]? png)
    {
        _cursorX = x;
        _cursorY = y;
        _cursorHotX = hotX;
        _cursorHotY = hotY;
        _cursorVisible = visible;
        if (png is not { Length: > 32 })
            return;
        byte[] copy = png;
        Dispatcher.BeginInvoke(() =>
        {
            BitmapImage image = new();
            using MemoryStream stream = new(copy);
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();
            RemoteCursor.Source = image;
            _cursorPixelW = Math.Max(1, image.PixelWidth);
            _cursorPixelH = Math.Max(1, image.PixelHeight);
            _haveCursor = true;
            PlaceCursor();
        }, DispatcherPriority.Background);
    }

    public void ShowVideo(byte[] data, int width, int height)
    {
        if (width > 0 && height > 0)
        {
            _screenW = width;
            _screenH = height;
        }
        Enqueue(new FramePiece(data, video: true));
    }

    private void Enqueue(FramePiece piece)
    {
        if (piece.Keyframe)
            Interlocked.Increment(ref _inKeys);
        else if (piece.Region)
            Interlocked.Increment(ref _inRegions);
        else
            Interlocked.Increment(ref _inOther);
        Interlocked.Add(ref _inBytes, piece.Data.Length);
        Volatile.Write(ref _lastPieceTick, Environment.TickCount64);
        lock (_frameLock)
        {
            if (piece.Keyframe)
                _pieces.Clear();
            _pieces.Enqueue(piece);
        }
        if (Interlocked.CompareExchange(ref _frameBusy, 1, 0) != 0)
            return;
        ThreadPool.QueueUserWorkItem(_ => PaintFrames());
    }

    private void PaintFrames()
    {
        try
        {
            int paintW = 0;
            int paintH = 0;
            int screenW = 1;
            int screenH = 1;
            bool fullPaint = false;
            int dirtyX = 0;
            int dirtyY = 0;
            int dirtyW = 0;
            int dirtyH = 0;
            bool haveDirty = false;
            while (true)
            {
                while (true)
                {
                    FramePiece piece;
                    lock (_frameLock)
                    {
                        if (_pieces.Count == 0)
                            break;
                        piece = _pieces.Dequeue();
                    }
                    ApplyPiece(piece, ref fullPaint, ref haveDirty, ref dirtyX, ref dirtyY, ref dirtyW, ref dirtyH, ref paintW, ref paintH, ref screenW, ref screenH);
                }
                if (_pixels == null || paintW < 2)
                {
                    lock (_frameLock)
                    {
                        if (_pieces.Count == 0)
                        {
                            _frameBusy = 0;
                            return;
                        }
                    }
                    continue;
                }
                int rowBytes = paintW * 3;
                int rowStride = (rowBytes + 3) & ~3;
                byte[]? shown = Interlocked.Exchange(ref _spareBuffer, null);
                if (shown == null || shown.Length != rowStride * paintH)
                    shown = new byte[rowStride * paintH];
                for (int row = 0; row < paintH; row++)
                    Buffer.BlockCopy(_pixels, row * paintW * 3, shown, row * rowStride, rowBytes);
                PostFrame(shown, rowStride, paintW, paintH, screenW, screenH);
                lock (_frameLock)
                {
                    if (_pieces.Count > 0)
                    {
                        fullPaint = false;
                        haveDirty = false;
                        continue;
                    }
                    _frameBusy = 0;
                    if (_pieces.Count == 0)
                        return;
                    if (Interlocked.CompareExchange(ref _frameBusy, 1, 0) != 0)
                        return;
                    fullPaint = false;
                    haveDirty = false;
                }
            }
        }
        catch (Exception ex)
        {
            Fail("drawing crashed: " + ex.GetType().Name + " " + ex.Message);
            lock (_frameLock)
                _frameBusy = 0;
        }
    }

    private void PostFrame(byte[] shown, int stride, int shownW, int shownH, int screenW, int screenH)
    {
        lock (_presentLock)
        {
            if (_presentBuffer != null)
                Volatile.Write(ref _spareBuffer, _presentBuffer);
            _presentBuffer = shown;
            _presentStride = stride;
            _presentW = shownW;
            _presentH = shownH;
            _presentScreenW = screenW;
            _presentScreenH = screenH;
            _presentGeneration++;
        }
        if (Interlocked.CompareExchange(ref _presentQueued, 1, 0) == 0)
            Dispatcher.BeginInvoke(FlushPresent, DispatcherPriority.Render);
    }

    private void FlushPresent()
    {
        byte[]? buffer;
        int stride;
        int shownW;
        int shownH;
        int screenW;
        int screenH;
        int generation;
        lock (_presentLock)
        {
            buffer = _presentBuffer;
            _presentBuffer = null;
            stride = _presentStride;
            shownW = _presentW;
            shownH = _presentH;
            screenW = _presentScreenW;
            screenH = _presentScreenH;
            generation = _presentGeneration;
        }
        if (buffer != null && shownW > 1 && shownH > 1)
        {
            _sourceWidth = screenW > 1 ? screenW : shownW;
            _sourceHeight = screenH > 1 ? screenH : shownH;
            _bitmapIndex ^= 1;
            WriteableBitmap? bitmap = _bitmaps[_bitmapIndex];
            if (bitmap == null || bitmap.PixelWidth != shownW || bitmap.PixelHeight != shownH)
                _bitmaps[_bitmapIndex] = bitmap = new WriteableBitmap(shownW, shownH, 96, 96, PixelFormats.Bgr24, null);
            bitmap.WritePixels(new System.Windows.Int32Rect(0, 0, shownW, shownH), buffer, stride, 0);
            ScreenImage.Source = bitmap;
            _presented++;
            _lastPresentTick = Environment.TickCount64;
            PlaceCursor();
        }
        if (buffer != null)
            Volatile.Write(ref _spareBuffer, buffer);
        Interlocked.Exchange(ref _presentQueued, 0);
        bool newer;
        lock (_presentLock)
            newer = _presentGeneration != generation;
        if (newer && Interlocked.CompareExchange(ref _presentQueued, 1, 0) == 0)
            Dispatcher.BeginInvoke(FlushPresent, DispatcherPriority.Render);
    }

    private void ApplyPiece(FramePiece piece, ref bool fullPaint, ref bool haveDirty, ref int dirtyX, ref int dirtyY, ref int dirtyW, ref int dirtyH, ref int paintW, ref int paintH, ref int screenW, ref int screenH)
    {
        if (piece.Video)
        {
            _video ??= new H264Decoder();
            if (!_video.TryDecode(piece.Data, out byte[] decoded, out int width, out int height))
            {
                _video.Dispose();
                _video = null;
                return;
            }
            _pixels = decoded;
            _pixelW = width;
            _pixelH = height;
            fullPaint = true;
        }
        else if (piece.Keyframe)
        {
            _video?.Dispose();
            _video = null;
            if (!ScreenFrames.TryDecodeJpeg(piece.Data, out byte[] decoded, out int width, out int height))
            {
                Fail($"full picture would not decode ({piece.Data.Length} bytes)");
                return;
            }
            _pixels = decoded;
            _pixelW = width;
            _pixelH = height;
            fullPaint = true;
            Interlocked.Increment(ref _applied);
        }
        else if (piece.Region && _pixels == null)
        {
            Fail("part arrived before any full picture");
            return;
        }
        else if (piece.Region && _pixels != null)
        {
            if (!ScreenFrames.TryApplyRegion(piece.Data, _pixels, _pixelW, _pixelH, piece.X, piece.Y, piece.W, piece.H))
            {
                Fail($"part {piece.X},{piece.Y} {piece.W}x{piece.H} did not fit picture {_pixelW}x{_pixelH} ({piece.Data.Length} bytes)");
                return;
            }
            Interlocked.Increment(ref _applied);
            if (!fullPaint)
            {
                if (!haveDirty)
                {
                    dirtyX = piece.X;
                    dirtyY = piece.Y;
                    dirtyW = piece.W;
                    dirtyH = piece.H;
                    haveDirty = true;
                }
                else
                {
                    int right = Math.Max(dirtyX + dirtyW, piece.X + piece.W);
                    int bottom = Math.Max(dirtyY + dirtyH, piece.Y + piece.H);
                    dirtyX = Math.Min(dirtyX, piece.X);
                    dirtyY = Math.Min(dirtyY, piece.Y);
                    dirtyW = right - dirtyX;
                    dirtyH = bottom - dirtyY;
                }
            }
        }
        else if (_pixels != null)
        {
            ScreenFrames.TryApplyDelta(piece.Data, _pixels, _pixelW, _pixelH);
            fullPaint = true;
        }
        if (_pixels == null)
            return;
        paintW = _pixelW;
        paintH = _pixelH;
        screenW = _screenW;
        screenH = _screenH;
    }

    private void Fail(string reason)
    {
        Interlocked.Increment(ref _failed);
        _lastFailure = reason;
    }

    private readonly struct FramePiece
    {
        public FramePiece(byte[] data, bool keyframe = false, bool video = false)
        {
            Data = data;
            Keyframe = keyframe;
            Video = video;
        }

        public FramePiece(byte[] jpeg, int x, int y, int width, int height)
        {
            Data = jpeg;
            Region = true;
            X = x;
            Y = y;
            W = width;
            H = height;
        }

        public byte[] Data { get; }
        public bool Keyframe { get; }
        public bool Video { get; }
        public bool Region { get; }
        public int X { get; }
        public int Y { get; }
        public int W { get; }
        public int H { get; }
    }

    private void QualityChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (!IsLoaded || QualityBox.SelectedItem is not ScreenSharePreset preset)
            return;
        _ = _conversation.SendControlAsync(
            $"share:{preset.Quality},{preset.DelayMs},{preset.MaxWidth},{(preset.Smooth ? 1 : 0)},{(preset.SkipStill ? 1 : 0)}");
    }

    private async void StopClick(object sender, RoutedEventArgs e)
    {
        await _conversation.StopScreenShareAsync();
        if (IsLoaded)
            Close();
    }

    private void FilesClick(object sender, RoutedEventArgs e) => _openFiles?.Invoke();

    private void ChatClick(object sender, RoutedEventArgs e)
    {
        bool open = ChatColumn.Width.Value > 0;
        ChatColumn.Width = open ? new GridLength(0) : new GridLength(300);
        if (!open)
            ChatBox.Focus();
    }

    private async void ChatSendClick(object sender, RoutedEventArgs e)
    {
        string text = ChatBox.Text.Trim();
        if (text.Length == 0)
            return;
        ChatBox.Clear();
        await _conversation.SendTextAsync(text);
    }

    private void FullClick(object sender, RoutedEventArgs e)
    {
        _full = !_full;
        if (_full)
        {
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            FullButton.Content = "Exit full screen";
            if (!_pinned)
                CommandBar.Visibility = Visibility.Collapsed;
        }
        else
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = WindowState.Normal;
            FullButton.Content = "Full screen";
            CommandBar.Visibility = Visibility.Visible;
        }
    }

    private void PinClick(object sender, RoutedEventArgs e)
    {
        _pinned = !_pinned;
        PinGlyph.Text = _pinned ? "\uE77A" : "\uE718";
        PinButton.ToolTip = _pinned ? "Unpin toolbar" : "Pin toolbar";
        if (_pinned)
            CommandBar.Visibility = Visibility.Visible;
        else if (_full)
            CommandBar.Visibility = Visibility.Collapsed;
    }

    private void UpdateBar(System.Windows.Point point)
    {
        if (!_full || _pinned)
            return;
        bool show = point.Y < 56 && Math.Abs(point.X - PictureHost.ActualWidth / 2) < 240;
        CommandBar.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
    }

    private void PictureMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        NoteSteering();
        UpdateBar(e.GetPosition(PictureHost));
        if (!TryMap(e.GetPosition(PictureHost), out int x, out int y))
            return;
        System.Windows.Point point = new(x, y);
        if (_hasSent && point == _lastSent)
            return;
        if ((DateTime.UtcNow - _lastMove).TotalMilliseconds < 50 && _hasSent)
            return;
        _lastSent = point;
        _hasSent = true;
        _lastMove = DateTime.UtcNow;
        _ = _conversation.SendControlAsync($"M,{x},{y},mv");
    }

    private void PictureMouseDown(object sender, MouseButtonEventArgs e)
    {
        NoteSteering();
        Focus();
        if (!TryMap(e.GetPosition(PictureHost), out int x, out int y))
            return;
        string action = e.ChangedButton switch
        {
            MouseButton.Right => "rd",
            MouseButton.Middle => "md",
            _ => e.ClickCount > 1 ? "dbl" : "ld"
        };
        _ = _conversation.SendControlAsync($"M,{x},{y},{action}");
        e.Handled = true;
    }

    private void PictureMouseUp(object sender, MouseButtonEventArgs e)
    {
        _fileDrag = false;
        NoteSteering();
        if (!TryMap(e.GetPosition(PictureHost), out int x, out int y))
            return;
        string action = e.ChangedButton switch
        {
            MouseButton.Right => "ru",
            MouseButton.Middle => "mu",
            _ => "lu"
        };
        if (e.ChangedButton != MouseButton.Left || e.ClickCount < 2)
            _ = _conversation.SendControlAsync($"M,{x},{y},{action}");
    }

    private void PictureWheel(object sender, MouseWheelEventArgs e)
    {
        NoteSteering();
        if (!TryMap(e.GetPosition(PictureHost), out int x, out int y))
            return;
        _ = _conversation.SendControlAsync($"M,{x},{y},wh,{e.Delta}");
    }

    private async void WindowKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (Keyboard.FocusedElement is System.Windows.Controls.TextBox)
            return;
        int key = KeyInterop.VirtualKeyFromKey(e.Key == Key.System ? e.SystemKey : e.Key);
        if (key <= 0)
            return;
        bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
        if (ctrl && key == 0x56)
            await _conversation.FlushClipboardAsync();
        await _conversation.SendControlAsync($"K,{key},down");
        e.Handled = true;
    }

    private async void WindowKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (Keyboard.FocusedElement is System.Windows.Controls.TextBox)
            return;
        int key = KeyInterop.VirtualKeyFromKey(e.Key == Key.System ? e.SystemKey : e.Key);
        if (key <= 0)
            return;
        bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
        await _conversation.SendControlAsync($"K,{key},up");
        if (ctrl && key is 0x43 or 0x58)
        {
            await Task.Delay(120);
            await _conversation.RequestPeerClipboardAsync();
        }
        e.Handled = true;
    }

    private void OpenAttachmentClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement element || element.DataContext is not ConversationMessage message || !message.CanOpen)
            return;
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(message.SavedPath) { UseShellExecute = true });
        e.Handled = true;
    }

    private void PictureMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        _pointerOver = true;
        _fileDrag = false;
        NoteSteering();
    }

    private void PictureMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        _pointerOver = false;
        if (Mouse.LeftButton == MouseButtonState.Pressed || Mouse.RightButton == MouseButtonState.Pressed)
            _fileDrag = true;
        PlaceCursor();
    }

    private void PictureSizeChanged(object sender, SizeChangedEventArgs e) => PlaceCursor();

    private void NoteSteering()
    {
        _pointerOver = true;
        _steerUntil = DateTime.UtcNow.AddMilliseconds(450);
        PlaceCursor();
    }

    private bool HostMovedOnTheirOwn()
    {
        if (!_hasSent)
            return true;
        int dx = _cursorX - (int)_lastSent.X;
        int dy = _cursorY - (int)_lastSent.Y;
        return dx * dx + dy * dy >= 48 * 48;
    }

    private void PlaceCursor()
    {
        bool viewerDriving = _pointerOver || _fileDrag || DateTime.UtcNow < _steerUntil || !HostMovedOnTheirOwn();
        bool showRemote = !viewerDriving && _cursorVisible && _haveCursor && _sourceWidth > 1;
        if (!showRemote || !TryLetterbox(out double left, out double top, out double scale))
        {
            RemoteCursor.Visibility = Visibility.Collapsed;
            PictureHost.Cursor = System.Windows.Input.Cursors.Arrow;
            return;
        }
        RemoteCursor.Width = Math.Max(1, _cursorPixelW * scale);
        RemoteCursor.Height = Math.Max(1, _cursorPixelH * scale);
        RemoteCursor.Margin = new Thickness(left + (_cursorX - _cursorHotX) * scale, top + (_cursorY - _cursorHotY) * scale, 0, 0);
        RemoteCursor.Visibility = Visibility.Visible;
        PictureHost.Cursor = System.Windows.Input.Cursors.None;
    }

    private bool TryLetterbox(out double left, out double top, out double scale)
    {
        left = 0;
        top = 0;
        scale = 0;
        double boxWidth = PictureHost.ActualWidth;
        double boxHeight = PictureHost.ActualHeight;
        if (boxWidth <= 1 || boxHeight <= 1 || _sourceWidth <= 0 || _sourceHeight <= 0)
            return false;
        scale = Math.Min(boxWidth / _sourceWidth, boxHeight / _sourceHeight);
        double shownWidth = _sourceWidth * scale;
        double shownHeight = _sourceHeight * scale;
        left = (boxWidth - shownWidth) / 2;
        top = (boxHeight - shownHeight) / 2;
        return true;
    }

    private bool TryMap(System.Windows.Point local, out int x, out int y)
    {
        x = 0;
        y = 0;
        double boxWidth = PictureHost.ActualWidth;
        double boxHeight = PictureHost.ActualHeight;
        if (boxWidth <= 1 || boxHeight <= 1 || _sourceWidth <= 0 || _sourceHeight <= 0)
            return false;
        double scale = Math.Min(boxWidth / _sourceWidth, boxHeight / _sourceHeight);
        double shownWidth = _sourceWidth * scale;
        double shownHeight = _sourceHeight * scale;
        double left = (boxWidth - shownWidth) / 2;
        double top = (boxHeight - shownHeight) / 2;
        if (local.X < left || local.Y < top || local.X > left + shownWidth || local.Y > top + shownHeight)
            return false;
        x = (int)((local.X - left) * _sourceWidth / shownWidth);
        y = (int)((local.Y - top) * _sourceHeight / shownHeight);
        return true;
    }

    protected override void OnClosed(EventArgs e)
    {
        _cursorTimer.Stop();
        _statsTimer.Stop();
        _watching = false;
        Dispatcher.Hooks.OperationStarted -= OperationStarted;
        Dispatcher.Hooks.OperationCompleted -= OperationCompleted;
        _conversation.EndClipboardSync();
        _video?.Dispose();
        base.OnClosed(e);
    }
}
