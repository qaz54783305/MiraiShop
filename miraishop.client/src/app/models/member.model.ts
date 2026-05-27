export interface RegisterMemberRequest {
  name: string;
  email: string;
  password: string;
  mailingAddress: string;
  residentialAddress: string;
}

export interface MemberDto {
  id: string;
  name: string;
  email: string;
  mailingAddress: string;
  residentialAddress: string;
  createdAt: string;
}
