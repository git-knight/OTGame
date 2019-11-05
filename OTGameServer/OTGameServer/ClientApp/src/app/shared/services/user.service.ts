import { Injectable } from '@angular/core';
import { Http, Headers } from '@angular/http';
import { UserRegistration } from '../auth/user-registration';

@Injectable()
export class UserService {
  private baseUrl = window.location.origin;

  loggedIn = !!localStorage.getItem("auth_token");
  displayName = localStorage.getItem("displayName");

  constructor(private http: Http) { }

  login(email, password) {
    var headers = new Headers()
    headers.append('Content-Type', 'application/json')
    let authToken = localStorage.getItem('auth_token');
    headers.append('Authorization', `Bearer ${authToken}`);

    //console.log(this.baseUrl + "/api/user/auth");
    console.log(JSON.stringify({ email, password }))
    
    return this.http
      .post(this.baseUrl + "/api/user/auth", JSON.stringify({ email, password }), { headers })
      .map(res => res.json())
      .map(res => {
        console.log(res)
        if (res.result === "OK") {
          localStorage.setItem('auth_token', res.auth_token)
          localStorage.setItem('displayName', res.displayName)
          this.loggedIn = true
          this.displayName = res.displayName
        }
        //this._authNavStatusSource.next(true);
        return res;
      })
      //*/
      //.catch(err => { console.log(err) })
  }

  logout() {
    localStorage.removeItem('auth_token')
    this.loggedIn = false
  }

  register(value: UserRegistration) {
    var headers = new Headers()
    headers.append('Content-Type', 'application/json')

    let object = {
      email: value.email,
      username: value.nick,
      password: value.pwd
    }

    console.log("registering by object " + JSON.stringify(object))

    return this.http
      .post(this.baseUrl + "/api/user/register", JSON.stringify(object), { headers })
      .map(res => res.json())
      .map(res => {
        console.log(res)
        if (res.result === "OK") {
          localStorage.setItem('auth_token', res.auth_token)
          localStorage.setItem('displayName', res.displayName)
          this.loggedIn = true
        }
        return res;
      })
  }
}
