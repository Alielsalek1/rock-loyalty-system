import { Injectable } from '@angular/core';
import { environment } from '../../../env';

declare var google: any;

// TODO: THIS IMPLEMENTATION SHOULD BE CHANGED ENTIRELY INTO OBSERVABLE PATTERN

@Injectable({
  providedIn: 'root',
})
export class GoogleAuthService {
  private initialized = false;

  constructor() {
    this.loadGoogleScript();
  }

  private loadGoogleScript(): Promise<void> {
    return new Promise((resolve, reject) => {
      if (this.initialized) {
        resolve();
        return;
      }

      // Check if script already exists
      const existingScript = document.querySelector('script[src*="gsi/client"]');
      if (existingScript) {
        this.initializeGoogleAuth().then(resolve).catch(reject);
        return;
      }

      const script = document.createElement('script');
      script.src = 'https://accounts.google.com/gsi/client';
      script.async = true;
      script.defer = true;
      script.onload = () => {
        this.initializeGoogleAuth().then(resolve).catch(reject);
      };
      script.onerror = () => reject(new Error('Failed to load Google Identity Services'));
      document.head.appendChild(script);
    });
  }

  private async initializeGoogleAuth(): Promise<void> {
    return new Promise((resolve) => {
      const checkGoogle = () => {
        if (typeof google !== 'undefined' && google.accounts) {
          this.initialized = true;
          resolve();
        } else {
          setTimeout(checkGoogle, 100);
        }
      };
      checkGoogle();
    });
  }

  async login(): Promise<any> {
    await this.loadGoogleScript();
    
    // Debug information
    console.log('Current origin:', window.location.origin);
    console.log('Client ID:', environment.googleClientId);
    
    return new Promise((resolve, reject) => {
      // Try using the popup-based sign-in instead of One Tap
      if (typeof google !== 'undefined' && google.accounts && google.accounts.oauth2) {
        const client = google.accounts.oauth2.initTokenClient({
          client_id: environment.googleClientId,
          scope: 'email profile',
          callback: (response: any) => {
            console.log('OAuth2 response:', response);
            if (response.access_token) {
              resolve({
                authentication: {
                  accessToken: response.access_token,
                  idToken: response.access_token
                },
                credential: response.access_token
              });
            } else {
              reject(new Error('No access token received'));
            }
          },
        });
        
        client.requestAccessToken();
      } else {
        // Fallback to One Tap
        google.accounts.id.initialize({
          client_id: environment.googleClientId,
          callback: (response: any) => {
            console.log('Google sign-in success:', response);
            resolve({
              authentication: {
                accessToken: response.credential,
                idToken: response.credential
              },
              credential: response.credential
            });
          },
          auto_select: false,
          cancel_on_tap_outside: true
        });

        // Use One Tap to show the sign-in prompt
        google.accounts.id.prompt((notification: any) => {
          console.log('Google prompt notification:', notification);
          if (notification.isNotDisplayed() || notification.isSkippedMoment()) {
            console.log('Google One Tap not available, reason:', notification.getNotDisplayedReason());
            reject(new Error('Google One Tap not available'));
          }
        });
      }
    });
  }

  async renderButton(element: HTMLElement): Promise<void> {
    await this.loadGoogleScript();
    
    return new Promise((resolve, reject) => {
      google.accounts.id.initialize({
        client_id: environment.googleClientId,
        callback: (response: any) => {
          // This will be handled by the component
          console.log('Google sign-in response:', response);
        },
        auto_select: false,
        cancel_on_tap_outside: true
      });

      google.accounts.id.renderButton(element, {
        type: 'standard',
        theme: 'outline',
        size: 'large',
        text: 'signin_with',
        shape: 'rectangular',
        width: 250
      });

      resolve();
    });
  }
}
