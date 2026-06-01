import { Component } from '@angular/core';
import { ProductFileService, UploadProductResponse } from '../services/product-file.service';

type UploadState = 'idle' | 'uploading' | 'success' | 'error';

@Component({
  selector: 'app-product-file',
  templateUrl: './product-file.component.html',
  styleUrls: ['./product-file.component.css']
})
export class ProductFileComponent {
  selectedFile: File | null = null;
  uploadState: UploadState = 'idle';
  uploadResult: UploadProductResponse | null = null;
  uploadErrorMsg = '';

  constructor(private productFileService: ProductFileService) {}

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;

    if (file && !file.name.endsWith('.xlsx')) {
      this.selectedFile = null;
      this.uploadState = 'error';
      this.uploadErrorMsg = '請上傳 .xlsx 格式的檔案。若使用 Mac Numbers，請選擇「檔案 > 輸出 > Excel (.xlsx)」後再上傳。';
      input.value = '';
      return;
    }

    this.selectedFile = file;
    this.uploadState = 'idle';
    this.uploadResult = null;
    this.uploadErrorMsg = '';
  }

  downloadTemplate(): void {
    this.productFileService.downloadTemplate().subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'product_template.xlsx';
        a.click();
        URL.revokeObjectURL(url);
      }
    });
  }

  upload(): void {
    if (!this.selectedFile) return;
    this.uploadState = 'uploading';
    this.uploadResult = null;

    this.productFileService.uploadProducts(this.selectedFile).subscribe({
      next: (result) => {
        this.uploadResult = result;
        this.uploadState = result.failCount > 0 && result.successCount === 0 ? 'error' : 'success';
        //即使上傳失敗也要打開按鈕
        //this.selectedFile = null;
      },
      error: (err) => {
        this.uploadErrorMsg = err.error?.error ?? '上傳失敗，請稍後再試';
        this.uploadState = 'error';
      }

    });
  }
}
