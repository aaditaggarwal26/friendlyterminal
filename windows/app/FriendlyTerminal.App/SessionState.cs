using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FriendlyTerminal.App.Models;

namespace FriendlyTerminal.App;

/// <summary>
/// Shared state for the sidebar views: the current working directory and the file
/// listing derived from it. The host (MainWindow) drives <see cref="SetCurrentDirectory"/>
/// from the parsed OSC cwd, marshaled onto the UI thread, and wires <see cref="SendToShell"/>.
/// </summary>
public sealed class SessionState : INotifyPropertyChanged
{
    private string _currentDirectory = "";
    private bool _showHidden;

    public ObservableCollection<FileEntry> Files { get; } = new();

    public string CurrentDirectory
    {
        get => _currentDirectory;
        private set => SetField(ref _currentDirectory, value);
    }

    public bool ShowHidden
    {
        get => _showHidden;
        set
        {
            if (SetField(ref _showHidden, value))
                RefreshFiles();
        }
    }

    /// <summary>Set by the host; sends text into the shell PTY (the caller adds the newline).</summary>
    public Action<string>? SendToShell { get; set; }

    public int ItemCount => Files.Count;

    public void SetCurrentDirectory(string path)
    {
        if (!string.Equals(path, _currentDirectory, StringComparison.OrdinalIgnoreCase))
            CurrentDirectory = path;
        // Refresh either way: contents can change without the directory changing.
        RefreshFiles();
    }

    public void RefreshFiles()
    {
        Files.Clear();
        foreach (var entry in FileEntry.List(_currentDirectory))
            if (_showHidden || !entry.IsHidden)
                Files.Add(entry);
        OnPropertyChanged(nameof(ItemCount));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }
}
