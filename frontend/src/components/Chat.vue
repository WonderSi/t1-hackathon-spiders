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
.chat {
    flex: 1;
    display: flex;
    flex-direction: row;
    min-width: 61px;
    height: 100%;
    gap: $space-16px;
    justify-content: end;
}

.chat__content {
    flex: 1 1 auto;
    overflow-y: auto;
    padding: 16px 16px 0px 16px;
    min-width: 0;
    /* Messages будут рендериться тут */
}

.chat__message-panel {
    width: 100%;
    padding: $space-16px;
    background: $clr-light-card;
    box-sizing: border-box;
    box-shadow: $shadow;
    min-width: 0;
    
}
</style>