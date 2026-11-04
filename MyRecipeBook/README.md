# Executar a API localmente

Execute os comandos abaixo na pasta `MyRecipeBook` que contém `MyRecipeBook.slnx` (após clonar, entre na subpasta `MyRecipeBook`).

## Pré-requisitos

- SDK .NET 10.
- PowerShell 7 para os exemplos abaixo.
- Docker com containers Linux, para SQL Server e testes de integração.
- Porta 1433 livre; se necessário, altere a porta no Docker e na connection string.

## SQL Server local

No PowerShell, defina uma senha exclusiva para o banco local, com maiúsculas, minúsculas, números e símbolos. Não use credenciais de Production.

```powershell
$env:MSSQL_SA_PASSWORD = Read-Host 'Senha do SQL Server local' -MaskInput
docker run -d --name myrecipebook-sql --env ACCEPT_EULA=Y --env MSSQL_SA_PASSWORD -p 127.0.0.1:1433:1433 -v myrecipebook-sql-data:/var/opt/mssql mcr.microsoft.com/mssql/server:2022-CU10-ubuntu-22.04
docker logs myrecipebook-sql
```

Aguarde a mensagem de que o SQL Server aceita conexões. Crie o banco antes de iniciar a API:

```powershell
docker exec --env SQLCMDPASSWORD=$env:MSSQL_SA_PASSWORD myrecipebook-sql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -C -Q "IF DB_ID('MyRecipeBook') IS NULL CREATE DATABASE MyRecipeBook"
```

Para iniciar novamente um container existente, use `docker start myrecipebook-sql`. O volume preserva os dados entre reinicializações. As migrations do FluentMigrator são aplicadas automaticamente ao banco configurado durante o startup; não execute `dotnet ef database update`.

## Configuração de Development

O projeto já possui `UserSecretsId`. Configure apenas os seus valores locais:

```powershell
dotnet user-secrets set 'ConnectionStrings:DefaultConnection' "Server=localhost,1433;Database=MyRecipeBook;User Id=sa;Password=$env:MSSQL_SA_PASSWORD;TrustServerCertificate=True" --project src/Backend/MyRecipeBook.Api
$jwtKey = [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
dotnet user-secrets set 'Jwt:SigningKey' $jwtKey --project src/Backend/MyRecipeBook.Api
dotnet dev-certs https --trust
dotnet restore MyRecipeBook.slnx
dotnet run --project src/Backend/MyRecipeBook.Api --launch-profile https
```

Acesse `https://localhost:7170/swagger`. Cadastre um usuário em `POST /users`, faça login em `POST /authentication/login` e informe o access token em **Authorize**. A renovação usa `POST /authentication/token/refresh`.

User Secrets é destinado a Development e armazena valores fora do repositório, sem criptografá-los. Variáveis de ambiente sobrescrevem os arquivos JSON e User Secrets; remova variáveis antigas que apontem para bancos remotos antes de executar.

