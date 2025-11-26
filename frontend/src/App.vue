<template>
    <router-view />
    <TaskGenerationModal 
      :is-open="showInitModal" 
      @close="handleModalClose"
      @success="handleTaskGenerated"
    />
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useAssessmentStore } from '@/stores/assessment';
import TaskGenerationModal from '@/components/interview/TaskGenerationModal.vue';

const assessmentStore = useAssessmentStore();
const showInitModal = ref<boolean>(false);

onMounted(() => {
  const sessionRestored = assessmentStore.restoreSession();
  
  if (!sessionRestored) {
    showInitModal.value = true;
  }
});

const handleModalClose = (): void => {
  showInitModal.value = false;
};

const handleTaskGenerated = (): void => {
  showInitModal.value = false;
};
</script>

<style scoped lang="scss">
</style>
