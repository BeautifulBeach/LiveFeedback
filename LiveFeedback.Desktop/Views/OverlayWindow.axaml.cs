using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace LiveFeedback.Views;

public partial class OverlayWindow : Window
{
    public OverlayWindow()
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.Manual;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void PointerPressed_DragWindow(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }
}