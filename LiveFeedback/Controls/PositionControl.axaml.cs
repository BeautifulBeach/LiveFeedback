using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LiveFeedback.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LiveFeedback.Controls;

public partial class PositionControl : UserControl
{
    public PositionControl()
    {
        InitializeComponent();
        if (DataContext == null)
        {
            DataContext = Program.Services.GetRequiredService<PositionSelectorViewModel>();
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}