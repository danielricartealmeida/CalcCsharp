using System.Globalization;

public static class MotorCalculadora
{
    public static List<string> Resolver(string expressao)
    {
        List<string> elementos = SepararElementos(expressao);
        int indice = 0;

        NoExpressao arvore = LerExpressao(elementos, ref indice);

        if (indice != elementos.Count)
        {
            throw new FormatException(
                $"Elemento inesperado: '{elementos[indice]}'. " +
                "Escreva as multiplicações com *.");
        }

        List<string> passos = new List<string>();
        passos.Add(MostrarExpressao(arvore));

        while (ResolverUmPasso(arvore))
        {
            string passo = MostrarExpressao(arvore);

            if (passo != passos[passos.Count - 1])
            {
                passos.Add(passo);
            }
        }

        return passos;
    }

    // Separa números, operadores e parênteses.
    private static List<string> SepararElementos(string expressao)
    {
        List<string> elementos = new List<string>();
        int posicao = 0;

        while (posicao < expressao.Length)
        {
            char simbolo = expressao[posicao];
            if (char.IsWhiteSpace(simbolo))
            {
                posicao++;
                continue;
            }

            if ((simbolo >= '0' && simbolo <= '9') || simbolo == '.' || simbolo == ',')
            {
                int inicio = posicao;
                bool decimalLido = false;
                while (posicao < expressao.Length)
                {
                    char c = expressao[posicao];
                    if (c >= '0' && c <= '9') { posicao++; continue; }
                    if ((c == '.' || c == ',') && !decimalLido)
                    {
                        decimalLido = true;
                        posicao++;
                        continue;
                    }
                    break;
                }
                if (posicao < expressao.Length && (expressao[posicao] == 'E' || expressao[posicao] == 'e'))
                {
                    posicao++;
                    if (posicao < expressao.Length && (expressao[posicao] == '+' || expressao[posicao] == '-'))
                        posicao++;
                    while (posicao < expressao.Length && char.IsAsciiDigit(expressao[posicao]))
                        posicao++;
                }
                elementos.Add(expressao.Substring(inicio, posicao - inicio).Replace(',', '.'));
            }
            else if ("+-*/()".Contains(simbolo))
            {
                elementos.Add(simbolo.ToString());
                posicao++;
            }
            else
            {
                throw new FormatException(
                    $"Caractere não permitido: '{simbolo}'. " +
                    "Use números, +, -, *, / e parênteses.");
            }
        }

        return elementos;
    }

    // Monta os nós de soma e subtração.
    private static NoExpressao LerExpressao(List<string> elementos, ref int indice)
    {
        NoExpressao esquerda = LerTermo(elementos, ref indice);

        while (indice < elementos.Count
               && (elementos[indice] == "+" || elementos[indice] == "-"))
        {
            string operador = elementos[indice];
            indice++;

            NoExpressao direita = LerTermo(elementos, ref indice);

            esquerda = new NoExpressao(operador, esquerda, direita);
        }

        return esquerda;
    }

    // Monta os nós de multiplicação e divisão.
    private static NoExpressao LerTermo(List<string> elementos, ref int indice)
    {
        NoExpressao esquerda = LerFator(elementos, ref indice);

        while (indice < elementos.Count
               && (elementos[indice] == "*" || elementos[indice] == "/"))
        {
            string operador = elementos[indice];
            indice++;

            NoExpressao direita = LerFator(elementos, ref indice);

            esquerda = new NoExpressao(operador, esquerda, direita);
        }

        return esquerda;
    }

