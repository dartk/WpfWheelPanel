using System.Windows;
using System.Windows.Controls;


namespace WpfWheelPanel.Controls;


public class WheelContextMenu : ContextMenu
{
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        // aligning center of the menu with the mouse pointer
        if (e.Property == ActualWidthProperty)
        {
            this.HorizontalOffset = -this.ActualWidth / 2;
        }
        else if (e.Property == ActualHeightProperty)
        {
            this.VerticalOffset = -this.ActualHeight / 2;
        }

        base.OnPropertyChanged(e);
    }
}