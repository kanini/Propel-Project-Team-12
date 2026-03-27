/**
 * Simple toast notification utility
 * Lightweight replacement for react-toastify
 */

export interface ToastOptions {
  autoClose?: number;
}

class ToastManager {
  private container: HTMLDivElement | null = null;

  private getContainer(): HTMLDivElement {
    if (!this.container) {
      this.container = document.createElement('div');
      this.container.id = 'toast-container';
      this.container.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 9999;
        display: flex;
        flex-direction: column;
        gap: 10px;
        pointer-events: none;
      `;
      document.body.appendChild(this.container);
    }
    return this.container;
  }

  private show(message: string, type: 'success' | 'error' | 'warning' | 'info', options?: ToastOptions): void {
    const container = this.getContainer();
    const toast = document.createElement('div');
    
    const colors = {
      success: { bg: '#10b981', text: '#ffffff' },
      error: { bg: '#ef4444', text: '#ffffff' },
      warning: { bg: '#f59e0b', text: '#ffffff' },
      info: { bg: '#3b82f6', text: '#ffffff' },
    };

    const color = colors[type];
    
    toast.style.cssText = `
      background-color: ${color.bg};
      color: ${color.text};
      padding: 12px 24px;
      border-radius: 8px;
      box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
      font-size: 14px;
      min-width: 200px;
      max-width: 400px;
      pointer-events: auto;
      animation: slideIn 0.3s ease-out;
    `;
    
    toast.textContent = message;
    container.appendChild(toast);

    // Auto-remove after specified duration (default 3000ms)
    const autoClose = options?.autoClose !== undefined ? options.autoClose : 3000;
    setTimeout(() => {
      toast.style.animation = 'slideOut 0.3s ease-out';
      setTimeout(() => {
        container.removeChild(toast);
      }, 300);
    }, autoClose);
  }

  success(message: string, options?: ToastOptions): void {
    this.show(message, 'success', options);
  }

  error(message: string, options?: ToastOptions): void {
    this.show(message, 'error', options);
  }

  warning(message: string, options?: ToastOptions): void {
    this.show(message, 'warning', options);
  }

  info(message: string, options?: ToastOptions): void {
    this.show(message, 'info', options);
  }
}

// Add animation keyframes
const style = document.createElement('style');
style.textContent = `
  @keyframes slideIn {
    from {
      transform: translateX(100%);
      opacity: 0;
    }
    to {
      transform: translateX(0);
      opacity: 1;
    }
  }
  
  @keyframes slideOut {
    from {
      transform: translateX(0);
      opacity: 1;
    }
    to {
      transform: translateX(100%);
      opacity: 0;
    }
  }
`;
document.head.appendChild(style);

export const toast = new ToastManager();
