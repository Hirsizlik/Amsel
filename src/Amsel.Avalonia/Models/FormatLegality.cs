namespace Amsel.Avalonia.Models;

public record FormatLegality(string FormatName, bool Banned, string? Comment)
{
    public bool HasComment { get
        {
            return Comment != null;
        }
    }
}
