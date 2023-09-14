import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
	selector: 'app-login',
	templateUrl: './login.component.html',
	styleUrls: ['./login.component.css'],
})
export class LoginComponent implements OnInit {
	loginForm: FormGroup | any;
	registerForm: FormGroup | any;
	currentState: boolean = false;
	res: any;
	constructor(private fb: FormBuilder, private authService: AuthService, private router: Router, private _snackBar: MatSnackBar
	) {
	}

	loadSnackBar(message: string) {
		this._snackBar.open(message, 'Close', {
			duration: 3000,
			horizontalPosition: 'center',
			verticalPosition: 'top',
		});
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
			if (this.res.message == "Login success") {
				localStorage.setItem('token', this.res.data.token);
				localStorage.setItem('username', this.res.data.data.username);
				localStorage.setItem('userId', this.res.data.data.id);
				this.router.navigateByUrl("/channels");
				this.loadSnackBar('Login success')


			}
		},
			(error) => {
				console.log(error);
				if (error.status == 400)
					this.loadSnackBar('username or password is incorrect');
				else
					this.loadSnackBar('Login failed');
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
			if (this.res.message == "Login success") {
				localStorage.setItem('token', this.res.data.token);
				localStorage.setItem('username', this.res.data.data.username);
				localStorage.setItem('userId', this.res.data.data.id);
				this.router.navigateByUrl("/channels");
				this.loadSnackBar('Register success')
			}
		},
			(error) => {
				console.log(error);
				if (error.status == 400)
					this.loadSnackBar('username already exists');
				else
					this.loadSnackBar('Register failed');
			}
		)

	}

	toggleForm(formType: 'login' | 'register') {
		this.currentState = formType === 'login' ? false : true;
		if (this.currentState) {
			this.registerForm.reset(
				{
					username: this.loginForm.value.username,
					password: '',
					confirmPassword: ''
				});
		}
		else {
			this.loginForm.reset({
				username: this.registerForm.value.username,
				password: ''
			});
		}
	}
}
