# ProcSim: Simulador de Gerenciamento de Processos e Escalonamento

**Uma ferramenta didática de alta fidelidade para o ensino de Sistemas Operacionais, desenvolvida como Trabalho de Conclusão de Curso em Ciência da Computação na FURB.**

<img src="https://i.imgur.com/DaRDGAw.png" alt="Banner do ProcSim" width="800">

[![Licença MIT](https://img.shields.io/badge/Licença-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://github.com/casuffitsharp/procsim/actions/workflows/ci.yml/badge.svg)](https://github.com/casuffitsharp/procsim/actions/workflows/ci.yml)
[![Última Release](https://img.shields.io/github/v/release/casuffitsharp/procsim)](https://github.com/casuffitsharp/procsim/releases/latest)

---

## Índice
1. [Sobre o Projeto](#1-sobre-o-projeto)
2. [Principais Funcionalidades](#2-principais-funcionalidades)
3. [Capturas de Tela](#3-capturas-de-tela)
4. [Tecnologias Utilizadas](#4-tecnologias-utilizadas)
5. [Como Executar o Projeto](#5-como-executar-o-projeto)
6. [Arquitetura do Projeto](#6-arquitetura-do-projeto)
7. [Licença](#7-licença)

---

## Releases / Download

A versão mais recente do ProcSim, já compilada e pronta para uso no Windows, pode ser encontrada na página de **[Releases](https://github.com/casuffitsharp/procsim/releases)** do repositório.

Basta acessar a última versão, expandir a seção "Assets" e baixar o arquivo executável.

## 1. Sobre o Projeto

O **ProcSim** é um simulador visual de gerenciamento de processos, desenvolvido para auxiliar no ensino de conceitos complexos de Sistemas Operacionais. A ferramenta aborda a lacuna existente em simuladores educacionais ao oferecer uma **simulação de alta fidelidade**, onde a execução de processos não é um simples atraso de tempo, mas sim o processamento de uma sequência de instruções e micro-operações, emulando de forma mais autêntica o funcionamento de uma CPU.

Com uma interface inspirada no Gerenciador de Tarefas do Windows, o objetivo do ProcSim é transformar a teoria abstrata de escalonamento, concorrência e gerenciamento de recursos em uma experiência visual, interativa e observável, permitindo que estudantes e entusiastas possam experimentar e entender o impacto de diferentes algoritmos e configurações de sistema.

## 2. Principais Funcionalidades

- **Configuração Flexível:** Permite a criação de cenários de simulação customizados, com controle sobre:
    - **Máquina Virtual:** Número de núcleos de CPU e duração do *quantum*.
    - **Dispositivos de I/O:** Habilitação de dispositivos como Disco e USB, com configuração de canais e latência.
    - **Processos:** Criação detalhada de processos, definindo prioridade estática, operações de CPU e I/O, e comportamento de loop (finito, infinito ou aleatório).

- **Dois Modos de Escalonamento:**
    - **Round Robin:** Algoritmo clássico para demonstração de preempção e tempo compartilhado.
    - **Prioridades Híbrido:** Um escalonador avançado inspirado em sistemas operacionais comerciais, que ajusta a prioridade dinâmica dos processos com base em heurísticas como:
        - ***Aging* (Envelhecimento):** Para evitar a inanição de processos de baixa prioridade.
        - ***Boost* de Prioridade:** Para processos *I/O-bound*, melhorando a responsividade do sistema.
        - **Penalidade de Fila:** Para gerenciar a contenção na fila de prontos.

- **Monitoramento em Tempo Real:** Uma suíte completa de visualização de dados, incluindo:
    - Gráficos de uso de **CPU por núcleo**, separando tempo de usuário e de sistema.
    - Gráficos de uso para cada **canal de dispositivo de I/O**.
    - Grade de **Detalhes** com a lista de todos os processos, seus estados, PIDs e prioridades.
    - Aba de **Histórico do Processo** para análise detalhada de um único processo ao longo do tempo.

- **Interatividade Dinâmica:** O usuário pode interagir com a simulação em tempo real para:
    - Alterar a prioridade estática de um processo.
    - Encerrar um processo em execução.
    - Adicionar novas instâncias de processos configurados com a simulação em andamento.

- **Persistência de Cenários:** Capacidade de salvar e carregar configurações completas da VM e dos processos em arquivos formato JSON, garantindo a reprodutibilidade de experimentos e cenários de aula.

## 3. Capturas de Tela

<table>
  <tr>
    <td align="center"><b>Tela de Configuração</b></td>
    <td align="center"><b>Monitoramento de CPU</b></td>
  </tr>
  <tr>
    <td><img src="https://i.imgur.com/DaRDGAw.png" alt="Tela de Configuração do ProcSim" width="500"/></td>
    <td><img src="https://i.imgur.com/aRUzVYQ.png" alt="Gráficos de CPU" width="500"/></td>
  </tr>
  <tr>
    <td align="center"><b>Aba de Detalhes</b></td>
    <td align="center"><b>Histórico do Processo</b></td>
  </tr>
  <tr>
    <td><img src="https://i.imgur.com/g4LrBFz.png" alt="Aba de Detalhes" width="500"/></td>
    <td><img src="https://i.imgur.com/VtMFCT2.png" alt="Aba de Histórico do Processo" width="500"/></td>
  </tr>
</table>

## 4. Tecnologias Utilizadas

| Componente | Tecnologia |
| :--- | :--- |
| Backend (Camada Core) | C# 12, .NET 9 |
| Frontend (UI) | WPF, XAML, Padrão MVVM, Material Design |
| Gráficos | LiveCharts 2 |
| Ambiente de Dev | Visual Studio 2022 |
| Controle de Versão | Git & GitHub |
| Integração Contínua | GitHub Actions |

## 5. Como Executar o Projeto

> **Nota para Usuários:** Se o seu objetivo é apenas **utilizar** o simulador, recomendamos baixar a versão mais recente diretamente da [página de Releases](https://github.com/casuffitsharp/procsim/releases), sem a necessidade de seguir os passos abaixo. As instruções a seguir são para desenvolvedores que desejam compilar o código-fonte.

### Pré-requisitos
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) com a carga de trabalho ".NET desktop development".

### Passos para Execução
1. Clone o repositório para a sua máquina local:
   ```bash
   git clone [https://github.com/casuffitsharp/procsim.git](https://github.com/casuffitsharp/procsim.git)
2. Abra a solution `ProcSim.sln` no Visual Studio 2022.
3. Defina o projeto `ProcSim` como projeto de inicialização (*Startup Project*).
4. Pressione `F5` ou clique no botão "Start" para compilar e executar a aplicação.

## 5. Arquitetura do Projeto

O simulador é projetado com uma arquitetura modular e desacoplada, dividida em duas soluções principais, visando a manutenibilidade e a extensibilidade:

-   **`ProcSim.Core`**: Esta é a biblioteca de classes que contém todo o "motor" da simulação. Ela é responsável pelo Kernel, pelos escalonadores, pela lógica da CPU e dos dispositivos de I/O. Por não ter nenhuma dependência de UI, este núcleo pode ser reutilizado em outras plataformas (ex: web, mobile) no futuro.

-   **`ProcSim`**: Este é o projeto da interface do usuário, desenvolvido em WPF. Ele implementa o padrão MVVM e é responsável por toda a camada de apresentação, consumindo os serviços e os dados fornecidos pelo `ProcSim.Core`.

Essa separação garante que a lógica complexa da simulação seja independente de sua representação visual.

## 6. Licença

Este projeto está licenciado sob a **Licença MIT**. Veja o arquivo `LICENSE` para mais detalhes.