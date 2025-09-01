import {
  HttpHandlerFn,
  HttpInterceptorFn,
  HttpRequest,
} from '@angular/common/http';
import { environment } from '../../../env';

export const ApiKeyInterceptor: HttpInterceptorFn = (
  req: HttpRequest<any>,
  next: HttpHandlerFn
) => {
  const isBackendRequest = req.url.includes(environment.apiUrl) ||
    req.url.startsWith('http://localhost:5152') ||
    req.url.startsWith('https://localhost:7219');

  if (isBackendRequest) {
    const modifiedReq = req.clone({
      headers: req.headers.set('x-apikey', environment.apiKey)
    });
    return next(modifiedReq);
  }

  // for google Auth2
  return next(req);
}