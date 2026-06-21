import {Routes} from '@angular/router';
import {Products} from './pages/products/products';
import {Orders} from './pages/orders/orders';

export const routes: Routes = [
    {path: '', component: Products},
    {path: 'orders', component: Orders},
    {path: '**', redirectTo: ''}
];