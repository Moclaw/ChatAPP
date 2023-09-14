import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

@Injectable({
	providedIn: 'root'
})
export class AuthService {
	apiUrl = environment.apiUrl + 'auth';
	constructor(private http: HttpClient) { }
	login(json: any) {
		return this.http.post(this.apiUrl + '/login',
			json);
	}
	register(json: any) {
		return this.http.post(this.apiUrl + '/register',
			json);
	}
	isUserLoggedIn() {
		return localStorage.getItem('token') != null;
	}
	getToken() {
		return localStorage.getItem('token');
	}
	getUsers() {
		return this.http.get(this.apiUrl + '/GetUser');
	}
	getUserById(id: any) {
		return this.http.get(this.apiUrl + '/GetUserById?id=' + id);
	}

}
