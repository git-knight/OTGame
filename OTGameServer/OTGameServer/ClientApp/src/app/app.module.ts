import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { HttpModule } from '@angular/http';
//import { ViewService } from './game/classes/engine/view.service'
import { WebsocketService } from './shared/services/websocket.service'

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './index/home/home.component';
import { GameComponent } from './game/game.component';
import { FetchDataComponent } from './index/fetch-data/fetch-data.component';
import { PinfoComponent } from './index/pinfo/pinfo.component';
import { UserService } from './shared/services/user.service';
import { RegisterComponent } from './index/register/register.component';
import { UnitEditorComponent } from './index/unit-editor/unit-editor.component';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    GameComponent,
    FetchDataComponent,
    PinfoComponent,
    RegisterComponent,
    UnitEditorComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    HttpModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'game', component: GameComponent },
      { path: 'unitEd', component: UnitEditorComponent },
      //{ path: 'register', component: RegisterComponent },
      //{ path: 'fetch-data', component: FetchDataComponent },
      //{ path: 'pinfo', component: PinfoComponent },
    ])
  ],
  providers: [
    //ViewService,
    WebsocketService,
    UserService,
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
