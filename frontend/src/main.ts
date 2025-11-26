import { createApp } from 'vue'
import { createPinia } from 'pinia'
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'
import router from './router'
import App from './App.vue'
import './assets/styles/main.scss'

import { useTasks } from '@/composables/useTasks';
import { useScenarios } from '@/composables/useScenarios';
import { useAssessment } from '@/composables/useAssessment';
import { useAssessmentStore } from '@/stores/assessment';

const pinia = createPinia()
pinia.use(piniaPluginPersistedstate)

const app = createApp(App)

if (import.meta.env.DEV) {
  (window as any).test = {
    tasks: useTasks,
    scenarios: useScenarios,
    assessment: useAssessment,
    store: useAssessmentStore,
  };
  
  console.log('%cAPI Testing: window.test', 'color: #cc382eff; font-size: 16px; font-weight: bold');
}

app.use(pinia)
app.use(router)
app.mount('#app')
