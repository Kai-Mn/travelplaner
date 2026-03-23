import { inject } from '@angular/core';
import { signalStore, withState, withMethods, patchState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { tapResponse } from '@ngrx/operators';
import { pipe, switchMap, tap } from 'rxjs';
import { ApiService, LocationDto, CoordinatesDto, TagDto, ImageDto } from '../../core/services/api.service';

interface LocationState {
  locations: LocationDto[];
  selected: LocationDto | null;
  loading: boolean;
  error: string | null;
}

export const LocationStore = signalStore(
  { providedIn: 'root' },
  withState<LocationState>({ locations: [], selected: null, loading: false, error: null }),
  withMethods((store) => {
    const api = inject(ApiService);
    return {
      loadLocations: rxMethod<void>(
        pipe(
          tap(() => patchState(store, { loading: true })),
          switchMap(() => api.getLocations().pipe(
            tapResponse({
              next: (locations: LocationDto[]) => patchState(store, { locations, loading: false }),
              error: () => patchState(store, { loading: false, error: 'Failed to load locations' })
            })
          ))
        )
      ),

      selectLocation(id: string) {
        const found = store.locations().find(l => l.id === id) ?? null;
        patchState(store, { selected: found });
      },

      clearSelected() { patchState(store, { selected: null }); },

      createLocation: rxMethod<{ name: string; coordinates: CoordinatesDto; description: string }>(
        pipe(
          switchMap((body) => api.createLocation(body).pipe(
            tapResponse({
              next: (loc: LocationDto) => patchState(store, { locations: [...store.locations(), loc], selected: loc }),
              error: () => patchState(store, { error: 'Failed to create location' })
            })
          ))
        )
      ),

      updateLocation: rxMethod<{ id: string; name: string; coordinates: CoordinatesDto; description: string }>(
        pipe(
          switchMap(({ id, ...body }) => api.updateLocation(id, body).pipe(
            tapResponse({
              next: (loc: LocationDto) => patchState(store, {
                locations: store.locations().map(l => l.id === id ? loc : l),
                selected: loc
              }),
              error: () => patchState(store, { error: 'Failed to update location' })
            })
          ))
        )
      ),

      deleteLocation: rxMethod<string>(
        pipe(
          switchMap((id) => api.deleteLocation(id).pipe(
            tapResponse({
              next: () => patchState(store, {
                locations: store.locations().filter(l => l.id !== id),
                selected: store.selected()?.id === id ? null : store.selected()
              }),
              error: () => patchState(store, { error: 'Failed to delete location' })
            })
          ))
        )
      ),

      addTag: rxMethod<{ locationId: string; name: string }>(
        pipe(
          switchMap(({ locationId, name }) => api.addTag(locationId, name).pipe(
            tapResponse({
              next: (tag: TagDto) => patchState(store, {
                locations: store.locations().map(l =>
                  l.id === locationId ? { ...l, tags: [...l.tags, tag] } : l
                ),
                selected: store.selected()?.id === locationId
                  ? { ...store.selected()!, tags: [...store.selected()!.tags, tag] }
                  : store.selected()
              }),
              error: () => patchState(store, { error: 'Failed to add tag' })
            })
          ))
        )
      ),

      removeTag: rxMethod<{ locationId: string; tagId: string }>(
        pipe(
          switchMap(({ locationId, tagId }) => api.removeTag(locationId, tagId).pipe(
            tapResponse({
              next: () => patchState(store, {
                locations: store.locations().map(l =>
                  l.id === locationId ? { ...l, tags: l.tags.filter(t => t.id !== tagId) } : l
                ),
                selected: store.selected()?.id === locationId
                  ? { ...store.selected()!, tags: store.selected()!.tags.filter(t => t.id !== tagId) }
                  : store.selected()
              }),
              error: () => patchState(store, { error: 'Failed to remove tag' })
            })
          ))
        )
      ),

      uploadImage: rxMethod<{ locationId: string; file: File }>(
        pipe(
          switchMap(({ locationId, file }) => api.uploadImage(locationId, file).pipe(
            tapResponse({
              next: (img: ImageDto) => patchState(store, {
                locations: store.locations().map(l =>
                  l.id === locationId ? { ...l, images: [...l.images, img] } : l
                ),
                selected: store.selected()?.id === locationId
                  ? { ...store.selected()!, images: [...store.selected()!.images, img] }
                  : store.selected()
              }),
              error: () => patchState(store, { error: 'Failed to upload image' })
            })
          ))
        )
      )
    };
  })
);
