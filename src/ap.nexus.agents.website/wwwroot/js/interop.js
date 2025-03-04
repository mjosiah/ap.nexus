// JavaScript interop functions for Blazor WebAssembly

// Initialize Bootstrap tooltips and popovers when the document loads
document.addEventListener('DOMContentLoaded', function () {
    initializeBootstrapComponents();
});

window.addEventListener('resize', function () {
    const activeTextarea = document.querySelector('.input-field');
    if (activeTextarea) {
        window.autoResizeTextarea(activeTextarea);
    }
});

// Register a handler to close dropdowns when clicking outside
window.registerDropdownOutsideClickHandler = function (componentSelector) {
    document.addEventListener('click', function (event) {
        // Find the component instance
        const dropdownComponent = document.querySelector('.' + componentSelector);

        // If we clicked outside the dropdown
        if (dropdownComponent && !dropdownComponent.contains(event.target)) {
            // Find the Blazor component instance
            const componentInstance = dropdownComponent.closest('[__internal_dotnetref_id]');
            if (componentInstance) {
                // Get the .NET reference and call the close method
                const dotNetRef = Blazor.getInstance(componentInstance);
                if (dotNetRef) {
                    dotNetRef.invokeMethodAsync('CloseDropdown');
                }
            }
        }
    });
}

// Function to initialize Bootstrap components
function initializeBootstrapComponents() {
    // Initialize all tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize all popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
}

// Function to scroll an element to the bottom (used for message container)
window.scrollToBottom = function (element) {
    let container;

    // Check if element is a string (CSS selector)
    if (typeof element === 'string') {
        container = document.querySelector(element);
    }
    // If it's a direct reference to an element
    else if (element && element.tagName) {
        container = element;
    }

    if (container) {
        container.scrollTop = container.scrollHeight;
    }
};

// Function to auto-resize a textarea based on content
window.autoResizeTextarea = function (textarea) {
    if (!textarea) return;

    // Reset height to calculate correct scrollHeight
    textarea.style.height = 'auto';

    // Set the height to scrollHeight with min height constraint
    const newHeight = Math.max(60, Math.min(textarea.scrollHeight, 200));
    textarea.style.height = newHeight + 'px';

    // Make sure the parent container adjusts as well
    const wrapper = textarea.closest('.input-wrapper');
    if (wrapper) {
        wrapper.style.height = newHeight + 'px';
    }

    return newHeight;
};

// Function to focus an input element and select its content
window.focusAndSelectElement = function (element) {
    if (element) {
        element.focus();
        element.select();
    }
};

// Function to show a confirmation dialog
window.showConfirmDialog = function (message) {
    return confirm(message);
};

// Function to copy text to clipboard
window.copyToClipboard = function (text) {
    navigator.clipboard.writeText(text)
        .then(() => {
            console.log('Text copied to clipboard');
            return true;
        })
        .catch(err => {
            console.error('Failed to copy text: ', err);
            return false;
        });
};

// Function to handle file upload dialog
window.uploadFile = function (accept) {
    return new Promise((resolve, reject) => {
        const input = document.createElement('input');
        input.type = 'file';
        input.multiple = false;
        if (accept) {
            input.accept = accept;
        }

        input.onchange = _ => {
            const file = input.files[0];
            if (!file) {
                resolve(null);
                return;
            }

            const reader = new FileReader();
            reader.onload = function (e) {
                const result = {
                    fileName: file.name,
                    fileType: file.type,
                    fileSize: file.size,
                    fileContent: e.target.result
                };
                resolve(result);
            };

            reader.onerror = function (e) {
                reject(e);
            };

            reader.readAsDataURL(file);
        };

        input.click();
    });
};

// Function to check if device is mobile
window.isMobileDevice = function () {
    return (window.innerWidth <= 768);
};

// Function to format code blocks in messages
window.formatCodeBlocks = function () {
    document.querySelectorAll('pre code').forEach((block) => {
        // Check if highlight.js is available
        if (window.hljs) {
            hljs.highlightBlock(block);
        }
    });
};

// Function to apply markdown formatting (if you decide to use it)
window.applyMarkdown = function (elementId) {
    if (window.marked && elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            const content = element.textContent || element.innerText;
            element.innerHTML = marked(content);

            // Format code blocks after markdown is applied
            formatCodeBlocks();
        }
    }
};

// Listen for page visibility changes to handle background/foreground state
document.addEventListener('visibilitychange', function () {
    if (document.visibilityState === 'visible') {
        // Page is visible again (user came back)
        const dotnetReference = window.chatAppInstance;
        if (dotnetReference) {
            dotnetReference.invokeMethodAsync('OnPageVisible');
        }
    }
});