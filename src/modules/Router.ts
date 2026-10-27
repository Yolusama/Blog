import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router';


const routes:RouteRecordRaw[] = [];

routes.push({
    path: '/',
    redirect: '/login'
});
routes.push({
    path: '/login',
    name: 'Login',  
    component: () => import('../views/user/UserLogin.vue')
});
routes.push({
    path: '/index',
    name: 'Index',  
    component: () => import('../views/index.vue')
});
routes.push({
    path: '/article/editor',
    name: 'ArticleEditor',
    component: () => import('../views/article/ArticleEditOrView.vue')
});

const router = createRouter({
    history: createWebHistory(),
    routes: routes
})

export default router;