//Typescript interfaces that mirror the .NET DTOs from the backend 
//Keep these in sync with :
// - Orders.Api/Program.cs
// - Inventory.Api (ProductDTO)
// - Payments.Api (PaymentDTO)

export interface ProductDto {
  id: string;
  productId: string;
  name: string;
  unitPrice: number;
  availableQuantity: number;
  reservedQuantity: number;
}

export interface OrderDto {
  id: string;
  customerId: string;
  productId: string;
  quantity: number;
  amount: number; 
  status: 'Pending' | 'Confirmed' | 'Cancelled';
  createdAt : string; //ISO timestamp
}

export interface CreateOrderRequest {
  customerId: string;
  productId: string;
  quantity: number;
  amount: number;
}

export interface PaymentDto {
  id: string;
  orderId: string;
  amount: number;
  status: 'Pending' | 'Succeeded' | 'Failed';
  processedAt: string;
}
// SignalR event payload — broadcast by Orders.Api when an order's status changes.
// See Orders.Api/Handlers/PaymentSucceededHandler.cs and StockReleasedHandler.cs.
export interface OrderStatusChange {
  orderId: string;
  status: 'Confirmed' | 'Cancelled';
  changedAt: string;  // ISO timestamp
}
