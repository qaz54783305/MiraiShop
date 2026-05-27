import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MemberService } from '../services/member.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  loginForm: FormGroup;
  apiError = '';
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private memberService: MemberService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  get email() { return this.loginForm.get('email')!; }
  get password() { return this.loginForm.get('password')!; }

  onSubmit(): void {
    this.loginForm.markAllAsTouched();
    if (this.loginForm.invalid) return;

    this.isLoading = true;
    this.apiError = '';

    this.memberService.login(this.loginForm.value).subscribe({
      next: (response) => {
        localStorage.setItem('miraishop_auth', JSON.stringify({
          token: response.token,
          expiry: response.expiry,
          memberId: response.memberId
        }));
        this.isLoading = false;
        this.router.navigate(['/']);
      },
      error: (err) => {
        this.apiError = err.error?.error ?? '登入失敗，請稍後再試';
        this.isLoading = false;
      }
    });
  }
}
