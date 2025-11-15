// Main application JavaScript
import "../scss/site.scss";

/**
 * Achievement tracking system
 */
class AchievementTracker {
  constructor() {
    this.counterElement = document.getElementById("achievement-counter");
    this.achievements = document.querySelectorAll(".achievement-badge");
    this.unlockedAchievements = document.querySelectorAll(
      ".achievement-unlocked"
    );
  }

  /**
   * Update achievement counter in navbar
   */
  updateCounter() {
    if (this.counterElement) {
      const unlockedCount = this.unlockedAchievements.length;
      const totalCount = this.achievements.length;
      this.counterElement.textContent = `${unlockedCount}/${totalCount}`;
    }
  }

  /**
   * Animate unlocked achievements
   */
  animateUnlocked() {
    this.unlockedAchievements.forEach((achievement, index) => {
      setTimeout(() => {
        achievement.style.animation = "unlock-glow 0.5s ease-in-out";
      }, index * 200);
    });
  }

  /**
   * Initialize achievement system
   */
  init() {
    this.updateCounter();
    this.animateUnlocked();
  }
}

/**
 * Data table enhancement
 */
class DataTableEnhancer {
  constructor(selector = ".data-table") {
    this.tables = document.querySelectorAll(selector);
  }

  /**
   * Add responsive wrapper to tables
   */
  makeResponsive() {
    this.tables.forEach((table) => {
      if (!table.parentElement.classList.contains("table-responsive")) {
        const wrapper = document.createElement("div");
        wrapper.className = "table-responsive";
        table.parentNode.insertBefore(wrapper, table);
        wrapper.appendChild(table);
      }
    });
  }

  /**
   * Add striped styling if not present
   */
  addStyling() {
    this.tables.forEach((table) => {
      if (!table.classList.contains("table")) {
        table.classList.add("table", "table-striped", "table-hover");
      }
    });
  }

  /**
   * Initialize data table enhancements
   */
  init() {
    this.makeResponsive();
    this.addStyling();
  }
}

/**
 * Form validation enhancement
 */
class FormValidator {
  constructor() {
    this.forms = document.querySelectorAll('form[data-validate="true"]');
  }

  /**
   * Initialize client-side validation
   */
  init() {
    if (typeof $ !== "undefined" && $.validator) {
      this.forms.forEach((form) => {
        $(form).validate({
          errorClass: "is-invalid",
          validClass: "is-valid",
          errorElement: "div",
          errorPlacement: function (error, element) {
            error.addClass("invalid-feedback");
            element.closest(".form-group, .mb-3").append(error);
          },
          highlight: function (element) {
            $(element).addClass("is-invalid").removeClass("is-valid");
          },
          unhighlight: function (element) {
            $(element).addClass("is-valid").removeClass("is-invalid");
          },
        });
      });
    }
  }
}

/**
 * Smooth scroll enhancement
 */
class SmoothScroll {
  constructor() {
    this.anchors = document.querySelectorAll('a[href^="#"]');
  }

  /**
   * Initialize smooth scrolling for anchor links
   */
  init() {
    this.anchors.forEach((anchor) => {
      anchor.addEventListener("click", (e) => {
        const href = anchor.getAttribute("href");
        if (href === "#") return;

        const target = document.querySelector(href);
        if (target) {
          e.preventDefault();
          target.scrollIntoView({
            behavior: "smooth",
            block: "start",
          });
        }
      });
    });
  }
}

/**
 * Toast notification system
 */
class ToastNotifier {
  constructor() {
    this.toasts = document.querySelectorAll(".toast");
  }

  /**
   * Show all toast notifications
   */
  showAll() {
    if (typeof bootstrap !== "undefined") {
      this.toasts.forEach((toastEl) => {
        const toast = new bootstrap.Toast(toastEl);
        toast.show();
      });
    }
  }

  /**
   * Create and show a toast message
   * @param {string} message - The message to display
   * @param {string} type - Toast type (success, danger, warning, info)
   */
  static show(message, type = "info") {
    const toastContainer =
      document.getElementById("toast-container") ||
      ToastNotifier.createContainer();

    const toastEl = document.createElement("div");
    toastEl.className = `toast align-items-center text-white bg-${type} border-0`;
    toastEl.setAttribute("role", "alert");
    toastEl.setAttribute("aria-live", "assertive");
    toastEl.setAttribute("aria-atomic", "true");

    toastEl.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" 
                        data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        `;

    toastContainer.appendChild(toastEl);

    if (typeof bootstrap !== "undefined") {
      const toast = new bootstrap.Toast(toastEl);
      toast.show();

      toastEl.addEventListener("hidden.bs.toast", () => {
        toastEl.remove();
      });
    }
  }

  /**
   * Create toast container if it doesn't exist
   */
  static createContainer() {
    const container = document.createElement("div");
    container.id = "toast-container";
    container.className = "toast-container position-fixed top-0 end-0 p-3";
    container.style.zIndex = "9999";
    document.body.appendChild(container);
    return container;
  }
}

/**
 * Loading spinner utility
 */
class LoadingSpinner {
  static show(target = "body") {
    const spinner = document.createElement("div");
    spinner.id = "loading-spinner";
    spinner.className = "position-fixed top-50 start-50 translate-middle";
    spinner.style.zIndex = "9999";
    spinner.innerHTML = `
            <div class="spinner-border text-primary" role="status" style="width: 3rem; height: 3rem;">
                <span class="visually-hidden">Loading...</span>
            </div>
        `;

    const overlay = document.createElement("div");
    overlay.id = "loading-overlay";
    overlay.className = "position-fixed top-0 start-0 w-100 h-100";
    overlay.style.backgroundColor = "rgba(0,0,0,0.5)";
    overlay.style.zIndex = "9998";

    document.body.appendChild(overlay);
    document.body.appendChild(spinner);
  }

  static hide() {
    const spinner = document.getElementById("loading-spinner");
    const overlay = document.getElementById("loading-overlay");

    if (spinner) spinner.remove();
    if (overlay) overlay.remove();
  }
}

/**
 * Initialize all modules when DOM is ready
 */
document.addEventListener("DOMContentLoaded", () => {
  // Initialize achievement tracking
  const achievementTracker = new AchievementTracker();
  achievementTracker.init();

  // Enhance data tables
  const dataTableEnhancer = new DataTableEnhancer();
  dataTableEnhancer.init();

  // Initialize form validation
  const formValidator = new FormValidator();
  formValidator.init();

  // Initialize smooth scrolling
  const smoothScroll = new SmoothScroll();
  smoothScroll.init();

  // Show any toast notifications
  const toastNotifier = new ToastNotifier();
  toastNotifier.showAll();

  // Log initialization
  console.log("🕵️‍♀️ Slurper Demo Web initialized successfully");
});

// Export utilities for global use
window.SlurperUtils = {
  ToastNotifier,
  LoadingSpinner,
};

// Export for module use
export {
  AchievementTracker,
  DataTableEnhancer,
  FormValidator,
  SmoothScroll,
  ToastNotifier,
  LoadingSpinner,
};
