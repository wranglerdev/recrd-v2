# PRD — recrd-agile-testing

## 1. Visão Geral

**recrd-agile-testing** é uma aplicação desktop **local-first**, desenvolvida em **.NET 10 + WPF + SQLite**, focada em ambientes corporativos com alta restrição de rede e segurança.

O objetivo da ferramenta é transformar **ações manuais de um usuário em um navegador sandboxado** (cliques, preenchimento de campos, navegações, esperas e validações) em **testes automatizados Robot Framework utilizando Python + Playwright**.

A filosofia central é:

* **YAGNI (You Aren't Gonna Need It)**: apenas funcionalidades necessárias ao fluxo de criação de testes.
* **Zero dependência de cloud**.
* **Tudo auditável localmente**.
* **Compatibilidade total com Windows corporativo**.
* **Integração com autenticação Windows (Active Directory/Kerberos)**.
* **Funcionar offline após instalação inicial**.
* **Experiência de criação de testes em poucos cliques**.

---

# 2. Objetivos do MVP

O MVP deve permitir:

* Criar projetos de testes.
* Importar massa de testes CSV.
* Gravar interações web.
* Organizar testes hierarquicamente.
* Executar testes.
* Visualizar logs.
* Gerar código Robot Framework + Playwright.
* Integrar com repositórios Robot existentes.
* Exportar os artefatos.

Não será implementado no MVP:

* Testes desktop.
* Testes mobile.
* Execução distribuída.
* Integração com pipelines CI/CD.
* Dashboard web.
* Banco centralizado.
* Sincronização entre usuários.

---

# 3. Stack Tecnológica

## Aplicação

| Camada       | Tecnologia                               |
| ------------ | ---------------------------------------- |
| UI           | WPF                                      |
| Linguagem    | C# 14 (.NET 10)                          |
| Banco local  | SQLite                                   |
| ORM          | Entity Framework Core                    |
| Arquivos     | JSON/YAML para export                    |
| Logging      | Serilog                                  |
| DI           | Microsoft.Extensions.DependencyInjection |
| Configuração | appsettings.json                         |

---

## Automação

| Componente | Tecnologia      |
| ---------- | --------------- |
| Framework  | Robot Framework |
| Browser    | Playwright      |
| Linguagem  | Python          |
| Ambiente   | venv local      |

---

# 4. Arquitetura Local First

Cada instalação do recrd possui:

```
C:\Users\<user>\AppData\Local\recrd\
│
├── database.sqlite
├── logs/
│   ├── app.log
│   └── executions/
│
├── exports/
│
├── cache/
│
└── settings.json
```

Não existe servidor.

Todo o histórico pertence ao usuário Windows logado.

---

# 5. Autenticação

## Produção (Windows)

A aplicação utiliza o usuário do Windows:

Exemplo:

```
DOMAIN\jose.silva
```

Informações coletadas:

* Username.
* Nome completo.
* SID do usuário.
* Domínio.

Essas informações serão usadas para:

* Auditoria.
* Histórico de alterações.
* Logs de execução.

Não existe tela de login.

---

# Desenvolvimento Linux (Mock)

Como o desenvolvimento acontece em Linux, será criado um provedor de autenticação fake.

Interface:

```csharp
public interface IUserContext
{
    string Username { get; }
    string DisplayName { get; }
    string Domain { get; }
}
```

Implementação Linux:

```csharp
public class MockUserContext : IUserContext
{
    public string Username => "dev";
    public string DisplayName => "Linux Developer";
    public string Domain => "LOCAL";
}
```

---

# Estrutura para remover o mock

Arquivo:

```
Infrastructure/Auth/MockUserContext.cs
```

Remover:

```
MockUserContext
```

No `Program.cs` alterar:

Antes:

```csharp
services.AddSingleton<IUserContext, MockUserContext>();
```

Depois:

```csharp
services.AddSingleton<IUserContext, WindowsUserContext>();
```

---

# Implementação WindowsUserContext

Usar:

```csharp
WindowsIdentity.GetCurrent()
```

Exemplo:

```csharp
public class WindowsUserContext : IUserContext
{
    private readonly WindowsIdentity _identity;

    public WindowsUserContext()
    {
        _identity = WindowsIdentity.GetCurrent();
    }

    public string Username => _identity.Name;

    public string DisplayName =>
        _identity.Name;

    public string Domain =>
        _identity.Name.Split('\\')[0];
}
```

---

# 6. Modelo de Dados

## Projeto

Representa um conjunto de automações.

```
Projeto
 |
 +-- Plano de teste
       |
       +-- Suite
             |
             +-- Caso de teste
                    |
                    +-- Script manual
                    |
                    +-- Script compilado
                    |
                    +-- Execuções
```

---

## Entidades

### Projeto

```
- Id
- Nome
- Descrição
- CaminhoRobot
- CriadoPor
- CriadoEm
- AlteradoEm
```

---

### Plano de Teste

```
Id
ProjetoId
Nome
Descrição
```

---

### Suite

```
Id
PlanoId
Nome
```

---

### Caso de Teste

```
Id
SuiteId
Nome
Descrição
Status
```

---

### Script

Representação intermediária do teste.

Exemplo:

```json
[
 {
  "action": "click",
  "selector": "#login"
 },
 {
  "action": "input",
  "selector": "#email",
  "value": "{{usuario}}"
 }
]
```

---

### Execução

```
Id
CasoTesteId
Data
Resultado
Usuario
Log
Duração
```

---

# 7. Massa de Testes

Tela dedicada:

```
Massas
```

Suporte ao formato:

```
usuario,senha,email
admin,123456,admin@email.com
```

Regras:

* Quantidade N de colunas.
* Primeira linha é a variável.
* Segunda linha é o valor.
* Permitir editar valores.
* Permitir renomear massa.
* Histórico de importação.

Modelo:

```
Massa
 |
 +-- Variáveis
```

Exemplo interno:

```
usuario -> admin
senha -> 123456
```

---

# 8. Tela Inicial

Objetivo: permitir chegar a uma automação em até 3 cliques.

## Widgets

### Últimas execuções

Exemplo:

```
✔ Login Banco XYZ
Hoje 10:34
1min 20s
```

---

### Ações rápidas

Botões:

```
[ Novo Projeto ]

[ Gravar Novo Teste ]

[ Importar Massa ]

[ Abrir Último Projeto ]
```

---

# 9. Tela de Automação

A tela mais importante do produto.

Layout:

```
-------------------------------------------------
Header
-------------------------------------------------
Play | Pause | Stop | Reload | Exportar | Compilar
-------------------------------------------------

Sidebar              Browser Sandbox

Timeline             [ website ]
Massas
Propriedades
Toggles
Element Inspector
```

---

# Header

## Play

Executa o script.

---

## Pause

Pausa execução.

---

## Stop

Interrompe imediatamente.

---

## Reload

Recarrega a página mantendo a sessão quando possível.

---

## Export

Exporta:

* Script manual JSON.
* Script Robot compilado.

---

## Compilar

Transforma:

```
ações do usuário
```

em:

```
Robot Framework + Playwright
```

---

# 10. Browser Sandbox

O browser é um ambiente controlado.

Deve permitir:

* Captura de cliques.
* Captura de teclado.
* Navegação.
* Captura de URLs.
* Identificação de elementos.

Deve possuir um modo:

```
Inspect
```

Onde ao passar o mouse exibe:

```
Elemento:
<input>

ID:
login

Classes:
form-control

XPath:
...
```

---

# 11. Seletores Inteligentes

A geração de seletores deve seguir prioridade.

Ordem:

1. data-testid
2. aria-label
3. id
4. name
5. role
6. texto visível
7. CSS selector estável
8. XPath

Nunca gerar XPath absoluto.

---

Quando o seletor tiver baixa confiança:

Exemplo:

```
div:nth-child(5)
```

O usuário deve receber alerta:

```
⚠ Elemento com seletor instável.
Escolha um seletor alternativo.
```

---

# 12. Drag and Drop de Massa

Durante gravação:

O usuário pode arrastar:

```
usuario
```

para:

```
<input email>
```

O script será armazenado como:

```
{{usuario}}
```

e não:

```
admin
```

---

# 13. Pipeline de Compilação

Fluxo:

```
Script Manual
        |
        |
Validação de ações
        |
Análise de seletores
        |
Otimização
        |
Geração Robot
        |
Validação sintática
        |
Export
```

---

# 14. Integração com Repositórios Robot

Na criação do projeto:

Opções:

```
( ) Criar novo repositório

( ) Utilizar repositório existente
```

---

## Novo repositório

Estrutura padrão:

```
robot-project/
|
├── tests/
│   └── login.robot
|
├── resources/
|
├── variables/
|
├── data/
|
├── reports/
|
├── requirements.txt
|
└── .gitignore
```

---

# Configuração automática

O recrd deve verificar:

* Python instalado.
* venv existente.
* Robot instalado.
* Browser Playwright instalado.

Caso falte:

```
Instalar ambiente
```

com um clique.

---

# Git

Se existir:

```
.git
```

O aplicativo deve:

* Mostrar branch atual.
* Mostrar arquivos alterados.
* Permitir abrir o diff externo.

Não deve:

* Criar interface de Git complexa.
* Substituir GitHub Desktop ou Git.

---

# 15. Execução e Logs

Toda execução gera:

```
Execution
```

com:

* Usuário.
* Data.
* Duração.
* Resultado.
* Logs.

Exemplo:

```
10:35:01 Click login button

10:35:02 Input username

10:35:04 Assertion successful
```

---

# 16. Auditoria

Todos os objetos devem possuir:

```
CreatedBy
CreatedAt
UpdatedBy
UpdatedAt
```

Eventos importantes:

* Importação de massa.
* Alteração de teste.
* Compilação.
* Exportação.
* Execução.

---

# 17. Exportações

Formatos:

## Script bruto

JSON:

```
login.recrd.json
```

---

## Script compilado

```
login.robot
```

---

## Logs

```
execution-2026-06-20.log
```

---

# 18. Requisitos Não Funcionais

## Performance

* Abrir aplicação em menos de 2 segundos.
* Executar sem internet.
* Suportar projetos com milhares de casos de teste.

---

## Segurança

* Nenhuma comunicação externa.
* Dados armazenados localmente.
* Logs não devem registrar senhas.

---

## Compatibilidade

Sistema suportado:

```
Windows 10
Windows 11
Windows Server 2019+
```

---

# 19. Fluxo ideal do usuário

Criar um teste novo:

```
Home

↓ (1 clique)

Novo Teste

↓ (1 clique)

Abrir Browser Sandbox

↓ (ações naturais)

Arrastar massa se necessário

↓ (1 clique)

Compilar

↓ (1 clique)

Executar
```

---

# 20. Decisões YAGNI

Para manter simplicidade, o MVP **não terá**:

* IA para gerar testes.
* Editor manual de Robot avançado.
* Editor visual de fluxos.
* Sistema de permissões.
* Banco remoto.
* Compartilhamento em tempo real.
* Marketplace de plugins.
* CI/CD integrado.

---

# 21. Visão final

O **recrd-agile-testing** deve se comportar como um **gravador corporativo de automações**:

> O analista testa manualmente uma vez, o recrd registra a intenção, transforma em um teste Robot Framework confiável, mantém a rastreabilidade da ação e permite que o resultado seja versionado em Git — tudo funcionando localmente, sem depender de serviços externos.

O MVP foca em fazer **uma única coisa extremamente bem: converter testes manuais web em automações Robot Framework estáveis e auditáveis**.

## 22. Engenharia de Software, TDD e Qualidade

O desenvolvimento do **recrd-agile-testing** deve seguir uma abordagem **test-first (TDD)** para garantir alta confiabilidade em um ambiente corporativo.

O objetivo é que regras de negócio, geração de scripts Robot Framework, manipulação de massas, validações e integrações sejam previsíveis e facilmente auditáveis.

### Filosofia de Desenvolvimento

O ciclo padrão de desenvolvimento será:

```
Red
 ↓
Escrever um teste que falha

Green
 ↓
Implementar o mínimo necessário para passar

Refactor
 ↓
Melhorar o código mantendo os testes passando
```

Não serão criadas funcionalidades sem antes existir um teste que defina seu comportamento esperado.

---

# 23. Estratégia de Testes

A pirâmide de testes deve seguir:

```
              UI Tests
                 ▲
                 |
         Integration Tests
                 ▲
                 |
            Unit Tests
```

Prioridade:

* Muitos testes unitários.
* Alguns testes de integração.
* Poucos testes de interface.

---

## Testes Unitários

Responsáveis por validar regras de negócio.

Exemplos:

* Conversão de ações manuais em comandos internos.
* Geração de código Robot Framework.
* Priorização de seletores.
* Validação de massas CSV.
* Regras de auditoria.
* Configuração de projetos.
* Tratamento de caminhos de arquivos.

Ferramentas:

* xUnit.
* FluentAssertions.
* NSubstitute.

---

## Testes de Integração

Responsáveis por validar comunicação entre componentes.

Exemplos:

* SQLite funcionando corretamente.
* Repositórios.
* Exportação de arquivos.
* Integração com Git.
* Geração de estrutura de projetos Robot.
* Criação e ativação de venv Python.
* Execução de comandos do Playwright.

Deve ser utilizada uma base SQLite temporária por teste.

---

## Testes de UI

Como WPF possui alto custo de manutenção em automação de interface, serão usados com moderação.

Cobrir apenas fluxos críticos:

* Abrir projeto.
* Criar teste.
* Importar massa.
* Iniciar gravação.
* Compilar script.
* Executar teste.

---

# 24. Estrutura da Solution

Estrutura sugerida:

```
recrd-agile-testing/
│
├── src/
│   │
│   ├── Recrd.App/              WPF
│   ├── Recrd.Domain/           Regras de negócio
│   ├── Recrd.Application/      Casos de uso
│   ├── Recrd.Infrastructure/   SQLite, Git, Robot, Python
│
├── tests/
│   │
│   ├── Recrd.Domain.Tests/
│   ├── Recrd.Application.Tests/
│   ├── Recrd.Infrastructure.Tests/
│   └── Recrd.UI.Tests/
│
├── scripts/
│   ├── build.ps1
│   ├── test.ps1
│   └── release.ps1
│
├── docs/
│   ├── WINDOWS_AUTH_SETUP.md
│   ├── ARCHITECTURE.md
│   └── CONTRIBUTING.md
│
├── .editorconfig
├── .gitignore
├── Directory.Build.props
└── recrd-agile-testing.sln
```

---

# 25. CI/CD

O projeto deve possuir uma pipeline automatizada para garantir que todo artefato `.exe` gerado seja rastreável.

A pipeline deve executar:

```
Checkout
   |
Restore NuGet packages
   |
Static Analysis
   |
Run Unit Tests
   |
Run Integration Tests
   |
Build Release
   |
Publish WPF executable
   |
Generate artifacts
```

---

## Validações obrigatórias

Um build será considerado válido somente se:

* Todos os testes passarem.
* Não existirem warnings críticos.
* A solução compilar em modo Release.
* O executável iniciar corretamente em ambiente Windows de teste.

---

# 26. Build de Release

O produto será distribuído como um executável Windows.

Publicação:

```
dotnet publish
    -c Release
    -r win-x64
    --self-contained true
```

Características:

* Não depende de instalação do .NET Runtime na máquina corporativa.
* Pode ser distribuído internamente via compartilhamento de rede, SCCM, Intune ou ferramenta equivalente.
* Mantém compatibilidade total com Windows.

---

# 27. Versionamento de Releases

Formato:

```
MAJOR.MINOR.PATCH
```

Exemplo:

```
1.3.5
```

Regras:

* MAJOR: alterações incompatíveis.
* MINOR: novas funcionalidades.
* PATCH: correções.

Cada release deve gerar:

```
release/
│
├── recrd-agile-testing.exe
├── CHANGELOG.md
├── SHA256SUM.txt
└── version.json
```

---

# 28. Scripts PowerShell locais

O desenvolvedor deve conseguir realizar todas as operações sem depender da pipeline.

## Testar solução

```powershell
./scripts/test.ps1
```

Fluxo:

```
dotnet restore
dotnet test
```

---

## Build local

```powershell
./scripts/build.ps1
```

Responsável por:

```
Restore
 ↓
Build Debug
 ↓
Executar testes
 ↓
Gerar relatório
```

---

## Release local

```powershell
./scripts/release.ps1
```

Responsável por:

```
Limpar diretórios anteriores
        |
Restaurar dependências
        |
Executar todos os testes
        |
Publicar Release win-x64 self-contained
        |
Gerar checksum SHA256
        |
Organizar pasta release/
```

---

# 29. Compatibilidade com ambiente Linux de desenvolvimento

Apesar da aplicação final ser Windows-only, o código deve ser escrito permitindo desenvolvimento parcial em Linux.

Regras:

* `Domain` e `Application` devem ser 100% multiplataforma.
* Testes unitários devem rodar em Linux.
* Infraestruturas dependentes de Windows devem utilizar abstrações.
* `WindowsUserContext` deve existir apenas no projeto de infraestrutura Windows.
* O mock de autenticação deve ser utilizado no desenvolvimento Linux.

A compilação final da aplicação WPF será realizada em uma etapa Windows da pipeline CI/CD.

---

# 30. Reprodutibilidade e Auditoria de Build

Cada executável gerado deve ser rastreável.

Metadados mínimos:

* Versão da aplicação.
* Data do build.
* Commit Git utilizado.
* Ambiente que gerou o build.

Exemplo:

```json
{
  "version": "1.0.0",
  "gitCommit": "a4f8d22",
  "buildDate": "2026-06-20T14:35:00Z",
  "target": "win-x64"
}
```

A tela **Sobre** do aplicativo deve exibir essas informações para facilitar auditoria em ambientes corporativos.

---

# 31. Princípios de Engenharia Aplicados

O desenvolvimento do recrd deve seguir:

* **TDD** para comportamento.
* **SOLID** para arquitetura.
* **YAGNI** para evitar complexidade desnecessária.
* **KISS** para manter a experiência simples.
* **Clean Architecture** para isolamento das regras de negócio.
* **Dependency Injection** em toda a aplicação.
* **Fail Fast** para erros de configuração.
* **Observabilidade local** através de logs estruturados.

---

Com essa adição, o **recrd-agile-testing** deixa de ser apenas uma ferramenta desktop e passa a ter um ciclo de vida corporativo completo: **código testável, builds reproduzíveis, releases rastreáveis e distribuição simples via `.exe` self-contained para Windows**.
