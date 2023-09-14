import { Component, ElementRef, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ChannelsService } from '../services/channels.service';
import * as signalR from '@microsoft/signalr';
import { environment } from 'src/environments/environment';
import { MatSnackBar, MatSnackBarHorizontalPosition, MatSnackBarVerticalPosition } from '@angular/material/snack-bar';


@Component({
	selector: 'app-message',
	templateUrl: './message.component.html',
	styleUrls: ['./message.component.css'],
})

export class MessageComponent implements OnInit {
	@Input() channelId: any = 0;
	channel: any;
	users: any = [];
	messages: any = [];
	isSendding = false;
	content = "";
	type = "text";
	horizontalPosition: MatSnackBarHorizontalPosition = 'start';
	verticalPosition: MatSnackBarVerticalPosition = 'bottom';
	userId = localStorage.getItem('userId') ? parseInt(localStorage.getItem('userId')!) : 0;
	name: string = "";
	@ViewChild('messageContainer') private scrollBottom!: ElementRef;
	constructor(private channelsService: ChannelsService, private _snackBar: MatSnackBar) {

	}
	openSnackBar() {
		this._snackBar.open('Loadding', 'Close', {
			horizontalPosition: this.horizontalPosition,
			verticalPosition: this.verticalPosition,
		});
	}

	closeSnackBar() {
		this._snackBar.dismiss();
	}
	ngOnInit(): void {
		this.getChannel();
		this.updateStatus();
		this.channelsService.getMessages(this.channelId).subscribe((result) => {
			let data = JSON.parse(JSON.stringify(result)).data;
			this.messages = data;
			this.updateScroll();
		}, (error) => {
			console.log(error);
		});
	}

	getMessages() {
		const connection = new signalR.HubConnectionBuilder()
			.configureLogging(signalR.LogLevel.Information)
			.withUrl(environment.ws + 'channels')
			.withAutomaticReconnect([0, 1000, 5000, 10000, 30000])
			.build();

		connection.start().then(function () {
			console.log('Connected!');
		}
		).catch(function (err) {
			return console.error(err.toString());
		}
		);

		connection.on("Message", (message: any) => {
			if (message.userId == this.userId && !this.IsCheckMessageHadInList(message)) {
				this.updateStatus();
				message.senderStatus = "Seen";
				this.messages.push(message);
			}
		}
		);
	}

	IsCheckMessageHadInList(message: any) {
		for (let i = 0; i < this.messages.length; i++) {
			if (this.messages[i].id == message.id) {
				return true;
			}
		}
		return false;
	}


	getChannel() {
		this.channelsService.getChannelById(this.channelId).subscribe((result) => {
			let data = JSON.parse(JSON.stringify(result)).data;
			this.users = data.users;
			let _name = "";
			for (let i = 0; i < this.users.length; i++) {
				_name += this.users[i].username + ", ";
			}
			_name = _name.substring(0, _name.length - 2);
			this.name = _name;
			this.channel = data.channel;
		}, (error) => {
			console.log(error);
		}
		);
	}

	sendMessage() {
		this.openSnackBar();
		let userId = localStorage.getItem('userId') ? parseInt(localStorage.getItem('userId')!) : 0;
		let message = {
			content: this.content,
			userId: userId,
			type: this.getType(this.content),
			sendTime: new Date()
		}
		this.channelsService.sendMessage(this.channelId, message).subscribe((result) => {
			if (result) {
				this.content = "";
				this.closeSnackBar();
			}
		})

		this.getMessages();

	}

	updateScroll() {
		this.scrollBottom.nativeElement.scrollTop = this.scrollBottom.nativeElement.scrollHeight;
	}

	updateStatus() {
		this.channelsService.updateStaus(this.channelId).subscribe((result) => {
		}, (error) => {
			console.log(error);
		});
	}

	getType(content: any) {
		content = content.trim();
		if (content.startsWith("http") && (content.endsWith(".png") || content.endsWith(".jpg") || content.endsWith(".jpeg"))) {
			return "image";
		}

		if (content.startsWith("http") && (content.endsWith(".mp4") || content.endsWith(".avi") || content.endsWith(".mov"))) {
			return "video";
		}

		if (content.startsWith("http") && (content.endsWith(".mp3") || content.endsWith(".wav") || content.endsWith(".flac"))) {
			return "audio";
		}

		return "text";

	}
}
