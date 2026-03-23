import { Component, Input, Output, EventEmitter, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { LocationStore } from '../store/location.store';

@Component({
  selector: 'app-location-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <div class="p-4">
      <div class="flex justify-between items-center mb-3">
        <h3 class="font-bold text-gray-800">New Location</h3>
        <button (click)="cancelled.emit()" class="text-gray-400 hover:text-gray-600">
          <i class="pi pi-times"></i>
        </button>
      </div>
      <p class="text-xs text-gray-500 mb-3">
        {{ coordinates.latitude.toFixed(5) }}, {{ coordinates.longitude.toFixed(5) }}
      </p>
      <form [formGroup]="form" (ngSubmit)="submit()" class="space-y-3">
        <input formControlName="name" placeholder="Name" type="text"
          class="w-full border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:ring-1"
          style="--tw-ring-color: var(--color-primary)">
        <textarea formControlName="description" placeholder="Description (optional)" rows="3"
          class="w-full border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:ring-1 resize-none"></textarea>
        <div class="flex gap-2">
          <button type="button" (click)="cancelled.emit()"
            class="flex-1 border border-gray-300 text-gray-700 py-2 rounded text-sm">Cancel</button>
          <button type="submit" [disabled]="form.invalid"
            class="flex-1 text-white py-2 rounded text-sm font-medium disabled:opacity-50"
            style="background-color: var(--color-primary)">Save</button>
        </div>
      </form>
    </div>
  `
})
export class LocationFormComponent {
  @Input({ required: true }) coordinates!: { latitude: number; longitude: number };
  @Output() saved = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  locationStore = inject(LocationStore);
  form = inject(FormBuilder).nonNullable.group({
    name: ['', Validators.required],
    description: ['']
  });

  submit() {
    if (this.form.invalid) return;
    const { name, description } = this.form.getRawValue();
    this.locationStore.createLocation({
      name,
      description,
      coordinates: this.coordinates
    });
    this.saved.emit();
  }
}
