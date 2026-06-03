namespace Jellyfin.MediaEncoding.Hls.Playlist;

/// <summary>
/// Represents a master playlist variant stream.
/// </summary>
public sealed record MasterPlaylistVariant(string StreamInfo, string Url);
