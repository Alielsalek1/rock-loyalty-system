import { jwtDecode } from 'jwt-decode';

export class User {
  public id: number;
  public email: string;
  public phonenumber: string;
  public name: string;
  private _token: string;
  private _tokenExpirationDate: Date;
  private _tokenDurationMs: number;

  constructor(id: number, email: string, phonenumber: string, name: string, token: string) {
    this.id = id;
    this.email = email;
    this.phonenumber = phonenumber;
    this.name = name;
    this.updateToken(token);
  }

  get token() {
    if (!this._tokenExpirationDate || new Date() > this._tokenExpirationDate) {
      return null;
    }
    return this._token;
  }

  updateToken(token: string) {
    try {

      if (!this._tokenDurationMs && !this._tokenExpirationDate) {
        this._token = token;

        const decodedToken: any = jwtDecode(token);
        this._tokenExpirationDate = new Date(decodedToken.exp * 1000); 

        const issuedAt = decodedToken.iat ? decodedToken.iat * 1000 : new Date().getTime();
        this._tokenDurationMs = (decodedToken.exp * 1000) - issuedAt;
        
      } else if (this._tokenDurationMs) {
        this._token = token;
        this._tokenExpirationDate = new Date(new Date().getTime() + this._tokenDurationMs);
      }

    } catch (error) {
      this._token = null;
      this._tokenExpirationDate = null;
      this._tokenDurationMs = 0;
      console.error('Invalid token:', error);
    }
  }

  get expirationDate() {
    return this._tokenExpirationDate;
  }

}
