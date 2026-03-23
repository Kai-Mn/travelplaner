import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import Map from 'ol/Map';
import View from 'ol/View';
import TileLayer from 'ol/layer/Tile';
import OSM from 'ol/source/OSM';
import { fromLonLat } from 'ol/proj';

@Component({
  selector: 'app-map',
  standalone: true,
  imports: [],
  template: `<div #mapEl style="width:100%;height:100%;"></div>`,
  styles: [`:host { display:block; width:100%; height:100%; }`]
})
export class MapComponent implements OnInit, OnDestroy {
  @ViewChild('mapEl', { static: true }) mapEl!: ElementRef<HTMLDivElement>;

  private map!: Map;

  ngOnInit() {
    this.map = new Map({
      target: this.mapEl.nativeElement,
      layers: [new TileLayer({ source: new OSM() })],
      view: new View({
        center: fromLonLat([13.405, 52.52]),
        zoom: 12
      })
    });
  }

  ngOnDestroy() {
    this.map?.setTarget(undefined as any);
  }
}
