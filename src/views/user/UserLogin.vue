<template>
    <div id="user-login">
        <el-form label-width="120px" label-position="right" label-suffix=":">
            <h4 class="title">个人博客登录</h4>
            <div class="intro-img">
                <el-image src="https://account.cnblogs.com/assets/img/sign-in-avatar.svg" fit="cover"
                    style="width: 80px; height: 80px;">
                </el-image>
            </div>
            <el-form-item label="电子邮箱" :rules="rules">
                <el-input v-model="form.email" maxlength="50"></el-input>
            </el-form-item>
            <el-form-item label="密码" v-if="!useVerificationCode">
                <el-input type="password" v-model="plainPassword" show-password minlength="6" maxlength="20"
                    @input="encrpt"></el-input>

            </el-form-item>
            <el-form-item label="验证码" v-if="useVerificationCode">
                <el-input v-model="form.verificationCode" maxlength="6">
                    <template #append>
                        <el-button @click="getVerificationCode" :disabled="hasGotCode">{{ countDownText
                        }}</el-button>
                    </template>
                </el-input>
            </el-form-item>
            <el-form-item>
                <el-button type="primary" @click="login">登录/注册</el-button>
                <el-button link size="small" type="info" @click="useVerificationCode = !useVerificationCode">
                    {{ useVerificationCode ? '使用密码' : '使用验证码' }}
                </el-button>
                <el-button size="small" link type="warning" @click="forgotPassword">忘记密码</el-button>
            </el-form-item>
        </el-form>
    </div>
</template>
<script setup lang="ts">
import { GetVerificationCode, UserExists, Login, Register, VerifyToken, type UserLoginDTO, type User } from '../../api/user';
import { reactive, ref, onBeforeMount } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage, ElNotification } from 'element-plus';
import { encryptPassword, loading} from '../../modules/Utils';
import stateStroge from '../../modules/StateStorage';

type UserLoginForm = {
    email: string;
    password: string;
    verificationCode: string;
};

const rules = ref({
    email: [
        { required: true, message: '请输入电子邮箱', trigger: 'blur' },
        { type: 'email', message: '请输入有效的电子邮箱', trigger: 'blur' }
    ],
    password: [
        { required: true, message: '请输入密码', trigger: 'blur' },
        { min: 6, max: 20, message: '密码长度应为6-20个字符', trigger: 'blur' }
    ],
    verificationCode: [
        { required: true, message: '请输入验证码', trigger: 'blur' },
        { len: 6, message: '验证码长度应为6个字符', trigger: 'blur' }
    ]
});

const form = reactive<UserLoginForm>({
    email: '',
    password: '',
    verificationCode: ''
});
const count = ref(0);
const countDownText = ref('获取验证码');
const hasGotCode = ref(false);
const useVerificationCode = ref(false);
const plainPassword = ref('');
const router = useRouter();


onBeforeMount(() => {
    const user = stateStroge.get<User>("user");
    if (!user) return;
    const loadingInstance = loading({ message: '正在登录...', duration: 0 });
    VerifyToken(user.token, user.id)
        .then(res => {
            if (!res.success) {
                ElMessage.error(res.message);

                return;
            }
            if (res.data)
                router.push('/index');
            else
                ElMessage.error('Token验证失败');
        }).catch(error => {
            ElMessage.error((error as Error).message);
        }).finally(() => {
            loadingInstance.close();
        });
});

async function encrpt() {
    if (form.password.length < 6 || form.password.length > 20)
        return;
    if (!plainPassword.value) {
        form.password = '';
        return;
    }

    form.password = await encryptPassword(plainPassword.value);
}

async function login() {
    try {
        if (useVerificationCode.value) {
            const res = await UserExists(form.email);
            if (!res.success) {
                ElMessage.error(res.message);
                return;
            }
            if (res.data)
                await loginFunc();
            else
                await register();
        }
        else
            await loginFunc();
    } catch (error: unknown) {
        ElMessage.error((error as Error).message);
    }
}

async function register() {
    const loadingInstance = loading({ message: '正在注册...', duration: 0 });
    try {
        const res = await Register({ email: form.email, verificationCode: form.verificationCode });
        if (res.success)
            await loginFunc();
        else
            ElMessage.error(res.message);
    } catch (error: unknown) {
        ElMessage.error((error as Error).message);
    }
    finally {
        loadingInstance.close();
    }
}

async function loginFunc() {
    const data: UserLoginDTO = { email: form.email, password: form.password };
    if (useVerificationCode.value)
        data.verificationCode = form.verificationCode;
    const loadingInstance = loading({ message: '正在登录...', duration: 0 });
    const loginRes = await Login(data, useVerificationCode.value);
    if (loginRes.success) {
        {
            stateStroge.set("User", loginRes.data);
            router.replace('/index');
        }
    } else
        ElMessage.error(loginRes.message);
    loadingInstance.close();
}

function forgotPassword() {
    console.log('Forgot password clicked');
}
function getVerificationCode() {
    if (hasGotCode.value) return;
    const loadingInstance = loading({ message: '正在获取验证码...', duration: 0 });
    GetVerificationCode(form.email, 6)
        .then(res => {
            if (res.success) {
                hasGotCode.value = true;
                count.value = 60;
                countDownText.value = '60s';
                const timer = setInterval(() => {
                    count.value--;
                    countDownText.value = `${count.value}s`;
                    if (count.value <= 0) {
                        clearInterval(timer);
                        hasGotCode.value = false;
                        countDownText.value = '获取验证码';
                    }
                }, 1000);
            }
            else
                ElMessage.error(res.message);
        })
        .catch(error => {
            ElNotification.error({
                title: 'Error',
                message: error.message
            });
        }).finally(() => {
            loadingInstance.close();
        });
}

</script>
<style scoped>
#user-login {
    min-height: 100%;
    display: flex;
    justify-content: center;
    align-items: center;
    padding: 48px 24px;
    box-sizing: border-box;
    background-color: aliceblue;
}

#user-login :deep(.el-form) {
    width: 46%;
    padding: 32px 28px;
    border-radius: 16px;
    background: var(--bg);
    box-shadow: var(--shadow);
    box-sizing: border-box;
}

#user-login :deep(.el-form-item:last-child .el-form-item__content) {
    justify-content: center;
}

#user-login .intro-img {
    margin-bottom: 24px;
}

@media (max-width: 768px) {
    #user-login {
        padding: 24px 16px;
    }

    #user-login :deep(.el-form) {
        width: 100%;
        padding: 24px 20px;
    }
}
</style>