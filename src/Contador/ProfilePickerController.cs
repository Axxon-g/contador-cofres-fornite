using System.Windows.Controls;
using ChestCounter;

namespace Contador;

/// <summary>Account selection remains available while the replay scanner works.</summary>
public sealed class ProfilePickerController
{
    private sealed record Choice(string Key, string Label);
    private readonly ComboBox picker;
    private readonly Func<string, Task> save;
    private List<Choice> choices = [];
    private bool updating;
    private string? pendingKey;

    public ProfilePickerController(ComboBox picker, Func<string, Task> save)
    {
        this.picker = picker;
        this.save = save;
        picker.DisplayMemberPath = nameof(Choice.Label);
        picker.SelectedValuePath = nameof(Choice.Key);
        picker.SelectionChanged += SelectionChanged;
    }

    public void Refresh(TrackerData data, Config config)
    {
        var next = Profiles.List(data).Select(p => new Choice(p.Key, Profiles.Describe(p))).ToList();
        updating = true;
        try
        {
            // Do not replace the list during ordinary refreshes: it can close an open dropdown.
            if (!choices.SequenceEqual(next)) { choices = next; picker.ItemsSource = choices; }
            picker.SelectedValue = pendingKey ?? Profiles.Current(data, config)?.Key;
            picker.IsEnabled = pendingKey == null && choices.Count > 0;
        }
        finally { updating = false; }
    }

    private async void SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (updating || pendingKey != null || picker.SelectedValue is not string key) return;
        pendingKey = key;
        picker.IsEnabled = false;
        try { await save(key); }
        finally
        {
            pendingKey = null;
            picker.IsEnabled = choices.Count > 0;
        }
    }
}
