namespace KivaBelion.API.Data.Entities;

/// <summary>Entities with created/updated times. Both are always UTC and are set automatically on save.</summary>
public interface ITimestamped
{
    DateTimeOffset CreatedAtUtc { get; set; }
    DateTimeOffset UpdatedAtUtc { get; set; }
}
