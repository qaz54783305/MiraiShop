import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RegisterMemberRequest, MemberDto } from '../models/member.model';

@Injectable({ providedIn: 'root' })
export class MemberService {
  constructor(private http: HttpClient) {}

  register(request: RegisterMemberRequest): Observable<MemberDto> {
    return this.http.post<MemberDto>('/api/members/register', request);
  }
}
