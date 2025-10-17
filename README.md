# mypldotnet

### Step-by-Step Project: 

A Simple and Detailed Integration of C#, Entity Framework Core, PostgreSQL, and a PL/pgSQL Function.

This project will be a .NET console application designed to manage a product catalog.

What We’ll Do

##### * Set Up the Database: Create a products table in PostgreSQL.

##### * Write a PL/pgSQL Function: Create a function that calculates the total inventory value.

##### * Create the C# Project: Set up a console application.

##### * Integrate with Entity Framework Core: Map the products table to a C# class, configure the connection, and set up the DbContext.

##### * Develop CRUD Operations: Implement Create, Read, Update, and Delete operations using EF Core.

##### * Call the PL/pgSQL Function: Execute the custom PostgreSQL function from C# and retrieve the result.

### Prerequisites

Before starting, be sure to have the following installed:

##### * .NET 8 SDK (or >): https://dotnet.microsoft.com/download

##### * PostgreSQL: https://www.postgresql.org/download/

##### * A data base administrator, like pgAdmin is a plus.


### Part 1: The Database (PostgreSQL and PL/pgSQL)

First, create a database, table, and function directly in PostgreSQL using psql CLI.
##### * Create a Database: Using pgAdmin or psql, create a new database. It’ll call it 'postgres'.
##### * Create the Products Table: Once conncted in the new db, Rrun the following SQL script to create the table that will store our products.    

```SQL
CREATE TABLE products (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    price NUMERIC(10, 2) NOT NULL,
    stock_quantity INT NOT NULL,
    registration_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Insert +100 itens
INSERT INTO products (name, price, stock_quantity) VALUE
('Laptop Gamer', 7500.50, 10),
('Wireless Mouse', 150.75, 50),
('Mechanical keyboard XPTO', 410, 35),
('Laptop Gamer gen.2', 7500.50, 10),
('Wireless Mouse', 150.75, 50),
.
.
('Portable Bluetooth Radio', 270.00, 20),
('Noise Cancelling Earbuds', 699.00, 22),
('Smart Ceiling Light', 380.00, 14);

```

### Create the PL/pgSQL Function:

Now, let’s create a function that calculates the total value of all products in stock (price × quantity).
SQL:

    CREATE OR REPLACE FUNCTION fn_calcular_valor_total_estoque()
    RETURNS NUMERIC AS $$
    DECLARE
        valor_total NUMERIC := 0;
    BEGIN
        -- Calcula a soma de (preço * quantidade) para todos os produtos
        SELECT SUM(preco * quantidade_estoque)
        INTO valor_total
        FROM produtos;

        -- Retorna o valor calculado
        RETURN COALESCE(valor_total, 0);
    END;
    $$ LANGUAGE plpgsql;


Now the database configuration and actions are finished by now.

### Part 2: C# App  with Entity Framework Core

Agora vamos para o código C#.

    Crie um novo projeto de Console: Abra seu terminal ou prompt de comando e execute:
    Bash

dotnet new console -o GerenciadorDeProdutosApp
cd GerenciadorDeProdutosApp

Instale os Pacotes NuGet Necessários: Precisamos do driver do PostgreSQL para o EF Core.
Bash

dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design

    Npgsql.EntityFrameworkCore.PostgreSQL: O provedor de banco de dados que permite ao EF Core se comunicar com o PostgreSQL.

    Microsoft.EntityFrameworkCore.Design: Contém ferramentas de linha de comando para o EF Core (útil para migrações, embora não vamos gerá-las aqui, é uma boa prática incluí-lo).

Crie a Classe de Modelo (Entity): Crie um novo arquivo chamado Produto.cs. Esta classe será o "espelho" da nossa tabela produtos no C#.
C#

// Produto.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("produtos")] // Mapeia esta classe para a tabela "produtos"
public class Produto
{
    [Key] // Marca a propriedade como chave primária
    [Column("id")]
    public int Id { get; set; }

    [Column("nome")]
    public string Nome { get; set; }

    [Column("preco")]
    public decimal Preco { get; set; }

    [Column("quantidade_estoque")]
    public int QuantidadeEstoque { get; set; }

