import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'

const routes: Array<RouteRecordRaw> = [
  {
    path: '/',
    name: 'Assessment',
    component: () => import('@/page/AssessmentPage.vue'),
  },
  {
    path: '/admin',
    name: 'Admin',
    component: () => import('@/page/AdminPage.vue'),
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {

})

export default router
