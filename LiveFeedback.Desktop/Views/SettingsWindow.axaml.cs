using System.Threading.Tasks;
using Avalonia.Controls;
using LiveFeedback.ViewModels;

namespace LiveFeedback.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        Opened += (_, _) =>
        {
            if (DataContext is not SettingsWindowViewModel vm)
            {
                return;
            }

            // TODO: Blocks UI thread, especially noticeable when servers are down.
            Task.Run(vm.UpdateExternalServersUriStatus);
        };
    }
}