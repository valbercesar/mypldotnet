
/*
* PL/.NET Project
* valber.cesar@brickabode.com
* File: Program.cs
* 
* Natal, Oct, 15 2025
*/

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

// CRUD class
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("--- Manage products using Entity FrameworkCore and PostgreSQL ---");

        // Applying 'using' to guarantee that the DB connection be closed correctly.
        using (var context = new AppDbContext())
        {
            // It does not create DB, only test connection.
            context.Database.EnsureCreated();

            // Add a new product item
            Console.WriteLine("\n>> Inserting a new product...");
            var newProduct = new Product
            {
                Name = "Paper Clip",
                Price = 2.3m,
                StockQuantity = 40
            };
            context.Products.Add(newProduct);
            context.SaveChanges(); // Salva as mudanças no banco de dados
            Console.WriteLine("Product inserted correctly!");

            // Readind and receiving products
            Console.WriteLine("\n>> Listing all products:");
            var allOfProducts = context.Products.OrderBy(p => p.Name).ToList();
            foreach (var product in allOfProducts)
            {
                Console.WriteLine($"ID: {product.Id}, Name: {product.Name}, Price: {product.Price:C}, Stock: {product.StockQuantity}");
            }

            // Update poduct
            Console.WriteLine("\n>> Updating price of 'Tablet 10-inch'...");
            var productToUpdate = context.Products.FirstOrDefault(p => p.Name == "Tablet 10-inch");
            if (productToUpdate != null)
            {
                productToUpdate.Price = 165.50m; // Novo preço
                context.SaveChanges();
                Console.WriteLine("Price updated!");
            }

            // 4. DELETE: Removendo um produto
            Console.WriteLine("\n>> Delete item already inserted...");
            var productToDelete = context.Products.FirstOrDefault(p => p.Name == "LED Desk Lamp");
            if (productToDelete != null)
            {
                context.Products.Remove(productToDelete);
                context.SaveChanges();
                Console.WriteLine("Product deleted!");
            }

            // Calling PL/pgSQL functions
            Console.WriteLine("\n>> Computing the total value from stock, using a PL/pgSQL function...");

            // The safest way to call a scalar funcion that return single value,
            // is using pure SQL with EF Core.
            decimal TotalStockValue = 0;

            // Open connection managed by EF Core
            var connection = context.Database.GetDbConnection();
            connection.Open();

            // Create a command to execute function
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT fn_get_value_total_stock();";
                
                // Runs and returns first value of the 1st line.
                var result = command.ExecuteScalar(); 
                if (result != null && result != DBNull.Value)
                {
                    TotalStockValue = Convert.ToDecimal(result);
                }
            }
            connection.Close(); // close connection

            Console.WriteLine($"The value of total stock is: {TotalStockValue:C}");
        }

        Console.WriteLine("\n--- End of Execution ---");
    }
}
