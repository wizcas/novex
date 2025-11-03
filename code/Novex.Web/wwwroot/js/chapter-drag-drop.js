// 章节拖放功能
window.chapterDragDrop = {
    draggedElement: null,
    draggedChapterId: null,

    initializeDragDrop: function (dotNetHelper) {
        const chapterList = document.getElementById('chapter-list');
        if (!chapterList) {
            console.warn('Chapter list not found');
            return;
        }

        // 为所有章节项添加拖放事件
        const chapterItems = chapterList.querySelectorAll('.chapter-item');
        chapterItems.forEach(item => {
            this.setupDragEvents(item, dotNetHelper);
        });
    },

    setupDragEvents: function (element, dotNetHelper) {
        element.setAttribute('draggable', 'true');

        element.addEventListener('dragstart', (e) => {
            this.draggedElement = element;
            this.draggedChapterId = parseInt(element.getAttribute('data-chapter-id'));
            element.classList.add('dragging');
            e.dataTransfer.effectAllowed = 'move';
            e.dataTransfer.setData('text/html', element.innerHTML);
        });

        element.addEventListener('dragend', (e) => {
            element.classList.remove('dragging');
            this.draggedElement = null;
            this.draggedChapterId = null;
            
            // 移除所有拖放指示器
            const allItems = document.querySelectorAll('.chapter-item');
            allItems.forEach(item => {
                item.classList.remove('drag-over-top', 'drag-over-bottom');
            });
        });

        element.addEventListener('dragover', (e) => {
            e.preventDefault();
            e.dataTransfer.dropEffect = 'move';

            if (this.draggedElement && this.draggedElement !== element) {
                const rect = element.getBoundingClientRect();
                const midpoint = rect.top + rect.height / 2;
                
                // 移除之前的指示器
                element.classList.remove('drag-over-top', 'drag-over-bottom');
                
                // 根据鼠标位置添加指示器
                if (e.clientY < midpoint) {
                    element.classList.add('drag-over-top');
                } else {
                    element.classList.add('drag-over-bottom');
                }
            }
        });

        element.addEventListener('dragleave', (e) => {
            element.classList.remove('drag-over-top', 'drag-over-bottom');
        });

        element.addEventListener('drop', (e) => {
            e.preventDefault();
            e.stopPropagation();

            if (this.draggedElement && this.draggedElement !== element) {
                const targetChapterId = parseInt(element.getAttribute('data-chapter-id'));
                const rect = element.getBoundingClientRect();
                const midpoint = rect.top + rect.height / 2;
                const insertBefore = e.clientY < midpoint;

                // 调用 .NET 方法处理重新排序
                dotNetHelper.invokeMethod('HandleChapterReorder',
                    this.draggedChapterId,
                    targetChapterId,
                    insertBefore);
            }

            // 清理拖放指示器
            element.classList.remove('drag-over-top', 'drag-over-bottom');
        });
    },

    cleanup: function () {
        const chapterList = document.getElementById('chapter-list');
        if (chapterList) {
            const chapterItems = chapterList.querySelectorAll('.chapter-item');
            chapterItems.forEach(item => {
                item.removeAttribute('draggable');
                item.classList.remove('dragging', 'drag-over-top', 'drag-over-bottom');
            });
        }
        this.draggedElement = null;
        this.draggedChapterId = null;
    }
};

