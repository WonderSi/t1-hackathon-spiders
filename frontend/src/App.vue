<template>
  <Header />
  <div class="main-layout">
    <Chat />
    <CodeWindow />
  </div>
</template>

<script setup lang="ts">
import Header from './components/Header.vue';
import Chat from './components/Chat.vue';
import CodeWindow from './components/CodeWindow.vue';
</script>

<style lang="scss">
@use "./scss/_base.scss";
</style>