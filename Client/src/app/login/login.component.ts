import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { trigger, state, style, animate, transition } from '@angular/animations';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  animations: [
    trigger('formAnimation', [
      state('login', style({ transform: 'translateX(0)' })),
      state('register', style({ transform: 'translateX(-100%)' })),
      transition('login <=> register', animate('300ms ease-in-out')),
    ]),
  ],
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup | any;
  registerForm: FormGroup | any;
  currentState: 'login' | 'register' = 'login';
  res : any;
  constructor(private fb: FormBuilder, private authService : AuthService, private router: Router) {

  }

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required],
    });

    this.registerForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required],
      confirmPassword: ['', Validators.required],
    });
  }

  onLoginSubmit() {
    this.authService.login(this.loginForm.value).subscribe((result) => {
      this.res = result;
      console.log(this.res);
      if(this.res.message == "Login success"){
        localStorage.setItem('token', this.res.data.token);
        localStorage.setItem('username', this.res.data.data.username);
        localStorage.setItem('userId', this.res.data.data.id);
        this.router.navigateByUrl("/message");
        alert('Login success');
      }
    },
    (error) => {
      console.log(error);
      if(error.status == 400)
        alert('username or password is incorrect');
      else
        alert('login failed');
    }
    )
  }

  onRegisterSubmit() {
    if (this.registerForm.value.password !== this.registerForm.value.confirmPassword) {
      alert('Password and Confirm Password do not match');
      return;
    }

    this.authService.register(this.registerForm.value).subscribe((result) => {
      this.res = result;
      console.log(this.res);
      if(this.res.message == "Login success"){
        localStorage.setItem('token', this.res.data.token);
        localStorage.setItem('username', this.res.data.data.username);
        localStorage.setItem('userId', this.res.data.data.id);
        this.router.navigateByUrl("/message");
        alert('register success');
      }
    },
    (error) => {
      console.log(error);
      if(error.status == 400)
        alert('username is already exist');
      else
        alert('register failed');
    }
    )
    
  }

  toggleForm(formType: 'login' | 'register') {
    this.currentState = formType;
  }
}
