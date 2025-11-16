// Vendor libraries - third-party dependencies
import "bootstrap";
import $ from "jquery";
import "jquery-validation";
import "jquery-validation-unobtrusive";

// Import Bootstrap CSS
import "bootstrap/dist/css/bootstrap.css";

// Import Bootstrap Icons
import "bootstrap-icons/font/bootstrap-icons.css";

// Make jQuery globally available for ASP.NET validation
window.jQuery = $;
window.$ = $;

// Export for use in other modules
export { $ };
