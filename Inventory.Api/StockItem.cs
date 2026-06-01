namespace Inventory.Api;

public class StockItem
{
    public Guid Id{get;set;}
    public string ProductId{get;set;} = default!;
    public int AvailableQuantity {get;set;} 
    public int ReservedQuantity {get;set;}

}