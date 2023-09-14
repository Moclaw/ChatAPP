import { Component } from '@angular/core';
import { MessageComponent } from '../message/message.component';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';
import * as signalR from '@microsoft/signalr';
import { environment } from 'src/environments/environment';
import { timeout } from 'rxjs';
@Component({
	selector: 'app-channel',
	templateUrl: './channel.component.html',
	styleUrls: ['./channel.component.css']
})
export class ChannelComponent {

}
