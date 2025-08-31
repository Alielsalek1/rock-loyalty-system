
/// <reference types="google.accounts" />

export interface GoogleUserInfo {
    id: string;
    email: string;
    name: string;
    picture: string;
    given_name: string;
    family_name: string;
}

export interface AuthenticationResult {
    authentication: {
        accessToken: string;
        idToken: string;
    };
    credential: string;
    userInfo?: GoogleUserInfo;
}


declare global {
    interface Window {
        google?: typeof google;
    }
}