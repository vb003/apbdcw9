using System.ComponentModel.DataAnnotations;

namespace Tutorial9.DTOs;

public class ProductWarehouseDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int IdWarehouse { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int IdProduct { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int Amount { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    public int IdOrder { get; set; }
}