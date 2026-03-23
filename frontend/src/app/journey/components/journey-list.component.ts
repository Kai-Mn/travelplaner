import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { JourneyStore } from '../store/journey.store';

@Component({
  selector: 'app-journey-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="p-4 max-w-2xl mx-auto">
      <h2 class="text-xl font-bold mb-4" style="color: var(--color-primary)">My Trips</h2>

      <!-- Create form -->
      <div class="bg-white rounded-lg shadow p-4 mb-6">
        <h3 class="font-semibold text-gray-700 mb-3">New Trip</h3>
        <form [formGroup]="createForm" (ngSubmit)="create()" class="space-y-3">
          <input formControlName="name" placeholder="Trip name" type="text"
            class="w-full border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none">
          <textarea formControlName="description" placeholder="Description (optional)" rows="2"
            class="w-full border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none resize-none"></textarea>
          <button type="submit" [disabled]="createForm.invalid"
            class="text-white px-4 py-2 rounded text-sm font-medium disabled:opacity-50"
            style="background-color: var(--color-primary)">Create Trip</button>
        </form>
      </div>

      <!-- Journey list -->
      @if (store.loading()) {
        <p class="text-gray-500 text-center py-4">Loading...</p>
      }

      <div class="space-y-3">
        @for (journey of store.journeys(); track journey.id) {
          <div class="bg-white rounded-lg shadow p-4 flex justify-between items-start">
            <div>
              <h4 class="font-semibold text-gray-900">{{ journey.name }}</h4>
              <p class="text-sm text-gray-500 mt-1">{{ journey.description }}</p>
              <p class="text-xs text-gray-400 mt-2">{{ journey.locationCount }} locations</p>
            </div>
            <button (click)="deleteJourney(journey.id)"
              class="text-gray-400 hover:text-red-500 transition-colors ml-4">
              <i class="pi pi-trash"></i>
            </button>
          </div>
        }

        @if (store.journeys().length === 0 && !store.loading()) {
          <p class="text-gray-400 text-center py-8">No trips yet. Create your first one!</p>
        }
      </div>
    </div>
  `
})
export class JourneyListComponent implements OnInit {
  store = inject(JourneyStore);

  createForm = inject(FormBuilder).nonNullable.group({
    name: ['', Validators.required],
    description: ['']
  });

  ngOnInit() { this.store.loadJourneys(); }

  create() {
    if (this.createForm.invalid) return;
    this.store.createJourney(this.createForm.getRawValue());
    this.createForm.reset();
  }

  deleteJourney(id: string) {
    if (confirm('Delete this trip?')) this.store.deleteJourney(id);
  }
}
