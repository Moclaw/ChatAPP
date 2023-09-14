import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';

@Injectable({
	providedIn: 'root'
})
export class ChannelsService {
	userId: number = 0;
	constructor(private http: HttpClient) {
		this.userId = localStorage.getItem('userId') ? parseInt(localStorage.getItem('userId')!) : 0;
	}
	url = environment.apiUrl + 'channel';
	urlNoification = environment.apiUrl + 'notification';
	getChannels() {
		return this.http.get(this.url);
	}

	getChannelsByUserId() {
		return this.http.get(this.url + '/' + this.userId + '/channel');
	}

	getChannelById(id: any) {
		return this.http.get(this.url + '/' + id);
	}

	createChannel(channel: any) {
		return this.http.post(this.url, channel);
	}

	updateChannel(channel: any) {
		return this.http.put(this.url + '/' + channel.id, channel);
	}

	deleteChannel(id: any) {
		return this.http.delete(this.url + '?channelId=' + id);
	}

	getChannelByUserId(id: any) {
		return this.http.get(this.url + '/user/' + id);
	}
	getMessages(id: any) {
		return this.http.get(this.url + '/' + id + '/messages');
	}
	getMessagesByUserId(id: any) {
		return this.http.get(this.url + '/' + this.userId + '/channel/' + id + '/messages');
	}
	sendMessage(id: any, content: any) {
		return this.http.post(this.url + '/' + id + '/messages', content);
	}
	getNotification() {
		return this.http.get(this.urlNoification);
	}

	updateStaus(id: any) {
		return this.http.put(this.url + '/' + id + '/updateStatus', null);
	}

}
