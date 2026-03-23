import { inject } from '@angular/core';
import { signalStore, withState, withMethods, patchState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { tapResponse } from '@ngrx/operators';
import { pipe, switchMap, tap } from 'rxjs';
import { ApiService, JourneySummaryDto, JourneyDto } from '../../core/services/api.service';

interface JourneyState {
  journeys: JourneySummaryDto[];
  selected: JourneyDto | null;
  loading: boolean;
  error: string | null;
}

export const JourneyStore = signalStore(
  { providedIn: 'root' },
  withState<JourneyState>({ journeys: [], selected: null, loading: false, error: null }),
  withMethods((store) => {
    const api = inject(ApiService);
    return {
      loadJourneys: rxMethod<void>(
        pipe(
          tap(() => patchState(store, { loading: true })),
          switchMap(() => api.getJourneys().pipe(
            tapResponse({
              next: (journeys: JourneySummaryDto[]) => patchState(store, { journeys, loading: false }),
              error: () => patchState(store, { loading: false, error: 'Failed to load journeys' })
            })
          ))
        )
      ),

      loadJourney: rxMethod<string>(
        pipe(
          switchMap((id) => api.getJourney(id).pipe(
            tapResponse({
              next: (journey: JourneyDto) => patchState(store, { selected: journey }),
              error: () => patchState(store, { error: 'Failed to load journey' })
            })
          ))
        )
      ),

      createJourney: rxMethod<{ name: string; description: string }>(
        pipe(
          switchMap((body) => api.createJourney(body).pipe(
            tapResponse({
              next: (j: JourneySummaryDto) => patchState(store, { journeys: [...store.journeys(), j] }),
              error: () => patchState(store, { error: 'Failed to create journey' })
            })
          ))
        )
      ),

      deleteJourney: rxMethod<string>(
        pipe(
          switchMap((id) => api.deleteJourney(id).pipe(
            tapResponse({
              next: () => patchState(store, {
                journeys: store.journeys().filter(j => j.id !== id),
                selected: store.selected()?.id === id ? null : store.selected()
              }),
              error: () => patchState(store, { error: 'Failed to delete journey' })
            })
          ))
        )
      ),

      addLocationToJourney: rxMethod<{ journeyId: string; locationId: string }>(
        pipe(
          switchMap(({ journeyId, locationId }) => api.addLocationToJourney(journeyId, locationId).pipe(
            tapResponse({
              next: () => patchState(store, {
                journeys: store.journeys().map(j =>
                  j.id === journeyId ? { ...j, locationCount: j.locationCount + 1 } : j
                )
              }),
              error: () => patchState(store, { error: 'Failed to add location to journey' })
            })
          ))
        )
      ),

      removeLocationFromJourney: rxMethod<{ journeyId: string; locationId: string }>(
        pipe(
          switchMap(({ journeyId, locationId }) => api.removeLocationFromJourney(journeyId, locationId).pipe(
            tapResponse({
              next: () => {
                patchState(store, {
                  journeys: store.journeys().map(j =>
                    j.id === journeyId ? { ...j, locationCount: Math.max(0, j.locationCount - 1) } : j
                  ),
                  selected: store.selected()?.id === journeyId
                    ? { ...store.selected()!, locations: store.selected()!.locations.filter(l => l.id !== locationId) }
                    : store.selected()
                });
              },
              error: () => patchState(store, { error: 'Failed to remove location from journey' })
            })
          ))
        )
      )
    };
  })
);
