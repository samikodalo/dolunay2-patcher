using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Dolunay2Launcher
{
    public partial class MainWindow
    {
        string _baseUrl = "https://dolunay2global.com";
        MfManifest _mf;

        async Task InitAsync()
        {
            try
            {
                string ini = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "patcher.ini");
                if (File.Exists(ini))
                {
                    foreach (var line in File.ReadAllLines(ini))
                    {
                        var t = line.Trim();
                        if (t.StartsWith("Server=", StringComparison.OrdinalIgnoreCase))
                            _baseUrl = t.Substring(7).Trim().TrimEnd('/');
                    }
                }

                var wc = new WebClient { Encoding = System.Text.Encoding.UTF8 };
                wc.Headers.Add("User-Agent", "Dolunay2Launcher/1.0");
                string json = await wc.DownloadStringTaskAsync(_baseUrl + "/patcher/manifest.json");
                var ser = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                var m = ser.Deserialize<MfManifest>(json);
                if (m != null)
                {
                    Log.Info("manifest ok v" + m.version);
                    if (await TrySelfUpdate(m)) return;
                    ApplyManifest(m);
                    if (_settings.AutoDownload) _ = CheckFilesAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Info("manifest fail: " + ex.Message);
            }
        }

        void ApplyManifest(MfManifest m)
        {
            _mf = m;
            ApplyRemovals();

            if (!string.IsNullOrEmpty(m.title)) Title = m.title + " Launcher";

            for (int i = 0; i < _slides.Length && i < m.slides.Count; i++)
            {
                var s = m.slides[i];
                _slides[i].TitleTr = s.title_tr;
                _slides[i].TitleEn = s.title_en;
                _slides[i].DescTr = s.desc_tr;
                _slides[i].DescEn = s.desc_en;
                _slides[i].DateTr = s.date;
                _slides[i].DateEn = s.date;
                _slides[i].Image = s.image;
                if (!string.IsNullOrEmpty(s.tag_tr)) _slides[i].TagTr = s.tag_tr;
                if (!string.IsNullOrEmpty(s.tag_en)) _slides[i].TagEn = s.tag_en;
            }

            for (int i = 0; i < _news.Length && i < m.announcements.Count; i++)
            {
                var n = m.announcements[i];
                _news[i].Tag = n.category_tr;
                _news[i].TagEn = n.category_en;
                _news[i].TagColor = n.color;
                _news[i].Date = n.date;
                _news[i].TitleTr = n.title_tr;
                _news[i].TitleEn = n.title_en;
                _news[i].DescTr = n.excerpt_tr;
                _news[i].DescEn = n.excerpt_en;
                _news[i].Image = n.image;
            }

            // Manifestte 4'ten az duyuru varsa eski ornek kartlar gorunmesin.
            for (int i = Math.Min(m.announcements.Count, _news.Length); i < _news.Length; i++)
            {
                _news[i].Tag = "";
                _news[i].TagEn = "";
                _news[i].TagColor = "#2D7FF9";
                _news[i].Date = "";
                _news[i].TitleTr = "";
                _news[i].TitleEn = "";
                _news[i].DescTr = "";
                _news[i].DescEn = "";
                _news[i].Image = "";
            }

            if (HomeViewCtl != null) HomeViewCtl.Reload();
            BuildAllPages();
            InitLaunchInfo();

            if (_tray != null && _settings.Notifications && !_notified && m.announcements.Count > 0)
            {
                var n = m.announcements[0];
                _tray.ShowBalloonTip(2500, m.title, _en ? n.title_en : n.title_tr, System.Windows.Forms.ToolTipIcon.Info);
                _notified = true;
            }
        }

        string FmtDate(string iso)
        {
            DateTime d;
            if (!DateTime.TryParseExact(iso, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out d)) return iso;
            try
            {
                // useUserOverride:false -> makine ayarindan bagimsiz, tutarli gorunum
                var ci = new System.Globalization.CultureInfo(Culture(), false);
                if (_lang == "tr") return d.ToString("d MMMM yyyy", ci);
                return d.ToString("d MMMM yyyy", ci);
            }
            catch (Exception) { return iso; }
        }
    }
}
