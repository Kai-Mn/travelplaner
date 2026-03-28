import { Component, ElementRef, OnDestroy, OnInit, ViewChild, inject, effect, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import Map from 'ol/Map';
import View from 'ol/View';
import TileLayer from 'ol/layer/Tile';
import VectorLayer from 'ol/layer/Vector';
import VectorSource from 'ol/source/Vector';
import OSM from 'ol/source/OSM';
import Feature from 'ol/Feature';
import Point from 'ol/geom/Point';
import { fromLonLat, toLonLat } from 'ol/proj';
import { Style, Fill, Stroke, RegularShape, Text as OlText } from 'ol/style';
import { LocationStore } from '../../location/store/location.store';
import { AuthStore } from '../../auth/store/auth.store';

@Component({
  selector: 'app-map',
  standalone: true,
  imports: [CommonModule, FormsModule],
  styles: [`:host { display: block; position: absolute; inset: 0; }`],
  template: `
    <div class="relative w-full h-full">

      <!-- OL map canvas -->
      <div #mapEl class="w-full h-full"></div>

      <!-- MAP LEGEND (desktop) -->
      <div class="hidden md:block absolute top-3 left-3 bg-white shadow p-3 z-10 text-[10px]">
        <div class="font-black tracking-widest mb-2" style="color:#121212">MAP LEGEND</div>
        <div class="flex items-center gap-2">
          <span class="w-3 h-3 flex-shrink-0" style="background:var(--color-primary)"></span>
          <span class="font-semibold tracking-wider" style="color:#121212">LOCATION</span>
        </div>
      </div>

      <!-- MARK LOCATION button -->
      @if (auth.isAuthenticated()) {
        <button (click)="toggleMarkMode()"
          class="absolute bottom-6 left-3 flex items-center gap-2 px-5 py-3 text-white text-xs font-black tracking-widest z-10 shadow"
          [style]="markMode() ? 'background:#121212' : 'background:var(--color-secondary)'">
          <i [class]="markMode() ? 'pi pi-times' : 'pi pi-map-marker'" class="text-sm"></i>
          {{ markMode() ? 'CANCEL' : 'MARK LOCATION' }}
        </button>
      }

      <!-- Mark mode hint banner -->
      @if (markMode() && !pendingCoords()) {
        <div class="absolute top-3 inset-x-0 flex justify-center z-10 pointer-events-none">
          <div class="bg-black/70 text-white text-[10px] font-black tracking-widest px-4 py-2">
            CLICK ON THE MAP TO PLACE A MARKER
          </div>
        </div>
      }

      <!-- Quick-create card (after map click in mark mode) -->
      @if (pendingCoords()) {
        <div class="absolute bottom-20 left-3 bg-white shadow-lg p-4 w-64 z-10">
          <div class="font-black tracking-widest text-[10px] mb-1" style="color:#121212">NEW LOCATION</div>
          <div class="text-[10px] text-gray-400 font-mono mb-3">
            {{ pendingCoords()!.lat | number:'1.4-4' }}°N &nbsp;
            {{ pendingCoords()!.lng | number:'1.4-4' }}°E
          </div>
          <input
            #nameInput
            [(ngModel)]="newLocationName"
            type="text"
            placeholder="LOCATION NAME"
            (keydown.enter)="saveLocation()"
            class="w-full border border-gray-300 px-3 py-2 text-xs font-semibold tracking-wider
                   focus:outline-none focus:border-blue-500 mb-3 uppercase">
          <div class="flex gap-2">
            <button (click)="saveLocation()"
              [disabled]="!newLocationName.trim()"
              class="flex-1 py-2 text-white text-[10px] font-black tracking-widest disabled:opacity-40"
              style="background:var(--color-primary)">
              SAVE
            </button>
            <button (click)="cancelMark()"
              class="px-3 py-2 text-[10px] font-black tracking-widest border border-gray-300 hover:bg-gray-50">
              ✕
            </button>
          </div>
        </div>
      }

      <!-- SELECTED NODE panel -->
      @if (locationStore.selected(); as loc) {
        <div class="absolute top-3 right-3 bg-white shadow-lg p-4 z-10 w-72 max-w-[calc(100vw-2rem)] md:max-w-xs">
          <div class="flex items-start justify-between mb-3">
            <div>
              <div class="font-black tracking-widest text-[10px]" style="color:#121212">SELECTED NODE</div>
              <div class="text-[10px] text-gray-400 font-mono mt-0.5">
                {{ loc.coordinates.latitude | number:'1.4-4' }}°N,
                {{ loc.coordinates.longitude | number:'1.4-4' }}°E
              </div>
            </div>
            <button (click)="locationStore.clearSelected()" class="text-gray-400 hover:text-gray-700 ml-3 flex-shrink-0">
              <i class="pi pi-times text-xs"></i>
            </button>
          </div>

          <div class="border-t border-gray-100 pt-3">
            <div class="text-xs font-black tracking-widest mb-2" style="color:#121212">
              {{ loc.name | uppercase }}
            </div>

            @if (loc.images.length) {
              <div class="flex gap-2 items-start mb-2">
                <img [src]="loc.images[0].url" class="w-10 h-10 object-cover flex-shrink-0">
                @if (loc.description) {
                  <p class="text-[10px] text-gray-500 leading-tight line-clamp-3">{{ loc.description }}</p>
                }
              </div>
            } @else if (loc.description) {
              <p class="text-[10px] text-gray-500 leading-relaxed mb-2">{{ loc.description }}</p>
            }

            <button class="w-full mt-1 py-2 border border-gray-300 text-[10px] font-black tracking-widest hover:bg-gray-50">
              VIEW DETAILS
            </button>
          </div>
        </div>
      }

    </div>
  `
})
export class MapComponent implements OnInit, OnDestroy {
  @ViewChild('mapEl', { static: true }) mapEl!: ElementRef<HTMLDivElement>;

  locationStore = inject(LocationStore);
  auth = inject(AuthStore);

  private map!: Map;
  private vectorSource = new VectorSource();

  markMode = signal(false);
  pendingCoords = signal<{ lat: number; lng: number } | null>(null);
  newLocationName = '';

  constructor() {
    // Reactively sync store → OL vector features
    effect(() => {
      const locations = this.locationStore.locations();
      const selectedId = this.locationStore.selected()?.id;
      this.vectorSource.clear();
      for (const loc of locations) {
        const feature = new Feature(
          new Point(fromLonLat([loc.coordinates.longitude, loc.coordinates.latitude]))
        );
        feature.setId(loc.id);
        feature.setStyle(this.markerStyle(loc.name, loc.id === selectedId));
        this.vectorSource.addFeature(feature);
      }
    });
  }

  ngOnInit() {
    this.map = new Map({
      target: this.mapEl.nativeElement,
      layers: [
        new TileLayer({ source: new OSM() }),
        new VectorLayer({ source: this.vectorSource }),
      ],
      view: new View({
        center: fromLonLat([13.405, 52.52]),
        zoom: 12,
      }),
    });

    this.map.on('click', (evt) => {
      if (this.markMode()) {
        const [lng, lat] = toLonLat(evt.coordinate);
        this.pendingCoords.set({ lat, lng });
        return;
      }
      const feature = this.map.forEachFeatureAtPixel(evt.pixel, f => f);
      if (feature) {
        this.locationStore.selectLocation(feature.getId() as string);
      } else {
        this.locationStore.clearSelected();
      }
    });

    this.map.on('pointermove', (evt) => {
      const el = this.mapEl.nativeElement;
      if (this.markMode()) {
        el.style.cursor = 'crosshair';
      } else {
        el.style.cursor = this.map.hasFeatureAtPixel(evt.pixel) ? 'pointer' : '';
      }
    });

    if (this.auth.isAuthenticated()) {
      this.locationStore.loadLocations();
    }
  }

  ngOnDestroy() {
    this.map?.setTarget(undefined as any);
  }

  toggleMarkMode() {
    this.markMode.update(m => !m);
    this.pendingCoords.set(null);
    this.newLocationName = '';
  }

  cancelMark() {
    this.markMode.set(false);
    this.pendingCoords.set(null);
    this.newLocationName = '';
  }

  saveLocation() {
    const coords = this.pendingCoords();
    const name = this.newLocationName.trim();
    if (!coords || !name) return;
    this.locationStore.createLocation({
      name,
      coordinates: { latitude: coords.lat, longitude: coords.lng },
      description: '',
    });
    this.cancelMark();
  }

  private markerStyle(label: string, selected = false): Style {
    return new Style({
      image: new RegularShape({
        points: 4,
        radius: 12,
        angle: Math.PI / 4,
        fill: new Fill({ color: selected ? '#121212' : '#003BFF' }),
        stroke: new Stroke({ color: 'white', width: 2 }),
      }),
      text: new OlText({
        text: (label || 'LOCATION').toUpperCase().substring(0, 16),
        offsetY: 20,
        font: 'bold 9px sans-serif',
        fill: new Fill({ color: '#121212' }),
        stroke: new Stroke({ color: 'white', width: 3 }),
        textAlign: 'center',
      }),
    });
  }
}
