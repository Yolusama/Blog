import { ElLoading } from 'element-plus';

export interface LoadingOptions {
    message: string;
    duration?: number;
    background?: string;
    lock?: boolean;
}

export function encryptPassword(plainPassword: string): Promise<string> {
    return new Promise(async (resolve, reject) => {
        if (!plainPassword) {
            resolve('');
            return;
        }

        try {
            const encoded = new TextEncoder().encode(plainPassword);
            const digest = await crypto.subtle.digest('SHA-256', encoded);
            const encryptedPassword = Array.from(new Uint8Array(digest))
                .map(value => value.toString(16).padStart(2, '0'))
                .join('');
            resolve(encryptedPassword);
        } catch (error) {
            reject(error);
        }
    });
}

export function loading(options: LoadingOptions) {
    const duration = options.duration ?? 2000; // Default duration to 2000ms if not provided
    const lock = options.lock ?? true; // Default lock to true if not provided
    const background = options.background ?? 'rgba(0, 0, 0, 0.7)'; // Default background if not provided
    const loading = ElLoading.service({
        lock: lock,
        text: options.message,
        background: background,
    })
    if (duration > 0) {
       const timeout = setTimeout(() => {
            loading.close()
            clearTimeout(timeout);
        }, duration)
    }
    return loading;
}

export function frameAnimate(func:Function){
    const execution = ()=>{
        requestAnimationFrame(execution);
        func();
    }
    execution();
}