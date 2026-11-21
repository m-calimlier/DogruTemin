using System.Windows.Media;

namespace DogruTemin.Wpf.Features.Workspace;
internal sealed partial class WorkspaceModule
{
    internal void MarkDirty()
    {
        if (Context.State.SuppressDirtyTracking)
            return;
        Context.Session.RecordDirty = true;
        ShowDirtyStatus();
        UpdateDerivedState();
    }

    internal void ShowDirtyStatus(string text = "Kaydedilmedi")
    {
        View<System.Windows.Shapes.Ellipse>("SaveStatusDot").Fill = new SolidColorBrush(Color.FromRgb(255, 180, 31));
        View<System.Windows.Controls.TextBlock>("SaveStatusText").Foreground = new SolidColorBrush(Color.FromRgb(255, 230, 183));
        View<System.Windows.Controls.TextBlock>("SaveStatusText").Text = text;
        RefreshArchiveEditState();
    }

    internal void SetSaveErrorStatus()
    {
        View<System.Windows.Shapes.Ellipse>("SaveStatusDot").Fill = new SolidColorBrush(Color.FromRgb(224, 82, 82));
        View<System.Windows.Controls.TextBlock>("SaveStatusText").Foreground = new SolidColorBrush(Color.FromRgb(255, 220, 220));
        View<System.Windows.Controls.TextBlock>("SaveStatusText").Text = "Kayıt başarısız";
    }

    internal void RefreshSaveStatusVisual()
    {
        if (HasUnsavedWork())
        {
            ShowDirtyStatus();
            return;
        }

        ShowSavedStatus();
    }

    internal void ShowSavedStatus()
    {
        View<System.Windows.Shapes.Ellipse>("SaveStatusDot").Fill = new SolidColorBrush(Color.FromRgb(67, 199, 122));
        View<System.Windows.Controls.TextBlock>("SaveStatusText").Foreground = new SolidColorBrush(Color.FromRgb(221, 246, 231));
        View<System.Windows.Controls.TextBlock>("SaveStatusText").Text = Context.Session.LastSavedAt.HasValue ? $"Kaydedildi • {Context.Session.LastSavedAt.Value:HH:mm}" : "Kaydedildi";
        UpdateHomeActivitySummary();
        RefreshArchiveEditState();
    }

    internal void SetSavedStatus(DateTime? savedAt = null)
    {
        Context.Session.MarkSaved(savedAt ?? DateTime.Now);
        ShowSavedStatus();
    }

    internal void RefreshArchiveEditState()
    {
        if (View<System.Windows.Controls.Border>("ArchiveEditBar")is null)
            return;
        var editing = Context.Session.IsEditingArchiveRecord;
        View<System.Windows.Controls.Border>("ArchiveEditBar").Visibility = editing && Context.Shell._currentView is not ("Home" or "Archive" or "Statistics" or "AboutSafety" or "Settings") ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        View<System.Windows.Controls.Button>("ArchiveChangesSaveButton").IsEnabled = editing && HasUnsavedWork();
        View<System.Windows.Controls.TextBlock>("ArchiveEditStatusText").Text = HasUnsavedWork() ? "Arşiv kaydı düzenleniyor • Kaydedilmemiş değişiklikler var." : "Arşiv kaydı açıldı • Değişiklik yaptığınızda kaydedebilirsiniz.";
    }
}
