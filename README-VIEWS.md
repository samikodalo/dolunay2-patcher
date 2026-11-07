# Dolunay2 Launcher - XAML Sayfa Yapısı

Launcher arayüzü artık sayfa sayfa XAML UserControl dosyalarına ayrıldı.
Her dosya Visual Studio'da **Designer** ile açılabilir; buton, kart, boşluk
ve yazılar fareyle taşınıp düzenlenebilir.

## Sayfalar

| Dosya | Sayfa |
|---|---|
| `Views/HomeView.xaml` | Ana sayfa (hero + duyuru şeridi) |
| `Views/NewsView.xaml` | Duyurular |
| `Views/EventsView.xaml` | Etkinlikler |
| `Views/DevLogView.xaml` | Dev-Log |
| `Views/SettingsView.xaml` | Ayarlar kabuğu (başlık + sol menü) |
| `Views/Settings/GeneralView.xaml` | Ayarlar > Genel |
| `Views/Settings/AppearanceView.xaml` | Ayarlar > Görünüm |
| `Views/Settings/DownloadsView.xaml` | Ayarlar > İndirmeler |
| `Views/Settings/GameView.xaml` | Ayarlar > Oyun |
| `Views/Settings/StorageView.xaml` | Ayarlar > Depolama ve Destek |
| `Views/Settings/AboutView.xaml` | Ayarlar > Hakkında |

`MainWindow.xaml` sadece kabuğu (üst bar, sol menü, alt bar, arka plan)
tutar. Sayfalar bu kabuğun içine yerleştirilir.

## Designer'da açma

1. `Dolunay2Launcher.sln` dosyasını Visual Studio 2022 ile açın.
2. Çözüm Gezgini'nde ilgili `.xaml` dosyasına çift tıklayın.
3. Tasarım penceresi açılır. Kontrolleri seçip özelliklerini
   (Margin, Width, Height, FontSize, Foreground vb.) düzenleyebilirsiniz.

> Dinamik listeler (duyurular, etkinlikler, dev-log kayıtları) panelden
> geldiği için Designer'da boş görünür. Sayfanın başlığı, kartları,
> butonları ve yerleşimi Designer'da görünür; gerçek liste içeriği
> F5 ile çalıştırınca panelden dolar.

## F5 ile sayfa açma

`Properties/launchSettings.json` içinde hazır profiller var:

- Dolunay2 Launcher (Ana Sayfa)
- Duyurular
- Etkinlikler
- Dev-Log
- Ayarlar - Genel / Gorunum / Indirmeler / Oyun / Depolama ve Destek / Hakkinda
- Daraltilmis Menu

Visual Studio'da üstteki çalıştırma düğmesinin yanındaki listeden profil
seçip **F5**'e basın.

Alternatif olarak `--page=` ve `--setsection=` argümanları kullanılabilir:

```text
--page=home
--page=news
--page=events
--page=devlog
--page=settings --setsection=0   (0=Genel, 1=Görünüm, 2=İndirmeler,
                                  3=Oyun, 4=Depolama, 5=Hakkında)
```

## Kod tarafı

- `Views/PageViewBase.cs`: Tüm sayfaların ortak taban sınıfı. `Host`
  üzerinden mevcut launcher işleyişine (dil, ayar kaydetme, URL açma,
  klasör açma, panel verisi) erişir.
- `MainWindow.ViewHost.cs`: `MainWindow` ile XAML sayfaları arasındaki
  köprü. Motor dosyalarına (`Engine`, `SelfUpdate`, `Api`) dokunulmadı.
- Sayfa kodları (`*.xaml.cs`) yalnızca kendi görsel öğelerini doldurur;
  indirme, doğrulama, kendini güncelleme, tepsi ve bildirim mantığı
  eskisi gibi `MainWindow` içinde kalır.

## Derleme / yayınlama

```bat
cd C:\laragon\www\web\patcher-wpf
build.bat
```

`build.bat` Release derlemesi yapıp exe'yi
`..\public\patcher\Dolunay2Launcher.exe` üzerine kopyalar.

Yerel test için panel sunucusu (`node server.js`, port 3002) açıkken
`run-local.bat` kullanılabilir. Projedeki `patcher.ini` yerel sunucuyu
gösterir; yayındaki exe canlı adresi kullanır.
