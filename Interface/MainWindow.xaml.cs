using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media.Animation;

namespace Calculadora.Visual;

public partial class MainWindow : Window
{
    private static readonly CultureInfo Invariante = CultureInfo.InvariantCulture;
    private List<string> passos = new();
    private bool painelAberto;
    private bool avisoDisponivel;
    private int versaoAviso;
    private string operacao = "";
    private double acumulado;
    private bool novoNumero = true;
    private bool resultadoExibido;
    private bool alteracaoInterna;
    private bool operandoTransformado;
    private bool modoExpressao;
    private string ultimaOperacao = "";
    private double ultimoOperando;
    private readonly DispatcherTimer previa = new() { Interval = TimeSpan.FromMilliseconds(400) };

    public MainWindow()
    {
        InitializeComponent();
        TemaJanela.Aplicar(this);
        previa.Tick += (_, _) => { previa.Stop(); AtualizarPrevia(); };
        Closed += (_, _) => previa.Stop();
        Loaded += (_, _) => { Entrada.Focus(); Entrada.SelectAll(); };
    }

    private static string Numero(double n) => n.ToString("G17", Invariante);
    private static string Exibir(string texto) => texto.Replace('.', ',');
    private static string Normalizar(string texto) => texto.Replace(',', '.')
        .Replace('×', '*').Replace('÷', '/').Replace('−', '-');

    private double Avaliar(string expressao)
    {
        var lista = MotorCalculadora.Resolver(Normalizar(expressao));
        return double.Parse(lista[lista.Count - 1], NumberStyles.Float, Invariante);
    }

    private void Definir(string texto)
    {
        alteracaoInterna = true;
        Entrada.Text = Exibir(texto);
        Entrada.CaretIndex = Entrada.Text.Length;
        alteracaoInterna = false;
        Erro.Text = "";
    }

