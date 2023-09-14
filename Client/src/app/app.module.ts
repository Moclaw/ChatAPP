import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from './services/auth.service';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { AuthenticationGuard } from './services/auth.guard';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { TokenInterceptor } from './services/token-interceptor.service';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { NgxPageScrollCoreModule } from 'ngx-page-scroll-core';
import {MatCardModule} from '@angular/material/card';
@NgModule({
	declarations: [
		AppComponent,
	],
	imports: [
		BrowserModule,
		AppRoutingModule,
		FormsModule,
		ReactiveFormsModule,
		HttpClientModule,
		BrowserAnimationsModule,
		MatProgressSpinnerModule,
		NgxPageScrollCoreModule.forRoot({ duration: 1600 }),
		MatCardModule
	],
	providers: [
		AuthService,
		AuthenticationGuard,
		{
			provide: HTTP_INTERCEPTORS,
			useClass: TokenInterceptor,
			multi: true,
		}
	],
	bootstrap: [AppComponent]
})
export class AppModule { }
