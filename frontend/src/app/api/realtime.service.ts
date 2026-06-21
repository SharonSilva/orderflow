import { Injectable, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel} from '@microsoft/signalr'; // this imports the 
// the classes from the signalR typescript client library the aNGULAR aPP can connect ot Signal R hub in the .NET backend
import { OrderStatusChange } from './api.types';

@Injectable({providedIn: 'root'})   //this class is a service that Angular can create and inject
export class RealtimeService{       //would just be a normal Typescript class and angular wouldn't manage it 
    private connection: HubConnection | null = null;    

    //Signals - components read these to react to live state
    readonly connectionState = signal<HubConnectionState>(HubConnectionState.Disconnected);       
    readonly lastOrderStatusChange = signal<OrderStatusChange | null>(null);     

    //Open the hub connection and called it once at app startup via provideAppIntializer  
    //safe to call multiple times since its idempotent   

    async start(): Promise<void>{   //synchronous method waiting for for server to response when connection successful  
        //promise is a value that will exist in the future so that js returns a response if the server hadsnt responded  
        if(this.connection){    //if the connection is not null
            return;
        }

        this.connection = new HubConnectionBuilder()
            .withUrl('/hubs/orders')        //Goes through dev proxy -> gateway -> orders hub
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();

        //Register the event handler before start() otherwise events that arrive
        // during the handshake window would be missed
        this.connection.on('OrderStatusChanged', (msg: OrderStatusChange) => {
            this.lastOrderStatusChange.set(msg);
        }); 

        //Track connection lifecycle in the signal so UI can show status 
        this.connection.onreconnecting(()=>{ 
            this.connectionState.set(HubConnectionState.Reconnecting); 
        }); 
        this.connection.onreconnected(()=>{ 
            this.connectionState.set(HubConnectionState.Connected); 
        }) 
        this.connection.onclose(()=>{ 
            this.connectionState.set(HubConnectionState.Disconnected); 
        }); 

        try{
            await this.connection.start();
            this.connectionState.set(HubConnectionState.Connected);
        }catch(err){
            console.error('SignalR start failed', err);
            this.connectionState.set(HubConnectionState.Disconnected)
        }
    }

    async stop(): Promise<void>{
        if(this.connection){
            await this.connection.stop();
            this.connection = null;
        }
    }
}