    [Column("data_cadastro")]
    public DateTime DataCadastro { get; set; }
}

    Explicação:

        [Table("produtos")]: Informa ao EF Core que esta classe corresponde à tabela produtos.

        [Key]: Define Id como a chave primária.

        [Column("...")]: Mapeia cada propriedade para sua respectiva coluna no banco de dados. Isso é importante para seguir a convenção de nomes do C# (PascalCase) e do SQL (snake_case).

Crie o Contexto do Banco de Dados (DbContext): O DbContext é a ponte entre suas classes C# e o banco de dados. Crie um novo arquivo chamado AppDbContext.cs.
C#

    // AppDbContext.cs
    using Microsoft.EntityFrameworkCore;

    public class AppDbContext : DbContext
    {
        // DbSet representa a coleção de todas as entidades no contexto,
        // ou seja, uma representação da tabela "produtos".
        public DbSet<Produto> Produtos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Substitua com sua string de conexão real
            var connectionString = "Host=localhost;Database=gerenciador_produtos;Username=seu_usuario;Password=sua_senha";
            optionsBuilder.UseNpgsql(connectionString);
        }
    }

        Explicação:

            public DbSet<Produto> Produtos { get; set; }: Cria uma propriedade que o EF Core usará para interagir com a tabela produtos. Você fará consultas como context.Produtos.ToList().

            OnConfiguring: Este método configura a conexão com o banco de dados.

            IMPORTANTE: Altere seu_usuario e sua_senha para as credenciais do seu banco de dados PostgreSQL.

Parte 3: Mão na Massa! O Código Principal

Agora vamos editar o arquivo Program.cs para realizar as operações CRUD e chamar nossa função.
C#

// Program.cs
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("--- Gerenciador de Produtos com EF Core e PostgreSQL ---");

        // Usando 'using' para garantir que a conexão com o banco seja fechada corretamente.
        using (var context = new AppDbContext())
        {
            // Garante que o banco de dados está acessível.
            // Não cria o banco, apenas testa a conexão.
            context.Database.EnsureCreated();

            // 1. CREATE: Adicionando um novo produto
            Console.WriteLine("\n>> Inserindo um novo produto...");
            var novoProduto = new Produto
            {
                Nome = "Monitor Ultrawide",
                Preco = 2300.00m,
                QuantidadeEstoque = 15
            };
            context.Produtos.Add(novoProduto);
            context.SaveChanges(); // Salva as mudanças no banco de dados
            Console.WriteLine("Produto inserido com sucesso!");

            // 2. READ: Lendo e exibindo todos os produtos
            Console.WriteLine("\n>> Listando todos os produtos:");
            var todosOsProdutos = context.Produtos.OrderBy(p => p.Nome).ToList();
            foreach (var produto in todosOsProdutos)
            {
                Console.WriteLine($"ID: {produto.Id}, Nome: {produto.Nome}, Preço: {produto.Preco:C}, Estoque: {produto.QuantidadeEstoque}");
            }

            // 3. UPDATE: Atualizando um produto
            Console.WriteLine("\n>> Atualizando o preço do 'Mouse sem Fio'...");
            var produtoParaAtualizar = context.Produtos.FirstOrDefault(p => p.Nome == "Mouse sem Fio");
            if (produtoParaAtualizar != null)
            {
                produtoParaAtualizar.Preco = 165.50m; // Novo preço
                context.SaveChanges();
                Console.WriteLine("Preço atualizado!");
            }

            // 4. DELETE: Removendo um produto
            Console.WriteLine("\n>> Deletando o 'Monitor Ultrawide' recém-criado...");
            var produtoParaDeletar = context.Produtos.FirstOrDefault(p => p.Nome == "Monitor Ultrawide");
            if (produtoParaDeletar != null)
            {
                context.Produtos.Remove(produtoParaDeletar);
                context.SaveChanges();
                Console.WriteLine("Produto deletado!");
            }

            // 5. CHAMANDO A FUNÇÃO PL/pgSQL
            Console.WriteLine("\n>> Calculando o valor total do estoque com a função PL/pgSQL...");

            // Para chamar uma função scalar (que retorna um único valor),
            // a forma mais simples e segura é usar SQL puro com EF Core.
            decimal valorTotalEstoque = 0;

            // Abre a conexão gerenciada pelo EF Core
            var connection = context.Database.GetDbConnection();
            connection.Open();

            // Cria um comando para executar a função
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT fn_calcular_valor_total_estoque();";
                var result = command.ExecuteScalar(); // Executa e retorna o primeiro valor da primeira linha

                if (result != null && result != DBNull.Value)
                {
                    valorTotalEstoque = Convert.ToDecimal(result);
                }
            }
            connection.Close(); // Fecha a conexão

            Console.WriteLine($"O valor total do estoque é: {valorTotalEstoque:C}");
        }

        Console.WriteLine("\n--- Fim da Execução ---");
    }
}

    Explicação do Código Principal:

        using (var context = new AppDbContext()): Instancia nosso contexto. O using garante que os recursos de conexão sejam liberados ao final.

        context.Add(): Adiciona uma nova entidade para ser rastreada pelo EF Core.

        projeto passo a passo, bem detalhado e simples, que integra C#, Entity Framework Core, PostgreSQL e uma função em PL/pgSQL.

