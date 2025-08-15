export class User {
  public id: number;
  public email: string;
  public phonenumber: string;
  public name: string;
  private _tokenExpirationDate: Date;

  constructor(private _token: string, expireDate: Date = null) {
    if (expireDate) {
      this._tokenExpirationDate = expireDate;
      return;
    }
    this._tokenExpirationDate = new Date(new Date().getTime() + 29 * 60 * 1000);
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
