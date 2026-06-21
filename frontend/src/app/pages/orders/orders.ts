import { Component, OnInit, computed, effect, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HubConnectionState } from '@microsoft/signalr';
import { ApiService } from '../../api/api.service';
import { RealtimeService } from '../../api/realtime.service';
import { OrderDto } from '../../api/api.types';

interface ConnectionPill {
  label: string;
  classes: string;
}

@Component({
  selector: 'app-orders',
  imports: [CommonModule],
  templateUrl: './orders.html',
  styleUrl: './orders.css'
})
export class Orders implements OnInit {
  private readonly api = inject(ApiService);
  private readonly realtime = inject(RealtimeService);

  readonly orders = signal<OrderDto[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  // Derived signal — turns the SignalR enum into a label + Tailwind classes
  // for the connection pill. Re-runs whenever connectionState changes.
  readonly connectionPill = computed<ConnectionPill>(() => {
    switch (this.realtime.connectionState()) {
      case HubConnectionState.Connected:
        return {
          label: 'Connected',
          classes: 'bg-emerald-100 text-emerald-800 border-emerald-200'
        };
      case HubConnectionState.Connecting:
        return {
          label: 'Connecting',
          classes: 'bg-amber-100 text-amber-800 border-amber-200'
        };
      case HubConnectionState.Reconnecting:
        return {
          label: 'Reconnecting',
          classes: 'bg-amber-100 text-amber-800 border-amber-200'
        };
      case HubConnectionState.Disconnected:
      case HubConnectionState.Disconnecting:
      default:
        return {
          label: 'Disconnected',
          classes: 'bg-slate-100 text-slate-600 border-slate-200'
        };
    }
  });

  constructor() {
    // React to live status changes — runs every time lastOrderStatusChange updates
    effect(() => {
      const event = this.realtime.lastOrderStatusChange();
      if (!event) return;

      this.orders.update(list =>
        list.map(o =>
          o.id === event.orderId
            ? { ...o, status: event.status }
            : o
        )
      );
    });
  }

  ngOnInit(): void {
    this.api.getOrders().subscribe({
      next: (orders) => {
        // Most recent first
        this.orders.set([...orders].sort((a, b) =>
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        ));
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load orders', err);
        this.error.set('Could not load orders. Is the backend running?');
        this.loading.set(false);
      }
    });
  }

  // Helper for the template — Tailwind classes for order status pill
  statusClasses(status: OrderDto['status']): string {
    switch (status) {
      case 'Pending':
        return 'bg-amber-100 text-amber-800 border-amber-200';
      case 'Confirmed':
        return 'bg-emerald-100 text-emerald-800 border-emerald-200';
      case 'Cancelled':
        return 'bg-rose-100 text-rose-800 border-rose-200';
    }
  }
}