O nosso projeto será uma aplicação de console (.NET) para gerenciar um catálogo de produtos.

O que vamos fazer:

    Configurar o Banco de Dados: Criar uma tabela produtos no PostgreSQL.

    Escrever uma Função em PL/pgSQL: Criar uma função que calcula o valor total do estoque.

    Criar o Projeto C#: Configurar uma aplicação de console.

    Integrar com Entity Framework Core: Mapear a tabela produtos para uma classe C#, configurar a conexão e o DbContext.

    Desenvolver as Operações (CRUD): Criar, Ler, Atualizar e Deletar produtos usando EF Core.

    Chamar a Função PL/pgSQL: Executar a nossa função customizada a partir do C# e obter o resultado.

Pré-requisitos

Antes de começar, certifique-se de que você tem o seguinte instalado:

    .NET 8 SDK (ou superior): https://dotnet.microsoft.com/download

    PostgreSQL: https://www.postgresql.org/download/

    Um editor de código: Visual Studio 2022, JetBrains Rider ou Visual Studio Code.

    Uma ferramenta para gerenciar o Postgres: pgAdmin (geralmente vem com a instalação) ou DBeaver.

Parte 1: O Banco de Dados (PostgreSQL e PL/pgSQL)

Primeiro, vamos criar nossa base, a tabela e a função diretamente no PostgreSQL.

    Crie um Banco de Dados: Usando o pgAdmin ou psql, crie um novo banco de dados. Vamos chamá-lo de gerenciador_produtos.

    Crie a Tabela de Produtos: Execute o seguinte script SQL no seu banco de dados gerenciador_produtos para criar a tabela que armazenará nossos produtos.
    SQL