    private void Entrada_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (alteracaoInterna || Erro is null || AvisoPassos is null || TextoPassos is null) return;
        LimparPassos();
        modoExpressao = true;
        resultadoExibido = false;
        novoNumero = false;
        operacao = "";
        Resumo.Text = "";
        Erro.Text = "";
        previa.Stop();
        previa.Start();
    }

    private bool ExpressaoComposta(string texto)
        => Regex.Matches(Normalizar(texto), @"[+*/-]").Count > 1
           || texto.Contains('(');

    private void AtualizarPrevia()
    {
        if (!ExpressaoComposta(Entrada.Text) || resultadoExibido) return;
        try
        {
            passos = MotorCalculadora.Resolver(Normalizar(Entrada.Text));
            DisponibilizarPassos();
        }
        catch (FormatException) { }
        catch (ArithmeticException) { }
    }

    private void Digito(string texto)
    {
        if (modoExpressao && !resultadoExibido)
        {
            InserirNaExpressao(texto);
            return;
        }
        LimparPassos();
        if (novoNumero || resultadoExibido || Entrada.Text == "0")
        {
            Definir(texto == "," ? "0," : texto);
            novoNumero = false;
            resultadoExibido = false;
        }
        else
        {
            int pos = Entrada.SelectionStart;
            if (texto == "," && Entrada.SelectionLength == 0)
            {
                var trecho = Regex.Match(Entrada.Text[..pos], @"[0-9.,]*$").Value;
                if (trecho.Contains(',') || trecho.Contains('.')) return;
            }
            string atual = Entrada.Text.Remove(pos, Entrada.SelectionLength).Insert(pos, texto);
            Definir(atual);
            Entrada.CaretIndex = pos + texto.Length;
        }
        Entrada.Focus();
    }

    private void Tecla_Click(object sender, RoutedEventArgs e)
    {
        string tecla = ((Button)sender).Tag?.ToString() ?? "";
        Executar(() =>
        {
            switch (tecla)
            {
                case "C": Limpar(); break;
                case "CE":
                    LimparPassos(); Definir("0"); modoExpressao = false; novoNumero = true; resultadoExibido = false; break;
                case "back": Apagar(); break;
                case "(": case ")": Parenteses(tecla); break;
                case "=": Calcular(); break;
                case "+": case "-": case "*": case "/": Operador(tecla); break;
                case "sign": Funcao("sign"); break;
                case "%": case "square": case "sqrt": case "inverse": Funcao(tecla); break;
                default: Digito(tecla); break;
            }
        });
    }

    private void Operador(string op)
    {
        if (modoExpressao && !resultadoExibido)
        {
            InserirNaExpressao(op);
            return;
        }
        if (operacao != "" && (!novoNumero || operandoTransformado)) Calcular();
        acumulado = Avaliar(Entrada.Text);
        LimparPassos();
        operacao = op;
        Resumo.Text = Exibir(Numero(acumulado)) + " " + op.Replace("*", "×").Replace("/", "÷");
        novoNumero = true;
        resultadoExibido = false;
        ultimaOperacao = "";
        operandoTransformado = false;
    }

    private void Calcular()
    {
        string expressao;
        if (operacao != "")
        {
            ultimoOperando = Avaliar(Entrada.Text);
            ultimaOperacao = operacao;
            expressao = "(" + Numero(acumulado) + ")" + operacao + "(" + Numero(ultimoOperando) + ")";
            operacao = "";
        }
        else if (resultadoExibido && ultimaOperacao != "")
            expressao = "(" + Normalizar(Entrada.Text) + ")" + ultimaOperacao + "(" + Numero(ultimoOperando) + ")";
        else if (resultadoExibido) return;
        else
        {
            expressao = Normalizar(Entrada.Text);
            ultimaOperacao = "";
        }

        var resolucao = MotorCalculadora.Resolver(expressao);
        double resultado = double.Parse(resolucao[resolucao.Count - 1], NumberStyles.Float, Invariante);
        passos = resolucao;
        Resumo.Text = Exibir(expressao) + " =";
        Definir(Numero(resultado));
        resultadoExibido = true;
        novoNumero = true;
        modoExpressao = false;
        operandoTransformado = false;
        DisponibilizarPassos();
    }

    private void Funcao(string funcao)
    {
        double n = Avaliar(Entrada.Text);
        double r;
        string descricao;
        switch (funcao)
        {
            case "sign": r = -n; descricao = "−(" + Numero(n) + ")"; break;
            case "square": r = n * n; descricao = "(" + Numero(n) + ")²"; break;
            case "sqrt":
                if (n < 0) throw new ArithmeticException("Não existe raiz quadrada real de número negativo.");
                r = Math.Sqrt(n); descricao = "√(" + Numero(n) + ")"; break;
            case "inverse":
                if (n == 0) throw new DivideByZeroException("Não é possível dividir por zero.");
                r = 1 / n; descricao = "1/(" + Numero(n) + ")"; break;
            default:
                r = operacao is "+" or "-" ? acumulado * n / 100 : n / 100;
                descricao = Numero(n) + "%"; break;
        }
        if (!double.IsFinite(r)) throw new ArithmeticException("O resultado ultrapassou o limite numérico.");
        Definir(Numero(r));
        passos = new() { Exibir(descricao), Entrada.Text };
        novoNumero = true;
        operandoTransformado = operacao != "";
        resultadoExibido = operacao == "";
        if (operacao == "")
        {
            Resumo.Text = Exibir(descricao) + " =";
            ultimaOperacao = "";
        }
        DisponibilizarPassos();
    }

    private void Apagar()
    {
        LimparPassos();
        if (resultadoExibido) { Limpar(); return; }
        int pos = Entrada.SelectionStart;
        string texto = Entrada.Text;
        if (Entrada.SelectionLength > 0) texto = texto.Remove(pos, Entrada.SelectionLength);
        else if (pos > 0) { texto = texto.Remove(pos - 1, 1); pos--; }
        Definir(texto.Length == 0 ? "0" : texto);
        Entrada.CaretIndex = Math.Min(pos, Entrada.Text.Length);
        novoNumero = false;
    }

    private void Limpar()
    {
        previa.Stop();
        Definir("0");
        operacao = ""; ultimaOperacao = "";
        acumulado = 0; resultadoExibido = false; novoNumero = true;
        operandoTransformado = false;
        modoExpressao = false;
        Resumo.Text = ""; LimparPassos();
    }

    private void Executar(Action acao)
    {
        try { acao(); }
        catch (FormatException ex) { Erro.Text = ex.Message; }
        catch (ArithmeticException ex) { Erro.Text = ex.Message; }
        Entrada.Focus();
    }

    private void Entrada_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) { Executar(Calcular); e.Handled = true; }
        else if (e.Key == Key.Escape) { Limpar(); e.Handled = true; }
        else if (e.Key == Key.F9) { Executar(() => Funcao("sign")); e.Handled = true; }

    }

    private void AtivarModoExpressao()
    {
        if (modoExpressao && !resultadoExibido) return;
        if (operacao != "")
        {
            string direita = novoNumero && !operandoTransformado ? "" : Entrada.Text;
            Definir(Exibir(Numero(acumulado)) + operacao + direita);
        }
        else if (novoNumero && !resultadoExibido && Entrada.Text == "0")
            Definir("");
        operacao = "";
        ultimaOperacao = "";
        operandoTransformado = false;
        novoNumero = false;
        resultadoExibido = false;
        modoExpressao = true;
        Resumo.Text = "";
    }

    private void Parenteses(string simbolo)
    {
        // A new opening group after a result starts a fresh expression.
        if (resultadoExibido && simbolo == "(")
        {
            Limpar();
            Definir("");
        }
        AtivarModoExpressao();
        InserirNaExpressao(simbolo);
    }

    private void InserirNaExpressao(string texto)
    {
        int pos = Entrada.SelectionStart;
        string atual = Entrada.Text.Remove(pos, Entrada.SelectionLength);
        string antes = atual[..pos];
        if (texto == ")" && antes.Count(c => c == '(') <= antes.Count(c => c == ')'))
        {
            Erro.Text = "Abra um parêntese antes de fechá-lo.";
            return;
        }
        string prefixo = antes.TrimEnd();
        if (texto == "(" && prefixo.Length > 0
            && (char.IsAsciiDigit(prefixo[^1]) || prefixo[^1] == ')'))
            texto = "*" + texto;
        else if (texto.Length > 0 && char.IsAsciiDigit(texto[0])
                 && prefixo.EndsWith(')'))
            texto = "*" + texto;

        LimparPassos();
        Definir(atual.Insert(pos, texto));
        Entrada.CaretIndex = pos + texto.Length;
        novoNumero = false;
        resultadoExibido = false;
        modoExpressao = true;
        previa.Stop();
        previa.Start();
        Entrada.Focus();
    }

    private void Entrada_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (e.Text == "(" || e.Text == ")")
        {
            Executar(() => Parenteses(e.Text));
            e.Handled = true;
            return;
        }
        if (e.Text.Length == 1 && "+-*/×÷−".Contains(e.Text[0]))
        {
            Executar(() =>
            {
                AtivarModoExpressao();
                InserirNaExpressao(Normalizar(e.Text));
            });
            e.Handled = true;
            return;
        }
        if (e.Text.All(c => char.IsAsciiDigit(c) || c is ',' or '.')
            && (operacao != "" || modoExpressao))
        {
            Executar(() => Digito(e.Text.Replace('.', ',')));
            e.Handled = true;
            return;
        }
        if (resultadoExibido || novoNumero)
        {
            if (e.Text.All(c => char.IsDigit(c) || c is ',' or '.'))
            {
                Definir(""); resultadoExibido = false; novoNumero = false;
            }
        }
    }

    private void Painel_Click(object sender, RoutedEventArgs e) => AbrirPainel();

    private void Fixar_Click(object sender, RoutedEventArgs e)
    {
        Topmost = !Topmost;
        AnimarSeta();

    }

    private void LimparPassos()
    {
        passos.Clear();
        AtualizarAviso(false);
        BotaoPassos.ToolTip = "Resolução passo a passo (F2)";
        AtualizarPainel();
    }

    private void DisponibilizarPassos()
    {
        bool disponivel = passos.Count > 1;
        AtualizarAviso(disponivel && !painelAberto);
        BotaoPassos.ToolTip = disponivel
            ? "Passo a passo disponível — clique para abrir (F2)"
            : "Resolução passo a passo (F2)";
        AtualizarPainel();
    }

    private void AbrirPainel()
    {
        AnimarTracos();
        if (painelAberto)
        {
            FecharPainel();
            return;
        }
        previa.Stop();
        if (!resultadoExibido)
        {
            try { passos = MotorCalculadora.Resolver(Normalizar(Entrada.Text)); }
            catch (FormatException) { passos.Clear(); }
            catch (ArithmeticException) { passos.Clear(); }
        }
        painelAberto = true;
        AtualizarPainel();
        FundoPainel.Visibility = Visibility.Visible;
        PainelPassos.Visibility = Visibility.Visible;
        AtualizarAviso(false);
        BotaoPassos.ToolTip = "Resolução passo a passo (F2)";
        double altura = Math.Max(1, LayoutCalculadora.RowDefinitions[4].ActualHeight
            + LayoutCalculadora.RowDefinitions[5].ActualHeight);
        DeslocamentoPainel.BeginAnimation(
            System.Windows.Media.TranslateTransform.YProperty,
            new DoubleAnimation(altura, 0, TimeSpan.FromMilliseconds(180))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            });
        FundoPainel.BeginAnimation(OpacityProperty,
            new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150)));
        TextoPassos.Focus();
    }

    private void FecharPainel()
    {
        painelAberto = false;
        DeslocamentoPainel.BeginAnimation(
            System.Windows.Media.TranslateTransform.YProperty, null);
        FundoPainel.BeginAnimation(OpacityProperty, null);
        FundoPainel.Visibility = Visibility.Collapsed;
        PainelPassos.Visibility = Visibility.Collapsed;
        Entrada.Focus();
        Entrada.CaretIndex = Entrada.Text.Length;
    }

    private void FecharPainel_Click(object sender, RoutedEventArgs e) => FecharPainel();

    private void FundoPainel_Click(object sender, MouseButtonEventArgs e)
    {
        FecharPainel();
        e.Handled = true;
    }

    private void Janela_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F2)
        {
            AbrirPainel();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && painelAberto)
        {
            FecharPainel();
            e.Handled = true;
        }
    }

    private static DoubleAnimationUsingKeyFrames Movimento(params (double ms, double valor)[] pontos)
    {
        var animacao = new DoubleAnimationUsingKeyFrames();
        foreach (var ponto in pontos)
            animacao.KeyFrames.Add(new EasingDoubleKeyFrame(
                ponto.valor, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(ponto.ms)),
                new QuadraticEase { EasingMode = EasingMode.EaseOut }));
        return animacao;
    }

    private void AnimarTracos()
    {
        if (!SystemParameters.ClientAreaAnimation) return;
        EscalaTracos.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty,
            Movimento((0, 1), (65, 0.62), (200, 1)));
        EscalaTracos.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty,
            Movimento((0, 1), (65, 0.82), (200, 1)));
    }

    private void AnimarSeta()
    {
        var destino = Topmost ? System.Windows.Media.Color.FromRgb(76, 194, 255)
            : System.Windows.Media.Colors.White;
        Fixar.ToolTip = Topmost ? "Sempre visível: ativado" : "Sempre visível: desativado";
        if (!SystemParameters.ClientAreaAnimation)
        {
            CorSeta.Color = destino;
            return;
        }
        CorSeta.BeginAnimation(System.Windows.Media.SolidColorBrush.ColorProperty,
            new ColorAnimation(destino, TimeSpan.FromMilliseconds(150)));
        double sentido = Topmost ? 1 : -1;
        MovimentoSeta.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty,
            Movimento((0, 0), (80, 2 * sentido), (220, 0)));
        MovimentoSeta.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty,
            Movimento((0, 0), (80, -2 * sentido), (220, 0)));
    }

    private void AtualizarAviso(bool mostrar)
    {
        if (avisoDisponivel == mostrar) return;
        avisoDisponivel = mostrar;
        int versao = ++versaoAviso;
        if (!SystemParameters.ClientAreaAnimation)
        {
            AvisoPassos.BeginAnimation(OpacityProperty, null);
            AvisoPassos.Opacity = 1;
            AvisoPassos.Visibility = mostrar ? Visibility.Visible : Visibility.Collapsed;
            return;
        }
        if (mostrar)
        {
            AvisoPassos.Visibility = Visibility.Visible;
            AvisoPassos.BeginAnimation(OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180)));
            EscalaAviso.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty,
                Movimento((0, 0.25), (160, 1.15), (280, 1)));
            EscalaAviso.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty,
                Movimento((0, 0.25), (160, 1.15), (280, 1)));
        }
        else
        {
            var sumir = new DoubleAnimation(0, TimeSpan.FromMilliseconds(100));
            sumir.Completed += (_, _) =>
            {
                if (versao == versaoAviso && !avisoDisponivel)
                    AvisoPassos.Visibility = Visibility.Collapsed;
            };
            AvisoPassos.BeginAnimation(OpacityProperty, sumir);
        }
    }

    private void AtualizarPainel()
    {
        TextoPassos.Text = passos.Count < 2
            ? "Faça uma conta para ver a resolução aqui."
            : string.Join(Environment.NewLine + "= ", passos).Replace('.', ',');
    }
}
