import { createApp } from 'vue'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import './style.css'
import App from './App.vue'
import router from './modules/Router'
import ArticlesView from './components/ArticlesView.vue'

import * as ElementPlusIconsVue from '@element-plus/icons-vue'

const app = createApp(App)
for (const [key, component] of Object.entries(ElementPlusIconsVue)) 
  app.component(key, component)
app.component('articles-view', ArticlesView);
app.use(ElementPlus);
app.use(router);
app.mount('#app')