CREATE TABLE produtos (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    preco NUMERIC(10, 2) NOT NULL,
    quantidade_estoque INT NOT NULL,
    data_cadastro TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Inserindo alguns dados de exemplo (opcional)
INSERT INTO produtos (nome, preco, quantidade_estoque) VALUES
('Laptop Gamer', 7500.50, 10),
('Mouse sem Fio', 150.75, 50),
('Teclado Mecânico', 450.00, 25);

    Explicação:

        id SERIAL PRIMARY KEY: Uma chave primária que se auto-incrementa.

        nome VARCHAR(100): O nome do produto.

        preco NUMERIC(10, 2): O preço, ideal para valores monetários.

        quantidade_estoque INT: A quantidade de itens em estoque.

        data_cadastro TIMESTAMP: A data e hora em que o registro foi criado, com valor padrão para o momento atual.

Crie a Função em PL/pgSQL: Agora, vamos criar uma função que calcula o valor total de todos os produtos em estoque (preço * quantidade).
SQL

    CREATE OR REPLACE FUNCTION fn_calcular_valor_total_estoque()
    RETURNS NUMERIC AS $$
    DECLARE
        valor_total NUMERIC := 0;
    BEGIN
        -- Calcula a soma de (preço * quantidade) para todos os produtos
        SELECT SUM(preco * quantidade_estoque)
        INTO valor_total
        FROM produtos;

        -- Retorna o valor calculado
        RETURN COALESCE(valor_total, 0);
    END;
    $$ LANGUAGE plpgsql;

        Explicação:

            CREATE OR REPLACE FUNCTION ...: Define o início da nossa função.

            RETURNS NUMERIC: Especifica que a função retornará um valor numérico.

            DECLARE valor_total NUMERIC := 0;: Declara uma variável local para armazenar o resultado.

            SELECT SUM(...) INTO valor_total FROM produtos;: Este é o coração da função. Ele executa a consulta de agregação e armazena o resultado na nossa variável valor_total.

            RETURN COALESCE(valor_total, 0);: Retorna o valor total. COALESCE é usado para garantir que, se a tabela estiver vazia e o SUM retornar NULL, a função retorne 0 em vez de NULL.

            $$ LANGUAGE plpgsql;: Define que a linguagem usada na função é PL/pgSQL.

Pronto! A parte do banco de dados está configurada.

Parte 2: A Aplicação C# com Entity Framework Core

Agora vamos para o código C#.

    Crie um novo projeto de Console: Abra seu terminal ou prompt de comando e execute:
    Bash

dotnet new console -o GerenciadorDeProdutosApp
cd GerenciadorDeProdutosApp

Instale os Pacotes NuGet Necessários: Precisamos do driver do PostgreSQL para o EF Core.
Bash

dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design

    Npgsql.EntityFrameworkCore.PostgreSQL: O provedor de banco de dados que permite ao EF Core se comunicar com o PostgreSQL.

    Microsoft.EntityFrameworkCore.Design: Contém ferramentas de linha de comando para o EF Core (útil para migrações, embora não vamos gerá-las aqui, é uma boa prática incluí-lo).

Crie a Classe de Modelo (Entity): Crie um novo arquivo chamado Produto.cs. Esta classe será o "espelho" da nossa tabela produtos no C#.
C#

// Produto.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("produtos")] // Mapeia esta classe para a tabela "produtos"
public class Produto
{
    [Key] // Marca a propriedade como chave primária
    [Column("id")]
    public int Id { get; set; }

    [Column("nome")]
    public string Nome { get; set; }

    [Column("preco")]
    public decimal Preco { get; set; }

    [Column("quantidade_estoque")]
    public int QuantidadeEstoque { get; set; }

    [Column("data_cadastro")]
    public DateTime DataCadastro { get; set; }
}

    Explicação:

        [Table("produtos")]: Informa ao EF Core que esta classe corresponde à tabela produtos.

        [Key]: Define Id como a chave primária.

        [Column("...")]: Mapeia cada propriedade para sua respectiva coluna no banco de dados. Isso é importante para seguir a convenção de nomes do C# (PascalCase) e do SQL (snake_case).

Crie o Contexto do Banco de Dados (DbContext): O DbContext é a ponte entre suas classes C# e o banco de dados. Crie um novo arquivo chamado AppDbContext.cs.
C#

    // AppDbContext.cs
    using Microsoft.EntityFrameworkCore;

    public class AppDbContext : DbContext
    {
        // DbSet representa a coleção de todas as entidades no contexto,
        // ou seja, uma representação da tabela "produtos".
        public DbSet<Produto> Produtos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Substitua com sua string de conexão real
            var connectionString = "Host=localhost;Database=gerenciador_produtos;Username=seu_usuario;Password=sua_senha";
            optionsBuilder.UseNpgsql(connectionString);
        }
    }

        Explicação:

            public DbSet<Produto> Produtos { get; set; }: Cria uma propriedade que o EF Core usará para interagir com a tabela produtos. Você fará consultas como context.Produtos.ToList().

            OnConfiguring: Este método configura a conexão com o banco de dados.

            IMPORTANTE: Altere seu_usuario e sua_senha para as credenciais do seu banco de dados PostgreSQL.

Parte 3: Mão na Massa! O Código Principal

Agora vamos editar o arquivo Program.cs para realizar as operações CRUD e chamar nossa função.
C#

