# ProcSim: Simulador de Gerenciamento de Processos e Escalonamento

**Uma ferramenta didática de alta fidelidade para o ensino de Sistemas Operacionais, desenvolvida como Trabalho de Conclusão de Curso em Ciência da Computação na FURB.**

<img src="https://i.imgur.com/DaRDGAw.png" alt="Banner do ProcSim" width="800">

[![Licença: GPL v3](https://img.shields.io/badge/Licença-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
[![Build Status](https://github.com/casuffitsharp/procsim/actions/workflows/ci.yml/badge.svg)](https://github.com/casuffitsharp/procsim/actions/workflows/ci.yml)
[![Última Release](https://img.shields.io/github/v/release/casuffitsharp/procsim)](https://github.com/casuffitsharp/procsim/releases/latest)

---

## Índice
1. [Sobre o Projeto](#1-sobre-o-projeto)
2. [Objetivo Pedagógico](#2-objetivo-pedagógico)
3. [Principais Funcionalidades](#3-principais-funcionalidades)
4. [Capturas de Tela](#4-capturas-de-tela)
5. [Tecnologias Utilizadas](#5-tecnologias-utilizadas)
6. [Como Executar o Projeto](#6-como-executar-o-projeto)
7. [Arquitetura do Projeto](#7-arquitetura-do-projeto)
8. [Como Contribuir](#8-como-contribuir)
9. [Licença](#9-licença)

---

## Releases / Download

A versão mais recente do ProcSim, já compilada e pronta para uso no Windows, pode ser encontrada na página de **[Releases](https://github.com/casuffitsharp/procsim/releases)** do repositório.

Basta acessar a última versão, expandir a seção "Assets" e baixar o arquivo .zip que contém o executável.

## 1. Sobre o Projeto

O **ProcSim** é um simulador visual de gerenciamento de processos, desenvolvido para auxiliar no ensino de conceitos complexos de Sistemas Operacionais. A ferramenta aborda a lacuna existente em simuladores educacionais ao oferecer uma **simulação de alta fidelidade**, onde a execução de processos não é um simples atraso de tempo, mas sim o processamento de uma sequência de instruções e micro-operações, emulando de forma mais autêntica o funcionamento de uma CPU.

Com uma interface inspirada no Gerenciador de Tarefas do Windows, o objetivo do ProcSim é transformar a teoria abstrata de escalonamento, concorrência e gerenciamento de recursos em uma experiência visual, interativa e observável.

## 2. Objetivo Pedagógico

O ProcSim foi projetado para ser um laboratório virtual onde conceitos teóricos se tornam eventos mensuráveis. A ferramenta permite que estudantes e professores:

* **Visualizem o Invisível:** Observem em tempo real o ciclo de vida dos processos, a troca de contexto e as decisões do escalonador.
* **Experimentem na Prática:** Configurem cenários complexos com múltiplos núcleos, dispositivos de I/O e cargas de trabalho variadas para analisar o impacto no desempenho do sistema.
* **Analisem Heurísticas Modernas:** Entendam na prática como funcionam mecanismos avançados, como *aging* e *priority boost*, que são implementados em sistemas operacionais comerciais.

O "Apêndice B - Jornada do Usuário" do [artigo de apresentação do projeto](docs/artigo.pdf) serve como um guia prático de como a ferramenta pode ser utilizada em sala de aula.

## 3. Principais Funcionalidades

- **Configuração Flexível:** Permite a criação de cenários de simulação customizados, com controle sobre:
    - **Máquina Virtual:** Número de núcleos de CPU e duração do *quantum*.
    - **Dispositivos de I/O:** Habilitação de dispositivos de I/O, com configuração de canais e latência.
    - **Processos:** Criação detalhada de processos, definindo prioridade estática, operações de CPU e I/O, e comportamento de loop.

- **Dois Modos de Escalonamento:**
    - **Round Robin:** Algoritmo clássico para demonstração de preempção e tempo compartilhado.
    - **Prioridades Híbrido:** Um escalonador avançado que ajusta a prioridade dinâmica com base em heurísticas como *aging*, *boost* de I/O e penalidade de fila.

- **Monitoramento em Tempo Real:** Uma suíte completa de visualização de dados, incluindo:
    - Gráficos de uso de **CPU por núcleo**, separando tempo de usuário e de sistema.
    - Gráficos de uso para cada **canal de dispositivo de I/O**.
    - Grade de **Detalhes** com a lista de todos os processos, seus estados e prioridades.
    - Aba de **Histórico do Processo** para análise detalhada de um único processo ao longo do tempo.

- **Interatividade Dinâmica:** O usuário pode interagir com a simulação para:
    - Alterar a prioridade estática de um processo.
    - Encerrar um processo em execução.
    - Adicionar novas instâncias de processos com a simulação em andamento.

- **Persistência de Cenários:** Capacidade de salvar e carregar configurações em formato JSON, garantindo a reprodutibilidade de experimentos.

## 4. Capturas de Tela

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

## 5. Tecnologias Utilizadas

| Componente | Tecnologia |
| :--- | :--- |
| Backend (Camada Core) | C# 12, .NET 9  |
| Frontend (UI) | WPF, XAML, Padrão MVVM, Material Design  |
| Gráficos | LiveCharts 2  |
| Ambiente de Dev | Visual Studio 2022  |
| Controle de Versão | Git & GitHub  |
| Integração Contínua | GitHub Actions |

## 6. Como Executar o Projeto

> **Nota para Usuários:** Se o seu objetivo é apenas **utilizar** o simulador, recomendamos baixar a versão mais recente diretamente da [página de Releases](https://github.com/casuffitsharp/procsim/releases). As instruções a seguir são para desenvolvedores.

### Pré-requisitos
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) com a carga de trabalho ".NET desktop development" ou IDE equivalente que permita desenvolvimento com WPF.

### Passos para Execução
1. Clone o repositório:
   ```bash
   git clone [https://github.com/casuffitsharp/procsim.git](https://github.com/casuffitsharp/procsim.git)
   ```
2. Abra a solution `ProcSim.sln` no Visual Studio 2022.
3. Defina `ProcSim` como projeto de inicialização.
4. Pressione `F5` para compilar e executar.

## 7. Arquitetura do Projeto

O simulador possui uma arquitetura modular dividida em duas camadas principais, visando a manutenibilidade e a extensibilidade:

-   **`ProcSim.Core`**: A biblioteca de classes que contém o "motor" da simulação. É responsável pelo Kernel, escalonadores, lógica da CPU e dispositivos de I/O. Por ser agnóstica à interface, pode ser reutilizada em outras plataformas no futuro.

-   **`ProcSim`**: O projeto da interface do usuário em WPF. Implementa o padrão MVVM e é responsável por toda a camada de apresentação, consumindo os serviços do `ProcSim.Core`.

Essa separação garante que a lógica da simulação seja independente de sua representação visual.

## 8. Como Contribuir

Este é um projeto de código aberto e toda contribuição é bem-vinda! Se você deseja reportar um bug, sugerir uma melhoria ou enviar código, por favor, leia nosso **[guia de contribuição](CONTRIBUTING.md)** para começar.

## 9. Licença

Este projeto está licenciado sob a Licença Pública Geral GNU v3.0 (GPLv3). Veja o arquivo [`LICENSE`](LICENSE) para mais detalhes. Isso significa que você tem a liberdade de usar, modificar e distribuir este software, inclusive para fins comerciais, contanto que qualquer trabalho derivado que você distribua também seja licenciado sob a GPLv3 e tenha seu código-fonte disponibilizado.