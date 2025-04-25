// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Add this to your site.js or a common JavaScript file
// This function should be called before any AJAX request to protected endpoints
function setupAjaxHeaders() {
    const token = localStorage.getItem("jwtToken");
    if (token) {
        $.ajaxSetup({
            beforeSend: function (xhr) {
                xhr.setRequestHeader("Authorization", "Bearer " + token);
            }
        });
    }
}

// Call this when the page loads
$(document).ready(function () {
    setupAjaxHeaders();
});
