namespace Tutorial9.Model.DTOs;

public class ProductWarehouseRequest
{
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public int  Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public int OrderId { get; set; }
}