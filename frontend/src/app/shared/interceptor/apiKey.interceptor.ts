import {
  HttpHandlerFn,
  HttpInterceptorFn,
  HttpRequest,
} from '@angular/common/http';
import { enviroment } from '../../../env';

export const ApiKeyInterceptor: HttpInterceptorFn = (
  req: HttpRequest<any>,
  next: HttpHandlerFn
) => {
  let modifiedReq: HttpRequest<any> = req;
  modifiedReq = req.clone({
    headers: req.headers.append('X-ApiKey', `${enviroment.apiKey}`),
  });
  return next(modifiedReq);
};
