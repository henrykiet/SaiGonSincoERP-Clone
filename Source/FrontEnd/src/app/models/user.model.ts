export interface User {
    userId: number;
    userName: string;
    fullName: string;
    email: string;
    role: string;
    createdAt: Date;
    isLocked: boolean;
}

export interface UserCreateDto {
    userName: string;
    fullName: string;
    email: string;
    password: string;
    ConfirmPassword: string;
}

export interface UserUpdateDto {
    fullName: string;
    email: string;
    isLocked: boolean;
}

export interface UserLoginDto {
    userName: string;
    password: string;
    unit: string;
    ipAddress?: string;
    userAgent?: string;
} 