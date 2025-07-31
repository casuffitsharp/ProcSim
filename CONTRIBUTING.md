# Guia de Contribuição para o ProcSim

Agradecemos o interesse em contribuir com o projeto ProcSim. Este documento estabelece as diretrizes para a contribuição, visando manter a organização e a qualidade do projeto.

## Tipos de Contribuição

Contribuições são bem-vindas em diversas formas:

* **Relatos de Bugs:** Caso encontre um comportamento inesperado ou um erro, por favor, abra uma nova [Issue](https://github.com/casuffitsharp/procsim/issues) descrevendo o problema, os passos para reproduzi-lo e o resultado esperado.
* **Sugestões de Melhorias:** Novas ideias para funcionalidades ou aprimoramentos de recursos existentes podem ser propostas através de uma [Issue](https://github.com/casuffitsharp/procsim/issues).
* **Desenvolvimento de Código:** Contribuições de código para corrigir bugs ou implementar novas funcionalidades são incentivadas. Para isso, siga o fluxo de trabalho descrito abaixo.

## Fluxo de Trabalho para Contribuição de Código

Para garantir a integração organizada das alterações, o projeto utiliza o modelo de Fork e Pull Request.

1.  **Fork:** Crie um "fork" do repositório oficial para a sua conta pessoal no GitHub.

2.  **Clone:** Clone o seu fork para o ambiente de desenvolvimento local.
    ```bash
    git clone [https://github.com/SEU-USUARIO/procsim.git](https://github.com/SEU-USUARIO/procsim.git)
    ```

3.  **Branch:** Crie uma nova branch a partir da `main` para isolar suas alterações. Utilize um nome descritivo.
    ```bash
    git checkout -b feature/nome-da-funcionalidade
    ```

4.  **Desenvolvimento:** Realize as modificações no código. Atente-se à arquitetura do projeto:
    * **`ProcSim.Core`**: Contém a lógica central da simulação. Não deve possuir dependências ou código relacionado à interface de usuário.
    * **`ProcSim`**: Contém a aplicação WPF (interface de usuário), implementando o padrão MVVM.

5.  **Commit e Push:** Adicione suas alterações ao stage, crie um commit com uma mensagem clara e envie a branch para o seu fork.
    ```bash
    git commit -m "feat: Implementa a funcionalidade X"
    git push origin feature/nome-da-funcionalidade
    ```

6.  **Pull Request (PR):** A partir da página do seu fork no GitHub, crie um "Pull Request" direcionado à branch `main` do repositório original. Preencha o template do PR com uma descrição clara das alterações e da motivação para elas.

## Padrões de Código

* **Estilo de Código:** O projeto adota as convenções de estilo padrão do C# e .NET. O arquivo `.editorconfig` na raiz do projeto auxilia na manutenção da consistência.
* **Mensagens de Commit:** Recomenda-se seguir o padrão [Conventional Commits](https://www.conventionalcommits.org/) para as mensagens de commit, a fim de manter um histórico claro e legível.

Agradecemos por sua contribuição para o aprimoramento do ProcSim.