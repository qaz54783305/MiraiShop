import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { MemberService } from '../services/member.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  form: FormGroup;
  successMessage = '';
  errorMessage = '';
  isSubmitting = false;

  constructor(private fb: FormBuilder, private memberService: MemberService, private router: Router) {
    this.form = this.fb.group({
      name: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]],
      secondPassword: ['', Validators.required],
      mailingAddress: ['', [Validators.required, Validators.minLength(5)]],
      residentialAddress: ['']
    }, { validators: this.passwordMatchValidator })
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.isSubmitting = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.memberService.register(this.form.value).subscribe({
      next: () => {
        setTimeout(() => {

          this.successMessage = '即將跳轉至登入...';
        }, 3000);
        //this.successMessage = '註冊成功！歡迎加入 MiraiShop。';
       // this.form.reset();
        //this.isSubmitting = false;
       //倒數三秒
        setTimeout(() => {

          this.router.navigate(['/login']);
        }, 3000);
      },
      error: (err) => {
        if (err.status === 409) {
          this.errorMessage = '此電子信箱已被註冊，請使用其他信箱或直接登入。';
        } else {
          this.errorMessage = '註冊失敗，請稍後再試。';
        }
        this.isSubmitting = false;
      }
    });
  }
  passwordMatchValidator(group: AbstractControl) {
    const original = group.get('password')?.value;
    const confirm = group.get('secondPassword')?.value;
    if (!confirm) return null; // secondPassword 沒輸入時不比對
    return original === confirm ? null : { passwordMismatch: true };
  }
  getError(field: string): string {
    const ctrl = this.form.get(field);
    if (!ctrl || !ctrl.touched) return '';

    if (ctrl.errors?.['required']) return '此欄位為必填';
    if (ctrl.errors?.['email']) return '請填寫有效的電子信箱格式';
    if (ctrl.errors?.['minlength']) return '長度至少需要 5 個字元';

    if (field === 'secondPassword' && this.form.errors?.['passwordMismatch']) {
      return '輸入的密碼必須與原密碼相同';
    }

    return '';
  }
}
