<template>
  <Header />
  <div class="main-layout">
    <Chat />
    <CodeWindow />
  </div>
</template>

<script setup lang="ts">
  import Header from '@/components/interview/Header.vue';
  import Chat from '@/components/interview/Chat.vue';
  import CodeWindow from '@/components/interview/CodeWindow.vue';
</script>

<style scoped lang="scss">
</style>
