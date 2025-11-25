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
@use "../scss/components/_codeWindow.scss";
</style>