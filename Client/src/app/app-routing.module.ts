import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AuthenticationGuard } from './services/auth.guard';

const routes: Routes = [
	{
		path: 'login',
		loadChildren: () => import('./login/login.module').then(m => m.LoginModule),    // Lazy loading
	},
	{
		path: 'channels',
		loadChildren: () => import('./channel/channel.module').then(m => m.ChannelModule),    // Lazy loading
		canActivate: [AuthenticationGuard]
	},
	{
		path: '',
		redirectTo: '/login',
		pathMatch: 'full'
	}
];

@NgModule({
	imports: [RouterModule.forRoot(routes)],
	exports: [RouterModule]
})
export class AppRoutingModule { }