// Program.cs
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("--- Gerenciador de Produtos com EF Core e PostgreSQL ---");

        // Usando 'using' para garantir que a conexão com o banco seja fechada corretamente.
        using (var context = new AppDbContext())
        {
            // Garante que o banco de dados está acessível.
            // Não cria o banco, apenas testa a conexão.
            context.Database.EnsureCreated();

            // 1. CREATE: Adicionando um novo produto
            Console.WriteLine("\n>> Inserindo um novo produto...");
            var novoProduto = new Produto
            {
                Nome = "Monitor Ultrawide",
                Preco = 2300.00m,
                QuantidadeEstoque = 15
            };
            context.Produtos.Add(novoProduto);
            context.SaveChanges(); // Salva as mudanças no banco de dados
            Console.WriteLine("Produto inserido com sucesso!");

            // 2. READ: Lendo e exibindo todos os produtos
            Console.WriteLine("\n>> Listando todos os produtos:");
            var todosOsProdutos = context.Produtos.OrderBy(p => p.Nome).ToList();
            foreach (var produto in todosOsProdutos)
            {
                Console.WriteLine($"ID: {produto.Id}, Nome: {produto.Nome}, Preço: {produto.Preco:C}, Estoque: {produto.QuantidadeEstoque}");
            }

            // 3. UPDATE: Atualizando um produto
            Console.WriteLine("\n>> Atualizando o preço do 'Mouse sem Fio'...");
            var produtoParaAtualizar = context.Produtos.FirstOrDefault(p => p.Nome == "Mouse sem Fio");
            if (produtoParaAtualizar != null)
            {
                produtoParaAtualizar.Preco = 165.50m; // Novo preço
                context.SaveChanges();
                Console.WriteLine("Preço atualizado!");
            }

            // 4. DELETE: Removendo um produto
            Console.WriteLine("\n>> Deletando o 'Monitor Ultrawide' recém-criado...");
            var produtoParaDeletar = context.Produtos.FirstOrDefault(p => p.Nome == "Monitor Ultrawide");
            if (produtoParaDeletar != null)
            {
                context.Produtos.Remove(produtoParaDeletar);
                context.SaveChanges();
                Console.WriteLine("Produto deletado!");
            }

            // 5. CHAMANDO A FUNÇÃO PL/pgSQL
            Console.WriteLine("\n>> Calculando o valor total do estoque com a função PL/pgSQL...");

            // Para chamar uma função scalar (que retorna um único valor),
            // a forma mais simples e segura é usar SQL puro com EF Core.
            decimal valorTotalEstoque = 0;

            // Abre a conexão gerenciada pelo EF Core
            var connection = context.Database.GetDbConnection();
            connection.Open();

            // Cria um comando para executar a função
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT fn_calcular_valor_total_estoque();";
                var result = command.ExecuteScalar(); // Executa e retorna o primeiro valor da primeira linha

                if (result != null && result != DBNull.Value)
                {
                    valorTotalEstoque = Convert.ToDecimal(result);
                }
            }
            connection.Close(); // Fecha a conexão

            Console.WriteLine($"O valor total do estoque é: {valorTotalEstoque:C}");
        }

        Console.WriteLine("\n--- Fim da Execução ---");
    }
}

    Explicação do Código Principal:

        using (var context = new AppDbContext()): Instancia nosso contexto. O using garante que os recursos de conexão sejam liberados ao final.

        context.Add(): Adiciona uma nova entidade para ser rastreada pelo EF Core.

        context.SaveChanges(): Persiste no banco de dados todas as mudanças rastreadas (inserções, atualizações, deleções).

        context.Produtos.ToList(): Executa um SELECT * FROM produtos e materializa os resultados em uma lista de objetos Produto.

        FirstOrDefault(): Busca o primeiro elemento que satisfaz a condição ou retorna null se nenhum for encontrado.

        context.Remove(): Marca uma entidade para ser deletada.

        Chamando a Função: A forma mais direta de chamar uma função que retorna um único valor (scalar function) é usar o DbConnection do próprio EF Core. Abrimos a conexão, criamos um comando com o SQL SELECT nome_da_funcao(); e usamos ExecuteScalar() para obter o resultado.

Como Executar o Projeto

    Verifique se o seu servidor PostgreSQL está rodando.

    Abra o terminal na pasta do projeto (GerenciadorDeProdutosApp).

    Execute o comando:
    dotnet run


