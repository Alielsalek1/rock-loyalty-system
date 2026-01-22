# ROCK Loyalty System

The ROCK Loyalty System generates vouchers based on customer points, which increase with each transaction.
## Features

- Generate vouchers based on customer points.
- Points increase with each customer transaction.
- Works with multiple restaurants, each identified by a unique ID.
- Frontend built with Angular for customer interaction.

## Running the Backend (ASP.NET Core API)

1. Clone the repository:

    ```bash
    git clone https://github.com/RealOrangeKun/rock-loyalty-system/
    cd rock-loyalty-system/backend
    ```

2. Restore the dependencies and build the project:

    ```bash
    dotnet restore
    dotnet build
    ```

3. Create a `.env` file in the project directory. Below is a [breakdown](#env-file-configuration) of the required environment variables.

4. Run the application:

    ```bash
    dotnet run
    ```

### .env File Configuration

- **JWT Settings**
  - `JwtOptions__SigningKey`: The secret key used for signing JWT tokens to ensure their integrity.

- **Rock API **
  - `API__UserId`: The user ID for authenticating with the ROCK API.
  - `API__Password`: The password for authenticating with the ROCK API.

- **OAuth2 Settings (Facebook & Google)**
  - `FacebookOptions__AppId`: The Facebook App ID for OAuth2 authentication.
  - `FacebookOptions__AppSecret`: The Facebook App Secret for OAuth2 authentication.

- **Rock User**
  - `AdminOptions__Username`: The username for logging in to the ROCK loyalty system as an admin.
  - `AdminOptions__Password`: The password for the ROCK loyalty system as an admin user.

- **Email Options**
  - `EmailOptions__BaseUrl`: The base URL of the email service used for notifications.
  - `EmailOptions__Name`: The display name for the email sender.
  - `EmailOptions__Key`: The API key for accessing the email service.

- **Frontend Options**
  - `FrontendOptions__BaseUrl`: The base URL for the Angular frontend.

The backend should now be running at `http://localhost:5152` or the port specified in your settings.

## Frontend (Angular)

1. Navigate to the frontend folder:

    ```bash
    cd ../frontend
    ```

2. Build the frontend for production:

    ```bash
    ng build --prod
    ```

3. Deploy the contents of the `dist` folder to a static file server (e.g., using FTP, SCP, or cloud providers like AWS S3, Firebase Hosting, etc.).

4. The frontend relies on an environment configuration, which you should update as needed. Below is a sample `env.ts` file:

    ```typescript
    export const environment = {
      apiUrl: 'http://localhost:5152', // Backend API URL
      googleClientId: 'your-google-client-id', // Google OAuth2 Client ID
      facebookClientId: 'your-facebook-client-id', // Facebook OAuth2 Client ID
      apiKey: 'your-api-key', // API key for request authentication
    };
    ```

5. Serve the Angular application:

    ```bash
    ng serve
    ```

6. Access the frontend at:

    ```http://localhost:4200```
