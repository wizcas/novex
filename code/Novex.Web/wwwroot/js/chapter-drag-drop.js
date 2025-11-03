// 章节拖放功能
window.chapterDragDrop = {
    draggedElement: null,
    draggedChapterId: null,
    dotNetHelper: null,
    isInitialized: false,

    initializeDragDrop: function (dotNetHelper) {
        const chapterList = document.getElementById('chapter-list');
        if (!chapterList) {
            console.warn('Chapter list not found');
            return;
        }

        // 如果已经初始化过，直接返回
        if (this.isInitialized) {
            return;
        }

        this.dotNetHelper = dotNetHelper;
        this.isInitialized = true;

        // 使用事件委托处理拖放事件
        chapterList.addEventListener('dragstart', (e) => this.handleDragStart(e));
        chapterList.addEventListener('dragend', (e) => this.handleDragEnd(e));
        chapterList.addEventListener('dragover', (e) => this.handleDragOver(e));
        chapterList.addEventListener('dragleave', (e) => this.handleDragLeave(e));
        chapterList.addEventListener('drop', (e) => this.handleDrop(e));

        // 为所有章节项设置 draggable 属性
        const chapterItems = chapterList.querySelectorAll('.chapter-item');
        chapterItems.forEach(item => {
            item.setAttribute('draggable', 'true');
        });
    },

    handleDragStart: function (e) {
        const chapterItem = e.target.closest('.chapter-item');
        if (chapterItem) {
            this.draggedElement = chapterItem;
            this.draggedChapterId = parseInt(chapterItem.getAttribute('data-chapter-id'));
            chapterItem.classList.add('dragging');
            e.dataTransfer.effectAllowed = 'move';
            e.dataTransfer.setData('text/html', chapterItem.innerHTML);
        }
    },

    handleDragEnd: function (e) {
        const chapterItem = e.target.closest('.chapter-item');
        if (chapterItem) {
            chapterItem.classList.remove('dragging');
        }
        this.draggedElement = null;
        this.draggedChapterId = null;

        // 移除所有拖放指示器
        const allItems = document.querySelectorAll('.chapter-item');
        allItems.forEach(item => {
            item.classList.remove('drag-over-top', 'drag-over-bottom');
        });
    },

    handleDragOver: function (e) {
        e.preventDefault();
        e.dataTransfer.dropEffect = 'move';

        const chapterItem = e.target.closest('.chapter-item');
        if (chapterItem && this.draggedElement && this.draggedElement !== chapterItem) {
            const rect = chapterItem.getBoundingClientRect();
            const midpoint = rect.top + rect.height / 2;

            // 移除之前的指示器
            chapterItem.classList.remove('drag-over-top', 'drag-over-bottom');

            // 根据鼠标位置添加指示器
            if (e.clientY < midpoint) {
                chapterItem.classList.add('drag-over-top');
            } else {
                chapterItem.classList.add('drag-over-bottom');
            }
        }
    },

    handleDragLeave: function (e) {
        const chapterItem = e.target.closest('.chapter-item');
        if (chapterItem) {
            chapterItem.classList.remove('drag-over-top', 'drag-over-bottom');
        }
    },

    handleDrop: function (e) {
        e.preventDefault();
        e.stopPropagation();

        const chapterItem = e.target.closest('.chapter-item');
        if (chapterItem && this.draggedElement && this.draggedElement !== chapterItem) {
            const targetChapterId = parseInt(chapterItem.getAttribute('data-chapter-id'));
            const rect = chapterItem.getBoundingClientRect();
            const midpoint = rect.top + rect.height / 2;
            const insertBefore = e.clientY < midpoint;

            // 调用 .NET 方法处理重新排序（异步调用）
            if (this.dotNetHelper) {
                this.dotNetHelper.invokeMethodAsync('HandleChapterReorder',
                    this.draggedChapterId,
                    targetChapterId,
                    insertBefore)
                    .catch(error => {
                        console.error('Error reordering chapters:', error);
                    });
            }
        }

        // 清理拖放指示器
        const allItems = document.querySelectorAll('.chapter-item');
        allItems.forEach(item => {
            item.classList.remove('drag-over-top', 'drag-over-bottom');
        });
    },

    cleanup: function () {
        this.draggedElement = null;
        this.draggedChapterId = null;
        this.isInitialized = false;
    }
};