context.SaveChanges(): Persiste no banco de dados todas as mudanças rastreadas (inserções, atualizações, deleções).

        context.Produtos.ToList(): Executa um SELECT * FROM produtos e materializa os resultados em uma lista de objetos Produto.

        FirstOrDefault(): Busca o primeiro elemento que satisfaz a condição ou retorna null se nenhum for encontrado.

        context.Remove(): Marca uma entidade para ser deletada.

        Chamando a Função: A forma mais direta de chamar uma função que retorna um único valor (scalar function) é usar o DbConnection do próprio EF Core. Abrimos a conexão, criamos um comando com o SQL SELECT nome_da_funcao(); e usamos ExecuteScalar() para obter o resultado.

Como Executar o Projeto

    Verifique se o seu servidor PostgreSQL está rodando.

    Abra o terminal na pasta do projeto (GerenciadorDeProdutosApp).

    Execute o comando:


dotnet run



After all, execute 
$ sudo dotnet build

OUTPUT: Restauração concluída (0,7s)
  ProductsManagementApp êxito (1,1s) → bin/Debug/net9.0/ProductsManagementApp.dll

Then, 

$ dotnet run

OUTPUT:

--- Manage products using Entity FrameworkCore and PostgreSQL ---

>> Inserting a new product...
Product inserted correctly!

