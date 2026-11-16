using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Calculadora.Visual;

// Native frame retains Windows dragging, taskbar, corners and DWM shadow.
public static class TemaJanela
{
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd, int atributo, ref int valor, int tamanho);

    public static void Aplicar(Window janela)
    {
        janela.SourceInitialized += (_, _) =>
        {
            janela.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(
                new Uri("pack://application:,,,/Assets/logo.ico", UriKind.Absolute));
            IntPtr hwnd = new WindowInteropHelper(janela).Handle;
            int escuro = 1, arredondado = 2, fundo = 0x202020, branco = 0xFAFAFA;
            DwmSetWindowAttribute(hwnd, 20, ref escuro, sizeof(int));
            DwmSetWindowAttribute(hwnd, 33, ref arredondado, sizeof(int));
            DwmSetWindowAttribute(hwnd, 35, ref fundo, sizeof(int));
            DwmSetWindowAttribute(hwnd, 36, ref branco, sizeof(int));
        };
    }
}
