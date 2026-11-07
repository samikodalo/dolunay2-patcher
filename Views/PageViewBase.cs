using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dolunay2Launcher.Views
{
    /// <summary>
    /// Tum sayfa UserControl'leri icin ortak taban sinif.
    /// Tasarimci (designer) icinde Host null olabilir; bu yuzden tum
    /// yardimcilar null kontrolu yapar ve sayfa statik XAML ile gorunur.
    /// </summary>
    public class PageViewBase : UserControl
    {
        internal MainWindow Host;

        protected bool En { get { return Host != null && Host.En; } }
        protected string LangCode { get { return Host != null ? Host.Lang : "tr"; } }
        protected MfManifest Mf { get { return Host != null ? Host.Manifest : null; } }
        protected LocalSettings Settings { get { return Host != null ? Host.Settings : new LocalSettings(); } }

        protected string T(string key) { return Host != null ? Host.Tr(key) : key; }

        protected static Brush B(string hex)
        {
            try { return (Brush)new BrushConverter().ConvertFromString(hex); }
            catch (Exception) { return Brushes.Transparent; }
        }

        protected void OpenUrl(string url) { if (Host != null) Host.Url(url); }
        protected void OpenFolder() { if (Host != null) Host.Folder(); }
        protected string FmtDate(string iso) { return Host != null ? Host.DateFmt(iso) : iso; }

        /// <summary>Sayfa icerigini panelden gelen verilerle yeniler.</summary>
        public virtual void Refresh() { }

        /// <summary>Dil degistiginde cagrilir.</summary>
        public virtual void ApplyLanguage() { Refresh(); }
    }
}
