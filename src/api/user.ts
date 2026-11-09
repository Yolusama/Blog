import request from '../modules/Request'

export interface User {
    userId: number;
    nickname: string;
    email: string;
    token: string,
    gender: number;
    avatar: string;
}

export interface UserProfile {
    userId: number;
    nickname: string;
    email: string;
    avatar:string,
    gender:number,
    joinTime:Date,
    creationCount:number,
    fansCount:number,
    followingCount:number
}

export type UserProfileDTO = {
    userId: number;
    nickname?: string;
    email?: string;
    avatar?: string;
    gender?: number;
    avatarFile?: File;
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

export async function VerifyToken(token: string, userId: number) {
    return await request.post('api/User/VerifyToken', { token, userId });
}

export function GetUserprofile(userId:number) {
    return request.get(`api/User/GetUserProfile/${userId}`);
}

export function ChangeAvatar(profile: UserProfileDTO) {
    const formData = new FormData();
    formData.append('UserId', String(profile.userId));
    if (profile.nickname !== undefined) formData.append('Nickname', profile.nickname);
    if (profile.email !== undefined) formData.append('Email', profile.email);
    if (profile.avatar !== undefined) formData.append('Avatar', profile.avatar);
    if (profile.gender !== undefined) formData.append('Gender', String(profile.gender));
    if (profile.avatarFile) formData.append('AvatarFile', profile.avatarFile);
    return request.post('api/User/ChangeAvatar', formData);
}