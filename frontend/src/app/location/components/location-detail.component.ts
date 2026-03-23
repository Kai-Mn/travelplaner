import { Component, Input, Output, EventEmitter, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { LocationDto } from '../../core/services/api.service';
import { LocationStore } from '../store/location.store';

@Component({
  selector: 'app-location-detail',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div>
      <!-- Header -->
      <div class="p-4 border-b flex justify-between items-start">
        <div>
          <p class="text-xs font-bold uppercase tracking-wide" style="color: var(--color-primary)">SELECTED NODE</p>
          <h3 class="font-bold text-gray-900 mt-1">{{ location.name }}</h3>
          <p class="text-xs text-gray-500">{{ location.coordinates.latitude.toFixed(5) }}°N, {{ location.coordinates.longitude.toFixed(5) }}°E</p>
        </div>
        <button (click)="close.emit()" class="text-gray-400 hover:text-gray-600 ml-2">
          <i class="pi pi-times"></i>
        </button>
      </div>

      <div class="p-4 space-y-3 max-h-96 overflow-y-auto">
        <!-- Description -->
        @if (location.description) {
          <p class="text-sm text-gray-700">{{ location.description }}</p>
        }

        <!-- Images -->
        @if (location.images.length > 0) {
          <div class="flex gap-2 flex-wrap">
            @for (img of location.images; track img.id) {
              <div class="relative">
                <img [src]="img.url" class="w-20 h-20 object-cover rounded" [alt]="location.name">
                <button (click)="deleteImage(img.id)"
                  class="absolute -top-1 -right-1 bg-red-500 text-white rounded-full w-5 h-5 flex items-center justify-center text-xs">
                  <i class="pi pi-times"></i>
                </button>
              </div>
            }
          </div>
        }

        <!-- Image upload -->
        <label class="flex items-center gap-2 text-sm cursor-pointer"
          style="color: var(--color-primary)">
          <i class="pi pi-upload"></i> Add image
          <input type="file" accept="image/*" class="hidden" (change)="onImageUpload($event)">
        </label>

        <!-- Tags -->
        <div>
          <div class="flex flex-wrap gap-1 mb-2">
            @for (tag of location.tags; track tag.id) {
              <span class="inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs text-white"
                style="background-color: var(--color-tertiary); color: var(--color-neutral)">
                {{ tag.name }}
                <button (click)="removeTag(tag.id)" class="hover:opacity-70">×</button>
              </span>
            }
          </div>
          <div class="flex gap-2">
            <input [formControl]="tagInput" placeholder="Add tag..." type="text"
              class="flex-1 border border-gray-300 rounded px-2 py-1 text-xs focus:outline-none"
              (keyup.enter)="addTag()">
            <button (click)="addTag()" class="px-2 py-1 text-white text-xs rounded"
              style="background-color: var(--color-tertiary); color: var(--color-neutral)">Add</button>
          </div>
        </div>

        <!-- Actions -->
        <div class="flex gap-2 pt-2 border-t">
          <button (click)="confirmDelete()"
            class="flex-1 text-white py-2 rounded text-sm font-medium"
            style="background-color: var(--color-secondary)">
            <i class="pi pi-trash mr-1"></i>Delete
          </button>
        </div>
      </div>
    </div>
  `
})
export class LocationDetailComponent {
  @Input({ required: true }) location!: LocationDto;
  @Output() close = new EventEmitter<void>();

  locationStore = inject(LocationStore);
  tagInput = inject(FormBuilder).control('');

  addTag() {
    const name = this.tagInput.value?.trim();
    if (name) {
      this.locationStore.addTag({ locationId: this.location.id, name });
      this.tagInput.setValue('');
    }
  }

  removeTag(tagId: string) {
    this.locationStore.removeTag({ locationId: this.location.id, tagId });
  }

  onImageUpload(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (file) this.locationStore.uploadImage({ locationId: this.location.id, file });
  }

  deleteImage(imageId: string) {
    // handled via separate store method if needed — for now reload
  }

  confirmDelete() {
    if (confirm(`Delete "${this.location.name}" and all its data?`)) {
      this.locationStore.deleteLocation(this.location.id);
      this.close.emit();
    }
  }
}
