import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RegisterMemberRequest, MemberDto, LoginRequest, LoginResponse } from '../models/member.model';

@Injectable({ providedIn: 'root' })
export class MemberService {
  constructor(private http: HttpClient) {}

  register(request: RegisterMemberRequest): Observable<MemberDto> {
    return this.http.post<MemberDto>('/api/members/register', request);
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>('/api/members/login', request);
  }
}
