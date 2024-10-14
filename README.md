# swift-api-rest-cs

REST Web API using C# and ASP.NET Core

## Run

Configure project:

```bash
source configure.sh
```

Init database:

```bash
pushd ./swift-api
  dotnet ef migrations add InitialCreate
  dotnet ef database update
popd
```

Run:

```bash
./watch.sh
```

Browse the docs and test the API via the Swagger UI:

```bash
open http://localhost:5000/swagger
```

## How to create a new project

Activate `dotnet`:

```bash
export DOTNET_ROOT=$HOME/.dotnet
export PATH="$HOME/.dotnet:$PATH"
```

#### Create .net solution

```bash
dotnet new sln --name swift-api
```

#### Create C# project

```bash
# add web project
dotnet new web --name swift-api --framework net8.0
dotnet sln add swift-api

# add packages
dotnet add swift-api package DotNetEnv
dotnet add swift-api package Microsoft.EntityFrameworkCore
dotnet add swift-api package Microsoft.EntityFrameworkCore.Design
dotnet add swift-api package Microsoft.EntityFrameworkCore.Sqlite

dotnet add swift-api package Microsoft.AspNetCore.OpenApi
dotnet add swift-api package Swashbuckle.AspNetCore

# add tools
dotnet tool install --global dotnet-ef
```
