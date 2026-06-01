import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface UploadProductResponse {
  successCount: number;
  failCount: number;
  errors: string[];
}

@Injectable({ providedIn: 'root' })
export class ProductFileService {

  constructor(private http: HttpClient) {}

  downloadTemplate(): Observable<Blob> {
    return this.http.get('/api/file/products/template', { responseType: 'blob' });
  }

  uploadProducts(file: File): Observable<UploadProductResponse> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<UploadProductResponse>('/api/file/products/upload', form);
  }
}
