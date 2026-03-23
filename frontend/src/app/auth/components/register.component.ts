import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthStore } from '../store/auth.store';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-100">
      <div class="bg-white p-8 rounded-lg shadow-md w-full max-w-md">
        <h1 class="text-2xl font-bold mb-2" style="color: var(--color-primary)">TRAVELPLANER</h1>
        <h2 class="text-lg text-gray-600 mb-6">Create your account</h2>

        @if (auth.error()) {
          <div class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded mb-4">
            {{ auth.error() }}
          </div>
        }

        <form [formGroup]="form" (ngSubmit)="submit()" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Email</label>
            <input formControlName="email" type="email" placeholder="you@example.com"
              class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2">
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Password</label>
            <input formControlName="password" type="password" placeholder="••••••••"
              class="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2">
          </div>
          <button type="submit" [disabled]="form.invalid || auth.loading()"
            class="w-full text-white py-2 px-4 rounded-md font-semibold transition-opacity disabled:opacity-50"
            style="background-color: var(--color-primary)">
            {{ auth.loading() ? 'Creating account...' : 'Create Account' }}
          </button>
        </form>

        <p class="mt-4 text-center text-sm text-gray-600">
          Already have an account?
          <a routerLink="/auth/login" class="font-medium" style="color: var(--color-primary)">Sign in</a>
        </p>
      </div>
    </div>
  `
})
export class RegisterComponent {
  auth = inject(AuthStore);
  form = inject(FormBuilder).nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]]
  });

  submit() {
    if (this.form.valid) {
      this.auth.register(this.form.getRawValue());
    }
  }
}
