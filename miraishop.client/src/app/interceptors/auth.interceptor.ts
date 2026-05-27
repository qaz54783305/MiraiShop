import { Injectable } from '@angular/core';
import {
  HttpInterceptor, HttpRequest, HttpHandler,
  HttpEvent, HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';

export const AUTH_STORAGE_KEY = 'miraishop_auth';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  constructor(private router: Router) {}

  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const raw = localStorage.getItem(AUTH_STORAGE_KEY);

    if (raw) {
      const auth = JSON.parse(raw) as { token: string; expiry: string };

      if (new Date(auth.expiry) <= new Date()) {
        localStorage.removeItem(AUTH_STORAGE_KEY);
        this.router.navigate(['/login']);
        return throwError(() => new Error('Token expired'));
      }

      req = req.clone({
        setHeaders: { Authorization: `Bearer ${auth.token}` }
      });
    }

    return next.handle(req).pipe(
      catchError((err: HttpErrorResponse) => {
        if (err.status === 401) {
          localStorage.removeItem(AUTH_STORAGE_KEY);
          this.router.navigate(['/login']);
        }
        return throwError(() => err);
      })
    );
  }
}
