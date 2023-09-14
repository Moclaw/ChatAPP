import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { LoginComponent } from './login.component';
import { ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatSnackBarModule } from '@angular/material/snack-bar';


@NgModule({
	declarations: [LoginComponent],
	imports: [
		ReactiveFormsModule,
		RouterModule.forChild([
			{
				path: '',
				component: LoginComponent
			}
		]),
		CommonModule,
		MatSnackBarModule
	],
	exports: [LoginComponent],
	providers: [AuthService],
	schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class LoginModule { }
