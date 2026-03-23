import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthStore } from '../../auth/store/auth.store';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  template: `
    <!-- ── DESKTOP ── -->
    <div class="hidden md:flex h-screen w-full overflow-hidden">

      <!-- Sidebar -->
      <aside class="w-48 flex-shrink-0 flex flex-col" style="background:#121212">
        <!-- Brand -->
        <div class="px-4 pt-5 pb-4">
          <div class="font-black text-white text-sm tracking-widest leading-none">TRAVELPLANER</div>
          <div class="text-xs mt-2 font-bold leading-tight" style="color:#003BFF">
            THE TRAVELPLANER<br>CARTOGRAPHER
          </div>
        </div>

        <!-- Nav -->
        <nav class="flex-1 mt-2">
          @for (item of navItems; track item.path) {
            <a [routerLink]="item.path" routerLinkActive="bg-[#003BFF]"
              class="flex items-center gap-3 px-4 py-3 text-white text-xs font-semibold tracking-wider hover:bg-[#003BFF] transition-colors">
              <i [class]="'pi ' + item.icon + ' text-sm'"></i>
              {{ item.label }}
            </a>
          }
        </nav>

        <!-- Bottom CTA -->
        <div class="p-4">
          @if (auth.isAuthenticated()) {
            <button (click)="auth.logout()"
              class="w-full py-3 font-black text-white text-sm tracking-widest rounded-sm"
              style="background:var(--color-secondary)">
              SIGN OUT
            </button>
          } @else {
            <a routerLink="/auth/register"
              class="block w-full py-3 font-black text-white text-sm tracking-widest text-center rounded-sm"
              style="background:var(--color-secondary)">
              BOOK NOW
            </a>
          }
        </div>
      </aside>

      <!-- Main: top bar + content -->
      <div class="flex-1 flex flex-col overflow-hidden bg-white">
        <!-- Top bar -->
        <header class="flex items-center gap-3 px-4 py-2 border-b border-gray-200 bg-white">
          <i class="pi pi-search text-gray-400"></i>
          <input type="text" placeholder="FIND A DESTINATION"
            class="flex-1 text-xs font-semibold tracking-wider border border-gray-300 px-3 py-1.5 focus:outline-none focus:border-blue-500"
            style="text-transform:uppercase">
          <button class="px-4 py-1.5 text-white text-xs font-bold tracking-widest"
            style="background:var(--color-primary)">SEARCH</button>
          <button class="text-gray-500 hover:text-gray-800 ml-2"><i class="pi pi-bell"></i></button>
          <button class="text-gray-500 hover:text-gray-800"><i class="pi pi-cog"></i></button>
        </header>

        <!-- Page content -->
        <div class="flex-1 overflow-hidden relative">
          <router-outlet />
        </div>
      </div>
    </div>

    <!-- ── MOBILE ── -->
    <div class="flex flex-col h-screen md:hidden">
      <!-- Mobile top bar -->
      <header class="flex items-center justify-between px-4 py-3 text-white"
        style="background:#121212">
        <span class="font-black tracking-widest text-sm">TRAVELPLANER</span>
        <div class="flex gap-3">
          @if (auth.isAuthenticated()) {
            <button (click)="auth.logout()" class="text-xs px-3 py-1 rounded"
              style="background:var(--color-secondary)">Out</button>
          } @else {
            <a routerLink="/auth/login" class="text-xs" style="color:var(--color-primary)">Sign in</a>
            <a routerLink="/auth/register" class="text-xs px-2 py-1 rounded text-white"
              style="background:var(--color-primary)">Register</a>
          }
          <button class="text-gray-400"><i class="pi pi-bell"></i></button>
        </div>
      </header>

      <main class="flex-1 overflow-hidden relative">
        <router-outlet />
      </main>

      <!-- Bottom nav -->
      <nav class="flex border-t border-gray-200 bg-white">
        @for (item of navItems; track item.path) {
          <a [routerLink]="item.path" routerLinkActive="text-[#003BFF]"
            class="flex-1 flex flex-col items-center py-2 text-gray-400 text-xs gap-0.5 font-semibold tracking-wider">
            <i [class]="'pi ' + item.icon + ' text-base'"></i>
            {{ item.label }}
          </a>
        }
      </nav>
    </div>
  `
})
export class ShellComponent {
  auth = inject(AuthStore);

  navItems = [
    { path: '/map',      label: 'DISCOVER', icon: 'pi-circle-fill' },
    { path: '/journeys', label: 'TRIPS',    icon: 'pi-th-large'    },
    { path: '/planner',  label: 'PLANNER',  icon: 'pi-calendar'    },
    { path: '/profile',  label: 'PROFILE',  icon: 'pi-user'        },
  ];
}
