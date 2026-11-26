# CalcCSharp 🧮

Calculadora para Windows desenvolvida em C# com WPF, como projeto de aprendizado.

Começou como uma aplicação de console e evoluiu para uma interface visual com interpretador de expressões matemáticas.

## Funcionalidades

- Operações básicas, porcentagem, potência ao quadrado, raiz e inverso.
- Expressões com parênteses, números negativos e decimais.
- Resolução passo a passo em um painel integrado.
- Interface escura com animações e menu lateral.
- Opção de manter a janela sempre visível.

## Como executar

É necessário ter Windows e o SDK .NET 10 instalado.

Na pasta do projeto, execute:

```powershell
dotnet run --project Interface/Calculadora.Visual.csproj
```

## Exemplo

```text
5*(2+3) = 25
```

O painel de resolução mostra as etapas do cálculo.

## Próximas etapas

- Conversores de unidades.
- Calculadora científica.
- Outros modos disponíveis no menu.

Essas opções ainda não estão implementadas.

## Sobre

Projeto pessoal de Daniel Ricarte Almeida, desenvolvido para praticar C#, interfaces gráficas e Git.