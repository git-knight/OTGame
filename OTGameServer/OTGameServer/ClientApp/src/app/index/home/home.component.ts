import { Component, OnInit } from '@angular/core';
import { Credentials } from '../../shared/auth/credentials';
import { UserService } from '../../shared/services/user.service';
import { UserRegistration } from '../../shared/auth/user-registration';
import { Http, Headers } from '@angular/http';


@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})

export class HomeComponent implements OnInit {

  //credentials: Credentials = { email: '', password: '' };
  isRequesting = false

  private displayName = localStorage.getItem('displayName')
  private errorMessage = ""

  private maps: string[]
  private monsterTypes: string[]

  constructor(
    private userService: UserService,
    private http: Http) {
  }

  ngOnInit() {
    this.http.get("api/maps").subscribe(maps => this.maps = maps.json().maps);
    this.http.get("api/monster/GetTypes").subscribe(types => this.monsterTypes = types.json().types);
  }

  getIdOf(s: string) {
    return s.split(' ')[0].slice(1)
  }

  doSignIn({ value, valid }: { value: Credentials, valid: boolean }) {

    this.isRequesting = true

    console.log(value)

    this.userService
      .login(value.email, value.password)
      .subscribe(res => {
        this.isRequesting = false
        this.displayName = res.displayName
      });
  }

  registerUser({ value, valid }: { value: UserRegistration, valid: boolean }) {
    this.userService
      .register(value)
      .subscribe(res => {
        if (res.result === "OK") {
          this.isRequesting = false
          this.displayName = res.displayName
        }
        else this.errorMessage = res.result
      })
  }


  createMap({ value, valid }: { value: any, valid: boolean }) {
    var headers = new Headers()
    headers.append('Content-Type', 'application/json')

    console.log(value)

    this.http.post('api/maps/create', JSON.stringify(value), { headers })
      .subscribe(obj => {
        console.log(obj)
      })
  }

  buildMap({ value, valid }: { value: any, valid: boolean }) {
    var headers = new Headers()
    headers.append('Content-Type', 'application/json')

    console.log(value)

    this.http.post('api/maps/build?name='+value.name, JSON.stringify(value), { headers })
      .subscribe(obj => {
        console.log(obj)
      })
  }

  createMonster({ value, valid }: { value: any, valid: boolean }) {
    var headers = new Headers()
    headers.append('Content-Type', 'application/json')

    console.log(value)

    this.http.post('api/monster/create', JSON.stringify(value), { headers })
      .subscribe(obj => {
        console.log(obj)
      })
  }

  createMonsterType({ value, valid }: { value: any, valid: boolean }) {
    var headers = new Headers()
    headers.append('Content-Type', 'application/json')

    console.log(value)

    this.http.post('api/monster/createtype', JSON.stringify(value), { headers })
      .subscribe(obj => {
        console.log(obj)
      })
  }
}
