import { Injectable } from '@angular/core';
import { enviroment } from '../../../env';
declare var FB;
@Injectable({
  providedIn: 'root',
})
export class FacebookAuthService {
  constructor() {}

  login(): Promise<any> {
    return new Promise((resolve, reject) => {
      FB.login(
        (response: any) => {
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
