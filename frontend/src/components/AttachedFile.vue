<template>
    <div class="attachment" @click="openModal">
        <svg width="53" height="63" viewBox="0 0 53 63" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path
                d="M10.1133 0.500366L27.0957 0.702515C29.5609 0.731893 31.9177 1.7192 33.6689 3.45447L49.1641 18.807C50.9644 20.591 51.9766 23.0214 51.9766 25.556V52.3529C51.9764 57.5995 47.7232 61.8529 42.4766 61.8529H10C4.75338 61.8529 0.500136 57.5995 0.5 52.3529V10.0004C0.5 4.70944 4.82273 0.437383 10.1133 0.500366Z"
                fill="white" stroke="#55463F" />
            <path
                d="M42.8647 24.1329L28.5335 9.80177C28.5056 9.77406 28.4702 9.75521 28.4316 9.74759C28.3931 9.73997 28.3531 9.74392 28.3168 9.75893C28.2805 9.77395 28.2494 9.79937 28.2275 9.83199C28.2056 9.86461 28.1938 9.90298 28.1937 9.94228V22.8783C28.1937 23.3012 28.3617 23.7067 28.6607 24.0058C28.9597 24.3048 29.3653 24.4728 29.7881 24.4728H42.7242C42.7634 24.4726 42.8018 24.4608 42.8344 24.4389C42.8671 24.417 42.8925 24.3859 42.9075 24.3496C42.9225 24.3133 42.9265 24.2733 42.9188 24.2348C42.9112 24.1962 42.8924 24.1608 42.8647 24.1329Z"
                fill="#55463F" />
            <path
                d="M15.4381 10.5282H23.0045V22.8788C23.0046 24.6777 23.7198 26.4027 24.9918 27.6747C26.2639 28.9467 27.9888 29.6619 29.7877 29.662H42.1383V46.7948C42.1383 47.9558 41.677 49.0695 40.8561 49.8905C40.0351 50.7115 38.9214 51.1727 37.7604 51.1727H15.4381C14.277 51.1727 13.1634 50.7115 12.3424 49.8905C11.5215 49.0695 11.0602 47.9558 11.0602 46.7948V14.9061L11.066 14.6893C11.1196 13.6072 11.5726 12.5802 12.3424 11.8104C13.1634 10.9894 14.277 10.5282 15.4381 10.5282ZM38.3766 22.4725H30.194V14.2899L38.3766 22.4725Z"
                stroke="#55463F" stroke-width="4" />
        </svg>
        <label>{{ label }}</label>
    </div>

    <Teleport to="body">
        <div v-if="isModalOpen" class="modal-overlay" @click.self="closeModal">
            <div class="modal-content">
                <button class="modal-close" @click="closeModal">&times;</button>

                <h3>Загрузить файл: {{ label }}</h3>

                <div class="upload-area" @click="triggerFileInput">
                    <input type="file" ref="fileInput" class="hidden-input" @change="handleFileChange"
                        accept=".md,.txt,.doc,.docx" />

                    <div v-if="!selectedFile" class="upload-placeholder">
                        <p>Нажмите для выбора файла</p>
                        <span>(.txt, .docx, .md)</span>
                    </div>

                    <div v-else class="file-info">
                        <span class="file-name">{{ selectedFile.name }}</span>
                        <span class="change-file-text">Изменить</span>
                    </div>
                </div>

                <div class="modal-actions">
                    <button class="btn-cancel" @click="closeModal">Отмена</button>
                    <button class="btn-upload" :disabled="!selectedFile" @click="uploadFile">
                        Загрузить
                    </button>
                </div>
            </div>
        </div>
    </Teleport>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';

interface Props {
    type: 'resume' | 'company' | 'job';
    label: string;
}
const props = defineProps<Props>();

const isModalOpen = ref<boolean>(false);
const fileInput = ref<HTMLInputElement | null>(null);
const selectedFile = ref<File | null>(null);

const openModal = (): void => {
    isModalOpen.value = true;
}

const closeModal = (): void => {
    isModalOpen.value = false;
    selectedFile.value = null;
}

const handleFileChange = (event: Event) => {
    const target = event.target as HTMLInputElement;
    if (target.files && target.files.length > 0) {
        selectedFile.value = target.files[0];
    }
};

const uploadFile = () => {
    if (!selectedFile.value) return;
    // Здесь будет логика отправки файла на бэкенд
    closeModal();
};

const triggerFileInput = () => {
    fileInput.value?.click();
};
</script>

<style lang="scss" scoped>
.attachment {
    width: 65px;
    justify-content: flex-start;
    display: flex;
    flex-direction: column;
    align-items: center;
    cursor: pointer;
    transition: transform 0.2s ease;

    &:hover {
        transform: translateY(-4px);

        svg {
            fill: $clr-light-main;
        }
    }

    svg {
        width: 100%;
        max-height: 63px;
        max-width: 53px;
        display: block;
        margin: 0 auto;
        border-radius: 10px;
        flex-shrink: 0;
    }

    label {
        cursor: pointer;
        font-family: $font-sans;
        font-size: $font-size-base;
        font-weight: 400;
        color: $clr-light-header-font;
        text-align: center;
        margin-top: 4px;
    }
}

// Modal
.modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(41, 34, 30, 0.85);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 1000;
}

.modal-content {
    background: $clr-light-card;
    padding: 2rem;
    border-radius: 12px;
    width: 400px;
    max-width: 90%;
    position: relative;
    box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);

    h3 {
        margin-top: 0;
        font-family: $font-sans;
        margin-bottom: 1.5rem;
        text-align: center;
        color: $clr-light-main;
    }
}

.modal-close {
    position: absolute;
    top: 10px;
    right: 15px;
    background: none;
    border: none;
    font-size: 24px;
    cursor: pointer;
    color: $clr-light-main;

    &:hover {
        color: $clr-light-accent;
    }
}

.upload-area {
    border: 2px dashed $clr-light-accent;
    border-radius: $radius-1;
    padding: 2em;
    text-align: center;
    font-family: $font-sans;
    font-size: $font-size-base;
    cursor: pointer;
    transition: border-color 0.2s;
    margin-bottom: 1.5rem;

    &:hover {
        border-color: $clr-light-main;
        background-color: rgba($clr-light-accent, 0.25);
    }
}

.hidden-input {
    display: none;
}

.upload-placeholder {
    p {
        color: $clr-light-main;
        margin: 0;
        font-weight: 500;
    }
    span {
        font-size: 12px;
        color: $clr-light-accent;
    }
}

.file-info {
    display: flex;
    flex-direction: column;
    align-items: center;

    .file-name {
        width: 100%;
        font-weight: bold;
        color: $clr-light-main;
        margin-bottom: 0.5rem;
        word-break: break-all;
    }

    .change-file-text {
        font-size: 12px;
        color: $clr-light-main;
        text-decoration: underline;
    }
}

.modal-actions {
    display: flex;
    justify-content: flex-end;
    gap: 1rem;

    button {
        padding: 8px 16px;
        border-radius: 8px;
        cursor: pointer;
        font-weight: 500;
        transition: all 0.2s;
        border: none;

        &.btn-cancel {
            background: transparent;
            border: 1.5px solid $clr-light-accent;
            color: $clr-light-accent;

            &:hover {
                border-color: $clr-light-main;
                color: $clr-light-main;
            }
        }

        &.btn-upload {
            background: $clr-light-main;
            color: $clr-light-card;

            &:hover:not(:disabled) {
                background: $clr-light-accent;
            }

            &:disabled {
                background: $clr-light-accent-hover;
                cursor: not-allowed;
            }
        }
    }
}
</style>