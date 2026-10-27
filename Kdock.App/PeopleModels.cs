using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace Kdock.App;

public enum Presence
{
    Offline,
    Online,
    InSession
}

public sealed class PersonRecord : INotifyPropertyChanged
{
    private bool _isFavorite;
    private bool _isTrusted;
    private Presence _presence;

    public string DeskId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string LastAddress { get; set; } = string.Empty;
    public bool IsFavorite
    {
        get => _isFavorite;
        set
        {
            if (_isFavorite == value)
                return;
            _isFavorite = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFavorite)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Star)));
        }
    }
    public bool IsTrusted
    {
        get => _isTrusted;
        set
        {
            if (_isTrusted == value)
                return;
            _isTrusted = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTrusted)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TrustLabel)));
        }
    }
    public Presence Presence
    {
        get => _presence;
        set
        {
            if (_presence == value)
                return;
            _presence = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Presence)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PresenceLabel)));
        }
    }
    public DateTime LastSeenUtc { get; set; }
    public string Star => IsFavorite ? "★" : "☆";
    public string TrustLabel => IsTrusted ? "Trusted" : "Code once";
    public string PresenceLabel => Presence switch
    {
        Presence.Online => "Available",
        Presence.InSession => "Connected",
        _ => "Offline"
    };
    public string Initials
    {
        get
        {
            string name = (DisplayName ?? string.Empty).Trim();
            string[] parts = name.Split([' ', '-', ':'], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2 && parts[0].Length > 0 && parts[1].Length > 0)
                return string.Concat(char.ToUpperInvariant(parts[0][0]), char.ToUpperInvariant(parts[1][0]));
            if (name.Length >= 2)
                return name[..2].ToUpperInvariant();
            return name.Length == 1 ? name.ToUpperInvariant() : "?";
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
}

public sealed class PeopleStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly string _favoritesPath;
    private readonly string _recentPath;
    private readonly string _trustedPath;
    private readonly object _gate = new();
    private readonly HashSet<string> _favorites;
    private readonly HashSet<string> _trusted;
    private readonly List<string> _recent;

    public PeopleStore(string dataDirectory)
    {
        _favoritesPath = Path.Combine(dataDirectory, "favorites.json");
        _recentPath = Path.Combine(dataDirectory, "recent.json");
        _trustedPath = Path.Combine(dataDirectory, "trusted.json");
        _favorites = LoadSet(_favoritesPath);
        _trusted = LoadSet(_trustedPath);
        _recent = LoadList(_recentPath);
    }

    public IReadOnlyCollection<string> Favorites => _favorites;
    public IReadOnlyCollection<string> Trusted => _trusted;
    public IReadOnlyList<string> Recent => _recent;

    public void SetFavorite(string deskId, bool value)
    {
        lock (_gate)
        {
            if (value)
                _favorites.Add(deskId);
            else
                _favorites.Remove(deskId);
            SaveSet(_favoritesPath, _favorites);
        }
    }

    public void Remove(string deskId)
    {
        lock (_gate)
        {
            _favorites.Remove(deskId);
            _trusted.Remove(deskId);
            _recent.RemoveAll(item => item.Equals(deskId, StringComparison.OrdinalIgnoreCase));
            SaveSet(_favoritesPath, _favorites);
            SaveSet(_trustedPath, _trusted);
            SaveList(_recentPath, _recent);
        }
    }

    public void AddRecent(string deskId)
    {
        lock (_gate)
        {
            _recent.Remove(deskId);
            _recent.Insert(0, deskId);
            if (_recent.Count > 50)
                _recent.RemoveRange(50, _recent.Count - 50);
            SaveList(_recentPath, _recent);
        }
    }

    public void SetTrusted(string deskId, bool value)
    {
        lock (_gate)
        {
            if (value)
                _trusted.Add(deskId);
            else
                _trusted.Remove(deskId);
            SaveSet(_trustedPath, _trusted);
        }
    }

    private static HashSet<string> LoadSet(string path)
    {
        try
        {
            if (File.Exists(path))
                return new HashSet<string>(JsonSerializer.Deserialize<List<string>>(File.ReadAllText(path), JsonOptions) ?? [], StringComparer.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
        }
        return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    private static List<string> LoadList(string path)
    {
        try
        {
            if (File.Exists(path))
                return JsonSerializer.Deserialize<List<string>>(File.ReadAllText(path), JsonOptions) ?? [];
        }
        catch (JsonException)
        {
        }
        return [];
    }

    private static void SaveSet(string path, IEnumerable<string> values)
    {
        Save(path, values.OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToList());
    }

    private static void SaveList(string path, IEnumerable<string> values)
    {
        Save(path, values.ToList());
    }

    private static void Save<T>(string path, T value)
    {
        string temporaryPath = path + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(value, JsonOptions));
        File.Move(temporaryPath, path, true);
    }
}

public sealed class PeopleViewModel
{
    public PeopleViewModel(AppState state)
    {
        State = state;
        Store = new PeopleStore(state.DataDirectory);
        People = new ObservableCollection<PersonRecord>();
    }

    public AppState State { get; }
    public string DisplayDeskId => State.DisplayDeskId;
    public PeopleStore Store { get; }
    public ObservableCollection<PersonRecord> People { get; }
    public IEnumerable<PersonRecord> Favorites => People.Where(person => person.IsFavorite);
    public IEnumerable<PersonRecord> OnNetwork => People.Where(person => person.LastSeenUtc != default);
    public IEnumerable<PersonRecord> Recent => People.Where(person => Store.Recent.Contains(person.DeskId, StringComparer.OrdinalIgnoreCase));

    public void Upsert(PersonRecord person)
    {
        PersonRecord? existing = People.FirstOrDefault(value => value.DeskId.Equals(person.DeskId, StringComparison.OrdinalIgnoreCase));
        if (existing == null)
        {
            People.Add(person);
            return;
        }

        existing.DisplayName = person.DisplayName;
        existing.LastAddress = person.LastAddress;
        existing.Presence = person.Presence;
        existing.LastSeenUtc = person.LastSeenUtc;
        existing.IsFavorite |= person.IsFavorite;
        existing.IsTrusted |= person.IsTrusted;
    }

    public void Remove(string deskId)
    {
        Store.Remove(deskId);
        PersonRecord? existing = People.FirstOrDefault(value => value.DeskId.Equals(deskId, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
            People.Remove(existing);
    }
}
