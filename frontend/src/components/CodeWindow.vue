<template>
    <div class="code-window">
        <!-- добавить хедер с выбором языка и др -->
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
    height: 100%;
    min-width: 0;
    background: $clr-light-card;
    border-radius: $radius-1;
    overflow: hidden;
    display: flex;
    flex-direction: column;
}
.code-window-content {
    flex: 1 1 auto;
    padding: 16px;
    overflow-y: auto;
    min-height: 0;
}
</style>