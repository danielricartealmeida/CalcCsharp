using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Calculadora.Visual;

public partial class MainWindow : Window
{
    private static readonly CultureInfo Invariante = CultureInfo.InvariantCulture;
    private List<string> passos = new();
    private PainelAuxiliar? painel;
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
        LocationChanged += (_, _) => PosicionarPainel();
        Closed += (_, _) => { previa.Stop(); painel?.Close(); };
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
        if (alteracaoInterna || Erro is null || AvisoPassos is null) return;
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
        else if (e.Key == Key.F2) { AbrirPainel(); e.Handled = true; }
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
        Fixar.Foreground = Topmost
            ? System.Windows.Media.Brushes.DeepSkyBlue : System.Windows.Media.Brushes.White;
        if (painel is not null) painel.Topmost = Topmost;
    }

    private void LimparPassos()
    {
        passos.Clear();
        AvisoPassos.Visibility = Visibility.Collapsed;
        BotaoPassos.ToolTip = "Resolução passo a passo (F2)";
        AtualizarPainel();
    }

    private void DisponibilizarPassos()
    {
        bool disponivel = passos.Count > 1;
        AvisoPassos.Visibility = disponivel && painel is null
            ? Visibility.Visible : Visibility.Collapsed;
        BotaoPassos.ToolTip = disponivel
            ? "Passo a passo disponível — clique para abrir (F2)"
            : "Resolução passo a passo (F2)";
        AtualizarPainel();
    }

    private void AbrirPainel()
    {
        previa.Stop();
        if (!resultadoExibido)
        {
            try { passos = MotorCalculadora.Resolver(Normalizar(Entrada.Text)); }
            catch (FormatException) { passos.Clear(); }
            catch (ArithmeticException) { passos.Clear(); }
        }
        if (painel is null)
        {
            painel = new PainelAuxiliar { Owner = this, Topmost = Topmost };
            painel.Closed += (_, _) => painel = null;
        }
        AtualizarPainel();
        PosicionarPainel();
        if (!painel.IsVisible) painel.Show();
        AvisoPassos.Visibility = Visibility.Collapsed;
        BotaoPassos.ToolTip = "Resolução passo a passo (F2)";
        painel.Activate();
    }

    private void AtualizarPainel()
    {
        painel?.Atualizar(passos);
    }

    private void PosicionarPainel()
    {
        if (painel is null) return;
        var area = SystemParameters.WorkArea;
        double direita = Left + Width + 8;
        painel.Left = direita + painel.Width <= area.Right
            ? direita : Math.Max(area.Left, Left - painel.Width - 8);
        painel.Top = Math.Max(area.Top, Math.Min(Top, area.Bottom - painel.Height));
    }
}
