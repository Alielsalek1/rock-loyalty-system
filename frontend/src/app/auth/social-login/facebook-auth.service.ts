import { Injectable } from '@angular/core';
import { environment } from '../../../env';

declare var FB: any;

@Injectable({
  providedIn: 'root',
})
export class FacebookAuthService {
  private sdkLoaded = false;

  constructor() {
    this.loadFacebookSDK();
  }

  private loadFacebookSDK(): Promise<void> {
    return new Promise((resolve) => {
      if (this.sdkLoaded) {
        resolve();
        return;
      }

      // Check if SDK script already exists
      const existingScript = document.getElementById('facebook-sdk');
      if (existingScript) {
        resolve();
        return;
      }

      // Load Facebook SDK
      const script = document.createElement('script');
      script.id = 'facebook-sdk';
      script.src = 'https://connect.facebook.net/en_US/sdk.js';
      script.async = true;
      script.defer = true;
      
      script.onload = () => {
        FB.init({
          appId: environment.facebookClientId,
          cookie: true,
          xfbml: true,
          version: 'v18.0'
        });
        this.sdkLoaded = true;
        resolve();
      };

      document.head.appendChild(script);
    });
  }

  async login(): Promise<any> {
    await this.loadFacebookSDK();
    
    return new Promise((resolve, reject) => {
      FB.login(
        (response: any) => {
          console.log('Facebook login response:', response);
          if (response.authResponse) {
            resolve(response);
          } else {
            reject('User cancelled login or did not fully authorize.');
          }
        },
        { scope: 'email' }
      );
    });
  }
}
