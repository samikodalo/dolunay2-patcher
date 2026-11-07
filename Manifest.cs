using System.Collections.Generic;

namespace Dolunay2Launcher
{
    public class MfLinks
    {
        public string website = "";
        public string patchNotes = "";
        public string terms = "";
        public string privacy = "";
        public string support = "";
    }

    public class MfSlide
    {
        public string image = "";
        public string tag_tr = "";
        public string tag_en = "";
        public string title_tr = "";
        public string title_en = "";
        public string desc_tr = "";
        public string desc_en = "";
        public string content_tr = "";
        public string content_en = "";
        public string date = "";
        public string url = "";
        public string platforms = "pc";
    }

    public class MfNews
    {
        public int id;
        public string category_tr = "";
        public string category_en = "";
        public string color = "#2D7FF9";
        public string title_tr = "";
        public string title_en = "";
        public string excerpt_tr = "";
        public string excerpt_en = "";
        public string body_tr = "";
        public string body_en = "";
        public string image = "";
        public string date = "";
        public string url = "";
        public string author = "";
    }

    public class MfEvent
    {
        public string title_tr = "";
        public string title_en = "";
        public string category_tr = "";
        public string category_en = "";
        public string color = "#2D7FF9";
        public int day;
        public string start = "00:00";
        public string end = "23:59";
        public string start_date = "";
        public string end_date = "";
        public string desc_tr = "";
        public string desc_en = "";
    }

    public class MfEvents
    {
        public List<MfEvent> routine = new List<MfEvent>();
        public List<MfEvent> special = new List<MfEvent>();
    }

    public class MfDev
    {
        public string date = "";
        public string title_tr = "";
        public string title_en = "";
        public string tags = "";
        public string changes = "";
        public string url = "";
    }

    public class MfFile
    {
        public string path = "";
        public long size;
        public string sha256 = "";
        public string url = "";
    }

    public class MfManifest
    {
        public string title = "DOLUNAY2";
        public string subtitle = "Auto Patcher";
        public string version = "1.0.0";
        public string news = "";
        public string exe = "dolunay2client.bin";
        public string args = "";
        public bool force = true;
        public bool maintenance;
        public MfLinks links = new MfLinks();
        public List<MfSlide> slides = new List<MfSlide>();
        public List<MfNews> announcements = new List<MfNews>();
        public MfEvents events = new MfEvents();
        public List<MfDev> devlog = new List<MfDev>();
        public List<MfFile> files = new List<MfFile>();
        public List<string> remove = new List<string>();
    }
}
