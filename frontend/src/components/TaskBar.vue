<template>
    <div class="task-bar">
        <div class="tasks-section">
            <div class="section-header">code</div>
            <!-- подгружаем кол-во задач как отдельные task items-->
        </div>
        <div class="task-section-divider"></div>

        <div class="tasks-section">
            <div class="section-header">soft skills</div>
            <!-- подгружаем кол-во задач как отдельные task items-->
        </div>
        <div class="task-section-divider"></div>

        <div class="tasks-section">
            <div class="section-header">hard skills</div>
            <!-- подгружаем кол-во задач как отдельные task items-->
        </div>
        <div class="task-section-divider"></div>

        <div class="progress"></div>
        <div class="exit-btn"></div>
    </div>
</template>

<script setup lang="ts">
// 
</script>
<style lang="scss">
@use "../scss/components/_taskBar.scss";
</style>