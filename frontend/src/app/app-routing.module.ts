import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthComponent } from './auth/auth.component';
import { LoginComponent } from './auth/login/login.component';
import { RegisterComponent } from './auth/register/register.component';
import { MainComponent } from './main/main.component';
import { authGuard } from './shared/guards/auth.guard';
import { NotFoundComponent } from './shared/components/not-found/not-found.component';
import { phoneGuardGuard } from './shared/guards/phone-guard.guard';
import { notAuthGuard } from './shared/guards/not-auth.guard';
import { PhoneComponent } from './auth/phone/phone.component';
import { notPhoneGuard } from './shared/guards/not-phone.guard';
import { ConfirmEmailComponent } from './auth/confirm-email/confirm-email.component';
import { ForgetPasswordComponent } from './auth/forget-password/forget-password.component';
import { ForgetPasswordRequestComponent } from './auth/forget-password-request/forget-password-request.component';
import { ExchangePointsComponent } from './main/exchange-points/exchange-points.component';
import { PointsComponent } from './main/points/points.component';
import { VouchersComponent } from './main/vouchers/vouchers.component';
import { ProfileComponent } from './main/profile/profile.component';
import { AppComponent } from './app.component';

const routes: Routes = [
  {
    path: ':restId',
    component: AppComponent,
    children: [
      {
        path: '',
        redirectTo: 'main',
        pathMatch: 'full',
      },
      {
        path: 'auth',
        component: AuthComponent,
        children: [
          {
            path: '',
            redirectTo: 'login',
            pathMatch: 'full',
          },
          {
            path: 'login',
            component: LoginComponent,
            canActivate: [notAuthGuard],
          },
          {
            path: 'register',
            component: RegisterComponent,
            canActivate: [notAuthGuard],
          },
          {
            path: 'phone',
            component: PhoneComponent,
            canActivate: [authGuard, notPhoneGuard],
          },
          {
            path: 'forget-password-request',
            component: ForgetPasswordRequestComponent,
            canActivate: [authGuard],
          },
          {
            path: 'confirm-email/:token',
            component: ConfirmEmailComponent,
          },
          {
            path: 'forget-request',
            component: ForgetPasswordRequestComponent,
          },
          {
            path: 'forget-password/:token',
            component: ForgetPasswordComponent,
          },
        ],
      },
      {
        path: 'main',
        component: MainComponent,
        canActivate: [authGuard, phoneGuardGuard],
        children: [
          {
            path: '',
            component: ExchangePointsComponent,
          },
          {
            path: 'points',
            component: PointsComponent,
          },
          {
            path: 'vouchers',
            component: VouchersComponent,
          },
          {
            path: 'profile',
            component: ProfileComponent,
          },
        ],
      },
    ],
  },
  {
    path: '**',
    component: NotFoundComponent,
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
