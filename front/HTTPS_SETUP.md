# HTTPS Setup for Local Development

## Overview

This project uses HTTPS for both backend and frontend to ensure secure communication:
- Backend: `https://localhost:7130`
- Frontend: `https://localhost:5174`
  
## Certificate Setup for Local Development

This project uses custom SSL certificates for local HTTPS. The certificates are stored in the `front/cert/` directory:

- `cert.pem` - The SSL certificate file
- `key.pem` - The private key file

### Generating Certificates

Generate self-signed certificates for localhost development using OpenSSL:

```bash
cd front/cert
openssl req -x509 -newkey rsa:2048 -keyout key.pem -out cert.pem -days 365 -nodes \
  -subj "/CN=localhost" -addext "subjectAltName=DNS:localhost,IP:127.0.0.1"
```

These certificates are specifically created for `localhost` and will work on your local machine.

## Why HTTPS?

Using HTTPS even in development provides several benefits:

1. **Security**: Encrypted communication between frontend and backend
2. **Consistency with production**: Mimics the production environment
3. **Avoiding mixed content warnings**: Prevents browser warnings when making API calls
4. **Modern web features**: Some browser features require HTTPS

## Running the Application

### Starting the Frontend

```bash
cd front
npm run dev
```

This will start the frontend server on HTTPS (typically `https://localhost:5174`)

### Browser Security Warning

When first accessing the site, you'll see a security warning about the self-signed certificate:

![Browser Security Warning](https://developer.mozilla.org/en-US/docs/Web/Security/Certificate_Transparency/not_secure.png)

This is **normal and expected** for local development with self-signed certificates.

To proceed:
1. Click "Advanced" or "Details" in the browser warning
2. Click "Proceed to localhost (unsafe)" or similar option
3. The browser will remember this choice for your current session

## How It Works

The HTTPS configuration in `vite.config.ts` uses the certificates we generated:

```typescript
import fs from 'fs';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  server: {
    https: {
      key: fs.readFileSync(path.resolve(__dirname, 'cert/key.pem')),
      cert: fs.readFileSync(path.resolve(__dirname, 'cert/cert.pem')),
    },
    port: 5173,
  },
});
```

This configuration:
1. Points Vite to our custom self-signed certificates in the `cert/` folder
2. Sets up the development server to use HTTPS
3. Attempts to use port 5173 (may use another port if 5173 is busy)

## API Communication

Our API service (`api.service.ts`) is configured to communicate with the backend at `https://localhost:7130`. With the frontend also on HTTPS, there are no mixed-content warnings or security issues.

## Regenerating Certificates (If Needed)

If you need to regenerate the certificates (for example, if they expire or have issues), run:

```bash
# 1. Create the certificate directory if it doesn't exist
mkdir -p front/cert

# 2. Generate new certificates
cd front/cert
openssl req -x509 -newkey rsa:2048 -keyout key.pem -out cert.pem -days 365 -nodes \
  -subj "/CN=localhost" \
  -addext "subjectAltName=DNS:localhost,IP:127.0.0.1"
```

The parameters explained:
- `-x509`: Generate a self-signed certificate
- `-newkey rsa:2048`: Create a new 2048-bit RSA key
- `-keyout key.pem`: Where to save the private key
- `-out cert.pem`: Where to save the certificate
- `-days 365`: Certificate validity period
- `-nodes`: No passphrase for the private key
- `-subj "/CN=localhost"`: Set the Common Name to localhost
- `-addext "subjectAltName=..."`: Add localhost and 127.0.0.1 as alternative names

## Advanced: Using Trusted Local Certificates (Optional)

For an even better development experience without browser warnings, you can use a tool like [mkcert](https://github.com/FiloSottile/mkcert) to create locally-trusted certificates:

1. Install mkcert
2. Run `mkcert -install`
3. Create certificates for localhost:
   ```bash
   cd front/cert
   mkcert localhost 127.0.0.1 ::1
   mv localhost+2.pem cert.pem
   mv localhost+2-key.pem key.pem
   ```
4. Restart the development server

## Troubleshooting

### Certificate Issues

If you experience certificate issues:
1. Clear browser cache and cookies
2. Restart your development server
3. Make sure both frontend and backend use HTTPS

### CORS Issues

If you see CORS errors in the console:
1. Check that the backend is properly configured to allow requests from `https://localhost:5173`
2. Ensure both frontend and backend are running on HTTPS
