using Microsoft.UI.Xaml.Controls;

namespace FriendlyTerminal.App.Views;

public sealed partial class SidebarColumnView : UserControl
{
    private SessionState? _session;

    public SessionState? Session
    {
        get => _session;
        set
        {
            _session = value;
            FilesView.Session = value;
            HelpView.Session = value;
        }
    }

    public SidebarColumnView()
    {
        InitializeComponent();
    }
}
