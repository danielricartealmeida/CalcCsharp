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

    if (entrada == "0")
    {
        break;
    }

    if (entrada == "1")
    {
        Console.Write("Digite o primeiro número: ");
        if (!double.TryParse(Console.ReadLine(), out double numero1))
        {
            Console.WriteLine("Primeiro número inválido.");
            continue;
        }

        Console.Write("Digite o segundo número: ");
        if (!double.TryParse(Console.ReadLine(), out double numero2))
        {
            Console.WriteLine("Segundo número inválido.");
            continue;
        }

        double resultado = numero1 + numero2;
        Console.WriteLine($"Resultado: {resultado}");
    }

    else if (entrada == "2")
    {
        Console.Write("Digite o primeiro número: ");
        if (!double.TryParse(Console.ReadLine(), out double numero1))
        {
            Console.WriteLine("Primeiro número inválido.");
            continue;
        }

        Console.Write("Digite o segundo número: ");
        if (!double.TryParse(Console.ReadLine(), out double numero2))
        {
            Console.WriteLine("Segundo número inválido.");
            continue;
        }

        double resultado = numero1 - numero2;
        Console.WriteLine($"Resultado: {resultado}");
    }

    else if (entrada == "3")
    {
        Console.Write("Digite o primeiro número: ");
        if (!double.TryParse(Console.ReadLine(), out double numero1))
        {
            Console.WriteLine("Primeiro número inválido.");
            continue;
        }

        Console.Write("Digite o segundo número: ");
        if (!double.TryParse(Console.ReadLine(), out double numero2))
        {
            Console.WriteLine("Segundo número inválido.");
            continue;
        }

        double resultado = numero1 * numero2;
        Console.WriteLine($"Resultado: {resultado}");
    }

    else if (entrada == "4")
    {
        Console.Write("Digite o primeiro número: ");
        if (!double.TryParse(Console.ReadLine(), out double numero1))
        {
            Console.WriteLine("Primeiro número inválido.");
            continue;
        }

        Console.Write("Digite o segundo número: ");
        if (!double.TryParse(Console.ReadLine(), out double numero2))
        {
            Console.WriteLine("Segundo número inválido.");
            continue;
        }

        if (numero2 == 0)
        {
            Console.WriteLine("Não é possível dividir por zero.");
        }
        else
        {
            double resultado = numero1 / numero2;
            Console.WriteLine($"Resultado: {resultado}");
        }
    }

    else if (entrada == "5")
    {
        Console.Write("Digite a base: ");
        if (!double.TryParse(Console.ReadLine(), out double baseNum))
        {
            Console.WriteLine("Base inválida.");
            continue;
        }

        Console.Write("Digite o expoente: ");
        if (!double.TryParse(Console.ReadLine(), out double expoente))
        {
            Console.WriteLine("Expoente inválido.");
            continue;
        }

        double resultado = Math.Pow(baseNum, expoente);
        Console.WriteLine($"Resultado: {resultado}");
    }

    else if (entrada == "6")
    {
        Console.Write("Digite o número: ");
        if (!double.TryParse(Console.ReadLine(), out double numero))
        {
            Console.WriteLine("Número inválido.");
            continue;
        }

        if (numero < 0)
        {
            Console.WriteLine(
                "Não é possível calcular a raiz quadrada real de um número negativo.");
            continue;
        }

        double resultado = Math.Sqrt(numero);
        Console.WriteLine($"Resultado: {resultado}");
    }

    else if (entrada == "7")
    {
        Console.Write("Digite o número: ");
        if (!double.TryParse(Console.ReadLine(), out double numero))
        {
            Console.WriteLine("Número inválido.");
            continue;
        }

        if (numero <= 0)
        {
            Console.WriteLine(
                "Não é possível calcular o logaritmo de um número menor ou igual a zero.");
            continue;
        }

        double resultado = Math.Log10(numero);
        Console.WriteLine($"Resultado: {resultado}");
    }

    else if (entrada == "8")
    {
        Console.Write("Digite a expressão matemática: ");
        string expressao = Console.ReadLine() ?? "";

        int posicao = 0;
        List<string> elementos = new List<string>();

        while (posicao < expressao.Length)
        {
            char simbolo = expressao[posicao];

            if (simbolo >= '0' && simbolo <= '9')
            {
                string numeroTexto = "";

                while (posicao < expressao.Length
                       && expressao[posicao] >= '0'
                       && expressao[posicao] <= '9')
                {
                    numeroTexto += expressao[posicao];
                    posicao++;
                }

                elementos.Add(numeroTexto);
                Console.WriteLine($"Número encontrado: {numeroTexto}");
            }
            else
            {
                if (!char.IsWhiteSpace(simbolo))
                {
                    elementos.Add(simbolo.ToString());
                }

                posicao++;
            }
        }

        Console.WriteLine(
            $"Elementos: {string.Join(" | ", elementos)}");

        int indice = 0;

        try
        {
            double resultado = LerExpressao(elementos, ref indice);

            if (indice != elementos.Count)
            {
                throw new FormatException(
                    $"Elemento inesperado: '{elementos[indice]}'.");
            }

            Console.WriteLine($"Resultado: {resultado}");
        }

        catch (FormatException erro)
        {
            Console.WriteLine(erro.Message);
        }

        catch (DivideByZeroException erro)
        {
            Console.WriteLine(erro.Message);
        }
    }
    
    else
    {
        Console.WriteLine("Opção inválida.");
    }
}

Console.WriteLine("Calculadora encerrada.");

double LerNumero(List<string> elementos, ref int indice)
{
    if (indice >= elementos.Count)
    {
        throw new FormatException("Faltou um número na expressão.");
    }

    if (!double.TryParse(elementos[indice], out double numero))
    {
        throw new FormatException(
            $"Era esperado um número, mas foi encontrado '{elementos[indice]}'.");
    }

    indice++;

    return numero;
}

double LerExpressao(List<string> elementos, ref int indice)
{
    double resultado = LerTermo(elementos, ref indice);

    while (indice < elementos.Count
           && (elementos[indice] == "+" || elementos[indice] == "-"))
    {
        string operador = elementos[indice];
        indice++;

        double proximoTermo = LerTermo(elementos, ref indice);

        if (operador == "+")
        {
            resultado += proximoTermo;
        }
        
        else
        {
            resultado -= proximoTermo;
        }
    }

    return resultado;
}

double LerTermo(List<string> elementos, ref int indice)
{
    double resultado = LerFator(elementos, ref indice);

    while (indice < elementos.Count
           && (elementos[indice] == "*" || elementos[indice] == "/"))
    {
        string operador = elementos[indice];
        indice++;

        double proximoNumero = LerFator(elementos, ref indice);

        if (operador == "*")
        {
            resultado *= proximoNumero;
        }

        else
        {
            if (proximoNumero == 0)
            {
                throw new DivideByZeroException(
                    "Não é possível dividir por zero.");
            }

            resultado /= proximoNumero;
        }
    }

    return resultado;
}

double LerFator(List<string> elementos, ref int indice)
{
    if (indice >= elementos.Count)
    {
        throw new FormatException("Faltou um número na expressão.");
    }

    if (elementos[indice] == "(")
    {
        indice++;
        double resultado = LerExpressao(elementos, ref indice);

        if (indice >= elementos.Count || elementos[indice] != ")")
        {
            throw new FormatException("Faltou um parêntese de fechamento.");
        }

        indice++;
        return resultado;
    }

    return LerNumero(elementos, ref indice);
}