using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Dolunay2Launcher
{
    // Kucuk HTML -> FlowDocument cevirici (haber detay icin).
    // p/br/h1-h4/ul-li/b/i/u/a destekler; img baglantiya donusur.
    public static class Html
    {
        public static FlowDocument Build(string src, Brush fg, Brush heading, Brush linkFg, Action<string> openUrl)
        {
            var doc = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 13,
                FontWeight = FontWeights.Normal,
                Foreground = fg,
                LineHeight = 22,
                PagePadding = new Thickness(0),
                Background = Brushes.Transparent
            };
            TextOptions.SetTextFormattingMode(doc, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(doc, TextRenderingMode.ClearType);
            if (string.IsNullOrWhiteSpace(src))
                return doc;
            string s = src.Replace("\r\n", "\n");
            s = Regex.Replace(s, @"(?is)<(script|style)[^>]*>.*?</\1>", "");
            s = Regex.Replace(s, @"(?is)<!--.*?-->", "");
            bool hasTags = Regex.IsMatch(s, @"<\s*(p|br|div|li|ul|ol|h[1-4]|b|strong|i|em|u|a|img|figure|figcaption|span|tr|td)\b", RegexOptions.IgnoreCase);
            if (!hasTags)
            {
                foreach (var line in s.Split('\n'))
                {
                    string t = System.Net.WebUtility.HtmlDecode(line.Trim());
                    if (t.Length == 0) continue;
                    doc.Blocks.Add(new Paragraph(new Run(t)));
                }
                return doc;
            }
            s = Regex.Replace(s, @"(?is)<(figure|figcaption|div|tr|table|tbody)[^>]*>", "\n");
            s = Regex.Replace(s, @"(?is)<p\b([^>]*)>", m =>
            {
                string attrs = m.Groups[1].Value;
                if (Regex.IsMatch(attrs, @"ql-align-center", RegexOptions.IgnoreCase)) return "\n\u0001C\u0001";
                if (Regex.IsMatch(attrs, @"ql-align-right", RegexOptions.IgnoreCase)) return "\n\u0001R\u0001";
                if (Regex.IsMatch(attrs, @"ql-align-justify", RegexOptions.IgnoreCase)) return "\n\u0001J\u0001";
                return "\n";
            });
            s = Regex.Replace(s, @"(?is)</(figure|div|tr|table|tbody|p)>", "\n");
            s = Regex.Replace(s, @"(?is)<br[^>]*>", "\n");
            // Sirali listeler: <ol><li> -> 1. 2. 3. (data-list=bullet ise •)
            s = Regex.Replace(s, @"(?is)<ol\b[^>]*>(.*?)</ol>", m =>
            {
                int n = 0;
                string inner = Regex.Replace(m.Groups[1].Value, @"(?is)<li\b([^>]*)>", mm =>
                {
                    bool bullet = Regex.IsMatch(mm.Groups[1].Value, @"data-list\s*=\s*[""']bullet[""']", RegexOptions.IgnoreCase);
                    if (bullet) return "\n• ";
                    n++;
                    return "\n" + n + ". ";
                });
                return "\n" + inner + "\n";
            });
            // Sirasiz listeler: <ul><li> -> •
            s = Regex.Replace(s, @"(?is)<ul\b[^>]*>(.*?)</ul>", m =>
                "\n" + Regex.Replace(m.Groups[1].Value, @"(?is)<li\b[^>]*>", "\n• ") + "\n");
            s = Regex.Replace(s, @"(?is)<li[^>]*>", "\n• ");
            s = Regex.Replace(s, @"(?is)</?(ul|ol)[^>]*>", "\n");
            foreach (Match m in Regex.Matches(s, @"(?is)<h([1-4])[^>]*>(.*?)</h\1>"))
            {
                string inner = Inline(m.Groups[2].Value, linkFg, openUrl);
                s = s.Replace(m.Value, "\n§H" + m.Groups[1].Value + "§" + inner + "\n");
            }
            foreach (var para in s.Split('\n'))
            {
                string t = para.Trim();
                TextAlignment align = TextAlignment.Left;
                if (t.StartsWith("\u0001C\u0001")) { align = TextAlignment.Center; t = t.Substring(3).TrimStart(); }
                else if (t.StartsWith("\u0001R\u0001")) { align = TextAlignment.Right; t = t.Substring(3).TrimStart(); }
                else if (t.StartsWith("\u0001J\u0001")) { align = TextAlignment.Justify; t = t.Substring(3).TrimStart(); }
                if (t.Length == 0) continue;
                if (t.StartsWith("§H"))
                {
                    var p = new Paragraph { FontSize = 15, Margin = new Thickness(0, 10, 0, 4), TextAlignment = align };
                    p.Inlines.Add(new Bold(new Run(Strip(t.Substring(4)))) { Foreground = heading });
                    doc.Blocks.Add(p);
                    continue;
                }
                var pg = new Paragraph { TextAlignment = align };
                foreach (var inl in Inlines(t, linkFg, openUrl)) pg.Inlines.Add(inl);
                doc.Blocks.Add(pg);
            }
            return doc;
        }

        static string Inline(string s, Brush linkFg, Action<string> openUrl) { return s; }

        static string Strip(string s)
        {
            s = Regex.Replace(s, @"<[^>]+>", "");
            return System.Net.WebUtility.HtmlDecode(s);
        }

        static List<Inline> Inlines(string s, Brush linkFg, Action<string> openUrl)
        {
            var out_ = new List<Inline>();
            s = Regex.Replace(s, @"(?is)<(figure|figcaption|span|div|font)[^>]*>", "");
            s = Regex.Replace(s, @"(?is)</(figure|figcaption|span|div|font)>", "");
            int i = 0;
            var stack = new Stack<string>();
            var buf = new System.Text.StringBuilder();
            Action flush = () =>
            {
                if (buf.Length == 0) return;
                string t = System.Net.WebUtility.HtmlDecode(buf.ToString());
                buf.Length = 0;
                Inline r = new Run(t);
                bool b = false, it = false, u = false;
                foreach (var tg in stack) { if (tg == "b") b = true; else if (tg == "i") it = true; else if (tg == "u") u = true; }
                if (b) r = new Bold(r);
                if (it) r = new Italic(r);
                if (u) r = new Underline(r);
                out_.Add(r);
            };
            while (i < s.Length)
            {
                if (s[i] == '<')
                {
                    // inline gorsel: <img src="...">
                    var im = Regex.Match(s.Substring(i), @"^<\s*img\b[^>]*\bsrc\s*=\s*[""']([^""']+)[""'][^>]*>", RegexOptions.IgnoreCase);
                    if (im.Success)
                    {
                        flush();
                        string src = im.Groups[1].Value;
                        try
                        {
                            var bmp = new System.Windows.Media.Imaging.BitmapImage();
                            bmp.BeginInit();
                            bmp.UriSource = new Uri(src, UriKind.RelativeOrAbsolute);
                            bmp.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                            bmp.EndInit();
                            var img = new System.Windows.Controls.Image
                            {
                                Source = bmp,
                                MaxWidth = 520,
                                MaxHeight = 360,
                                Stretch = Stretch.Uniform,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                Margin = new Thickness(0, 6, 0, 6)
                            };
                            out_.Add(new InlineUIContainer(img) { BaselineAlignment = BaselineAlignment.Bottom });
                        }
                        catch (Exception) { }
                        i += im.Length;
                        continue;
                    }
                    var m = Regex.Match(s.Substring(i), @"^<\s*(/\s*)?(a|b|strong|i|em|u)\b([^>]*)>", RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        flush();
                        bool close = m.Groups[1].Value.Length > 0;
                        string tag = m.Groups[2].Value.ToLowerInvariant();
                        string norm = tag == "strong" ? "b" : (tag == "em" ? "i" : tag);
                        if (norm == "a" && !close)
                        {
                            var hm = Regex.Match(m.Groups[3].Value, @"href\s*=\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase);
                            string url = hm.Success ? hm.Groups[1].Value : "";
                            int end = s.IndexOf("</a>", i, StringComparison.OrdinalIgnoreCase);
                            if (end < 0) end = s.Length;
                            string inner = s.Substring(i + m.Length, Math.Max(0, end - (i + m.Length)));
                            var link = new Hyperlink(new Run(System.Net.WebUtility.HtmlDecode(Strip(inner)))) { Foreground = linkFg };
                            string u2 = url;
                            link.Click += (ss, ee) => { try { openUrl(u2); } catch (Exception) { } };
                            out_.Add(link);
                            i = end < s.Length ? end + 4 : s.Length;
                            continue;
                        }
                        if (close) { if (stack.Count > 0) stack.Pop(); }
                        else stack.Push(norm);
                        i += m.Length;
                        continue;
                    }
                    int gt = s.IndexOf('>', i);
                    i = gt < 0 ? s.Length : gt + 1;
                    continue;
                }
                buf.Append(s[i]);
                i++;
            }
            flush();
            return out_;
        }
    }
}
