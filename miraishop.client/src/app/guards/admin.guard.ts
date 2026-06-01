import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AUTH_STORAGE_KEY } from '../interceptors/auth.interceptor';

@Injectable({ providedIn: 'root' })
export class AdminGuard implements CanActivate {

  constructor(private router: Router) {}

  canActivate(): boolean {
    const raw = localStorage.getItem(AUTH_STORAGE_KEY);
    if (!raw) {
      this.router.navigate(['/login']);
      return false;
    }

    const auth = JSON.parse(raw) as { token: string; expiry: string };
    if (new Date(auth.expiry) <= new Date()) {
      localStorage.removeItem(AUTH_STORAGE_KEY);
      this.router.navigate(['/login']);
      return false;
    }

    if (!this.hasAdminRole(auth.token)) {
      this.router.navigate(['/login']);
      return false;
    }

    return true;
  }

  private hasAdminRole(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const role: string | string[] | undefined =
        payload['role'] ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      if (!role) return false;
      return Array.isArray(role) ? role.includes('Admin') : role === 'Admin';
    } catch {
      return false;
    }
  }
}
