import { Component } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from 'src/environments/environment';
@Component({
	selector: 'app-root',
	templateUrl: './app.component.html',
	styleUrls: ['./app.component.css']
})
export class AppComponent {
	title = 'Client';
	count = 0;

	ngOnInit(): void {
		const connection = new signalR.HubConnectionBuilder()
			.configureLogging(signalR.LogLevel.Information)
			.withUrl(environment.ws + 'channels')
			.build();

		connection.start().then(function () {
			console.log('Connected!s');
		}
		).catch(function (err) {
			return console.error(err.toString());
		}
		);

		connection.on("Message", (message: any) => {
			console.log(message);
			this.count = ++this.count;
			if (this.count != 0) {
				this.title = "You have " + this.count + " new messages";
			}
		}
		);

		connection.on("DeleteChannel", (message: any) => {
			console.log(message);
		}
		);
	}
}
