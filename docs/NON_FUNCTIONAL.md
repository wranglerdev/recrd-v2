# Requisitos Não Funcionais (PRD §18)

Rastreabilidade de como cada requisito não funcional é atendido — para auditoria
corporativa.

## Performance

| Requisito | Como é atendido |
| --------- | --------------- |
| Abrir em < 2s | Composition root mínimo (`AddRecrdInfrastructure`), publish self-contained single-file. |
| Rodar sem internet | Tudo local: SQLite + arquivos. Sem clientes HTTP (ver guarda abaixo). |
| Milhares de casos | Hierarquia normalizada por FK no SQLite; consultas via EF Core. |

## Segurança

| Requisito | Como é atendido |
| --------- | --------------- |
| Nenhuma comunicação externa | `OfflineGuardTests` falha o build se Domain/Application referenciarem `System.Net.Http`/`Sockets`. |
| Dados armazenados localmente | `RecrdPaths` em `%LOCALAPPDATA%/recrd` (PRD §4); sem servidor. |
| Logs não registram senhas | `AuditTrail` loga apenas metadados (nomes, contagens, ids, resultado) — nunca valores de massa. |

## Compatibilidade

| Requisito | Como é atendido |
| --------- | --------------- |
| Windows 10 / 11 / Server 2019+ | TFM `net10.0-windows`, publish `win-x64 --self-contained` (não exige .NET Runtime na máquina). |

## Desenvolvimento em Linux

`Domain` e `Application` são 100% multiplataforma (PRD §29) e cobertos a 100%
linha/branch no CI. O WPF compila no Linux via `EnableWindowsTargeting`, mas só
executa no Windows (etapa Windows da pipeline).