>> Listing all products:
ID: 61, Name: 3D Printer, Price: R$ 2.890,00, Stock: 4
ID: 33, Name: Action Camera 4K, Price: R$ 1.350,00, Stock: 12
ID: 88, Name: Air Fryer 5L, Price: R$ 680,00, Stock: 10
ID: 89, Name: Bluetooth Adapter, Price: R$ 90,00, Stock: 40
ID: 12, Name: Bluetooth Headphones, Price: R$ 599,90, Stock: 30
ID: 45, Name: Bluetooth Speaker Mini, Price: R$ 210,00, Stock: 55
ID: 65, Name: Bluetooth Tracker, Price: R$ 95,00, Stock: 60
ID: 74, Name: Cable Organizer Box, Price: R$ 70,00, Stock: 60
ID: 66, Name: Car Charger USB-C, Price: R$ 120,00, Stock: 50
ID: 56, Name: CPU Cooler RGB, Price: R$ 380,00, Stock: 15
ID: 67, Name: Dash Cam, Price: R$ 650,00, Stock: 12
ID: 109, Name: Desktop Computer i7, Price: R$ 6.299,00, Stock: 5
ID: 103, Name: Digital Alarm Clock, Price: R$ 120,00, Stock: 40
ID: 84, Name: Digital Photo Frame, Price: R$ 450,00, Stock: 15
ID: 64, Name: Digital Scale, Price: R$ 110,00, Stock: 25
ID: 76, Name: Drawing Tablet Small, Price: R$ 590,00, Stock: 20
ID: 37, Name: Drone with Camera, Price: R$ 3.200,00, Stock: 6
ID: 87, Name: Electric Kettle, Price: R$ 180,00, Stock: 25
ID: 19, Name: Ergonomic Office Chair, Price: R$ 1.250,00, Stock: 12
ID: 79, Name: Ethernet Cable 10m, Price: R$ 85,00, Stock: 80
ID: 14, Name: External Hard Drive 2TB, Price: R$ 450,00, Stock: 35
ID: 62, Name: Filament PLA 1kg, Price: R$ 150,00, Stock: 20
ID: 104, Name: Fitness Tracker, Price: R$ 310,00, Stock: 25
ID: 54, Name: Gaming Controller, Price: R$ 420,00, Stock: 28
ID: 69, Name: Gaming Desk, Price: R$ 1.299,00, Stock: 7
ID: 25, Name: Gaming Headset, Price: R$ 430,00, Stock: 25
ID: 102, Name: Gaming Laptop Cooling Stand, Price: R$ 390,00, Stock: 12
ID: 10, Name: Gaming Monitor 27", Price: R$ 1.899,99, Stock: 15
ID: 42, Name: Gaming Mouse Pad, Price: R$ 120,00, Stock: 45
ID: 55, Name: Graphic Card RTX 4070, Price: R$ 4.999,00, Stock: 5
ID: 28, Name: Graphics Tablet, Price: R$ 690,00, Stock: 15
ID: 83, Name: HD Capture Card, Price: R$ 920,00, Stock: 8
ID: 60, Name: HD External 4TB, Price: R$ 750,00, Stock: 18
ID: 17, Name: HDMI Cable 2m, Price: R$ 49,90, Stock: 100
ID: 50, Name: HDMI Splitter, Price: R$ 130,00, Stock: 30
ID: 32, Name: Ink Cartridge Pack, Price: R$ 250,00, Stock: 35
ID: 52, Name: Keyboard Wrist Rest, Price: R$ 85,00, Stock: 60
ID: 96, Name: Laptop 14", Price: R$ 3.899,00, Stock: 12
ID: 1, Name: Laptop Gamer, Price: R$ 7.500,50, Stock: 10
ID: 4, Name: Laptop Gamer, Price: R$ 7.500,50, Stock: 10
ID: 7, Name: Laptop Gamer gen.2, Price: R$ 7.500,50, Stock: 10
ID: 77, Name: Laptop Sleeve 15", Price: R$ 140,00, Stock: 30
ID: 48, Name: Laptop Stand, Price: R$ 190,00, Stock: 50
ID: 20, Name: LED Desk Lamp, Price: R$ 210,00, Stock: 30
ID: 100, Name: LED Strip 5m, Price: R$ 160,00, Stock: 30
ID: 9, Name: Mechanical Keyboard, Price: R$ 450,00, Stock: 25
ID: 95, Name: Mechanical Keyboard RGB, Price: R$ 550,00, Stock: 15
ID: 6, Name: Mechanical keyboard XPTO, Price: R$ 410,00, Stock: 35
ID: 3, Name: Mechanical keyboard XPTO, Price: R$ 410,00, Stock: 35
ID: 43, Name: Mechanical Pencil Set, Price: R$ 35,00, Stock: 100
ID: 68, Name: Mechanical Switch Set, Price: R$ 180,00, Stock: 30
ID: 58, Name: Memory RAM 16GB, Price: R$ 450,00, Stock: 20
ID: 105, Name: Microphone Arm Stand, Price: R$ 170,00, Stock: 28
ID: 34, Name: Microphone Condenser, Price: R$ 400,00, Stock: 28
ID: 70, Name: Micro SD Card 128GB, Price: R$ 220,00, Stock: 40
ID: 93, Name: Mini Bluetooth Keyboard, Price: R$ 240,00, Stock: 20
ID: 27, Name: Mini Projector, Price: R$ 850,00, Stock: 10
ID: 49, Name: Monitor 32" Curved, Price: R$ 2.499,00, Stock: 10
ID: 80, Name: Monitor Arm Mount, Price: R$ 330,00, Stock: 25
ID: 57, Name: Motherboard ATX, Price: R$ 1.250,00, Stock: 8
ID: 113, Name: Noise Cancelling Earbuds, Price: R$ 699,00, Stock: 22
ID: 75, Name: Noise Cancelling Headphones, Price: R$ 1.199,00, Stock: 12
ID: 44, Name: Notebook Cooling Pad, Price: R$ 180,00, Stock: 40
ID: 59, Name: NVMe Adapter PCIe, Price: R$ 120,00, Stock: 25
ID: 115, Name: Paper Clip, Price: R$ 2,30, Stock: 40
ID: 112, Name: Portable Bluetooth Radio, Price: R$ 270,00, Stock: 20
ID: 85, Name: Portable Fan USB, Price: R$ 70,00, Stock: 50
ID: 16, Name: Portable Speaker, Price: R$ 320,00, Stock: 45
ID: 41, Name: Portable SSD 512GB, Price: R$ 580,00, Stock: 25
ID: 24, Name: Power Bank 20000mAh, Price: R$ 250,00, Stock: 40
ID: 31, Name: Printer LaserJet, Price: R$ 1.100,00, Stock: 14
ID: 91, Name: Robot Vacuum Cleaner, Price: R$ 1.899,00, Stock: 6
ID: 30, Name: Router Wi-Fi 6, Price: R$ 720,00, Stock: 22
ID: 72, Name: Security Camera Kit, Price: R$ 1.690,00, Stock: 5
ID: 39, Name: Smart Bulb RGB, Price: R$ 95,00, Stock: 50
ID: 114, Name: Smart Ceiling Light, Price: R$ 380,00, Stock: 14
ID: 92, Name: Smart Doorbell, Price: R$ 820,00, Stock: 9
ID: 38, Name: Smart Home Hub, Price: R$ 499,00, Stock: 18
ID: 82, Name: Smart Light Strip, Price: R$ 280,00, Stock: 40
ID: 71, Name: Smart Lock, Price: R$ 880,00, Stock: 8
ID: 21, Name: Smartphone 128GB, Price: R$ 2.799,00, Stock: 20
ID: 97, Name: Smartphone 256GB, Price: R$ 3.699,00, Stock: 18
ID: 51, Name: Smart Plug Wi-Fi, Price: R$ 145,00, Stock: 45
ID: 90, Name: Smart Scale, Price: R$ 310,00, Stock: 18
ID: 63, Name: Smart Thermostat, Price: R$ 680,00, Stock: 10
ID: 29, Name: Smart TV 55", Price: R$ 3.999,00, Stock: 8
ID: 108, Name: Smart TV 65", Price: R$ 5.299,00, Stock: 6
ID: 101, Name: Smartwatch Lite, Price: R$ 720,00, Stock: 20
ID: 13, Name: Smartwatch Pro, Price: R$ 999,00, Stock: 20
ID: 86, Name: Smart Water Bottle, Price: R$ 250,00, Stock: 20
ID: 110, Name: Soundbar 2.1, Price: R$ 1.199,00, Stock: 8
ID: 15, Name: SSD 1TB NVMe, Price: R$ 720,00, Stock: 25
ID: 22, Name: Tablet 10-inch, Price: R$ 1.799,00, Stock: 25
ID: 98, Name: Tablet Stylus Pen, Price: R$ 250,00, Stock: 35
ID: 35, Name: Tripod Stand, Price: R$ 180,00, Stock: 40
ID: 73, Name: TV Wall Mount, Price: R$ 260,00, Stock: 20
ID: 94, Name: USB-C Cable 1m, Price: R$ 55,00, Stock: 100
ID: 78, Name: USB-C Docking Station, Price: R$ 480,00, Stock: 15
ID: 11, Name: USB-C Hub 6-in-1, Price: R$ 230,49, Stock: 40
ID: 26, Name: USB Flash Drive 64GB, Price: R$ 80,00, Stock: 60
ID: 106, Name: USB Hub 10-Port, Price: R$ 260,00, Stock: 22
ID: 47, Name: USB Microphone, Price: R$ 350,00, Stock: 25
ID: 36, Name: VR Headset, Price: R$ 2.100,00, Stock: 7
ID: 53, Name: Webcam Cover Set, Price: R$ 30,00, Stock: 80
ID: 18, Name: Webcam Full HD, Price: R$ 260,00, Stock: 18
ID: 40, Name: Wi-Fi Extender, Price: R$ 210,00, Stock: 30
ID: 107, Name: Wi-Fi Router Dual Band, Price: R$ 450,00, Stock: 16
ID: 23, Name: Wireless Charger, Price: R$ 120,00, Stock: 50
ID: 111, Name: Wireless Charging Pad, Price: R$ 220,00, Stock: 35
ID: 46, Name: Wireless Earbuds, Price: R$ 399,00, Stock: 30
ID: 2, Name: Wireless Mouse, Price: R$ 150,75, Stock: 50
ID: 8, Name: Wireless Mouse, Price: R$ 150,75, Stock: 50
ID: 5, Name: Wireless Mouse, Price: R$ 150,75, Stock: 50
ID: 81, Name: Wireless Numeric Keypad, Price: R$ 170,00, Stock: 30
ID: 99, Name: Wireless Presentation Clicker, Price: R$ 190,00, Stock: 25

>> Updating price of 'Tablet 10-inch'...
Price updated!

>> Delete item already inserted...
Product deleted!

>> Computing the total value from stock, using a PL/pgSQL function...
The value of total stock is: R$ 1.429.261,45

--- End of Execution ---


