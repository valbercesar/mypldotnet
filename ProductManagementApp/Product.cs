/*
* PL/.NET Project
* valber.cesar@brickabode.com
* File: Product.cs
* 
* Natal, Oct, 15 2025
*/

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


// This is the Entity representation in the DB, who maps such class to create a table 'products'
[Table("products")]
public class Product
{
    [Key] // Flags it a property primary key
    [Column("id")]
    public int Id { get; set; }

    // To permit that Name receives NULL values, at start of
    // construtor, a '?' must to came after string type to
    // indicate that the value 'Name' can be empty. Otherwise,
    // to force that it must to have some value at inicialization,
    // use 'required' modifier before string type.
    [Column("name")]
    public string? Name { get; set; }

    [Column("price")]
    public decimal Price { get; set; }

    [Column("stock_quantity")]
    public int StockQuantity { get; set; }

    [Column("registration_date")]
    public DateTime RegistrationDate { get; set; }
}




