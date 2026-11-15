using System.Globalization;

Console.WriteLine("Iniciando calculadora...");
Thread.Sleep(2000);

Console.WriteLine("Calculadora iniciada, acessando menu");
Thread.Sleep(2000);

while (true)
{
    Console.WriteLine();
    Console.WriteLine("=== Calculadora ===");
    Console.WriteLine("Digite 1 para SOMAR");
    Console.WriteLine("Digite 2 para SUBTRAIR");
    Console.WriteLine("Digite 3 para MULTIPLICAR");
    Console.WriteLine("Digite 4 para DIVIDIR");
    Console.WriteLine("Digite 5 para POTENCIAÇÃO");
    Console.WriteLine("Digite 6 para RAIZ QUADRADA");
    Console.WriteLine("Digite 7 para LOGARITMO");
    Console.WriteLine("Digite 8 para EXPRESSÃO MATEMÁTICA");
    Console.WriteLine("Digite 0 para SAIR");

    string? entrada = Console.ReadLine();

    if (entrada is null || entrada == "0")
    {
        break;
    }

    if (entrada == "8")
    {
        Console.Write("Digite a expressão matemática: ");
        string expressao = Console.ReadLine() ?? "";

        try
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

            // Guarda os passos antes de mostrar qualquer resultado.
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

            Console.WriteLine();
            Console.WriteLine("Resolução:");

            for (int i = 0; i < passos.Count; i++)
            {
                Console.WriteLine(i == 0 ? passos[i] : $"= {passos[i]}");
            }
        }
        catch (FormatException erro)
        {
            Console.WriteLine($"Expressão inválida: {erro.Message}");
        }
        catch (DivideByZeroException erro)
        {
            Console.WriteLine(erro.Message);
        }
        catch (ArithmeticException erro)
        {
            Console.WriteLine(erro.Message);
        }

        continue;
    }

    if (entrada != "1" && entrada != "2" && entrada != "3"
        && entrada != "4" && entrada != "5"
        && entrada != "6" && entrada != "7")
    {
        Console.WriteLine("Opção inválida.");
        continue;
    }

    // As opções 1 a 5 usam dois números.
    // Raiz e logaritmo usam apenas um.
    string pergunta = entrada == "5"
        ? "Digite a base: "
        : entrada == "6" || entrada == "7"
            ? "Digite o número: "
            : "Digite o primeiro número: ";

    Console.Write(pergunta);

    if (!double.TryParse(Console.ReadLine(), out double numero1)
        || !double.IsFinite(numero1))
    {
        Console.WriteLine("Número inválido.");
        continue;
    }

    double numero2 = 0;

    if (entrada != "6" && entrada != "7")
    {
        Console.Write(entrada == "5"
            ? "Digite o expoente: "
            : "Digite o segundo número: ");

        if (!double.TryParse(Console.ReadLine(), out numero2)
            || !double.IsFinite(numero2))
        {
            Console.WriteLine("Segundo número inválido.");
            continue;
        }
    }

    double resultado;

    if (entrada == "1")
    {
        resultado = numero1 + numero2;
    }

    else if (entrada == "2")
    {
        resultado = numero1 - numero2;
    }

    else if (entrada == "3")
    {
        resultado = numero1 * numero2;
    }

    else if (entrada == "4")
    {
        if (numero2 == 0)
        {
            Console.WriteLine("Não é possível dividir por zero.");
            continue;
        }

        resultado = numero1 / numero2;
    }

    else if (entrada == "5")
    {
        resultado = Math.Pow(numero1, numero2);
    }

    else if (entrada == "6")
    {
        if (numero1 < 0)
        {
            Console.WriteLine(
                "Não existe raiz quadrada real de número negativo.");
            continue;
        }

        resultado = Math.Sqrt(numero1);
    }
    
    else
    {
        if (numero1 <= 0)
        {
            Console.WriteLine(
                "O número do logaritmo deve ser maior que zero.");
            continue;
        }

        resultado = Math.Log10(numero1);
    }

    if (!double.IsFinite(resultado))
    {
        Console.WriteLine(
            "O cálculo não produziu um resultado real finito.");
        continue;
    }

    Console.WriteLine($"Resultado: {resultado}");
}

Console.WriteLine("Calculadora encerrada.");

// Separa números, operadores e parênteses.
List<string> SepararElementos(string expressao)
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

        if (simbolo >= '0' && simbolo <= '9')
        {
            int inicio = posicao;

            while (posicao < expressao.Length
                   && expressao[posicao] >= '0'
                   && expressao[posicao] <= '9')
            {
                posicao++;
            }

            elementos.Add(expressao.Substring(inicio, posicao - inicio));
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
                "Use inteiros positivos, +, -, *, / e parênteses.");
        }
    }

    return elementos;
}

// Monta os nós de soma e subtração.
NoExpressao LerExpressao(List<string> elementos, ref int indice)
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
NoExpressao LerTermo(List<string> elementos, ref int indice)
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
NoExpressao LerFator(List<string> elementos, ref int indice)
{
    if (indice >= elementos.Count)
    {
        throw new FormatException("Faltou um número na expressão.");
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
            NumberStyles.None,
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
bool ResolverUmPasso(NoExpressao no)
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
string MostrarExpressao(NoExpressao no)
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