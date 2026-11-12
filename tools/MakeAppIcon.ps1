<#
.SYNOPSIS
    Rigenera src\insonnia.ico (icona dell'applicazione) dal disegno vettoriale di TrayIconRenderer.

.DESCRIPTION
    Carica l'eseguibile compilato, chiama Insonnia.UI.TrayIconRenderer.RenderAppIcon per ogni
    dimensione standard di Windows e scrive un .ico multi-risoluzione:
     - fino a 128 px: immagini DIB a 32 bit con canale alfa (formato classico, letto ovunque);
     - 256 px: immagine PNG (formato standard per la dimensione "jumbo").

.EXAMPLE
    powershell -STA -ExecutionPolicy Bypass -File tools\MakeAppIcon.ps1 -Exe src\bin\Debug\Insonnia.exe
#>
param(
    [Parameter(Mandatory = $true)][string]$Exe,
    [string]$Output   # default: src\insonnia.ico accanto alla cartella tools
)

$ErrorActionPreference = 'Stop'
if (-not $Output) {
    # $PSScriptRoot non è disponibile nei default di param() in PowerShell 5.1
    $Output = Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) '..\src\insonnia.ico'
}
Add-Type -AssemblyName System.Drawing

$asm    = [System.Reflection.Assembly]::LoadFrom((Resolve-Path $Exe))
$render = $asm.GetType('Insonnia.UI.TrayIconRenderer').GetMethod('RenderAppIcon')

# Converte una bitmap 32bpp in DIB per ICO: BITMAPINFOHEADER + pixel BGRA dal basso + maschera AND vuota
function ConvertTo-IconDib([System.Drawing.Bitmap]$bmp) {
    $size = $bmp.Width
    $rect = New-Object System.Drawing.Rectangle(0, 0, $size, $size)
    $data = $bmp.LockBits($rect, [System.Drawing.Imaging.ImageLockMode]::ReadOnly,
                          [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $pixels = New-Object byte[] ($size * $size * 4)
    [System.Runtime.InteropServices.Marshal]::Copy($data.Scan0, $pixels, 0, $pixels.Length)
    $bmp.UnlockBits($data)

    $maskStride = [int]([Math]::Ceiling($size / 32.0) * 4)
    $ms = New-Object System.IO.MemoryStream
    $w  = New-Object System.IO.BinaryWriter($ms)
    $w.Write([UInt32]40); $w.Write([Int32]$size); $w.Write([Int32]($size * 2))
    $w.Write([UInt16]1);  $w.Write([UInt16]32);   $w.Write([UInt32]0)
    $w.Write([UInt32]($size * $size * 4 + $maskStride * $size))
    $w.Write([Int32]0); $w.Write([Int32]0); $w.Write([UInt32]0); $w.Write([UInt32]0)
    for ($y = $size - 1; $y -ge 0; $y--) { $w.Write($pixels, $y * $size * 4, $size * 4) }
    $w.Write((New-Object byte[] ($maskStride * $size)))   # trasparenza già nel canale alfa
    $w.Flush()
    return ,$ms.ToArray()
}

$sizes  = @(16, 20, 24, 32, 40, 48, 64, 96, 128, 256)
$images = @()
foreach ($size in $sizes) {
    $bmp = $render.Invoke($null, @($size))
    if ($size -ge 256) {
        $ms = New-Object System.IO.MemoryStream
        $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
        $bytes = $ms.ToArray()
    } else {
        $bytes = ConvertTo-IconDib $bmp
    }
    $bmp.Dispose()
    $images += ,@($size, $bytes)
}

# ICONDIR (6 byte) + ICONDIRENTRY (16 byte ciascuna) + dati immagine
$out    = New-Object System.IO.MemoryStream
$writer = New-Object System.IO.BinaryWriter($out)
$writer.Write([UInt16]0)               # riservato
$writer.Write([UInt16]1)               # tipo: icona
$writer.Write([UInt16]$images.Count)

$offset = 6 + 16 * $images.Count
foreach ($img in $images) {
    $size = $img[0]; $data = $img[1]
    $dim  = if ($size -ge 256) { 0 } else { $size }   # 0 = 256 px
    $writer.Write([Byte]$dim)          # larghezza
    $writer.Write([Byte]$dim)          # altezza
    $writer.Write([Byte]0)             # colori in palette
    $writer.Write([Byte]0)             # riservato
    $writer.Write([UInt16]1)           # piani
    $writer.Write([UInt16]32)          # bit per pixel
    $writer.Write([UInt32]$data.Length)
    $writer.Write([UInt32]$offset)
    $offset += $data.Length
}
foreach ($img in $images) { $writer.Write([byte[]]$img[1]) }
$writer.Flush()

[System.IO.File]::WriteAllBytes([System.IO.Path]::GetFullPath($Output), $out.ToArray())
"Scritto $Output ($($images.Count) dimensioni: $($sizes -join ', ') px)"
