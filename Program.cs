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
            List<string> passos = MotorCalculadora.Resolver(expressao);

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