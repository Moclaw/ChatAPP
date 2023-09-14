import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ChannelComponent } from './channel.component';
import { ChannelsListComponent } from '../channels-list/channels-list.component';
import { ChannelsService } from '../services/channels.service';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MessageComponent } from '../message/message.component';
import { FormsModule } from '@angular/forms';
import { NgSelectModule } from "@ng-select/ng-select";
import {
	MatSnackBarModule,
} from '@angular/material/snack-bar';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { ReactiveFormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { NgFor } from '@angular/common';
@NgModule({
	declarations: [
		ChannelComponent,
		ChannelsListComponent,
		MessageComponent
	],
	providers: [ChannelsService],
	bootstrap: [ChannelComponent],
	imports: [
		RouterModule.forChild([
			{
				path: '',
				component: ChannelComponent
			}
		]),
		CommonModule,
		FormsModule,
		NgSelectModule,
		MatSnackBarModule,
		MatButtonModule,
		MatSelectModule,
		MatFormFieldModule,
		ReactiveFormsModule,
		NgFor,
		MatInputModule
	],
	exports: [ChannelComponent],
	schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class ChannelModule {
	showMessage: boolean = false;
	constructor() {
		console.log("Channel Module Loaded");
	}
}
