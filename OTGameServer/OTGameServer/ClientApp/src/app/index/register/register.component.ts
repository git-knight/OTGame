import { Component, OnInit } from '@angular/core';
import { UserRegistration } from '../../shared/auth/user-registration';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})

export class RegisterComponent implements OnInit {

  isRequesting: boolean;

  constructor() { }

  ngOnInit() {
  }

  registerUser({ value, valid }: { value: UserRegistration, valid: boolean }) {
    
  }
}