    // Lê um número ou um grupo entre parênteses.
    private static NoExpressao LerFator(List<string> elementos, ref int indice)
    {
        if (indice >= elementos.Count)
        {
            throw new FormatException("Faltou um número na expressão.");
        }

        if (elementos[indice] == "+" || elementos[indice] == "-")
        {
            string sinal = elementos[indice++];
            NoExpressao operando = LerFator(elementos, ref indice);
            if (sinal == "+") return operando;
            return new NoExpressao("-", new NoExpressao(0), operando);
        }

        if (elementos[indice] == "(")
        {
            indice++;

            NoExpressao conteudo = LerExpressao(elementos, ref indice);

            if (indice >= elementos.Count)
            {
                throw new FormatException("Faltou fechar um parêntese.");
            }

            if (elementos[indice] != ")")
            {
                throw new FormatException(
                    $"Era esperado ')', mas apareceu '{elementos[indice]}'. " +
                    "Verifique se faltou um operador.");
            }

            indice++;

            return new NoExpressao("()", conteudo);
        }

        string elemento = elementos[indice];

        if (!double.TryParse(
                elemento,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double numero)
            || !double.IsFinite(numero))
        {
            throw new FormatException(
                $"Era esperado um número, mas apareceu '{elemento}'.");
        }

        indice++;

        return new NoExpressao(numero);
    }

    // Resolve apenas uma operação por chamada.
    // Visita os filhos antes de calcular o nó atual.
    private static bool ResolverUmPasso(NoExpressao no)
    {
        if (no.Operador == "")
        {
            return false;
        }

        NoExpressao esquerda = no.Esquerda
            ?? throw new FormatException("Operação sem lado esquerdo.");

        if (no.Operador == "()")
        {
            bool resolveu = ResolverUmPasso(esquerda);

            if (esquerda.Operador == "")
            {
                no.Valor = esquerda.Valor;
                no.Operador = "";
                no.Esquerda = null;
            }

            return resolveu;
        }

        NoExpressao direita = no.Direita
            ?? throw new FormatException("Operação sem lado direito.");

        if (ResolverUmPasso(esquerda))
        {
            return true;
        }

        if (ResolverUmPasso(direita))
        {
            return true;
        }

        double resultado;

        if (no.Operador == "+")
        {
            resultado = esquerda.Valor + direita.Valor;
        }
        else if (no.Operador == "-")
        {
            resultado = esquerda.Valor - direita.Valor;
        }
        else if (no.Operador == "*")
        {
            resultado = esquerda.Valor * direita.Valor;
        }
        else
        {
            if (direita.Valor == 0)
            {
                throw new DivideByZeroException(
                    "Não é possível dividir por zero.");
            }

            resultado = esquerda.Valor / direita.Valor;
        }

        if (!double.IsFinite(resultado))
        {
            throw new ArithmeticException(
                "O resultado ultrapassou o limite numérico.");
        }

        // Substitui a operação pelo número calculado.
        no.Valor = resultado;
        no.Operador = "";
        no.Esquerda = null;
        no.Direita = null;

        return true;
    }

    // Reconstrói o texto usando o estado atual da árvore.
    private static string MostrarExpressao(NoExpressao no)
    {
        if (no.Operador == "")
        {
            return no.Valor.ToString(CultureInfo.InvariantCulture);
        }

        NoExpressao esquerda = no.Esquerda
            ?? throw new FormatException("Expressão incompleta.");

        if (no.Operador == "()")
        {
            // Parênteses em torno de um único número são dispensáveis.
            return esquerda.Operador == ""
                ? MostrarExpressao(esquerda)
                : $"({MostrarExpressao(esquerda)})";
        }

        NoExpressao direita = no.Direita
            ?? throw new FormatException("Expressão incompleta.");

        string textoEsquerdo = MostrarExpressao(esquerda);
        string textoDireito = MostrarExpressao(direita);

        // Evita mostrar sequências confusas como 2--3.
        if (esquerda.Operador == "" && esquerda.Valor < 0)
        {
            textoEsquerdo = $"({textoEsquerdo})";
        }

        if (direita.Operador == "" && direita.Valor < 0)
        {
            textoDireito = $"({textoDireito})";
        }

        return textoEsquerdo + no.Operador + textoDireito;
    }

    // Classes ficam depois de todo o código principal e dos métodos.
    class NoExpressao
    {
        public double Valor;
        public string Operador;
        public NoExpressao? Esquerda;
        public NoExpressao? Direita;

        public NoExpressao(double valor)
        {
            Valor = valor;
            Operador = "";
        }

        public NoExpressao(
            string operador,
            NoExpressao esquerda,
            NoExpressao? direita = null)
        {
            Operador = operador;
            Esquerda = esquerda;
            Direita = direita;
        }
    }
}
