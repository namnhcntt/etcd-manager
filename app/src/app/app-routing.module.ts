import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AppMainComponent } from './app.main.component';
import { AccessComponent } from './components/access/access.component';
import { ErrorComponent } from './components/error/error.component';
import { LandingComponent } from './components/landing/landing.component';
import { NotfoundComponent } from './components/notfound/notfound.component';
import { ConnectionManagerComponent } from './pages/connection-manager/connection-manager.component';
import { HomeComponent } from './pages/home/home.component';
import { LoginComponent } from './pages/login/login.component';
import { UserManagerComponent } from './pages/user-manager/user-manager.component';
@NgModule({
    imports: [
        RouterModule.forRoot([
            {
                path: '', component: AppMainComponent,
                children: [
                    { path: '', component: HomeComponent },
                    { path: 'connection-manager', component: ConnectionManagerComponent },
                    { path: 'user-manager', component: UserManagerComponent },
                ],
            },
            { path: 'login', component: LoginComponent },
            { path: 'pages/landing', component: LandingComponent },
            { path: 'pages/error', component: ErrorComponent },
            { path: 'pages/notfound', component: NotfoundComponent },
            { path: 'pages/access', component: AccessComponent },
            { path: '**', redirectTo: 'pages/notfound' },
        ], { scrollPositionRestoration: 'enabled', anchorScrolling: 'enabled' })
    ],
    exports: [RouterModule]
})
export class AppRoutingModule {
}
