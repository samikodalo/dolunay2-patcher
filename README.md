# Dolunay2 Auto Patcher

Dolunay2 için modern launcher tarzında hazırlanmış WPF (`.NET Framework 4.8`)
auto patcher / launcher.

## Özellikler

- Modern launcher görünümü: kabuk, ana sayfa, duyurular, etkinlikler, Dev-Log
  ve ayarlar sayfaları.
- Duyurular forumun **Duyurular** kategorisinden gelir; rozet, tarih, yazar,
  başlık ve HTML içerik aynen kullanılır.
- Modern indirme ekranı: çift halkalı ilerleme, dosya/durum bilgisi,
  dosya sayacı, duraklat/devam et ve alt ilerleme çubuğu.
- **Hile önlemi:** OYNA'ya her basıldığında manifestteki tüm dosyalar
  boyut + SHA-256 ile doğrulanır. Değişmiş/eksik dosya varsa önce indirilir,
  doğrulanır, sonra oyun başlatılır.
- Launcher kendini güncelleyebilir; sistem tepsisi ve tema desteği vardır.

## Gereksinimler

- Windows
- .NET Framework 4.8
- Derleme için .NET SDK (`dotnet`) veya Visual Studio 2022

## Derleme

```powershell
cd patcher-wpf
dotnet build -c Release
```

Çıktı:

```text
patcher-wpf\bin\Release\net48\Dolunay2Launcher.exe
```

## Sunucu ayarı

Launcher, yanındaki `patcher.ini` dosyasından sunucu adresini okur:

```ini
Server=http://localhost:3002
```

Yerel geliştirme için `patcher.ini.example` dosyasını `patcher.ini` olarak
kopyalayın. Dosya yoksa varsayılan adres `https://dolunay2global.com` olur.

Manifest adresi:

```text
<Server>/patcher/manifest.json
```

Manifest, web panelindeki **Auto Patcher** bölümünden yönetilir.

## Yayınlama

1. Release exe'yi web projesindeki `public/patcher/Dolunay2Launcher.exe`
   üzerine kopyalayın.
2. Panelde **Auto Patcher → Dosya Yöneticisi → Diski tara** ile manifest
   hash/boyutlarını yenileyin.

## Geliştirme argümanları

```text
--page=news
--page=events
--page=devlog
--page=settings --setsection=0
--lang=tr
--collapsed
```

## İlgili repo

Web paneli: https://github.com/samikodalo/dolunay2-web
