using FriendlyTerminal.Core.Help;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FriendlyTerminal.App.Views;

public sealed partial class CommandHelpView : UserControl
{
    private SessionState? _session;
    private CommandCategory? _selected;
    private string _search = "";

    // Category icon-key -> Segoe Fluent Icons code point (verified against Microsoft Learn).
    private static readonly Dictionary<string, int> CategoryGlyphs = new()
    {
        ["folder"] = 0xE8B7,
        ["file"] = 0xE8A5,
        ["github"] = 0xE943,
        ["ai"] = 0xE99A,
        ["search"] = 0xE721,
        ["system"] = 0xE977,
        ["network"] = 0xE968,
        ["lock"] = 0xE72E,
        ["cpu"] = 0xEEA1,
        ["archive"] = 0xF012,
        ["text"] = 0xE8C1,
        ["edit"] = 0xE70F,
        ["package"] = 0xE7B8,
        ["cube"] = 0xE7B8,
        ["python"] = 0xEC7A,
        ["node"] = 0xEC7A,
        ["store"] = 0xE7BF,
        ["docker"] = 0xE950,
        ["env"] = 0xE713,
        ["remote"] = 0xE8AF,
        ["disk"] = 0xEDA2,
        ["more"] = 0xE712,
    };

    public SessionState? Session
    {
        get => _session;
        set => _session = value;
    }

    public CommandHelpView()
    {
        InitializeComponent();
        Render();
    }

    private static string GlyphOf(string key) =>
        CategoryGlyphs.TryGetValue(key, out var code) ? char.ConvertFromUtf32(code) : char.ConvertFromUtf32(0xE7C3);

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        _search = SearchBox.Text;
        Render();
    }

    private void OnBack(object sender, RoutedEventArgs e)
    {
        _search = "";
        _selected = null;
        SearchBox.Text = "";
        Render();
    }

    private void OnCategoryClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is CategoryTile tile)
        {
            _selected = tile.Category;
            Render();
        }
    }

    private void OnCommandClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not CommandRow row || _session is null) return;

        // Prefill the prompt line without running it, matching the macOS command-bar
        // behavior: the text lands on the PowerShell prompt for editing before Enter.
        // Dangerous commands are flagged with a warning icon in the list, not run.
        _session.SendToShell?.Invoke(row.Command);
    }

    private void Render()
    {
        var drilled = _selected is not null;
        var searching = !drilled && !string.IsNullOrWhiteSpace(_search);

        SearchBox.Visibility = drilled ? Visibility.Collapsed : Visibility.Visible;
        BackButton.Visibility = (drilled || searching) ? Visibility.Visible : Visibility.Collapsed;
        TitleText.Text = drilled ? _selected!.Name : (searching ? "Search" : "Help with commands");

        if (drilled)
        {
            CategoryGrid.Visibility = Visibility.Collapsed;
            CommandList.Visibility = Visibility.Visible;
            CommandList.ItemsSource = _selected!.Commands
                .Select(i => new CommandRow(i.Command, i.Detail, null, i.IsDangerous))
                .ToList();
        }
        else if (searching)
        {
            CategoryGrid.Visibility = Visibility.Collapsed;
            CommandList.Visibility = Visibility.Visible;
            CommandList.ItemsSource = CommandSearch.Search(CommandCatalog.All, _search)
                .Select(h => new CommandRow(h.Item.Command, h.Item.Detail, h.Category.Name, h.Item.IsDangerous))
                .ToList();
        }
        else
        {
            CategoryGrid.Visibility = Visibility.Visible;
            CommandList.Visibility = Visibility.Collapsed;
            CategoryGrid.ItemsSource = CommandCatalog.All
                .Where(c => CommandCatalog.DefaultEnabledIds.Contains(c.Id))
                .Select(c => new CategoryTile(c.Name, GlyphOf(c.Icon), c))
                .ToList();
        }
    }
}

internal sealed record CategoryTile(string Name, string Glyph, CommandCategory Category);

internal sealed record CommandRow(string Command, string Detail, string? Badge, bool IsDangerous)
{
    public Visibility DangerVisibility => IsDangerous ? Visibility.Visible : Visibility.Collapsed;
    public Visibility BadgeVisibility => string.IsNullOrEmpty(Badge) ? Visibility.Collapsed : Visibility.Visible;
}
