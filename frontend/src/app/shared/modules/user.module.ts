import { jwtDecode } from 'jwt-decode';

export class User {
  public id: number;
  public email: string;
  public phonenumber: string;
  public name: string;
  private _tokenExpirationDate: Date;
  private _tokenDurationMs: number;

  constructor(private _token: string) {

    try {
      const decodedToken: any = jwtDecode(_token);
      this._tokenExpirationDate = new Date(decodedToken.exp * 1000);

      const issuedAt = decodedToken.iat ? decodedToken.iat * 1000 : new Date().getTime();
      this._tokenDurationMs = (decodedToken.exp * 1000) - issuedAt;

    } catch (error) {
      this._tokenDurationMs = 35 * 60 * 1000; // 35 minutes in MS
      this._tokenExpirationDate = new Date(new Date().getTime() + this._tokenDurationMs);
    }

  }

  get token() {
    if (!this._tokenExpirationDate || new Date() > this._tokenExpirationDate) {
      return null;
    }
    return this._token;
  }

  get expirationDate() {
    return this._tokenExpirationDate;
  }
}
