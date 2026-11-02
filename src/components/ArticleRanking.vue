<template>
    <section class="ranking-card" aria-label="文章排行">
        <header class="ranking-header">
            <h2 class="ranking-title">文章排行</h2>
            <div class="ranking-tabs" role="tablist" aria-label="排行周期">
                <button v-for="tab in tabs" :key="tab.key" class="ranking-tab"
                    :class="{ 'is-active': activePeriod === tab.key }" type="button" role="tab"
                    :aria-selected="activePeriod === tab.key" @click="activePeriod = tab.key">
                    {{ tab.label }}
                </button>
            </div>
        </header>

        <ol class="ranking-list">
            <li v-for="(article, index) in rankings[activePeriod]" :key="article" class="ranking-item">
                <span class="ranking-number" :class="{ 'is-top': index < 3 }">{{ index + 1 }}</span>
                <el-tooltip :content="article" placement="top" :show-after="300">
                    <a class="ranking-article" href="#" @click.prevent>{{ article }}</a>
                </el-tooltip>
            </li>
        </ol>
    </section>
</template>

<script setup lang="ts">
import { ref } from 'vue';

type RankingPeriod = 'day' | 'week' | 'halfMonth';

const tabs: { key: RankingPeriod; label: string }[] = [
    { key: 'day', label: '24小时' },
    { key: 'week', label: '7天' },
    { key: 'halfMonth', label: '15天' },
];

const activePeriod = ref<RankingPeriod>('day');

const rankings: Record<RankingPeriod, string[]> = {
    day: [
        '让 Agent 可靠运行：幂等、持久化与失败恢复',
        '纯 C# 高精度数学库：放大 10^290 倍也不糊',
        '从零搭建 Vue 3 + TypeScript 项目',
        'KNX 照明系统原理与开关控制实践',
        '用 PaddleOCR 打通 .NET OCR 全流程',
        'EF Core 生成更合理的 UPDATE 语句',
    ],
    week: [
        '.NET 发布最小能多小？从 86MB 到 657 字节',
        'SimdCodecs 开源：纯 .NET 图像编解码实践',
        'C# 与 Skia：Avalonia 绘制耗时分析',
        '汉字典籍关系库：从字形到构件',
        '用 Agent 自动化日常开发任务',
        '部署 Sharp 驱动 PaddleOCR 全流程',
    ],
    halfMonth: [
        '从入门到实战：构建高性能 .NET 服务',
        '前端工程中的组件拆分与状态管理',
        'Qwen-Image 人像姿势迁移原理解析',
        '使用 ONNX Runtime 部署轻量级模型',
        'C# 异步编程常见问题与解决方案',
        'Vue 3 项目中的可访问性实践',
    ],
};
</script>

<style scoped>
.ranking-card {
    overflow: hidden;
    border: 1px solid #f0f0f0;
    border-radius: 8px;
    background: #fff;
}

.ranking-header {
    padding: 18px 18px 0;
}

.ranking-title {
    margin: 0 0 14px;
    color: #222226;
    font-size: 17px;
    font-weight: 600;
}

.ranking-tabs {
    display: flex;
    gap: 20px;
    border-bottom: 1px solid #f2f3f5;
}

.ranking-tab {
    position: relative;
    padding: 0 0 12px;
    border: 0;
    background: transparent;
    color: #777b83;
    font: inherit;
    font-size: 14px;
    cursor: pointer;
}

.ranking-tab:hover,
.ranking-tab.is-active {
    color: #fc5531;
}

.ranking-tab.is-active::after {
    position: absolute;
    right: 0;
    bottom: -1px;
    left: 0;
    height: 2px;
    background: #fc5531;
    content: '';
}

.ranking-list {
    display: grid;
    grid-template-rows: repeat(10, minmax(62px, auto));
    margin: 0;
    padding: 8px 18px 14px;
    list-style: none;
}

.ranking-item {
    display: grid;
    grid-template-columns: 20px minmax(0, 1fr);
    gap: 10px;
    align-items: start;
    padding: 9px 0;
}

.ranking-number {
    color: #9c9fa6;
    font-size: 14px;
    line-height: 1.55;
    text-align: center;
}

.ranking-number.is-top {
    color: #fc5531;
    font-weight: 600;
}

.ranking-article {
    display: -webkit-box;
    overflow: hidden;
    color: #33363d;
    font-size: 14px;
    line-height: 1.55;
    text-decoration: none;
    -webkit-box-orient: vertical;
    -webkit-line-clamp: 2;
}

.ranking-article:hover {
    color: #1677ff;
    text-decoration: underline;
    text-decoration-color: #1677ff;
    text-underline-offset: 3px;
    cursor:pointer;
}

@media (max-width: 640px) {
    .ranking-header {
        padding-right: 14px;
        padding-left: 14px;
    }

    .ranking-list {
        padding-right: 14px;
        padding-left: 14px;
    }
}
</style>
