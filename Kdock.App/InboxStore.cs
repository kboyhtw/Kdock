using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Kdock.App;

public sealed class InboxJob
{
    public string Id { get; set; } = string.Empty;
    public string PeerId { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long Length { get; set; }
    public long Offset { get; set; }
}

public sealed class InboxArrival
{
    public string Name { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime Utc { get; set; } = DateTime.UtcNow;
    public string WhenText => Utc.ToLocalTime().ToString("g");
    public string SizeText => Size < 1024 ? Size + " B" : Size < 1024 * 1024 ? $"{Size / 1024.0:0.0} KB" : $"{Size / 1024.0 / 1024.0:0.0} MB";
}

public sealed class InboxStore
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    private readonly string _jobsPath;
    private readonly string _historyPath;
    private readonly string _namesPath;
    private readonly object _gate = new();
    private List<InboxJob> _jobs;
    private List<InboxArrival> _history;
    private Dictionary<string, string> _names;

    public InboxStore(string dataDirectory)
    {
        Directory.CreateDirectory(dataDirectory);
        _jobsPath = Path.Combine(dataDirectory, "inbox-jobs.json");
        _historyPath = Path.Combine(dataDirectory, "inbox-history.json");
        _namesPath = Path.Combine(dataDirectory, "names.json");
        _jobs = LoadList<InboxJob>(_jobsPath);
        _history = LoadList<InboxArrival>(_historyPath);
        _names = LoadNames();
        Arrivals = new ObservableCollection<InboxArrival>(_history.TakeLast(50).Reverse());
    }

    public ObservableCollection<InboxArrival> Arrivals { get; }
    public IReadOnlyList<InboxJob> Jobs
    {
        get { lock (_gate) return _jobs.ToList(); }
    }

    public string NameFor(string deskId)
    {
        string digits = Digits(deskId);
        lock (_gate)
        {
            if (_names.TryGetValue(digits, out string? name) && !string.IsNullOrWhiteSpace(name))
                return name;
        }
        return digits.Length == 10 ? AppState.FormatDeskId(digits) : deskId;
    }

    public void SetName(string deskId, string name)
    {
        string digits = Digits(deskId);
        if (digits.Length == 0 || string.IsNullOrWhiteSpace(name))
            return;
        lock (_gate)
        {
            _names[digits] = name.Trim();
            Save(_namesPath, _names);
        }
    }

    public InboxJob AddJob(string peerId, string path)
    {
        var info = new FileInfo(path);
        InboxJob job = new()
        {
            Id = Guid.NewGuid().ToString("N"),
            PeerId = peerId,
            Path = path,
            Name = info.Name,
            Length = info.Length,
            Offset = 0
        };
        lock (_gate)
        {
            InboxJob? existing = _jobs.FirstOrDefault(item =>
                item.PeerId.Equals(job.PeerId, StringComparison.OrdinalIgnoreCase)
                && item.Path.Equals(path, StringComparison.OrdinalIgnoreCase)
                && item.Length == job.Length);
            if (existing != null)
                return existing;
            _jobs.Add(job);
            Save(_jobsPath, _jobs);
            return job;
        }
    }

    public void UpdateJob(string id, long offset)
    {
        lock (_gate)
        {
            InboxJob? job = _jobs.FirstOrDefault(item => item.Id == id);
            if (job == null)
                return;
            job.Offset = offset;
            Save(_jobsPath, _jobs);
        }
    }

    public void RemoveJob(string id)
    {
        lock (_gate)
        {
            _jobs.RemoveAll(item => item.Id == id);
            Save(_jobsPath, _jobs);
        }
    }

    public void RememberArrival(InboxArrival item)
    {
        lock (_gate)
        {
            _history.Add(item);
            if (_history.Count > 80)
                _history = _history.TakeLast(80).ToList();
            Save(_historyPath, _history);
        }
        Arrivals.Insert(0, item);
        while (Arrivals.Count > 50)
            Arrivals.RemoveAt(Arrivals.Count - 1);
    }

    private Dictionary<string, string> LoadNames()
    {
        try
        {
            if (File.Exists(_namesPath))
                return JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(_namesPath), Json)
                    ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
        }
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    private static List<T> LoadList<T>(string path)
    {
        try
        {
            if (File.Exists(path))
                return JsonSerializer.Deserialize<List<T>>(File.ReadAllText(path), Json) ?? [];
        }
        catch (JsonException)
        {
        }
        return [];
    }

    private static void Save<T>(string path, T value)
    {
        string temp = path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(value, Json));
        File.Move(temp, path, true);
    }

    private static string Digits(string value) => new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
}
