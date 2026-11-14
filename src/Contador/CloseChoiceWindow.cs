using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Contador;

public sealed class CloseChoiceWindow : Window
{
    public WindowCloseChoice Choice { get; private set; } = WindowCloseChoice.Cancel;

    public CloseChoiceWindow(Window owner, bool backgroundAvailable)
    {
        Owner = owner;
        Style = (Style)Application.Current.Resources[typeof(Window)];
        Title = "Cerrar Contador";
        Width = 500;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var content = new StackPanel { Margin = new Thickness(24) };
        content.Children.Add(new TextBlock
        {
            Text = "¿Qué quieres hacer?", FontSize = 22, FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 12)
        });
        content.Children.Add(new TextBlock
        {
            Text = backgroundAvailable
                ? "En segundo plano, Contador seguirá contando y el overlay continuará activo. Puedes volver desde el icono junto al reloj de Windows."
                : "Windows no pudo mostrar el icono de la bandeja. Puedes cancelar o salir; la ventana no se ocultará sin un icono disponible.",
            TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 20)
        });
        var background = AddButton(content, "Mantener en segundo plano", WindowCloseChoice.Background);
        background.IsEnabled = backgroundAvailable;
        background.Background = (Brush)Application.Current.Resources["Gold"];
        background.Foreground = (Brush)Application.Current.Resources["Ground"];
        AddButton(content, "Salir de Contador", WindowCloseChoice.Exit);
        var cancel = AddButton(content, "Cancelar", WindowCloseChoice.Cancel);
        cancel.IsCancel = true;
        Content = content;
    }

    private Button AddButton(Panel panel, string caption, WindowCloseChoice choice)
    {
        var button = new Button
        {
            Content = caption, Margin = new Thickness(0, 0, 0, 8),
            HorizontalContentAlignment = HorizontalAlignment.Center
        };
        button.Click += (_, _) => { Choice = choice; DialogResult = choice != WindowCloseChoice.Cancel; };
        panel.Children.Add(button);
        return button;
    }
}
