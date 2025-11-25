<template>
  <div class="chat">
    <div class="chat__content">
      <!-- containing AI responses -->
    </div>
    <MessagePanel />
  </div>
</template>

<script setup lang="ts">
import MessagePanel from './MessagePanel.vue'
</script>

<style lang="scss" scoped>
@use "../scss/components/_chat.scss";
</style>