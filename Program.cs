Console.WriteLine("Iniciando calculadora...");
Thread.Sleep(2000);

Console.WriteLine("Calculadora iniciada, acessando menu");
Thread.Sleep(2000);

Console.WriteLine("=== Calculadora ===");
Console.WriteLine("Digite 1 para SOMAR");
Console.WriteLine("Digite 2 para SUBTRAIR");
Console.WriteLine("Digite 3 para MULTIPLICAR");
Console.WriteLine("Digite 4 para DIVIDIR");

string? entrada = Console.ReadLine();

if (entrada == "1")
{
    Console.Write("Digite o primeiro número: ");
    double numero1 = double.Parse(Console.ReadLine());

    Console.Write("Digite o segundo número: ");
    double numero2 = double.Parse(Console.ReadLine());

    double resultado = numero1 + numero2;

    Console.WriteLine($"Resultado: {resultado}");
}

else if (entrada == "2")
{
    Console.Write("Digite o primeiro número: ");
    double numero1 = double.Parse(Console.ReadLine());

    Console.Write("Digite o segundo número: ");
    double numero2 = double.Parse(Console.ReadLine());

    double resultado = numero1 - numero2;

    Console.WriteLine($"Resultado: {resultado}");
}

else if (entrada == "3")
{
    Console.Write("Digite o primeiro número: ");
    double numero1 = double.Parse(Console.ReadLine());

    Console.Write("Digite o segundo número: ");
    double numero2 = double.Parse(Console.ReadLine());

    double resultado = numero1 * numero2;

    Console.WriteLine($"Resultado: {resultado}");
}

else if (entrada == "4")
{
    Console.Write("Digite o primeiro número: ");
    double numero1 = double.Parse(Console.ReadLine());

    Console.Write("Digite o segundo número: ");
    double numero2 = double.Parse(Console.ReadLine());

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

else
{
    Console.WriteLine("Opção Inválida");
}