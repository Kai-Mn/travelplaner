import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { signalStore, withState, withMethods, withComputed, patchState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { tapResponse } from '@ngrx/operators';
import { pipe, switchMap } from 'rxjs';
import { computed } from '@angular/core';
import { ApiService } from '../../core/services/api.service';

interface AuthState {
  token: string | null;
  email: string | null;
  userId: string | null;
  loading: boolean;
  error: string | null;
}

const TOKEN_KEY = 'tp_token';
const EMAIL_KEY = 'tp_email';
const USER_ID_KEY = 'tp_userId';

function loadFromStorage(): Partial<AuthState> {
  return {
    token: localStorage.getItem(TOKEN_KEY),
    email: localStorage.getItem(EMAIL_KEY),
    userId: localStorage.getItem(USER_ID_KEY),
  };
}

export const AuthStore = signalStore(
  { providedIn: 'root' },
  withState<AuthState>({
    token: null,
    email: null,
    userId: null,
    loading: false,
    error: null,
    ...loadFromStorage()
  }),
  withComputed((state) => ({
    isAuthenticated: computed(() => !!state.token()),
  })),
  withMethods((store) => {
    const api = inject(ApiService);
    const router = inject(Router);

    function persistAuth(token: string, email: string, userId: string) {
      localStorage.setItem(TOKEN_KEY, token);
      localStorage.setItem(EMAIL_KEY, email);
      localStorage.setItem(USER_ID_KEY, userId);
      patchState(store, { token, email, userId, loading: false, error: null });
    }

    return {
      register: rxMethod<{ email: string; password: string }>(
        pipe(
          switchMap(({ email, password }) => {
            patchState(store, { loading: true, error: null });
            return api.register(email, password).pipe(
              tapResponse({
                next: (res) => { persistAuth(res.token, res.email, res.userId); router.navigate(['/map']); },
                error: (err: any) => patchState(store, { loading: false, error: err?.error?.detail ?? 'Registration failed' })
              })
            );
          })
        )
      ),

      login: rxMethod<{ email: string; password: string }>(
        pipe(
          switchMap(({ email, password }) => {
            patchState(store, { loading: true, error: null });
            return api.login(email, password).pipe(
              tapResponse({
                next: (res) => { persistAuth(res.token, res.email, res.userId); router.navigate(['/map']); },
                error: (err: any) => patchState(store, { loading: false, error: err?.error?.detail ?? 'Invalid credentials' })
              })
            );
          })
        )
      ),

      logout() {
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(EMAIL_KEY);
        localStorage.removeItem(USER_ID_KEY);
        patchState(store, { token: null, email: null, userId: null });
        router.navigate(['/auth/login']);
      }
    };
  })
);
