import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AUTH_STORAGE_KEY } from '../interceptors/auth.interceptor';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {

  constructor(private router: Router) {}

  canActivate(): boolean {
    const raw = localStorage.getItem(AUTH_STORAGE_KEY);
    if (!raw) {
      this.router.navigate(['/login']);
      return false;
    }

    const auth = JSON.parse(raw) as { expiry: string };
    if (new Date(auth.expiry) <= new Date()) {
      localStorage.removeItem(AUTH_STORAGE_KEY);
      this.router.navigate(['/login']);
      return false;
    }

    return true;
  }
}
