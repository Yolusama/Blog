import request from '../modules/Request'

export interface User {
    id: number;
    nickname: string;
    email: string;
    token: string,
    gender: number;
}
export type UserLoginDTO = {
    email: string;
    password: string;
    verificationCode?: string;
}
export type UserRegisterDTO = {
    email: string;
    verificationCode: string;
}

export async function Login(user: UserLoginDTO, useVerificationCode: boolean) {
    return await request.post(`api/User/Login?UseVerificationCode=${useVerificationCode}`, user);
}

export async function GetVerificationCode(email: string, length: number) {
    const query = { email, codeLength: length };
    return await request.get('api/User/GetVerificationCode', { params: query });
}

export async function Register(user: UserRegisterDTO) {
    return await request.put('api/User/Register', user);
}

export async function UserExists(email: string) {
    return await request.get('api/User/UserExists', { params: { email } });
}

export async function VerifyToken(token: string,userId:number) {
    return await request.post('api/User/VerifyToken', { token, userId });
}