using System;

namespace Kdock.App;

public sealed class PermissionRequest
{
    public string Id { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string Description => Kind switch
    {
        "files" => "Browse your files",
        "watch" => "Watch your screen",
        "share" => "Look at my screen",
        _ => Kind
    };
    public string Result { get; set; } = "Pending";
    public bool IsPending => Result == "Pending";
    public IWireSession? ReplySession { get; set; }
    public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;
}
