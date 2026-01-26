import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { toObservable } from '@angular/core/rxjs-interop';
import { filter, map, take } from 'rxjs/operators';

export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Wait for auth check to complete before deciding
  return toObservable(authService.loading).pipe(
    filter(loading => !loading), // Wait until loading is false
    take(1),
    map(() => {
      if (authService.isAuthenticated()) {
        return true;
      }
      // Redirect to home page if not authenticated
      router.navigate(['/']);
      return false;
    })
  );
};
