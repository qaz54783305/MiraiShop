import { Component } from '@angular/core';
import { AUTH_STORAGE_KEY } from '../interceptors/auth.interceptor';

@Component({
  selector: 'app-welcome',
  templateUrl: './welcome.component.html',
  styleUrls: ['./welcome.component.css']
})
export class WelcomeComponent {

  private get jwtPayload(): Record<string, string> {
    const raw = localStorage.getItem(AUTH_STORAGE_KEY);
    if (!raw) return {};
    try {
      const auth = JSON.parse(raw) as { token: string };
      // base64url → base64 → Uint8Array → UTF-8 string
      const base64 = auth.token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
      const bytes = Uint8Array.from(atob(base64), c => c.charCodeAt(0));
      return JSON.parse(new TextDecoder('utf-8').decode(bytes));
    } catch { return {}; }
  }

  logout() {
    localStorage.removeItem(AUTH_STORAGE_KEY);
    window.location.href = '/login';
  }


  get memberName(): string  { return this.jwtPayload['name']  ?? ''; }
  get memberEmail(): string { return this.jwtPayload['email'] ?? ''; }

}
