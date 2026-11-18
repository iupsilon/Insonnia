<#
.SYNOPSIS
    Cattura gli screenshot della finestra principale per il README (docs\screenshots\*.png).

.DESCRIPTION
    Carica l'eseguibile compilato, crea MainForm e la porta via reflection negli stati da mostrare
    (sempre attivo, timer, sospeso per inattività) nei temi chiaro/scuro e natalizio, poi cattura la
    finestra dallo schermo (barra del titolo Windows 11 compresa). Serve una sessione desktop
    interattiva: in CI gira sul runner Windows (.github\workflows\screenshots.yml).
    Nessun lock viene tenuto: SetThreadExecutionState vale solo per questo processo.

.EXAMPLE
    powershell -STA -ExecutionPolicy Bypass -File tools\Screenshots.ps1 -Exe src\bin\Release\Insonnia.exe
#>
param(
    [Parameter(Mandatory = $true)][string]$Exe,
    [string]$OutDir   # default: docs\screenshots accanto alla cartella tools
)

$ErrorActionPreference = 'Stop'
if (-not $OutDir) {
    $OutDir = Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) '..\docs\screenshots'
}
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
$OutDir = (Resolve-Path $OutDir).Path

if ([Threading.Thread]::CurrentThread.ApartmentState -ne 'STA') {
    throw 'Avviare PowerShell con -STA (WinForms richiede un thread STA).'
}

Add-Type -AssemblyName System.Windows.Forms, System.Drawing

# Cattura dallo schermo del riquadro visibile della finestra (DWM extended frame bounds:
# esclude i bordi di ridimensionamento invisibili di Windows 10/11)
Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System;
using System.Drawing;
using System.Runtime.InteropServices;

public static class WindowShot
{
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }

    [DllImport("dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(IntPtr hwnd, int attr, out RECT value, int size);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern bool SetProcessDPIAware();

    public static void DpiAware() { SetProcessDPIAware(); }

    public static void Front(IntPtr hwnd) { SetForegroundWindow(hwnd); }

    public static Bitmap Capture(IntPtr hwnd)
    {
        RECT r;
        if (DwmGetWindowAttribute(hwnd, 9 /* DWMWA_EXTENDED_FRAME_BOUNDS */, out r, Marshal.SizeOf(typeof(RECT))) != 0)
            GetWindowRect(hwnd, out r);
        int w = r.Right - r.Left, h = r.Bottom - r.Top;
        Bitmap bmp = new Bitmap(w, h);
        using (Graphics g = Graphics.FromImage(bmp))
            g.CopyFromScreen(r.Left, r.Top, 0, 0, new Size(w, h));
        return bmp;
    }
}
'@

[WindowShot]::DpiAware()
[System.Windows.Forms.Application]::EnableVisualStyles()
[System.Windows.Forms.Application]::SetCompatibleTextRenderingDefault($false)

$asm      = [System.Reflection.Assembly]::LoadFrom((Resolve-Path $Exe))
$BF       = [System.Reflection.BindingFlags]'Public,NonPublic,Instance,Static'
$themeT   = $asm.GetType('Insonnia.UI.Theme', $true)
$formT    = $asm.GetType('Insonnia.Forms.MainForm', $true)

# Font disponibili sul sistema (il tema ripiega su Segoe UI / MDL2 Assets se mancano)
$installed = (New-Object System.Drawing.Text.InstalledFontCollection).Families | ForEach-Object Name
foreach ($f in 'Segoe UI Variable Text', 'Segoe UI Variable Display', 'Segoe Fluent Icons', 'Segoe MDL2 Assets') {
    Write-Host ("font {0,-26} {1}" -f $f, $(if ($installed -contains $f) { 'yes' } else { 'NO' }))
}

function Set-Theme([bool]$dark, [bool]$festive) {
    # Theme.Update(dark, festive): crea la palette e solleva Theme.Changed (la finestra si ridipinge)
    $themeT.GetMethod('Update', $BF).Invoke($null, @($dark, $festive)) | Out-Null
}

function Get-F([string]$name)            { $formT.GetField($name, $BF).GetValue($form) }
function Set-F([string]$name, $value)    { $formT.GetField($name, $BF).SetValue($form, $value) }
function Invoke-M([string]$name, $argv)  { $formT.GetMethod($name, $BF).Invoke($form, $argv) | Out-Null }

function Wait-UI([int]$ms) {
    $sw = [Diagnostics.Stopwatch]::StartNew()
    while ($sw.ElapsedMilliseconds -lt $ms) {
        [System.Windows.Forms.Application]::DoEvents()
        Start-Sleep -Milliseconds 15
    }
}

# Avvia una sessione e la "retrodata" di $ago, così anello e tempo mostrano valori realistici
function Start-Session([int]$durationIndex, [TimeSpan]$ago, [switch]$idle) {
    if (Get-F '_keepAwake') { Invoke-M 'StopInsonnia' @() }
    (Get-F 'cmbDuration').SelectedIndex = $durationIndex
    (Get-F 'chkIdleSuspend').Checked    = $true
    Invoke-M 'StartInsonnia' @()
    Set-F '_idleThreshold' $null              # niente sospensione automatica durante la cattura
    Set-F '_insonniaStarted' ([DateTime]::Now - $ago)
    if ($idle) { Invoke-M 'EnterIdleSuspend' @() }
    Invoke-M 'UpdateUI' @()
    Invoke-M 'UpdateStatusDisplay' @($ago)
}

function Save-Shot([string]$name) {
    [WindowShot]::Front($form.Handle)
    $form.Activate()
    Wait-UI 1500                              # timer di 1 s + qualche frame di neve
    $bmp  = [WindowShot]::Capture($form.Handle)
    $path = Join-Path $OutDir "$name.png"
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    Write-Host ("saved {0} ({1}x{2})" -f $path, $bmp.Width, $bmp.Height)
    $bmp.Dispose()
}

Set-Theme $false $false
$form = [Activator]::CreateInstance($formT, @($false))
$form.StartPosition = 'Manual'
$form.Location      = New-Object System.Drawing.Point(80, 60)
$form.TopMost       = $true
$form.Show()
Wait-UI 800

$always = 0; $twoHours = 3
$shots = @(
    @{ Name = 'light-always-on'; Dark = $false; Festive = $false; Index = $always;   Ago = '01:23:45'; Idle = $false },
    @{ Name = 'dark-timer';      Dark = $true;  Festive = $false; Index = $twoHours; Ago = '01:35:12'; Idle = $false },
    @{ Name = 'light-idle';      Dark = $false; Festive = $false; Index = $always;   Ago = '00:42:07'; Idle = $true  },
    @{ Name = 'dark-always-on';  Dark = $true;  Festive = $false; Index = $always;   Ago = '03:07:21'; Idle = $false },
    @{ Name = 'christmas-light'; Dark = $false; Festive = $true;  Index = $always;   Ago = '02:15:30'; Idle = $false },
    @{ Name = 'christmas-dark';  Dark = $true;  Festive = $true;  Index = $twoHours; Ago = '00:48:05'; Idle = $false }
)

foreach ($s in $shots) {
    Set-Theme $s.Dark $s.Festive
    Wait-UI 300
    Start-Session $s.Index ([TimeSpan]::Parse($s.Ago)) -idle:$s.Idle
    Save-Shot $s.Name
}

# Chiusura reale (non solo nascondi nel tray)
Invoke-M 'StopInsonnia' @()
Set-F '_forceClose' $true
$form.Close()
$form.Dispose()
