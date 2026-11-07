namespace Dolunay2Launcher
{
    public partial class MainWindow
    {
        // Yerel ayarlar (settings.json) ve sayfalar arasi kalan durum.
        LocalSettings _settings = new LocalSettings();
        string _eventSearch = "";

        internal string EventSearch
        {
            get { return _eventSearch; }
            set { _eventSearch = value ?? ""; }
        }

        static int TodayDay()
        {
            int dow = (int)System.DateTime.Now.DayOfWeek;
            return dow == 0 ? 7 : dow;
        }
    }
}
