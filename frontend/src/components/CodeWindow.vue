<template>
  <div class="code-window-placeholder">
    <div class="code-window-header">
      <h3>Code Window</h3>
    </div>
    <div class="code-window-content">
      <p>Здесь будет отображаться код, примеры или логика работы приложения...</p>
      <pre>
        // Заглушка для проверки layout
        function example() {
          console.log("Hello from CodeWindow!");
        }
      </pre>
    </div>
  </div>
</template>

<script setup lang="ts">
//
</script>

<style lang="scss">
@use '../scss/_variables.scss' as *;

.code-window {
    flex: 2;
    box-sizing: border-box;
    background: $clr-light-card;
    border-radius: $radius-1;
}

.code-window-header {
    margin-bottom: 16px;
    border-bottom: 1px solid #d9c8b8;
}

.code-window-content {
    flex: 1 1 auto;
    padding-top: 16px;
    overflow-y: auto;
}
</style>