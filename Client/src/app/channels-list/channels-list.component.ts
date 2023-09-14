import { Component, NgModule, OnInit } from '@angular/core';
import { ChannelsService } from '../services/channels.service';
import { AuthService } from '../services/auth.service';
import * as signalR from '@microsoft/signalr';
import { environment } from 'src/environments/environment';
import { Title } from "@angular/platform-browser";
import { MatSnackBar } from '@angular/material/snack-bar';
import { FormControl } from '@angular/forms';

@Component({
	selector: 'app-channels-list',
	templateUrl: './channels-list.component.html',
	styleUrls: ['./channels-list.component.css']
})
export class ChannelsListComponent implements OnInit {
	isHaveData = false;
	isShowMessages: boolean = false;
	channels: any;
	messages: any;
	messageContent: string = "";
	channelId: any;
	channel: any;
	sender: any;
	receiver: any;
	isCreate = false;
	userId: any;
	name = new FormControl('');
	users: any;
	selected: any = [];
	selectedChannel: any = null; // Ban đầu không có mục nào được chọn
	notifications: any = [];
	isRead = true;
	notificationMess = "";
	userSelect = new FormControl('');
	ngOnInit(): void {
		this.getChannels();
		this.getNotification();
		this.connectToHub();

	}

	connectToHub() {
		const connection = new signalR.HubConnectionBuilder()
			.configureLogging(signalR.LogLevel.Information)
			.withAutomaticReconnect()
			.withUrl(environment.ws + 'channels')
			.build();

		connection.start().then(function () {
			console.log('Connected!');
		}
		).catch(function (err) {
			return console.error(err.toString());
		}
		);

		connection.on("ChannelMessage", (message: any) => {
			this.getChannels();
		}
		);

		connection.on("DeleteChannel", (message: any) => {
			console.log(message);
			setTimeout(() => {
				this.getChannels("delete");
			}, 3000);

		}
		);

		connection.on("Message", (message: any) => {
			this.notifications();
		}
		);

		connection.on('Notification', (message: any) => {
			console.log(message);
			this.notificationMess = message.Body;
			this.openSnackBar(message.Body);
			setTimeout(() => {
				this.closeSnackBar();
			}
				, 3000);
		}
		);
	}

	openSnackBar(message: string) {
		this._snackBar.open(message, '', {
			horizontalPosition: 'center',
			verticalPosition: 'top',
			duration: 2000,
			politeness: 'assertive',
			panelClass: ['snackbar']
		});
		this.getChannels();
	}

	closeSnackBar() {
		this._snackBar.dismiss();
	}
	ngOnDestroy() {
		this.notifications();
		this.connectToHub();
	}

	ngChanges() {
		this.notifications();
		this.connectToHub();
	}

	getChannels(type?: string) {
		this.channelsService.getChannelsByUserId().subscribe((result) => {
			if (type === "delete") {
				this.openSnackBar("Delete success");
			}
			this.channels = JSON.parse(JSON.stringify(result)).data;
			this.isHaveData = JSON.parse(JSON.stringify(result))?.Count > 0 ? true : false;
		})
	}

	constructor(
		private channelsService: ChannelsService,
		private authService: AuthService,
		private titleService: Title,
		private _snackBar: MatSnackBar
	) {
	}
	showMessages(_channel: any) {
		this.isShowMessages = false;
		this.channelId = _channel.id;
		this.isShowMessages = true;
	}
	getNotification() {
		this.channelsService.getNotification().subscribe((result) => {
			this.notifications = JSON.parse(JSON.stringify(result)).data;
			let count = JSON.parse(JSON.stringify(result)).Count;
			this.titleService.setTitle(count > 0 ? '(' + count + ') ' + 'Chat App' : 'Chat App');
			this.openSnackBar("You have " + this.notifications.length + " new messages");
		})
	}
	closeMessages() {
		this.isShowMessages = false;
		this.getChannels();
	}
	showPopup() {
		this.isCreate = true;
		this.authService.getUsers().subscribe((result) => {
			this.users = JSON.parse(JSON.stringify(result)).data.data;
		}
		)
	}
	onOptionsSelected(event: any) {
		event.forEach((element: any) => {
			this.selected.push(element.id
			);
		}
		);
	}
	onSubmit() {
		let cuserId = localStorage.getItem('userId') ? parseInt(localStorage.getItem('userId')!) : 0;
		this.channelsService.createChannel(
			{
				name: this.name.value,
				userId: cuserId,
				assignMember: this.userSelect.value
			}).subscribe((result) => {
				this.isCreate = false;
				this.getChannels();
				this.openSnackBar("Create success");
			})

	}

	deleteChannel(id: any) {
		this.channelsService.deleteChannel(id).subscribe((result) => {
			if (result) {
				this.getChannels();
			}
		})
	}

	closePopup() {
		this.isCreate = false;
	}
}
