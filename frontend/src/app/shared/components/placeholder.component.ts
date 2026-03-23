import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-placeholder',
  standalone: true,
  template: `
    <div class="flex items-center justify-center h-full text-gray-400">
      <p class="text-xl">{{ title }} — coming soon</p>
    </div>
  `
})
export class PlaceholderComponent {
  title = inject(ActivatedRoute).snapshot.data['title'] ?? 'Page';
}
