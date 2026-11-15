using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Calculadora.Visual;

public sealed class PainelAuxiliar : Window
{
    private readonly TextBox resolucao = new()
    {
        IsReadOnly = true,
        TextWrapping = TextWrapping.Wrap,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        FontFamily = new FontFamily("Consolas"),
        FontSize = 19,
        Background = Brushes.Transparent,
        Foreground = Brushes.Gainsboro,
        BorderThickness = new Thickness(0),
        Padding = new Thickness(16)
    };

    public PainelAuxiliar()
    {
        Title = "Resolução passo a passo";
        Width = 330; Height = 500;
        ResizeMode = ResizeMode.CanMinimize;
        WindowStyle = WindowStyle.SingleBorderWindow;
        ShowInTaskbar = false;
        Background = new SolidColorBrush(Color.FromRgb(32, 32, 32));
        Foreground = Brushes.White;
        Content = resolucao;
        TemaJanela.Aplicar(this);
    }

    public void Atualizar(IReadOnlyList<string> passos)
    {
        resolucao.Text = passos.Count < 2
            ? "O passo a passo aparecerá aqui quando houver uma conta válida."
            : string.Join(Environment.NewLine + "= ", passos).Replace('.', ',');
    }
}
