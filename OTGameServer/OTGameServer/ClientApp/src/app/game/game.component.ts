import { Component, AfterViewInit } from '@angular/core';
// tslint:disable: comment-format
//import { UnityLoader } from 'unity-loader'
import { UnityLoader } from './UnityLoader.js';
import { UnityProgress } from './UnityProgress.js';
import { HubConnection, HubConnectionBuilder } from '@aspnet/signalr';
import { windowWhen } from 'rxjs-compat/operator/windowWhen';

//declare var UnityLoader: any;
declare var unityInstance: any;

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.css']
})

export class GameComponent implements AfterViewInit {
  unityInstance: any;
  connection: HubConnection;

  constructor() { }

  ngAfterViewInit() {
    if (!localStorage.getItem('auth_token')) {
      console.log('no auth_token');
      alert('not authorized!');
      return;
    }

    console.log('connecting to the server...');

    window['UnityLoader'] = UnityLoader;

    window['hubConnection'] = this.connection = new HubConnectionBuilder()
      .withUrl('/gameserver', { accessTokenFactory: async () => localStorage.getItem('auth_token') })
      .build();

    unityInstance = window['unityInstance'] = this.unityInstance = UnityLoader.instantiate('unityContainer', '../../assets/Unity/Build/Unity.json');

    this.connection.on('OnConnected', function (a1) {
      unityInstance.SendMessage('GameHub', 'OnConnectedRaw', JSON.stringify(a1));
    });
    this.connection.on('InvokeMethod', function () {
      unityInstance.SendMessage('GameHub', 'InvokeMethod', JSON.stringify(Array.prototype.slice.call(arguments)));
    });

    //let values = "", target = ""
    //this.connection.invoke.apply(this.connection, [target].concat(JSON.parse(values)))

    //connection.start().catch(err => console.log(err));
    //$.getScript('TemplateData/UnityProgress.js').done(function(a, b) {
      //$.getScript('Build/UnityLoader.js').done(function(c, d) {
      //  this.unityInstance = UnityLoader.instantiate("unityContainer", "Build/HTML5.json");//, { onProgress: UnityProgress });
      //}.bind(this))
    //}.bind(this));
  }
}
