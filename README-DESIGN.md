# Dolunay2 Launcher · Tasarım

Bu sürümde WPF launcher'ın görsel katmanı, referans alınan modern oyun
launcher tasarımına göre yeniden kuruldu. Patcher/manifest işleyişi
(Engine, Api, SelfUpdate, Tray) korunur.

## Neler alındı

- 1600×900 tasarım alanı + `Viewbox Stretch="Fill"` ile 1280×720 ve diğer
  16:9 çözünürlüklere oransal ölçekleme
- `Resources/Images/Backgrounds/sungma_gray.jpg` arka planı ve tema gradyanları
- Launcher buton stilleri: `PlayButtonStyle` (taş dokulu yeşil/kırmızı),
  `OutlineButton`, `GoldButton`, `DangerButton`, `TopBar*`, `PillTabStyle`
- Tüm vektör ikonlar (`Resources/Styles/vectoricons.xaml`)
- ComboBox kapalı/açık stilleri, toggle switch, slider, arama kutusu
- Krem renkli yuvarlak tooltip stili
- `SidebarNavItem` (animasyonlu seçim çizgisi, hover, rozet, ikon dönüşü)
- `SidebarServerIcon` (hover parlama/shine, seçili sunucu animasyonu)
- `SubNewsControl` (haber kartı + hover çerçevesi)
- `OrnamentFrame` (haber detayı altın köşe süslemesi)
- Çift halkalı dairesel ilerleme, yüzde, durum/dosya bilgisi, dosya sayacı,
  duraklat/devam et butonu ve pencerenin altında ince ilerleme çubuğu
  (`Controls/RadialProgressBar.cs`).
- **Oyun başlatma doğrulaması:** OYNA'ya her basıldığında manifestteki tüm
  dosyalar boyut + SHA-256 ile yeniden doğrulanır. Değiştirilmiş/eksik dosya
  varsa önce indirilir, doğrulanır, sonra oyun başlatılır. Böylece güncelleme
  sonrası dosya değiştirip doğrudan oyuna girme engellenir.
- Language popup ve dişli menüsü (DropDownMenuItem)
- Tema motoru `ThemeManager` (Default, Brown, Gray, Blue, Green)

## Sayfalar

| Sayfa | Dosya |
|---|---|
| Ana Sayfa (hero + duyuru şeridi) | `Views/HomeView.xaml` |
| Duyurular (filtre + liste + detay) | `Views/NewsView.xaml` |
| Etkinlikler (rutin hafta + özel takvim) | `Views/EventsView.xaml` |
| Dev-Log (zaman çizelgesi) | `Views/DevLogView.xaml` |
| Ayarlar (6 bölüm) | `Views/SettingsView.xaml` |
| Kabuk (sidebar, üst bar, alt bar) | `MainWindow.xaml` |

## Yeni ortak kontroller

`Controls/` altında: `ClippingBorder`, `SidebarNavItem`,
`SidebarServerIcon`, `SubNewsControl`, `OrnamentFrame`, `PageHeader`,
`DropDownMenuItem`.

## Görseller

Launcher UI görselleri `patcher-wpf/Resources` altına kopyalandı. Ana sayfa
slaytları ve duyuru kartları panelden (`public/patcher/slides`,
`public/patcher/news`) beslendiği için bu klasördeki örnek görseller de
güncel launcher görselleriyle değiştirildi. Panelden istediğiniz zaman
değiştirilebilir.

## Derleme

```text
cd patcher-wpf
dotnet build -c Release
```

Çıktı: `patcher-wpf/bin/Release/net48/Dolunay2Launcher.exe`

## Yayınlarken

1. Yeni exe'yi `public/patcher/Dolunay2Launcher.exe` üzerine kopyalayın.
2. Admin panelde **Auto Patcher → Dosya Yöneticisi → Diski tara** ile
   manifest hash/boyutlarını yenileyin.
3. Aksi halde eski manifest hash'i nedeniyle istemciler sürekli indirme
   döngüsüne girebilir.
