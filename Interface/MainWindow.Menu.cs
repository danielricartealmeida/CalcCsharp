using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Calculadora.Visual;

public partial class MainWindow
{
    private bool menuAberto;
    private int versaoMenu;

    private void Menu_Click(object sender, RoutedEventArgs e) => AlternarMenu();

    private void AlternarMenu()
    {
        if (menuAberto) { FecharMenu(); return; }
        if (painelAberto) FecharPainel();
        ++versaoMenu;
        menuAberto = true;
        FundoMenu.Visibility = Visibility.Visible;
        BarraMenu.Visibility = Visibility.Visible;
        AnimarIconeMenu();
        if (SystemParameters.ClientAreaAnimation)
        {
            DeslocamentoMenu.BeginAnimation(TranslateTransform.XProperty,
                new DoubleAnimation(DeslocamentoMenu.X, 0, TimeSpan.FromMilliseconds(180))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                });
            FundoMenu.BeginAnimation(OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(140)));
        }
        else
        {
            DeslocamentoMenu.BeginAnimation(TranslateTransform.XProperty, null);
            DeslocamentoMenu.X = 0;
            FundoMenu.BeginAnimation(OpacityProperty, null);
            FundoMenu.Opacity = 1;
        }
        FecharBotaoMenu.Focus();
    }

    private void AnimarIconeMenu()
    {
        if (!SystemParameters.ClientAreaAnimation) return;
        EscalaMenu.BeginAnimation(ScaleTransform.ScaleXProperty,
            Movimento((0, 1), (65, 0.62), (200, 1)));
    }

    private void FecharMenu(bool animar = true)
    {
        if (!menuAberto && BarraMenu.Visibility == Visibility.Collapsed) return;
        menuAberto = false;
        int versao = ++versaoMenu;
        if (animar && SystemParameters.ClientAreaAnimation)
        {
            var fechar = new DoubleAnimation(DeslocamentoMenu.X, -280,
                TimeSpan.FromMilliseconds(140))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            fechar.Completed += (_, _) =>
            {
                if (versao == versaoMenu && !menuAberto) OcultarMenu();
            };
            DeslocamentoMenu.BeginAnimation(TranslateTransform.XProperty, fechar);
            FundoMenu.BeginAnimation(OpacityProperty,
                new DoubleAnimation(0, TimeSpan.FromMilliseconds(140)));
        }
        else OcultarMenu();
        Entrada.Focus();
    }

    private void OcultarMenu()
    {
        FundoMenu.Visibility = Visibility.Collapsed;
        BarraMenu.Visibility = Visibility.Collapsed;
        DeslocamentoMenu.BeginAnimation(TranslateTransform.XProperty, null);
        DeslocamentoMenu.X = -280;
        FundoMenu.BeginAnimation(OpacityProperty, null);
        FundoMenu.Opacity = 1;
    }

    private void FundoMenu_Click(object sender, MouseButtonEventArgs e)
    {
        FecharMenu();
        e.Handled = true;
    }

    private void Padrao_Click(object sender, RoutedEventArgs e)
    {
        // Choosing the current mode preserves the in-progress calculation.
        FecharMenu();
    }
}
