# G24_Busca_EDA2-2026.2

Repositório acadêmico para estudar e implementar a otimização proposta em [`dotnet/aspnetcore#68343`](https://github.com/dotnet/aspnetcore/issues/68343) e realizar o banchmark após as mudanças.

## Objetivo

O `PrefixContainer.GetKeysFromPrefix` atual percorre `_originalValues` linearmente. A issue propõe reutilizar `_sortedValues`, localizar o início da faixa do prefixo com busca binária e percorrer somente as entradas relevantes.

O código-base deste repositório permanece apenas os diretórios mínimos para a implementação e avaliação do professor, sendo realizada a contribuição com o repositorio real dotnet em outro repositorio.

## Pré-requisitos

- Git
- .NET SDK 8.0 ou superior capaz de compilar `net8.0`

Instalação git e asp em ditribuições derivadas de Debian:
```bash
sudo apt install git -y 
sudo apt install -y dotnet-sdk-8.0
```

Instalação git e asp no Windows:

```bash
winget install --id Git.Git -e --source winget
winget install --id Microsoft.DotNet.SDK.8 -e --source winget
```

Verifique em ambos sistemas:

```bash
git --version
dotnet --info
```

## Configuração nas distros Debian e Windows

```bash
git clone https://github.com/eda2-2026/G24_Busca_EDA2-2026.2.git
cd G24_Busca_EDA2-2026.2

dotnet restore G24_Busca_EDA2.sln
dotnet build G24_Busca_EDA2.sln
dotnet test academic/PrefixContainer.Tests/PrefixContainer.Tests.csproj
```

## Referências

- Issue: https://github.com/dotnet/aspnetcore/issues/68343
- PrefixContainer: https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Core/src/ModelBinding/PrefixContainer.cs
- Testes: https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Core/test/ModelBinding/PrefixContainerTest.cs
- Contribuição: https://github.com/dotnet/aspnetcore/blob/main/CONTRIBUTING.md
