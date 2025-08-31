import { Injectable } from '@angular/core';
import { environment } from '../../../env';
import { Observable, BehaviorSubject, from, throwError, of } from 'rxjs';
import { map, switchMap, catchError, tap } from 'rxjs/operators';
import { GoogleUserInfo, AuthenticationResult } from './google-interfaces';

@Injectable({
  providedIn: 'root',
})
export class GoogleAuthService {
  private readonly scriptLoaded$ = new BehaviorSubject<boolean>(false);
  private readonly userProfile$ = new BehaviorSubject<GoogleUserInfo | null>(null);
  private readonly isAuthenticated$ = new BehaviorSubject<boolean>(false);

  constructor() {
    this.loadGoogleScript().subscribe();
  }

  // Public observables
  get scriptLoaded(): Observable<boolean> {
    return this.scriptLoaded$.asObservable();
  }

  get userProfile(): Observable<GoogleUserInfo | null> {
    return this.userProfile$.asObservable();
  }

  get isAuthenticated(): Observable<boolean> {
    return this.isAuthenticated$.asObservable();
  }

  // Load Google script as Observable
  private loadGoogleScript(): Observable<boolean> {
    if (this.scriptLoaded$.value) {
      return of(true);
    }

    const existingScript = document.querySelector('script[src*="gsi/client"]');
    if (existingScript) {
      return this.initializeGoogleAuth();
    }

    return new Observable<boolean>(observer => {
      const script = document.createElement('script');
      script.src = 'https://accounts.google.com/gsi/client';
      script.async = true;
      script.defer = true;

      script.onload = () => {
        this.initializeGoogleAuth().subscribe({
          next: (initialized) => {
            observer.next(initialized);
            observer.complete();
          },
          error: (error) => observer.error(error)
        });
      };

      script.onerror = () => {
        observer.error(new Error('Failed to load Google Identity Services'));
      };

      document.head.appendChild(script);
    });
  }

  private initializeGoogleAuth(): Observable<boolean> {
    return new Observable<boolean>(observer => {
      const checkGoogle = () => {
        if (this.isGoogleLoaded()) {
          this.scriptLoaded$.next(true);
          observer.next(true);
          observer.complete();
        } else {
          setTimeout(checkGoogle, 100);
        }
      };
      checkGoogle();
    });
  }

  private isGoogleLoaded(): boolean {
    return !!(window.google && window.google.accounts && window.google.accounts.oauth2);
  }

  // main login method
  login(): Observable<AuthenticationResult> {
    return this.ensureScriptLoaded().pipe(
      switchMap(() => this.performLogin()),
      switchMap((tokenResponse) => this.getUserInfo(tokenResponse.access_token).pipe(
        map(userInfo => ({
          authentication: {
            accessToken: tokenResponse.access_token,
            idToken: tokenResponse.access_token
          },
          credential: tokenResponse.access_token,
          userInfo
        }))
      )),
      tap((result) => {
        this.userProfile$.next(result.userInfo || null);
        this.isAuthenticated$.next(true);
      }),
      catchError(error => {
        console.error('Google login failed:', error);
        this.isAuthenticated$.next(false);
        return throwError(() => error);
      })
    );
  }

  private ensureScriptLoaded(): Observable<boolean> {
    return this.scriptLoaded$.value ? of(true) : this.loadGoogleScript();
  }


  private performLogin(): Observable<google.accounts.oauth2.TokenResponse> {
    return new Observable<google.accounts.oauth2.TokenResponse>(observer => {
      if (!this.isGoogleLoaded()) {
        observer.error(new Error('Google API not loaded'));
        return;
      }

      try {
        const client = window.google!.accounts.oauth2.initTokenClient({
          client_id: environment.googleClientId,
          scope: 'openid profile email',
          callback: (tokenResponse: google.accounts.oauth2.TokenResponse) => {
            if (tokenResponse.access_token) {
              observer.next(tokenResponse);
              observer.complete();
            } else {
              observer.error(new Error('No access token received'));
            }
          },
        });

        client.requestAccessToken();
      } catch (error) {
        observer.error(error);
      }
    });
  }

  private getUserInfo(accessToken: string): Observable<GoogleUserInfo> {
    const userInfoUrl = `https://www.googleapis.com/oauth2/v1/userinfo?access_token=${accessToken}`;

    return from(fetch(userInfoUrl)).pipe(
      switchMap(response => {
        if (!response.ok) {
          throw new Error('Failed to fetch user info');
        }
        return from(response.json());
      }),
      map((userInfo: GoogleUserInfo) => userInfo)
    );
  }

  logout(): Observable<void> {
    return new Observable<void>(observer => {
      if (this.isGoogleLoaded()) {
        window.google!.accounts.id.disableAutoSelect();
      }

      this.userProfile$.next(null);
      this.isAuthenticated$.next(false);

      observer.next();
      observer.complete();
    });
  }
}