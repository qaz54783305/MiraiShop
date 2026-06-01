import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { AUTH_STORAGE_KEY } from './interceptors/auth.interceptor';

interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  public forecasts: WeatherForecast[] = [];
  isLoggedIn = false;
  isAdmin = false;
  isLoginPage = false;
  title = 'miraishop.client';

  constructor(private http: HttpClient, private router: Router) {}

  ngOnInit() {
    this.getForecasts();
    this.updateAuthState();
    // 每次路由變換後重新判斷（登入/登出後更新 header）
    this.router.events.pipe(
      filter(e => e instanceof NavigationEnd)
    ).subscribe((e) => {
      this.isLoginPage = (e as NavigationEnd).urlAfterRedirects === '/login';
      this.updateAuthState();
    });
  }

  private updateAuthState(): void {
    const raw = localStorage.getItem(AUTH_STORAGE_KEY);
    if (!raw) { this.isLoggedIn = false; this.isAdmin = false; return; }

    const auth = JSON.parse(raw) as { token: string; expiry: string };
    if (new Date(auth.expiry) <= new Date()) {
      localStorage.removeItem(AUTH_STORAGE_KEY);
      this.isLoggedIn = false; this.isAdmin = false; return;
    }

    this.isLoggedIn = true;
    this.isAdmin = this.hasAdminRole(auth.token);
  }

  private hasAdminRole(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const role: string | string[] | undefined =
        payload['role'] ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      if (!role) return false;
      return Array.isArray(role) ? role.includes('Admin') : role === 'Admin';
    } catch { return false; }
  }

  getForecasts() {
    this.http.get<WeatherForecast[]>('/weatherforecast').subscribe({
      error: (error) => console.error(error)
    });
  }
}
