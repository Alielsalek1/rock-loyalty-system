import { Injectable } from '@angular/core';
import { enviroment } from '../../../env';
import { GoogleAuth } from '@codetrix-studio/capacitor-google-auth';

@Injectable({
  providedIn: 'root',
})
export class GoogleAuthService {
  private gapiLoaded: Promise<any>;

  constructor() {
    this.loadGapi();
  }

  private loadGapi() {
    GoogleAuth.initialize({
      clientId: enviroment.googleClientId,
      scopes: ['profile', 'email'],
      grantOfflineAccess: true,
    });
  }

  async login(): Promise<any> {
    return GoogleAuth.signIn();
  }
}