Alternativa aos User Secrets, na mesma sessão do PowerShell:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost,1433;Database=MyRecipeBook;User Id=sa;Password=$env:MSSQL_SA_PASSWORD;TrustServerCertificate=True"
$env:Jwt__SigningKey = $jwtKey
```

No Bash, use `export Nome__Da__Chave='valor'`. O .NET converte `__` em `:`. Não versione senhas, chaves ou arquivos com valores locais.

## Configurações e integrações

| Configuração | Necessidade e comportamento atual |
|---|---|
| `ConnectionStrings:DefaultConnection` | Obrigatória. Banco SQL Server existente e acessível; o usuário precisa executar DDL para as migrations. Falhas de conexão ou permissão impedem o startup. |
| `Jwt:SigningKey` | Obrigatória para autenticação. Use uma chave aleatória de pelo menos 32 bytes para HS256. Ausência ou tamanho insuficiente impede os fluxos autenticados. |
| `Jwt:ExpirationTimeMinutes` | Já configurada nos arquivos de ambiente; pode ser sobrescrita por segredo ou variável. |
| `Settings:Google:ClientId`, `Settings:Google:ClientSecret` | OIDC opcional em Development: o esquema só é registrado quando ambos estão preenchidos. As rotas Google exigem essa configuração; sem ela, não as utilize. Não há login Google simulado. |
| `ExternalLoginReturnUrlOptions:Site`, `ExternalLoginReturnUrlOptions:App` | Necessárias para retorno do login externo. Para Google local, cadastre `https://localhost:7170/signin-google` como URI de redirecionamento no seu próprio cliente OAuth. |
| `Settings:OpenAI:EndPoint`, `Settings:OpenAI:ApiKey`, `Settings:OpenAI:Chat:DeploymentName`, `Settings:OpenAI:Image:DeploymentName` | Necessárias somente para gerar receitas com IA. A implementação cria os clientes de chat e imagem sob demanda; sem configuração, esse endpoint falha, mas cadastro/login e CRUD de receitas continuam disponíveis. Use seu próprio endpoint compatível com os clientes OpenAI existentes. |
| `ConnectionStrings:BlobStorage` | O registro atual usa `DisabledStorageService`, inclusive em Production: uploads não são persistidos e URLs são vazias. Preencher a chave não habilita Blob Storage. |
| `ConnectionStrings:ServiceBus` | Necessária para solicitar exclusão de conta. O publisher utiliza a fila `account-deletion`. Sem configuração, não utilize exclusão de conta. O consumidor está comentado em `Program.cs`; configurar a conexão não o inicia. |

Para integrações opcionais, use `dotnet user-secrets set 'Nome:Da:Chave' 'seu-valor' --project src/Backend/MyRecipeBook.Api` ou variáveis com `__`. Não é necessário configurar essas integrações para iniciar a API e usar os fluxos básicos. Não foram adicionados fallbacks nem respostas simuladas.

## Docker da API e Production

O `Dockerfile` existente pode ser reutilizado a partir desta pasta:

```powershell
docker build -t myrecipebook-api .
$env:ConnectionStrings__DefaultConnection = "Server=host.docker.internal,1433;Database=MyRecipeBook;User Id=sa;Password=$env:MSSQL_SA_PASSWORD;TrustServerCertificate=True"
$env:Jwt__SigningKey = $jwtKey
docker run --rm -p 127.0.0.1:8080:8080 --env ASPNETCORE_ENVIRONMENT=Development --env ConnectionStrings__DefaultConnection --env Jwt__SigningKey myrecipebook-api
```

Ao executar a imagem, forneça configurações por variáveis de ambiente; User Secrets do host não são copiados para a imagem. O comando usa o SQL publicado pelo Docker Desktop e expõe Swagger em `http://localhost:8080/swagger`. Em Linux sem Docker Desktop, configure a rede dos containers ou o endereço do host correspondente. Antes de voltar a executar a API diretamente no host, remova essa variável de conexão com `Remove-Item Env:ConnectionStrings__DefaultConnection`, para voltar ao valor local dos User Secrets. O perfil HTTPS acima é a opção local recomendada.

Production mantém seus registros e seu arquivo de configuração existentes; não usa User Secrets. Forneça os valores reais por variáveis de ambiente ou pelo gerenciador de segredos da sua infraestrutura, incluindo JWT e integrações utilizadas. Os valores de exemplo existentes em `appsettings.Production.json` não são credenciais utilizáveis. Este procedimento não provisiona nem acessa Production.

## Validação e problemas comuns

```powershell
dotnet build MyRecipeBook.slnx
dotnet test MyRecipeBook.slnx
```

Os testes de integração usam Testcontainers, bancos descartáveis e suas próprias configurações; exigem Docker disponível e podem baixar imagens de SQL Server e Service Bus. Não precisam dos seus User Secrets nem do banco local.

- Erro no startup: confira se o SQL está pronto, o banco foi criado e a connection string aponta para `localhost`.
- Erro JWT: confira a chave e as variáveis que podem sobrescrevê-la.
- Erro HTTPS: execute `dotnet dev-certs https --trust`.
- Erro em integração opcional: confira a tabela acima; configurar apenas parte das chaves não habilita o fluxo completo.
