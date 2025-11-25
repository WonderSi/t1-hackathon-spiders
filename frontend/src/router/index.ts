import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'

const routes: Array<RouteRecordRaw> = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/page/LoginPage.vue'),
  },
  {
    path: '/register',
    name: 'Register',
    component: () => import('@/page/RegisterPage.vue'),
  },
  {
    path: '/',
    name: 'Assessment',
    component: () => import('@/page/AssessmentPage.vue'),
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  const isAuthenticated = !!localStorage.getItem('token')

  if (isAuthenticated) {
    if (to.name !== 'Assessment') {
      return next({ name: 'Assessment' })
    }
  }

  next()
})

export default router
