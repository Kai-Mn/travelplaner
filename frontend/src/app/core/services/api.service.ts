import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CoordinatesDto { latitude: number; longitude: number; }
export interface TagDto { id: string; name: string; }
export interface ImageDto { id: string; url: string; }
export interface LocationDto {
  id: string; name: string; coordinates: CoordinatesDto;
  description: string; createdAt: string;
  tags: TagDto[]; images: ImageDto[];
}
export interface JourneySummaryDto {
  id: string; name: string; description: string; createdAt: string; locationCount: number;
}
export interface JourneyDto {
  id: string; name: string; description: string; createdAt: string; locations: LocationDto[];
}
export interface AuthResponse { token: string; email: string; userId: string; }

@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);
  private base = environment.apiBase;

  // Auth
  register(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.base}/auth/register`, { email, password });
  }
  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.base}/auth/login`, { email, password });
  }

  // Locations
  getLocations(): Observable<LocationDto[]> {
    return this.http.get<LocationDto[]>(`${this.base}/locations`);
  }
  getLocation(id: string): Observable<LocationDto> {
    return this.http.get<LocationDto>(`${this.base}/locations/${id}`);
  }
  createLocation(body: { name: string; coordinates: CoordinatesDto; description: string }): Observable<LocationDto> {
    return this.http.post<LocationDto>(`${this.base}/locations`, body);
  }
  updateLocation(id: string, body: { name: string; coordinates: CoordinatesDto; description: string }): Observable<LocationDto> {
    return this.http.put<LocationDto>(`${this.base}/locations/${id}`, body);
  }
  deleteLocation(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/locations/${id}`);
  }

  // Tags
  addTag(locationId: string, name: string): Observable<TagDto> {
    return this.http.post<TagDto>(`${this.base}/locations/${locationId}/tags`, { name });
  }
  removeTag(locationId: string, tagId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/locations/${locationId}/tags/${tagId}`);
  }

  // Images
  uploadImage(locationId: string, file: File): Observable<ImageDto> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<ImageDto>(`${this.base}/locations/${locationId}/images`, form);
  }
  deleteImage(locationId: string, imageId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/locations/${locationId}/images/${imageId}`);
  }

  // Journeys
  getJourneys(): Observable<JourneySummaryDto[]> {
    return this.http.get<JourneySummaryDto[]>(`${this.base}/journeys`);
  }
  getJourney(id: string): Observable<JourneyDto> {
    return this.http.get<JourneyDto>(`${this.base}/journeys/${id}`);
  }
  createJourney(body: { name: string; description: string }): Observable<JourneySummaryDto> {
    return this.http.post<JourneySummaryDto>(`${this.base}/journeys`, body);
  }
  updateJourney(id: string, body: { name: string; description: string }): Observable<JourneySummaryDto> {
    return this.http.put<JourneySummaryDto>(`${this.base}/journeys/${id}`, body);
  }
  deleteJourney(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/journeys/${id}`);
  }
  addLocationToJourney(journeyId: string, locationId: string): Observable<void> {
    return this.http.post<void>(`${this.base}/journeys/${journeyId}/locations/${locationId}`, {});
  }
  removeLocationFromJourney(journeyId: string, locationId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/journeys/${journeyId}/locations/${locationId}`);
  }
}
