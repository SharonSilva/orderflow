import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateOrderRequest, OrderDto, PaymentDto, ProductDto } from './api.types';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);

//Inventory
getProducts(): Observable<ProductDto[]>{
  return this.http.get<ProductDto[]>('/api/inventory/products');
}

//Orders
 getOrders(customerId?: string): Observable<OrderDto[]> {
    let params = new HttpParams();
    if (customerId) {
      params = params.set('customerId', customerId);
    }
    return this.http.get<OrderDto[]>('/api/orders', { params });
  }

getOrder(id: string) : Observable<OrderDto>{
  return this.http.get<OrderDto>(`/api/orders/${id}`);
}

createOrder(request: CreateOrderRequest): Observable<OrderDto>{
  return this.http.post<OrderDto>('/api/orders', request);
}

//Payments
getPayments() : Observable<PaymentDto[]>{
  return this.http.get<PaymentDto[]>('/api/payments');
}

}