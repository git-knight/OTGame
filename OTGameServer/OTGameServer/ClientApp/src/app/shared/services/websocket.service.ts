import { Injectable } from '@angular/core';
import * as io from 'socket.io-client';
import { Observable } from 'rxjs/Observable';
import * as Rx from 'rxjs/Rx';
import { environment } from '../../../environments/environment';
import { HubConnection, HubConnectionBuilder } from '@aspnet/signalr';

@Injectable()
export class WebsocketService {
  // Our socket connection
  //private socket;
  private subject: Rx.Subject<MessageEvent>;
  public messages: Rx.Subject<any>;
  connection: HubConnection;

  constructor() { }

  connectR(url, accessToken: string) {
    console.log(accessToken)
    this.connection = new HubConnectionBuilder()
      .withUrl(url, { accessTokenFactory: async () => { return accessToken } })
      .build();

    this.connection.start().catch(err => console.log(err));

    //connection.on("test", (val) => console.log(val));
  }

  connect(url) {
    console.log('test')

    if (!this.subject) {
      this.subject = this.create(url);
      console.log("Successfully connected: " + url);
      this.messages = <Rx.Subject<any>>this.subject.map((response: MessageEvent): any => {
        return (response.data);
      })
    }
    return this.subject;

    //console.log("received response: " + msg);
  }

  private create(url): Rx.Subject<MessageEvent> {
    let ws = new WebSocket(url);

    let observable = Rx.Observable.create(
      (obs: Rx.Observer<MessageEvent>) => {
        ws.onmessage = obs.next.bind(obs);
        ws.onerror = obs.error.bind(obs);
        ws.onclose = obs.complete.bind(obs);
        return ws.close.bind(ws);
      })
    let observer = {
      next: (data: Object) => {
        if (ws.readyState === WebSocket.OPEN) {
          ws.send(JSON.stringify(data));
        }
      }
    }
    return Rx.Subject.create(observer, observable);
  }

  public sendMessage(name: string, data): boolean {
    //this.ws.send(JSON.stringify(data));

    return true;
  }
}
