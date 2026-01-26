import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, of, tap } from 'rxjs';

export interface User {
  id: string;
  name: string;
  email: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly userSignal = signal<User | null>(null);
  private readonly loadingSignal = signal(true);
  private readonly errorSignal = signal<string | null>(null);

  readonly user = this.userSignal.asReadonly();
  readonly loading = this.loadingSignal.asReadonly();
  readonly error = this.errorSignal.asReadonly();
  readonly isAuthenticated = computed(() => this.userSignal() !== null);

  constructor(private http: HttpClient) {
    this.checkAuth();
  }

  checkAuth(): void {
    this.loadingSignal.set(true);
    this.errorSignal.set(null);

    this.http.get<User>('/api/auth/user').pipe(
      tap(user => {
        this.userSignal.set(user);
        this.loadingSignal.set(false);
      }),
      catchError(err => {
        this.userSignal.set(null);
        this.loadingSignal.set(false);
        // 401 is expected when not logged in, not an error
        if (err.status !== 401) {
          this.errorSignal.set('Failed to check authentication status');
        }
        return of(null);
      })
    ).subscribe();
  }

  login(returnUrl: string = '/'): void {
    // Redirect to backend login endpoint which will initiate Google OAuth
    window.location.href = `/api/auth/login?returnUrl=${encodeURIComponent(returnUrl)}`;
  }

  logout(): void {
    // Redirect to backend logout endpoint
    window.location.href = '/api/auth/logout';
  }
}
