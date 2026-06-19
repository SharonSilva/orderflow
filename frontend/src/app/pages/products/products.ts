import {Component, OnInit, inject, signal} from '@angular/core';
import {CommonModule} from '@angular/common';
import {ApiService} from '../../api/api.service';
import {ProductDto} from '../../api/api.types';

@Component({
    selector:'app-products',
    imports: [CommonModule],
    templateUrl:'./products.html',
    styleUrl:'./products.css'
  })
  export class Products implements OnInit{
      private readonly api = inject(ApiService);

      readonly products = signal<ProductDto[]>([]);
      readonly loading = signal(true);
      readonly error = signal<string | null>(null);

      ngOnInit(): void {
          this.api.getProducts().subscribe({
            next: (products) => {
              this.products.set(products);
              this.loading.set(false);
            },
            error: (err) => {
              console.error('Failed to load products', err);
              this.error.set('Could not load products. Is the backend running?')
              this.loading.set(false);
            }
          });
      }
  }
